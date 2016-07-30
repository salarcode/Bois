using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace SalarCompactSerializer
{
	class JsonDeSerialReflection
	{
		private delegate object CreateObject();
		private SafeDictionary<Type, CreateObject> _constrcache = new SafeDictionary<Type, CreateObject>();
		private SafeDictionary<string, Type> _typecache = new SafeDictionary<string, Type>();
		internal delegate object GenericSetter(object target, object value);
		internal delegate object GenericGetter(object obj);

		internal static GenericSetter CreateSetField(Type type, FieldInfo fieldInfo)
		{
			Type[] arguments = new Type[2];
			arguments[0] = arguments[1] = typeof(object);

			DynamicMethod dynamicSet = new DynamicMethod("_", typeof(object), arguments, type, true);
			ILGenerator il = dynamicSet.GetILGenerator();

			if (!type.IsClass) // structs
			{
				var lv = il.DeclareLocal(type);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Unbox_Any, type);
				il.Emit(OpCodes.Stloc_0);
				il.Emit(OpCodes.Ldloca_S, lv);
				il.Emit(OpCodes.Ldarg_1);
				if (fieldInfo.FieldType.IsClass)
					il.Emit(OpCodes.Castclass, fieldInfo.FieldType);
				else
					il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
				il.Emit(OpCodes.Stfld, fieldInfo);
				il.Emit(OpCodes.Ldloc_0);
				il.Emit(OpCodes.Box, type);
				il.Emit(OpCodes.Ret);
			}
			else
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				if (fieldInfo.FieldType.IsValueType)
					il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
				il.Emit(OpCodes.Stfld, fieldInfo);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ret);
			}
			return (GenericSetter)dynamicSet.CreateDelegate(typeof(GenericSetter));
		}

		internal static GenericSetter CreateSetMethod(Type type, PropertyInfo propertyInfo)
		{
			MethodInfo setMethod = propertyInfo.GetSetMethod();
			if (setMethod == null)
				return null;

			Type[] arguments = new Type[2];
			arguments[0] = arguments[1] = typeof(object);

			DynamicMethod setter = new DynamicMethod("_", typeof(object), arguments);
			ILGenerator il = setter.GetILGenerator();

			if (!type.IsClass) // structs
			{
				var lv = il.DeclareLocal(type);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Unbox_Any, type);
				il.Emit(OpCodes.Stloc_0);
				il.Emit(OpCodes.Ldloca_S, lv);
				il.Emit(OpCodes.Ldarg_1);
				if (propertyInfo.PropertyType.IsClass)
					il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
				else
					il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
				il.EmitCall(OpCodes.Call, setMethod, null);
				il.Emit(OpCodes.Ldloc_0);
				il.Emit(OpCodes.Box, type);
			}
			else
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
				il.Emit(OpCodes.Ldarg_1);
				if (propertyInfo.PropertyType.IsClass)
					il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
				else
					il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
				il.EmitCall(OpCodes.Callvirt, setMethod, null);
				il.Emit(OpCodes.Ldarg_0);
			}

			il.Emit(OpCodes.Ret);

			return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
		}

		internal static GenericGetter CreateGetField(Type type, FieldInfo fieldInfo)
		{
			DynamicMethod dynamicGet = new DynamicMethod("_", typeof(object), new Type[] { typeof(object) }, type, true);
			ILGenerator il = dynamicGet.GetILGenerator();

			if (!type.IsClass) // structs
			{
				var lv = il.DeclareLocal(type);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Unbox_Any, type);
				il.Emit(OpCodes.Stloc_0);
				il.Emit(OpCodes.Ldloca_S, lv);
				il.Emit(OpCodes.Ldfld, fieldInfo);
				if (fieldInfo.FieldType.IsValueType)
					il.Emit(OpCodes.Box, fieldInfo.FieldType);
			}
			else
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, fieldInfo);
				if (fieldInfo.FieldType.IsValueType)
					il.Emit(OpCodes.Box, fieldInfo.FieldType);
			}

			il.Emit(OpCodes.Ret);

			return (GenericGetter)dynamicGet.CreateDelegate(typeof(GenericGetter));
		}

		internal static GenericGetter CreateGetMethod(Type type, PropertyInfo propertyInfo)
		{
			MethodInfo getMethod = propertyInfo.GetGetMethod();
			if (getMethod == null)
				return null;

			Type[] arguments = new Type[1];
			arguments[0] = typeof(object);

			DynamicMethod getter = new DynamicMethod("_", typeof(object), arguments, type);
			ILGenerator il = getter.GetILGenerator();

			if (!type.IsClass) // structs
			{
				var lv = il.DeclareLocal(type);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Unbox_Any, type);
				il.Emit(OpCodes.Stloc_0);
				il.Emit(OpCodes.Ldloca_S, lv);
				il.EmitCall(OpCodes.Call, getMethod, null);
				if (propertyInfo.PropertyType.IsValueType)
					il.Emit(OpCodes.Box, propertyInfo.PropertyType);
			}
			else
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
				il.EmitCall(OpCodes.Callvirt, getMethod, null);
				if (propertyInfo.PropertyType.IsValueType)
					il.Emit(OpCodes.Box, propertyInfo.PropertyType);
			}

			il.Emit(OpCodes.Ret);

			return (GenericGetter)getter.CreateDelegate(typeof(GenericGetter));
		}


		internal Type GetTypeFromCache(string typename)
		{
			Type val = null;
			if (_typecache.TryGetValue(typename, out val))
				return val;
			else
			{
				Type t = Type.GetType(typename);
				_typecache.Add(typename, t);
				return t;
			}
		}

		internal object FastCreateInstance(Type objtype)
		{
			try
			{
				CreateObject c = null;
				if (_constrcache.TryGetValue(objtype, out c))
				{
					return c();
				}
				else
				{
					if (objtype.IsClass)
					{
						DynamicMethod dynMethod = new DynamicMethod("_", objtype, null);
						ILGenerator ilGen = dynMethod.GetILGenerator();
						ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(Type.EmptyTypes));
						ilGen.Emit(OpCodes.Ret);
						c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
						_constrcache.Add(objtype, c);
					}
					else // structs
					{
						DynamicMethod dynMethod = new DynamicMethod("_",
							MethodAttributes.Public | MethodAttributes.Static,
							CallingConventions.Standard,
							typeof(object),
							null,
							objtype, false);
						ILGenerator ilGen = dynMethod.GetILGenerator();
						var lv = ilGen.DeclareLocal(objtype);
						ilGen.Emit(OpCodes.Ldloca_S, lv);
						ilGen.Emit(OpCodes.Initobj, objtype);
						ilGen.Emit(OpCodes.Ldloc_0);
						ilGen.Emit(OpCodes.Box, objtype);
						ilGen.Emit(OpCodes.Ret);
						c = (CreateObject)dynMethod.CreateDelegate(typeof(CreateObject));
						_constrcache.Add(objtype, c);
					}
					return c();
				}
			}
			catch (Exception exc)
			{
				throw new Exception(string.Format("Failed to fast create instance for type '{0}' from assemebly '{1}'",
					objtype.FullName, objtype.AssemblyQualifiedName), exc);
			}
		}

	}

	internal sealed class SafeDictionary<TKey, TValue>
	{
		private readonly object _Padlock = new object();
		private readonly Dictionary<TKey, TValue> _Dictionary;

		public SafeDictionary(int capacity)
		{
			_Dictionary = new Dictionary<TKey, TValue>(capacity);
		}

		public SafeDictionary()
		{
			_Dictionary = new Dictionary<TKey, TValue>();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			lock (_Padlock)
				return _Dictionary.TryGetValue(key, out value);
		}

		public TValue this[TKey key]
		{
			get
			{
				lock (_Padlock)
					return _Dictionary[key];
			}
			set
			{
				lock (_Padlock)
					_Dictionary[key] = value;
			}
		}

		public void Add(TKey key, TValue value)
		{
			lock (_Padlock)
			{
				if (_Dictionary.ContainsKey(key) == false)
					_Dictionary.Add(key, value);
			}
		}
	}
}
