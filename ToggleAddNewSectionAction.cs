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
using LCU.Graphs.Registry.Enterprises.IDE;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class ToggleAddNewSectionActionRequest
    { }

    public static class ToggleAddNewSectionAction
    {
        [FunctionName("ToggleAddNewSectionAction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<ToggleAddNewSectionActionRequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
                if (state.AddNew == null)
                    state.AddNew = new IdeSettingsAddNew();

                state.AddNew.SectionAction = !state.AddNew.SectionAction;

                state.EditSectionAction = null;

                return state;
            });
        }
    }
}
