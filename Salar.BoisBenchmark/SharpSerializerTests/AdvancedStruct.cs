using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace HelloWorldApp.BusinessObjects
{
	[Serializable, DataContract]
	[ProtoContract]
	public struct AdvancedStruct
	{
		[DataMember, ProtoMember(1)]
		public int SimpleInt { get; set; }
		[DataMember, ProtoMember(2)]
		public string SimpleText { get; set; }
		[DataMember, ProtoMember(3)]
		public DateTime DateTime { get; set; }
	}
}