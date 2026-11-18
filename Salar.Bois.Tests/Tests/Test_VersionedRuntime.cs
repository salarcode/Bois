using Salar.BinaryBuffers;
using Salar.Bois.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

#nullable disable

namespace Salar.Bois.NetFx.Tests.Tests;

#region Versioned test types

// Bois stores no type names, so writing with one class and reading with another acts like two app versions.

[BoisContract(Versioned = true)]
[BoisReserved(2, typeof(string))]
[BoisReserved(4, typeof(BoisSkip.Contract))]
public class VrOrder
{
	[BoisMember(1)] public int Id { get; set; }
	[BoisMember(3)] public decimal? Discount { get; set; }
	[BoisMember(5)] public List<VrOrderLine> Lines { get; set; }
	[BoisMember(false)] public string DisplayText { get; set; }
}

[BoisContract(Versioned = true)]
public class VrOrderLine
{
	[BoisMember(1)] public string Sku { get; set; }
	[BoisMember(2)] public int Quantity { get; set; }
}

[BoisContract(Versioned = true)]
public class VrAddress
{
	[BoisMember(1)] public string City { get; set; }
	[BoisMember(2)] public string Street { get; set; }
}

[BoisContract(Versioned = true)]
public class VrCustomerV1
{
	[BoisMember(1)] public int Id { get; set; }
	[BoisMember(2)] public string Name { get; set; }
	[BoisMember(3)] public VrAddress Address { get; set; }
}

[BoisContract(Versioned = true)]
public class VrCustomerV2
{
	[BoisMember(1)] public int Id { get; set; }
	[BoisMember(2)] public string Name { get; set; }
	[BoisMember(3)] public VrAddress Address { get; set; }
	[BoisMember(4)] public string Email { get; set; } = "ctor-email";
	[BoisMember(5)] public List<string> Tags { get; set; } = ["ctor-tag"];
}

/// <summary>V2 with Name, Address and Tags removed. Tags was the last slot, so slot 5 is not written.</summary>
[BoisContract(Versioned = true)]
[BoisReserved(2, typeof(string))]
[BoisReserved(3, typeof(BoisSkip.Contract))]
[BoisReserved(5, typeof(List<string>))]
public class VrCustomerV3
{
	[BoisMember(1)] public int Id { get; set; }
	[BoisMember(4)] public string Email { get; set; } = "ctor-email";
}

/// <summary>The same members as <see cref="VrCustomerV1"/> in the same order, before it became versioned.</summary>
public class VrLegacyCustomer
{
	public int Id { get; set; }
	public string Name { get; set; }
	public VrLegacyAddress Address { get; set; }
}

public class VrLegacyAddress
{
	public string City { get; set; }
	public string Street { get; set; }
}

[BoisContract(Versioned = true)]
public class VrItemV1
{
	[BoisMember(1)] public int Value { get; set; }
}

[BoisContract(Versioned = true)]
public class VrItemV2
{
	[BoisMember(1)] public int Value { get; set; }
	[BoisMember(2)] public string Note { get; set; } = "ctor-note";
}

[BoisContract(Versioned = true)]
public class VrBagV1
{
	[BoisMember(1)] public List<VrItemV1> Items { get; set; }
	[BoisMember(2)] public Dictionary<string, VrItemV1> ByName { get; set; }
	[BoisMember(3)] public VrItemV1[] Array { get; set; }
}

[BoisContract(Versioned = true)]
public class VrBagV2
{
	[BoisMember(1)] public List<VrItemV2> Items { get; set; }
	[BoisMember(2)] public Dictionary<string, VrItemV2> ByName { get; set; }
	[BoisMember(3)] public VrItemV2[] Array { get; set; }
}

public class VrLegacyItem
{
	public int Value { get; set; }
}

/// <summary>A normal class holding versioned items, and the reverse in <see cref="VrBagV1"/>.</summary>
public class VrLegacyBag
{
	public List<VrLegacyItem> Items { get; set; }
}

public class VrNormalHolder
{
	public string Title { get; set; }
	public VrCustomerV2 Customer { get; set; }
	public List<VrItemV2> Items { get; set; }
}

[BoisContract(Versioned = true)]
public class VrBig
{
	[BoisMember(1)] public string Text { get; set; }
	[BoisMember(2)] public List<VrBig> Children { get; set; }
	[BoisMember(3)] public VrBig Next { get; set; }
}

[BoisContract(Versioned = true)]
public struct VrPoint
{
	[BoisMember(1)] public int X;
	[BoisMember(2)] public int Y;
}

[BoisContract(Versioned = true)]
public class VrShape
{
	[BoisMember(1)] public VrPoint Center { get; set; }
	[BoisMember(2)] public VrPoint? Corner { get; set; }
	[BoisMember(3)] public List<VrPoint> Points { get; set; }
}

