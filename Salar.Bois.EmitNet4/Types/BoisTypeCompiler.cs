using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Salar.Bois.Serializers;
// ReSharper disable AssignNullToNotNullAttribute

namespace Salar.Bois.Types
{
	sealed class BoisTypeCompiler
	{
		internal struct ComputeResult
		{
			public Delegate Delegate;

			public MethodInfo Method;
		}

		internal static string GetTypeMethodName(Type type, bool serialize)
		{
			var rnd = new Random();
			var nameExtension = serialize ? "Writer" : "Reader";

			return $"Computed_{type.Name}_{nameExtension}_{rnd.Next()}";
		}

#if EmitAssemblyOut
		private static TypeBuilder _computeWriterSaveAssModule = null;

		internal static ComputeResult ComputeWriterSaveAss(Type type, BoisComplexTypeInfo typeInfo)
		{
			var saveAssembly = _computeWriterSaveAssModule == null;

			var name = GetTypeMethodName(type, true) + ".exe";
			var methodName = name;

			TypeBuilder programmClass;
			AssemblyBuilder assemblyBuilder = null;
			if (_computeWriterSaveAssModule == null)
			{
				var assemblyName = new AssemblyName(name);

				assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
					name: assemblyName,
					access: AssemblyBuilderAccess.RunAndSave);

				var moduleBuilder = assemblyBuilder.DefineDynamicModule(name);
				programmClass = moduleBuilder.DefineType("Program", TypeAttributes.Public);

				_computeWriterSaveAssModule = programmClass;

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
			}
			else
			{
				programmClass = _computeWriterSaveAssModule;
				methodName = GetTypeMethodName(type, true);
			}

			var ilMethod = programmClass.DefineMethod(
				name: GetTypeMethodName(type, serialize: true),
				attributes: MethodAttributes.Public | MethodAttributes.Static,
				returnType: null,
				// Arg0: BinaryWriter, Arg1: instance, Arg2: Encoding
				parameterTypes: new[] { typeof(BinaryWriter), type/*typeof(object)*/, typeof(Encoding) });

			ilMethod.DefineParameter(1, ParameterAttributes.None, "writer");
			ilMethod.DefineParameter(2, ParameterAttributes.None, "instance");
			ilMethod.DefineParameter(3, ParameterAttributes.None, "encoding");

			var il = ilMethod.GetILGenerator();

			ComputeWriter(il, type, typeInfo);

			// never forget
			il.Emit(OpCodes.Ret);


			if (saveAssembly)
			{
				var generatedType = programmClass.CreateType();

				assemblyBuilder.SetEntryPoint(((Type)programmClass).GetMethod("Main"));
				assemblyBuilder.Save(name);

				var delegateType = typeof(SerializeDelegate<>).MakeGenericType(type);
				var writerDelegate = generatedType.GetMethod(ilMethod.Name).CreateDelegate(delegateType);

				return new ComputeResult()
				{
					Method = ilMethod,
					Delegate = writerDelegate
				};
			}


			return new ComputeResult()
			{
				Method = ilMethod,
				Delegate = null
			};
		}
#endif

		public static ComputeResult ComputeWriter(Type type, BoisComplexTypeInfo typeInfo, Module containerModule = null)
		{
			var module = containerModule ?? typeof(BoisSerializer).Module;

			var ilMethod = new DynamicMethod(
				name: GetTypeMethodName(type, serialize: true),
				returnType: null,
				// Arg0: BinaryWriter, Arg1: instance, Arg2: Encoding
				parameterTypes: new[] { typeof(BinaryWriter), type/*typeof(object)*/, typeof(Encoding) },
				m: module,
				skipVisibility: true);
#if NetFX
			ilMethod.DefineParameter(1, ParameterAttributes.None, "writer");
			ilMethod.DefineParameter(2, ParameterAttributes.None, "instance");
			ilMethod.DefineParameter(3, ParameterAttributes.None, "encoding");
#endif

			var il = ilMethod.GetILGenerator();

			ComputeWriter(il, type, typeInfo);

			// never forget
			il.Emit(OpCodes.Ret);

			var delegateType = typeof(SerializeDelegate<>).MakeGenericType(type);

			// the serializer method is ready
			var writerDelegate = ilMethod.CreateDelegate(delegateType);

			return new ComputeResult()
			{
				Delegate = writerDelegate,
				Method = ilMethod
			};
		}



