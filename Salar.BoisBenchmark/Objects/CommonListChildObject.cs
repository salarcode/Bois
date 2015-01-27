using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ProtoBuf;

namespace Salar.Bois.Tests.Objects
{
	[Serializable()]
	[ProtoContract]
	[DataContract]
	public class CommonListChildObject : CommonListParentObject<int>, ICommonListGeneralInterface<float>
	{
		[ProtoMember(1)]
		[DataMember]
		public string ListName { get; set; }
		
		[ProtoMember(2)]
		[DataMember]
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

		[DataMember]
		public float Age { get; set; }
	}

	[Serializable()]
	[ProtoContract]
	[DataContract]
	public class CommonListParentObject<T> : List<string>
	{
		[ProtoMember(3)]
		[DataMember]
		public T Holder { get; set; }
		[ProtoMember(1)]
		[DataMember]
		public string DocName { get; set; }
		[ProtoMember(2)]
		[DataMember]
		public DateTime CreateDate { get; set; }
		 
	}

	public interface ICommonListGeneralInterface<T>
	{
		T Age { get; set; }
	}
}
