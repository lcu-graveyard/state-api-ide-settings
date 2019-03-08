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
	public class DeleteActivityRequest
	{
		[DataMember]
		public virtual string Activity { get; set; }
	}

	public static class DeleteActivity
    {
        [FunctionName("DeleteActivity")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
		{
			return await req.WithState<DeleteActivityRequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
			{
				state.Activities = state.Activities.Where(a => a.Lookup != reqData.Activity).ToList();

				return state;
			});
		}
    }
}
