using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if SILVERLIGHT
using System.Reflection.Emit;
#endif
#if DotNet || DotNetCore || DotNetStandard
using System.Reflection.Emit;
using System.Collections.Specialized;
using System.Drawing;
#endif
#if !SILVERLIGHT && DotNet
using System.Data;
#endif

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois
{
	/// <summary>
	/// Cached information about types, for internal use.
	/// </summary>
	class BoisTypeCache
	{
		internal delegate void GenericSetter(object target, object value);
		internal delegate object GenericGetter(object target);
		internal delegate object GenericConstructor();


#if SILVERLIGHT || (!DotNet && !DotNetStandard)
		private readonly Dictionary<Type, GenericConstructor> _constructorCache;
		private readonly Dictionary<Type, BoisMemberInfo> _cache;
		public BoisTypeCache()
		{
			_cache = new Dictionary<Type, BoisMemberInfo>();
			_constructorCache = new Dictionary<Type, GenericConstructor>();
		}
#else
		private readonly Hashtable _constructorCache = new Hashtable();
		private readonly Hashtable _cache;
		public BoisTypeCache()
		{
			_cache = new Hashtable();
			_constructorCache = new Hashtable();
		}
#endif

		/// <summary>
		/// Removes all cached information about types.
		/// </summary>
		public void ClearCache()
		{
			lock (_cache)
			{
				_cache.Clear();
			}
		}
		/// <summary>
		/// Removes a cached entry.
		/// </summary>
		/// <param name="type">The object type.</param>
		public void RemoveEntry(Type type)
		{
			lock (_cache)
			{
				_cache.Remove(type);
			}
		}

		/// <summary>
		/// Reads type information and caches it.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		public void Initialize<T>()
		{
			var type = typeof(T);
			InitializeInternal(type);
		}

		/// <summary>
		/// Reads type information and caches it.
		/// </summary>
		/// <param name="types">The objects types.</param>
		public void Initialize(params Type[] types)
		{
			foreach (var t in types)
			{
				InitializeInternal(t);
			}
		}

		internal object CreateInstance(Type t)
		{
#if SILVERLIGHT || (!DotNet && !DotNetStandard)
			// Falling back to default parameterless constructor.
			return Activator.CreateInstance(t, null);
#else
			// Read from cache
#if SILVERLIGHT || (!DotNet && !DotNetStandard)
			GenericConstructor info = null;
			_constructorCache.TryGetValue(t, out info);
#else
			var info = _constructorCache[t] as GenericConstructor;
#endif
			if (info == null)
			{
				ConstructorInfo ctor = t.GetConstructor(Type.EmptyTypes);
				if (ctor == null)
				{
					// Falling back to default parameterless constructor.
					return Activator.CreateInstance(t, null);
				}

#if SILVERLIGHT || (!DotNet && !DotNetStandard)
				var dynamicCtor = new DynamicMethod("_", t, Type.EmptyTypes);
#else
				var dynamicCtor = new DynamicMethod("_", t, Type.EmptyTypes, t, true);
#endif

				var il = dynamicCtor.GetILGenerator();

				il.Emit(OpCodes.Newobj, ctor);
				il.Emit(OpCodes.Ret);

				info = (GenericConstructor)dynamicCtor.CreateDelegate(typeof(GenericConstructor));

				_constructorCache[t] = info;
			}
			if (info == null)
				throw new MissingMethodException(string.Format("No parameterless constructor defined for '{0}'.", t));
			return info.Invoke();
#endif
		}
		internal object CreateInstanceDirect(Type t)
		{
			// Falling back to default parameterless constructor.
			return Activator.CreateInstance(t, null);
		}

		internal BoisMemberInfo GetTypeInfo(Type type, bool generate)
		{
#if SILVERLIGHT || (!DotNet && !DotNetStandard)
			BoisMemberInfo memInfo;
			if (_cache.TryGetValue(type, out memInfo))
			{
				return memInfo;
			}
#else
			var memInfo = _cache[type] as BoisMemberInfo;
			if (memInfo != null)
			{
				return memInfo;
			}
#endif

			if (generate)
			{
				memInfo = ReadMemberInfo(type);
				CacheInsert(type, memInfo);
			}
			return memInfo;
		}

		private void InitializeInternal(Type type)
		{
			if (!_cache.ContainsKey(type))
			{
				var info = ReadMemberInfo(type);
				CacheInsert(type, info);
			}
		}
		private void CacheInsert(Type type, BoisMemberInfo memInfo)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(type))
				{
					_cache.Add(type, memInfo);
				}
			}
		}

		private BoisMemberInfo ReadObject(Type type)
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
			var typeInfo = new BoisTypeInfo
			{
				MemberType = EnBoisMemberType.Object,
				KnownType = EnBoisKnownType.Unknown,
				IsContainerObject = true,
				IsStruct = type.IsValueType
			};

			var members = new List<BoisMemberInfo>();

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

						var info = ReadMemberInfo(p.PropertyType);
						info.PropertyGetter = GetPropertyGetter(type, p);
						//info.PropertySetter = CreateSetMethod(p);
						info.PropertySetter = GetPropertySetter(type, p);
						info.Info = p;
						info.MemberType = EnBoisMemberType.Property;

						if (index > -1)
						{
							members.Insert(index, info);
						}
						else
						{
							members.Add(info);
						}
					}
				}
			}

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

					var info = ReadMemberInfo(f.FieldType);
					info.Info = f;
					info.MemberType = EnBoisMemberType.Field;
					if (index > -1)
					{
						members.Insert(index, info);
					}
					else
					{
						members.Add(info);
					}
				}
			}

			typeInfo.Members = members.ToArray();

			CacheInsert(type, typeInfo);
			return typeInfo;
		}

		private BoisMemberInfo ReadMemberInfo(Type memType)
		{
			if (memType == typeof(string))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.String,
					IsNullable = true,
					IsSupportedPrimitive = true,
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


			if (memActualType == typeof(char))
			{
				// is struct and uses Nullable<>
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Char,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(bool))
			{
				// is struct and uses Nullable<>
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Bool,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(DateTime))
			{
				// is struct and uses Nullable<>
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.DateTime,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(DateTimeOffset))
			{
				// is struct and uses Nullable<>
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.DateTimeOffset,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(byte[]))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.ByteArray,
					IsNullable = isNullable,
					IsArray = true,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Enum)))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Enum,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Unknown,
					IsNullable = isNullable,
					IsArray = true,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}

			var isGenericType = memActualType.IsGenericType;
			Type[] interfaces = null;
			if (isGenericType)
			{
				//// no more checking for a dictionary with its first argumnet as String
				//if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)) &&
				//	memActualType.GetGenericArguments()[0] == typeof(string))
				//	return new BoisMemberInfo
				//	{
				//		KnownType = EnBoisKnownType.Unknown,
				//		IsNullable = isNullable,
				//		IsDictionary = true,
				//		IsStringDictionary = true,
				//		IsGeneric = true,
				//		NullableUnderlyingType = underlyingTypeNullable,
				//	};

				interfaces = memActualType.GetInterfaces();

				if (ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(IDictionary<,>)) ||
					memActualType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
					return new BoisMemberInfo
					{
						KnownType = EnBoisKnownType.Unknown,
						IsNullable = isNullable,
						IsDictionary = true,
						IsGeneric = true,
						NullableUnderlyingType = underlyingTypeNullable,
					};

#if DotNet4_NotYET
				if (ReflectionHelper.CompareInterface(memType, typeof(ISet<>)))
					return new BoisMemberInfo
							   {
								   KnownType = EnBoisKnownType.Unknown,
								   IsNullable = isNullable,
								   IsGeneric = true,
								   IsSet = true,
					NullableUnderlyingType = underlyingTypeNullable,
							   };
#endif
			}


			if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Unknown,
					IsNullable = isNullable,
					IsDictionary = true,
					IsGeneric = true,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			// the IDictionary should be checked before IList<>
			if (isGenericType)
			{
				if (ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(IList<>)) ||
					ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(ICollection<>)))
					return new BoisMemberInfo
					{
						KnownType = EnBoisKnownType.Unknown,
						IsNullable = isNullable,
						IsGeneric = true,
						IsCollection = true,
						IsArray = true,
						NullableUnderlyingType = underlyingTypeNullable,
					};
			}

			// checking for IList and ICollection should be after NameValueCollection
			if (ReflectionHelper.CompareInterface(memActualType, typeof(IList)) ||
				ReflectionHelper.CompareInterface(memActualType, typeof(ICollection)))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Unknown,
					IsNullable = isNullable,
					IsGeneric = memActualType.IsGenericType,
					IsCollection = true,
					IsArray = true,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}

