using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	public class HierarchyObjects1 : IBaseType
	{
		public class TheChild : IBaseType
		{
			public string Name { get; set; }
			public int Age { get; set; }
			public Guid SessionID { get; set; }
			public DateTime Birth { get; set; }
			public void Initialize()
			{
				Name = "Test" + DateTime.Now.Ticks;
				Age = 11;
				SessionID = Guid.NewGuid();
			}
		}

		public string Name { get; set; }
		public TheChild Child1 { get; set; }
		public List<int> AcceptableAges { get; set; }
		public TheChild Child2 { get; set; }
		public string LastName { get; set; }

		public void Initialize()
		{
			Child1 = new TheChild();
			Child1.Initialize();
			Child1.Name += "_1";

			Child2 = new TheChild();
			Child2.Initialize();
			Child2.Name += "_2";

			Name = "Father-" + DateTime.Now.Ticks;
			LastName = "Grand-" + DateTime.Now.Ticks;
			AcceptableAges = new List<int>
				                 {
					                 10,
					                 17,
					                 20,
					                 22,
					                 30
				                 };
		}
	}
}
