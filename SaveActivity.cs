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
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.State.API.IdeSettings.Models;
using System.Linq;

namespace LCU.State.API.IDESettings
{
	[Serializable]
	[DataContract]
	public class SaveActivityRequest
	{
		[DataMember]
		public virtual IDEActivity Activity { get; set; }
	}

	public static class SaveActivity
	{
		[FunctionName("SaveActivity")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
			ILogger log)
		{
			return await req.WithState<SaveActivityRequest, IdeSettingsState>(log, async (details, reqData, state, stateMgr) =>
			{
				var existing = state.Activities.FirstOrDefault(a => a.Lookup == reqData.Activity.Lookup);

				if (existing != null)
				{
					existing.Icon = reqData.Activity.Icon;

					existing.IconSet = reqData.Activity.IconSet;

					existing.Lookup = reqData.Activity.Lookup;

					existing.Title = reqData.Activity.Title;
				}
				else
					state.Activities.Add(reqData.Activity);

				state.EditActivity = null;

				return state;
			});
		}
	}
}
