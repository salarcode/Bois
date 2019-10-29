using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salar.Bois.Types
{
	class BoisBasicEnumTypeInfo
	{
		/// <summary>
		/// Holds the member-type or Array Item type or nullable item type
		/// </summary>
		public Type UnderlyingType;

		public EnBasicEnumType KnownType;

		public bool IsNullable;

		public Type BareType;

		public override string ToString()
		{
			return $"{KnownType} {UnderlyingType}";
		}
	}
}
