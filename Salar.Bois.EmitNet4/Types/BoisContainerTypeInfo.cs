using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Types
{
	class BoisContainerTypeInfo : BoisTypeInfo
	{
		public BoisTypeInfo[] Members;
#if DEBUG
		public override string ToString()
		{
			return string.Format("{0}: {1}: {2}: Members= ", MemberType, KnownType,// Info,
				Members?.Length ?? 0);
		}
#endif
	}

}
