using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LCU.State.API.IdeSettings.Models;
using System.Runtime.Serialization;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises.IDE;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class SetConfigLCURequest
    {
        [DataMember]
        public virtual string LCU { get; set; }
    }

    public static class SetConfigLCU
    {
        [FunctionName("SetConfigLCU")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<SetConfigLCURequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
                var ideGraph = req.LoadGraph<IDEGraph>();

                state.Config.ConfigLCU = reqData.LCU;

                var clearConfig = false;

                log.LogInformation("Starting to set config LCU");

                if (!state.Config.ConfigLCU.IsNullOrEmpty())
                {
                    state.Config.Files = state.Config.LCUFiles = await ideGraph.ListLCUFiles(state.Config.ConfigLCU, details.Host, req.Scheme);

                    state.Config.LCUSolutions = await ideGraph.ListLCUSolutions(state.Config.ConfigLCU, details.EnterpriseAPIKey, "Default");

                    var client = new HttpClient();
                    //  TODO:  This should hard code at https once that is enforced on the platform
                    client.BaseAddress = new Uri($"http://{details.Host}");

                    var lcuConfigResp = await client.GetAsync($"/_lcu/{state.Config.ConfigLCU}/lcu.json");

                    var lcuConfigStr = await lcuConfigResp.Content.ReadAsStringAsync();

                    if (lcuConfigResp.IsSuccessStatusCode && !lcuConfigStr.IsNullOrEmpty() && !lcuConfigStr.StartsWith("<"))
                    {
                        var lcuConfig = lcuConfigStr.FromJSON<dynamic>();

                        var slnsDict = ((JToken)lcuConfig.config.solutions).ToObject<Dictionary<string, dynamic>>();

                        state.Config.Solutions = slnsDict.Select(sd => new IdeSettingsConfigSolution()
                        {
                            Element = sd.Value.element,
                            Name = sd.Key
                        }).ToList();
                    }
                    else
                        clearConfig = true;
                }
                else
                    clearConfig = true;

                if (clearConfig)
                {
                    state.Config.Files = new List<string>();

                    state.Config.LCUFiles = new List<string>();

                    state.Config.LCUSolutions = new List<IdeSettingsConfigSolution>();

                    state.Config.Solutions = new List<IdeSettingsConfigSolution>();
                }

                return state;
            });
        }
    }
}
