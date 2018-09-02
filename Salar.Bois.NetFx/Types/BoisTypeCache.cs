#define DotNet
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Reflection;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois.Types
{
	static class BoisTypeCache
	{
		private static readonly BoisComputedTypeHashtable<BoisComputedTypeInfo> _computedCache;
		private static readonly BoisComputedTypeHashtable<BoisBasicTypeInfo> _basicTypeCache;
		static BoisTypeCache()
		{
			_computedCache = new BoisComputedTypeHashtable<BoisComputedTypeInfo>();
			_basicTypeCache = new BoisComputedTypeHashtable<BoisBasicTypeInfo>();
		}


		internal static void ClearCache()
		{
			_basicTypeCache.Clear();
			_computedCache.Clear();
		}

		internal static BoisComputedTypeInfo GetRootTypeComputed(
			Type type,
			bool generateReader,
			bool generateWriter
#if EmitAssemblyOut
			,bool outputAssembly = true
#endif
			)
		{
			BoisComputedTypeInfo result;
			if (_computedCache.TryGetValue(type, out result))
			{
				if ((!generateReader || result.ReaderMethod != null) &&
					(!generateWriter || result.WriterMethod != null))
					return result;
			}
			else
			{
				result = new BoisComputedTypeInfo();

				_computedCache.TryAdd(type, result);
			}

			BoisComplexTypeInfo complexTypeInfo = null;

			if (generateWriter && result.WriterDelegate == null)
			{
				complexTypeInfo = GetComplexTypeUnCached(type);

#if EmitAssemblyOut
				var computed = BoisTypeCompiler.ComputeWriterSaveAss(type, complexTypeInfo, outputAssembly,
					(dynamicMethod) =>
					{
						// this call is usefull for Recursive Methods

						result.WriterMethod = dynamicMethod;
					});
#else
				var computed = BoisTypeCompiler.ComputeWriter(type, complexTypeInfo,
					(dynamicMethod) =>
					{
						// this call is usefull for Recursive Methods

						result.WriterMethod = dynamicMethod;
					});
#endif
				result.WriterDelegate = computed.Delegate;
				result.WriterMethod = computed.Method;
			}

			if (generateReader && result.ReaderDelegate == null)
			{
				if (complexTypeInfo == null)
					complexTypeInfo = GetComplexTypeUnCached(type);

#if EmitAssemblyOut
				var computed = BoisTypeCompiler.ComputeReaderSaveAss(type, complexTypeInfo, outputAssembly,
					(dynamicMethod) =>
					{
						// this call is usefull for Recursive Methods

						result.ReaderMethod = dynamicMethod;
					});
#else
				var computed = BoisTypeCompiler.ComputeReader(type, complexTypeInfo,
					(dynamicMethod) =>
					{
						// this call is usefull for Recursive Methods

						result.ReaderMethod = dynamicMethod;
					});
#endif
				result.ReaderDelegate = computed.Delegate;
				result.ReaderMethod = computed.Method;
			}


			return result;
		}

		internal static BoisBasicTypeInfo GetBasicType(Type type)
		{
			BoisBasicTypeInfo result;
			if (_basicTypeCache.TryGetValue(type, out result))
			{
				return result;
			}

			result = ReadBasicTypeInfo(type);
			_basicTypeCache.TryAdd(type, result);

			return result;
		}

		internal static BoisComplexTypeInfo GetComplexTypeUnCached(Type type)
		{
			var basicType = GetBasicType(type);
			if (basicType.KnownType != EnBasicKnownType.Unknown)
			{
				// TODO: should I throw exception?
				return null;
			}

			return ReadComplexType(type);
		}

		private static BoisBasicTypeInfo ReadBasicTypeInfo(Type memType)
		{
			if (memType == typeof(string))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.String,
					IsNullable = true,
					AsRootNeedsCompute = false,
					BareType = typeof(string)
					//IsSupportedPrimitive = true,
				};
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


			if (memActualType == typeof(bool))
			{
				// is struct and uses Nullable<>
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Bool,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}

			// numbers
			var numberKnownType = ReadBasicTypeInfo_Number(memActualType);
			if (numberKnownType != null && numberKnownType.KnownType != EnBasicKnownType.Unknown)
			{
				numberKnownType.IsNullable = isNullable;
				return numberKnownType;
			}

			if (memActualType == typeof(DateTime))
			{
				// is struct and uses Nullable<>
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.DateTime,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(DateTimeOffset))
			{
				// is struct and uses Nullable<>
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.DateTimeOffset,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(byte[]))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.ByteArray,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			{
				var arrayItemType = memActualType.GetElementType();

				if (arrayItemType == null)
					throw new Exception($"Unknown array '{memType}' with no element type not suported.");

				var arrayItemInfo = ReadBasicTypeInfo(arrayItemType);

				if (arrayItemInfo.KnownType == EnBasicKnownType.Unknown)
				{
					// as root this type of array can't be writted directly
					arrayItemInfo.AsRootNeedsCompute = true;
					arrayItemInfo.IsNullable = isNullable;
					arrayItemInfo.BareType = arrayItemType;
				}
				else
				{
					arrayItemInfo.AsRootNeedsCompute = false;
					arrayItemInfo.IsNullable = isNullable;
					arrayItemInfo.BareType = arrayItemType;
					arrayItemInfo.KnownType = EnBasicKnownType.KnownTypeArray;
				}
				return arrayItemInfo;
			}

			if (ReflectionHelper.CompareSubType(memActualType, typeof(Enum)))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Enum,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}


			if (memActualType == typeof(TimeSpan))
			{
				// is struct and uses Nullable<>
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.TimeSpan,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(char))
			{
				// is struct and uses Nullable<>
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Char,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable
				};
			}

			if (memActualType == typeof(Guid))
			{
				// is struct and uses Nullable<>
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Guid,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}

