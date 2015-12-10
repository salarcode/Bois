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
	public class SimpleCollections
	{
		[ProtoMember(1)]
		[DataMember]
		public string[] Names { get; set; }

		[ProtoMember(2)]
		[DataMember]
		public int[] Ages { get; set; }

		[ProtoMember(3)]
		[DataMember]
		public List<string> Addresses { get; set; }

		[ProtoMember(4)]
		[DataMember]
		public Dictionary<string, int> Dictionary1 { get; set; }

		[ProtoMember(5)]
		[DataMember]
		public Dictionary<int, string> Dictionary2 { get; set; }

		[ProtoMember(6)]
		[DataMember]
		public SortedDictionary<int, string> SortedDictionary { get; set; }

		//[ProtoMember(7)]
		//[DataMember]
		//public SortedList<int, string> SortedList { get; set; }


		public static SimpleCollections CreateSimple()
		{
			var obj =
				new SimpleCollections
				{
					Addresses = new List<string>() {"Addr1", "", "Addr3"},
					Dictionary2 = new Dictionary<int, string>()
					{
						{1, "1"},
						{2, "2"},
						{3, "3"}
					},
					SortedDictionary = new SortedDictionary<int, string>()
					{
						{10, "ten"},
						{20, "20"},
						{4, "four"}
					},
					//SortedList = new SortedList<int, string>()
					//					 {
					//						 {10, "ten"},
					//						 {20, "20"},
					//						 {4, "four"},
					//						 {3, "zero"}
					//					 },
					Ages = new int[] {30, 27, 17, 70},
					Names = new string[] {"Salar", "BOIS", "Codeplex"},
					Dictionary1 = new Dictionary<string, int>()
					{
						{"NULL", 0},
						{"one", 1},
						{"40", 40}
					},
				};
			return obj;
		}

		public static SimpleCollections CreateBig()
		{
			var obj =
				new SimpleCollections
				{
					Addresses = new List<string>() {"Addr1", "", "Addr3"},
					Dictionary2 = new Dictionary<int, string>()
					{
						{1, "1"},
						{2, "2"},
						{3, "3"}
					},
					SortedDictionary = new SortedDictionary<int, string>()
					{
						{10, "ten"},
						{20, "20"},
						{4, "four"}
					},
					//SortedList = new SortedList<int, string>()
					//					 {
					//						 {10, "ten"},
					//						 {20, "20"},
					//						 {4, "four"},
					//						 {3, "zero"}
					//					 },
					Ages = new int[] {30, 27, 17, 70},
					Names = new string[] {"Salar", "BOIS", "Codeplex"},
					Dictionary1 = new Dictionary<string, int>()
					{
						{"NULL", 0},
						{"one", 1},
						{"40", 40}
					},
				};
			obj.Ages = new int[short.MaxValue];
			for (int i = 0; i < byte.MaxValue; i++)
			{
				obj.Ages[i] = i + 10;
				obj.Addresses.Add("add-" + i);
				obj.Dictionary1.Add("D1-" + i, i + 500);
				obj.Dictionary2.Add(i + 200, "D2-" + i);
				obj.SortedDictionary.Add(i + 200, "SD-" + i);
			}
			return obj;
		}

	}
}
