using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;


namespace HelloWorldApp.BusinessObjects
{

	public interface IComplexObject { int SimpleInt { get; set; } }

	[Serializable, DataContract]
	public class ComplexObject : IComplexObject { public int SimpleInt { get; set; } }

	[Serializable,DataContract]
	public class ComplexObjectPolymorphicCollection : Collection<ComplexObject> {}

	[Serializable, DataContract]
	public class ComplexObjectCollection : Collection<ComplexObject> { }

	[Serializable, DataContract]
	public class ComplexObjectPolymorphicDictionary : Dictionary<int, ComplexObject>
	{
		public ComplexObjectPolymorphicDictionary()
		{
		}

		public ComplexObjectPolymorphicDictionary(int capacity) : base(capacity)
		{
		}

		public ComplexObjectPolymorphicDictionary(IEqualityComparer<int> comparer) : base(comparer)
		{
		}

		public ComplexObjectPolymorphicDictionary(int capacity, IEqualityComparer<int> comparer) : base(capacity, comparer)
		{
		}

		public ComplexObjectPolymorphicDictionary(IDictionary<int, ComplexObject> dictionary) : base(dictionary)
		{
		}

		public ComplexObjectPolymorphicDictionary(IDictionary<int, ComplexObject> dictionary, IEqualityComparer<int> comparer) : base(dictionary, comparer)
		{
		}

		protected ComplexObjectPolymorphicDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}

	[Serializable, DataContract]
	public class ComplexObjectDictionary : Dictionary<int, ComplexObject> { }
}