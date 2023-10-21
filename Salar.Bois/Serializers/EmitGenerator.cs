using Salar.BinaryBuffers;
using Salar.Bois.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
// ReSharper disable InconsistentNaming

// ReSharper disable AssignNullToNotNullAttribute

namespace Salar.Bois.Serializers
{
	static class EmitGenerator
	{

		#region Write Root Complex Types

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteRootUnknownArray(Type containerType, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			WriteUnknownArray(null, null, () =>
				{
					il.LoadArgAuto(1, containerType); // instance
					return containerType;
				},
				containerType,
				il, typeInfo.IsNullable);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteRootCollection(Type containerType, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			WriteCollection(null, null, () =>
				{
					il.LoadArgAuto(1, containerType); // instance
					return containerType;
				},
				containerType,
				il, typeInfo.IsNullable);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteRootISet(Type containerType, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			WriteRootCollection(containerType, typeInfo, il);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteRootList(Type containerType, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			WriteRootCollection(containerType, typeInfo, il);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteRootDictionary(Type containerType, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			WriteDictionary(null, null, () =>
				{
					il.LoadArgAuto(1, containerType); // instance
					return containerType;
				},
				containerType,
				il, typeInfo.IsNullable);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteRootNameValueCol(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			WriteNameValueColl(null, null, () =>
				{
					il.LoadArgAuto(1, type); // instance
					return type;
				},
				il, typeInfo.IsNullable);
		}

		#endregion

		#region Write Simple Types
		internal static void WriteInt16(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(short?) } : new[] { typeof(BufferWriterBase), typeof(short) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteInt32(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(int?) } : new[] { typeof(BufferWriterBase), typeof(int) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteInt64(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(long?) } : new[] { typeof(BufferWriterBase), typeof(long) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUInt16(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(ushort?) } : new[] { typeof(BufferWriterBase), typeof(ushort) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUInt32(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(uint?) } : new[] { typeof(BufferWriterBase), typeof(uint) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteUInt64(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(ulong?) } : new[] { typeof(BufferWriterBase), typeof(ulong) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDouble(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(double?) } : new[] { typeof(BufferWriterBase), typeof(double) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarDecimal),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDecimal(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader,
			Type containerType, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(decimal?) } : new[] { typeof(BufferWriterBase), typeof(decimal) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarDecimal),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteFloat(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(float?) } : new[] { typeof(BufferWriterBase), typeof(float) };
			il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarDecimal),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteByte(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			if (nullable)
			{
				var methodArg = new[] { typeof(BufferWriterBase), typeof(byte?) };
				il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BufferWriterBase).GetMethod(nameof(BufferWriterBase.Write),
						BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { typeof(byte) }, null));
			}
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteSByte(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}


			if (nullable)
			{
				var methodArg = new[] { typeof(BufferWriterBase), typeof(sbyte?) };
				il.Emit(OpCodes.Call, meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteVarInt),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BufferWriterBase).GetMethod(nameof(BufferWriterBase.Write),
						BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { typeof(sbyte) }, null));
			}
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteString(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			il.Emit(OpCodes.Ldarg_2); // Encoding
			var methodArg = new[] { typeof(BufferWriterBase), typeof(string), typeof(Encoding) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteBool(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(bool?) } : new[] { typeof(BufferWriterBase), typeof(bool) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDateTime(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader,
			Type containerType, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(DateTime?) } : new[] { typeof(BufferWriterBase), typeof(DateTime) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteDateTimeOffset(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader,
			Type containerType, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(DateTimeOffset?) } : new[] { typeof(BufferWriterBase), typeof(DateTimeOffset) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteEnum(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			Type itemType;

			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				itemType = prop.PropertyType;
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				itemType = field.FieldType;
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				itemType = valueLoader();
			}

			il.Emit(OpCodes.Box, itemType);
			if (nullable)
				il.Emit(OpCodes.Ldc_I4_1);
			else
				il.Emit(OpCodes.Ldc_I4_0);

			var methodArg = new[] { typeof(BufferWriterBase), typeof(Enum), typeof(bool) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteTimeSpan(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader,
			Type containerType, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(TimeSpan?) } : new[] { typeof(BufferWriterBase), typeof(TimeSpan) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteChar(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(char?) } : new[] { typeof(BufferWriterBase), typeof(char) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteGuid(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(Guid?) } : new[] { typeof(BufferWriterBase), typeof(Guid) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		internal static void WriteColor(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = nullable ? new[] { typeof(BufferWriterBase), typeof(Color?) } : new[] { typeof(BufferWriterBase), typeof(Color) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteDbNull(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = new[] { typeof(BufferWriterBase), typeof(DBNull) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteUri(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType,
			ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = new[] { typeof(BufferWriterBase), typeof(Uri) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteVersion(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader,
			Type containerType, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = new[] { typeof(BufferWriterBase), typeof(Version) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}


		internal static void WriteByteArray(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader,
			Type containerType, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}

			var methodArg = new[] { typeof(BufferWriterBase), typeof(byte[]) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteKnownTypeArray(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader,
			Type containerType, ILGenerator il, bool nullable)
		{
			WriteUnknownArray(prop, field, valueLoader, containerType, il, nullable);
		}
		#endregion

		#region Write Complex Types

		internal static void WriteUnknownComplexTypeCall(Type memberType, PropertyInfo prop, FieldInfo field, ILGenerator il, Type containerType, BoisComplexTypeInfo complexTypeInfo)
		{
			var nullableBareType = Nullable.GetUnderlyingType(memberType);
			var memberIsStruct = memberType.IsExplicitStruct();

			var LabelWriteNull = il.DefineLabel();
			var LabelEndOfCode = il.DefineLabel();
			var hasWriteNullValue = false;

			if (nullableBareType != null)
			{
				LocalBuilder tempValueCheck;

				// CODE-FOR: if (instance.prop.HasValue)
				if (prop != null)
				{
					var getter = prop.GetGetMethod(true);
					tempValueCheck = il.DeclareLocal(getter.ReturnType);


					il.LoadArgAuto(1, containerType); // instance
					il.Emit(OpCodes.Callvirt, meth: getter);
					il.StoreLocal(tempValueCheck);

					il.LoadLocalAddress(tempValueCheck);
					il.Emit(OpCodes.Call,
						// ReSharper disable once PossibleNullReferenceException
						meth: getter.ReturnType.GetProperty(nameof(Nullable<uint>.HasValue)).GetGetMethod());
				}
				else
				{
					tempValueCheck = il.DeclareLocal(field.FieldType);

					il.LoadArgAuto(1, containerType); // instance
					il.Emit(OpCodes.Ldfld, field: field);
					il.StoreLocal(tempValueCheck);

					il.LoadLocalAddress(tempValueCheck);
					il.Emit(OpCodes.Call,
						// ReSharper disable once PossibleNullReferenceException
						meth: field.FieldType.GetProperty(nameof(Nullable<uint>.HasValue)).GetGetMethod());
				}
				il.Emit(OpCodes.Brfalse_S, LabelWriteNull);
				hasWriteNullValue = true;

				// IMPORTANT: because we need to detect whether the object is null or not we need this byte
				// CODE-FOR: writer.Write((byte)0);
				il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
				il.Emit(OpCodes.Ldc_I4_0);
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BufferWriterBase).GetMethod(nameof(BufferWriterBase.Write),
						BindingFlags.Instance | BindingFlags.Public,
						Type.DefaultBinder, new[] { typeof(byte) }, null));
				il.Emit(OpCodes.Nop);

				// write value
				{
					var valueTypeInfo = BoisTypeCache.GetRootTypeComputed(nullableBareType, false, true);


					// CODE-FOR: computed_function_name(tempValueCheck.Value);
					il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
					il.LoadLocalAddress(tempValueCheck);

					// int value = num.Value;
					il.Emit(OpCodes.Call,
						// ReSharper disable once PossibleNullReferenceException
						meth: tempValueCheck.LocalType.GetProperty(nameof(Nullable<uint>.Value)).GetGetMethod());

					il.Emit(OpCodes.Ldarg_2); // Encoding
					il.Emit(OpCodes.Call, meth: valueTypeInfo.WriterMethod);
				}
			}
			else
			{
				if (!memberIsStruct)
				{
					// reference type
					// the underlying type is not struct

					// for complex types, a method is generated
					var valueTypeInfo = BoisTypeCache.GetRootTypeComputed(memberType, false, true);

					// CODE-FOR: if (instance.prop != null)
					if (prop != null)
					{
						var getter = prop.GetGetMethod(true);
						il.LoadArgAuto(1, containerType); // instance
						il.Emit(OpCodes.Callvirt, meth: getter);
					}
					else
					{
						il.LoadArgAuto(1, containerType); // instance
						il.Emit(OpCodes.Ldfld, field: field);
					}

					il.Emit(OpCodes.Ldnull);
					il.Emit(OpCodes.Cgt_Un);
					il.Emit(OpCodes.Brfalse_S, LabelWriteNull);
					hasWriteNullValue = true;

					// IMPORTANT: because we need to detect whether the object is null or not we need this byte
					// CODE-FOR: writer.Write((byte)0);
					il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Callvirt,
						meth: typeof(BufferWriterBase).GetMethod(nameof(BufferWriterBase.Write),
							BindingFlags.Instance | BindingFlags.Public,
							Type.DefaultBinder, new[] { typeof(byte) }, null));
					il.Emit(OpCodes.Nop);

					// write value
					{
						// CODE-FOR: Calling the emitted writer
						il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
						if (prop != null)
						{
							var getter = prop.GetGetMethod(true);
							il.LoadArgAuto(1, containerType); // instance
							il.Emit(OpCodes.Callvirt, meth: getter);
						}
						else
						{
							il.LoadArgAuto(1, containerType); // instance
							il.Emit(OpCodes.Ldfld, field: field);
						}
						il.Emit(OpCodes.Ldarg_2); // Encoding
						il.Emit(OpCodes.Call, meth: valueTypeInfo.WriterMethod);
					}

				}
				else
				{
					// no null check struct
				}
			}

			if (hasWriteNullValue)
			{
				// CODE-FOR: else
				il.Emit(OpCodes.Br_S, LabelEndOfCode);
				{
					il.MarkLabel(LabelWriteNull);

					// CODE-FOR: PrimitiveWriter.WriteNullValue(writer);
					il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
					il.Emit(OpCodes.Call,
						typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteNullValue),
							BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
					il.Emit(OpCodes.Nop);
				}
			}

			il.MarkLabel(LabelEndOfCode);
		}

		internal static void WriteCollection(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType, ILGenerator il, bool nullable)
		{
			/*
			var list1 = instance.GenericList;
			if (list1 == null)
			{
				PrimitiveWriter.WriteNullValue(writer);
			}
			else
			{
				NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)list1.Count);

				using (var enumurator = list1.GetEnumerator())
					while (enumurator.MoveNext())
					{
						var item = enumurator.Current;
						NumericSerializers.WriteVarInt(writer, item);
					}
			}
			*/

			var actualCode = il.DefineLabel();
			var codeEnds = il.DefineLabel();
			var loopStart = il.DefineLabel();

			Type collectionType;

			// var dic  = instance.GenericDictionary;
			if (prop != null)
			{
				collectionType = prop.PropertyType;

				var getter = prop.GetGetMethod(true);
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				collectionType = field.FieldType;

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				collectionType = valueLoader();
			}
			var instanceVar = il.DeclareLocal(collectionType);
			il.StoreLocal(instanceVar);


			// if (dic == null)
			il.LoadLocalValue(instanceVar); // instance dic
			il.Emit(OpCodes.Brtrue_S, actualCode); // jump to not null


			// PrimitiveWriter.WriteNullValue(writer);
			{
				il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
				il.Emit(OpCodes.Call,
					typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteNullValue),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
				il.Emit(OpCodes.Br_S, codeEnds);
			}

			// NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)coll.Count);
			il.MarkLabel(actualCode);

			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			il.LoadLocalValue(instanceVar); // instance coll
			il.Emit(OpCodes.Callvirt,
				// ReSharper disable once PossibleNullReferenceException
				meth: collectionType.GetProperty(nameof(ICollection.Count)).GetGetMethod());
			il.Emit(OpCodes.Call,
				meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteUIntNullableMemberCount),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BufferWriterBase), typeof(uint) }, null));
			il.Emit(OpCodes.Nop);


			// IEnumerator enumerator = nameValueCollection.GetEnumerator();
			il.LoadLocalValue(instanceVar); // instance coll
			var getEnumeratorMethodInfo =
				collectionType.GetMethod(nameof(ICollection.GetEnumerator), BindingFlags.Instance | BindingFlags.Public) ??
				typeof(IEnumerable<>)
					.MakeGenericType(collectionType.GetGenericArguments()[0]).GetMethod(nameof(IEnumerable.GetEnumerator));
			il.Emit(OpCodes.Callvirt, getEnumeratorMethodInfo);

			var enumuratorType = getEnumeratorMethodInfo.ReturnType;
			var enumurator = il.DeclareLocal(enumuratorType);
			il.StoreLocal(enumurator);

			il.BeginExceptionBlock();
			{
				var blockEnd = il.DefineLabel();

				//while (enumerator.MoveNext())
				il.MarkLabel(loopStart);

				il.LoadLocalAuto(enumurator);
				var moveNextInfo = enumuratorType.GetMethod(nameof(IEnumerator.MoveNext));
				if (enumuratorType == typeof(IEnumerator))
				{
					il.Emit(OpCodes.Callvirt, moveNextInfo);
				}
				else
				{
					if (moveNextInfo != null)
					{
						il.Emit(OpCodes.Call, moveNextInfo);
					}
					else
					{
						moveNextInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
						il.Emit(OpCodes.Callvirt, moveNextInfo);
					}
				}


				il.Emit(OpCodes.Brfalse_S, blockEnd);

				// reading key-value type
				var genericTypes = enumuratorType.GetGenericArguments();
				Type valueType;
				if (genericTypes.Length == 0)
				{
					valueType = ReflectionHelper.FindUnderlyingGenericElementType(collectionType);
					if (valueType == null)
					{
						throw new InvalidTypeException(
							$"Collection type '{collectionType}' is not generic and only generic types are supported.");
					}
				}
				else
				{
					valueType = genericTypes[0];
				}

				// var item = arrEnumurator.Current;
				var dicItemType = valueType;
				var dicItemVar = il.DeclareLocal(dicItemType);
				il.LoadLocalAuto(enumurator);

				// ReSharper disable once PossibleNullReferenceException
				var getCurrentInfo = enumuratorType.GetProperty(nameof(IEnumerator.Current)).GetGetMethod();
				if (enumuratorType.IsValueType)
				{
					il.Emit(OpCodes.Call, getCurrentInfo);
				}
				else
				{
					il.Emit(OpCodes.Callvirt, getCurrentInfo);
				}
				if (getCurrentInfo.ReturnType != dicItemType)
				{
					// This is only if the return type is Object
					// If the return type is anything other Object this code is not tested for that
					if (getCurrentInfo.ReturnType == typeof(object))
					{
						il.Emit(OpCodes.Unbox_Any, dicItemType);
					}
					else
					{
						il.Emit(OpCodes.Box, dicItemType);
					}
				}

				il.StoreLocal(dicItemVar);


				// VALUE -------------
				var valueTypeBasicInfo = BoisTypeCache.GetBasicType(valueType);
				if (valueTypeBasicInfo.KnownType != EnBasicKnownType.Unknown)
				{
					BoisTypeCompiler.WriteBasicTypeDirectly(collectionType, il, valueTypeBasicInfo,
						() =>
						{
							// read the key
							il.LoadLocalValue(dicItemVar);

							return valueType;
						});
				}
				else
				{
					// for complex types, a method is generated
					var valueTypeInfo = BoisTypeCache.GetRootTypeComputed(valueType, false, true);

					il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
					il.LoadLocalValue(dicItemVar);
					il.Emit(OpCodes.Ldarg_2); // Encoding
					il.Emit(OpCodes.Call, meth: valueTypeInfo.WriterMethod);
				}

				// end of loop
				il.Emit(OpCodes.Br_S, loopStart);

				// end of block
				il.MarkLabel(blockEnd);
			}
			il.BeginFinallyBlock();
			{
				if (typeof(IDisposable).IsAssignableFrom(enumuratorType))
				{
					il.LoadLocalAuto(enumurator);
					if (enumuratorType.IsValueType)
						il.Emit(OpCodes.Constrained, enumuratorType);
					il.Emit(OpCodes.Callvirt,
						meth: typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose)));
				}
#if DEBUG
				else
				{
					il.Emit(OpCodes.Nop);
				}
#endif
				il.Emit(OpCodes.Nop);
			}
			il.EndExceptionBlock();
			il.MarkLabel(codeEnds);
		}

		internal static void WriteDictionary(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType, ILGenerator il, bool nullable)
		{
			/*
			var dic = instance.GenericDictionary;
			if (dic == null)
			{
				PrimitiveWriter.WriteNullValue(writer);
			}
			else
			{
				NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)dic.Count);

				using (var arrEnumurator = dic.GetEnumerator())
					while (arrEnumurator.MoveNext())
					{
						var item = arrEnumurator.Current;
						PrimitiveWriter.WriteValue(writer, item.Key, encoding);
						PrimitiveWriter.WriteValue(writer, item.Value, encoding);
					}
			}
			*/

			var actualCode = il.DefineLabel();
			var codeEnds = il.DefineLabel();
			var loopStart = il.DefineLabel();

			Type dictionaryType;

			// var dic  = instance.GenericDictionary;
			if (prop != null)
			{
				dictionaryType = prop.PropertyType;

				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				dictionaryType = field.FieldType;

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				dictionaryType = valueLoader();
			}
			var instanceVar = il.DeclareLocal(dictionaryType);
			il.StoreLocal(instanceVar);

			// if (dic == null)
			il.LoadLocalValue(instanceVar); // instance dic
			il.Emit(OpCodes.Brtrue_S, actualCode); // jump to not null

			// PrimitiveWriter.WriteNullValue(writer);
			{
				il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
				il.Emit(OpCodes.Call,
					typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteNullValue),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
				il.Emit(OpCodes.Br_S, codeEnds);
			}

			// NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)coll.Count);
			il.MarkLabel(actualCode);

			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			il.LoadLocalValue(instanceVar); // instance coll
			il.Emit(OpCodes.Callvirt,
				// ReSharper disable once PossibleNullReferenceException
				meth: dictionaryType.GetProperty(nameof(IDictionary.Count)).GetGetMethod());
			il.Emit(OpCodes.Call,
				meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteUIntNullableMemberCount),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BufferWriterBase), typeof(uint) }, null));
			il.Emit(OpCodes.Nop);


			// IEnumerator enumerator = nameValueCollection.GetEnumerator();
			il.LoadLocalValue(instanceVar); // instance coll
			var getEnumeratorMethodInfo = dictionaryType.GetMethod(nameof(IDictionary.GetEnumerator),
				BindingFlags.Instance | BindingFlags.Public);
			il.Emit(OpCodes.Callvirt, getEnumeratorMethodInfo);

			var enumuratorType = getEnumeratorMethodInfo.ReturnType;
			var enumurator = il.DeclareLocal(enumuratorType);
			il.StoreLocal(enumurator);

			il.BeginExceptionBlock();
			{
				var blockEnd = il.DefineLabel();

				//while (enumerator.MoveNext())
				il.MarkLabel(loopStart);

				il.LoadLocalAuto(enumurator);
				var moveNextInfo = enumuratorType.GetMethod(nameof(IEnumerator.MoveNext));
				if (moveNextInfo != null)
				{
					il.Emit(OpCodes.Call, moveNextInfo);
				}
				else
				{
					moveNextInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
					il.Emit(OpCodes.Callvirt, moveNextInfo);
				}

				il.Emit(OpCodes.Brfalse_S, blockEnd);

				// reading key-value type
				var genericTypes = enumuratorType.GetGenericArguments();
				if (genericTypes.Length < 2)
				{
					genericTypes = ReflectionHelper.FindUnderlyingGenericDictionaryElementType(dictionaryType);
					if (genericTypes == null)
					{
						var dictionaryBaseType = ReflectionHelper.FindUnderlyingGenericElementType(dictionaryType);
						genericTypes = dictionaryBaseType.GetGenericArguments();
					}
				}
				var keyType = genericTypes[0];
				var valueType = genericTypes[1];


				// var item = arrEnumurator.Current;
				var dicItemType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
				var dicItemVar = il.DeclareLocal(dicItemType);
				il.LoadLocalAuto(enumurator);
				// ReSharper disable once PossibleNullReferenceException
				var getCurrentInfo = enumuratorType.GetProperty(nameof(IEnumerator.Current)).GetGetMethod();
				if (enumuratorType.IsValueType)
				{
					il.Emit(OpCodes.Call, getCurrentInfo);
				}
				else
				{
					il.Emit(OpCodes.Callvirt, getCurrentInfo);
				}
				il.StoreLocal(dicItemVar);

				// KEY ---------------
				var keyTypeBasicInfo = BoisTypeCache.GetBasicType(keyType);
				if (keyTypeBasicInfo.KnownType != EnBasicKnownType.Unknown)
				{

					BoisTypeCompiler.WriteBasicTypeDirectly(dictionaryType, il, keyTypeBasicInfo,
						() =>
						{
							// read the key
							il.LoadLocalAuto(dicItemVar);
							il.Emit(OpCodes.Call,
								// ReSharper disable once PossibleNullReferenceException
								dicItemType.GetProperty("Key").GetGetMethod());

							return keyType;
						});
				}
				else
				{
					// for complex types, a method is generated
					var keyTypeInfo = BoisTypeCache.GetRootTypeComputed(keyType, false, true);

					il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
					il.LoadLocalAuto(dicItemVar);
					il.Emit(OpCodes.Call,
						// ReSharper disable once PossibleNullReferenceException
						dicItemType.GetProperty("Key").GetGetMethod());
					il.Emit(OpCodes.Ldarg_2); // Encoding
					il.Emit(OpCodes.Call, meth: keyTypeInfo.WriterMethod);
				}

				// VALUE -------------
				var valueTypeBasicInfo = BoisTypeCache.GetBasicType(valueType);
				if (valueTypeBasicInfo.KnownType != EnBasicKnownType.Unknown)
				{
					BoisTypeCompiler.WriteBasicTypeDirectly(dictionaryType, il, valueTypeBasicInfo,
						() =>
						{
							// read the key
							il.LoadLocalAuto(dicItemVar);
							il.Emit(OpCodes.Call,
								// ReSharper disable once PossibleNullReferenceException
								dicItemType.GetProperty("Value").GetGetMethod());

							return valueType;
						});
				}
				else
				{
					// for complex types, a method is generated
					var valueTypeInfo = BoisTypeCache.GetRootTypeComputed(valueType, false, true);

					il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
					il.LoadLocalAuto(dicItemVar);
					il.Emit(OpCodes.Call,
						// ReSharper disable once PossibleNullReferenceException
						dicItemType.GetProperty("Value").GetGetMethod());
					il.Emit(OpCodes.Ldarg_2); // Encoding
					il.Emit(OpCodes.Call, meth: valueTypeInfo.WriterMethod);
				}

				il.Emit(OpCodes.Br_S, loopStart);

				// end of block
				il.MarkLabel(blockEnd);
			}
			il.BeginFinallyBlock();
			{
				il.LoadLocalAuto(enumurator);
				if (enumuratorType.IsValueType)
					il.Emit(OpCodes.Constrained, enumuratorType);
				il.Emit(OpCodes.Callvirt,
					meth: typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose)));
				il.Emit(OpCodes.Nop);
			}
			il.EndExceptionBlock();
			il.MarkLabel(codeEnds);
		}

		internal static void WriteUnknownArray(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType, ILGenerator il, bool nullable)
		{
			/*
			var arr = instance.UnknownArray1;
			if (arr == null)
			{
				PrimitiveWriter.WriteNullValue(writer);
			}
			else
			{
				NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)unknownArray.Length);
				int num = 0;
				int i = unknownArray.Length;
				for (num = 0; num < i; i++)
				{
					PrimitiveWriter.WriteValue(writer, unknownArray[i], encoding);
				}

				NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)arr.Length);
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

			// variables that are shared
			var loopIndexVar_Shared = il.DeclareLocal(typeof(int));
			var loopCountVar_Shared = il.DeclareLocal(typeof(int));

			// var arr = instance.UnknownArray1;

			if (prop != null)
			{
				arrayType = prop.PropertyType;

				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				arrayType = field.FieldType;

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				arrayType = valueLoader();
			}
			var instanceVar = il.DeclareLocal(arrayType);
			il.StoreLocal(instanceVar);

			// item type
			var arrItemType = arrayType.GetElementType();

			// if (coll == null)
			il.LoadLocalValue(instanceVar); // instance arr
			il.Emit(OpCodes.Brtrue_S, actualCode); // jump to not null


			// PrimitiveWriter.WriteNullValue(writer);
			{
				il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
				il.Emit(OpCodes.Call,
					typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteNullValue),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
				il.Emit(OpCodes.Nop);

				il.Emit(OpCodes.Br_S, codeEnds);
				il.Emit(OpCodes.Nop);
			}

			// NumericSerializers.WriteUIntNullableMemberCount(writer,(uint) arr.Length);
			il.MarkLabel(actualCode);

			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			il.LoadLocalValue(instanceVar); // instance arr
			il.Emit(OpCodes.Ldlen); // array length
			il.Emit(OpCodes.Conv_I4);
			il.Emit(OpCodes.Call,
				meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteUIntNullableMemberCount),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BufferWriterBase), typeof(uint) }, null));
			il.Emit(OpCodes.Nop);


			// CODE-FOR: int loopIndex = 0;
			il.Emit(OpCodes.Ldc_I4_0); // 0
			il.StoreLocal(loopIndexVar_Shared);

			// CODE-FOR: int loopCount = array.Length;
			il.LoadLocalValue(instanceVar); // instance arr
			il.Emit(OpCodes.Ldlen); // array length
			il.Emit(OpCodes.Conv_I4);
			il.StoreLocal(loopCountVar_Shared);

			// loop start
			var LoopCompareLabel = il.DefineLabel();
			il.Emit(OpCodes.Br_S, LoopCompareLabel);
			{
				var LoopStartLabel = il.DefineLabel();
				il.MarkLabel(LoopStartLabel);

				// CODE-FOR: PrimitiveWriter.WriteValue(writer, array[index], encoding);

				// VALUE -------------
				var valueTypeBasicInfo = BoisTypeCache.GetBasicType(arrItemType);
				if (valueTypeBasicInfo.KnownType != EnBasicKnownType.Unknown)
				{
					BoisTypeCompiler.WriteBasicTypeDirectly(arrayType, il, valueTypeBasicInfo,
						() =>
						{
							// CODE-FOR: array[index]
							il.LoadLocalValue(instanceVar); // instance arr
							il.LoadLocalValue(loopIndexVar_Shared);
							il.ReadArrayItem(arrItemType);

							return arrItemType;
						});
				}
				else
				{
					// for complex types, a method is generated
					var valueTypeInfo = BoisTypeCache.GetRootTypeComputed(arrItemType, false, true);

					il.Emit(OpCodes.Ldarg_0); // BufferWriterBase

					// CODE-FOR: array[index]
					il.LoadLocalValue(instanceVar); // instance arr
					il.LoadLocalValue(loopIndexVar_Shared); // index
					il.ReadArrayItem(arrItemType);

					il.Emit(OpCodes.Ldarg_2); // Encoding
					il.Emit(OpCodes.Call, meth: valueTypeInfo.WriterMethod);
				}

				// CODE-FOR: num++
				il.LoadLocalValue(loopIndexVar_Shared); // index
				il.Emit(OpCodes.Ldc_I4_1);
				il.Emit(OpCodes.Add);
				il.StoreLocal(loopIndexVar_Shared);

				// CODE-FOR: num < i
				il.MarkLabel(LoopCompareLabel);
				il.LoadLocalValue(loopIndexVar_Shared); // index
				il.LoadLocalValue(loopCountVar_Shared); // count
				il.Emit(OpCodes.Clt);
				il.Emit(OpCodes.Brtrue_S, LoopStartLabel);
			}

			il.MarkLabel(codeEnds);
		}


		internal static void WriteNameValueColl(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, ILGenerator il, bool nullable)
		{
			/*
			var coll = instance.NameValueCollection;
			if (coll == null)
			{
				PrimitiveWriter.WriteNullValue(writer);
			}
			else
			{
				NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)coll.Count);

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

			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);
				il.LoadArgAuto(1, typeof(NameValueCollection)); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, typeof(NameValueCollection)); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}
			il.StoreLocal(instanceVar);

			// if (coll == null)
			il.LoadLocalValue(instanceVar); // instance coll
			il.Emit(OpCodes.Ldnull); // null
			il.Emit(OpCodes.Ceq); // ==

			il.Emit(OpCodes.Brfalse_S, actualCode); // jump to not null
			il.Emit(OpCodes.Nop);


			// PrimitiveWriter.WriteNullValue(writer);
			{
				il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
				il.Emit(OpCodes.Call,
					typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteNullValue),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
				il.Emit(OpCodes.Nop);

				il.Emit(OpCodes.Br_S, codeEnds);
				il.Emit(OpCodes.Nop);
			}

			// NumericSerializers.WriteUIntNullableMemberCount(writer, (uint)coll.Count);
			il.MarkLabel(actualCode);

			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			il.LoadLocalValue(instanceVar); // instance coll
			il.Emit(OpCodes.Callvirt,
				// ReSharper disable once PossibleNullReferenceException
				meth: typeof(NameValueCollection).GetProperty(nameof(NameValueCollection.Count)).GetGetMethod());
			il.Emit(OpCodes.Call,
				meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.WriteUIntNullableMemberCount),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BufferWriterBase), typeof(uint) }, null));
			il.Emit(OpCodes.Nop);



			// IEnumerator enumerator = nameValueCollection.GetEnumerator();
			il.LoadLocalValue(instanceVar); // instance coll
			var getEnumeratorMethodInfo = typeof(NameValueCollection).GetMethod(nameof(NameValueCollection.GetEnumerator),
				BindingFlags.Instance | BindingFlags.Public);
			il.Emit(OpCodes.Callvirt, getEnumeratorMethodInfo);

			var enumuratorType = getEnumeratorMethodInfo.ReturnType;
			var enumurator = il.DeclareLocal(enumuratorType);
			il.Emit(OpCodes.Stloc, enumurator);


			//while (enumerator.MoveNext())
			{
				il.MarkLabel(loopStart);

				il.LoadLocalAuto(enumurator);
				il.Emit(OpCodes.Callvirt,
					enumuratorType.GetMethod(nameof(IEnumerator.MoveNext)));
				il.Emit(OpCodes.Brfalse_S, codeEnds);

				// foreach (*string item2* in nameValueCollection)
				var itemKeyVar = il.DeclareLocal(typeof(string));
				il.LoadLocalAuto(enumurator);
				il.Emit(OpCodes.Callvirt,
					// ReSharper disable once PossibleNullReferenceException
					enumuratorType.GetProperty(nameof(IEnumerator.Current)).GetGetMethod());
				il.Emit(OpCodes.Castclass, typeof(string));
				il.StoreLocal(itemKeyVar);
				il.Emit(OpCodes.Nop);

				// PrimitiveWriter.WriteValue(writer, item, encoding);
				il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
				il.LoadLocalValue(itemKeyVar); // item
				il.Emit(OpCodes.Ldarg_2); // Encoding
				il.Emit(OpCodes.Call,
					meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
						BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder,
						new[] { typeof(BufferWriterBase), typeof(string), typeof(Encoding) }, null));
				il.Emit(OpCodes.Nop);

				// PrimitiveWriter.WriteValue(writer, coll[item], encoding);
				il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
				il.LoadLocalValue(instanceVar); // instance coll
				il.LoadLocalValue(itemKeyVar); // item
				il.Emit(OpCodes.Callvirt,
					meth: typeof(NameValueCollection).GetMethod(nameof(NameValueCollection.Get),
						BindingFlags.Instance | BindingFlags.Public, Type.DefaultBinder, new[] { typeof(string) }, null));
				il.Emit(OpCodes.Ldarg_2); // Encoding
				il.Emit(OpCodes.Call,
					meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
						BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder,
						new[] { typeof(BufferWriterBase), typeof(string), typeof(Encoding) }, null));
				il.Emit(OpCodes.Nop);
				il.Emit(OpCodes.Br_S, loopStart);
			}
			il.MarkLabel(codeEnds);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteISet(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader, Type containerType, ILGenerator il, bool nullable)
		{
			WriteCollection(prop, field, valueLoader, containerType, il, nullable);
		}

		internal static void WriteDataSet(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader,
			Type containerType, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}
			il.Emit(OpCodes.Ldarg_2); // Encoding

			var methodArg = new[] { typeof(BufferWriterBase), typeof(DataSet), typeof(Encoding) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));

			il.Emit(OpCodes.Nop);
		}

		internal static void WriteDataTable(PropertyInfo prop, FieldInfo field, Func<Type> valueLoader,
			Type containerType, ILGenerator il, bool nullable)
		{
			il.Emit(OpCodes.Ldarg_0); // BufferWriterBase
			if (prop != null)
			{
				var getter = prop.GetGetMethod(true);

				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Callvirt, meth: getter);
			}
			else if (field != null)
			{
				il.LoadArgAuto(1, containerType); // instance
				il.Emit(OpCodes.Ldfld, field: field);
			}
			else
			{
				valueLoader();
			}
			il.Emit(OpCodes.Ldarg_2); // Encoding

			var methodArg = new[] { typeof(BufferWriterBase), typeof(DataTable), typeof(Encoding) };
			il.Emit(OpCodes.Call, meth: typeof(PrimitiveWriter).GetMethod(nameof(PrimitiveWriter.WriteValue),
				BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, methodArg, null));
			il.Emit(OpCodes.Nop);
		}
		#endregion

		#region Read Root Complex Types
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void ReadRootCollection(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			ReadGenericCollection(null, null, type, type, il, typeInfo.IsNullable, new SharedVariables(il));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void ReadRootDictionary(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			ReadDictionary(null, null, type, type, il, typeInfo.IsNullable, new SharedVariables(il));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void ReadRootUnknownArray(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			ReadUnknownArray(null, null, type, type, il, typeInfo.IsNullable, new SharedVariables(il));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void ReadRootNameValueColl(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			ReadNameValueColl(null, null, type, type, il, typeInfo.IsNullable, new SharedVariables(il));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void ReadRootISet(Type type, BoisComplexTypeInfo typeInfo, ILGenerator il)
		{
			ReadRootCollection(type, typeInfo, il);
		}

		#endregion

		#region Read Simple Types

		internal static void ReadInt16(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt16Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt16),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadInt32(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt32Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt32),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadInt64(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt64Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarInt64),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUInt16(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt16Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt16),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUInt32(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt32Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt32),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadUInt64(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt64Nullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarUInt64),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDouble(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDoubleNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDouble),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDecimal(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDecimalNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarDecimal),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadFloat(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarSingleNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarSingle),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadByte(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			if (isNullable)
			{
				il.Emit(OpCodes.Call,
					meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarByteNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BinaryBufferReader).GetMethod(nameof(BinaryBufferReader.ReadByte)));
			}

			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadSByte(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			if (isNullable)
			{
				il.Emit(OpCodes.Call,
					meth: typeof(NumericSerializers).GetMethod(nameof(NumericSerializers.ReadVarSByteNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null));
			}
			else
			{
				il.Emit(OpCodes.Callvirt,
					meth: typeof(BinaryBufferReader).GetMethod(nameof(BinaryBufferReader.ReadSByte)));
			}

			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadString(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType, ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
			il.Emit(OpCodes.Ldarg_1); // Encoding

			var methodArg = new[] { typeof(BinaryBufferReader), typeof(Encoding) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadString),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));

			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadBool(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType, ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadBooleanNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadBoolean),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDateTime(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTimeNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTime),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDateTimeOffset(PropertyInfo prop, FieldInfo field, Action valueSetter,
			Type containerType, ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTimeOffsetNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDateTimeOffset),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadEnum(PropertyInfo prop, FieldInfo field, Action valueSetter, Type memberType,
			Type containerType, ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

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
				// not needed. memberType = memberType;
			}

			var methodArg = new[] { typeof(BinaryBufferReader) };
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadEnumGeneric),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null)
						.MakeGenericMethod(memberType));

			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadTimeSpan(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadTimeSpanNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadTimeSpan),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadChar(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadCharNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadChar),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
		}


		internal static void ReadGuid(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadGuidNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadGuid),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}


		internal static void ReadColor(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var method =
				isNullable
					? typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadColorNullable),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null)
					: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadColor),
						BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
						new[] { typeof(BinaryBufferReader) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDbNull(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var methodArg = new[] { typeof(BinaryBufferReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDbNull),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}


		internal static void ReadUri(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var methodArg = new[] { typeof(BinaryBufferReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadUri),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadVersion(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var methodArg = new[] { typeof(BinaryBufferReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadVersion),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		internal static void ReadDataSet(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool nullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
			il.Emit(OpCodes.Ldarg_1); // Encoding

			var method = typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDataSet),
				BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
				new[] { typeof(BinaryBufferReader), typeof(Encoding) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}


		internal static void ReadDataTable(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool nullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
			il.Emit(OpCodes.Ldarg_1); // Encoding

			var method = typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadDataTable),
				BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder,
				new[] { typeof(BinaryBufferReader), typeof(Encoding) }, null);

			il.Emit(OpCodes.Call, meth: method);
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}
		internal static void ReadByteArray(PropertyInfo prop, FieldInfo field, Action valueSetter, Type containerType,
			ILGenerator il, bool isNullable)
		{
			if (valueSetter == null)
				il.LoadLocalAuto(0, containerType); // instance
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader

			var methodArg = new[] { typeof(BinaryBufferReader) };
			il.Emit(OpCodes.Call,
				meth: typeof(PrimitiveReader).GetMethod(nameof(PrimitiveReader.ReadByteArray),
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, methodArg, null));
			if (prop != null)
			{
				var setter = prop.GetSetMethod(true);
				il.Emit(OpCodes.Callvirt, meth: setter); // property value
			}
			else if (field != null)
			{
				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				valueSetter?.Invoke();
			}
			il.Emit(OpCodes.Nop);
		}

		#endregion

		#region Read Complex Types
		internal static void ReadUnknownComplexTypeCall(Type memberType, PropertyInfo prop, FieldInfo field, Type containerType, ILGenerator il, BoisComplexTypeInfo complexTypeInfo)
		{
			var nullableBareType = Nullable.GetUnderlyingType(memberType);
			var memberIsStruct = memberType.IsExplicitStruct();
			var LabelReadValue = il.DefineLabel();
			var LabelEndOfCode = il.DefineLabel();

			if (nullableBareType != null || !memberIsStruct)
			{
				// CODE-FOR: if (reader.ReadByte() == NumericSerializers.FlagNullable)
				il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
				il.Emit(OpCodes.Call,
					meth: typeof(BinaryBufferReader).GetMethod(nameof(BinaryBufferReader.ReadByte)));
				il.Emit(OpCodes.Ldc_I4_S, NumericSerializers.FlagIsNull);
				il.Emit(OpCodes.Ceq);
				il.Emit(OpCodes.Brfalse_S, LabelReadValue);

				// CODE-FOR: setting null as value
				{
					il.LoadLocalAuto(0, containerType); // instance

					if (nullableBareType != null)
					{
						var tempValue = il.DeclareLocal(memberType);

						il.LoadLocalAddress(tempValue);
						il.Emit(OpCodes.Initobj, memberType);
						il.LoadLocalValue(tempValue);
					}
					else
					{
						il.Emit(OpCodes.Ldnull);
					}

					if (prop != null)
					{
						il.Emit(OpCodes.Callvirt, prop.GetSetMethod(true));
					}
					else
					{
						il.Emit(OpCodes.Stfld, field: field); // field value
					}
				}

				// CODE-FOR: else
				il.Emit(OpCodes.Br_S, LabelEndOfCode);
			}

			if (nullableBareType != null)
			{
				var valueTypeInfo = BoisTypeCache.GetRootTypeComputed(nullableBareType, true, false);
				il.MarkLabel(LabelReadValue);

				// CODE-FOR: Set value

				il.LoadLocalAuto(0, containerType); // instance
				il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
				il.Emit(OpCodes.Ldarg_1); // Encoding
				il.Emit(OpCodes.Call, meth: valueTypeInfo.ReaderMethod);

				var constructor = typeof(Nullable<>).MakeGenericType(nullableBareType)
					.GetConstructor(new[] { nullableBareType });
				il.Emit(OpCodes.Newobj, constructor);

				if (prop != null)
				{
					if (memberIsStruct)
					{
						il.Emit(OpCodes.Call, prop.GetSetMethod(true));
					}
					else
					{
						il.Emit(OpCodes.Callvirt, prop.GetSetMethod(true));
					}
				}
				else
				{
					il.Emit(OpCodes.Stfld, field: field); // field value
				}

			}
			else
			{
				if (!memberIsStruct)
				{
					var valueTypeInfo = BoisTypeCache.GetRootTypeComputed(memberType, true, false);

					il.MarkLabel(LabelReadValue);

					// CODE-FOR: Set value
					il.LoadLocalAuto(0, containerType); // instance
					il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
					il.Emit(OpCodes.Ldarg_1); // Encoding
					il.Emit(OpCodes.Call, meth: valueTypeInfo.ReaderMethod);

					if (prop != null)
					{
						il.Emit(OpCodes.Callvirt, prop.GetSetMethod(true));
					}
					else
					{
						il.Emit(OpCodes.Stfld, field: field); // field value
					}
				}
			}

			il.MarkLabel(LabelEndOfCode);
		}

		internal static void ReadGenericCollection(PropertyInfo prop, FieldInfo field, Type rootType, Type containerType, ILGenerator il, bool nullable, SharedVariables variableCache)
		{
			/*
			itemCount = NumericSerializers.ReadVarUInt32Nullable(reader);
			if (itemCount != null)
			{
				var coll = new Collection<int>();
				for (int i = 0; i < itemCount; i++)
				{
					var item = NumericSerializers.ReadVarInt32(reader);
					coll[i] = item;
				}
				instance.GenericCollection1 = coll;
			}
			*/
			// OpCodes.Ldloc_0 // instance
			// OpCodes.Ldarg_1 // Encoding
			var beforeEndReturn = il.DefineLabel();

			Type collectionType;
			string propFieldName;
			if (prop != null)
			{
				collectionType = prop.PropertyType;
				propFieldName = prop.Name;
			}
			else if (field != null)
			{
				collectionType = field.FieldType;
				propFieldName = field.Name;
			}
			else
			{
				// only if called from root
				collectionType = rootType;
				propFieldName = "";
			}
			var methodReadVarInt32Nullable = typeof(NumericSerializers)
				.GetMethod(nameof(NumericSerializers.ReadVarUInt32Nullable),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BinaryBufferReader) }, null);

			// var num = NumericSerializers.ReadVarUInt32Nullable(reader);
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
			il.Emit(OpCodes.Call, meth: methodReadVarInt32Nullable);
			var itemCountNullableVar_shared = variableCache.GetOrAdd(methodReadVarInt32Nullable.ReturnType);
			var itemCountVar_shared = variableCache.GetOrAdd(ReflectionHelper.FindUnderlyingGenericElementType(methodReadVarInt32Nullable.ReturnType));
			il.StoreLocal(itemCountNullableVar_shared);

			// if (num.HasValue)
			il.LoadLocalAddress(itemCountNullableVar_shared);
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: methodReadVarInt32Nullable.ReturnType.GetProperty(nameof(Nullable<uint>.HasValue)).GetGetMethod());
			il.Emit(OpCodes.Brfalse_S, beforeEndReturn);

			// int value = num.Value;
			il.LoadLocalAddress(itemCountNullableVar_shared);
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: methodReadVarInt32Nullable.ReturnType.GetProperty(nameof(Nullable<uint>.Value)).GetGetMethod());
			il.StoreLocal(itemCountVar_shared);


			// List<int> list2 = new List<int>();
			var collectionInstance = il.DeclareLocal(collectionType);
			var collectionConstructor = collectionType.GetConstructor(Type.EmptyTypes);
			if (collectionConstructor == null)
				throw new InvalidTypeException($"Member '{propFieldName}' is defined as '{collectionType}' which doesn't have parameterless constructor.");
			il.Emit(OpCodes.Newobj, collectionConstructor);
			il.StoreLocal(collectionInstance);


			// for (/*uint i = 0*/; i < num; i++)
			var forIndexVar = il.DeclareLocal(typeof(uint));
			il.Emit(OpCodes.Ldc_I4_0);
			il.StoreLocal(forIndexVar);
			{
				// Ignore this: jump to value compare
				var compareIndex = il.DefineLabel();
				il.Emit(OpCodes.Br_S, compareIndex);

				var startOfLoop = il.DefineLabel();
				il.MarkLabel(startOfLoop);

				// reading key-value type
				var valueType = ReflectionHelper.FindUnderlyingGenericElementType(collectionType);
				if (valueType == null)
					throw new InvalidTypeException($"Member '{propFieldName}' is defined as '{collectionType}' which is not generic and only generic types are supported.");

				var addMethodInfo = ReflectionHelper.GetIListAddMethod(collectionType, valueType);


				// VALUE -------------
				var valueTypeBasicInfo = BoisTypeCache.GetBasicType(valueType);
				if (valueTypeBasicInfo.KnownType != EnBasicKnownType.Unknown)
				{
					il.LoadLocalValue(collectionInstance);
					BoisTypeCompiler.ReadBasicTypeDirectly(il, valueType, valueTypeBasicInfo, () =>
					 {
						 if (addMethodInfo.NeedsArgumentBoxing)
							 il.Emit(OpCodes.Box, valueType);

						 il.Emit(OpCodes.Callvirt, meth: addMethodInfo.MethodInfo);

						 if (addMethodInfo.HasRetunValue)
							 il.Emit(OpCodes.Pop);
					 });
				}
				else
				{
					// for complex types, a method is generated
					var valueTypeInfo = BoisTypeCache.GetRootTypeComputed(valueType, true, false);

					il.LoadLocalValue(collectionInstance);
					il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
					il.Emit(OpCodes.Ldarg_1); // Encoding
					il.Emit(OpCodes.Call, meth: valueTypeInfo.ReaderMethod);

					if (addMethodInfo.NeedsArgumentBoxing)
						il.Emit(OpCodes.Box, valueType);

					il.Emit(OpCodes.Callvirt, meth: addMethodInfo.MethodInfo);

					if (addMethodInfo.HasRetunValue)
						il.Emit(OpCodes.Pop);
				}

				// num2++;
				il.Emit(OpCodes.Ldloc, forIndexVar);
				il.Emit(OpCodes.Ldc_I4_1);
				il.Emit(OpCodes.Add);
				il.StoreLocal(forIndexVar);

				il.MarkLabel(compareIndex);
				// i < value
				il.Emit(OpCodes.Ldloc, forIndexVar);
				il.Emit(OpCodes.Ldloc, itemCountVar_shared);
				il.Emit(OpCodes.Clt_Un);
				il.Emit(OpCodes.Brtrue_S, startOfLoop);
			}

			// emitSample.GenericList = list2;
			if (prop != null)
			{
				il.LoadLocalAuto(0, containerType); // instance
				il.LoadLocalValue(collectionInstance);

				il.Emit(OpCodes.Callvirt, prop.GetSetMethod(true));
			}
			else if (field != null)
			{
				il.LoadLocalAuto(0, containerType); // instance
				il.LoadLocalValue(collectionInstance);

				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				// returning the instance
				il.LoadLocalValue(collectionInstance);
				il.Emit(OpCodes.Ret);
			}

			il.MarkLabel(beforeEndReturn);

			if (rootType != null)
			{
				// returning null 
				il.Emit(OpCodes.Ldnull);
				il.Emit(OpCodes.Ret);
			}

			// returning the variables
			variableCache.ReturnVariable(itemCountNullableVar_shared);
			variableCache.ReturnVariable(itemCountVar_shared);
		}

		internal static void ReadDictionary(PropertyInfo prop, FieldInfo field, Type rootType, Type containerType, ILGenerator il, bool nullable, SharedVariables variableCache)
		{
			/*
			var dicCoun0 = NumericSerializers.ReadVarUInt32Nullable(reader);
			if (dicCoun0.HasValue)
			{
				var count = dicCoun0.Value;

				var dic = new Dictionary<string, string>();
				for (int i = 0; i < count; i++)
				{
					var key = PrimitiveReader.ReadString(reader, encoding);
					var value = PrimitiveReader.ReadString(reader, encoding);
					dic.Add(key, value);

					dic.Add(PrimitiveReader.ReadString(reader, encoding), PrimitiveReader.ReadString(reader, encoding));
				}
				instance.GenericDictionary = dic;
			}
			*/
			var beforeEndReturn = il.DefineLabel();

			Type dictionaryType;
			string propFieldName;
			if (prop != null)
			{
				dictionaryType = prop.PropertyType;
				propFieldName = prop.Name;
			}
			else if (field != null)
			{
				dictionaryType = field.FieldType;
				propFieldName = field.Name;
			}
			else
			{
				dictionaryType = rootType;
				propFieldName = "";
			}
			var methodReadVarInt32Nullable = typeof(NumericSerializers)
				.GetMethod(nameof(NumericSerializers.ReadVarUInt32Nullable),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BinaryBufferReader) }, null);


			// ReSharper disable once PossibleNullReferenceException
			var itemCountNullableVar_shared = variableCache.GetOrAdd(methodReadVarInt32Nullable.ReturnType);
			var itemCountVar_shared = variableCache.GetOrAdd(ReflectionHelper.FindUnderlyingGenericElementType(methodReadVarInt32Nullable.ReturnType));

			// var num = NumericSerializers.ReadVarInt32Nullable(reader);
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
			il.Emit(OpCodes.Call, meth: methodReadVarInt32Nullable);
			il.StoreLocal(itemCountNullableVar_shared);

			// if (num.HasValue)
			il.LoadLocalAddress(itemCountNullableVar_shared);
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: methodReadVarInt32Nullable.ReturnType.GetProperty(nameof(Nullable<uint>.HasValue)).GetGetMethod());
			il.Emit(OpCodes.Brfalse_S, beforeEndReturn);

			// int value = num.Value;
			il.LoadLocalAddress(itemCountNullableVar_shared);
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: methodReadVarInt32Nullable.ReturnType.GetProperty(nameof(Nullable<uint>.Value)).GetGetMethod());
			il.StoreLocal(itemCountVar_shared);

			// var dic = new Dictionary<string, string>();
			var dictionaryInstance = il.DeclareLocal(dictionaryType);
			var dictionaryConstructor = dictionaryType.GetConstructor(Type.EmptyTypes);
			if (dictionaryConstructor == null)
				throw new InvalidTypeException($"Member '{propFieldName}' is defined as '{dictionaryType}' which doesn't have parameterless constructor.");
			il.Emit(OpCodes.Newobj, dictionaryConstructor);
			il.StoreLocal(dictionaryInstance);

			// for (/*uint i = 0*/; i < num; i++)
			var forIndexVar = il.DeclareLocal(typeof(uint));
			il.Emit(OpCodes.Ldc_I4_0);
			il.StoreLocal(forIndexVar);
			{
				// Ignore this: jump to value compare
				var compareIndex = il.DefineLabel();
				il.Emit(OpCodes.Br_S, compareIndex);

				var startOfLoop = il.DefineLabel();
				il.MarkLabel(startOfLoop);

				// reading key-value type
				var genericTypes = ReflectionHelper.FindUnderlyingGenericDictionaryElementType(dictionaryType);
				if (genericTypes == null)
					throw new InvalidTypeException($"Member '{propFieldName}' is defined as '{dictionaryType}' which is not generic and only generic types are supported.");
				var keyType = genericTypes[0];
				var valueType = genericTypes[1];

				var addMethodInfo = ReflectionHelper.GetIDictionaryAddMethod(dictionaryType, keyType, valueType);

				//  dictionary
				il.LoadLocalValue(dictionaryInstance);
				// KEY -------------
				var keyTypeBasicInfo = BoisTypeCache.GetBasicType(keyType);
				if (keyTypeBasicInfo.KnownType != EnBasicKnownType.Unknown)
				{
					BoisTypeCompiler.ReadBasicTypeDirectly(il, keyType, keyTypeBasicInfo, () =>
					{
						if (addMethodInfo.NeedsArgumentBoxing)
							il.Emit(OpCodes.Box, keyType);
					});
				}
				else
				{
					// for complex types, a method is generated
					var keyTypeInfo = BoisTypeCache.GetRootTypeComputed(keyType, true, false);

					il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
					il.Emit(OpCodes.Ldarg_1); // Encoding
					il.Emit(OpCodes.Call, meth: keyTypeInfo.ReaderMethod);

					if (addMethodInfo.NeedsArgumentBoxing)
						il.Emit(OpCodes.Box, keyType);
				}

				// VALUE -------------
				var valueTypeBasicInfo = BoisTypeCache.GetBasicType(valueType);
				if (valueTypeBasicInfo.KnownType != EnBasicKnownType.Unknown)
				{
					BoisTypeCompiler.ReadBasicTypeDirectly(il, valueType, valueTypeBasicInfo, () =>
					{
						if (addMethodInfo.ValueNeedsArgumentBoxing)
							il.Emit(OpCodes.Box, valueType);
					});
				}
				else
				{
					// for complex types, a method is generated
					var valueTypeInfo = BoisTypeCache.GetRootTypeComputed(valueType, true, false);

					il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
					il.Emit(OpCodes.Ldarg_1); // Encoding
					il.Emit(OpCodes.Call, meth: valueTypeInfo.ReaderMethod);

					if (addMethodInfo.ValueNeedsArgumentBoxing)
						il.Emit(OpCodes.Box, valueType);
				}

				// dictionary.Add
				if (addMethodInfo.NeedsIDictionaryBoxing)
					il.Emit(OpCodes.Box, addMethodInfo.BoxingIDictionaryType);

				il.Emit(OpCodes.Callvirt, meth: addMethodInfo.MethodInfo);
				if (addMethodInfo.HasRetunValue)
					il.Emit(OpCodes.Pop);

				// num2++;
				il.Emit(OpCodes.Ldloc, forIndexVar);
				il.Emit(OpCodes.Ldc_I4_1);
				il.Emit(OpCodes.Add);
				il.StoreLocal(forIndexVar);

				il.MarkLabel(compareIndex);
				// i < value
				il.Emit(OpCodes.Ldloc, forIndexVar);
				il.Emit(OpCodes.Ldloc, itemCountVar_shared);
				il.Emit(OpCodes.Clt_Un);
				il.Emit(OpCodes.Brtrue_S, startOfLoop);
			}

			// emitSample.GenericDictionary = dictionary;
			if (prop != null)
			{
				il.LoadLocalAuto(0, containerType); // instance
				il.LoadLocalValue(dictionaryInstance);

				il.Emit(OpCodes.Callvirt, prop.GetSetMethod(true));
			}
			else if (field != null)
			{
				il.LoadLocalAuto(0, containerType); // instance
				il.LoadLocalValue(dictionaryInstance);

				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				// returning the instance
				il.LoadLocalValue(dictionaryInstance);
				il.Emit(OpCodes.Ret);
			}

			il.MarkLabel(beforeEndReturn);

			if (rootType != null)
			{
				// returning null 
				il.Emit(OpCodes.Ldnull);
				il.Emit(OpCodes.Ret);
			}

			// returning the variables
			variableCache.ReturnVariable(itemCountNullableVar_shared);
			variableCache.ReturnVariable(itemCountVar_shared);
		}



		internal static void ReadNameValueColl(PropertyInfo prop, FieldInfo field, Type rootType, Type containerType, ILGenerator il, bool nullable, SharedVariables variableCache)
		{
			/*
			var dicCoun0 = NumericSerializers.ReadVarUInt32Nullable(reader);
			if (dicCoun0.HasValue)
			{
				var count = dicCoun0.Value;

				var dic = new Dictionary<string, string>();
				for (int i = 0; i < count; i++)
				{
					var key = PrimitiveReader.ReadString(reader, encoding);
					var value = PrimitiveReader.ReadString(reader, encoding);
					dic.Add(key, value);

					dic.Add(PrimitiveReader.ReadString(reader, encoding), PrimitiveReader.ReadString(reader, encoding));
				}
				instance.GenericDictionary = dic;
			}
			*/
			var beforeEndReturn = il.DefineLabel();

			Type dictionaryType;
			string propFieldName;
			if (prop != null)
			{
				dictionaryType = prop.PropertyType;
				propFieldName = prop.Name;
			}
			else if (field != null)
			{
				dictionaryType = field.FieldType;
				propFieldName = field.Name;
			}
			else
			{
				dictionaryType = rootType;
				propFieldName = "";
			}
			var methodReadVarInt32Nullable = typeof(NumericSerializers)
				.GetMethod(nameof(NumericSerializers.ReadVarUInt32Nullable),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BinaryBufferReader) }, null);


