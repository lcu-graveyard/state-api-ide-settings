using LCU.Graphs.Registry.Enterprises.IDE;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.State.API.IDESettings.Models
{
    [Serializable]
	[DataContract]
	public class IdeSettingsState
	{
		[DataMember]
		public virtual List<IDEActivity> Activities { get; set; }

		[DataMember]
		public virtual IdeSettingsAddNew AddNew { get; set; }

		[DataMember]
		public virtual IdeSettingsArchitechtureState Arch { get; set; }

		[DataMember]
		public virtual IdeSettingsConfigState Config { get; set; }

		[DataMember]
		public virtual string EditActivity { get; set; }

		[DataMember]
		public virtual string EditSection { get; set; }

		[DataMember]
		public virtual string EditSectionAction { get; set; }

		[DataMember]
		public virtual Dictionary<string, List<string>> LCUSolutionOptions { get; set; }

		[DataMember]
		public virtual bool Loading { get; set; }

		[DataMember]
		public virtual List<IDESideBarAction> SectionActions { get; set; }

		[DataMember]
		public virtual string SideBarEditActivity { get; set; }

		[DataMember]
		public virtual List<string> SideBarSections { get; set; }
	}
}