#if DotNet || DotNetCore || DotNetStandard
			if (ReflectionHelper.CompareSubType(memActualType, typeof(NameValueCollection)))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.NameValueColl,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(Color))
			{
				// is struct and uses Nullable<>
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Color,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
#endif
#if !SILVERLIGHT && DotNet
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataSet)))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.DataSet,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataTable)))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.DataTable,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}

#endif

			if (memActualType == typeof(TimeSpan))
			{
				// is struct and uses Nullable<>
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.TimeSpan,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}

			if (memActualType == typeof(Version))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Version,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}

			BoisMemberInfo output;
			if (TryReadNumber(memActualType, out output))
			{
				output.IsNullable = isNullable;
				output.NullableUnderlyingType = underlyingTypeNullable;
				return output;
			}
			if (memActualType == typeof(Guid))
			{
				// is struct and uses Nullable<>
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Guid,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
#if DotNet || DotNetCore || DotNetStandard
			if (memActualType == typeof(DBNull))
			{
				// ignore!
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.DbNull,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
#endif

			var objectMemInfo = ReadObject(memType);
			objectMemInfo.NullableUnderlyingType = underlyingTypeNullable;
			objectMemInfo.IsNullable = isNullable;

			return objectMemInfo;
		}

		static Type UnNullify(Type type)
		{
			return Nullable.GetUnderlyingType(type) ?? type;
		}
		/// <summary>
		/// Slower convertion
		/// </summary>
		private bool IsNumber(Type memType, out BoisMemberInfo output)
		{
			if (memType.IsClass)
			{
				output = null;
				return false;
			}
			output = null;
			switch (Type.GetTypeCode(UnNullify(memType)))
			{
				case TypeCode.Int16:
					output = new BoisMemberInfo
					{
						KnownType = EnBoisKnownType.Int16,
						IsSupportedPrimitive = true,
					};
					break;

				case TypeCode.Int32:
					output = new BoisMemberInfo
					{
						KnownType = EnBoisKnownType.Int32,
						IsSupportedPrimitive = true,
					};
					break;

				case TypeCode.Int64:
					output = new BoisMemberInfo
					{
						KnownType = EnBoisKnownType.Int64,
						IsSupportedPrimitive = true,
					};
					break;
				case TypeCode.Single:
					output = new BoisMemberInfo { KnownType = EnBoisKnownType.Single };
					break;
				case TypeCode.Double:
					output = new BoisMemberInfo { KnownType = EnBoisKnownType.Double };
					break;
				case TypeCode.Decimal:
					output = new BoisMemberInfo { KnownType = EnBoisKnownType.Decimal };
					break;

				case TypeCode.Byte:
					output = new BoisMemberInfo { KnownType = EnBoisKnownType.Byte };
					break;
				case TypeCode.SByte:
					output = new BoisMemberInfo { KnownType = EnBoisKnownType.SByte };
					break;

				case TypeCode.UInt16:
					output = new BoisMemberInfo { KnownType = EnBoisKnownType.UInt16 };
					break;
				case TypeCode.UInt32:
					output = new BoisMemberInfo { KnownType = EnBoisKnownType.UInt32 };
					break;
				case TypeCode.UInt64:
					output = new BoisMemberInfo { KnownType = EnBoisKnownType.UInt64 };
					break;
			}
			return output != null;
		}

		private bool TryReadNumber(Type memType, out BoisMemberInfo output)
		{
			if (memType.IsClass)
			{
				output = null;
				return false;
			}
			if (memType == typeof(int))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Int32,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(long))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Int64,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(short))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Int16,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(double))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Double,
				};
			}
			else if (memType == typeof(decimal))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Decimal,
				};
			}
			else if (memType == typeof(float))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Single,
				};
			}
			else if (memType == typeof(byte))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Byte,
				};
			}
			else if (memType == typeof(sbyte))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.SByte,
				};
			}
			else if (memType == typeof(ushort))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.UInt16,
				};
			}
			else if (memType == typeof(uint))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.UInt32,
				};
			}
			else if (memType == typeof(ulong))
			{
				output = new BoisMemberInfo
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
		private bool TryReadNumber__Nullable(Type memType, out BoisMemberInfo output)
		{
			if (memType.IsClass)
			{
				output = null;
				return false;
			}
			if (memType == typeof(int) || memType == typeof(int?))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Int32,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(long) || memType == typeof(long?))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Int64,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(short) || memType == typeof(short?))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Int16,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(double) || memType == typeof(double?))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Double,
				};
			}
			else if (memType == typeof(decimal) || memType == typeof(decimal?))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Decimal,
				};
			}
			else if (memType == typeof(float) || memType == typeof(float?))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Single,
				};
			}
			else if (memType == typeof(byte) || memType == typeof(byte?))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Byte,
				};
			}
			else if (memType == typeof(sbyte) || memType == typeof(sbyte?))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.SByte,
				};
			}
			else if (memType == typeof(ushort) || memType == typeof(ushort?))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.UInt16,
				};
			}
			else if (memType == typeof(uint) || memType == typeof(uint?))
			{
				output = new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.UInt32,
				};
			}
			else if (memType == typeof(ulong) || memType == typeof(ulong?))
			{
				output = new BoisMemberInfo
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

		/// <summary>
		///  Creates a dynamic setter for the property
		/// </summary>
		/// <author>
		/// Gerhard Stephan 
		/// http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/
		/// </author>
#if DotNet || DotNetStandard
		private GenericSetter CreateSetMethod(PropertyInfo propertyInfo)
		{
			/*
			* If there's no setter return null
			*/
			MethodInfo setMethod = propertyInfo.GetSetMethod();
			if (setMethod == null)
				return null;

			/*
			* Create the dynamic method
			*/
			var arguments = new Type[2];
			arguments[0] = arguments[1] = typeof(object);


#if SILVERLIGHT || (!DotNet && !DotNetStandard)
			var setter = new DynamicMethod(
			  String.Concat("_Set", propertyInfo.Name, "_"),
			  typeof(void), arguments);
#else
			var setter = new DynamicMethod(
			  String.Concat("_Set", propertyInfo.Name, "_"),
			  typeof(void), arguments, propertyInfo.DeclaringType);
#endif


			ILGenerator generator = setter.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
			generator.Emit(OpCodes.Ldarg_1);

			if (propertyInfo.PropertyType.IsClass)
				generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
			else
				generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);

			generator.EmitCall(OpCodes.Callvirt, setMethod, null);
			generator.Emit(OpCodes.Ret);

			/*
			* Create the delegate and return it
			*/
			return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
		}
