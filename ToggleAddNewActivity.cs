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
using LCU.State.API.IDESettings.Harness;
using LCU.State.API.IDESettings.Models;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class ToggleAddNewActivityRequest
    { }

    public static class ToggleAddNewActivity
    {
        [FunctionName("ToggleAddNewActivity")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<ToggleAddNewActivityRequest, IdeSettingsState, IDESettingsStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Toggling Add New Activity.");

                return await mgr.ToggleAddNew(AddNewTypes.Activity);
            });
        }
    }
}
