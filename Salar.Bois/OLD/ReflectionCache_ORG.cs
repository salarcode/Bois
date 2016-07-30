using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Salar.Bion
{

	class ReflectionCache
	{
		delegate void GenericSetter(object target, object value);
		delegate object GenericGetter(object target);
		public class TypeInfoCache
		{
			public MemberInfo[] Members;
			public PropertyInfo[] Properties;
			public FieldInfo[] Fields;
		}
		private readonly Hashtable _typeCache = new Hashtable();
		private readonly Hashtable _constructorCache = new Hashtable();
		private readonly Hashtable _setValueCache = new Hashtable();
		private readonly Hashtable _getValueCache = new Hashtable();

		public void SetValue(object obj, object value, PropertyInfo memInfo)
		{
			//memInfo.Set(obj, value);
			//obj.SetPropertyValue(memInfo.Name, value);
			var setterMethod = _setValueCache[memInfo] as GenericSetter;
			if (setterMethod == null)
			{
				setterMethod = CreateSetMethod(memInfo);
				_setValueCache.Add(memInfo, setterMethod);
			}
			try
			{
				setterMethod(obj, value);
			}
			catch (MethodAccessException)
			{
				memInfo.SetValue(obj, value, null);
			}
		}
		public object GetValue(object obj, PropertyInfo memInfo)
		{
			//return memInfo.Get(obj);
			//return obj.GetPropertyValue(memInfo.Name);
			var getter = _getValueCache[memInfo] as GenericGetter;
			if (getter == null)
			{
				getter = CreateGetMethod(memInfo);
				_getValueCache.Add(memInfo, getter);
			}
			return getter(obj);
		}

		public TypeInfoCache GetTypeInfo(Type type)
		{
			var info = _typeCache[type] as TypeInfoCache;
			if (info == null)
			{
				MemberInfo[] members = null;
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
				var plen = props.Length;
				var flen = fields.Length;

				// selecting writable properties
				if (plen > 0)
				{
					var wprops = new PropertyInfo[plen];
					int wpindex = 0;
					for (int i = 0; i < plen; i++)
					{
						var p = props[i];
						if (p.CanWrite)
						{
							wprops[wpindex] = p;
							wpindex++;
						}
					}
					if (wpindex < plen)
					{
						Array.Resize(ref wprops, wpindex);
						props = wprops;
					}
				}
				if (plen > 0 && flen > 0)
				{
					members = new MemberInfo[plen + flen];
					Array.Copy(props, members, plen);
					Array.Copy(fields, 0, members, plen, flen);
				}
				else if (plen > 0)
				{
					members = props;
				}
				else
				{
					members = fields;
				}


				info = new TypeInfoCache()
						   {
							   Fields = fields,
							   Properties = props,
							   Members = members,
						   };
				_typeCache.Add(type, info);
			}
			return info;
		}

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

	}
}
