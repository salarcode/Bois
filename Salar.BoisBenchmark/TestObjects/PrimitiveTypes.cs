using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Salar.BoisBenchmark.TestObjects
{
	[Serializable]
	[ProtoContract]
	[DataContract]
	public class PrimitiveTypes
	{
		[ProtoMember(1)]
		[DataMember]
		public Int32 Int32 { get; set; }

		[ProtoMember(2)]
		[DataMember]
		public Int64 Int64 { get; set; }

		[ProtoMember(3)]
		[DataMember]
		public string Text { get; set; }

		[ProtoMember(4)]
		[DataMember]
		public DateTime Date { get; set; }


		[ProtoMember(5)]
		[DataMember]
		public Language Lng { get; set; }


		[ProtoMember(6)]
		[DataMember]
		public Guid Guid { get; set; }


		[ProtoMember(7)]
		[DataMember]
		public double PriceAmount { get; set; }


		[ProtoMember(8)]
		[DataMember]
		public float RetailPrice { get; set; }

		public static PrimitiveTypes CreateSimple()
		{
			var obj = new PrimitiveTypes()
			{
				Lng = Language.Csharp,
				Date = DateTime.Now,
				Text = "DateTime ticks: " + DateTime.Now.Ticks.ToString(),
				Guid = Guid.NewGuid(),
				PriceAmount = 13.5,
				RetailPrice = 15,
				Int32 = 2015,
				Int64 = 2015*2
			};
			return obj;
		}
		public static PrimitiveTypes CreateBig()
		{
			var obj = new PrimitiveTypes()
			{
				Lng = Language.Csharp,
				Date = DateTime.Now,
				Text = "DateTime ticks: " + DateTime.Now.Ticks.ToString(),
				Guid = Guid.NewGuid(),
				PriceAmount = double.MaxValue,
				RetailPrice = float.MaxValue,
				Int32 = int.MaxValue,
				Int64 = Int64.MaxValue
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
