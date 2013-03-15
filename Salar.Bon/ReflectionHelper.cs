using System;
using System.Reflection;

namespace Salar.Bon
{
	class ReflectionHelper
	{
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

	//public delegate void Procedure();
	//public delegate object Function();
	//public delegate TResult Function<out TResult>();
	//public delegate void Procedure<in T1>(T1 arg1);
	//public delegate void Procedure<in T1, in T2>(T1 arg1, T2 arg2);
	//public delegate void Procedure<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
	//public delegate TResult Function<in T, out TResult>(T arg);
	//public delegate TResult Function<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

}