#endif

		/// <summary>
		/// Creates a dynamic getter for the property
		/// </summary>
		/// <author>
		/// Gerhard Stephan 
		/// http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/
		/// </author>
#if DotNet || DotNetStandard
		private GenericGetter CreateGetMethod(PropertyInfo propertyInfo)
		{
			/*
			* If there's no getter return null
			*/
			MethodInfo getMethod = propertyInfo.GetGetMethod();
			if (getMethod == null)
				return null;

			/*
			* Create the dynamic method
			*/
			var arguments = new Type[1];
			arguments[0] = typeof(object);

#if SILVERLIGHT
			var getter = new DynamicMethod(
			  String.Concat("_Get", propertyInfo.Name, "_"),
			  typeof(object), arguments);
#else
			var getter = new DynamicMethod(
			  String.Concat("_Get", propertyInfo.Name, "_"),
			  typeof(object), arguments, propertyInfo.DeclaringType);
#endif

			ILGenerator generator = getter.GetILGenerator();
			generator.DeclareLocal(typeof(object));
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
			generator.EmitCall(OpCodes.Callvirt, getMethod, null);

			if (!propertyInfo.PropertyType.IsClass)
				generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

			generator.Emit(OpCodes.Ret);

			/*
			* Create the delegate and return it
			*/
			return (GenericGetter)getter.CreateDelegate(typeof(GenericGetter));
		}