#if DotNet || DotNetCore || DotNetStandard
			if (memActualType == typeof(Color))
			{
				// is struct and uses Nullable<>
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Color,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(DBNull))
			{
				// ignore!
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.DbNull,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}
#endif
			if (memActualType == typeof(Uri))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Uri,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(Version))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Version,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}


#if !SILVERLIGHT && DotNet
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataSet)))
			{
				return new BoisBasicTypeInfo
				{
					IsNullable = isNullable,
					KnownType = EnBasicKnownType.DataSet,
					BareType = underlyingTypeNullable,
				};
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataTable)))
			{
				return new BoisBasicTypeInfo
				{
					IsNullable = isNullable,
					KnownType = EnBasicKnownType.DataTable,
					BareType = underlyingTypeNullable,
				};
			}

#endif
			// not compatible simple type found
			// cant be used as root, should be computed
			return new BoisBasicTypeInfo()
			{
				BareType = underlyingTypeNullable,
				KnownType = EnBasicKnownType.Unknown,
				IsNullable = isNullable,
				AsRootNeedsCompute = true
			};
		}

		private static BoisBasicTypeInfo ReadBasicTypeInfo_Number(Type memType)
		{
			if (memType.IsClass)
			{
				return null;
			}
			if (memType == typeof(int))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Int32,
					BareType = typeof(int),
					AsRootNeedsCompute = false,
				};
			}
			else if (memType == typeof(long))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Int64,
					BareType = typeof(long),
					AsRootNeedsCompute = false,
				};
			}
			else if (memType == typeof(short))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Int16,
					BareType = typeof(short),
					AsRootNeedsCompute = false,
				};
			}
			else if (memType == typeof(double))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Double,
					BareType = typeof(double),
					AsRootNeedsCompute = false,
				};
			}
			else if (memType == typeof(decimal))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Decimal,
					BareType = typeof(decimal),
					AsRootNeedsCompute = false,
				};
			}
			else if (memType == typeof(float))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Single,
					BareType = typeof(float),
					AsRootNeedsCompute = false,
				};
			}
			else if (memType == typeof(byte))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.Byte,
					BareType = typeof(byte),
					AsRootNeedsCompute = false,
				};
			}
			else if (memType == typeof(sbyte))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.SByte,
					BareType = typeof(sbyte),
					AsRootNeedsCompute = false,
				};
			}
			else if (memType == typeof(ushort))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.UInt16,
					BareType = typeof(ushort),
					AsRootNeedsCompute = false,
				};
			}
			else if (memType == typeof(uint))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.UInt32,
					BareType = typeof(uint),
					AsRootNeedsCompute = false,
				};
			}
			else if (memType == typeof(ulong))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.UInt64,
					BareType = typeof(ulong),
					AsRootNeedsCompute = false,
				};
			}

			return null;
		}

		private static BoisComplexTypeInfo ReadComplexType(Type memType)
		{
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



			if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			{
				return new BoisComplexTypeInfo
				{
					IsNullable = isNullable,
					ComplexKnownType = EnComplexKnownType.UnknownArray,
					BareType = underlyingTypeNullable,
				};
			}

			var isGenericType = memActualType.IsGenericType;
			Type[] interfaces = null;
			if (isGenericType)
			{
				//// no more checking for a dictionary with its first argumnet as String
				//if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)) &&
				//	memActualType.GetGenericArguments()[0] == typeof(string))
				//	return new BoisComplexTypeInfo
				//	{
				//		KnownType = EnBasicKnownType.Unknown,
				//		IsNullable = isNullable,
				//		IsDictionary = true,
				//		IsStringDictionary = true,
				//		IsGeneric = true,
				//		ComplexKnownType = EnComplexKnownType.UnknownArray,
				//		NullableUnderlyingType = underlyingTypeNullable,
				//	};

				interfaces = memActualType.GetInterfaces();

				if (ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(IDictionary<,>)) ||
					memActualType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
					return new BoisComplexTypeInfo
					{
						IsNullable = isNullable,
						ComplexKnownType = EnComplexKnownType.Dictionary,
						IsGeneric = true,
						BareType = underlyingTypeNullable,
					};

				if (ReflectionHelper.CompareInterface(memType, typeof(ISet<>)))
					return new BoisComplexTypeInfo
					{
						IsNullable = isNullable,
						IsGeneric = true,
						ComplexKnownType = EnComplexKnownType.ISet,
						BareType = underlyingTypeNullable,
					};
			}


			// the IDictionary should be checked before IList<>
			if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)))
			{
				return new BoisComplexTypeInfo
				{
					IsNullable = isNullable,
					ComplexKnownType = EnComplexKnownType.Dictionary,
					IsGeneric = true,
					BareType = underlyingTypeNullable,
				};
			}
			if (isGenericType)
			{
				//ConcurrentBag<int> a;
				//ConcurrentStack<int> s;

				//// Concurent ones hould be checked before IList
				//if (ReflectionHelper.CompareInterface(memActualType, typeof(IProducerConsumerCollection<>)))
				//{
				//	return new BoisComplexTypeInfo
				//	{
				//		IsNullable = isNullable,
				//		IsGeneric = memActualType.IsGenericType,
				//		//IsCollection = true,
				//		//IsArray = true,
				//		ComplexKnownType = EnComplexKnownType.Collection,
				//		BareType = underlyingTypeNullable,
				//	};
				//}

				if (ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(IList<>)) ||
					ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(ICollection<>)))
					return new BoisComplexTypeInfo
					{
						IsNullable = isNullable,
						IsGeneric = true,
						//IsCollection = true,
						//IsArray = true,
						ComplexKnownType = EnComplexKnownType.Collection,
						BareType = underlyingTypeNullable,
					};
			}

