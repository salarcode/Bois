﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	public class HierarchyWithStruct : IBaseType
	{
		public StructType1 SType { get; set; }
		public List<int> AcceptableAges { get; set; }
		public string LastName { get; set; }

		public void Initialize()
		{
			SType = StructType1.InitializeThis();
			LastName = "HierarchyWithStruct-" + DateTime.Now.Ticks;
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

