#define DebugSaveAsAssembly
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Salar.Bois.Serializers;
using Sigil;
using Sigil.NonGeneric;

// ReSharper disable AssignNullToNotNullAttribute

namespace Salar.Bois.Types
{
	internal delegate void ComputedTypeSerializer(BinaryWriter writer, object instance, Encoding encoding);

	static class IlExtensions
	{
		internal static void LoadLocal(this ILGenerator il, LocalBuilder local)
		{
			switch (local.LocalIndex)
			{
				case 0: il.Emit(OpCodes.Ldloc_0); break;
				case 1: il.Emit(OpCodes.Ldloc_1); break;
				case 2: il.Emit(OpCodes.Ldloc_2); break;
				case 3: il.Emit(OpCodes.Ldloc_3); break;
				default:
					if (local.LocalIndex < 256)
					{
						il.Emit(OpCodes.Ldloc_S, (byte)local.LocalIndex);
					}
					else
					{
						il.Emit(OpCodes.Ldloc, (ushort)local.LocalIndex);
					}
					break;
			}
		}
	}

	sealed class BoisTypeCompiler
	{
		/// <summary>
		/// Will hold dynamically generated methods
		/// </summary>
		class BoisCompiledTypesHolder
		{
			internal static string GetTypeMethodName(Type type, bool serialize)
			{
				var rnd = new Random();
				var nameExtension = serialize ? "Writer" : "Reader";

				return $"Computed_{type.Name}_{nameExtension}_{rnd.Next()}";
			}
		}


		public static Delegate ComputeRootWriter(Type type)
		{
#if DebugSaveAsAssembly
			var name = BoisCompiledTypesHolder.GetTypeMethodName(type, true) + ".exe";
			var assemblyName = new AssemblyName(name);

			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
				name: assemblyName,
				access: AssemblyBuilderAccess.RunAndSave);

			var moduleBuilder = assemblyBuilder.DefineDynamicModule(name);

			var programmClass = moduleBuilder.DefineType("Program", TypeAttributes.Public);

			var mainMethod = programmClass.DefineMethod(name: "Main",
				attributes: MethodAttributes.Public | MethodAttributes.Static,
				returnType: null,
				parameterTypes: new Type[] { typeof(string[]) });

			var mailIl = mainMethod.GetILGenerator();
			mainMethod.InitLocals = true;
			mailIl.DeclareLocal(typeof(string));

			mailIl.Emit(OpCodes.Ldstr, "Hello World!");
			mailIl.Emit(OpCodes.Stloc_0);
			mailIl.Emit(OpCodes.Ldloc_0);
			mailIl.Emit(OpCodes.Call, (typeof(Console)).GetMethod("WriteLine", new Type[] { typeof(string) }));
			mailIl.Emit(OpCodes.Call, (typeof(Console)).GetMethod("ReadKey", new Type[0]));
			mailIl.Emit(OpCodes.Pop);
			mailIl.Emit(OpCodes.Ret);

			var ilMethod = programmClass.DefineMethod(
				name: BoisCompiledTypesHolder.GetTypeMethodName(type, serialize: true),
				attributes: MethodAttributes.Public | MethodAttributes.Static,
				returnType: null,
				// Arg0: BinaryWriter, Arg1: instance, Arg2: Encoding
				parameterTypes: new[] { typeof(BinaryWriter), type/*typeof(object)*/, typeof(Encoding) });

			ilMethod.DefineParameter(1, ParameterAttributes.None, "writer");
			ilMethod.DefineParameter(2, ParameterAttributes.None, "instance");
			ilMethod.DefineParameter(3, ParameterAttributes.None, "encoding");
#else
			var ilMethod = new DynamicMethod(
				name: BoisCompiledTypesHolder.GetTypeMethodName(type, serialize: true),
				returnType: null,
				// Arg0: BinaryWriter, Arg1: instance, Arg2: Encoding
				parameterTypes: new[] { typeof(BinaryWriter), type/*typeof(object)*/, typeof(Encoding) },
				m: typeof(BoisSerializer).Module,
				skipVisibility: true);
			ilMethod.DefineParameter(1, ParameterAttributes.None, "writer");
			ilMethod.DefineParameter(2, ParameterAttributes.None, "instance");
			ilMethod.DefineParameter(3, ParameterAttributes.None, "encoding");

#endif

