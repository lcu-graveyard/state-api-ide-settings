using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Fathym.Business.Models;

namespace LCU.Graphs.Registry.Enterprises.IDE
{
	[Serializable]
	[DataContract]
	public class IDEContainerSettings : BusinessModel<Guid>
	{
		[DataMember]
		public virtual string Container { get; set; }

		[DataMember]
		public virtual string EnterprisePrimaryAPIKey { get; set; }
	}
	
    [Serializable]
    [DataContract]
    public class LowCodeUnitConfig
    {
        [DataMember]
        public virtual string Lookup { get; set; }
        
        [DataMember]
        public virtual string NPMPackage { get; set; }
        
        [DataMember]
        public virtual string PackageVersion { get; set; }
    }
}
