using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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

		public EnBoisMemberType MemberType;
		public EnBoisKnownType KnownType;
		public MemberInfo Info;
		public Type NullableUnderlyingType;

		public Function<object, object, object> PropertySetter;
		public BoisTypeCache.GenericGetter PropertyGetter;
		public override string ToString()
		{
			return string.Format("{0}: {1}: {2}", MemberType, KnownType, Info);
		}
	}
}
