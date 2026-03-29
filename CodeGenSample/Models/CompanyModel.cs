using Salar.BinaryBuffers;
using Salar.Bois.Generator.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenSample.Models;

public class CompanyModel
{
    /// <summary>
    /// Unique identifier for the company.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Company name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Primary address of the company.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Date the company was founded. Default is <see cref="DateTime.MinValue"/> when unknown.
    /// </summary>
    public DateTime Founded { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Annual revenue in the company's currency.
    /// </summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// Whether the company is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Simple list of employee names. Kept as strings in this sample to avoid extra model types.
    /// </summary>
    public List<string> Employees { get; } = new();

    /// <summary>
    /// Number of employees.
    /// </summary>
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
