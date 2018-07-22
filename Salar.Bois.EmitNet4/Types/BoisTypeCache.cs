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
	sealed class BoisTypeCache
	{
		private readonly BoisComputedTypeHashtable _hashtable;
		public BoisTypeCache()
		{
			_hashtable = new BoisComputedTypeHashtable();
		}

		internal BoisComputedTypeInfo GetRootTypeComputed(Type type, bool generateReader, bool generateWriter)
		{
			BoisComputedTypeInfo result;
			if (_hashtable.TryGetValue(type, out result))
			{
				return result;
			}
			else
			{
				result = new BoisComputedTypeInfo();
			}

			if (generateWriter && result.WriterDelegate == null)
				result.WriterDelegate = BoisTypeCompiler.ComputeRootWriter(type);

			if (generateReader && result.ReaderDelegate == null)
				result.ReaderDelegate = BoisTypeCompiler.ComputeRootReader(type);

			_hashtable.TryAdd(type, result);

			return result;
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

			if (IsNumber(memActualType))
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
				var arrayItemType = memActualType.GetElementType();

				return IsPrimitveType(arrayItemType);
			}

			var isGenericType = memActualType.IsGenericType;
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

		private bool IsNumber(Type memType)
		{
			if (memType.IsClass)
			{
				return false;
			}
			if (memType == typeof(int))
			{
				return true;
			}
			else if (memType == typeof(long))
			{
				return true;
			}
			else if (memType == typeof(short))
			{
				return true;
			}
			else if (memType == typeof(double))
			{
				return true;
			}
			else if (memType == typeof(decimal))
			{
				return true;
			}
			else if (memType == typeof(float))
			{
				return true;
			}
			else if (memType == typeof(byte))
			{
				return true;
			}
			else if (memType == typeof(sbyte))
			{
				return true;
			}
			else if (memType == typeof(ushort))
			{
				return true;
			}
			else if (memType == typeof(uint))
			{
				return true;
			}
			else if (memType == typeof(ulong))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