#endif

		//private Func<T, object> MakeDelegate_2<T, U>(MethodInfo @get)
		//{
		//	var f = (Func<T, U>)Delegate.CreateDelegate(typeof(Func<T, U>), @get);
		//	return t => f(t);
		//}

		private GenericGetter MakeDelegate(MethodInfo @get)
		{
			var f = (GenericGetter)Delegate.CreateDelegate(typeof(GenericGetter), @get);
			return t => f(t);
		}

		private GenericGetter GetPropertyGetter(Type objType, PropertyInfo propertyInfo)
		{
#if SILVERLIGHT || (!DotNet && !DotNetStandard)
			// this is a fallback to slower method.
			var method = propertyInfo.GetGetMethod();

			// generating the caller.
			return new GenericGetter(target => method.Invoke(target, null));
#else
			if (objType.IsInterface)
			{
				throw new Exception("Type is an interface or abstract class and cannot be instantiated.");
			}
			if (objType.IsValueType &&
				!objType.IsPrimitive &&
				!objType.IsArray &&
				objType != typeof(string))
			{
				// this is a fallback to slower method.
				var method = propertyInfo.GetGetMethod(true);

				// generating the caller.
				return new GenericGetter(target => method.Invoke(target, null));
			}
			else
			{
				//var method = objType.GetMethod("get_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
				var method = propertyInfo.GetGetMethod(true);
				return GetFastGetterFunc(propertyInfo, method);
			}
#endif
		}

		private Function<object, object, object> GetPropertySetter(Type objType, PropertyInfo propertyInfo)
		{
#if SILVERLIGHT || (!DotNet && !DotNetStandard)
			// this is a fallback to slower method.
			var method = propertyInfo.GetSetMethod(true);

			// generating the caller.
			return new Function<object, object, object>((target, value) => method.Invoke(target, new object[] { value }));
#else
			if (objType.IsValueType &&
				!objType.IsPrimitive &&
				!objType.IsArray &&
				objType != typeof(string))
			{
				// this is a fallback to slower method.
				var method = propertyInfo.GetSetMethod(true);

				// generating the caller.
				return new Function<object, object, object>((target, value) => method.Invoke(target, new object[] { value }));
			}
			else
			{
				//var method = objType.GetMethod("set_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
				var method = propertyInfo.GetSetMethod(true);
				return GetFastSetterFunc(propertyInfo, method);
			}
#endif

			////var method = objType.GetMethod("set_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
			//var method = propertyInfo.GetSetMethod();
			//return GetFastSetterFunc(propertyInfo, method);
		}

		/// <summary>
		/// http://social.msdn.microsoft.com/Forums/en-US/netfxbcl/thread/8754500e-4426-400f-9210-554f9f2ad58b/
		/// </summary>
		/// <returns></returns>
