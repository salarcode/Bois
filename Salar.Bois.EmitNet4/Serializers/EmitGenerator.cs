using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
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
		internal static void WriteInt16(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(short?) } : new[] { typeof(BinaryWriter), typeof(short) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteInt16(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(short?) } : new[] { typeof(BinaryWriter), typeof(short) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteInt32(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(int?) } : new[] { typeof(BinaryWriter), typeof(int) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteInt32(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(int?) } : new[] { typeof(BinaryWriter), typeof(int) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteInt64(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(long?) } : new[] { typeof(BinaryWriter), typeof(long) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteInt64(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(long?) } : new[] { typeof(BinaryWriter), typeof(long) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUInt16(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(ushort?) } : new[] { typeof(BinaryWriter), typeof(ushort) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUInt16(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(ushort?) } : new[] { typeof(BinaryWriter), typeof(ushort) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUInt32(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(uint?) } : new[] { typeof(BinaryWriter), typeof(uint) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUInt32(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(uint?) } : new[] { typeof(BinaryWriter), typeof(uint) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUInt64(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(ulong?) } : new[] { typeof(BinaryWriter), typeof(ulong) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUInt64(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(ulong?) } : new[] { typeof(BinaryWriter), typeof(ulong) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDouble(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(double?) } : new[] { typeof(BinaryWriter), typeof(double) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarDecimal),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDouble(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(double?) } : new[] { typeof(BinaryWriter), typeof(double) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarDecimal),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDecimal(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(decimal?) } : new[] { typeof(BinaryWriter), typeof(decimal) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarDecimal),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDecimal(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(decimal?) } : new[] { typeof(BinaryWriter), typeof(decimal) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarDecimal),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteFloat(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(float?) } : new[] { typeof(BinaryWriter), typeof(float) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarDecimal),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteFloat(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(float?) } : new[] { typeof(BinaryWriter), typeof(float) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarDecimal),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteByte(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);

			if (nullable)
			{
				var methodArg = new[] { typeof(BinaryWriter), typeof(byte?) };
				il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write),
						BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { typeof(byte) }, null));
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteByte(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);

			if (nullable)
			{
				var methodArg = new[] { typeof(BinaryWriter), typeof(byte?) };
				il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write),
						BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { typeof(byte) }, null));
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteSByte(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);

			if (nullable)
			{
				var methodArg = new[] { typeof(BinaryWriter), typeof(sbyte?) };
				il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write),
						BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { typeof(sbyte) }, null));
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteSByte(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);

			if (nullable)
			{
				var methodArg = new[] { typeof(BinaryWriter), typeof(sbyte?) };
				il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write),
						BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { typeof(sbyte) }, null));
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteString(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
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
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field); // field value
			il.Emit(OpCodes.Ldarg_2); // Encoding
			var methodArg = new[] { typeof(BinaryWriter), typeof(string), typeof(Encoding) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteBool(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(bool?) } : new[] { typeof(BinaryWriter), typeof(bool) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteBool(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(bool?) } : new[] { typeof(BinaryWriter), typeof(bool) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDateTime(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(DateTime?) } : new[] { typeof(BinaryWriter), typeof(DateTime) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDateTime(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(DateTime?) } : new[] { typeof(BinaryWriter), typeof(DateTime) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDateTimeOffset(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(DateTimeOffset?) } : new[] { typeof(BinaryWriter), typeof(DateTimeOffset) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDateTimeOffset(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(DateTimeOffset?) } : new[] { typeof(BinaryWriter), typeof(DateTimeOffset) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteEnum(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			il.Emit(OpCodes.Box, prop.PropertyType);

			var methodArg = new[] { typeof(BinaryWriter), typeof(Enum) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteEnum(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			il.Emit(OpCodes.Box, field.FieldType);


			var methodArg = new[] { typeof(BinaryWriter), typeof(Enum) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteTimeSpan(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(TimeSpan?) } : new[] { typeof(BinaryWriter), typeof(TimeSpan) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteTimeSpan(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(TimeSpan?) } : new[] { typeof(BinaryWriter), typeof(TimeSpan) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteChar(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(char?) } : new[] { typeof(BinaryWriter), typeof(char) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteChar(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(char?) } : new[] { typeof(BinaryWriter), typeof(char) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteGuid(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(Guid?) } : new[] { typeof(BinaryWriter), typeof(Guid) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteGuid(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(Guid?) } : new[] { typeof(BinaryWriter), typeof(Guid) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteColor(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(Color?) } : new[] { typeof(BinaryWriter), typeof(Color) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteColor(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = nullable ? new[] { typeof(BinaryWriter), typeof(Color?) } : new[] { typeof(BinaryWriter), typeof(Color) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDbNull(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = new[] { typeof(BinaryWriter), typeof(DBNull) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDbNull(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = new[] { typeof(BinaryWriter), typeof(DBNull) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUri(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = new[] { typeof(BinaryWriter), typeof(Uri) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUri(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = new[] { typeof(BinaryWriter), typeof(Uri) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteVersion(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = new[] { typeof(BinaryWriter), typeof(Version) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteVersion(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = new[] { typeof(BinaryWriter), typeof(Version) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteByteArray(PropertyInfo prop, ILGenerator il, bool nullable)
		{
			var getter = prop.GetGetMethod(true);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Callvirt, meth: getter);
			var methodArg = new[] { typeof(BinaryWriter), typeof(byte[]) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteByteArray(FieldInfo field, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.Emit(OpCodes.Ldarg_1); // instance
			il.Emit(OpCodes.Ldfld, field: field);
			var methodArg = new[] { typeof(BinaryWriter), typeof(byte[]) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
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

		internal static void WriteUnknownArray(PropertyInfo prop, FieldInfo field, ILGenerator il, bool nullable)
		{
			/*
			var arr = instance.UnknownArray1;
			if (arr == null)
			{
				PrimitiveWriter.WriteNullValue(writer);
			}
			else
			{
				NumericSerializers.WriteVarInt(writer, arr.Length);

				var arrEnumurator = arr.GetEnumerator();
				while (arrEnumurator.MoveNext())
				{
					PrimitiveWriter.WriteValue(writer, (string)arrEnumurator.Current, encoding);
				}
			}
			 */
			Type arrayType;
			var actualCode = il.DefineLabel();
			var codeEnds = il.DefineLabel();
			var loopStart = il.DefineLabel();

			LocalBuilder instanceVar;

			// var arr = instance.UnknownArray1;
			il.Emit(OpCodes.Ldarg_1); // instance

			if (prop != null)
			{
				arrayType = prop.PropertyType;

				instanceVar = il.DeclareLocal(arrayType);
				var getter = prop.GetGetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else
			{
				arrayType = field.FieldType;

				instanceVar = il.DeclareLocal(arrayType);
				il.Emit(OpCodes.Ldfld, field: field);
			}
			il.StoreLocal(instanceVar);

			// item type
			var arrItemType = arrayType.GetElementType();

			// if (coll == null)
			il.LoadLocal(instanceVar); // instance arr
			il.Emit(OpCodes.Ldnull); // null
			il.Emit(OpCodes.Ceq); // ==

			il.Emit(OpCodes.Brfalse_S, actualCode); // jump to not null
			il.Emit(OpCodes.Nop);

			// PrimitiveWriter.WriteNullValue(writer);
			{
				il.Emit(OpCodes.Ldarg_0); // BinaryWriter
				il.Emit(OpCodes.Call,
					typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteNullValue),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
				il.Emit(OpCodes.Nop);

				il.Emit(OpCodes.Br_S, codeEnds);
				il.Emit(OpCodes.Nop);
			}

			// NumericSerializers.WriteVarInt(writer, arr.Length);
			il.MarkLabel(actualCode);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.LoadLocal(instanceVar); // instance arr
			il.Emit(OpCodes.Ldlen);
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Call,
				meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BinaryWriter), typeof(int) }, null));
			il.Emit(OpCodes.Nop);


			// IEnumerator enumerator = arr.GetEnumerator();
			il.LoadLocal(instanceVar); // instance coll
			var getEnumeratorMethodInfo = typeof(Array).GetMethod(nameof(Array.GetEnumerator),
				BindingFlags.Instance | BindingFlags.Public);
			il.Emit(OpCodes.Callvirt, getEnumeratorMethodInfo);

			var enumuratorType = getEnumeratorMethodInfo.ReturnType;
			var enumurator = il.DeclareLocal(enumuratorType);
			il.Emit(OpCodes.Stloc, enumurator);

			//while (enumerator.MoveNext())
			{
				il.MarkLabel(loopStart);

				il.LoadLocal(enumurator);
				il.Emit(OpCodes.Callvirt,
					enumuratorType.GetMethod(nameof(IEnumerator.MoveNext)));
				il.Emit(OpCodes.Brfalse_S, codeEnds);

				var typeInfo = BoisTypeCache.GetRootTypeComputed(arrItemType, false, true);

				il.Emit(OpCodes.Ldarg_0); // BinaryWriter
				il.LoadLocal(enumurator);
				il.Emit(OpCodes.Callvirt,
					// ReSharper disable once PossibleNullReferenceException
					enumuratorType.GetProperty(nameof(IEnumerator.Current)).GetGetMethod());
				il.Emit(OpCodes.Castclass, arrItemType);
				il.Emit(OpCodes.Ldarg_2); // Encoding
				il.Emit(OpCodes.Call, meth: typeInfo.WriterMethod);

				il.Emit(OpCodes.Nop);
				il.Emit(OpCodes.Br_S, loopStart);
			}
			il.MarkLabel(codeEnds);

		}

		internal static void WriteNameValueColl(PropertyInfo prop, FieldInfo field, ILGenerator il, bool nullable)
		{
			/*
			var coll = instance.NameValueCollection;
			if (coll == null)
			{
				PrimitiveWriter.WriteNullValue(writer);
			}
			else
			{
				NumericSerializers.WriteVarInt(writer, coll.Count);

				var collEnumurator = coll.GetEnumerator();
				while (collEnumurator.MoveNext())
				{
					string key = (string) collEnumurator.Current;
					PrimitiveWriter.WriteValue(writer, key, encoding);
					PrimitiveWriter.WriteValue(writer, instance.NameValueCollection[key], encoding);
				}
			}
			 */

			var actualCode = il.DefineLabel();
			var codeEnds = il.DefineLabel();
			var loopStart = il.DefineLabel();

			var instanceVar = il.DeclareLocal(typeof(NameValueCollection));

			// NameValueCollection coll = instance.NameValueCollection;
			il.Emit(OpCodes.Ldarg_1); // instance

			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Ldfld, field: field);
			}
			il.StoreLocal(instanceVar);

			// if (coll == null)
			il.LoadLocal(instanceVar); // instance coll
			il.Emit(OpCodes.Ldnull); // null
			il.Emit(OpCodes.Ceq); // ==

			il.Emit(OpCodes.Brfalse_S, actualCode); // jump to not null
			il.Emit(OpCodes.Nop);


			// PrimitiveWriter.WriteNullValue(writer);
			{
				il.Emit(OpCodes.Ldarg_0); // BinaryWriter
				il.Emit(OpCodes.Call,
					typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteNullValue),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
				il.Emit(OpCodes.Nop);

				il.Emit(OpCodes.Br_S, codeEnds);
				il.Emit(OpCodes.Nop);
			}

			// NumericSerializers.WriteVarInt(writer, coll.Count);
			il.MarkLabel(actualCode);

			il.Emit(OpCodes.Ldarg_0); // BinaryWriter
			il.LoadLocal(instanceVar); // instance coll
			il.Emit(OpCodes.Callvirt,
				// ReSharper disable once PossibleNullReferenceException
				meth: typeof(NameValueCollection).GetProperty(nameof(NameValueCollection.Count)).GetGetMethod());
			il.Emit(OpCodes.Call,
				meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BinaryWriter), typeof(int) }, null));
			il.Emit(OpCodes.Nop);



			// IEnumerator enumerator = nameValueCollection.GetEnumerator();
			il.LoadLocal(instanceVar); // instance coll
			var getEnumeratorMethodInfo = typeof(NameValueCollection).GetMethod(nameof(NameValueCollection.GetEnumerator),
				BindingFlags.Instance | BindingFlags.Public);
			il.Emit(OpCodes.Callvirt, getEnumeratorMethodInfo);

			var enumuratorType = getEnumeratorMethodInfo.ReturnType;
			var enumurator = il.DeclareLocal(enumuratorType);
			il.Emit(OpCodes.Stloc, enumurator);


			//while (enumerator.MoveNext())
			{
				il.MarkLabel(loopStart);

				il.LoadLocal(enumurator);
				il.Emit(OpCodes.Callvirt,
					enumuratorType.GetMethod(nameof(IEnumerator.MoveNext)));
				il.Emit(OpCodes.Brfalse_S, codeEnds);

				// foreach (*string item2* in nameValueCollection)
				var itemKeyVar = il.DeclareLocal(typeof(string));
				il.LoadLocal(enumurator);
				il.Emit(OpCodes.Callvirt,
					// ReSharper disable once PossibleNullReferenceException
					enumuratorType.GetProperty(nameof(IEnumerator.Current)).GetGetMethod());
				il.Emit(OpCodes.Castclass, typeof(string));
				il.Emit(OpCodes.Stloc, itemKeyVar);
				il.Emit(OpCodes.Nop);

				// PrimitiveWriter.WriteValue(writer, item, encoding);
				il.Emit(OpCodes.Ldarg_0); // BinaryWriter
				il.LoadLocal(itemKeyVar); // item
				il.Emit(OpCodes.Ldarg_2); // Encoding
				il.Emit(OpCodes.Call,
					meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
						BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder,
						new[] { typeof(BinaryWriter), typeof(string), typeof(Encoding) }, null));
				il.Emit(OpCodes.Nop);

				// PrimitiveWriter.WriteValue(writer, coll[item], encoding);
				il.Emit(OpCodes.Ldarg_0); // BinaryWriter
				il.LoadLocal(instanceVar); // instance coll
				il.LoadLocal(itemKeyVar); // item
				il.Emit(OpCodes.Callvirt,
					meth: typeof(NameValueCollection).GetMethod(nameof(NameValueCollection.Get),
						BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { typeof(string) }, null));
				il.Emit(OpCodes.Ldarg_2); // Encoding
				il.Emit(OpCodes.Call,
					meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
						BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder,
						new[] { typeof(BinaryWriter), typeof(string), typeof(Encoding) }, null));
				il.Emit(OpCodes.Nop);
				il.Emit(OpCodes.Br_S, loopStart);
			}
			il.MarkLabel(codeEnds);
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

		internal static void ReadInt16(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt16Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt16),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadInt16(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt16Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt16),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadInt32(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt32Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt32),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadInt32(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt32Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt32),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadInt64(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt64Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt64),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadInt64(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt64Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt64),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUInt16(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt16Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt16),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUInt16(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt16Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt16),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUInt32(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt32Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt32),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUInt32(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt32Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt32),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUInt64(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt64Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt64),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUInt64(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt64Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt64),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDouble(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDoubleNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDouble),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDouble(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDoubleNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDouble),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDecimal(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDecimalNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDecimal),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDecimal(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDecimalNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDecimal),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadFloat(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarSingleNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarSingle),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadFloat(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarSingleNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarSingle),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadByte(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			if (isNullable)
			{
				il.Emit(OpCodes.Call,
					meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarByteNullable),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
					new[] { typeof(BinaryReader) }, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadByte)));
			}

			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadByte(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			if (isNullable)
			{
				il.Emit(OpCodes.Call,
					meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarByteNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadByte)));
			}

			il.Emit(OpCodes.Stfld, field); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadSByte(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			if (isNullable)
			{
				il.Emit(OpCodes.Call,
					meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarSByteNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadSByte)));
			}

			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadSByte(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			if (isNullable)
			{
				il.Emit(OpCodes.Call,
					meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarSByteNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadSByte)));
			}

			il.Emit(OpCodes.Stfld, field); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadString(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
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
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader
			il.Emit(OpCodes.Ldarg_1); // Encoding

			var methodArg = new[] { typeof(BinaryReader), typeof(Encoding) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadString),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadBool(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadBooleanNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadBoolean),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadBool(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadBooleanNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadBoolean),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDateTime(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTimeNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTime),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDateTime(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTimeNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTime),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDateTimeOffset(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTimeOffsetNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTimeOffset),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDateTimeOffset(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTimeOffsetNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTimeOffset),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadEnum(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var methodArg = new[] { typeof(BinaryReader) };
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadEnumGeneric),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null)
					.MakeGenericMethod(prop.PropertyType));

			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadEnum(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader


			var methodArg = new[] { typeof(BinaryReader) };
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadEnumGeneric),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null)
						.MakeGenericMethod(field.FieldType));

			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadTimeSpan(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadTimeSpanNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadTimeSpan),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadTimeSpan(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadTimeSpanNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadTimeSpan),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadChar(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadCharNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadChar),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadChar(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadCharNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadChar),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadGuid(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadGuidNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadGuid),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadGuid(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadGuidNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadGuid),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadColor(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadColorNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadColor),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadColor(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadColorNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadColor),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDbNull(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var methodArg = new[] { typeof(BinaryReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDbNull),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDbNull(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var methodArg = new[] { typeof(BinaryReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDbNull),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUri(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var methodArg = new[] { typeof(BinaryReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadUri),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUri(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var methodArg = new[] { typeof(BinaryReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadUri),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadVersion(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var methodArg = new[] { typeof(BinaryReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadVersion),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadVersion(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var methodArg = new[] { typeof(BinaryReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadVersion),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
		}


		internal static void ReadByteArray(PropertyInfo prop, ILGenerator il, bool isNullable)
		{
			var setter = prop.GetSetMethod(true);

			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var methodArg = new[] { typeof(BinaryReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadByteArray),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Callvirt, meth: setter); // property value
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadByteArray(FieldInfo field, ILGenerator il, bool isNullable)
		{
			il.Emit(OpCodes.Ldloc_0); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryReader

			var methodArg = new[] { typeof(BinaryReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadByteArray),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Stfld, field: field); // field value
			il.Emit(OpCodes.Nop);
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

		internal static void StoreLocal(this ILGenerator il, LocalBuilder local)
		{
			switch (local.LocalIndex)
			{
				case 0: il.Emit(OpCodes.Stloc_0); break;
				case 1: il.Emit(OpCodes.Stloc_1); break;
				case 2: il.Emit(OpCodes.Stloc_2); break;
				case 3: il.Emit(OpCodes.Stloc_3); break;
				default:
					if (local.LocalIndex < 256)
					{
						il.Emit(OpCodes.Stloc_S, (byte)local.LocalIndex);
					}
					else
					{
						il.Emit(OpCodes.Stloc, (ushort)local.LocalIndex);
					}
					break;
			}
		}

	}

}
