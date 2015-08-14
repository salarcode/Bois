using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	public class InterfaceDictAsProperty1 : IBaseType
	{
		public IDictionary<int, string> GenericDictionaryInt { get; set; }
		public IDictionary<string, int> GenericDictionaryStr { get; set; }
		public string Name { get; set; }


		public void Initialize()
		{
			GenericDictionaryStr = new Dictionary<string, int>();
			GenericDictionaryStr.Add("", 3);
			GenericDictionaryStr.Add("ten_1", 3);
			GenericDictionaryStr.Add("twenty_2", 4);

			GenericDictionaryInt = new Dictionary<int, string>();
			GenericDictionaryInt.Add(5, null);
			GenericDictionaryInt.Add(10, "ten");
			GenericDictionaryInt.Add(20, "twenty");

			Name = "Salar Kh";
		}
	}
}