#if DotNet || DotNetCore || DotNetStandard
			if (ReflectionHelper.CompareSubType(memActualType, typeof(NameValueCollection)))
			{
				return new BoisComplexTypeInfo
				{
					//KnownType = EnBasicKnownType.NameValueColl,
					IsNullable = isNullable,
					ComplexKnownType = EnComplexKnownType.NameValueColl,
					BareType = underlyingTypeNullable,
				};
			}
#endif

			// checking for IList and ICollection should be after NameValueCollection
			if (ReflectionHelper.CompareInterface(memActualType, typeof(IList)) ||
				ReflectionHelper.CompareInterface(memActualType, typeof(ICollection)))
			{
				return new BoisComplexTypeInfo
				{
					IsNullable = isNullable,
					IsGeneric = memActualType.IsGenericType,
					//IsCollection = true,
					//IsArray = true,
					ComplexKnownType = EnComplexKnownType.Collection,
					BareType = underlyingTypeNullable,
				};
			}

			var objectMemInfo = ReadComplexTypeMembers(memType);
			objectMemInfo.BareType = underlyingTypeNullable;
			objectMemInfo.IsNullable = isNullable;

			return objectMemInfo;
		}

		private static BoisComplexTypeInfo ReadComplexTypeMembers(Type type)
		{
			bool readFields = true, readProps = true;

			var objectAttr = type.GetCustomAttributes(typeof(BoisContractAttribute), false);
			if (objectAttr.Length > 0)
			{
				var boisContract = objectAttr[0] as BoisContractAttribute;
				if (boisContract != null)
				{
					readFields = boisContract.Fields;
					readProps = boisContract.Properties;
				}
			}
			var typeInfo = new BoisComplexTypeInfo
			{
				//MemberType = EnBoisMemberType.Object,
				//KnownType = EnBasicKnownType.Unknown,
				//IsContainerObject = true,
				ComplexKnownType = EnComplexKnownType.Unknown,
				BareType = type,
				IsStruct = type.IsValueType
			};

			var members = new List<MemberInfo>();

			if (readFields)
			{
				var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (var f in fields)
				{
					var index = -1;
					var memProp = f.GetCustomAttributes(typeof(BoisMemberAttribute), false);
					BoisMemberAttribute boisMember;
					if (memProp.Length > 0 && (boisMember = (memProp[0] as BoisMemberAttribute)) != null)
					{
						if (!boisMember.Included)
							continue;
						index = boisMember.Index;
					}

					//var info = ReadMemberInfo(f.FieldType);
					//info.Info = f;
					//info.MemberType = EnBoisMemberType.Field;

					if (index > -1)
					{
						members.Insert(index, f);
					}
					else
					{
						members.Add(f);
					}
				}
			}

			if (readProps)
			{
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

				foreach (var p in props)
				{
					if (p.CanWrite)
					{
						var index = -1;
						var memProp = p.GetCustomAttributes(typeof(BoisMemberAttribute), false);
						BoisMemberAttribute boisMember;
						if (memProp.Length > 0 && (boisMember = (memProp[0] as BoisMemberAttribute)) != null)
						{
							if (!boisMember.Included)
								continue;
							index = boisMember.Index;
						}

						//var info = ReadMemberInfo(p.PropertyType);
						//info.PropertyGetter = GetPropertyGetter(type, p);
						////info.PropertySetter = CreateSetMethod(p);
						//info.PropertySetter = GetPropertySetter(type, p);
						//info.Info = p;
						//info.MemberType = EnBoisMemberType.Property;

						if (index > -1)
						{
							members.Insert(index, p);
						}
						else
						{
							members.Add(p);
						}
					}
				}
			}

			typeInfo.Members = members.ToArray();

			return typeInfo;
		}

	}
}
