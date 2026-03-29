using Salar.BinaryBuffers;
using Salar.BinaryBuffers.Compatibility;
using Salar.Bois.Generator.Attributes;
using Salar.Bois.Generator.Serializers;
using System;
using System.Collections.Generic;

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

    [BoisWriter]
    public static partial void WriteCompanyModel(Stream output, CompanyModel? model);
}

public static partial class CompanyModelBois
{
    public static partial CompanyModel? ReadCompanyModel(Stream source)
    {
        BufferReaderBase reader = new StreamBufferReader(source);

        // Member count, if null then the whole object is null, otherwise the member count is not used in source gen models
        var memCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);
        if (memCount is null)
            return null;

        var model = new CompanyModel();

        model.Id = BoisPrimitiveReaders.ReadGuid(reader); // Id
        model.Name = BoisPrimitiveReaders.ReadString(reader, System.Text.Encoding.UTF8);
        model.Address = BoisPrimitiveReaders.ReadString(reader, System.Text.Encoding.UTF8);
        model.Phone = BoisPrimitiveReaders.ReadString(reader, System.Text.Encoding.UTF8);
        model.Founded = BoisPrimitiveReaders.ReadDateTime(reader);
        model.Revenue = BoisNumericSerializers.ReadVarDecimal(reader);
        model.IsActive = BoisPrimitiveReaders.ReadBoolean(reader);

        // Read the list of employees
        var employeesCount = BoisNumericSerializers.ReadVarUInt32Nullable(reader);
        if (employeesCount is not null)
        {
            var employees = model.Employees;
            employees.Clear();
            for (int i = 0; i < employeesCount; i++)
            {
                var employee = BoisPrimitiveReaders.ReadString(reader, System.Text.Encoding.UTF8);
                employees.Add(employee);
            }
        }

        _ = BoisNumericSerializers.ReadVarInt32(reader); // EmployeesCount, not used in source gen models since we have the list

        return model;
    }

    public static partial void WriteCompanyModel(Stream output, CompanyModel? model)
    {
        var writer = new StreamBufferWriter(output);
        if (model is null)
        {
            BoisPrimitiveWriters.WriteNullValue(writer);
            return;
        }

        // Write `CompanyModel` member count (properties + fields)
        BoisNumericSerializers.WriteUIntNullableMemberCount(writer, 9u);

        // Write the members in the same order as they are defined in the class

        BoisPrimitiveWriters.WriteValue(writer, model.Id);
        BoisPrimitiveWriters.WriteValue(writer, model.Name, System.Text.Encoding.UTF8);
        BoisPrimitiveWriters.WriteValue(writer, model.Address, System.Text.Encoding.UTF8);
        BoisPrimitiveWriters.WriteValue(writer, model.Phone, System.Text.Encoding.UTF8);
        BoisPrimitiveWriters.WriteValue(writer, model.Founded);
        BoisNumericSerializers.WriteVarDecimal(writer, model.Revenue);
        BoisPrimitiveWriters.WriteValue(writer, model.IsActive);

        // Write the list of employees
        if (model.Employees is null)
        {
            BoisPrimitiveWriters.WriteNullValue(writer);
        }
        else
        {
            BoisNumericSerializers.WriteUIntNullableMemberCount(writer, (uint)model.Employees.Count);
            foreach (var member in model.Employees)
            {
                BoisPrimitiveWriters.WriteValue(writer, member, System.Text.Encoding.UTF8);
            }
        }
        BoisNumericSerializers.WriteVarInt(writer, model.EmployeesCount);
    }
}