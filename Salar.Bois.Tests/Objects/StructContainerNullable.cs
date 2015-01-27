using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salar.Bois.Tests.Objects
{
	public class StructContainerNullable : IBaseType
	{
		public StructContainerNullable_Struct? Nullable_Null { get; set; }
		public StructContainerNullable_Struct? Nullable_Full { get; set; }
		public StructContainerNullable_Struct Normal_Full { get; set; }

		public void Initialize()
		{
			Nullable_Null = null;
			Nullable_Full = new StructContainerNullable_Struct
			{
				TestProp = "Nullable کرک Full!"
			};
			Normal_Full = new StructContainerNullable_Struct
			{
				TestProp = "Normal ک چ پ ژ ی Full"
			};
		}
	}

	public struct StructContainerNullable_Struct
	{
		public string TestProp { get; set; }
	}
}