#if SILVERLIGHT || DotNet || DotNetStandard
		private GenericGetter GetFastGetterFunc(PropertyInfo p, MethodInfo getter) // untyped cast from Func<T> to Func<object> 
		{
#if SILVERLIGHT
			var g = new DynamicMethod("_", p.DeclaringType, new[] { typeof(object) });
#else
			var g = new DynamicMethod("_", typeof(object), new[] { typeof(object) }, p.DeclaringType, true);
#endif
			var il = g.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);//load the delegate from function parameter
			il.Emit(OpCodes.Castclass, p.DeclaringType);//cast
			il.Emit(OpCodes.Callvirt, getter);//calls it's get method

			if (p.PropertyType.IsValueType)
				il.Emit(OpCodes.Box, p.PropertyType);//box

			il.Emit(OpCodes.Ret);

			//return (bool)((xViewModel)param1).get_IsEnabled();

			var _func = (GenericGetter)g.CreateDelegate(typeof(GenericGetter));
			return _func;
		}
#endif

		/// <summary>
		/// http://social.msdn.microsoft.com/Forums/en-US/netfxbcl/thread/8754500e-4426-400f-9210-554f9f2ad58b/
		/// </summary>
#if SILVERLIGHT || DotNet || DotNetStandard
		private Function<object, object, object> GetFastSetterFunc(PropertyInfo p, MethodInfo setter)
		{
#if SILVERLIGHT
			var s = new DynamicMethod("_", p.DeclaringType, new[] { typeof(object), typeof(object) });
#else
			var s = new DynamicMethod("_", typeof(object), new[] { typeof(object), typeof(object) }, p.DeclaringType, true);
#endif
			var il = s.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Castclass, p.DeclaringType);

			il.Emit(OpCodes.Ldarg_1);
			if (p.PropertyType.IsClass)
			{
				il.Emit(OpCodes.Castclass, p.PropertyType);
			}
			else
			{
				il.Emit(OpCodes.Unbox_Any, p.PropertyType);
			}


			il.EmitCall(OpCodes.Callvirt, setter, null);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ret);

			//(xViewModel)param1.set_IsEnabled((bool)param2)
			// return param1;

			var _func = (Function<object, object, object>)s.CreateDelegate(typeof(Function<object, object, object>));
			return _func;
		}
#endif

		private GenericGetter GetPropertyGetter_(Type objType, PropertyInfo propertyInfo)
		{
			var method = objType.GetMethod("get_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(typeof(GenericGetter), method);
			return (GenericGetter)dlg;
		}

		private GenericGetter GetPropertyGetter(object obj, string propertyName)
		{
			var t = obj.GetType();
			var method = t.GetMethod("get_" + propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(t, obj, method);
			return (GenericGetter)dlg;
		}

		private GenericSetter GetPropertySetter(object obj, string propertyName)
		{
			var t = obj.GetType();
			var method = t.GetMethod("set_" + propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(t, obj, method);
			return (GenericSetter)dlg;
		}


		//ParameterExpression arg = Expression.Parameter(typeof(Person));
		//Expression expr = Expression.Property(arg, propertyName);
		//Expression<Func<Person, string>> get = Expression.Lambda<Func<Person, string>>(expr, arg);
		//Getter = get.Compile();


		//var member = get.Body;
		//		var param = Expression.Parameter(typeof(string), "value");
		//		Setter =
		//Expression.Lambda<Action<Person, string>>(Expression.Assign(member, param), get.Parameters[0], param)
		//.Compile();

	}
}