[BoisContract(Versioned = true)]
public class VrBase
{
	[BoisMember(1)] public int BaseId { get; set; }
}

[BoisContract(Versioned = true)]
public class VrDerived : VrBase
{
	[BoisMember(2)] public string Name { get; set; }
}

[BoisContract(Versioned = true)]
public class VrTwoInts
{
	[BoisMember(1)] public int A { get; set; }
	[BoisMember(2)] public int B { get; set; }
}

public enum VrSmallEnum : byte { None, One, Two }

public enum VrIntEnum { None, Big = 100_000 }

public struct VrPlainStruct
{
	public int A;
	public int B;
}

public class VrPlainClass
{
	public string Name { get; set; }
	public int Value { get; set; }
}

/// <summary>Every kind of value in slots 1..33, then a tail slot.</summary>
[BoisContract(Versioned = true)]
public class VrAllTypesV1
{
	[BoisMember(1)] public int Int { get; set; }
	[BoisMember(2)] public long? NullableLong { get; set; }
	[BoisMember(3)] public string String { get; set; }
	[BoisMember(4)] public byte[] Bytes { get; set; }
	[BoisMember(5)] public decimal Decimal { get; set; }
	[BoisMember(6)] public double? NullableDouble { get; set; }
	[BoisMember(7)] public float Float { get; set; }
	[BoisMember(8)] public bool Bool { get; set; }
	[BoisMember(9)] public char Char { get; set; }
	[BoisMember(10)] public Guid Guid { get; set; }
	[BoisMember(11)] public Guid? NullableGuid { get; set; }
	[BoisMember(12)] public DateTime DateTime { get; set; }
	[BoisMember(13)] public DateTimeOffset? NullableDateTimeOffset { get; set; }
	[BoisMember(14)] public TimeSpan TimeSpan { get; set; }
	[BoisMember(15)] public VrSmallEnum SmallEnum { get; set; }
	[BoisMember(16)] public VrIntEnum? NullableIntEnum { get; set; }
	[BoisMember(17)] public List<int> IntList { get; set; }
	[BoisMember(18)] public List<string> StringList { get; set; }
	[BoisMember(19)] public int[] IntArray { get; set; }
	[BoisMember(20)] public Dictionary<string, int> Dictionary { get; set; }
	[BoisMember(21)] public VrItemV2 Versioned { get; set; }
	[BoisMember(22)] public VrPoint VersionedStruct { get; set; }
	[BoisMember(23)] public VrPlainStruct PlainStruct { get; set; }
	[BoisMember(24)] public VrPlainClass PlainClass { get; set; }
	[BoisMember(25)] public List<VrPlainClass> PlainClassList { get; set; }
	[BoisMember(26)] public Uri Uri { get; set; }
	[BoisMember(27)] public DateOnly DateOnly { get; set; }
	[BoisMember(28)] public short Short { get; set; }
	[BoisMember(29)] public ulong ULong { get; set; }
	[BoisMember(30)] public sbyte SByte { get; set; }
	[BoisMember(31)] public byte Byte { get; set; }
	[BoisMember(32)] public List<bool> BoolList { get; set; }
	[BoisMember(33)] public VrPoint? NullableVersionedStruct { get; set; }
	[BoisMember(34)] public int Tail { get; set; }
}

/// <summary><see cref="VrAllTypesV1"/> with every member but the tail removed.</summary>
[BoisContract(Versioned = true)]
[BoisReserved(1, typeof(int))]
[BoisReserved(2, typeof(long?))]
[BoisReserved(3, typeof(string))]
[BoisReserved(4, typeof(byte[]))]
[BoisReserved(5, typeof(decimal))]
[BoisReserved(6, typeof(double?))]
[BoisReserved(7, typeof(float))]
[BoisReserved(8, typeof(bool))]
[BoisReserved(9, typeof(char))]
[BoisReserved(10, typeof(Guid))]
[BoisReserved(11, typeof(Guid?))]
[BoisReserved(12, typeof(DateTime))]
[BoisReserved(13, typeof(DateTimeOffset?))]
[BoisReserved(14, typeof(TimeSpan))]
[BoisReserved(15, typeof(VrSmallEnum))]
[BoisReserved(16, typeof(VrIntEnum?))]
[BoisReserved(17, typeof(List<int>))]
[BoisReserved(18, typeof(List<string>))]
[BoisReserved(19, typeof(int[]))]
[BoisReserved(20, typeof(Dictionary<string, int>))]
[BoisReserved(21, typeof(BoisSkip.Contract))]
[BoisReserved(22, typeof(VrPoint))]
[BoisReserved(23, typeof(VrPlainStruct))]
[BoisReserved(24, typeof(VrPlainClass))]
[BoisReserved(25, typeof(List<VrPlainClass>))]
[BoisReserved(26, typeof(Uri))]
[BoisReserved(27, typeof(DateOnly))]
[BoisReserved(28, typeof(short))]
[BoisReserved(29, typeof(ulong))]
[BoisReserved(30, typeof(sbyte))]
[BoisReserved(31, typeof(byte))]
[BoisReserved(32, typeof(List<bool>))]
[BoisReserved(33, typeof(VrPoint?))]
public class VrAllTypesV2
{
	[BoisMember(34)] public int Tail { get; set; }
}

