#define DotNet
using Salar.BinaryBuffers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
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
		/// <summary>
		/// Computed delegates are specialized per buffer type, so each buffer type needs its own cache partition.
		/// Returning a delegate compiled for a different buffer type would fail the cast in BoisComputedTypeInfo.
		/// </summary>
		private static readonly Dictionary<Type, BoisComputedTypeHashtable<BoisComputedTypeInfo>> _computedCachesByBuffer
			= new Dictionary<Type, BoisComputedTypeHashtable<BoisComputedTypeInfo>>();
		private static readonly BoisComputedTypeHashtable<BoisBasicTypeInfo> _basicTypeCache;
		private static readonly BoisComputedTypeHashtable<BoisBasicEnumTypeInfo> _basicEnumCache;
		static BoisTypeCache()
		{
			_computedCache = new BoisComputedTypeHashtable<BoisComputedTypeInfo>();
			_basicTypeCache = new BoisComputedTypeHashtable<BoisBasicTypeInfo>();
			_basicEnumCache = new BoisComputedTypeHashtable<BoisBasicEnumTypeInfo>();
		}


		internal static void ClearCache()
		{
			lock (_basicTypeCache)
				_basicTypeCache.Clear();
			lock (_computedCache)
				_computedCache.Clear();
			lock (_computedCachesByBuffer)
			{
				foreach (var cache in _computedCachesByBuffer.Values)
					cache.Clear();
				_computedCachesByBuffer.Clear();
			}
			lock (_basicEnumCache)
				_basicEnumCache.Clear();
			_versionedGraphs.Clear();
		}

		private static BoisComputedTypeHashtable<BoisComputedTypeInfo> GetComputedCache(Type bufferType)
		{
			lock (_computedCachesByBuffer)
			{
				if (!_computedCachesByBuffer.TryGetValue(bufferType, out var cache))
				{
					cache = new BoisComputedTypeHashtable<BoisComputedTypeInfo>();
					_computedCachesByBuffer.Add(bufferType, cache);
				}
				return cache;
			}
		}

		internal static BoisComputedTypeInfo GetRootTypeComputed(
			Type type,
			bool generateReader,
			bool generateWriter,
			Type bufferType = null
#if EmitAssemblyOut
			, 
			bool outputAssembly = true
#endif
			)
		{
			if (generateReader && generateWriter && bufferType == null)
			{
				// each direction has its own cache partition and buffer type
				GetRootTypeComputed(type, false, true);
				return GetRootTypeComputed(type, true, false);
			}

			bufferType ??= generateWriter ? typeof(BufferWriterBase) : typeof(BufferReaderBase);
			var _computedCache = GetComputedCache(bufferType);
			lock (_computedCache)
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
					if (complexTypeInfo == null)
						return result;
#if EmitAssemblyOut && !NETCOREAPP
					var computed = BoisTypeCompiler.ComputeWriterSaveAss(type, complexTypeInfo, outputAssembly,
						(dynamicMethod) =>
						{
							// this call is useful for Recursive Methods

							result.WriterMethod = dynamicMethod;
						});
#else
					var computed = BoisTypeCompiler.ComputeWriter(type, complexTypeInfo,
						(dynamicMethod) =>
						{
							// this call is useful for Recursive Methods

							result.WriterMethod = dynamicMethod;
						}, writerType: bufferType);
#endif
					result.WriterDelegate = computed.Delegate;
					result.WriterMethod = computed.Method;
				}

				if (generateReader && result.ReaderDelegate == null)
				{
					if (complexTypeInfo == null)
					{
						complexTypeInfo = GetComplexTypeUnCached(type);
						if (complexTypeInfo == null)
							return result;
					}

#if EmitAssemblyOut && !NETCOREAPP
					var computed = BoisTypeCompiler.ComputeReaderSaveAss(type, complexTypeInfo, outputAssembly,
						(dynamicMethod) =>
						{
							// this call is useful for Recursive Methods

							result.ReaderMethod = dynamicMethod;
						});
#else
					var computed = BoisTypeCompiler.ComputeReader(type, complexTypeInfo,
						(dynamicMethod) =>
						{
							// this call is useful for Recursive Methods

							result.ReaderMethod = dynamicMethod;
						}, readerType: bufferType);
#endif
					result.ReaderDelegate = computed.Delegate;
					result.ReaderMethod = computed.Method;
				}

				return result;
			}
		}

		internal static BoisBasicTypeInfo GetBasicType(Type type)
		{
			lock (_basicTypeCache)
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
		}

		internal static BoisBasicEnumTypeInfo GetEnumType(Type type)
		{
			lock (_basicEnumCache)
			{
				BoisBasicEnumTypeInfo result;
				if (_basicEnumCache.TryGetValue(type, out result))
				{
					return result;
				}

				result = ReadBasicEnumTypeInfo(type);
				_basicEnumCache.TryAdd(type, result);

				return result;
			}
		}

		/// <summary>
		/// Read on every call of the non-generic stream overloads, so reads take no lock.
		/// </summary>
		private static readonly ConcurrentDictionary<Type, bool> _versionedGraphs = new ConcurrentDictionary<Type, bool>();

		/// <summary>
		/// True when <paramref name="rootType"/> or any type it contains is versioned, so it needs a writer that can
		/// fill in object sizes and a reader that knows its position. Computed once per type.
		/// </summary>
		internal static bool ContainsVersioned(Type rootType)
		{
			if (_versionedGraphs.TryGetValue(rootType, out var cached))
				return cached;

			return ComputeContainsVersioned(rootType);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		private static bool ComputeContainsVersioned(Type rootType)
		{
			bool result;
			try
			{
				result = ContainsVersioned(rootType, new HashSet<Type>());
			}
			catch (Exception)
			{
				// an invalid type fails with its real error when its serializer is generated
				result = false;
			}

			_versionedGraphs[rootType] = result;
			return result;
		}

		private static bool ContainsVersioned(Type type, HashSet<Type> visited)
		{
			if (type == null || !visited.Add(type))
				return false;

			var nullableOf = Nullable.GetUnderlyingType(type);
			if (nullableOf != null)
				return ContainsVersioned(nullableOf, visited);

			if (type.IsArray)
				return ContainsVersioned(type.GetElementType(), visited);

			if (GetBasicType(type).KnownType != EnBasicKnownType.Unknown)
				return false;

			var complex = GetComplexTypeUnCached(type);
			switch (complex?.ComplexKnownType)
			{
				case EnComplexKnownType.Collection:
				case EnComplexKnownType.ISet:
					return ContainsVersioned(ReflectionHelper.FindUnderlyingGenericElementType(type), visited);

				case EnComplexKnownType.Dictionary:
				{
					var keyValue = ReflectionHelper.FindUnderlyingGenericDictionaryElementType(type);
					return keyValue != null && (ContainsVersioned(keyValue[0], visited) || ContainsVersioned(keyValue[1], visited));
				}

				case EnComplexKnownType.Unknown:
					if (complex.VersionedSlots != null)
						return true;
					if (complex.Members == null)
						return false;
					foreach (var member in complex.Members)
					{
						var memberType = member is PropertyInfo property ? property.PropertyType : ((FieldInfo)member).FieldType;
						if (ContainsVersioned(memberType, visited))
							return true;
					}
					return false;

				default:
					return false;
			}
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

		/// <remarks>https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types</remarks>
		private static BoisBasicEnumTypeInfo ReadBasicEnumTypeInfo(Type memType)
		{
			Type memActualType = memType;
			Type bareType;
			bool isNullable = ReflectionHelper.IsNullable(memType, out bareType);

			// check the underling type
			if (isNullable && bareType != null)
			{
				memActualType = bareType;
			}
			else
			{
				bareType = memType;
			}
			memActualType = Enum.GetUnderlyingType(memActualType);

			if (memActualType.IsClass)
			{
				return null;
			}
			if (memActualType == typeof(int))
			{
				return new BoisBasicEnumTypeInfo
				{
					KnownType = EnBasicEnumType.Int32,
					UnderlyingType = typeof(int),
					IsNullable = isNullable,
					BareType = bareType
				};
			}
			else if (memActualType == typeof(long))
			{
				return new BoisBasicEnumTypeInfo
				{
					KnownType = EnBasicEnumType.Int64,
					UnderlyingType = typeof(long),
					IsNullable = isNullable,
					BareType = bareType
				};
			}
			else if (memActualType == typeof(short))
			{
				return new BoisBasicEnumTypeInfo
				{
					KnownType = EnBasicEnumType.Int16,
					UnderlyingType = typeof(short),
					IsNullable = isNullable,
					BareType = bareType
				};
			}
			else if (memActualType == typeof(byte))
			{
				return new BoisBasicEnumTypeInfo
				{
					KnownType = EnBasicEnumType.Byte,
					UnderlyingType = typeof(byte),
					IsNullable = isNullable,
					BareType = bareType
				};
			}
			else if (memActualType == typeof(sbyte))
			{
				return new BoisBasicEnumTypeInfo
				{
					KnownType = EnBasicEnumType.SByte,
					UnderlyingType = typeof(sbyte),
					IsNullable = isNullable,
					BareType = bareType
				};
			}
			else if (memActualType == typeof(ushort))
			{
				return new BoisBasicEnumTypeInfo
				{
					KnownType = EnBasicEnumType.UInt16,
					UnderlyingType = typeof(ushort),
					IsNullable = isNullable,
					BareType = bareType
				};
			}
			else if (memActualType == typeof(uint))
			{
				return new BoisBasicEnumTypeInfo
				{
					KnownType = EnBasicEnumType.UInt32,
					UnderlyingType = typeof(uint),
					IsNullable = isNullable,
					BareType = bareType
				};
			}
			else if (memActualType == typeof(ulong))
			{
				return new BoisBasicEnumTypeInfo
				{
					KnownType = EnBasicEnumType.UInt64,
					UnderlyingType = typeof(ulong),
					IsNullable = isNullable,
					BareType = bareType
				};
			}

			return null;
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
					throw new InvalidDataException($"Unknown array '{memType}' with no element type not suported.");

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

#if NET6_0_OR_GREATER
			if (memActualType == typeof(DateOnly))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.DateOnly,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(TimeOnly))
			{
				return new BoisBasicTypeInfo
				{
					KnownType = EnBasicKnownType.TimeOnly,
					IsNullable = isNullable,
					AsRootNeedsCompute = false,
					BareType = underlyingTypeNullable,
				};
			}
#endif

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

		internal static BoisComplexTypeInfo ReadComplexTypeMembers(Type type)
		{
			bool readFields = true, readProps = true, versioned = false;

			var objectAttr = type.GetCustomAttributes(typeof(BoisContractAttribute), false);
			if (objectAttr.Length > 0)
			{
				var boisContract = objectAttr[0] as BoisContractAttribute;
				if (boisContract != null)
				{
					readFields = boisContract.Fields;
					readProps = boisContract.Properties;
					versioned = boisContract.Versioned;
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

			if (versioned)
				return ReadVersionedTypeMembers(type, typeInfo, readFields, readProps);

			if (type.IsDefined(typeof(BoisReservedAttribute), false))
				throw new InvalidTypeException(VersionedContractRules.ReservedOnNonVersionedType(BoisContractInfo.TypeName(type)));

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

					if (index > -1)
					{
						members.Insert(Math.Min(index, members.Count), f);
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

						if (index > -1)
						{
							members.Insert(Math.Min(index, members.Count), p);
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

		/// <summary>
		/// Reads the slots of a versioned type and checks its rules. Throws one <see cref="InvalidTypeException"/>
		/// listing every broken rule.
		/// </summary>
		private static BoisComplexTypeInfo ReadVersionedTypeMembers(Type type, BoisComplexTypeInfo typeInfo, bool readFields, bool readProps)
		{
			var members = new List<VersionedMember>();

			if (readFields)
			{
				foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
				{
					ReadMemberOptions(field, out var excluded, out var slot);
					members.Add(new VersionedMember(field.Name, BoisContractInfo.TypeName(field.FieldType), slot, excluded,
						canWrite: !field.IsInitOnly, field));
				}
			}

			if (readProps)
			{
				foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					if (property.GetIndexParameters().Length > 0 || property.GetGetMethod() == null)
						continue;

					ReadMemberOptions(property, out var excluded, out var slot);
					var setter = property.GetSetMethod();
					members.Add(new VersionedMember(property.Name, BoisContractInfo.TypeName(property.PropertyType), slot, excluded,
						canWrite: setter != null && !IsInitOnly(setter), property));
				}
			}

			var reservations = new List<VersionedReservation>();
			for (var current = type; current != null && current != typeof(object); current = current.BaseType)
			{
				foreach (BoisReservedAttribute reserved in current.GetCustomAttributes(typeof(BoisReservedAttribute), false))
				{
					reservations.Add(new VersionedReservation(reserved.Index,
						reserved.Type == null ? null : BoisContractInfo.TypeName(reserved.Type),
						IsValidReservedType(reserved.Type), reserved));
				}
			}

			var typeName = BoisContractInfo.TypeName(type);
			var problems = new List<VersionedProblem>();
			var slots = VersionedContractRules.Build(typeName, members, reservations, problems);
			if (problems.Count > 0)
				throw new InvalidTypeException(VersionedContractRules.InvalidContractMessage(typeName, problems));

			var slotMembers = new List<MemberInfo>(slots.Length);
			foreach (var slot in slots)
			{
				if (slot.Member != null)
					slotMembers.Add((MemberInfo)slot.Member.Source);
			}

			typeInfo.Members = slotMembers.ToArray();
			typeInfo.VersionedSlots = slots;
			return typeInfo;
		}

		private static void ReadMemberOptions(MemberInfo member, out bool excluded, out int slot)
		{
			var attribute = (BoisMemberAttribute)Attribute.GetCustomAttribute(member, typeof(BoisMemberAttribute), false);
			excluded = attribute != null && !attribute.Included;
			slot = attribute?.Index ?? -1;
		}

		private static bool IsInitOnly(MethodInfo setter)
		{
			foreach (var modifier in setter.ReturnParameter.GetRequiredCustomModifiers())
			{
				if (modifier.FullName == "System.Runtime.CompilerServices.IsExternalInit")
					return true;
			}
			return false;
		}

		private static bool IsValidReservedType(Type type)
		{
			return type != null &&
				   type != typeof(void) &&
				   type != typeof(object) &&
				   !type.IsPointer &&
				   !type.IsByRef &&
				   !type.ContainsGenericParameters;
		}
	}
}
