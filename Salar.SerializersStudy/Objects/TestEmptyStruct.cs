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
	struct TestEmptyStruct
	{
	}
}
