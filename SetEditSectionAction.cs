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
using System.Linq;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises.IDE;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class SetEditSectionActionRequest
    {
        [DataMember]
        public virtual string Action { get; set; }
    }

    public static class SetEditSectionAction
    {
        [FunctionName("SetEditSectionAction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<SetEditSectionActionRequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
                var regGraphConfig = new LCUGraphConfig()
                {
                    APIKey = Environment.GetEnvironmentVariable("LCU_GRAPH_API_KEY"),
                    Host = Environment.GetEnvironmentVariable("LCU_GRAPH_HOST"),
                    Database = Environment.GetEnvironmentVariable("LCU_GRAPH_DATABASE"),
                    Graph = Environment.GetEnvironmentVariable("LCU_GRAPH")
                };

                var ideGraph = new IDEGraph(regGraphConfig);

                state.EditSectionAction = state.SectionActions?.FirstOrDefault(sa => sa.Action == reqData.Action)?.Action;

                state.SectionActions = await ideGraph.ListSectionActions(state.SideBarEditActivity, state.EditSection, details.EnterpriseAPIKey, "Default");

                return state;
            });
        }
    }
}