#endregion

/// <summary>
/// Versioned types ([BoisContract(Versioned = true)]) in the runtime serializer.
/// </summary>
public class Test_VersionedRuntime
{
	private static readonly BoisSerializer Bois = new() { Encoding = Encoding.UTF8 };

	private sealed class NonSeekableStream(Stream inner) : Stream
	{
		public override bool CanRead => inner.CanRead;
		public override bool CanSeek => false;
		public override bool CanWrite => inner.CanWrite;
		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		public override void Flush() => inner.Flush();
		public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
		// Stream.ReadByte allocates a byte[1] per call unless overridden
		public override int ReadByte() => inner.ReadByte();
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
	}

	/// <summary>
	/// Writes with every supported writer and checks they all give the same bytes.
	/// </summary>
	private static byte[] Write<T>(T value)
	{
		using var memory = new MemoryStream();
		Bois.Serialize(value, memory);
		var expected = memory.ToArray();

		using (var output = new MemoryStream())
		{
			Bois.Serialize(value, new NonSeekableStream(output));
			Assert.Equal(expected, output.ToArray());
		}

		using (var output = new MemoryStream())
		{
			// seekable, but not a MemoryStream
			using var buffered = new BufferedStream(output);
			Bois.Serialize(value, buffered);
			buffered.Flush();
			Assert.Equal(expected, output.ToArray());
		}

		using (var output = new MemoryStream())
		{
			Bois.Serialize(value, typeof(T), output);
			Assert.Equal(expected, output.ToArray());
		}

		var array = new byte[expected.Length + 3];
		Bois.Serialize(value, array, 3, expected.Length);
		Assert.Equal(expected, array.Skip(3).ToArray());

		var binaryWriter = new BinaryBufferWriter(new byte[expected.Length]);
		Bois.Serialize(value, ref binaryWriter);
		Assert.Equal(expected, binaryWriter.ToArray());

		var spanWriter = new BinarySpanBufferWriter(new byte[expected.Length]);
		Bois.Serialize(value, ref spanWriter);
		Assert.Equal(expected, spanWriter.ToArray());

		var boisWriter = new BoisBufferWriter(new byte[expected.Length]);
		Bois.Serialize(value, ref boisWriter);
		Assert.Equal(expected, boisWriter.ToArray());

		return expected;
	}

	/// <summary>
	/// Reads with every supported reader, checks each one leaves the reader at the end, and returns them all.
	/// </summary>
	private static List<T> ReadAll<T>(byte[] data)
	{
		var results = new List<T>
		{
			Bois.Deserialize<T>(data, 0, data.Length),
			Bois.Deserialize<T>(new MemoryStream(data)),
			(T)Bois.Deserialize(new MemoryStream(data), typeof(T)),
		};

		var nonSeekable = new MemoryStream(data);
		results.Add(Bois.Deserialize<T>(new NonSeekableStream(nonSeekable)));
		Assert.Equal(data.Length, nonSeekable.Position);

		var nonSeekableByType = new MemoryStream(data);
		results.Add((T)Bois.Deserialize(new NonSeekableStream(nonSeekableByType), typeof(T)));
		Assert.Equal(data.Length, nonSeekableByType.Position);

		var seekable = new MemoryStream(data);
		results.Add(Bois.Deserialize<T>(new BufferedStream(seekable)));

		var binaryReader = new BinaryBufferReader(data);
		results.Add(Bois.Deserialize<T, BinaryBufferReader>(ref binaryReader));
		Assert.Equal(data.Length, binaryReader.Position);

		var spanReader = new BinarySpanBufferReader(data);
		results.Add(Bois.Deserialize<T, BinarySpanBufferReader>(ref spanReader));
		Assert.Equal(data.Length, spanReader.Position);

		return results;
	}

	private static T Read<T>(byte[] data)
	{
		var results = ReadAll<T>(data);
		var first = results[0];
		for (var i = 1; i < results.Count; i++)
		{
			try
			{
				Assert.Equivalent(first, results[i], strict: true);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Reader #{i} read a different value than the byte[] reader.", ex);
			}
		}
		return first;
	}

	private static string Hex(byte[] data) => Convert.ToHexString(data);

	#region Format

