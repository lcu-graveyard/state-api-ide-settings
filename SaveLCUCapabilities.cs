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
using System.Collections.Generic;
using LCU.State.API.IdeSettings.Models;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises.IDE;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class SaveLCUCapabilitiesRequest
    {
        [DataMember]
        public virtual List<string> Files { get; set; }
        
        [DataMember]
        public virtual List<IdeSettingsConfigSolution> Solutions { get; set; }
        
        [DataMember]
        public virtual string LCU { get; set; }
    }

    public static class SaveLCUCapabilities
    {
        [FunctionName("SaveLCUCapabilities")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<SaveLCUCapabilitiesRequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
                if (!reqData.LCU.IsNullOrEmpty())
                {
                    var regGraphConfig = new LCUGraphConfig()
                    {
                        APIKey = Environment.GetEnvironmentVariable("LCU_GRAPH_API_KEY"),
                        Host = Environment.GetEnvironmentVariable("LCU_GRAPH_HOST"),
                        Database = Environment.GetEnvironmentVariable("LCU_GRAPH_DATABASE"),
                        Graph = Environment.GetEnvironmentVariable("LCU_GRAPH")
                    };

                    var ideGraph = new IDEGraph(regGraphConfig);

                    var status = await ideGraph.SaveLCUCapabilities(reqData.LCU, reqData.Files, reqData.Solutions, details.EnterpriseAPIKey, "Default");

                    state.Config.LCUFiles = await ideGraph.ListLCUFiles(reqData.LCU, details.Host, req.Scheme);
                    
                    state.Config.LCUSolutions = await ideGraph.ListLCUSolutions(reqData.LCU, details.EnterpriseAPIKey, "Default");
                }

                return state;
            });
        }
    }
}
