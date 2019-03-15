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
using LCU.State.API.IdeSettings.Models;
using System.Linq;

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
            return await req.WithState<SetEditLCURequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
            {
				state.Arch.EditLCU = state.Arch.LCUs?.FirstOrDefault(a => a.Lookup == reqData.LCU)?.Lookup;

                state.AddNew.LCU = false;

                return state;
            });
        }
    }
}
