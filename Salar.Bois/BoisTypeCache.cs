using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
#if !SILVERLIGHT
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
#endif

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://bois.codeplex.com/
 * Mozilla Public License v2
 */
namespace Salar.Bois
{
	/// <summary>
	/// Cached information about types, for internal use.
	/// </summary>
	public static class BoisTypeCache
	{
		internal delegate void GenericSetter(object target, object value);
		internal delegate object GenericGetter(object target);

		internal enum EnBoisMemberType
		{
			Object,
			Property,
			Field
		}
		internal enum EnBoisKnownType
		{
			Unknown = 0,
			Int16,
			Int32,
			Int64,
			UInt16,
			UInt32,
			UInt64,
			Double,
			Decimal,
			Single,
			Byte,
			SByte,
			ByteArray,
			String,
			Char,
			Guid,
			Bool,
			Enum,
			DateTime,
			TimeSpan,
			DataSet,
			DataTable,
			NameValueColl,
			Color,
			Version,
			DbNull,
		}
		internal class BoisMemberInfo
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
			public GenericGetter PropertyGetter;
			public override string ToString()
			{
				return string.Format("{0}: {1}: {2}", MemberType, KnownType, Info);
			}
		}

		internal class BoisTypeInfo : BoisMemberInfo
		{
			public BoisMemberInfo[] Members;
			public override string ToString()
			{
				return string.Format("{0}: {1}: {2}: Members= {3}", MemberType, KnownType, Info,
									 (Members != null) ? (Members.Length) : 0);
			}
		}
#if SILVERLIGHT
		private static Dictionary<Type, BoisMemberInfo> _cache;
		static BoisTypeCache()
		{
			_cache = new Dictionary<Type, BoisMemberInfo>();
		}
#else
		private static Hashtable _cache;
		static BoisTypeCache()
		{
			_cache = new Hashtable();
		}
#endif

		/// <summary>
		/// Removes all cached information about types.
		/// </summary>
		[Obsolete("Planned to be removed in next releases.", false)]
		public static void ClearCache()
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
		[Obsolete("Planned to be removed in next releases.", false)]
		public static void RemoveEntry(Type type)
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
		[Obsolete("Planned to be removed in next releases.", false)]
		public static void Initialize<T>()
		{
			var type = typeof(T);
			InitializeInternal(type);
		}

		/// <summary>
		/// Reads type information and caches it.
		/// </summary>
		/// <param name="types">The objects types.</param>
		[Obsolete("Planned to be removed in next releases.", false)]
		public static void Initialize(params Type[] types)
		{
			foreach (var t in types)
			{
				InitializeInternal(t);
			}
		}

		internal static BoisMemberInfo GetTypeInfo(Type type, bool generate)
		{
#if SILVERLIGHT
			BoisMemberInfo memInfo;
			if (_cache.TryGetValue(type, out memInfo))
			{
				return memInfo;
			}
#else
			var memInfo = _cache[type] as BoisMemberInfo;
#endif

			if (memInfo != null)
			{
				return memInfo;
			}
			if (generate)
			{
				memInfo = ReadMemberInfo(type);
				CacheInsert(type, memInfo);
			}
			return memInfo;
		}

