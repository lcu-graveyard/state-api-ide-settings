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
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.State.API.IdeSettings.Models;
using System.Linq;
using LCU.Graphs;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class SaveActivityRequest
    {
        [DataMember]
        public virtual IDEActivity Activity { get; set; }
    }

    public static class SaveActivity
    {
        [FunctionName("SaveActivity")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<SaveActivityRequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
                if (!reqData.Activity.Title.IsNullOrEmpty() && !reqData.Activity.Lookup.IsNullOrEmpty() && !reqData.Activity.Icon.IsNullOrEmpty())
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

                    var activity = await ideGraph.SaveActivity(reqData.Activity, details.EnterpriseAPIKey, settings.Container);

                    state.Activities = await ideGraph.ListActivities(details.EnterpriseAPIKey, settings.Container);

                    state.EditActivity = null;

                    if (state.AddNew == null)
                        state.AddNew = new IdeSettingsAddNew();

                    state.AddNew.Activity = false;
                }

                return state;
            });
        }
    }
}
