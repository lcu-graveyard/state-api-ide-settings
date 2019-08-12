using LCU.Graphs.Registry.Enterprises.IDE;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.State.API.IDESettings.Models
{
    [Serializable]
	[DataContract]
	public class IdeSettingsConfigState
	{
		[DataMember]
		public virtual List<string> ActiveFiles { get; set; }

		[DataMember]
		public virtual List<IdeSettingsConfigSolution> ActiveSolutions { get; set; }
		
		[DataMember]
		public virtual string CurrentLCUConfig { get; set; }

		[DataMember]
		public virtual LowCodeUnitConfiguration LCUConfig { get; set; }
	}
}
