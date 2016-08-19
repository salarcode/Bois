using System;
using System.Collections.Generic;
using System.Reflection;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois
{
	internal delegate TResult Function<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

	static class ReflectionHelper
	{
		public static Type FindUnderlyingArrayElementType(Type arrayType)
		{
			return arrayType.GetElementType();
		}

		/// <summary>
		/// Finds the underlying element type of a contained generic type
		/// Less acurate but cpu cheaper
		/// </summary>
		public static Type FindUnderlyingGenericElementType(Type type)
		{
			if (type.BaseType == null)
				return null;
			foreach (var inter in type.GetInterfaces())
			{
				if (inter.IsGenericType)
				{
					// it should have only one argument
					var args = inter.GetGenericArguments();
					if (args.Length == 1)
					{
						return args[0];
					}
				}
			}
			return null;
		}

		public static Type[] FindUnderlyingGenericDictionaryElementType(Type type)
		{
			if (type.BaseType == null)
				return null;
			foreach (var inter in type.GetInterfaces())
			{
				if (inter.IsGenericType)
				{
					// it should have only one argument
					var args = inter.GetGenericArguments();
					if (args.Length == 2)
					{
						return args;
					}
				}
			}
			return null;
		}


		/// <summary>
		/// Finds the underlying element type of a contained generic type
		/// CPU heavy but more accurate!
		/// </summary>
		public static Type FindUnderlyingIEnumerableElementType(Type type)
		{
			if (type.BaseType == null)
				return null;
			var enumType = typeof(IEnumerable<>);
			foreach (var inter in type.GetInterfaces())
			{
				if (inter.IsGenericType)
				{
					// it should have only one argument
					var args = inter.GetGenericArguments();
					if (args.Length == 1)
					{
						var enumGeneric = typeof(IEnumerable<>).MakeGenericType(args[0]);
						if (enumGeneric.IsAssignableFrom(type))
							return args[0];
					}
				}
			}
			return null;
		}
		/// <summary>
		/// Check to see if the type implements an specific generic interface type
		/// </summary>
		public static bool CompareInterfaceGenericTypeDefinition(Type type, Type genericType)
		{
			foreach (var @interface in type.GetInterfaces())
			{
				if (@interface.IsGenericType)
				{
					if (@interface.GetGenericTypeDefinition() == genericType)
					{
						// if needed, you can also return the type used as generic argument
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Check to see if the type implements an specific generic interface type
		/// </summary>
		public static bool CompareInterfaceGenericTypeDefinition(Type[] typeInterfaces, Type genericType)
		{
			foreach (var @interface in typeInterfaces)
			{
				if (@interface.IsGenericType)
				{
					if (@interface.GetGenericTypeDefinition() == genericType)
					{
						// if needed, you can also return the type used as generic argument
						return true;
					}
				}
			}
			return false;
		}

		public static bool CompareSubType(Type t1, Type t2)
		{
			if (t1 != t2)
			{
				return t1.IsSubclassOf(t2);
			}
			return true;
		}
		public static bool CompareInterface(Type type, Type interfaceType)
		{
			if (type != interfaceType)
			{
				return interfaceType.IsAssignableFrom(type);
			}
			return true;
		}

		public static Array CreateArray(Type elementType, int length)
		{
			return Array.CreateInstance(elementType, length);
		}


		public static void SetValue(object obj, object value, FieldInfo memInfo)
		{
			memInfo.SetValue(obj, value);
		}

		public static bool IsNullable(Type typeofResult)
		{
			if (!typeofResult.IsValueType)
				return true; // ref-type

			if (Nullable.GetUnderlyingType(typeofResult) != null)
				return true; // Nullable<T>

			return false; // value-type
		}

		public static bool IsNullable(Type typeofResult, out Type underlyingType)
		{
			underlyingType = null;

			if (!typeofResult.IsValueType)
				return true; // ref-type

			underlyingType = Nullable.GetUnderlyingType(typeofResult);
			if (underlyingType != null)
				return true; // Nullable<T>

			return false; // value-type
		}


		public static bool IsNullable<T>()
		{
			var type = typeof(T);
			if (!type.IsValueType)
				return true; // ref-type

			if (Nullable.GetUnderlyingType(type) != null)
				return true; // Nullable<T>

			return false; // value-type
		}
		public static bool IsNullable<T>(T obj)
		{
			if (obj == null)
				return true; // obvious

			var type = typeof(T);
			if (!type.IsValueType)
				return true; // ref-type

			if (Nullable.GetUnderlyingType(type) != null)
				return true; // Nullable<T>

			return false; // value-type
		}


	}
}
