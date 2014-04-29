using System;
using System.Drawing;

namespace Salar.Bois.Tests.Objects
{
	public class BasicTypesDateTime : IBaseType
	{
		public string Name { get; set; }
		public DateTime? BirthDate { get; set; }
		public DateTime RegDate { get; set; }
		public int Age { get; set; }
		public int Children { get; set; }

		public void Initialize()
		{
			Name = "Salar Khalilzadeh";
			BirthDate = new DateTime(2014, 2, 25, 4, 0, 16);
			RegDate = new DateTime(2014, 2, 25, 4, 0, 16, DateTimeKind.Utc);
			Age = 27;
			Children = 10;
		}
	}
}
