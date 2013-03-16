using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Salar.Bon
{

	class ReflectionCache
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
}
