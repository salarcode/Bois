using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Salar.Bon
{
	public static class Reflections2
	{
		public static PropertyData[] ReadObjectPropertiesDefinitions(object owner, bool getSetters = false)
		{
			if (owner == null) return new PropertyData[0];

			return ReadObjectPropertiesDefinitionsInternal(owner.GetType(), getSetters, owner);
		}

		//witout instance
		public static PropertyData[] ReadObjectPropertiesDefinitions<T>() where T : class
		{
			return ReadObjectPropertiesDefinitionsInternal(typeof(T), true);
		}

		private static PropertyData[] ReadObjectPropertiesDefinitionsInternal(Type t, bool getSet = false, object owner = null)
		{
			var properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var len = properties.Length;

			var arr = new PropertyData[len];

			for (int i = 0; i < len; i++)
			{
				var p = properties[i];
				var get = GetGetterFunc(p, t);
				var set = getSet ? GetSetterFunc(p, t) : null;
				var pdt = new PropertyData(p.Name, p.PropertyType, owner, get, set);
				arr[i] = pdt;
			}
			return arr;
		}

		public static Func<object, object> GetPropertyGetter<T>(string propertyName) where T : class
		{
			return GetPropertyGetter(typeof(T), propertyName);
		}
		public static Function<object, object, object> GetPropertySetter<T>(string propertyName) where T : class
		{
			return GetPropertySetter(typeof(T), propertyName);
		}

		public static Func<object, object> GetPropertyGetter(Type t, string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName))
				return null;

			var p = t.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

			if (p == null)
				return null;

			return GetGetterFunc(p, t);
		}

		public static Function<object, object, object> GetPropertySetter(Type t, string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName))
				return null;

			var p = t.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

			if (p == null)
				return null;

			return GetSetterFunc(p, t);
		}

		private static readonly Dictionary<string, Func<object, object>> _gettersCache = new Dictionary<string, Func<object, object>>(107);//107 prime init size for dictionary,reduce copying everything on add
		private static readonly Dictionary<string, Function<object, object, object>> _settersCache = new Dictionary<string, Function<object, object, object>>(107);//107 prime init size for dictionary,reduce copying everything on add

		private static Func<object, object> GetGetterFunc(PropertyInfo p, Type ownerType)
		{
			Func<object, object> func;
			string key = ownerType.FullName + "." + p.Name;
			if (_gettersCache.TryGetValue(key, out func))
			{
				return func;
			}

			var getter = ownerType.GetMethod("get_" + p.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			if (getter == null) return null;

			try
			{
				func = GetFastGetterFunc(p, getter);//fast
			}
			catch
			{
				func = (o) => getter.Invoke(o, null); //use the slow reflection if there are any other errors, shoudn't get here
			}

			_gettersCache.Add(key, func);
			return func;
		}

		private static Function<object, object, object> GetSetterFunc(PropertyInfo p, Type ownerType)
		{
			Function<object, object, object> function;
			string key = ownerType.FullName + "." + p.Name;
			if (_settersCache.TryGetValue(key, out function))
			{
				return function;
			}

			var setter = ownerType.GetMethod("set_" + p.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			if (setter == null) return null;

			try
			{
				function = GetFastSetterFunc(p, setter);//fast
			}
			catch
			{
				function = (o, v) => setter.Invoke(o, new[] { v }); //use the slow reflection if there are any other errors, shoudn't get here
			}

			_settersCache.Add(key, function);
			return function;
		}

		private static Func<object, object> GetFastGetterFunc(PropertyInfo p, MethodInfo getter) // untyped cast from Func<T> to Func<object> 
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

			var _func = (Func<object, object>)g.CreateDelegate(typeof(Func<object, object>));
			return _func;
		}

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

		public static T CreatInstanceFast<T>(out Func<T> _ctor)
		{
			var t = typeof(T);
			ConstructorInfo ctor = t.GetConstructor(Type.EmptyTypes);

			if (ctor == null)
			{
				_ctor = null;
				return default(T);
			}

			var dynamicCtor = new DynamicMethod("_", t, Type.EmptyTypes, t, true);
			var il = dynamicCtor.GetILGenerator();

			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Ret);

			_ctor = (Func<T>)dynamicCtor.CreateDelegate(typeof(Func<T>));

			T instance = _ctor();
			return instance;
		}
	}

	public struct PropertyData
	{
		public PropertyData(string name, Type type, object instance, Func<object, object> getter, Function<object, object, object> setter)
		{
			_name = name;
			_instance = instance;
			_type = type;
			_getter = getter;
			_setter = setter;
		}

		public string Name
		{
			get { return _name; }
		}
		public Type Type
		{
			get { return _type; }
		}

		public void SetInstance(object instance)
		{
			_instance = instance;
		}

		private readonly string _name;
		private object _instance;
		private readonly Type _type;
		private readonly Func<object, object> _getter;
		private readonly Function<object, object, object> _setter;


		public object Value
		{
			get
			{
				if (_getter != null) return _getter(_instance);
				return null;
			}
			set
			{
				if (_setter != null) _setter(_instance, value);
			}
		}
	}

	/// <summary>
	/// provides faster then usual reflection type construction and
	/// access with short helper methods to private members,
	/// and the ability to cache their fast access methods
	/// </summary>

	public static class Reflections
	{
		public static R CreatInstanceFast<R>(out Func<R> _ctor)
		{
			var t = typeof(R);
			ConstructorInfo ctor = t.GetConstructor(Type.EmptyTypes);
			if (ctor == null)
			{
				_ctor = null;
				return default(R);
			}
			var dynamicCtor = new DynamicMethod("_", t, Type.EmptyTypes, t, true);
			var il = dynamicCtor.GetILGenerator();
			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Ret);
			_ctor = (Func<R>)dynamicCtor.CreateDelegate(typeof(Func<R>));
			R instance = _ctor();
			return instance;
		}

		public static R CreatInstanceFast<T1, R>(T1 t1, out Func<T1, R> _ctor)
		{
			var t = typeof(R);
			var parameters = new[] { typeof(T1) };
			ConstructorInfo ctor = t.GetConstructor(parameters);
			if (ctor == null)
			{
				_ctor = null;
				return default(R);
			}
			var dynamicCtor = new DynamicMethod("_", t, parameters, t, true);
			var il = dynamicCtor.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Ret);
			_ctor = (Func<T1, R>)dynamicCtor.CreateDelegate(typeof(Func<T1, R>));
			R instance = _ctor(t1);
			return instance;
		}

		public static R CreatInstanceFast<T1, T2, R>(T1 t1, T2 t2, out Function<T1, T2, R> _ctor)
		{
			var t = typeof(R);
			var parameters = new[] { typeof(T1), typeof(T2) };
			ConstructorInfo ctor = t.GetConstructor(parameters);
			if (ctor == null)
			{
				_ctor = null;
				return default(R);
			}
			var dynamicCtor = new DynamicMethod("_", t, parameters, t, true);
			var il = dynamicCtor.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Ret);
			_ctor = (Function<T1, T2, R>)dynamicCtor.CreateDelegate(typeof(Function<T1, T2, R>));
			R instance = _ctor(t1, t2);
			return instance;
		}

		public static R CreatInstanceFast<T1, T2, T3, R>(T1 t1, T2 t2, T3 t3, out Func<T1, T2, T3, R> _ctor)
		{
			var t = typeof(R);
			var parameters = new[] { typeof(T1), typeof(T2), typeof(T3) };
			ConstructorInfo ctor = t.GetConstructor(parameters);
			if (ctor == null)
			{
				_ctor = null;
				return default(R);
			}
			var dynamicCtor = new DynamicMethod("_", t, parameters, t, true);
			var il = dynamicCtor.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Ret);
			_ctor = (Func<T1, T2, T3, R>)dynamicCtor.CreateDelegate(typeof(Func<T1, T2, T3, R>));
			R instance = _ctor(t1, t2, t3);
			return instance;
		}

		public static R CreatInstanceFast<T1, T2, T3, T4, R>(T1 t1, T2 t2, T3 t3, T4 t4, out Func<T1, T2, T3, T4, R> _ctor)
		{
			var t = typeof(R);
			var parameters = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
			ConstructorInfo ctor = t.GetConstructor(parameters);
			if (ctor == null)
			{
				_ctor = null;
				return default(R);
			}
			var dynamicCtor = new DynamicMethod("_", t, parameters, t, true);
			var il = dynamicCtor.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Ldarg_3);
			il.Emit(OpCodes.Newobj, ctor);
			il.Emit(OpCodes.Ret);
			_ctor = (Func<T1, T2, T3, T4, R>)dynamicCtor.CreateDelegate(typeof(Func<T1, T2, T3, T4, R>));
			R instance = _ctor(t1, t2, t3, t4);
			return instance;
		}

		public static R CreatInstanceFast<R>()
		{
			Func<R> ctor;
			return CreatInstanceFast(out ctor);
		}

		public static R CreatInstanceFast<T1, R>(T1 t1)
		{
			Func<T1, R> ctor;
			return CreatInstanceFast(t1, out ctor);
		}

		public static R CreatInstanceFast<T1, T2, R>(T1 t1, T2 t2)
		{
			Function<T1, T2, R> ctor;
			return CreatInstanceFast(t1, t2, out ctor);
		}

		public static R CreatInstanceFast<T1, T2, T3, R>(T1 t1, T2 t2, T3 t3)
		{
			Func<T1, T2, T3, R> ctor;
			return CreatInstanceFast(t1, t2, t3, out ctor);
		}

		public static R CreatInstanceFast<T1, T2, T3, T4, R>(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			Func<T1, T2, T3, T4, R> ctor;
			return CreatInstanceFast(t1, t2, t3, t4, out ctor);
		}

		public static Func<T> GetPropertyGetter<T>(object obj, string propertyName)
		{
			var t = obj.GetType();
			var method = t.GetMethod("get_" + propertyName, BindingFlags.Instance | BindingFlags.Public);
			var dlg = Delegate.CreateDelegate(t, obj, method);
			return (Func<T>)dlg;
		}

		public static Func<T> GetPrivatePropertyGetter<T>(object obj, string propertyName)
		{
			var t = obj.GetType();
			var method = t.GetMethod("get_" + propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(t, obj, method);
			return (Func<T>)dlg;
		}

		public static Action<T> GetPropertySetter<T>(object obj, string propertyName)
		{
			var t = obj.GetType();
			var method = t.GetMethod("set_" + propertyName, BindingFlags.Instance | BindingFlags.Public);
			var dlg = Delegate.CreateDelegate(t, obj, method);
			return (Action<T>)dlg;
		}

		public static Action<T> GetPrivatePropertySetter<T>(object obj, string propertyName)
		{
			var t = obj.GetType();
			var method = t.GetMethod("set_" + propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(t, obj, method);
			return (Action<T>)dlg;
		}

		public static FieldInfo SetPrivateField(object obj, string fieldName, object value)
		{
			var fld = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			fld.SetValue(obj, value);
			return fld;
		}

		public static FieldInfo SetStaticPrivateField(object obj, string fieldName, object value)
		{
			var fld = obj.GetType().GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
			fld.SetValue(obj, value);
			return fld;
		}

		public static object GetPrivateField(object obj, string fieldName)
		{
			return obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
		}

		public static object GetPrivateField(object obj, string fieldName, out FieldInfo fld)
		{
			fld = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			return fld.GetValue(obj);
		}

		public static object GetStaticPrivateField(object obj, string fieldName)
		{
			return obj.GetType().GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic).GetValue(obj);
		}

		public static object GetStaticPrivateField(object obj, string fieldName, out FieldInfo fld)
		{
			fld = obj.GetType().GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
			return fld.GetValue(obj);
		}

		public static object CallPrivateMethod(object obj, string methodName, params object[] parameters)
		{
			return obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(obj, parameters);
		}

		public static object CallPrivateMethod(object obj, string methodName, out Delegate dlg,
											   params object[] parameters)
		{
			var t = obj.GetType();
			var method = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
			dlg = Delegate.CreateDelegate(t, obj, method);
			return dlg.DynamicInvoke(obj, parameters);
		}

		public static object CallStaticPrivateMethod(object obj, string methodName, params object[] parameters)
		{
			return obj.GetType().GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic).Invoke(obj, parameters);
		}

		public static object CallStaticPrivateMethod(object obj, string methodName, out Delegate dlg,
													 params object[] parameters)
		{
			var t = obj.GetType();
			var method = t.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
			dlg = Delegate.CreateDelegate(t, obj, method);
			return dlg.DynamicInvoke(obj, parameters);
		}
	}
}