using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salar.Bois.Types
{
	class BoisMemberInfo
	{
		public EnBoisKnownType KnownType { get; set; }

		public bool IsNullable { get; set; }

		/// <summary>
		/// The original type that is hidden under Nullable<> is hidden as array item type like Type[]
		/// </summary>
		public Type BareType { get; set; }
	}
}
