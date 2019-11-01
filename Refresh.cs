using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises.IDE;
using System.Collections.Generic;
using System.Linq;
using LCU.State.API.IDESettings.Harness;
using LCU.State.API.IDESettings.Models;

namespace LCU.State.API.IDESettings
{
    public static class Refresh
    {
        [FunctionName("Refresh")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<dynamic, IdeSettingsState, IDESettingsStateHarness>(log, async (mgr, reqData) =>
            {
                await mgr.Ensure();

                log.LogInformation($"Refreshing.");

                return await mgr.WhenAll(
                    mgr.LoadActivities(),
                    mgr.ConfigureSideBarEditActivity(),
                    mgr.LoadLCUs(),
                    mgr.ClearConfig()
                );
            });
        }
    }
}
