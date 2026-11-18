using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Salar.Bois.Types
{
	class BoisComplexTypeInfo
	{
		/// <summary>
		/// Has Fields or Properties
		/// </summary>
		//public bool IsContainerObject;

		public EnComplexKnownType ComplexKnownType;

		public bool IsNullable;
		public bool IsGeneric;
		
		/// <summary>
		/// IsValueType
		/// </summary>
		public bool IsStruct;

		/// <summary>
		/// Holds the member-type or Array Item type or nullable item type
		/// </summary>
		public Type BareType;

		/// <summary>
		/// List of members
		/// </summary>
		public MemberInfo[] Members;

		/// <summary>
		/// Slots 1..N of a versioned type (<c>[BoisContract(Versioned = true)]</c>). Null for other types.
		/// For a versioned type, <see cref="Members"/> holds its members in slot order.
		/// </summary>
		public VersionedSlot[] VersionedSlots;
	}
}
