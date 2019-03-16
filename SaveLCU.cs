using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LCU.State.API.IdeSettings.Models;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises.IDE;
using System.Collections.Generic;
using LCU.Graphs.Registry.Enterprises;
using Fathym;
using System.Linq;
using System.Net.Http;
using Gremlin.Net.Process.Traversal;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class SaveLCURequest
    {
        [DataMember]
        public virtual LowCodeUnitConfig LCU { get; set; }
    }

    public static class SaveLCU
    {
        const string lcuPathRoot = "_lcu";

        [FunctionName("SaveLCU")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<SaveLCURequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
                if (!reqData.LCU.Lookup.IsNullOrEmpty() && !reqData.LCU.NPMPackage.IsNullOrEmpty() && !reqData.LCU.PackageVersion.IsNullOrEmpty())
                {
                    var regGraphConfig = new LCUGraphConfig()
                    {
                        APIKey = Environment.GetEnvironmentVariable("LCU_GRAPH_API_KEY"),
                        Host = Environment.GetEnvironmentVariable("LCU_GRAPH_HOST"),
                        Database = Environment.GetEnvironmentVariable("LCU_GRAPH_DATABASE"),
                        Graph = Environment.GetEnvironmentVariable("LCU_GRAPH")
                    };

                    var ideGraph = new IDEGraph(regGraphConfig);

                    var settings = new IDEContainerSettings()
                    {
                        Container = "Default",
                        EnterprisePrimaryAPIKey = details.EnterpriseAPIKey
                    };

                    var ideSettings = await ideGraph.EnsureIDESettings(settings);

                    var lcu = await ideGraph.SaveLCU(reqData.LCU, details.EnterpriseAPIKey, settings.Container);

                    if (lcu != null)
                    {
                        var status = await ensureApplication(regGraphConfig, details, reqData.LCU);

                        lcu = await ideGraph.SaveLCU(reqData.LCU, details.EnterpriseAPIKey, settings.Container);
                    }

                    state.Arch.LCUs = await ideGraph.ListLCUs(details.EnterpriseAPIKey, settings.Container);

                    state.Arch.EditLCU = null;

                    state.AddNew.LCU = false;
                }

                return state;
            });
        }

        private static async Task<Status> ensureApplication(LCUGraphConfig regGraphConfig, LCUStateDetails details, LowCodeUnitConfig lcu)
        {
            var appGraph = new ApplicationGraph(regGraphConfig);

            var apps = await appGraph.ListApplications(details.EnterpriseAPIKey);

            var lcuApp = apps?.FirstOrDefault(a => a.PathRegex == $"/_lcu/{lcu.Lookup}*");

            if (lcuApp == null)
            {
                lcuApp = await appGraph.Save(new Application()
                {
                    Name = lcu.Lookup,
                    PathRegex = $"/_lcu/{lcu.Lookup}*",
                    Priority = apps == null ? 500 : apps.Select(a => a.Priority).Max() + 500,
                    Hosts = new List<string>() { details.Host },
                    EnterprisePrimaryAPIKey = details.EnterpriseAPIKey
                });

            }

            if (lcuApp != null)
            {
                var dafApps = await appGraph.GetDAFApplications(details.EnterpriseAPIKey, lcuApp.ID);

                var dafApp = dafApps?.FirstOrDefault(a => a.Metadata["BaseHref"].ToString() == $"/_lcu/{lcu.Lookup}/");

                if (dafApp == null)
                    dafApp = new DAFViewConfiguration()
                    {
                        ApplicationID = lcuApp.ID,
                        BaseHref = $"/_lcu/{lcu.Lookup}/",
                        NPMPackage = lcu.NPMPackage,
                        PackageVersion = lcu.PackageVersion,
                        Priority = 10000
                    }.JSONConvert<DAFApplicationConfiguration>();
                else
                {
                    dafApp.Metadata["NPMPackage"] = lcu.NPMPackage;

                    dafApp.Metadata["PackageVersion"] = lcu.PackageVersion;
                }

                var status = await unpackView(regGraphConfig, dafApp, details.EnterpriseAPIKey);

                if (status)
                {
                    dafApp = appGraph.SaveDAFApplication(details.EnterpriseAPIKey, dafApp).Result;

                    if (dafApp != null)
                        lcu.PackageVersion = dafApp.Metadata["PackageVersion"].ToString();
                }
                else
                    return status;
            }

            return Status.Success;
        }

        private static async Task<Status> unpackView(LCUGraphConfig entGraphConfig, DAFApplicationConfiguration dafApp, string entApiKey)
        {
            var viewApp = dafApp.JSONConvert<DAFViewConfiguration>();

            if (viewApp.PackageVersion != "dev-stream")
            {
                var entGraph = new EnterpriseGraph(entGraphConfig);

                var ent = await entGraph.LoadByPrimaryAPIKey(entApiKey);

                var client = new HttpClient();

                var npmUnpackUrl = Environment.GetEnvironmentVariable("NPM_PUBLIC_URL");

                var npmUnpackCode = Environment.GetEnvironmentVariable("NPM_PUBLIC_CODE");

                var npmUnpack = $"{npmUnpackUrl}/api/npm-unpack?code={npmUnpackCode}&pkg={viewApp.NPMPackage}&version={viewApp.PackageVersion}&applicationId={dafApp.ApplicationID}&enterpriseId={ent.ID}";

                var response = await client.GetAsync(npmUnpack);

                object statusObj = await response.Content.ReadAsJSONAsync<dynamic>();

                var status = statusObj.JSONConvert<Status>();

                if (status)
                    dafApp.Metadata["PackageVersion"] = status.Metadata["Version"];

                return status;
            }
            else
                return Status.Success.Clone("Success", new { PackageVersion = viewApp.PackageVersion });
        }
    }
}
