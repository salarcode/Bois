using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Salar.Bois.Types
{
	class BoisTypeInfo
	{
		public bool IsNullable;
		public bool IsGeneric;
		public bool IsStringDictionary;
		public bool IsDictionary;
		public bool IsCollection;
		public bool IsArray;
		public bool IsSupportedPrimitive;

		/// <summary>
		/// Has Fields or Properties
		/// </summary>
		public bool IsContainerObject;

		/// <summary>
		/// IsValueType
		/// </summary>
		public bool IsStruct;

		internal EnBoisMemberType MemberType;
		internal EnBoisKnownType KnownType;
		//public MemberInfo Info;

		/// <summary>
		/// if the type is value-type and is nullable, the underlying type
		/// </summary>
		public Type NullableUnderlyingType;

		//public Function<object, object, object> PropertySetter;
		//public BoisTypeCache.GenericGetter PropertyGetter;

#if DEBUG
		public override string ToString()
		{
			return $"{MemberType}: {KnownType} ";
		}
#endif
	}
}
