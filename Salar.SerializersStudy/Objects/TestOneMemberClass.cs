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
	public class TestOneMemberClass
	{
		[ProtoMember(1)]
		[DataMember]
		public bool OneByte { get; set; }
	}
}