			var result = new BoisComputedTypeInfo();

			var il = ilMethod.GetILGenerator();
			ComputeWriterTypeCast(il, type);
			ComputeRootWriter(il, type);

			// never forget
			il.Emit(OpCodes.Ret);

#if DebugSaveAsAssembly
			programmClass.CreateType();

			assemblyBuilder.SetEntryPoint(((Type)programmClass).GetMethod("Main"));

			assemblyBuilder.Save(name);

			throw new NotImplementedException("این آخرشه");
#else
			var delegateType = typeof(SerializeDelegate<>).MakeGenericType(type);

			// the serializer method is ready
			var writerDelegate = ilMethod.CreateDelegate(delegateType);
			
			return writerDelegate;
#endif

		}

		/// <summary>
		/// The input is Object and should be casted to the original type
		/// </summary>
		private static void ComputeWriterTypeCast(ILGenerator il, Type type)
		{
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Castclass, type); // cast

			il.DeclareLocal(type);
			il.Emit(OpCodes.Stloc_0);
		}

		private static void ComputeRootWriter(ILGenerator il, Type memType)
		{
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

			// ----------------------------------
			// Step1: Check if we should the input is one of simple objects

#if DotNet || DotNetCore || DotNetStandard
			if (ReflectionHelper.CompareSubType(memActualType, typeof(NameValueCollection)))
			{
				WriteRootNameValueCol(il);
				return;
			}
#endif

			// TODO: array
			//if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			//{
			//	var arrayItemType = memActualType.GetElementType();

			//	return IsPrimitveType(arrayItemType);
			//}

			//var isGenericType = memActualType.IsGenericType;
			//Type[] interfaces = null;
			//if (isGenericType)
			//{
			//	return false;
			//}

			if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)))
			{
				WriteRootDictionary(il);
				return;
			}

			// checking for IList and ICollection should be after NameValueCollection
			if (ReflectionHelper.CompareInterface(memActualType, typeof(IList)))
			{
				WriteRootList(il);
				return;
			}
			if (ReflectionHelper.CompareInterface(memActualType, typeof(ICollection)))
			{
				WriteRootCollection(il);
				return;
			}

#if !SILVERLIGHT && DotNet
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataSet)))
			{
				WriteRootDataSet(il);
				return;
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataTable)))
			{
				WriteRootDataTable(il);
				return;
			}
