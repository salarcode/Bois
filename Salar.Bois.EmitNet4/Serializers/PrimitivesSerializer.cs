using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Salar.Bois.Serializers
{
	public static class PrimitivesSerializer
	{
		public static void WriteVarInt(BinaryWriter writer, int num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, int? num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, long num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, long? num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, short num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, short? num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, ushort num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, ushort? num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, uint num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, uint? num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, ulong num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}
		internal static void WriteVarInt(BinaryWriter writer, ulong? num)
		{
			PrimitivesConvertion.WriteVarInt(writer, num);
		}

		public static void WriteString(BinaryWriter writer, string str)
		{
			writer.Write(str);
		}
	}
}
