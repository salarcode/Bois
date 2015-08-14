using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	public class CollectionTypes1 : IBaseType
	{
		public void Initialize()
		{
			Addresses = new List<string>() { "Addr1", "", "Addr3" };
			Dictionary = new Dictionary<int, string>()
				             {
					             {1, "1"},
					             {2, "2"},
					             {3, "3"}
				             };
			SortedDictionary = new SortedDictionary<int, string>()
				                   {
					                   {10, "ten"},
					                   {20, "20"},
					                   {4, "four"}
				                   };
			SortedList = new SortedList<int, string>()
				             {
					             {10, "ten"},
					             {20, "20"},
					             {4, "four"},
					             {0, "zero"}
				             };
			Ages = new int[] { 30, 27, 17, 70 };
			Names = new string[] { "Salar", "Bois", "Codeplex" };
			StringDictionary = new Dictionary<string, int?>()
				                   {
					                   {"NULL", null},
					                   {"one", 1},
					                   {"40", 40}
				                   };
		}


		public string[] Names { get; set; }

		public int[] Ages { get; set; }

		public List<string> Addresses { get; set; }

		public IDictionary<string, int?> StringDictionary { get; set; }

		public IDictionary<int, string> Dictionary { get; set; }

		public SortedDictionary<int, string> SortedDictionary { get; set; }

		public SortedList<int, string> SortedList { get; set; }

	}
}