#endif
			// ----------------------------------
			// Step2: the input is not simple object is a complex one with fields and properties

			// Write Root Object
			WriteRootObject(il, memType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		private static void WriteRootObject(ILGenerator il, Type type)
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
			//var typeInfo = new BoisTypeInfo
			//{
			//	MemberType = EnBoisMemberType.Object,
			//	KnownType = EnBoisKnownType.Unknown,
			//	IsContainerObject = true,
			//	IsStruct = type.IsValueType
			//};

			if (readProps)
			{
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				for (var index = 0; index < props.Length; index++)
				{
					var p = props[index];
					if (!p.CanWrite)
						continue;

					var memProp = p.GetCustomAttributes(typeof(BoisMemberAttribute), false);
					BoisMemberAttribute boisMember;
					if (memProp.Length > 0 && (boisMember = (memProp[0] as BoisMemberAttribute)) != null)
					{
						if (!boisMember.Included)
							continue;
					}

					WriteRootMember(p.PropertyType, p, index, il);
				}
			}

			if (readFields)
			{
				var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (var f in fields)
				{
				}
			}


		}

		private static void WriteRootMember(Type memType, PropertyInfo memberInfo, int memberIndex, ILGenerator il)
		{
			if (memType == typeof(string))
			{
				WriteString(memberInfo, il);
				return;
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


			//			if (memActualType == typeof(char))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Char,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (memActualType == typeof(bool))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Bool,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (memActualType == typeof(DateTime))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.DateTime,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (memActualType == typeof(DateTimeOffset))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.DateTimeOffset,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (memActualType == typeof(byte[]))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.ByteArray,
			//					IsNullable = isNullable,
			//					IsArray = true,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (ReflectionHelper.CompareSubType(memActualType, typeof(Enum)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Enum,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Unknown,
			//					IsNullable = isNullable,
			//					IsArray = true,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}

			//			var isGenericType = memActualType.IsGenericType;
			//			Type[] interfaces = null;
			//			if (isGenericType)
			//			{
			//				//// no more checking for a dictionary with its first argumnet as String
			//				//if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)) &&
			//				//	memActualType.GetGenericArguments()[0] == typeof(string))
			//				//	return new BoisMemberInfo
			//				//	{
			//				//		KnownType = EnBoisKnownType.Unknown,
			//				//		IsNullable = isNullable,
			//				//		IsDictionary = true,
			//				//		IsStringDictionary = true,
			//				//		IsGeneric = true,
			//				//		NullableUnderlyingType = underlyingTypeNullable,
			//				//	};

			//				interfaces = memActualType.GetInterfaces();

			//				if (ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(IDictionary<,>)) ||
			//					memActualType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
			//					return new BoisMemberInfo
			//					{
			//						KnownType = EnBoisKnownType.Unknown,
			//						IsNullable = isNullable,
			//						IsDictionary = true,
			//						IsGeneric = true,
			//						NullableUnderlyingType = underlyingTypeNullable,
			//					};

			//#if DotNet4_NotYET
			//				if (ReflectionHelper.CompareInterface(memType, typeof(ISet<>)))
			//					return new BoisMemberInfo
			//							   {
			//								   KnownType = EnBoisKnownType.Unknown,
			//								   IsNullable = isNullable,
			//								   IsGeneric = true,
			//								   IsSet = true,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//							   };
			//#endif
			//			}


			//			if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Unknown,
			//					IsNullable = isNullable,
			//					IsDictionary = true,
			//					IsGeneric = true,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			// the IDictionary should be checked before IList<>
			//			if (isGenericType)
			//			{
			//				if (ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(IList<>)) ||
			//					ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(ICollection<>)))
			//					return new BoisMemberInfo
			//					{
			//						KnownType = EnBoisKnownType.Unknown,
			//						IsNullable = isNullable,
			//						IsGeneric = true,
			//						IsCollection = true,
			//						IsArray = true,
			//						NullableUnderlyingType = underlyingTypeNullable,
			//					};
			//			}

			//			// checking for IList and ICollection should be after NameValueCollection
			//			if (ReflectionHelper.CompareInterface(memActualType, typeof(IList)) ||
			//				ReflectionHelper.CompareInterface(memActualType, typeof(ICollection)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Unknown,
			//					IsNullable = isNullable,
			//					IsGeneric = memActualType.IsGenericType,
			//					IsCollection = true,
			//					IsArray = true,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}

			//#if DotNet || DotNetCore || DotNetStandard
			//			if (ReflectionHelper.CompareSubType(memActualType, typeof(NameValueCollection)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.NameValueColl,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (memActualType == typeof(Color))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Color,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//#endif
			//#if !SILVERLIGHT && DotNet
			//			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataSet)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.DataSet,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataTable)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.DataTable,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}

			//#endif

			//			if (memActualType == typeof(TimeSpan))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.TimeSpan,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}

			//			if (memActualType == typeof(Version))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Version,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}

			//			BoisMemberInfo output;
			//			if (TryReadNumber(memActualType, out output))
			//			{
			//				output.IsNullable = isNullable;
			//				output.NullableUnderlyingType = underlyingTypeNullable;
			//				return output;
			//			}
			//			if (memActualType == typeof(Guid))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Guid,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//#if DotNet || DotNetCore || DotNetStandard
			//			if (memActualType == typeof(DBNull))
			//			{
			//				// ignore!
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.DbNull,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//#endif

			//			var objectMemInfo = ReadObject(memType);
			//			objectMemInfo.NullableUnderlyingType = underlyingTypeNullable;
			//			objectMemInfo.IsNullable = isNullable;

			//			return objectMemInfo;

		}

		public static void WriteInt32(PropertyInfo memberInfo, short memberIndex, ILGenerator il, bool nullable)
		{
			var getter = memberInfo.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldloc, arg: memberIndex);
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(int?) } : new[] { typeof(int) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue), methodArg));
			il.Emit(OpCodes.Nop);
		}

		public static void WriteInt32(FieldInfo memberInfo, short memberIndex, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldloc, arg: memberIndex);
			il.Emit(OpCodes.Ldfld, field: memberInfo);
			var methodArg = nullable ? new[] { typeof(int?) } : new[] { typeof(int) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue), methodArg));
			il.Emit(OpCodes.Nop);
		}

		private static void WriteString(PropertyInfo memberInfo, ILGenerator il)
		{
			var getter = memberInfo.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Callvirt, meth: getter); // property value
			il.Emit(OpCodes.Ldarg_2); // Encoding
			var methodArg = new[] { typeof(BinaryWriter), typeof(string), typeof(Encoding) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		private static void WriteRootDataTable(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		private static void WriteRootDataSet(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		private static void WriteRootCollection(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		private static void WriteRootList(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		private static void WriteRootDictionary(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		private static void WriteRootNameValueCol(ILGenerator il)
		{
			throw new NotImplementedException();
		}


		delegate double DividerDelegate(int a, int b);
		public static void CompileType(Type type, bool writer, bool reader)
		{

			//تعریف امضای متد
			var myMethod = new DynamicMethod(
				name: "Compile_" + type.Name,
				returnType: null,
				parameterTypes: new[] { type, typeof(BinaryWriter) },
				m: typeof(BoisCompiledTypesHolder).Module);



			//تعریف بدنه متد
			var il = myMethod.GetILGenerator();
			il.Emit(opcode: OpCodes.Ldarg_0); //بارگذاری پارامتر اول بر روی پشته ارزیابی
			il.Emit(opcode: OpCodes.Ldarg_1); //بارگذاری پارامتر دوم بر روی پشته ارزیابی
			il.Emit(opcode: OpCodes.Div); // دو پارامتر از پشته ارزیابی دریافت و تقسیم خواهند شد
			il.Emit(opcode: OpCodes.Ret); // دریافت نتیجه نهایی از پشته ارزیابی و بازگشت آن




			//فراخوانی متد پویا
			//روش اول
			var result = myMethod.Invoke(obj: null, parameters: new object[] { 10, 2 });
			Console.WriteLine(result);

			//روش دوم
			var method = (DividerDelegate)myMethod.CreateDelegate(delegateType: typeof(DividerDelegate));
			Console.WriteLine(method(10, 2));
		}



		public static Delegate ComputeRootReader(Type type)
		{
#if DebugSaveAsAssembly
			var name = BoisCompiledTypesHolder.GetTypeMethodName(type, false) + ".exe";
			var assemblyName = new AssemblyName(name);

			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
				name: assemblyName,
				access: AssemblyBuilderAccess.RunAndSave);

			var moduleBuilder = assemblyBuilder.DefineDynamicModule(name);

			var programmClass = moduleBuilder.DefineType("Program", TypeAttributes.Public);

			var mainMethod = programmClass.DefineMethod(name: "Main",
				attributes: MethodAttributes.Public | MethodAttributes.Static,
				returnType: null,
				parameterTypes: new Type[] { typeof(string[]) });

			var mailIl = mainMethod.GetILGenerator();
			mainMethod.InitLocals = true;
			mailIl.DeclareLocal(typeof(string));

			mailIl.Emit(OpCodes.Ldstr, "Hello World!");
			mailIl.Emit(OpCodes.Stloc_0);
			mailIl.Emit(OpCodes.Ldloc_0);
			mailIl.Emit(OpCodes.Call, (typeof(Console)).GetMethod("WriteLine", new Type[] { typeof(string) }));
			mailIl.Emit(OpCodes.Call, (typeof(Console)).GetMethod("ReadKey", new Type[0]));
			mailIl.Emit(OpCodes.Pop);
			mailIl.Emit(OpCodes.Ret);

			var ilMethod = programmClass.DefineMethod(
				name: BoisCompiledTypesHolder.GetTypeMethodName(type, serialize: false),
				attributes: MethodAttributes.Public | MethodAttributes.Static,
				returnType: type,
				// Arg0: BinaryWriter, Arg1: Encoding
				parameterTypes: new[] { typeof(BinaryReader), typeof(Encoding) });

			ilMethod.DefineParameter(1, ParameterAttributes.None, "reader");
			ilMethod.DefineParameter(2, ParameterAttributes.None, "encoding");
#else
			var ilMethod = new DynamicMethod(
				name: BoisCompiledTypesHolder.GetTypeMethodName(type, serialize: false),
				returnType: type,
				// Arg0: BinaryWriter, Arg1: Encoding
				parameterTypes: new[] { typeof(BinaryReader), typeof(Encoding) },
				m: typeof(BoisSerializer).Module,
				skipVisibility: true);
			ilMethod.DefineParameter(1, ParameterAttributes.None, "reader");
			ilMethod.DefineParameter(2, ParameterAttributes.None, "encoding");

#endif


			var il = ilMethod.GetILGenerator();
			// TODO: Should be Reader new obj
			ComputeReaderTypeCreation(il, type);

			ComputeRootReader(il, type);

			// never forget
			il.Emit(OpCodes.Ret);


#if DebugSaveAsAssembly
			programmClass.CreateType();

			assemblyBuilder.SetEntryPoint(((Type)programmClass).GetMethod("Main"));

			assemblyBuilder.Save(name);

			throw new NotImplementedException("این آخرشه");
#else
			var delegateType = typeof(SerializeDelegate<>).MakeGenericType(type);

			// the serializer method is ready
			var readerDelegate = ilMethod.CreateDelegate(delegateType);
			return readerDelegate;
#endif


		}

		private static void ComputeReaderTypeCreation(ILGenerator il, Type type)
		{
			var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder,
				Type.EmptyTypes, null);
			if (constructor == null)
				throw new Exception($"Type '{type}' doesn't have a constructor with empty parameters");

			//il.DeclareLocal(type); no need it is on the stack
			il.Emit(OpCodes.Newobj, constructor);
			//il.Emit(OpCodes.Stloc_0); no need it is on the stack
		}

		private static void ComputeRootReader(ILGenerator il, Type memType)
		{
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

			// ----------------------------------
			// Step1: Check if we should the input is one of simple objects

#if DotNet || DotNetCore || DotNetStandard
			if (ReflectionHelper.CompareSubType(memActualType, typeof(NameValueCollection)))
			{
				ReadRootNameValueCol(il);
				return;
			}
#endif

			// TODO: array
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			{
				var arrayItemType = memActualType.GetElementType();

				ReadPrimitveArrayType(arrayItemType);
				return;
			}

			//var isGenericType = memActualType.IsGenericType;
			//Type[] interfaces = null;
			//if (isGenericType)
			//{
			// return ReadGenericType(aaaaaaa);
			//	return false;
			//}

			if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)))
			{
				ReadRootDictionary(il);
				return;
			}

			// checking for IList and ICollection should be after NameValueCollection
			if (ReflectionHelper.CompareInterface(memActualType, typeof(IList)))
			{
				ReadRootList(il);
				return;
			}
			if (ReflectionHelper.CompareInterface(memActualType, typeof(ICollection)))
			{
				ReadRootCollection(il);
				return;
			}

#if !SILVERLIGHT && DotNet
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataSet)))
			{
				ReturnRootDataSet(il);
				return;
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataTable)))
			{
				ReadRootDataTable(il);
				return;
			}
