using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Salar.BoisBenchmark.Objects
{
	[Serializable()]
	[ProtoContract]
	public class Collections
	{
		public static Collections CreateObject()
		{
			return
				new Collections
					{
						Addresses = new List<string>() { "Addr1", "", "Addr3" },
						Dictionary = new Dictionary<int, string>()
							             {
								             {1, "1"},
								             {2, "2"},
								             {3, "3"}
							             },
						//SortedDictionary = new SortedDictionary<int, string>()
						//					   {
						//						   {10, "ten"},
						//						   {20, "20"},
						//						   {4, "four"}
						//					   },
						//SortedList = new SortedList<int, string>()
						//				 {
						//					 {10, "ten"},
						//					 {20, "20"},
						//					 {4, "four"},
						//					 {0, "zero"}
						//				 },
						Ages = new int[] { 30, 27, 17, 70 },
						Names = new string[] { "Salar", "Bon", "Codeplex" },
						StringDictionary = new Dictionary<string, int?>()
											   {
												   {"NULL", null},
												   {"one", 1},
												   {"40", 40}
											   },
					};
		}


		[ProtoMember(1)]
		public string[] Names { get; set; }

		[ProtoMember(2)]
		public int[] Ages { get; set; }

		[ProtoMember(3)]
		public List<string> Addresses { get; set; }

		[ProtoMember(4)]
		public Dictionary<string, int?> StringDictionary { get; set; }

		[ProtoMember(5)]
		public Dictionary<int, string> Dictionary { get; set; }

		//[ProtoMember(6)]
		//public SortedDictionary<int, string> SortedDictionary { get; set; }

		//[ProtoMember(7)]
		//public SortedList<int, string> SortedList { get; set; }

	}
}
