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

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class AddSideBarSectionRequest
    {
        [DataMember]
        public virtual string Activity { get; set; }

        [DataMember]
        public virtual string Section { get; set; }
    }

    public static class AddSideBarSection
    {
        [FunctionName("AddSideBarSection")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<AddSideBarSectionRequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
                var regGraphConfig = new LCUGraphConfig()
                {
                    APIKey = Environment.GetEnvironmentVariable("LCU_GRAPH_API_KEY"),
                    Host = Environment.GetEnvironmentVariable("LCU_GRAPH_HOST"),
                    Database = Environment.GetEnvironmentVariable("LCU_GRAPH_DATABASE"),
                    Graph = Environment.GetEnvironmentVariable("LCU_GRAPH")
                };

                var ideGraph = new IDEGraph(regGraphConfig);

                await ideGraph.AddSideBarSection(reqData.Activity, reqData.Section, details.EnterpriseAPIKey, "Default");

                state.SideBarSections = await ideGraph.ListSideBarSections(state.SideBarEditActivity, details.EnterpriseAPIKey, "Default");

                return state;
            });
        }
    }
}
