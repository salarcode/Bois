using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	/// <summary>
	/// Child class null property value
	/// </summary>
	public class HierarchyObjects2 : IBaseType
	{
		public class KidClass : IBaseType
		{
			public int ZeroInt { get; set; }
			public string Name { get; set; }
			public int Age { get; set; }
			public void Initialize()
			{
				ZeroInt = 0;
				Name = "Salar";
				Age = int.MaxValue;
			}
		}

		public KidClass KidNull { get; set; }
		public KidClass KidValue { get; set; }

		public void Initialize()
		{

			KidNull = null;
			KidValue = new KidClass()
			{
				ZeroInt = 0,
				Name = "Salar",
				Age = int.MaxValue
			};
		}
	}


}
