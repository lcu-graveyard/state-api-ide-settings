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
	public class SetEditActivityRequest
	{
		[DataMember]
		public virtual string Activity { get; set; }
	}

	public static class SetEditActivity
    {
        [FunctionName("SetEditActivity")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
		{
			return await req.WithState<SetEditActivityRequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
			{
				state.EditActivity = state.Activities?.FirstOrDefault(a => a.Lookup == reqData.Activity)?.Lookup;

				state.AddNew.Activity = false;

				return state;
			});
		}
    }
}