	[Fact]
	public void OrderExampleIsSixBytes()
	{
		var data = Write(new VrOrder { Id = 7 });

		// size 5, Id = 7 (zigzag 0E, as Bois writes an int today), then null in slots 2, 3, 4 and 5
		Assert.Equal("C50E40404040", Hex(data));
		Assert.Equal(Hex(Write(7)), Hex(data.Skip(1).Take(1).ToArray()));

		var read = Read<VrOrder>(data);
		Assert.Equal(7, read.Id);
		Assert.Null(read.Discount);
		Assert.Null(read.Lines);
	}

	[Fact]
	public void SlotsAreTheSameBytesAsTodaysMembers()
	{
		// a versioned object is today's object with the size in place of the member count
		var versioned = Write(new VrCustomerV1 { Id = 300, Name = "Ann", Address = new VrAddress { City = "X", Street = null } });
		var legacy = Write(new VrLegacyCustomer { Id = 300, Name = "Ann", Address = new VrLegacyAddress { City = "X", Street = null } });

		Assert.Equal(legacy.Length, versioned.Length);
		Assert.Equal(0x83, legacy[0]);
		Assert.Equal(0xC0 | (versioned.Length - 1), versioned[0]);
		Assert.Equal(legacy.Skip(1).Take(legacy.Length - 5), versioned.Skip(1).Take(versioned.Length - 5));

		// the nested address: a member count of 2 today, a size of 3 bytes versioned
		Assert.Equal(0x82, legacy[legacy.Length - 4]);
		Assert.Equal(0xC3, versioned[versioned.Length - 4]);
	}

	[Fact]
	public void BigObjectsUseTheLongPrefix()
	{
		var value = new VrBig { Text = new string('a', 100) };
		var data = Write(value);

		Assert.Equal(0x01, data[0]);
		Assert.Equal(new string('a', 100), Read<VrBig>(data).Text);
	}

	[Fact]
	public void NestedBigObjects()
	{
		var value = new VrBig
		{
			Text = new string('r', 70),
			Children = Enumerable.Range(0, 30).Select(i => new VrBig { Text = new string((char)('a' + i % 26), 250 + i) }).ToList(),
		};

		// a long chain: every level is bigger than 63 bytes, and each one moves with its parent
		var tail = value;
		for (var i = 0; i < 60; i++)
		{
			tail.Next = new VrBig { Text = new string('n', 65), Children = [new VrBig { Text = i.ToString() }] };
			tail = tail.Next;
		}

		var data = Write(value);
		foreach (var read in ReadAll<VrBig>(data))
			AssertSameBig(value, read);
		Assert.True(data.Length > 8 * 1024, "bigger than the stream writer's buffer");
	}

	/// <summary>
	/// Assert.Equivalent stops at a depth of 50, and the chain is deeper.
	/// </summary>
	private static void AssertSameBig(VrBig expected, VrBig actual)
	{
		for (; expected != null; expected = expected.Next, actual = actual.Next)
		{
			Assert.NotNull(actual);
			Assert.Equal(expected.Text, actual.Text);
			Assert.Equal(expected.Children?.Count, actual.Children?.Count);
			for (var i = 0; i < (expected.Children?.Count ?? 0); i++)
				AssertSameBig(expected.Children[i], actual.Children[i]);
		}
		Assert.Null(actual);
	}

	[Fact]
	public void NullNestedObjectsAndItems()
	{
		var value = new VrBagV1
		{
			Items = [new VrItemV1 { Value = 1 }, null, new VrItemV1 { Value = 3 }],
			ByName = new Dictionary<string, VrItemV1> { ["a"] = null, ["b"] = new VrItemV1 { Value = 2 } },
			Array = [null],
		};

		var read = Read<VrBagV1>(Write(value));
		Assert.Equivalent(value, read, strict: true);

		var empty = Read<VrBagV1>(Write(new VrBagV1()));
		Assert.Null(empty.Items);
		Assert.Null(empty.ByName);
		Assert.Null(empty.Array);
	}

	[Fact]
	public void RootCollectionsOfVersionedObjects()
	{
		var list = new List<VrItemV2> { new() { Value = 1, Note = new string('a', 100) }, null, new() { Value = 2 } };
		Assert.Equivalent(list, Read<List<VrItemV2>>(Write(list)), strict: true);

		var dictionary = new Dictionary<int, VrItemV2> { [1] = new() { Value = 1 }, [2] = null };
		Assert.Equivalent(dictionary, Read<Dictionary<int, VrItemV2>>(Write(dictionary)), strict: true);

		var array = new[] { new VrPoint { X = 1 }, new VrPoint { Y = 2 } };
		Assert.Equal(array, Read<VrPoint[]>(Write(array)));

		// the old app reads the new items as a list of the old class
		Assert.Equal(new[] { 1, 2 }, Read<List<VrItemV1>>(Write(new List<VrItemV2> { new() { Value = 1 }, new() { Value = 2 } })).Select(i => i.Value));
	}