			// ReSharper disable once PossibleNullReferenceException
			var itemCountNullableVar_shared = variableCache.GetOrAdd(methodReadVarInt32Nullable.ReturnType);
			var itemCountVar_shared = variableCache.GetOrAdd(ReflectionHelper.FindUnderlyingGenericElementType(methodReadVarInt32Nullable.ReturnType));

			// var num = NumericSerializers.ReadVarInt32Nullable(reader);
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
			il.Emit(OpCodes.Call, meth: methodReadVarInt32Nullable);
			il.StoreLocal(itemCountNullableVar_shared);

			// if (num.HasValue)
			il.LoadLocalAddress(itemCountNullableVar_shared);
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: methodReadVarInt32Nullable.ReturnType.GetProperty(nameof(Nullable<uint>.HasValue)).GetGetMethod());
			il.Emit(OpCodes.Brfalse_S, beforeEndReturn);

			// int value = num.Value;
			il.LoadLocalAddress(itemCountNullableVar_shared);
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: methodReadVarInt32Nullable.ReturnType.GetProperty(nameof(Nullable<uint>.Value)).GetGetMethod());
			il.StoreLocal(itemCountVar_shared);

			// var dic = new Dictionary<string, string>();
			var dictionaryInstance = il.DeclareLocal(dictionaryType);
			var dictionaryConstructor = dictionaryType.GetConstructor(Type.EmptyTypes);
			if (dictionaryConstructor == null)
				throw new InvalidTypeException($"Member '{propFieldName}' is defined as '{dictionaryType}' which doesn't have parameterless constructor.");
			il.Emit(OpCodes.Newobj, dictionaryConstructor);
			il.StoreLocal(dictionaryInstance);

			// for (/*int i = 0*/; i < num; i++)
			var forIndexVar = il.DeclareLocal(typeof(uint));
			il.Emit(OpCodes.Ldc_I4_0);
			il.StoreLocal(forIndexVar);
			{
				// Ignore this: jump to value compare
				var compareIndex = il.DefineLabel();
				il.Emit(OpCodes.Br_S, compareIndex);

				var startOfLoop = il.DefineLabel();
				il.MarkLabel(startOfLoop);

				// reading key-value type
				var itemTypeBasicInfo = BoisTypeCache.GetBasicType(typeof(string));
				var addMethodInfo = dictionaryType.GetMethod(nameof(NameValueCollection.Add),
					BindingFlags.Instance | BindingFlags.Public,
					Type.DefaultBinder, new[] { typeof(string), typeof(string) }, null);

				//  dictionary
				il.LoadLocalValue(dictionaryInstance);
				// KEY -------------
				BoisTypeCompiler.ReadBasicTypeDirectly(il, typeof(string), itemTypeBasicInfo, () => { });

				// VALUE -------------
				BoisTypeCompiler.ReadBasicTypeDirectly(il, typeof(string), itemTypeBasicInfo, () => { });

				// dictionary.Add
				il.Emit(OpCodes.Callvirt, meth: addMethodInfo);

				// num2++;
				il.Emit(OpCodes.Ldloc, forIndexVar);
				il.Emit(OpCodes.Ldc_I4_1);
				il.Emit(OpCodes.Add);
				il.StoreLocal(forIndexVar);

				il.MarkLabel(compareIndex);
				// i < value
				il.Emit(OpCodes.Ldloc, forIndexVar);
				il.Emit(OpCodes.Ldloc, itemCountVar_shared);
				il.Emit(OpCodes.Clt_Un);
				il.Emit(OpCodes.Brtrue_S, startOfLoop);
			}

			// emitSample.GenericDictionary = dictionary;
			if (prop != null)
			{
				il.LoadLocalAuto(0, containerType); // instance
				il.LoadLocalValue(dictionaryInstance);

				il.Emit(OpCodes.Callvirt, prop.GetSetMethod(true));
			}
			else if (field != null)
			{
				il.LoadLocalAuto(0, containerType); // instance
				il.LoadLocalValue(dictionaryInstance);

				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				// returning the instance
				il.LoadLocalValue(dictionaryInstance);
				il.Emit(OpCodes.Ret);
			}
			il.MarkLabel(beforeEndReturn);

			if (rootType != null)
			{
				// returning null 
				il.Emit(OpCodes.Ldnull);
				il.Emit(OpCodes.Ret);
			}


			// returning the variables
			variableCache.ReturnVariable(itemCountNullableVar_shared);
			variableCache.ReturnVariable(itemCountVar_shared);
		}


		internal static void ReadUnknownArray(PropertyInfo prop, FieldInfo field, Type arrayType, Type containerType,
			ILGenerator il, bool nullable, SharedVariables variableCache)
		{
			/*
			itemCount = NumericSerializers.ReadVarUInt32Nullable(reader);
			if (itemCount != null)
			{
				var coll = new Collection<int>();
				for (int i = 0; i < itemCount; i++)
				{
					var item = NumericSerializers.ReadVarInt32(reader);
					coll[i] = item;
				}
				instance.GenericCollection1 = coll;
			}
			*/
			// OpCodes.Ldloc_0 // instance
			// OpCodes.Ldarg_1 // Encoding
			var beforeEndReturn = il.DefineLabel();

			Type arrType;
			if (prop != null)
			{
				arrType = prop.PropertyType;
			}
			else if (field != null)
			{
				arrType = field.FieldType;
			}
			else
			{
				arrType = arrayType;
			}
			var methodReadVarInt32Nullable = typeof(NumericSerializers)
				.GetMethod(nameof(NumericSerializers.ReadVarUInt32Nullable),
					BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new[] { typeof(BinaryBufferReader) }, null);

			// var num = NumericSerializers.ReadVarInt32Nullable(reader);
			il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
			il.Emit(OpCodes.Call, meth: methodReadVarInt32Nullable);
			var itemCountNullableVar_shared = variableCache.GetOrAdd(methodReadVarInt32Nullable.ReturnType);
			var itemCountVar_shared = variableCache.GetOrAdd(ReflectionHelper.FindUnderlyingGenericElementType(methodReadVarInt32Nullable.ReturnType));
			il.StoreLocal(itemCountNullableVar_shared);

			// if (num.HasValue)
			il.LoadLocalAddress(itemCountNullableVar_shared);
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: methodReadVarInt32Nullable.ReturnType.GetProperty(nameof(Nullable<uint>.HasValue)).GetGetMethod());
			il.Emit(OpCodes.Brfalse_S, beforeEndReturn);

			// int value = num.Value;
			il.LoadLocalAddress(itemCountNullableVar_shared);
			il.Emit(OpCodes.Call,
				// ReSharper disable once PossibleNullReferenceException
				meth: methodReadVarInt32Nullable.ReturnType.GetProperty(nameof(Nullable<uint>.Value)).GetGetMethod());
			il.StoreLocal(itemCountVar_shared);


			var arrayItemType = arrType.GetElementType();
			// var arr = new Array();
			var arrInstance = il.DeclareLocal(arrType);
			il.Emit(OpCodes.Ldloc, itemCountVar_shared);
			il.Emit(OpCodes.Newarr, arrayItemType);
			il.StoreLocal(arrInstance);


			// for (/*uint i = 0*/; i < num; i++)
			var forIndexVar = il.DeclareLocal(typeof(uint));
			il.Emit(OpCodes.Ldc_I4_0);
			il.StoreLocal(forIndexVar);
			{
				// Ignore this: jump to value compare
				var compareIndex = il.DefineLabel();
				il.Emit(OpCodes.Br_S, compareIndex);

				var startOfLoop = il.DefineLabel();
				il.MarkLabel(startOfLoop);

				// VALUE -------------
				var valueTypeBasicInfo = BoisTypeCache.GetBasicType(arrayItemType);
				if (valueTypeBasicInfo.KnownType != EnBasicKnownType.Unknown)
				{
					il.LoadLocalValue(arrInstance);
					il.Emit(OpCodes.Ldloc, forIndexVar);
					BoisTypeCompiler.ReadBasicTypeDirectly(il, arrayItemType, valueTypeBasicInfo, () =>
					{
						il.Emit(OpCodes.Stelem, arrayItemType);
					});
				}
				else
				{
					// for complex types, a method is generated
					var valueTypeInfo = BoisTypeCache.GetRootTypeComputed(arrayItemType, true, false);

					il.LoadLocalValue(arrInstance);
					il.Emit(OpCodes.Ldloc, forIndexVar);
					il.Emit(OpCodes.Ldarg_0); // BinaryBufferReader
					il.Emit(OpCodes.Ldarg_1); // Encoding
					il.Emit(OpCodes.Call, meth: valueTypeInfo.ReaderMethod);

					il.Emit(OpCodes.Stelem, arrayItemType);
				}

				// num2++;
				il.Emit(OpCodes.Ldloc, forIndexVar);
				il.Emit(OpCodes.Ldc_I4_1);
				il.Emit(OpCodes.Add);
				il.StoreLocal(forIndexVar);

				il.MarkLabel(compareIndex);
				// i < value
				il.Emit(OpCodes.Ldloc, forIndexVar);
				il.Emit(OpCodes.Ldloc, itemCountVar_shared);
				il.Emit(OpCodes.Clt_Un);
				il.Emit(OpCodes.Brtrue_S, startOfLoop);
			}

			// emitSample.GenericList = list2;
			if (prop != null)
			{
				il.LoadLocalAuto(0, containerType); // instance
				il.LoadLocalValue(arrInstance);

				il.Emit(OpCodes.Callvirt, prop.GetSetMethod(true));
			}
			else if (field != null)
			{
				il.LoadLocalAuto(0, containerType); // instance
				il.LoadLocalValue(arrInstance);

				il.Emit(OpCodes.Stfld, field: field); // field value
			}
			else
			{
				// returning the instance
				il.LoadLocalValue(arrInstance);
				il.Emit(OpCodes.Ret);
			}

			il.MarkLabel(beforeEndReturn);

			if (arrayType != null)
			{
				// returning null 
				il.Emit(OpCodes.Ldnull);
				il.Emit(OpCodes.Ret);
			}

			// returning the variables
			variableCache.ReturnVariable(itemCountNullableVar_shared);
			variableCache.ReturnVariable(itemCountVar_shared);
		}

		#endregion

	}

	static class TypeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsExplicitStruct(this Type type)
		{
			return type.IsValueType
				   && !type.IsPrimitive
				   && !type.IsClass
				   && !type.IsEnum;
		}
	}

	static class IlExtensions
	{
		internal static void LoadArgValue(this ILGenerator il, int index, Type argumentType)
		{
			switch (index)
			{
				case 0: il.Emit(OpCodes.Ldarg_0); break;
				case 1: il.Emit(OpCodes.Ldarg_1); break;
				case 2: il.Emit(OpCodes.Ldarg_2); break;
				case 3: il.Emit(OpCodes.Ldarg_3); break;
				default:
					if (index < 256)
					{
						il.Emit(OpCodes.Ldarg_S, (byte)index);
					}
					else
					{
						il.Emit(OpCodes.Ldarg, (ushort)index);
					}
					break;
			}
		}

		internal static void LoadArgAuto(this ILGenerator il, int index, Type argumentType)
		{
			if (argumentType.IsValueType && !argumentType.IsPrimitive && !argumentType.IsClass && !argumentType.IsEnum)
			{
				if (index < 256)
				{
					il.Emit(OpCodes.Ldarga_S, (byte)index);
				}
				else
				{
					il.Emit(OpCodes.Ldarga, (ushort)index);
				}
			}
			else
			{
				switch (index)
				{
					case 0: il.Emit(OpCodes.Ldarg_0); break;
					case 1: il.Emit(OpCodes.Ldarg_1); break;
					case 2: il.Emit(OpCodes.Ldarg_2); break;
					case 3: il.Emit(OpCodes.Ldarg_3); break;
					default:
						if (index < 256)
						{
							il.Emit(OpCodes.Ldarg_S, (byte)index);
						}
						else
						{
							il.Emit(OpCodes.Ldarg, (ushort)index);
						}
						break;
				}
			}

		}

		internal static void ReadArrayItem(this ILGenerator il, Type arrayItemType)
		{
			if (arrayItemType == typeof(string) ||
				!arrayItemType.IsValueType)
			{
				il.Emit(OpCodes.Ldelem_Ref);
			}
			else
			{
				il.Emit(OpCodes.Ldelem, arrayItemType);
			}
		}

		internal static void LoadLocalValue(this ILGenerator il, LocalBuilder local)
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

		internal static void LoadLocalAddress(this ILGenerator il, LocalBuilder local)
		{
			if (local.LocalIndex < 256)
			{
				il.Emit(OpCodes.Ldloca_S, (byte)local.LocalIndex);
			}
			else
			{
				il.Emit(OpCodes.Ldloca, local);
			}
		}

		internal static void LoadLocalAuto(this ILGenerator il, LocalBuilder local)
		{
			if (local.LocalType?.IsValueType == true)
			{
				LoadLocalAddress(il, local);
				return;
			}
			LoadLocalValue(il, local);
		}

		internal static void LoadLocalAuto(this ILGenerator il, int index, Type argumentType)
		{
			if (argumentType.IsValueType && !argumentType.IsPrimitive && !argumentType.IsClass && !argumentType.IsEnum)
			{
				if (index < 256)
				{
					il.Emit(OpCodes.Ldloca_S, (byte)index);
				}
				else
				{
					throw new ArgumentException("For index greater than 256 Local variable name is required", nameof(index));
				}
			}
			else
			{
				if (index < 256)
				{
					il.Emit(OpCodes.Ldloc_S, (byte)index);
				}
				else
				{
					throw new ArgumentException("For index greater than 256 Local variable name is required", nameof(index));
				}
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
