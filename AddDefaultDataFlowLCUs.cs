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
using LCU.State.API.IDESettings.Models;
using LCU.State.API.IDESettings.Harness;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class AddDefaultDataFlowLCUsRequest
    { }

    public static class AddDefaultDataFlowLCUs
    {
        [FunctionName("AddDefaultDataFlowLCUs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<AddDefaultDataFlowLCUsRequest, IdeSettingsState, IDESettingsStateHarness>(log, async (mgr, reqData) =>
            {
                return await mgr.AddDefaultDataFlowLCUs();
            });
        }
    }
}
