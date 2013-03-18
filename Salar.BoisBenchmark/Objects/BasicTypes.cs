using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Salar.BoisBenchmark.Objects
{
	[ProtoContract]
	[Serializable]
	public class BasicTypes
	{
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

		[ProtoMember(7)]
		public Guid guid { get; set; }

		public static BasicTypes CreateObject()
		{
			var obj = new BasicTypes()
			{
				Lng = Language.Csharp,
				Date = DateTime.Now,
				Text = "DateTime ticks: " + DateTime.Now.Ticks.ToString(),
				Data = new byte[] { 66, 20, 30, 50, 90, 122, 50, 22, 0, 0, 0, 16, 19, 177 },
				StrArray = new string[] { "This is ", "Salar", "BON", "Test" },
				guid = Guid.NewGuid()
			};
			return obj;
		}

	}

	public enum Language
	{
		Cpp,
		C,
		Csharp,
		Javascript,
		Php
	}

}
