using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace HelloWorldApp.BusinessObjects
{
	[Serializable, DataContract]
	[ProtoContract]
	public class GenericObject<T>
	{
		public GenericObject()
		{
		}

		public GenericObject(T data)
		{
			Data = data;
		}
		[DataMember, ProtoMember(1)]

		public T Data { get; set; }
	}
}