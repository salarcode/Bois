using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public struct TestStructPrimitives
	{
		public bool Boolean { get; set; }

		public bool? BooleanNullable { get; set; }

		public bool BooleanF;

		public bool? BooleanNullableF;

		public int Int32 { get; set; }

		public short Int16 { get; set; }

		public short? Int16Nullable { get; set; }

		public short Int16F;

		public short? Int16NullableF;

		public long Int64 { get; set; }

		public uint UInt32 { get; set; }

		public ushort UInt16 { get; set; }

		public ulong UInt64 { get; set; }

		public float Float { get; set; }

		public double Double { get; set; }

		public decimal Decimal { get; set; }

		public string String { get; set; }

		public string StringF;

		public char Char { get; set; }

		public byte Byte { get; set; }

		public byte? ByteNullable { get; set; }

		public sbyte SByte { get; set; }

		public sbyte? SByteNullable { get; set; }

		public DateTime DateTime { get; set; }

		public DateTimeOffset DateTimeOffset { get; set; }

		public TimeSpan TimeSpan { get; set; }

		public byte[] ByteArray { get; set; }

		public Guid Guid { get; set; }
		public Guid Guid2 { get; set; }

		public Color Color { get; set; }

		public DBNull DbNull { get; set; }

		public Uri Uri { get; set; }

		public Version Version { get; set; }

		public EnvironmentVariableTarget Enum { get; set; }

		public EnvironmentVariableTarget[] EnumArray { get; set; }

		public string[] UnknownArray1 { get; set; }

		public Guid[] UnknownArray2 { get; set; }

		public DataTable DataTable { get; set; }

		public DataSet DataSet { get; set; }

		public static IEnumerable<object[]> GetTestData()
		{
			yield return new object[]
			{
				new TestStructPrimitives
				{
					Guid = Guid.NewGuid(),
					Color = System.Drawing.Color.FromArgb(10,10,20,30),
					Boolean = true,
					DateTimeOffset = DateTimeOffset.UtcNow,
					Int16NullableF = 90
				}
			};
			yield return new object[]
			{
				new TestStructPrimitives
				{
					Int16 = 90,
					Int16NullableF = 123,
					TimeSpan = System.DateTime.Now.TimeOfDay,
					EnumArray = new []
					{
						EnvironmentVariableTarget.Machine,
						EnvironmentVariableTarget.Process
					},
					ByteArray = new byte[]{90,90,10,13}
				}
			};
		}
	}
}
