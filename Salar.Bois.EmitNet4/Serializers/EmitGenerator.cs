using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Salar.Bois.Types;

// ReSharper disable AssignNullToNotNullAttribute

namespace Salar.Bois.Serializers
{
	class EmitGenerator
	{
		#region Write Root Complex Types

		internal static void WriteRootISet(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{

		}

		internal static void WriteRootUnknownArray(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{


		}

		internal static void WriteRootDataTable(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void WriteRootDataSet(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void WriteRootCollection(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void WriteRootList(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void WriteRootDictionary(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void WriteRootNameValueCol(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}


		#endregion

		#region Write Simple Types

		internal static void WriteString(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

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

		internal static void WriteString(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteBool(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteBool(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteInt16(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteInt16(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteInt32(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(int?) } : new[] { typeof(int) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue), methodArg));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteInt32(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(int?) } : new[] { typeof(int) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue), methodArg));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteInt64(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteInt64(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteUInt16(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteUInt16(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteUInt32(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteUInt32(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteUInt64(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteUInt64(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDouble(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDouble(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDecimal(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDecimal(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteFloat(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteFloat(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteByte(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteByte(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteSByte(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteSByte(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDateTime(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDateTime(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDateTimeOffset(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDateTimeOffset(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteByteArray(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteByteArray(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteEnum(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteEnum(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteTimeSpan(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteTimeSpan(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteChar(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteChar(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteGuid(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteGuid(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteColor(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteColor(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDbNull(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDbNull(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteUri(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteUri(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteVersion(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteVersion(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		#endregion

		#region Write Complex Types


		internal static void WriteCollection(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDCollection(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDictionary(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDictionary(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteUnknownArray(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteUnknownArray(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteNameValueColl(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteNameValueColl(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteISet(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteISet(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDataSet(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDataSet(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDataTable(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			
		}

		internal static void WriteDataTable(FieldInfo field, ILGenerator il, bool nullable)
		{
			
		}


		#endregion

		#region Read Root Complex Types

		
		internal static void ReadRootCollection(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void ReadRootDictionary(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void ReadRootUnknownArray(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void ReadRootNameValueColl(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void ReadRootISet(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void ReadRootDataSet(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		internal static void ReadRootDataTable(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			
		}

		#endregion


		#region Read Simple Types

		internal static void ReadString(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

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

		internal static void ReadString(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadBool(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadBool(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadInt16(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadInt16(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadInt32(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadInt32(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadInt64(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadInt64(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadUInt16(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadUInt16(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadUInt32(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadUInt32(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadUInt64(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadUInt64(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadDouble(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadDouble(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadDecimal(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadDecimal(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadFloat(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadFloat(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadByte(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadByte(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadSByte(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadSByte(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadDateTime(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadDateTime(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadDateTimeOffset(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadDateTimeOffset(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadByteArray(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadByteArray(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadEnum(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadEnum(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadTimeSpan(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadTimeSpan(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadChar(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadChar(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadGuid(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadGuid(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadColor(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadColor(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadDbNull(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadDbNull(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadUri(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadUri(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadVersion(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			
		}

		internal static void ReadVersion(FieldInfo field, ILGenerator il, bool isNullable)
		{
			
		}


		#endregion


		#region Read Complex Types

		internal static void ReadCollection(PropertyInfo prop, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadCollection(FieldInfo field, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadDictionary(PropertyInfo prop, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadDictionary(FieldInfo field, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadUnknownArray(PropertyInfo prop, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadUnknownArray(FieldInfo field, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadNameValueColl(PropertyInfo prop, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadNameValueColl(FieldInfo field, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadISet(PropertyInfo prop, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadISet(FieldInfo field, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadDataSet(PropertyInfo prop, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadDataSet(FieldInfo field, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadDataTable(PropertyInfo prop, ILGenerator il, bool nullable)
		{

		}

		internal static void ReadDataTable(FieldInfo field, ILGenerator il, bool nullable)
		{

		}


		#endregion
	}

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

}
