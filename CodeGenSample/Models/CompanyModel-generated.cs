using Salar.BinaryBuffers;
using Salar.BinaryBuffers.Compatibility;
using Salar.Bois.Generator.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenSample.Models;

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
