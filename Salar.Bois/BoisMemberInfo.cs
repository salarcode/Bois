using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois
{
	class BoisMemberInfo
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
		public MemberInfo Info;
		public Type NullableUnderlyingType;

		public Function<object, object, object> PropertySetter;
		public BoisTypeCache.GenericGetter PropertyGetter;

#if DEBUG
		public override string ToString()
		{
			return string.Format("{0}: {1}: {2}", MemberType, KnownType, Info);
		}
#endif
	}
}