	[Fact]
	public void NullRootReadsAsNull()
	{
		Assert.Null(Bois.Deserialize<VrOrder>([0x40], 0, 1));
	}

	[Fact]
	public void VersionedStruct()
	{
		var point = new VrPoint { X = 5, Y = -6 };
		var data = Write(point);
		Assert.Equal(0xC2, data[0]);
		Assert.Equal(point, Read<VrPoint>(data));

		var shape = new VrShape
		{
			Center = new VrPoint { X = 1, Y = 2 },
			Corner = new VrPoint { X = 3, Y = 4 },
			Points = [new VrPoint { X = 7, Y = 8 }, default],
		};
		Assert.Equivalent(shape, Read<VrShape>(Write(shape)), strict: true);

		var noCorner = new VrShape { Center = new VrPoint { X = 9 } };
		Assert.Equivalent(noCorner, Read<VrShape>(Write(noCorner)), strict: true);
	}

	[Fact]
	public void Inheritance()
	{
		var value = new VrDerived { BaseId = 11, Name = "derived" };
		var data = Write(value);
		var read = Read<VrDerived>(data);
		Assert.Equal(11, read.BaseId);
		Assert.Equal("derived", read.Name);

		// the base class reads the first slot and skips the rest
		Assert.Equal(11, Read<VrBase>(data).BaseId);
	}

	[Fact]
	public void VersionedInsideNormal()
	{
		var value = new VrNormalHolder
		{
			Title = "holder",
			Customer = new VrCustomerV2 { Id = 1, Email = "e", Tags = ["t"] },
			Items = [new VrItemV2 { Value = 2, Note = "n" }],
		};
		Assert.Equivalent(value, Read<VrNormalHolder>(Write(value)), strict: true);
	}

	#endregion

	#region Compatibility

	private static VrCustomerV2 CustomerV2() => new()
	{
		Id = 42,
		Name = "Jane",
		Address = new VrAddress { City = "Paris", Street = new string('s', 80) },
		Email = "jane@example.com",
		Tags = ["a", "b"],
	};

	[Fact]
	public void MemberAdded_OldDataKeepsConstructorValues()
	{
		var read = Read<VrCustomerV2>(Write(new VrCustomerV1 { Id = 1, Name = "n", Address = new VrAddress { City = "c" } }));

		Assert.Equal(1, read.Id);
		Assert.Equal("n", read.Name);
		Assert.Equal("c", read.Address.City);
		Assert.Equal("ctor-email", read.Email);
		Assert.Equal(new[] { "ctor-tag" }, read.Tags);
	}

	[Fact]
	public void MemberAdded_OldAppSkipsNewSlots()
	{
		var value = CustomerV2();
		var read = Read<VrCustomerV1>(Write(value));

		Assert.Equal(42, read.Id);
		Assert.Equal("Jane", read.Name);
		Assert.Equivalent(value.Address, read.Address, strict: true);
	}

	[Fact]
	public void MemberRemoved_ReservedSlotsAreSkippedByType()
	{
		// Name (string), Address (BoisSkip.Contract, skipped by its size) and Tags (List<string>)
		var read = Read<VrCustomerV3>(Write(CustomerV2()));

		Assert.Equal(42, read.Id);
		Assert.Equal("jane@example.com", read.Email);
	}

	[Fact]
	public void MemberRemoved_OldAppReadsNullsAndKeepsConstructorValues()
	{
		var data = Write(new VrCustomerV3 { Id = 5, Email = "x" });

		// slot 5 was the last member, so it is not written
		var read = Read<VrCustomerV2>(data);
		Assert.Equal(5, read.Id);
		Assert.Null(read.Name);
		Assert.Null(read.Address);
		Assert.Equal("x", read.Email);
		Assert.Equal(new[] { "ctor-tag" }, read.Tags);
	}

	[Fact]
	public void ListAndDictionaryItemsChange()
	{
		var v1 = new VrBagV1
		{
			Items = [new VrItemV1 { Value = 1 }, new VrItemV1 { Value = 2 }],
			ByName = new Dictionary<string, VrItemV1> { ["x"] = new VrItemV1 { Value = 3 } },
			Array = [new VrItemV1 { Value = 4 }],
		};
		var asV2 = Read<VrBagV2>(Write(v1));
		Assert.Equal(new[] { 1, 2 }, asV2.Items.Select(i => i.Value));
		Assert.All(asV2.Items, i => Assert.Equal("ctor-note", i.Note));
		Assert.Equal(3, asV2.ByName["x"].Value);
		Assert.Equal("ctor-note", asV2.ByName["x"].Note);
		Assert.Equal(4, asV2.Array[0].Value);

		var v2 = new VrBagV2
		{
			Items = [new VrItemV2 { Value = 5, Note = "five" }, new VrItemV2 { Value = 6, Note = new string('z', 100) }],
			ByName = new Dictionary<string, VrItemV2> { ["y"] = new VrItemV2 { Value = 7, Note = "seven" } },
			Array = [new VrItemV2 { Value = 8 }],
		};
		var asV1 = Read<VrBagV1>(Write(v2));
		Assert.Equal(new[] { 5, 6 }, asV1.Items.Select(i => i.Value));
		Assert.Equal(7, asV1.ByName["y"].Value);
		Assert.Equal(8, asV1.Array[0].Value);
	}

