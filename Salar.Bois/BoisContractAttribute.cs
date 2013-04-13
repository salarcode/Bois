using System;

namespace Salar.Bois
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class BoisContractAttribute : Attribute
	{
		public bool Fields { get; set; }
		public bool Properties { get; set; }

		public BoisContractAttribute(bool fields, bool properties)
		{
			Fields = fields;
			Properties = properties;
		}

		public BoisContractAttribute()
			: this(true, true)
		{ }
	}
}
