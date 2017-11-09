using System;
using System.Collections.Generic;
using System.Text;

namespace Salar.Bois
{
	class BoisTypeInfo : BoisMemberInfo
	{
		public BoisMemberInfo[] Members;
#if DEBUG
		public override string ToString()
		{
			return string.Format("{0}: {1}: {2}: Members= {3}", MemberType, KnownType, Info,
								 Members?.Length ?? 0);
		}
#endif
	}
}
