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
using LCU.Manager;

namespace LCU.State.API.IDESettings
{
    [Serializable]
    [DataContract]
    public class SaveLCUCapabilitiesRequest
    {
        [DataMember]
        public virtual List<string> Files { get; set; }

        [DataMember]
        public virtual List<IdeSettingsConfigSolution> Solutions { get; set; }

        [DataMember]
        public virtual string LCU { get; set; }
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
                return await mgr.SaveLCUCapabilities(reqData.LCU, reqData.Files, reqData.Solutions);
            });
        }
    }
}