	[Fact]
	public void SameVersion()
	{
		var value = CustomerV2();
		Assert.Equivalent(value, Read<VrCustomerV2>(Write(value)), strict: true);

		var order = new VrOrder
		{
			Id = 1,
			Discount = 2.5m,
			Lines = [new VrOrderLine { Sku = "a", Quantity = 2 }, new VrOrderLine { Sku = "b", Quantity = 3 }],
			DisplayText = "not written",
		};
		var read = Read<VrOrder>(Write(order));
		Assert.Null(read.DisplayText);
		order.DisplayText = null;
		Assert.Equivalent(order, read, strict: true);
	}

	[Fact]
	public void SwitchToVersioned_ReadsTodaysFormat()
	{
		var legacy = new VrLegacyCustomer { Id = 9, Name = "old", Address = new VrLegacyAddress { City = "c", Street = "s" } };
		var read = Read<VrCustomerV2>(Write(legacy));

		Assert.Equal(9, read.Id);
		Assert.Equal("old", read.Name);
		Assert.Equal("c", read.Address.City);
		Assert.Equal("s", read.Address.Street);
		Assert.Equal("ctor-email", read.Email);

		// nested in a list
		var bag = Read<VrBagV2>(Write(new VrLegacyBag { Items = [new VrLegacyItem { Value = 1 }, new VrLegacyItem { Value = 2 }] }));
		Assert.Equal(1, bag.Items[0].Value);
		Assert.Equal(2, bag.Items[1].Value);
		Assert.Equal("ctor-note", bag.Items[1].Note);
		Assert.Null(bag.ByName);

		// today's format with a reserved slot: member 2 is skipped as a string
		var order = Read<VrOrder>(Write(new VrAllTypesLegacyPrefix { A = 3, B = "skipped", C = 1.5m }));
		Assert.Equal(3, order.Id);
		Assert.Equal(1.5m, order.Discount);
	}

	[Fact]
	public void SwitchToVersioned_OldAppReadsVersionedData()
	{
		var value = new VrCustomerV1 { Id = 9, Name = "new", Address = new VrAddress { City = "c", Street = new string('s', 70) } };
		var read = Read<VrLegacyCustomer>(Write(value));

		Assert.Equal(9, read.Id);
		Assert.Equal("new", read.Name);
		Assert.Equal("c", read.Address.City);
		Assert.Equal(value.Address.Street, read.Address.Street);
	}

