using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Salar.Bion.Demo.Samples
{
	[Serializable]
	[ProtoContract]
	public struct StructType1  
	{
		[ProtoMember(1)]
		public string Text { get; set; }
		[ProtoMember(2)]
		public string Text2 { get; set; }
		[ProtoMember(3)]
		public string Text3 { get; set; }
		[ProtoMember(4)]
		public char AcceptChar { get; set; }
		[ProtoMember(5)]
		public DateTime TestDate { get; set; }
 

		public void Initialize()
		{

		}

		public static StructType1 InitializeThis()
		{
			return new StructType1()
			{
				Text = "Well, hello!",
				Text2 = "This is Salar.Bon",
				Text3 = null,
				AcceptChar = 'c',
				TestDate = DateTime.Now.AddDays(7),
 			};
		}
	}
}
