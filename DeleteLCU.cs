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
using System.Linq;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class DeleteLCURequest
    {
        [DataMember]
        public virtual string LCU { get; set; }
    }

    public static class DeleteLCU
    {
        [FunctionName("DeleteLCU")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<DeleteLCURequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
                var regGraphConfig = new LCUGraphConfig()
                {
                    APIKey = Environment.GetEnvironmentVariable("LCU_GRAPH_API_KEY"),
                    Host = Environment.GetEnvironmentVariable("LCU_GRAPH_HOST"),
                    Database = Environment.GetEnvironmentVariable("LCU_GRAPH_DATABASE"),
                    Graph = Environment.GetEnvironmentVariable("LCU_GRAPH")
                };

                var ideGraph = new IDEGraph(regGraphConfig);

                await ideGraph.DeleteLCU(reqData.LCU, details.EnterpriseAPIKey, "Default");

                state.Arch.LCUs = await ideGraph.ListLCUs(details.EnterpriseAPIKey, "Default");

                //  TODO:  Need to delete other assets related to the LCU...  created apps, delete from filesystem, cleanup state??  Or what do we want to do with that stuff?

                return state;
            });
        }
    }
}