	private static VrAllTypesV1 AllTypes() => new()
	{
		Int = -12345,
		NullableLong = long.MaxValue,
		String = "string value",
		Bytes = [1, 2, 3, 4, 5],
		Decimal = 12345.6789m,
		NullableDouble = Math.PI,
		Float = 1.5f,
		Bool = true,
		Char = 'Z',
		Guid = Guid.NewGuid(),
		NullableGuid = Guid.NewGuid(),
		DateTime = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc),
		NullableDateTimeOffset = new DateTimeOffset(2020, 5, 6, 7, 8, 9, TimeSpan.FromHours(3)),
		TimeSpan = TimeSpan.FromMinutes(77),
		SmallEnum = VrSmallEnum.Two,
		NullableIntEnum = VrIntEnum.Big,
		IntList = [1, -1, 1000, int.MinValue],
		StringList = ["a", null, new string('b', 90)],
		IntArray = [5, 6, 7],
		Dictionary = new Dictionary<string, int> { ["k"] = 1, ["l"] = 2 },
		Versioned = new VrItemV2 { Value = 77, Note = new string('n', 70) },
		VersionedStruct = new VrPoint { X = 1, Y = 2 },
		PlainStruct = new VrPlainStruct { A = 3, B = 4 },
		PlainClass = new VrPlainClass { Name = "plain", Value = 5 },
		// no null items: today's format can't write a null class item in a list
		PlainClassList = [new VrPlainClass { Name = "p1", Value = 1 }, new VrPlainClass { Name = "p2" }],
		Uri = new Uri("https://example.com/a?b=c"),
		DateOnly = new DateOnly(2001, 2, 3),
		Short = -300,
		ULong = ulong.MaxValue,
		SByte = -5,
		Byte = 250,
		BoolList = [true, false, true],
		NullableVersionedStruct = new VrPoint { X = 8, Y = 9 },
		Tail = 123456,
	};

	[Fact]
	public void EveryReservedTypeIsSkipped()
	{
		var read = Read<VrAllTypesV2>(Write(AllTypes()));
		Assert.Equal(123456, read.Tail);
	}

	[Fact]
	public void EveryReservedTypeIsWrittenAsItsDefault()
	{
		var read = Read<VrAllTypesV1>(Write(new VrAllTypesV2 { Tail = 99 }));

		Assert.Equivalent(new VrAllTypesV1 { Tail = 99 }, read, strict: true);
	}

	[Fact]
	public void EveryTypeRoundTrips()
	{
		var value = AllTypes();
		Assert.Equivalent(value, Read<VrAllTypesV1>(Write(value)), strict: true);
	}

	[Fact]
	public void SkippingAllocatesNothing()
	{
		var withData = Write(AllTypes());
		var withoutData = Write(new VrAllTypesV2 { Tail = 1 });

		// warm up both, so the code is generated and the shared buffers exist
		for (var i = 0; i < 3; i++)
		{
			Bois.Deserialize<VrAllTypesV2>(withData, 0, withData.Length);
			Bois.Deserialize<VrAllTypesV2>(withoutData, 0, withoutData.Length);
			Bois.Deserialize<VrAllTypesV2>(new NonSeekableStream(new MemoryStream(withData)));
			Bois.Deserialize<VrAllTypesV2>(new NonSeekableStream(new MemoryStream(withoutData)));
		}

		var plain = Allocated(() => Bois.Deserialize<VrAllTypesV2>(withoutData, 0, withoutData.Length));
		var skipping = Allocated(() => Bois.Deserialize<VrAllTypesV2>(withData, 0, withData.Length));
		Assert.Equal(plain, skipping);

		var withDataStream = new MemoryStream(withData);
		var withoutDataStream = new MemoryStream(withoutData);
		var plainStream = Allocated(() =>
		{
			withoutDataStream.Position = 0;
			Bois.Deserialize<VrAllTypesV2>(new NonSeekableStream(withoutDataStream));
		});
		var skippingStream = Allocated(() =>
		{
			withDataStream.Position = 0;
			Bois.Deserialize<VrAllTypesV2>(new NonSeekableStream(withDataStream));
		});
		Assert.Equal(plainStream, skippingStream);
	}

	[Fact]
	public void WritingAllocatesTheSameAsTodaysFormat()
	{
		var versioned = new VrCustomerV1 { Id = 1, Name = new string('x', 200), Address = new VrAddress { City = "c" } };
		var legacy = new VrLegacyCustomer { Id = 1, Name = new string('x', 200), Address = new VrLegacyAddress { City = "c" } };
		var buffer = new byte[1024];
		for (var i = 0; i < 3; i++)
		{
			Bois.Serialize(versioned, buffer, 0, buffer.Length);
			Bois.Serialize(legacy, buffer, 0, buffer.Length);
		}

		Assert.Equal(
			Allocated(() => Bois.Serialize(legacy, buffer, 0, buffer.Length)),
			Allocated(() => Bois.Serialize(versioned, buffer, 0, buffer.Length)));
	}

	[Fact]
	public void WritingToAStreamAllocatesNoMoreThanTodaysFormat()
	{
		// big enough for nested objects over 63 bytes, which wait for the outermost object to move
		var versioned = new VrCustomerV1 { Id = 1, Name = new string('x', 200), Address = new VrAddress { City = new string('c', 100) } };
		var legacy = new VrLegacyCustomer { Id = 1, Name = new string('x', 200), Address = new VrLegacyAddress { City = new string('c', 100) } };
		var output = new MemoryStream();
		for (var i = 0; i < 3; i++)
		{
			output.Position = 0;
			Bois.Serialize(versioned, output);
			output.Position = 0;
			Bois.Serialize(legacy, output);
		}

		var legacyBytes = Allocated(() => { output.Position = 0; Bois.Serialize(legacy, output); });
		var versionedBytes = Allocated(() => { output.Position = 0; Bois.Serialize(versioned, output); });
		Assert.True(versionedBytes <= legacyBytes, $"versioned {versionedBytes} B, today's format {legacyBytes} B");
	}

	private static long Allocated(Action action)
	{
		var before = GC.GetAllocatedBytesForCurrentThread();
		action();
		return GC.GetAllocatedBytesForCurrentThread() - before;
	}

	#endregion

	#region Failures

	[Theory]
	[InlineData(0x02)]
	[InlineData(0x41)]
	[InlineData(0x7F)]
	public void CorruptPrefixIsRejected(byte prefix)
	{
		var data = new byte[] { prefix, 0, 0, 0 };
		var ex = Assert.Throws<InvalidDataException>(() => Bois.Deserialize<VrOrder>(data, 0, data.Length));
		Assert.Contains("VrOrder", ex.Message);
	}

	[Fact]
	public void MemberCountAboveTheSlotsIsRejected()
	{
		var data = new byte[] { 0x86, 0, 0, 0, 0, 0, 0 };
		var ex = Assert.Throws<InvalidDataException>(() => Bois.Deserialize<VrOrder>(data, 0, data.Length));
		Assert.Contains("VrOrder", ex.Message);
	}

	[Fact]
	public void NullStructIsRejected()
	{
		var ex = Assert.Throws<InvalidDataException>(() => Bois.Deserialize<VrPoint>([0x40], 0, 1));
		Assert.Contains("VrPoint", ex.Message);
	}

	[Fact]
	public void ReadingPastTheEndIsRejected()
	{
		var data = Write(new VrTwoInts { A = 100_000, B = 1 });
		Assert.True(data.Length > 3);

		// claim the object is 1 byte, but A takes more than that
		data[0] = 0xC1;
		var ex = Assert.Throws<InvalidDataException>(() => Bois.Deserialize<VrTwoInts>(data, 0, data.Length));
		Assert.Contains("VrTwoInts", ex.Message);
		Assert.Contains("past the end", ex.Message);
	}

	[Fact]
	public void TruncatedDataIsRejected()
	{
		var data = Write(CustomerV2());
		for (var length = 1; length < data.Length; length++)
		{
			var truncated = data.Take(length).ToArray();
			Assert.ThrowsAny<Exception>(() => Bois.Deserialize<VrCustomerV2>(truncated, 0, truncated.Length));
			Assert.ThrowsAny<Exception>(() => Bois.Deserialize<VrCustomerV2>(new NonSeekableStream(new MemoryStream(truncated))));
		}
	}

	[Fact]
	public void SizeAboveTheDataLeftIsRejectedBeforeSkipping()
	{
		// 0x01 then the varint of 100,000, then a few bytes
		var data = new byte[] { 0x01, 0xA0, 0x8D, 0x06, 0x0E, 0x40 };
		var ex = Assert.Throws<InvalidDataException>(() => Bois.Deserialize<VrOrder>(data, 0, data.Length));
		Assert.Contains("data left", ex.Message);

		var seekable = new BufferedStream(new MemoryStream(data));
		Assert.Throws<InvalidDataException>(() => Bois.Deserialize<VrOrder>(seekable));
	}

	[Fact]
	public void BoisSkipContractCantSkipTodaysFormat()
	{
		// today's format: Id, Name, then an object in slot 4 with no size
		var legacy = Write(new VrAllTypesLegacyPrefix { A = 1, B = "b", C = 3, D = new VrLegacyItem { Value = 4 } });
		var ex = Assert.Throws<InvalidDataException>(() => Bois.Deserialize<VrOrder>(legacy, 0, legacy.Length));
		Assert.Contains("BoisSkip.Contract", ex.Message);
	}

	[Fact]
	public void WriterThatCantFillInTheSizeIsRejected()
	{
		var writer = new Salar.BinaryBuffers.Compatibility.StreamBufferWriter(new MemoryStream());
		var ex = Assert.Throws<NotSupportedException>(() => Bois.Serialize(new VrItemV1 { Value = 1 }, ref writer));
		Assert.Contains("StreamBufferWriter", ex.Message);
	}

	#endregion
}

