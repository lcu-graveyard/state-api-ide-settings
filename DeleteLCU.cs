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
using System.Linq;
using LCU.State.API.IDESettings.Harness;
using LCU.State.API.IDESettings.Models;

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
            return await req.Manage<DeleteLCURequest, IdeSettingsState, IDESettingsStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Deleting LCU: {reqData.LCU}");

                return await mgr.DeleteLCU(reqData.LCU);
            });
        }
    }
}
