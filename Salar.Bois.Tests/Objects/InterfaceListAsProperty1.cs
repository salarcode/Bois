using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	public class InterfaceListAsProperty1 : IBaseType
	{
		public IList<string> GenericList { get; set; }
		public string Name { get; set; }


		public void Initialize()
		{
			GenericList = new List<string> { "item1", "item2", "item3" };
			Name = "Salar Kh";
		}
	}
}
