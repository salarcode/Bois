using Salar.Bois.BenchmarksObjects;
using System;
using System.Collections.Generic;

namespace Salar.Bois.BenchBois;

/// <summary>
/// The same graph and data as <see cref="Test2_Objects"/>, with every class versioned, so the two can be compared.
/// The struct stays a normal struct, the way most structs would.
/// </summary>
[BoisContract(Versioned = true)]
public class Test2_ObjectsVersioned : IBenchmarkTestObject
{
	[BoisMember(1)] public int Id { get; set; }
	[BoisMember(2)] public string Number { get; set; }
	[BoisMember(3)] public DateTime Created { get; set; }
	[BoisMember(4)] public Test2_Status Status { get; set; }
	[BoisMember(5)] public decimal Total { get; set; }
	[BoisMember(6)] public decimal? Discount { get; set; }
	[BoisMember(7)] public Test2_Point Location { get; set; }
	[BoisMember(8)] public Test2V_Customer Customer { get; set; }
	[BoisMember(9)] public Test2V_Customer? Referrer { get; set; }
	[BoisMember(10)] public List<Test2V_Line> Lines { get; set; }

	public Test2_ObjectsVersioned()
	{
		Id = 123456;
		Number = "ORD-2026-000123";
		Created = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
		Status = Test2_Status.Shipped;
		Total = 1234.56m;
		Location = new Test2_Point { X = 10, Y = 20 };
		Customer = new Test2V_Customer
		{
			Id = 42,
			Name = "Salar",
			Email = "customer@example.com",
			Address = new Test2V_Address { Street = "1 Main Street", City = "Sydney", PostCode = "2000" },
		};
		Lines = new List<Test2V_Line>();
		for (var i = 0; i < 10; i++)
		{
			Lines.Add(new Test2V_Line
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

[BoisContract(Versioned = true)]
public class Test2V_Customer
{
	[BoisMember(1)] public int Id { get; set; }
	[BoisMember(2)] public string Name { get; set; }
	[BoisMember(3)] public string Email { get; set; }
	[BoisMember(4)] public Test2V_Address Address { get; set; }
}

[BoisContract(Versioned = true)]
public class Test2V_Address
{
	[BoisMember(1)] public string Street { get; set; }
	[BoisMember(2)] public string City { get; set; }
	[BoisMember(3)] public string PostCode { get; set; }
}

[BoisContract(Versioned = true)]
public class Test2V_Line
{
	[BoisMember(1)] public int ProductId { get; set; }
	[BoisMember(2)] public string Name { get; set; }
	[BoisMember(3)] public int Quantity { get; set; }
	[BoisMember(4)] public decimal Price { get; set; }
	[BoisMember(5)] public string? Note { get; set; }
}
