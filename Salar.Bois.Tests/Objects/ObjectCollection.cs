using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	public class ObjectCollectionContainer : IBaseType
	{

		public string Name { get; set; }

		public ObjectCollection ObjectColl { get; set; }

		public string Family { get; set; }


		public void Initialize()
		{
			Name = "Salar";
			Family = "Khalilzadeh";
			ObjectColl = new ObjectCollection { "item1", "item2", "item3" };

		}
	}


	public class ObjectCollection : List<string>
	{
		public string Foo
		{
			get { return "Hi"; }
			set { }
		}
	}
}
