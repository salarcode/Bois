using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace Salar.Bois.NetFx.Tests.TestObjects
{
	public class TestObjectPrimitiveTypes
	{
		public bool Boolean { get; set; }

		public bool? BooleanNullable { get; set; }

		public bool BooleanF = true;

		public bool? BooleanNullableF = true;

		public int Int32 { get; set; } = Int32.MinValue;

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

		public char Char { get; set; } = 'ت';

		public byte Byte { get; set; }

		public byte? ByteNullable { get; set; }

		public sbyte SByte { get; set; }

		public sbyte? SByteNullable { get; set; }

		public DateTime DateTime { get; set; } = DateTime.Now;

		public DateTimeOffset DateTimeOffset { get; set; } = DateTimeOffset.Now;

		public TimeSpan TimeSpan { get; set; } = DateTime.Now.TimeOfDay;

		public byte[] ByteArray { get; set; } = new byte[10];

		public Guid Guid { get; set; }
		public Guid Guid2{ get; set; }

		public Color Color { get; set; } = Color.GreenYellow;

		public DBNull DbNull { get; set; } = DBNull.Value;

		public Uri Uri { get; set; } = new Uri("https://github.com/salarcode/Bois");

		public Version Version { get; set; } = new Version(3, 0);

		public EnvironmentVariableTarget Enum { get; set; } = EnvironmentVariableTarget.User;

		public EnvironmentVariableTarget[] EnumArray { get; set; } =
			new[] { EnvironmentVariableTarget.Machine, EnvironmentVariableTarget.User };

		public string[] UnknownArray1 { get; set; } = new[] { "This", "Is", "A", "Test" };

		public Guid[] UnknownArray2 { get; set; } = new[] { Guid.NewGuid(), Guid.NewGuid() };

		public DataTable DataTable { get; set; } = new DataTable("First-Table");

		public DataSet DataSet { get; set; } = new DataSet("DsTest");

		public static IEnumerable<object[]> GetTestData()
		{
			yield return new object[]
			{
				new TestObjectPrimitiveTypes
				{
					Guid = Guid.NewGuid()
				}
			};
		}
	}
}
