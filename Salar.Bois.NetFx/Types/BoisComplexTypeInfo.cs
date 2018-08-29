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
		
		//public bool IsStringDictionary;
		//public bool IsDictionary;
		//public bool IsCollection;
		//public bool IsArray;
		////////public bool IsSupportedPrimitive;

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
	}
}
