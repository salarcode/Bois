using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ProtoBuf;

namespace Salar.SerializersStudy.Objects
{
	[DataContract]
	[Serializable]
	[ProtoContract]
	public struct TestTwoMemberStruct
	{
		[ProtoMember(1)]
		[DataMember]
		public bool Byte1 { get; set; }
		[ProtoMember(2)]
		[DataMember]
		public bool Byte2 { get; set; }
	}
}
