using LCU.Graphs.Registry.Enterprises.IDE;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.State.API.IdeSettings.Models
{
	[Serializable]
	[DataContract]
	public class IdeSettingsState
	{
		[DataMember]
		public virtual List<IDEActivity> Activities { get; set; }

		[DataMember]
		public virtual string EditActivity{ get; set; }

		[DataMember]
		public virtual bool Loading { get; set; }
	}
}
