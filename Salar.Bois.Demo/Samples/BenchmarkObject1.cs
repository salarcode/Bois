using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompactBinarySerializer.Demo.Samples;
using ProtoBuf;

namespace Salar.Bion.Demo.Samples
{
	[ProtoContract]
	[Serializable]
	public class BenchmarkObject1
	{
		public BenchmarkObject1()
		{

		}

		 
		[ProtoMember(1)]
		public string Text { get; set; }

		[ProtoMember(2)]
		public DateTime Date { get; set; }

		[ProtoMember(3)]
		public byte[] Data { get; set; }

		[ProtoMember(4)]
		public Language Lng { get; set; }

		[ProtoMember(5)]
		public string[] StrArray { get; set; }

		public static BenchmarkObject1 CreateObject()
		{
			var obj = new BenchmarkObject1()
			{
				Lng = Language.Csharp,
				Date = DateTime.Now,
				Text = "DateTime ticks: " + DateTime.Now.Ticks.ToString(),
				Data = new byte[] { 66, 20, 30, 50, 90, 122, 50, 22, 0, 0, 0, 16, 19, 177 },
				StrArray = new string[] { "This is ", "Salar", "BON", "Test" },
			};
			return obj;
		}

	}
}
