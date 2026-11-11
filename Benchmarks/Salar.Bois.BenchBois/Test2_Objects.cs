using Salar.Bois.BenchmarksObjects;
using System;
using System.Collections.Generic;
using System.IO;

namespace Salar.Bois.BenchBois;

/// <summary>
/// An object graph with many nested objects, so the object prefix path is measured.
/// Used to compare normal types with versioned ones.
/// </summary>
public class Test2_Objects : IBenchmarkTestObject
{
	public int Id { get; set; }
	public string Number { get; set; }
	public DateTime Created { get; set; }
	public Test2_Status Status { get; set; }
	public decimal Total { get; set; }
	public decimal? Discount { get; set; }
	public Test2_Point Location { get; set; }
	public Test2_Customer Customer { get; set; }
	public Test2_Customer? Referrer { get; set; }
	public List<Test2_Line> Lines { get; set; }

	public Test2_Objects()
	{
		Id = 123456;
		Number = "ORD-2026-000123";
		Created = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
		Status = Test2_Status.Shipped;
		Total = 1234.56m;
		Location = new Test2_Point { X = 10, Y = 20 };
		Customer = new Test2_Customer
		{
			Id = 42,
			Name = "Salar",
			Email = "customer@example.com",
			Address = new Test2_Address { Street = "1 Main Street", City = "Sydney", PostCode = "2000" },
		};
		Lines = new List<Test2_Line>();
		for (var i = 0; i < 10; i++)
		{
			Lines.Add(new Test2_Line
			{
				ProductId = 1000 + i,
				Name = "Product " + i,
				Quantity = i + 1,
				Price = 9.99m + i,
				Note = i % 3 == 0 ? null : "note",
			});
		}
	}
}

public enum Test2_Status
{
	New,
	Paid,
	Shipped,
}

public struct Test2_Point
{
	public int X { get; set; }
	public int Y { get; set; }
}

public class Test2_Customer
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Email { get; set; }
	public Test2_Address Address { get; set; }
}

public class Test2_Address
{
	public string Street { get; set; }
	public string City { get; set; }
	public string PostCode { get; set; }
}

public class Test2_Line
{
	public int ProductId { get; set; }
	public string Name { get; set; }
	public int Quantity { get; set; }
	public decimal Price { get; set; }
	public string? Note { get; set; }
}

public static partial class CodeGenBois_Objects
{
	[CodeGen.BoisReader]
	public static partial Test2_Objects? Read(Stream source);

	[CodeGen.BoisWriter]
	public static partial void Write(Test2_Objects? model, Stream output);
}
