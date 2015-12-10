using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ProtoBuf;

namespace Salar.BoisBenchmark.TestObjects
{
	[Serializable()]
	[ProtoContract]
	[DataContract]
	public class ComplexCollections
	{
		[ProtoMember(3)]
		[DataMember]
		public List<Language> ListLang { get; set; }

		[ProtoMember(4)]
		[DataMember]
		public Dictionary<string, int?> Dictionary1 { get; set; }

		[ProtoMember(5)]
		[DataMember]
		public Dictionary<decimal?, DateTime?> Dictionary2 { get; set; }

		[ProtoMember(6)]
		[DataMember]
		public SortedDictionary<int, string> SortedDictionary { get; set; }

		[ProtoMember(7)]
		[DataMember]
		public SortedList<int?, string> SortedList { get; set; }

		//[ProtoMember(8)]
		//[DataMember]
		//public SortedSet<int?> SortedSet { get; set; }


		public static ComplexCollections CreateSimple()
		{
			var obj =
				new ComplexCollections
				{
					ListLang = new List<Language>() {Language.C, Language.Cpp, Language.Csharp, Language.Javascript, Language.Php},
					Dictionary2 = new Dictionary<decimal?, DateTime?>()
					{
						{1, DateTime.MaxValue},
						{15, null},
						{35, DateTime.MinValue},
						{45, DateTime.Today}
					},
					SortedDictionary = new SortedDictionary<int, string>()
					{
						{10, "ten"},
						{20, "20"},
						{4, "four"}
					},
					SortedList = new SortedList<int?, string>()
					{
						{10, "ten"},
						{20, "20"},
						{-1, "NULL"},
						{4, "four"},
					},
					//SortedSet = new SortedSet<int?>()
					//{
					//	{10},
					//	{20},
					//	{null},
					//	{4},
					//},
					Dictionary1 = new Dictionary<string, int?>()
					{
						{"NULL", null},
						{"one", 1},
						{"40", 40}
					},
				};
			return obj;
		}

		public static ComplexCollections CreateBig()
		{
			var obj =
				new ComplexCollections
				{
					ListLang = new List<Language>() { Language.C, Language.Cpp, Language.Csharp, Language.Javascript, Language.Php },
					Dictionary2 = new Dictionary<decimal?, DateTime?>()
					{
						{1, DateTime.MaxValue},
						{15, null},
						{35, DateTime.MinValue},
						{45, DateTime.Today}
					},
					SortedDictionary = new SortedDictionary<int, string>()
					{
						{10, "ten"},
						{20, "20"},
						{4, "four"}
					},
					SortedList = new SortedList<int?, string>()
					{
						{10, "ten"},
						{20, "20"},
						{-1, "NULL"},
						{4, "four"},
					},
					//SortedSet = new SortedSet<int?>()
					//{
					//	{10},
					//	{20},
					//	{null},
					//	{4},
					//},
					Dictionary1 = new Dictionary<string, int?>()
					{
						{"NULL", null},
						{"one", 1},
						{"40", 40}
					},
				};
 			for (int i = 0; i < byte.MaxValue; i++)
 			{
 				obj.SortedList.Add(i + 500, "SL-" + i);
 				obj.Dictionary1.Add("D1-" + i, i + 500);
 				obj.Dictionary2.Add(i + 200, DateTime.Now.AddDays(i));
				obj.SortedDictionary.Add(i + 200, "SD-" + i);
			}
			return obj;
		}

	}
}