		internal static void ComputeWriter(ILGenerator il, Type type, BoisComplexTypeInfo typeInfo)
		{
			switch (typeInfo.ComplexKnownType)
			{
				case EnComplexKnownType.Dictionary:
					EmitGenerator.WriteRootDictionary(type, typeInfo, il);
					break;

				case EnComplexKnownType.DataSet:
					EmitGenerator.WriteRootDataSet(type, typeInfo, il);
					break;

				case EnComplexKnownType.DataTable:
					EmitGenerator.WriteRootDataTable(type, typeInfo, il);
					break;

				case EnComplexKnownType.NameValueColl:
					EmitGenerator.WriteRootNameValueCol(type, typeInfo, il);
					return;

				case EnComplexKnownType.UnknownArray:
					EmitGenerator.WriteRootUnknownArray(type, typeInfo, il);
					break;

				case EnComplexKnownType.ISet:
					EmitGenerator.WriteRootISet(type, typeInfo, il);
					break;

				case EnComplexKnownType.Collection:
					EmitGenerator.WriteRootList(type, typeInfo, il);
					break;

				case EnComplexKnownType.Unknown:
				default:
					WriteRootObject(il, type, typeInfo);
					break;
			}
		}

		private static void WriteRootObject(ILGenerator il, Type type, BoisComplexTypeInfo typeInfo)
		{
			if (typeInfo.Members == null || typeInfo.Members.Length == 0)
			{
				// no mmeber
				return;
			}

			//TODO: check the impact of removing object count
			WriteRootObjectMembersCount(il, typeInfo);

			foreach (var member in typeInfo.Members)
			{
				WriteRootObjectMember(member, il);
			}
		}

