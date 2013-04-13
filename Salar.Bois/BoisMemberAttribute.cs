using System;

namespace Salar.Bois
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class BoisMemberAttribute : Attribute
	{
		public int Index { get; private set; }
		public bool Included { get; private set; }

		public BoisMemberAttribute(int index, bool included)
		{
			Index = index;
			Included = included;
		}

		public BoisMemberAttribute()
			: this(-1, true) 
		{ }

		public BoisMemberAttribute(int index)
			: this(index, true) 
		{ }

		public BoisMemberAttribute(bool included)
			: this(-1, included) 
		{ }
	}

}
