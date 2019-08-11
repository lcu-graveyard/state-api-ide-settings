using System;
using System.Runtime.Serialization;

namespace LCU.State.API.IDESettings.Models
{
    [Serializable]
	[DataContract]
	public class IdeSettingsAddNew
	{
		[DataMember]
		public virtual bool Activity { get; set; }

		[DataMember]
		public virtual bool LCU { get; set; }

		[DataMember]
		public virtual bool SectionAction { get; set; }
	}
}
