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
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.Manager;

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
            return await req.Manage<SaveSectionActionRequest, IdeSettingsState, IDESettingsStateHarness>(log, async (mgr, reqData) =>
            {
                return await mgr.SaveSectionAction(reqData.Action);
            });
        }
    }
}