		private static void InitializeInternal(Type type)
		{
			if (!_cache.ContainsKey(type))
			{
				var info = ReadMemberInfo(type);
				CacheInsert(type, info);
			}
		}
		private static void CacheInsert(Type type, BoisMemberInfo memInfo)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(type))
				{
					_cache.Add(type, memInfo);
				}
			}
		}

		private static BoisMemberInfo ReadObject(Type type)
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
								   IsNullable = true,
								   IsContainerObject = true
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

		private static BoisMemberInfo ReadMemberInfo(Type memType)
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


			if (memActualType == typeof(char) || memActualType == typeof(char?))
			{
				return new BoisMemberInfo
				{
					KnownType = EnBoisKnownType.Char,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(bool) || memActualType == typeof(bool?))
			{
				return new BoisMemberInfo
						   {
							   KnownType = EnBoisKnownType.Bool,
							   IsNullable = isNullable,
							   NullableUnderlyingType = underlyingTypeNullable,
						   };
			}
			if (memActualType == typeof(DateTime) || memActualType == typeof(DateTime?))
			{
				return new BoisMemberInfo
						   {
							   KnownType = EnBoisKnownType.DateTime,
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
			if (memActualType.IsGenericType)
			{
				if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)) &&
					memActualType.GetGenericArguments()[0] == typeof(string))
					return new BoisMemberInfo
							   {
								   KnownType = EnBoisKnownType.Unknown,
								   IsNullable = isNullable,
								   IsDictionary = true,
								   IsStringDictionary = true,
								   IsGeneric = true,
								   NullableUnderlyingType = underlyingTypeNullable,
							   };
#if DotNet4
				if (ReflectionHelper.CompareInterface(memType, typeof(ISet)))
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
			if (memActualType == typeof(TimeSpan) || memActualType == typeof(TimeSpan?))
			{
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
#if !SILVERLIGHT
			if (memActualType == typeof(Color) || memActualType == typeof(Color?))
			{
				return new BoisMemberInfo
						   {
							   KnownType = EnBoisKnownType.Color,
							   IsNullable = isNullable,
							   NullableUnderlyingType = underlyingTypeNullable,
						   };
			}
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
			if (ReflectionHelper.CompareSubType(memActualType, typeof(NameValueCollection)))
			{
				return new BoisMemberInfo
						   {
							   KnownType = EnBoisKnownType.NameValueColl,
							   IsNullable = isNullable,
							   NullableUnderlyingType = underlyingTypeNullable,
						   };
			}
#endif

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

			BoisMemberInfo output;
			if (TryReadNumber(memActualType, out output))
			{
				output.IsNullable = isNullable;
				output.NullableUnderlyingType = underlyingTypeNullable;
				return output;
			}
			if (memActualType == typeof(Guid) || memActualType == typeof(Guid?))
			{
				return new BoisMemberInfo
						   {
							   KnownType = EnBoisKnownType.Guid,
							   IsNullable = isNullable,
							   NullableUnderlyingType = underlyingTypeNullable,
						   };
			}
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

			var objectMemInfo = ReadObject(memType);
			objectMemInfo.NullableUnderlyingType = underlyingTypeNullable;
			return objectMemInfo;
		}

		static Type UnNullify(Type type)
		{
			return Nullable.GetUnderlyingType(type) ?? type;
		}
		/// <summary>
		/// Slower convertion
		/// </summary>
		private static bool IsNumber(Type memType, out BoisMemberInfo output)
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

		private static bool TryReadNumber(Type memType, out BoisMemberInfo output)
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
		private static GenericSetter CreateSetMethod(PropertyInfo propertyInfo)
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

			var setter = new DynamicMethod(
			  String.Concat("_Set", propertyInfo.Name, "_"),
			  typeof(void), arguments, propertyInfo.DeclaringType);
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

		/// <summary>
		/// Creates a dynamic getter for the property
		/// </summary>
		/// <author>
		/// Gerhard Stephan 
		/// http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/
		/// </author>
		private static GenericGetter CreateGetMethod(PropertyInfo propertyInfo)
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

			var getter = new DynamicMethod(
			  String.Concat("_Get", propertyInfo.Name, "_"),
			  typeof(object), arguments, propertyInfo.DeclaringType);
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

		//private static Func<T, object> MakeDelegate_2<T, U>(MethodInfo @get)
		//{
		//	var f = (Func<T, U>)Delegate.CreateDelegate(typeof(Func<T, U>), @get);
		//	return t => f(t);
		//}

		private static GenericGetter MakeDelegate(MethodInfo @get)
		{
			var f = (GenericGetter)Delegate.CreateDelegate(typeof(GenericGetter), @get);
			return t => f(t);
		}

		private static GenericGetter GetPropertyGetter(Type objType, PropertyInfo propertyInfo)
		{
			if (objType.IsValueType &&
				!objType.IsPrimitive &&
				!objType.IsArray &&
				objType != typeof(string))
			{
				// this is a fallback to slower method.
				var method = propertyInfo.GetGetMethod();

				// generating the caller.
				return new GenericGetter(target => method.Invoke(target, null));
			}
			else
			{
				//var method = objType.GetMethod("get_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
				var method = propertyInfo.GetGetMethod();
				return GetFastGetterFunc(propertyInfo, method);
			}
		}

		private static Function<object, object, object> GetPropertySetter(Type objType, PropertyInfo propertyInfo)
		{
			if (objType.IsValueType &&
				!objType.IsPrimitive &&
				!objType.IsArray &&
				objType != typeof(string))
			{
				// this is a fallback to slower method.
				var method = propertyInfo.GetSetMethod();

				// generating the caller.
				return new Function<object, object, object>((target, value) => method.Invoke(target, new object[] { value }));
			}
			else
			{
				//var method = objType.GetMethod("set_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
				var method = propertyInfo.GetSetMethod();
				return GetFastSetterFunc(propertyInfo, method);
			}

			////var method = objType.GetMethod("set_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
			//var method = propertyInfo.GetSetMethod();
			//return GetFastSetterFunc(propertyInfo, method);
		}

		/// <summary>
		/// http://social.msdn.microsoft.com/Forums/en-US/netfxbcl/thread/8754500e-4426-400f-9210-554f9f2ad58b/
		/// </summary>
		/// <returns></returns>
		private static GenericGetter GetFastGetterFunc(PropertyInfo p, MethodInfo getter) // untyped cast from Func<T> to Func<object> 
		{
			var g = new DynamicMethod("_", typeof(object), new[] { typeof(object) }, p.DeclaringType, true);
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

		/// <summary>
		/// http://social.msdn.microsoft.com/Forums/en-US/netfxbcl/thread/8754500e-4426-400f-9210-554f9f2ad58b/
		/// </summary>
		private static Function<object, object, object> GetFastSetterFunc(PropertyInfo p, MethodInfo setter)
		{
			var s = new DynamicMethod("_", typeof(object), new[] { typeof(object), typeof(object) }, p.DeclaringType, true);
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

		private static GenericGetter GetPropertyGetter_(Type objType, PropertyInfo propertyInfo)
		{
			var method = objType.GetMethod("get_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(typeof(GenericGetter), method);
			return (GenericGetter)dlg;
		}

		private static GenericGetter GetPropertyGetter(object obj, string propertyName)
		{
			var t = obj.GetType();
			var method = t.GetMethod("get_" + propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(t, obj, method);
			return (GenericGetter)dlg;
		}

		private static GenericSetter GetPropertySetter(object obj, string propertyName)
		{
			var t = obj.GetType();
			var method = t.GetMethod("set_" + propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(t, obj, method);
			return (GenericSetter)dlg;
		}

	}
}
