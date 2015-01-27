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
	class TestParentClassFull
	{
		public static TestParentClassFull Create()
		{
			return new TestParentClassFull()
			{
				TestOneMemberClass1 = new TestOneMemberClass(),
				TestOneMemberStruct1 = new TestOneMemberStruct(),
				TestTwoMemberClass1 = new TestTwoMemberClass(),
				TestTwoMemberStruct1 = new TestTwoMemberStruct()
			};
		}
		[ProtoMember(1)]
		[DataMember]
		public TestOneMemberClass TestOneMemberClass1 { get; set; }

		[ProtoMember(2)]
		[DataMember]
		public TestOneMemberStruct TestOneMemberStruct1 { get; set; }

		[ProtoMember(3)]
		[DataMember]
		public TestTwoMemberClass TestTwoMemberClass1 { get; set; }

		[ProtoMember(4)]
		[DataMember]
		public TestTwoMemberStruct TestTwoMemberStruct1 { get; set; }
	}
}
