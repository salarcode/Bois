using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	internal class ObjectWithPrivateSetter : IBaseType
	{
		public ObjectWithPrivateSetter()
		{

		}

		public ObjectWithPrivateSetter(string name, int age)
		{
			this.Name = name;
			this.Age = age;
		}

		public string Name { private set; get; }

		public int Age { get; private set; }
		public void Initialize()
		{

			Name = "Salar";
			Age = int.MaxValue;
		}
	}
}
