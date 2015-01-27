using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ProtoBuf;

namespace Salar.BoisBenchmark.Objects
{
	[ProtoContract]
	[Serializable]
	[DataContract]
	public class BasicTypes
	{
		[ProtoMember(1)]
		[DataMember]
		public string Text { get; set; }

		[ProtoMember(2)]
		[DataMember]
		public DateTime Date { get; set; }

		[ProtoMember(3)]
		[DataMember]
		public byte[] Data { get; set; }

		[ProtoMember(4)]
		[DataMember]
		public Language Lng { get; set; }

		[ProtoMember(5)]
		[DataMember]
		public string[] StrArray { get; set; }

		[ProtoMember(7)]
		[DataMember]
		public Guid guid { get; set; }

		[ProtoMember(8)]
		[DataMember]
		public double PriceAmount { get; set; }

		[ProtoMember(9)]
		[DataMember]
		public float RetailPrice { get; set; }

		public static BasicTypes CreateObject()
		{
			var obj = new BasicTypes()
			{
				Lng = Language.Csharp,
				Date = DateTime.Now,
				Text = "DateTime ticks: " + DateTime.Now.Ticks.ToString(),
				Data = new byte[] { 66, 20, 30, 50, 90, 122, 50, 22, 0, 0, 0, 16, 19, 177 },
				StrArray = new string[] { "This is ", "Salar", "BOIS", "Test" },
				guid = Guid.NewGuid(),
				PriceAmount = 13.5,
				RetailPrice = 15
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
