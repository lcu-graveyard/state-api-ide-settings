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
    public class SaveSectionActionRequest
    {
        [DataMember]
        public virtual IdeSettingsSectionAction Action { get; set; }
    }

    public static class SaveSectionAction
    {
        [FunctionName("SaveSectionAction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<SaveSectionActionRequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
                if (!reqData.Action.Action.IsNullOrEmpty() && !reqData.Action.Name.IsNullOrEmpty())
                {
                    var regGraphConfig = new LCUGraphConfig()
                    {
                        APIKey = Environment.GetEnvironmentVariable("LCU_GRAPH_API_KEY"),
                        Host = Environment.GetEnvironmentVariable("LCU_GRAPH_HOST"),
                        Database = Environment.GetEnvironmentVariable("LCU_GRAPH_DATABASE"),
                        Graph = Environment.GetEnvironmentVariable("LCU_GRAPH")
                    };

                    var ideGraph = new IDEGraph(regGraphConfig);

                    var secAct = await ideGraph.SaveSectionAction(state.SideBarEditActivity, state.EditSection, reqData.Action, details.EnterpriseAPIKey, "Default");

                    state.SectionActions = await ideGraph.ListSectionActions(state.SideBarEditActivity, state.EditSection, details.EnterpriseAPIKey, "Default");

                    state.EditSectionAction = null;

                    state.AddNew.SectionAction = false;
                }

                return state;
            });
        }
    }
}
