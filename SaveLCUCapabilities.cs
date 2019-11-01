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
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises.IDE;
using System.Linq;
using LCU.State.API.IDESettings.Harness;
using LCU.State.API.IDESettings.Models;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class SaveLCUCapabilitiesRequest
    {
        [DataMember]
        public virtual LowCodeUnitConfiguration LCUConfig { get; set; }

        [DataMember]
        public virtual string LCULookup { get; set; }
    }

    public static class SaveLCUCapabilities
    {
        [FunctionName("SaveLCUCapabilities")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<SaveLCUCapabilitiesRequest, IdeSettingsState, IDESettingsStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Saving LCU Capabilities: {reqData.LCULookup} {reqData.LCUConfig}");

                return await mgr.SaveLCUCapabilities(reqData.LCULookup, reqData.LCUConfig);
            });
        }
    }
}
