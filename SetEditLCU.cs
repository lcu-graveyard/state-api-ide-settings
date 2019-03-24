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
using System.Linq;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.Manager;

namespace LCU.State.API.IDESettings
{
	[Serializable]
	[DataContract]
	public class SetEditLCURequest
	{
		[DataMember]
		public virtual string LCU { get; set; }
	}

    public static class SetEditLCU
    {
        [FunctionName("SetEditLCU")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<SetEditLCURequest, IdeSettingsState, IDESettingsStateHarness>(log, async (mgr, reqData) =>
            {
                return await mgr.SetEditLCU(reqData.LCU);
            });
        }
    }
}
