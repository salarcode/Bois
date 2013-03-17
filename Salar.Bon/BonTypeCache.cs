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
 * Salar BON (Binary Object Notation)
 * by Salar Khalilzadeh
 * 
 * https://bon.codeplex.com/
 * Mozilla Public License v2
 */
namespace Salar.Bon
{
	public static class BonTypeCache
	{
		internal delegate void GenericSetter(object target, object value);
		internal delegate object GenericGetter(object target);

		internal enum EnBonMemberType
		{
			Object,
			Property,
			Field
		}
		internal enum EnBonKnownType
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
		internal class BonMemberInfo
		{
			public bool IsNullable;
			public bool IsGeneric;
			public bool IsStringDictionary;
			public bool IsDictionary;
			public bool IsCollection;
			public bool IsArray;
			public bool IsSupportedPrimitive;

			public EnBonMemberType MemberType;
			public EnBonKnownType KnownType;
			public MemberInfo Info;

			public Function<object, object, object> PropertySetter;
			public GenericGetter PropertyGetter;
			public override string ToString()
			{
				return string.Format("{0}: {1}: {2}", MemberType, KnownType, Info);
			}
		}

		internal class BonTypeInfo : BonMemberInfo
		{
			public BonMemberInfo[] Members;
			public override string ToString()
			{
				return string.Format("{0}: {1}: {2}: Members= {3}", MemberType, KnownType, Info,
									 (Members != null) ? (Members.Length) : 0);
			}
		}
#if SILVERLIGHT
		private static Dictionary<Type, BonMemberInfo> _cache;
		static BonTypeCache()
		{
			_cache = new Dictionary<Type, BonMemberInfo>();
		}
#else
		private static Hashtable _cache;
		static BonTypeCache()
		{
			_cache = new Hashtable();
		}
#endif

		public static void ClearCache()
		{
			lock (_cache)
			{
				_cache.Clear();
			}
		}
		public static void RemoveEntry(Type type)
		{
			lock (_cache)
			{
				_cache.Remove(type);
			}
		}

		public static void Initialize<T>()
		{
			var type = typeof(T);
			InitializeInternal(type);
		}

		public static void Initialize(params Type[] types)
		{
			foreach (var t in types)
			{
				InitializeInternal(t);
			}
		}

		internal static BonMemberInfo GetTypeInfo(Type type, bool generate)
		{
#if SILVERLIGHT
			BonMemberInfo memInfo;
			if (_cache.TryGetValue(type, out memInfo))
			{
				return memInfo;
			}
#else
			var memInfo = _cache[type] as BonMemberInfo;
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
		private static void CacheInsert(Type type, BonMemberInfo memInfo)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(type))
				{
					_cache.Add(type, memInfo);
				}
			}
		}

		private static BonMemberInfo ReadObject(Type type)
		{
			var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

			var typeInfo = new BonTypeInfo
							   {
								   MemberType = EnBonMemberType.Object,
								   KnownType = EnBonKnownType.Unknown,
								   IsNullable = true
							   };
			var members = new List<BonMemberInfo>();
			foreach (var p in props)
			{
				if (p.CanWrite)
				{
					var info = ReadMemberInfo(p.PropertyType);
					info.PropertyGetter = GetPropertyGetter(type, p);
					//info.PropertySetter = CreateSetMethod(p);
					info.PropertySetter = GetPropertySetter(type, p);
					info.Info = p;
					info.MemberType = EnBonMemberType.Property;
					members.Add(info);
				}
			}
			foreach (var f in fields)
			{
				var info = ReadMemberInfo(f.FieldType);
				info.Info = f;
				info.MemberType = EnBonMemberType.Field;
				members.Add(info);
			}
			typeInfo.Members = members.ToArray();

			CacheInsert(type, typeInfo);
			return typeInfo;
		}

		private static BonMemberInfo ReadMemberInfo(Type memType)
		{
			if (memType == typeof(string))
			{
				return new BonMemberInfo
				{
					KnownType = EnBonKnownType.String,
					IsNullable = true,
					IsSupportedPrimitive = true,
				};
			}
			bool isNullable = ReflectionHelper.IsNullable(memType);

			if (memType == typeof(char) || memType == typeof(char?))
			{
				return new BonMemberInfo
				{
					KnownType = EnBonKnownType.Char,
					IsNullable = isNullable
				};
			}
			if (memType == typeof(bool) || memType == typeof(bool?))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.Bool,
							   IsNullable = isNullable
						   };
			}
			if (memType == typeof(DateTime) || memType == typeof(DateTime?))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.DateTime,
							   IsNullable = isNullable
						   };
			}
			if (memType == typeof(byte[]))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.ByteArray,
							   IsNullable = isNullable,
							   IsArray = true
						   };
			}
			if (ReflectionHelper.CompareSubType(memType, typeof(Enum)))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.Enum,
							   IsNullable = isNullable
						   };
			}
			if (ReflectionHelper.CompareSubType(memType, typeof(Array)))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.Unknown,
							   IsNullable = isNullable,
							   IsArray = true,
						   };
			}
			if (memType.IsGenericType)
			{
				if (ReflectionHelper.CompareInterface(memType, typeof(IDictionary)) &&
					memType.GetGenericArguments()[0] == typeof(string))
					return new BonMemberInfo
							   {
								   KnownType = EnBonKnownType.Unknown,
								   IsNullable = isNullable,
								   IsDictionary = true,
								   IsStringDictionary = true,
								   IsGeneric = true,
							   };
#if DotNet4
				if (ReflectionHelper.CompareInterface(memType, typeof(ISet)))
					return new BonMemberInfo
						       {
							       KnownType = EnBonKnownType.Unknown,
							       IsNullable = isNullable,
							       IsGeneric = true,
							       IsSet = true,
						       };
#endif
			}

			if (ReflectionHelper.CompareInterface(memType, typeof(IDictionary)))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.Unknown,
							   IsNullable = isNullable,
							   IsDictionary = true,
							   IsGeneric = true,
						   };
			}
			if (memType == typeof(TimeSpan) || memType == typeof(TimeSpan?))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.TimeSpan,
							   IsNullable = isNullable
						   };
			}
			if (memType == typeof(Version))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.Version,
							   IsNullable = isNullable
						   };
			}
