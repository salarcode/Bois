using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://bois.codeplex.com/
 * Mozilla Public License v2
 */
namespace Salar.Bois
{

	class ReflectionCache_OLD
	{
		internal delegate object GenericGetter(object target);
		internal delegate object GenericConstructor();

#if SILVERLIGHT
		private static Dictionary<Type, GenericConstructor> _constructorCache;
#else
		private readonly Hashtable _constructorCache = new Hashtable();
#endif
	

//#if SILVERLIGHT
//		private static Dictionary<Type, ConstructorInfo> _constructorCache;
//#else
//		private readonly Hashtable _constructorCache = new Hashtable();
//#endif
		//public object CreateInstance(Type t)
		//{
		//	var info = _constructorCache[t] as ConstructorInfo;
		//	if (info == null)
		//	{
		//		info = t.GetConstructor(Type.EmptyTypes);
		//		_constructorCache[t] = info;
		//	}
		//	if (info == null)
		//		throw new MissingMethodException(string.Format("No parameterless constructor defined for '{0}'.", t));
		//	return info.Invoke(null);
		//}

		public object CreateInstance(Type t)
		{
			// Read from cache
			var info = _constructorCache[t] as GenericConstructor;
			if (info == null)
			{
 				ConstructorInfo ctor = t.GetConstructor(Type.EmptyTypes);
				if (ctor == null)
				{
					// Falling back to default parameterless constructor.
					return Activator.CreateInstance(t, null);
				}

				var dynamicCtor = new DynamicMethod("_", t, Type.EmptyTypes, t, true);
				var il = dynamicCtor.GetILGenerator();

				il.Emit(OpCodes.Newobj, ctor);
				il.Emit(OpCodes.Ret);

				info = (GenericConstructor)dynamicCtor.CreateDelegate(typeof(GenericConstructor));

				_constructorCache[t] = info;
			}
			if (info == null)
				throw new MissingMethodException(string.Format("No parameterless constructor defined for '{0}'.", t));
			return info.Invoke();
		}
  	}


	//public delegate void Procedure();
	//public delegate void Procedure<in T1>(T1 arg1);
	//public delegate void Procedure<in T1, in T2>(T1 arg1, T2 arg2);
	//public delegate void Procedure<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
	//public delegate object Function();
	//public delegate TResult Function<out TResult>();
	//public delegate TResult Function<in T, out TResult>(T arg);
	//public delegate TResult Function<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

#if !DotNet4
	//public delegate TResult Func<out TResult>();
	//public delegate TResult Func<in T, out TResult>(T arg);
	//public delegate TResult Fun<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
	//public delegate TResult Func<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);
	//public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
#endif


}
