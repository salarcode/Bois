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
	public class TestParentClassNull
	{
		[ProtoMember(1)]
		[DataMember]
		public TestOneMemberClass TestOneMemberClass { get; set; }
		
		[ProtoMember(2)]
		[DataMember]
		public TestOneMemberStruct TestOneMemberStruct { get; set; }
		
		[ProtoMember(3)]
		[DataMember]
		public TestTwoMemberClass TestTwoMemberClass { get; set; }
		
		[ProtoMember(4)]
		[DataMember]
		public TestTwoMemberStruct TestTwoMemberStruct { get; set; }
	}
}
