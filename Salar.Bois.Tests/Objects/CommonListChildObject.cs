using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	[Serializable()]
	public class CommonListChildObject : CommonListParentObject<int>, ICommonListGeneralInterface<float>
	{
		public string ListName { get; set; }
		public DateTime SyncDate { get; set; }

		public static CommonListChildObject CreateObject()
		{
			var r = new CommonListChildObject()
			{
				"Item1","Item3","Item2","Item4",
			};
			r.ListName = "The Test";
			r.SyncDate = DateTime.Now.AddYears(-1);
			r.CreateDate = DateTime.Now;
			r.DocName = "Nothing";
			r.Holder = 20;
			r.Age = 2.5f;
			return r;
		}

		public float Age { get; set; }
	}

	[Serializable()]
	public class CommonListParentObject<T> : List<string>
	{
		public T Holder { get; set; }
		public string DocName { get; set; }
		public DateTime CreateDate { get; set; }

	}

	public interface ICommonListGeneralInterface<T>
	{
		T Age { get; set; }
	}
}
