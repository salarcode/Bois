using FluentAssertions;
using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.Serializers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	public class Test_BasicTypes_Primitive : TestBase
	{
		[Theory]
		[MemberData(nameof(GetStringData))]
		public void TestingStrings(string init, Encoding encoding)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init, encoding);
			ResetStream();

			var final = PrimitiveReader.ReadString(Reader, encoding);

			final.Should().Be(init);
		}

		public static IEnumerable<object[]> GetStringData()
		{
			var utf8 = Encoding.UTF8;
			yield return new object[] { "", utf8 };
			yield return new object[] { null, utf8 };
			yield return new object[] { "این-یک-تست است", utf8 };
			yield return new object[] { "This is a test", utf8 };
			yield return new object[] { "😎 emojis 😍", utf8 };
			yield return new object[] { "ASCII", Encoding.ASCII };
		}

		[Theory]
		[InlineData(true), InlineData(false)]
		public void TestingBoolean(bool init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadBoolean(Reader);

			final.Should().Be(init);
		}

		[Theory]
		[InlineData(true), InlineData(null), InlineData(false)]
		public void TestingBooleanNullable(bool? init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadBooleanNullable(Reader);

			final.Should().Be(init);
		}

		[Theory]
		[InlineData(' '), InlineData('A'), InlineData('آ'), InlineData('6')]
		public void TestingChar(char init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadChar(Reader);

			final.Should().Be(init);
		}

		[Theory]
		[InlineData(null)]
		[InlineData(' '), InlineData('A'), InlineData('آ'), InlineData('6')]
		public void TestingCharNullable(char? init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadCharNullable(Reader);

			final.Should().Be(init);
		}

		[Theory]
		[InlineData(null, typeof(DayOfWeek))]
		[InlineData(EnvironmentVariableTarget.Machine, null), InlineData(DayOfWeek.Sunday, null), InlineData(DayOfWeek.Thursday, null)]
		public void TestingEnum(Enum init, Type enumType)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			if (init != null)
				enumType = init.GetType();

			var final = PrimitiveReader.ReadEnum(Reader, enumType);

			final.Should().Be(init);
		}

		public static IEnumerable<object[]> GetDBNullData()
		{
			yield return new object[] { DBNull.Value };
			yield return new object[] { null };
		}

		[Theory]
		[MemberData(nameof(GetDBNullData))]
		public void TestingDBNull(DBNull init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadDbNull(Reader);

			final.Should().Be(init);
		}

		public static IEnumerable<object[]> GetGuidData()
		{
			yield return new object[] { Guid.Empty };
			yield return new object[] { Guid.NewGuid() };
			yield return new object[] { Guid.NewGuid() };
			yield return new object[] { Guid.NewGuid() };
		}


		[Theory]
		[MemberData(nameof(GetGuidData))]
		public void TestingGuid(Guid init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadGuid(Reader);

			final.Should().Be(init);
		}

		[Theory]
		[InlineData(null)]
		[MemberData(nameof(GetGuidData))]
		public void TestingGuidNullable(Guid? init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadGuidNullable(Reader);

			final.Should().Be(init);
		}


		public static IEnumerable<object[]> GetUriData()
		{
			yield return new object[] { new Uri("https://github.com/salarcode/Bois"), };
			yield return new object[] { new Uri("https://github.com/salarcode/Bois", UriKind.Absolute), };
			yield return new object[] { new Uri("/salarcode/Bois", UriKind.Relative), };
			yield return new object[] { new Uri("https://github.com/salarcode/Bois?test=true", UriKind.RelativeOrAbsolute), };
			yield return new object[] { new Uri("/salarcode/Bois?test=true", UriKind.RelativeOrAbsolute), };
		}

		[Theory]
		[InlineData(null)]
		[MemberData(nameof(GetUriData))]
		public void TestingUri(Uri init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadUri(Reader);

			final.Should().Be(init);
		}

		public static IEnumerable<object[]> GetVersionData()
		{
			yield return new object[] { new Version(), };
			yield return new object[] { new Version("0.0.0.0"), };
			yield return new object[] { new Version("30.10.10.0"), };
			yield return new object[] { new Version(10, 10, 2, 2), };
			yield return new object[] { new Version(0, 0, 2, 2), };
			yield return new object[] { new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue), };
		}

		[Theory]
		[InlineData(null)]
		[MemberData(nameof(GetVersionData))]
		public void TestingVersion(Version init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadVersion(Reader);

			final.Should().Be(init);
		}


		public static IEnumerable<object[]> GetColorData()
		{
			yield return new object[] { Color.Empty };
			yield return new object[] { Color.Aquamarine, };
			yield return new object[] { Color.DarkRed, };
			yield return new object[] { Color.FromArgb(50, 100, 100, 100), };
			yield return new object[] { Color.FromKnownColor(KnownColor.ActiveCaption), };
			yield return new object[] { Color.FromName("Blue"), };
		}

		[Theory]
		[MemberData(nameof(GetColorData))]
		public void TestingColor(Color init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadColor(Reader);

			Assert.True(init.ToArgb() == final.ToArgb(), "Color value are not same");

			// saving name and compare against it is pointless
			//final.Should().BeEquivalentTo(init, because: "The colors name are not same");
		}

		[Theory]
		[InlineData(null)]
		[MemberData(nameof(GetColorData))]
		public void TestingColorNullable(Color? init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadColorNullable(Reader);

			Assert.True(init?.ToArgb() == final?.ToArgb(), "Color value are not same");

			// saving name and compare against it is pointless
			//final.Should().BeEquivalentTo(init, because: "The colors name are not same");
		}



		public static IEnumerable<object[]> GetTimeSpanData()
		{
			yield return new object[] { TimeSpan.MinValue };
			yield return new object[] { TimeSpan.MaxValue };
			yield return new object[] { TimeSpan.Zero };
			yield return new object[] { DateTime.Now.TimeOfDay };
			yield return new object[] { DateTime.UtcNow.TimeOfDay };
			yield return new object[] { new TimeSpan(0, 0, 0, 0, 0) };
		}

		[Theory]
		[MemberData(nameof(GetTimeSpanData))]
		public void TestingTimeSpan(TimeSpan init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadTimeSpan(Reader);

			final.Should().Be(init);
		}

		[Theory]
		[InlineData(null)]
		[MemberData(nameof(GetTimeSpanData))]
		public void TestingTimeSpanNullable(TimeSpan? init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadTimeSpanNullable(Reader);

			final.Should().Be(init);
		}


		public static IEnumerable<object[]> GetDateTimeData()
		{
			yield return new object[] { DateTime.MinValue };
			yield return new object[] { DateTime.MaxValue };
			yield return new object[] { DateTime.Now };
			yield return new object[] { DateTime.UtcNow };
			yield return new object[] { new DateTime(2018, 9, 1, 14, 29, 16, DateTimeKind.Utc) };
			yield return new object[] { new DateTime(2018, 9, 1, 19, 41, 16, DateTimeKind.Local) };
			yield return new object[] { new DateTime(2018, 9, 1, 19, 41, 16, DateTimeKind.Unspecified) };
		}

		[Theory]
		[MemberData(nameof(GetDateTimeData))]
		public void TestingDateTime(DateTime init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadDateTime(Reader);

			final.Should().Be(init);
		}

		[Theory]
		[InlineData(null)]
		[MemberData(nameof(GetDateTimeData))]
		public void TestingDateTimeNullable(DateTime? init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadDateTimeNullable(Reader);

			final.Should().Be(init);
		}


		public static IEnumerable<object[]> GetDateTimeOffsetData()
		{
			yield return new object[] { DateTimeOffset.MinValue };
			yield return new object[] { DateTimeOffset.MaxValue };
			yield return new object[] { DateTimeOffset.Now };
			yield return new object[] { DateTimeOffset.UtcNow };
			yield return new object[] { new DateTimeOffset(2018, 9, 1, 14, 30, 16, new TimeSpan(10, 0, 0)) };
		}

		[Theory]
		[MemberData(nameof(GetDateTimeOffsetData))]
		public void TestingDateTimeOffset(DateTimeOffset init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadDateTimeOffset(Reader);

			final.Should().Be(init);
		}

		[Theory]
		[InlineData(null)]
		[MemberData(nameof(GetDateTimeOffsetData))]
		public void TestingDateTimeOffsetNullable(DateTimeOffset? init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadDateTimeOffsetNullable(Reader);

			final.Should().Be(init);
		}


		public static IEnumerable<object[]> GetByteArrayData()
		{
			yield return new object[] { new byte[] { 0 } };
			yield return new object[] { new byte[] { 0, 1, 2, 100, 200 } };

			// BUG-CHECK: https://github.com/salarcode/Bois/issues/1
			yield return new object[] { new byte[ushort.MaxValue - 1] };
			yield return new object[] { new byte[ushort.MaxValue + 5] };
			yield return new object[] { new byte[ushort.MaxValue + 10] };
		}

		[Theory]
		[InlineData(null)]
		[MemberData(nameof(GetByteArrayData))]
		public void TestingByteArray(byte[] init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init);
			ResetStream();

			var final = PrimitiveReader.ReadByteArray(Reader);

			if (final != null && init != null)
				final.Should().BeEquivalentTo(init);
		}

		public static IEnumerable<object[]> GetDataSetData()
		{
			var table1 = new DataTable("Dt1")
			{
				Columns =
				{
					new DataColumn("Col1", typeof(int)),
					new DataColumn("Col2", typeof(string)),
					new DataColumn("Col3"),
				}
			};
			table1.Rows.Add(new object[] { 10, "Test1", true });
			table1.Rows.Add(new object[] { 20, null, true });
			table1.Rows.Add(new object[] { 500, "Test3", true });

			yield return new object[]
			{
				new DataSet()
				{
					Tables =
					{
						table1
					}
				}
			};

			yield return new object[] { new DataSet("DsName") };
		}

		[Theory]
		[InlineData(null)]
		[MemberData(nameof(GetDataSetData))]
		public void TestingDataSet(DataSet init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init, Encoding.UTF8);
			ResetStream();

			var final = PrimitiveReader.ReadDataSet(Reader, Encoding.UTF8);

			SerializeAreEqual(init, final);
		}

		public static IEnumerable<object[]> GetDataTableData()
		{
			yield return new object[] { new DataTable("Name") };
			yield return new object[] { new DataTable() };
			var table1 = new DataTable("Dt1")
			{
				Columns =
				{
					new DataColumn("Col1", typeof(int)),
					new DataColumn("Col2", typeof(string)),
					new DataColumn("Col3"),
				}
			};
			table1.Rows.Add(new object[] { 10, "Test1", true });
			table1.Rows.Add(new object[] { 20, null, true });
			table1.Rows.Add(new object[] { 500, "Test3", true });
			yield return new object[] { table1 };

			var table2 = new DataTable("Dt2") { };
			table1.Rows.Add(new object[] { 10, "Test1", true });
			table1.Rows.Add(new object[] { 20, null, true });
			table1.Rows.Add(new object[] { 500, "Test3", true });
			yield return new object[] { table2 };

		}

		[Theory]
		[InlineData(null)]
		[MemberData(nameof(GetDataTableData))]
		public void TestingDataTable(DataTable init)
		{
			ResetBois();

			PrimitiveWriter.WriteValue(Writer, init, Encoding.UTF8);
			ResetStream();

			var final = PrimitiveReader.ReadDataTable(Reader, Encoding.UTF8);

			SerializeAreEqual(init, final);
		}
	}
}