#if !SILVERLIGHT
			if (memType == typeof(Color) || memType == typeof(Color?))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.Color,
							   IsNullable = isNullable
						   };
			}
			if (ReflectionHelper.CompareSubType(memType, typeof(DataSet)))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.DataSet,
							   IsNullable = isNullable
						   };
			}
			if (ReflectionHelper.CompareSubType(memType, typeof(DataTable)))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.DataTable,
							   IsNullable = isNullable
						   };
			}
			if (ReflectionHelper.CompareSubType(memType, typeof(NameValueCollection)))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.NameValueColl,
							   IsNullable = isNullable
						   };
			}
#endif

			if (ReflectionHelper.CompareInterface(memType, typeof(IList)) ||
				   ReflectionHelper.CompareInterface(memType, typeof(ICollection)))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.Unknown,
							   IsNullable = isNullable,
							   IsGeneric = memType.IsGenericType,
							   IsCollection = true,
							   IsArray = true,
						   };
			}

			BonMemberInfo output;
			if (TryReadNumber(memType, out output))
			{
				output.IsNullable = isNullable;
				return output;
			}
			if (memType == typeof(Guid) || memType == typeof(Guid?))
			{
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.Guid,
							   IsNullable = isNullable
						   };
			}
			if (memType == typeof(DBNull))
			{
				// ignore!
				return new BonMemberInfo
						   {
							   KnownType = EnBonKnownType.DbNull,
							   IsNullable = isNullable
						   };
			}

			return ReadObject(memType);
		}

		static Type UnNullify(Type type)
		{
			return Nullable.GetUnderlyingType(type) ?? type;
		}
		/// <summary>
		/// Slower convertion
		/// </summary>
		private static bool IsNumber(Type memType, out BonMemberInfo output)
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
					output = new BonMemberInfo
					{
						KnownType = EnBonKnownType.Int16,
						IsSupportedPrimitive = true,
					};
					break;

				case TypeCode.Int32:
					output = new BonMemberInfo
					{
						KnownType = EnBonKnownType.Int32,
						IsSupportedPrimitive = true,
					};
					break;

				case TypeCode.Int64:
					output = new BonMemberInfo
					{
						KnownType = EnBonKnownType.Int64,
						IsSupportedPrimitive = true,
					};
					break;
				case TypeCode.Single:
					output = new BonMemberInfo { KnownType = EnBonKnownType.Single };
					break;
				case TypeCode.Double:
					output = new BonMemberInfo { KnownType = EnBonKnownType.Double };
					break;
				case TypeCode.Decimal:
					output = new BonMemberInfo { KnownType = EnBonKnownType.Decimal };
					break;

				case TypeCode.Byte:
					output = new BonMemberInfo { KnownType = EnBonKnownType.Byte };
					break;
				case TypeCode.SByte:
					output = new BonMemberInfo { KnownType = EnBonKnownType.SByte };
					break;

				case TypeCode.UInt16:
					output = new BonMemberInfo { KnownType = EnBonKnownType.UInt16 };
					break;
				case TypeCode.UInt32:
					output = new BonMemberInfo { KnownType = EnBonKnownType.UInt32 };
					break;
				case TypeCode.UInt64:
					output = new BonMemberInfo { KnownType = EnBonKnownType.UInt64 };
					break;
			}
			return output != null;
		}

		private static bool TryReadNumber(Type memType, out BonMemberInfo output)
		{
			if (memType.IsClass)
			{
				output = null;
				return false;
			}
			if (memType == typeof(int) || memType == typeof(int?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.Int32,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(long) || memType == typeof(long?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.Int64,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(short) || memType == typeof(short?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.Int16,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(double) || memType == typeof(double?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.Double,
				};
			}
			else if (memType == typeof(decimal) || memType == typeof(decimal?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.Decimal,
				};
			}
			else if (memType == typeof(float) || memType == typeof(float?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.Single,
				};
			}
			else if (memType == typeof(byte) || memType == typeof(byte?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.Byte,
				};
			}
			else if (memType == typeof(sbyte) || memType == typeof(sbyte?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.SByte,
				};
			}
			else if (memType == typeof(ushort) || memType == typeof(ushort?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.UInt16,
				};
			}
			else if (memType == typeof(uint) || memType == typeof(uint?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.UInt32,
				};
			}
			else if (memType == typeof(ulong) || memType == typeof(ulong?))
			{
				output = new BonMemberInfo
				{
					KnownType = EnBonKnownType.UInt64,
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
			var method = propertyInfo.GetSetMethod();
			//var method = objType.GetMethod("set_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
			return GetFastSetterFunc(propertyInfo, method);
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
