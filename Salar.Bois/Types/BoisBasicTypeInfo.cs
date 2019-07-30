using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salar.Bois.Types
{
	class BoisBasicTypeInfo
	{
		/// <summary>
		/// Only used when deciding to serialize root instance not used when properties/field of the object being serialized
		/// </summary>
		public bool AsRootNeedsCompute;

		/// <summary>
		/// Holds the member-type or Array Item type or nullable item type
		/// </summary>
		public Type BareType;

		public EnBasicKnownType KnownType;

		public bool IsNullable;

		public override string ToString()
		{
			return $"{KnownType} {BareType}";
		}
	}
}
