using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LCU.State.API.IdeSettings.Models;

namespace LCU.State.API.IDESettings
{
    public static class Refresh
    {
        [FunctionName("Refresh")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
			return await req.WithState<dynamic, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
			{
				//	TODO:  Load the connected list of LCUs

				return state;
			});
        }
    }
}
