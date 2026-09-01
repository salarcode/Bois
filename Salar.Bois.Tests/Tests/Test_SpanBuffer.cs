using Salar.Bois.NetFx.Tests.Base;
using Salar.Bois.NetFx.Tests.TestObjects;
using System;
using System.Collections.Generic;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Salar.Bois.NetFx.Tests.Tests
{
	/// <summary>
	/// Covers the byte[] overloads which use BinarySpanBufferWriter/BinarySpanBufferReader on modern .NET.
	/// These exercise the emitted serializers closed over a ref struct buffer type.
	/// </summary>
	public class Test_SpanBuffer : TestBase
	{
		private const int BufferSize = 4096;

		public class EnumMembers
		{
			public TestEnumsNormal Normal { get; set; }
			public TestsEnumUInt UInt { get; set; }
			public TestsEnumByte Byte { get; set; }
			public TestsEnumShort Short { get; set; }
			public TestsEnumInt64 Int64 { get; set; }
			public DayOfWeek Day { get; set; }
		}

		public class NullableEnumMembers
		{
			public TestEnumsNormal? Normal { get; set; }
			public TestsEnumUInt? UInt { get; set; }
			public TestsEnumByte? Byte { get; set; }
			public TestsEnumShort? Short { get; set; }
			public TestsEnumInt64? Int64 { get; set; }
		}

		public class MixedMembers
		{
			public string Name { get; set; }
			public int Number { get; set; }
			public TestEnumsNormal Enum { get; set; }
			public TestsEnumByte? NullableEnum { get; set; }
			public DateTime Date { get; set; }
			public List<TestsEnumShort> Enums { get; set; }
		}

		[Fact]
		public void EnumMembersRoundTripThroughSpanBuffer()
		{
			var init = new EnumMembers
			{
				Normal = TestEnumsNormal.Eleven,
				UInt = TestsEnumUInt.UInt2,
				Byte = TestsEnumByte.Byte2,
				Short = TestsEnumShort.Short2,
				Int64 = TestsEnumInt64.long2,
				Day = DayOfWeek.Wednesday
			};

			var final = RoundTrip(init);

			SerializeAreEqual(init, final);
		}

		[Fact]
		public void NullableEnumMembersWithValuesRoundTripThroughSpanBuffer()
		{
			var init = new NullableEnumMembers
			{
				Normal = TestEnumsNormal.Twelve,
				UInt = TestsEnumUInt.UInt1,
				Byte = TestsEnumByte.Byte1,
				Short = TestsEnumShort.Short1,
				Int64 = TestsEnumInt64.Long1
			};

			var final = RoundTrip(init);

			SerializeAreEqual(init, final);
		}

		[Fact]
		public void NullableEnumMembersWithNullsRoundTripThroughSpanBuffer()
		{
			var init = new NullableEnumMembers
			{
				Normal = null,
				UInt = null,
				Byte = null,
				Short = null,
				Int64 = null
			};

			var final = RoundTrip(init);

			SerializeAreEqual(init, final);
		}

		[Fact]
		public void MixedMembersRoundTripThroughSpanBuffer()
		{
			var init = new MixedMembers
			{
				Name = "span buffer",
				Number = 1234567,
				Enum = TestEnumsNormal.Ten,
				NullableEnum = TestsEnumByte.Byte2,
				Date = new DateTime(2024, 5, 17, 8, 30, 0, DateTimeKind.Utc),
				Enums = new List<TestsEnumShort> { TestsEnumShort.Short1, TestsEnumShort.Short2 }
			};

			var final = RoundTrip(init);

			SerializeAreEqual(init, final);
		}

		[Theory]
		[InlineData(TestEnumsNormal.Ten)]
		[InlineData(TestEnumsNormal.Eleven)]
		[InlineData(TestEnumsNormal.Twelve)]
		public void EachEnumValueRoundTripsThroughSpanBuffer(TestEnumsNormal value)
		{
			var init = new EnumMembers { Normal = value };

			var final = RoundTrip(init);

			Assert.Equal(value, final.Normal);
		}

		[Fact]
		public void SpanBufferAndStreamProduceIdenticalPayload()
		{
			var init = new EnumMembers
			{
				Normal = TestEnumsNormal.Twelve,
				UInt = TestsEnumUInt.UInt2,
				Byte = TestsEnumByte.Byte1,
				Short = TestsEnumShort.Short2,
				Int64 = TestsEnumInt64.Long1,
				Day = DayOfWeek.Saturday
			};

			ResetBois();
			BoisSerializer.Initialize(typeof(EnumMembers));

			Bois.Serialize(init, TestStream);
			var streamPayload = TestStream.ToArray();

			var spanBuffer = new byte[BufferSize];
			Bois.Serialize(init, spanBuffer, 0, spanBuffer.Length);

			var spanPayload = new byte[streamPayload.Length];
			Array.Copy(spanBuffer, spanPayload, streamPayload.Length);

			Assert.Equal(streamPayload, spanPayload);
		}

		private T RoundTrip<T>(T init)
		{
			BoisSerializer.Initialize(typeof(T));

			var buffer = new byte[BufferSize];
			Bois.Serialize(init, buffer, 0, buffer.Length);

			return Bois.Deserialize<T>(buffer, 0, buffer.Length);
		}
	}
}
