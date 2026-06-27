using Salar.BinaryBuffers;
using Salar.Bois;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Salar.Bois.NetFx.Tests.CodeGenFixtures;

public class CompanyModel
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Name { get; set; } = string.Empty;
	public string? Address { get; set; }
	public string? Phone { get; set; }
	public DateTime Founded { get; set; } = DateTime.MinValue;
	public decimal Revenue { get; set; }
	public bool IsActive { get; set; } = true;
	public List<string> Employees { get; } = new();
	public int EmployeesCount => Employees.Count;

	public void AddEmployee(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Employee name must not be empty.", nameof(name));

		Employees.Add(name);
	}

	public bool RemoveEmployee(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return false;

		return Employees.Remove(name);
	}

	public override string ToString()
	{
		return $"{Name} (Id={Id}, Employees={EmployeesCount})";
	}
}

public static partial class CompanyModelBois
{
	[BoisReader]
	public static partial CompanyModel? ReadCompanyModel(Stream source);

	[BoisReader]
	public static partial CompanyModel? ReadCompanyModel(BufferReaderBase reader);

	[BoisReader]
	public static partial CompanyModel? ReadCompanyModel(BufferReaderBase reader, Encoding encoding);

	[BoisReader]
	public static partial CompanyModel? ReadCompanyModel(byte[] buffer, int position, int length);

	[BoisWriter]
	public static partial void WriteCompanyModel(CompanyModel? model, Stream output);

	[BoisWriter]
	public static partial void WriteCompanyModel(CompanyModel? model, BufferWriterBase writer);

	[BoisWriter]
	public static partial void WriteCompanyModel(CompanyModel? model, BufferWriterBase writer, Encoding encoding);

	[BoisWriter]
	public static partial void WriteCompanyModel(CompanyModel? model, byte[] output, int position, int length);
}
