using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Salar.Bois.NetFx.Tests.Tests;

/// <summary>
/// Non-nullable custom struct members must be written and read like any other member.
/// </summary>
public class Test_StructMembers
{
	public struct Point
	{
		public int X { get; set; }
		public int Y { get; set; }
	}

	public struct Line
	{
		public Point Start { get; set; }
		public Point End;
	}

	public class Shape
	{
		public int Id { get; set; }
		public Point Origin { get; set; }
		public Point? MaybeOrigin { get; set; }
		public Point? NoOrigin { get; set; }
		public Point FieldOrigin;
		public Line Edge { get; set; }
		public Point[] Corners { get; set; }
		public List<Point> Path { get; set; }
		public string Name { get; set; }
	}

	private static Shape CreateShape() => new()
	{
		Id = 7,
		Origin = new Point { X = 3, Y = 4 },
		MaybeOrigin = new Point { X = 5, Y = 6 },
		NoOrigin = null,
		FieldOrigin = new Point { X = 8, Y = 9 },
		Edge = new Line { Start = new Point { X = 10, Y = 11 }, End = new Point { X = 12, Y = 13 } },
		Corners = [new Point { X = 14, Y = 15 }, new Point { X = 16, Y = 17 }],
		Path = [new Point { X = 18, Y = 19 }],
		Name = "after the structs",
	};

	private static void AssertShape(Shape expected, Shape actual)
	{
		Assert.Equal(expected.Id, actual.Id);
		Assert.Equal(expected.Origin, actual.Origin);
		Assert.Equal(expected.MaybeOrigin, actual.MaybeOrigin);
		Assert.Equal(expected.NoOrigin, actual.NoOrigin);
		Assert.Equal(expected.FieldOrigin, actual.FieldOrigin);
		Assert.Equal(expected.Edge.Start, actual.Edge.Start);
		Assert.Equal(expected.Edge.End, actual.Edge.End);
		Assert.Equal(expected.Corners, actual.Corners);
		Assert.Equal(expected.Path, actual.Path);
		Assert.Equal(expected.Name, actual.Name);
	}

	[Fact]
	public void ClassWithStructMembersRoundTrips()
	{
		var serializer = new BoisSerializer();
		var init = CreateShape();

		using var ms = new MemoryStream();
		serializer.Serialize(init, ms);
		var bytes = ms.ToArray();

		AssertShape(init, serializer.Deserialize<Shape>(bytes, 0, bytes.Length));
		AssertShape(init, serializer.Deserialize<Shape>(new MemoryStream(bytes)));
	}

	[Fact]
	public void StructWithStructMembersRoundTrips()
	{
		var serializer = new BoisSerializer();
		var init = new Line { Start = new Point { X = 1, Y = 2 }, End = new Point { X = 3, Y = 4 } };

		using var ms = new MemoryStream();
		serializer.Serialize(init, ms);
		var bytes = ms.ToArray();

		var final = serializer.Deserialize<Line>(bytes, 0, bytes.Length);
		Assert.Equal(init.Start, final.Start);
		Assert.Equal(init.End, final.End);
	}
}