		private static void WriteRootObjectMembersCount(ILGenerator il, BoisComplexTypeInfo typeInfo)
		{
			var memberCount = typeInfo.Members.Length;

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldc_I4_S, memberCount);
			il.Emit(OpCodes.Newobj, typeof(int?).GetConstructor(new[] { typeof(int) }));
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BinaryWriter), typeof(int?) }, null));
			il.Emit(OpCodes.Nop);
		}

		private static void WriteRootObjectMember(MemberInfo member, ILGenerator il)
		{
			var prop = member as PropertyInfo;
			var field = member as FieldInfo;

			Type memberType;
			if (prop != null)
			{
				memberType = prop.PropertyType;
			}
			else if (field != null)
			{
				memberType = field.FieldType;
			}
			else
			{
				return;
			}
			var basicInfo = BoisTypeCache.GetBasicType(memberType);

			if (basicInfo.KnownType != EnBasicKnownType.Unknown)
			{
				WriteRootObjectBasicMember(memberType, basicInfo, prop, field, il);
			}
			else
			{
				var complexTypeInfo = BoisTypeCache.GetComplexTypeUnCached(memberType);
				WriteRootObjectComplexMember(memberType, complexTypeInfo, prop, field, il);
			}
		}

		private static void WriteRootObjectComplexMember(Type memberType, BoisComplexTypeInfo complexTypeInfo, PropertyInfo prop, FieldInfo field, ILGenerator il)
		{
			switch (complexTypeInfo.ComplexKnownType)
			{
				case EnComplexKnownType.Collection:
					if (prop != null)
						EmitGenerator.WriteCollection(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.WriteDCollection(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.Dictionary:
					EmitGenerator.WriteDictionary(prop, field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.UnknownArray:
					EmitGenerator.WriteUnknownArray(prop, field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.NameValueColl:
					EmitGenerator.WriteNameValueColl(prop, field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.ISet:
					if (prop != null)
						EmitGenerator.WriteISet(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.WriteISet(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.DataSet:
					if (prop != null)
						EmitGenerator.WriteDataSet(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.WriteDataSet(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.DataTable:
					if (prop != null)
						EmitGenerator.WriteDataTable(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.WriteDataTable(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.Unknown:
				default:
					return;
			}
		}

		private static void WriteRootObjectBasicMember(Type memberType, BoisBasicTypeInfo basicInfo, PropertyInfo prop, FieldInfo field, ILGenerator il)
		{
			switch (basicInfo.KnownType)
			{
				case EnBasicKnownType.String:
					EmitGenerator.WriteString(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Bool:
					EmitGenerator.WriteBool(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Int16:
					EmitGenerator.WriteInt16(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Int32:
					EmitGenerator.WriteInt32(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Int64:
					EmitGenerator.WriteInt64(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.UInt16:
					EmitGenerator.WriteUInt16(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.UInt32:
					EmitGenerator.WriteUInt32(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.UInt64:
					EmitGenerator.WriteUInt64(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Double:
					EmitGenerator.WriteDouble(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Decimal:
					EmitGenerator.WriteDecimal(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Single:
					EmitGenerator.WriteFloat(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Byte:
					EmitGenerator.WriteByte(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.SByte:
					EmitGenerator.WriteSByte(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.DateTime:
					EmitGenerator.WriteDateTime(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.DateTimeOffset:
					EmitGenerator.WriteDateTimeOffset(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.KnownTypeArray:
					// TODO:
					break;

				case EnBasicKnownType.ByteArray:
					EmitGenerator.WriteByteArray(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Enum:
					EmitGenerator.WriteEnum(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.TimeSpan:
					EmitGenerator.WriteTimeSpan(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Char:
					EmitGenerator.WriteChar(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Guid:
					EmitGenerator.WriteGuid(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Color:
					EmitGenerator.WriteColor(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.DbNull:
					EmitGenerator.WriteDbNull(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Uri:
					EmitGenerator.WriteUri(prop, field, null, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Version:
					EmitGenerator.WriteVersion(prop, field, null, il, basicInfo.IsNullable);
					break;

				default:
				case EnBasicKnownType.Unknown:
					return;
			}
		}

		internal static void WriteBasicTypeDirectly(ILGenerator il, BoisBasicTypeInfo keyTypeBasicInfo, Func<Type> valueLoader)
		{
			switch (keyTypeBasicInfo.KnownType)
			{
				case EnBasicKnownType.String:
					EmitGenerator.WriteString(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Bool:
					EmitGenerator.WriteBool(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Int16:
					EmitGenerator.WriteInt16(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Int32:
					EmitGenerator.WriteInt32(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Int64:
					EmitGenerator.WriteInt64(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.UInt16:
					EmitGenerator.WriteUInt16(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.UInt32:
					EmitGenerator.WriteUInt32(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.UInt64:
					EmitGenerator.WriteUInt64(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Double:
					EmitGenerator.WriteDouble(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Decimal:
					EmitGenerator.WriteDecimal(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Single:
					EmitGenerator.WriteFloat(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Byte:
					EmitGenerator.WriteByte(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.SByte:
					EmitGenerator.WriteSByte(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.DateTime:
					EmitGenerator.WriteDateTime(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.DateTimeOffset:
					EmitGenerator.WriteDateTimeOffset(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.KnownTypeArray:
					//WriteKnownTypeArray(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.ByteArray:
					EmitGenerator.WriteByteArray(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Enum:
					EmitGenerator.WriteEnum(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.TimeSpan:
					EmitGenerator.WriteTimeSpan(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Char:
					EmitGenerator.WriteChar(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Guid:
					EmitGenerator.WriteGuid(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Color:
					EmitGenerator.WriteColor(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.DbNull:
					EmitGenerator.WriteDbNull(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Uri:
					EmitGenerator.WriteUri(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Version:
					EmitGenerator.WriteVersion(null, null, valueLoader, il, keyTypeBasicInfo.IsNullable);
					break;

				case EnBasicKnownType.Unknown:
				default:
					throw new ArgumentOutOfRangeException("keyTypeBasicInfo.KnownType");
			}
		}


		/// <summary>
		/// The input is Object and should be casted to the original type
		/// </summary>
		private static void ComputeWriterTypeCast_Obsolete_Stloc_0(ILGenerator il, Type type)
		{
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Castclass, type); // cast

			il.DeclareLocal(type);
			il.Emit(OpCodes.Stloc_0);
		}

#if EmitAssemblyOut
		private static TypeBuilder _computeReaderSaveAssModule = null;

		public static ComputeResult ComputeReaderSaveAss(Type type, BoisComplexTypeInfo typeInfo)
		{
			var saveAssembly = _computeReaderSaveAssModule == null;

			var name = GetTypeMethodName(type, false) + ".exe";

			AssemblyBuilder assemblyBuilder = null;
			TypeBuilder programmClass;
			if (_computeReaderSaveAssModule == null)
			{
				var assemblyName = new AssemblyName(name);

				assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
					name: assemblyName,
					access: AssemblyBuilderAccess.RunAndSave);

				var moduleBuilder = assemblyBuilder.DefineDynamicModule(name);

				programmClass = moduleBuilder.DefineType("Program", TypeAttributes.Public);
				_computeReaderSaveAssModule = programmClass;

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
			}
			else
			{
				programmClass = _computeReaderSaveAssModule;
			}


			var ilMethod = programmClass.DefineMethod(
				name: GetTypeMethodName(type, serialize: false),
				attributes: MethodAttributes.Public | MethodAttributes.Static,
				returnType: type,
				// Arg0: BinaryWriter, Arg1: Encoding
				parameterTypes: new[] { typeof(BinaryReader), typeof(Encoding) });

			ilMethod.DefineParameter(1, ParameterAttributes.None, "reader");
			ilMethod.DefineParameter(2, ParameterAttributes.None, "encoding");


			var il = ilMethod.GetILGenerator();
			ComputeReaderTypeCreation(il, type);
			ComputeReader(il, type, typeInfo);

			// never forget
			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ret);

			if (saveAssembly)
			{
				var generatedType = programmClass.CreateType();
				assemblyBuilder.SetEntryPoint(((Type)programmClass).GetMethod("Main"));
				assemblyBuilder.Save(name);

				var delegateType = typeof(DeserializeDelegate<>).MakeGenericType(type);
				var readerDelegate = generatedType.GetMethod(ilMethod.Name).CreateDelegate(delegateType);

				return new ComputeResult()
				{
					Method = ilMethod,
					Delegate = readerDelegate
				};
			}

			return new ComputeResult()
			{
				Method = ilMethod,
				Delegate = null
			};
		}
#endif

		public static ComputeResult ComputeReader(Type type, BoisComplexTypeInfo typeInfo, Module containerModule = null)
		{
			Module module = null;
			if (containerModule == null)
				module = typeof(BoisSerializer).Module;
			else
				module = containerModule;

			var ilMethod = new DynamicMethod(
			   name: GetTypeMethodName(type, serialize: false),
			   returnType: type,
			   // Arg0: BinaryWriter, Arg1: Encoding
			   parameterTypes: new[] { typeof(BinaryReader), typeof(Encoding) },
			   m: module,
			   skipVisibility: true);
#if NetFX
			ilMethod.DefineParameter(1, ParameterAttributes.None, "reader");
			ilMethod.DefineParameter(2, ParameterAttributes.None, "encoding");
#endif

			var il = ilMethod.GetILGenerator();

			ComputeReaderTypeCreation(il, type);
			ComputeReader(il, type, typeInfo);

			// never forget
			il.Emit(OpCodes.Ldloc_0);
			il.Emit(OpCodes.Ret);


			var delegateType = typeof(DeserializeDelegate<>).MakeGenericType(type);

			// the serializer method is ready
			var readerDelegate = ilMethod.CreateDelegate(delegateType);
			return new ComputeResult()
			{
				Method = ilMethod,
				Delegate = readerDelegate
			};
		}


		private static LocalBuilder ComputeReaderTypeCreation(ILGenerator il, Type type)
		{
			var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder,
				Type.EmptyTypes, null);
			if (constructor == null)
				throw new Exception($"Type '{type}' doesn't have a constructor with empty parameters");

			//// stored as the first stack
			//il.Emit(OpCodes.Newobj, constructor);

			// stored as first local variable
			var instanceVariable = il.DeclareLocal(type);
			il.Emit(OpCodes.Newobj, constructor);
			il.Emit(OpCodes.Stloc_0);

			return instanceVariable;
		}

		private static void ComputeReader(ILGenerator il, Type type, BoisComplexTypeInfo typeInfo)
		{
			switch (typeInfo.ComplexKnownType)
			{
				case EnComplexKnownType.Collection:
					EmitGenerator.ReadRootCollection(type, typeInfo, il);
					break;

				case EnComplexKnownType.Dictionary:
					EmitGenerator.ReadRootDictionary(type, typeInfo, il);
					break;

				case EnComplexKnownType.UnknownArray:
					EmitGenerator.ReadRootUnknownArray(type, typeInfo, il);
					break;

				case EnComplexKnownType.NameValueColl:
					EmitGenerator.ReadRootNameValueColl(type, typeInfo, il);
					break;

				case EnComplexKnownType.ISet:
					EmitGenerator.ReadRootISet(type, typeInfo, il);
					break;

				case EnComplexKnownType.DataSet:
					EmitGenerator.ReadRootDataSet(type, typeInfo, il);
					break;

				case EnComplexKnownType.DataTable:
					EmitGenerator.ReadRootDataTable(type, typeInfo, il);
					break;

				case EnComplexKnownType.Unknown:
				default:
					ReadRootObject(il, type, typeInfo);
					break;
			}
		}

		private static void ReadRootObject(ILGenerator il, Type type, BoisComplexTypeInfo typeInfo)
		{
			if (typeInfo.Members == null || typeInfo.Members.Length == 0)
			{
				// no mmeber
				return;
			}

			foreach (var member in typeInfo.Members)
			{
				ReadRootObjectMember(member, il);
			}
		}

		private static void ReadRootObjectMember(MemberInfo member, ILGenerator il)
		{
			var prop = member as PropertyInfo;
			var field = member as FieldInfo;

			Type memberType;
			if (prop != null)
			{
				memberType = prop.PropertyType;
			}
			else if (field != null)
			{
				memberType = field.FieldType;
			}
			else
			{
				return;
			}

			var basicInfo = BoisTypeCache.GetBasicType(memberType);

			if (basicInfo.KnownType != EnBasicKnownType.Unknown)
			{
				ReadRootObjectBasicMember(memberType, basicInfo, prop, field, il);
			}
			else
			{
				var complexTypeInfo = BoisTypeCache.GetComplexTypeUnCached(memberType);
				ReadRootObjectComplexMember(memberType, complexTypeInfo, prop, field, il);
			}
		}

		private static void ReadRootObjectComplexMember(Type memberType, BoisComplexTypeInfo complexTypeInfo, PropertyInfo prop, FieldInfo field, ILGenerator il)
		{
			switch (complexTypeInfo.ComplexKnownType)
			{
				case EnComplexKnownType.Collection:
					if (prop != null)
						EmitGenerator.ReadCollection(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.ReadCollection(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.Dictionary:
					if (prop != null)
						EmitGenerator.ReadDictionary(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.ReadDictionary(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.UnknownArray:
					if (prop != null)
						EmitGenerator.ReadUnknownArray(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.ReadUnknownArray(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.NameValueColl:
					if (prop != null)
						EmitGenerator.ReadNameValueColl(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.ReadNameValueColl(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.ISet:
					if (prop != null)
						EmitGenerator.ReadISet(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.ReadISet(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.DataSet:
					if (prop != null)
						EmitGenerator.ReadDataSet(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.ReadDataSet(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.DataTable:
					if (prop != null)
						EmitGenerator.ReadDataTable(prop, il, complexTypeInfo.IsNullable);
					else
						EmitGenerator.ReadDataTable(field, il, complexTypeInfo.IsNullable);
					break;

				case EnComplexKnownType.Unknown:
				default:
					return;
			}
		}

		private static void ReadRootObjectBasicMember(Type memberType, BoisBasicTypeInfo basicInfo, PropertyInfo prop, FieldInfo field, ILGenerator il)
		{
			switch (basicInfo.KnownType)
			{
				case EnBasicKnownType.String:
					if (prop != null)
						EmitGenerator.ReadString(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadString(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Bool:
					if (prop != null)
						EmitGenerator.ReadBool(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadBool(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Int16:
					if (prop != null)
						EmitGenerator.ReadInt16(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadInt16(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Int32:
					if (prop != null)
						EmitGenerator.ReadInt32(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadInt32(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Int64:
					if (prop != null)
						EmitGenerator.ReadInt64(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadInt64(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.UInt16:
					if (prop != null)
						EmitGenerator.ReadUInt16(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadUInt16(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.UInt32:
					if (prop != null)
						EmitGenerator.ReadUInt32(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadUInt32(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.UInt64:
					if (prop != null)
						EmitGenerator.ReadUInt64(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadUInt64(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Double:
					if (prop != null)
						EmitGenerator.ReadDouble(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadDouble(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Decimal:
					if (prop != null)
						EmitGenerator.ReadDecimal(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadDecimal(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Single:
					if (prop != null)
						EmitGenerator.ReadFloat(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadFloat(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Byte:
					if (prop != null)
						EmitGenerator.ReadByte(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadByte(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.SByte:
					if (prop != null)
						EmitGenerator.ReadSByte(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadSByte(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.DateTime:
					if (prop != null)
						EmitGenerator.ReadDateTime(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadDateTime(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.DateTimeOffset:
					if (prop != null)
						EmitGenerator.ReadDateTimeOffset(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadDateTimeOffset(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.KnownTypeArray:
					// TODO:
					break;

				case EnBasicKnownType.ByteArray:
					if (prop != null)
						EmitGenerator.ReadByteArray(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadByteArray(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Enum:
					if (prop != null)
						EmitGenerator.ReadEnum(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadEnum(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.TimeSpan:
					if (prop != null)
						EmitGenerator.ReadTimeSpan(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadTimeSpan(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Char:
					if (prop != null)
						EmitGenerator.ReadChar(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadChar(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Guid:
					if (prop != null)
						EmitGenerator.ReadGuid(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadGuid(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Color:
					if (prop != null)
						EmitGenerator.ReadColor(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadColor(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.DbNull:
					if (prop != null)
						EmitGenerator.ReadDbNull(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadDbNull(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Uri:
					if (prop != null)
						EmitGenerator.ReadUri(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadUri(field, il, basicInfo.IsNullable);
					break;

				case EnBasicKnownType.Version:
					if (prop != null)
						EmitGenerator.ReadVersion(prop, il, basicInfo.IsNullable);
					else
						EmitGenerator.ReadVersion(field, il, basicInfo.IsNullable);
					break;

				default:
				case EnBasicKnownType.Unknown:
					return;
			}
		}
	}
}