public class VrAllTypesLegacyPrefix
{
	public int A { get; set; }
	public string B { get; set; }
	public decimal? C { get; set; }
	public VrLegacyItem D { get; set; }
}

/// <summary>
/// <see cref="BoisSerializer.MaxVersionedObjectLength"/> is static, so this doesn't run in parallel with other tests.
/// </summary>
[Collection("UntrustedLimitSettings")]
public class Test_VersionedRuntimeLimit
{
	private sealed class NonSeekableStream(Stream inner) : Stream
	{
		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		public override void Flush() { }
		public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}

	[Fact]
	public void SizeAboveTheLimitIsRejectedBeforeSkipping()
	{
		var old = BoisSerializer.MaxVersionedObjectLength;
		try
		{
			BoisSerializer.MaxVersionedObjectLength = 1000;
			var bois = new BoisSerializer();

			// 0x01 then the varint of 100,000: the data left is unknown on a non-seekable stream
			var data = new byte[] { 0x01, 0xA0, 0x8D, 0x06, 0x0E };
			var ex = Assert.Throws<InvalidDataException>(() => bois.Deserialize<VrOrder>(new NonSeekableStream(new MemoryStream(data))));
			Assert.Contains(nameof(BoisSerializer.MaxVersionedObjectLength), ex.Message);

			ex = Assert.Throws<InvalidDataException>(() => bois.Deserialize<VrOrder>(data, 0, data.Length));
			Assert.Contains(nameof(BoisSerializer.MaxVersionedObjectLength), ex.Message);
		}
		finally
		{
			BoisSerializer.MaxVersionedObjectLength = old;
		}
	}
}
