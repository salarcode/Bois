using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Salar.Bois.Tests.Objects
{
	[Serializable()]
	[ProtoContract]
	public class CommonListChildObject : CommonListParentObject<int>, ICommonListGeneralInterface<float>
	{
		[ProtoMember(1)]
		public string ListName { get; set; }
		[ProtoMember(2)]
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
	[ProtoContract]
	public class CommonListParentObject<T> : List<string>
	{
		[ProtoMember(3)]
		public T Holder { get; set; }
		[ProtoMember(1)]
		public string DocName { get; set; }
		[ProtoMember(2)]
		public DateTime CreateDate { get; set; }
		 
	}

	public interface ICommonListGeneralInterface<T>
	{
		T Age { get; set; }
	}
}
