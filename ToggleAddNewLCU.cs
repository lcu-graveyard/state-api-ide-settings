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
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.State.API.IDESettings.Harness;
using LCU.State.API.IDESettings.Models;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class ToggleAddNewLCURequest
    { }

    public static class ToggleAddNewLCU
    {
        [FunctionName("ToggleAddNewLCU")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<ToggleAddNewLCURequest, IdeSettingsState, IDESettingsStateHarness>(log, async (mgr, reqData) =>
            {
                return await mgr.ToggleAddNew(AddNewTypes.LCU);
            });
        }
    }
}
