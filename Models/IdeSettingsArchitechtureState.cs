using LCU.Graphs.Registry.Enterprises.IDE;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.State.API.IDESettings.Models
{
    [Serializable]
	[DataContract]
	public class IdeSettingsArchitechtureState
	{
		[DataMember]
		public virtual string EditLCU { get; set; }

		[DataMember]
		public virtual List<LowCodeUnitSetupConfig> LCUs { get; set; }
	}
}
