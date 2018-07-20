#define DotNet
using System;
using Salar.Bois;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Reflection.Emit;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois.Types
{
	class BoisTypeCache_New
	{
		public BoisTypeCache_New()
		{
		}

		internal object GetComputedType(Type type, bool b)
		{

			return null;
		}

		/// <summary>
		/// Is this primitive type that doesn't need compilation directly
		/// </summary>
		internal bool IsPrimitveType(Type memType)
		{
			if (memType == typeof(string))
			{
				return true;
			}
			Type memActualType = memType;
			Type underlyingTypeNullable;
			bool isNullable = ReflectionHelper.IsNullable(memType, out underlyingTypeNullable);

			// check the underling type
			if (isNullable && underlyingTypeNullable != null)
			{
				memActualType = underlyingTypeNullable;
			}
			else
			{
				underlyingTypeNullable = null;
			}


			if (memActualType == typeof(char))
			{
				// is struct and uses Nullable<>
				return true;
			}
			if (memActualType == typeof(bool))
			{
				// is struct and uses Nullable<>
				return true;
			}
			if (memActualType == typeof(DateTime))
			{
				// is struct and uses Nullable<>
				return true;
			}
			if (memActualType == typeof(DateTimeOffset))
			{
				// is struct and uses Nullable<>
				return true;
			}
			if (memActualType == typeof(byte[]))
			{
				return true;
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Enum)))
			{
				return true;
			}

			BoisPrimitiveTypeInfo output;
			if (IsNumber(memActualType, out output))
			{
				return true;
			}

			if (memActualType == typeof(TimeSpan))
			{
				// is struct and uses Nullable<>
				return true;
			}

			if (memActualType == typeof(Version))
			{
				return true;
			}

			if (memActualType == typeof(Guid))
			{
				// is struct and uses Nullable<>
				return true;
			}
#if DotNet || DotNetCore || DotNetStandard
			if (memActualType == typeof(DBNull))
			{
				// ignore!
				return true;
			}
			if (memActualType == typeof(Color))
			{
				// is struct and uses Nullable<>
				return true;
			}
#endif
#if DotNet || DotNetCore || DotNetStandard
			if (ReflectionHelper.CompareSubType(memActualType, typeof(NameValueCollection)))
			{
				return false;
			}
#endif

			if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			{
				return false;
			}

			var isGenericType = memActualType.IsGenericType;
			Type[] interfaces = null;
			if (isGenericType)
			{
				return false;
			}

			if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)))
			{
				return false;
			}

			// checking for IList and ICollection should be after NameValueCollection
			if (ReflectionHelper.CompareInterface(memActualType, typeof(IList)) ||
				ReflectionHelper.CompareInterface(memActualType, typeof(ICollection)))
			{
				return false;
			}

#if !SILVERLIGHT && DotNet
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataSet)))
			{
				return false;
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataTable)))
			{
				return false;
			}

#endif


 
			return false;
		}

		private bool IsNumber(Type memType, out BoisPrimitiveTypeInfo output)
		{
			if (memType.IsClass)
			{
				output = null;
				return false;
			}
			if (memType == typeof(int))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.Int32,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(long))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.Int64,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(short))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.Int16,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(double))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.Double,
				};
			}
			else if (memType == typeof(decimal))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.Decimal,
				};
			}
			else if (memType == typeof(float))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.Single,
				};
			}
			else if (memType == typeof(byte))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.Byte,
				};
			}
			else if (memType == typeof(sbyte))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.SByte,
				};
			}
			else if (memType == typeof(ushort))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.UInt16,
				};
			}
			else if (memType == typeof(uint))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.UInt32,
				};
			}
			else if (memType == typeof(ulong))
			{
				output = new BoisPrimitiveTypeInfo
				{
					KnownType = EnBoisKnownType.UInt64,
				};
			}
			else
			{
				output = null;
				return false;
			}
			return true;
		}

	
	}

	class BoisPrimitiveTypeInfo
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
