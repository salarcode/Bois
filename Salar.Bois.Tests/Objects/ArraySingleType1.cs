using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	class ArraySingleType1
	{
		public byte[] Arr { get; set; }
		public long Num2 { get; set; }

		public void Initialize()
		{
			// may have problem with ushort data size
			// BUG: https://github.com/salarcode/Bois/issues/1
			Arr = new byte[ushort.MaxValue + 5];

			// The same bug with special integer numbers
			Num2 = ushort.MaxValue + 10;
		}
	}
}
