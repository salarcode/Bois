using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Salar.Bon
{

	class ReflectionCache
	{
#if SILVERLIGHT
		private static Dictionary<Type, ConstructorInfo> _constructorCache;
#else
		private readonly Hashtable _constructorCache = new Hashtable();
#endif
	
		public object CreateInstance(Type t)
		{
			var info = _constructorCache[t] as ConstructorInfo;
			if (info == null)
			{
				info = t.GetConstructor(Type.EmptyTypes);
				_constructorCache[t] = info;
			}
			if (info == null)
				throw new MissingMethodException(string.Format("No parameterless constructor defined for '{0}'.", t));
			return info.Invoke(null);
		}
	}
}