#endif

			// ----------------------------------
			// Step2: the input is not simple object is a complex one with fields and properties

			// Write Root Object
			ReadRootObject(il, memType);
		}

		private static void ReadRootObject(ILGenerator il, Type type)
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
			//var typeInfo = new BoisTypeInfo
			//{
			//	MemberType = EnBoisMemberType.Object,
			//	KnownType = EnBoisKnownType.Unknown,
			//	IsContainerObject = true,
			//	IsStruct = type.IsValueType
			//};

			if (readProps)
			{
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				for (var index = 0; index < props.Length; index++)
				{
					var p = props[index];
					if (!p.CanWrite)
						continue;

					var memProp = p.GetCustomAttributes(typeof(BoisMemberAttribute), false);
					BoisMemberAttribute boisMember;
					if (memProp.Length > 0 && (boisMember = (memProp[0] as BoisMemberAttribute)) != null)
					{
						if (!boisMember.Included)
							continue;
					}

					ReadRootMember(p.PropertyType, p, index, il);
				}
			}

			if (readFields)
			{
				var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (var f in fields)
				{
				}
			}
		}

		private static void ReadRootMember(Type memType, PropertyInfo memberInfo, int index, ILGenerator il)
		{
			if (memType == typeof(string))
			{
				GenerateReadString(memberInfo, il);
				return;
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


			//			if (memActualType == typeof(char))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Char,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (memActualType == typeof(bool))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Bool,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (memActualType == typeof(DateTime))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.DateTime,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (memActualType == typeof(DateTimeOffset))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.DateTimeOffset,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (memActualType == typeof(byte[]))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.ByteArray,
			//					IsNullable = isNullable,
			//					IsArray = true,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (ReflectionHelper.CompareSubType(memActualType, typeof(Enum)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Enum,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Unknown,
			//					IsNullable = isNullable,
			//					IsArray = true,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}

			//			var isGenericType = memActualType.IsGenericType;
			//			Type[] interfaces = null;
			//			if (isGenericType)
			//			{
			//				//// no more checking for a dictionary with its first argumnet as String
			//				//if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)) &&
			//				//	memActualType.GetGenericArguments()[0] == typeof(string))
			//				//	return new BoisMemberInfo
			//				//	{
			//				//		KnownType = EnBoisKnownType.Unknown,
			//				//		IsNullable = isNullable,
			//				//		IsDictionary = true,
			//				//		IsStringDictionary = true,
			//				//		IsGeneric = true,
			//				//		NullableUnderlyingType = underlyingTypeNullable,
			//				//	};

			//				interfaces = memActualType.GetInterfaces();

			//				if (ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(IDictionary<,>)) ||
			//					memActualType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
			//					return new BoisMemberInfo
			//					{
			//						KnownType = EnBoisKnownType.Unknown,
			//						IsNullable = isNullable,
			//						IsDictionary = true,
			//						IsGeneric = true,
			//						NullableUnderlyingType = underlyingTypeNullable,
			//					};

			//#if DotNet4_NotYET
			//				if (ReflectionHelper.CompareInterface(memType, typeof(ISet<>)))
			//					return new BoisMemberInfo
			//							   {
			//								   KnownType = EnBoisKnownType.Unknown,
			//								   IsNullable = isNullable,
			//								   IsGeneric = true,
			//								   IsSet = true,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//							   };
			//#endif
			//			}


			//			if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Unknown,
			//					IsNullable = isNullable,
			//					IsDictionary = true,
			//					IsGeneric = true,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			// the IDictionary should be checked before IList<>
			//			if (isGenericType)
			//			{
			//				if (ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(IList<>)) ||
			//					ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(ICollection<>)))
			//					return new BoisMemberInfo
			//					{
			//						KnownType = EnBoisKnownType.Unknown,
			//						IsNullable = isNullable,
			//						IsGeneric = true,
			//						IsCollection = true,
			//						IsArray = true,
			//						NullableUnderlyingType = underlyingTypeNullable,
			//					};
			//			}

			//			// checking for IList and ICollection should be after NameValueCollection
			//			if (ReflectionHelper.CompareInterface(memActualType, typeof(IList)) ||
			//				ReflectionHelper.CompareInterface(memActualType, typeof(ICollection)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Unknown,
			//					IsNullable = isNullable,
			//					IsGeneric = memActualType.IsGenericType,
			//					IsCollection = true,
			//					IsArray = true,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}

			//#if DotNet || DotNetCore || DotNetStandard
			//			if (ReflectionHelper.CompareSubType(memActualType, typeof(NameValueCollection)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.NameValueColl,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (memActualType == typeof(Color))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Color,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//#endif
			//#if !SILVERLIGHT && DotNet
			//			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataSet)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.DataSet,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataTable)))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.DataTable,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}

			//#endif

			//			if (memActualType == typeof(TimeSpan))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.TimeSpan,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}

			//			if (memActualType == typeof(Version))
			//			{
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Version,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}

			//			BoisMemberInfo output;
			//			if (TryReadNumber(memActualType, out output))
			//			{
			//				output.IsNullable = isNullable;
			//				output.NullableUnderlyingType = underlyingTypeNullable;
			//				return output;
			//			}
			//			if (memActualType == typeof(Guid))
			//			{
			//				// is struct and uses Nullable<>
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.Guid,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//#if DotNet || DotNetCore || DotNetStandard
			//			if (memActualType == typeof(DBNull))
			//			{
			//				// ignore!
			//				return new BoisMemberInfo
			//				{
			//					KnownType = EnBoisKnownType.DbNull,
			//					IsNullable = isNullable,
			//					NullableUnderlyingType = underlyingTypeNullable,
			//				};
			//			}
			//#endif

			//			var objectMemInfo = ReadObject(memType);
			//			objectMemInfo.NullableUnderlyingType = underlyingTypeNullable;
			//			objectMemInfo.IsNullable = isNullable;

			//			return objectMemInfo;
		}

		private static void GenerateReadString(PropertyInfo memberInfo, ILGenerator il)
		{
			/*
			 ldloc.s   parent
			 ldloc.s   reader
			 ldloc.s   encoding
			 call      string [Salar.Bois.EmitNet4]Salar.Bois.Serializers.PrimitiveReader::ReadString(class [mscorlib]System.IO.BinaryReader, class [mscorlib]System.Text.Encoding)
			 callvirt  instance void Salar.Bois.EmitPlayground.Program/ParentClass::set_Name(string)
			 nop
			 */
			var setter = memberInfo.GetSetMethod(true);

			il.Emit(OpCodes.Dup); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader
			il.Emit(OpCodes.Ldarg_1); // Encoding

			var methodArg = new[] { typeof(BinaryReader), typeof(Encoding) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadString),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		private static object ReadRootDataTable(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		private static void ReturnRootDataSet(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		private static void ReadRootCollection(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		private static void ReadRootList(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		private static void ReadRootDictionary(ILGenerator il)
		{
			throw new NotImplementedException();
		}

		private static void ReadPrimitveArrayType(Type arrayItemType)
		{
			throw new NotImplementedException();
		}

		private static void ReadRootNameValueCol(ILGenerator il)
		{
			throw new NotImplementedException();
		}
	}
}
