using System;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois.CodeGen;

/// <summary>
/// Can be used for classes and structs to specify that the serializer should serialize fields and properties or not.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class BoisContractAttribute : Attribute
{
    /// <summary>
    /// Specifies that fields should be serialized or not.
    /// </summary>
    public bool Fields { get; set; }

    /// <summary>
    /// Specifies that properties should be serialized or not.
    /// </summary>
    public bool Properties { get; set; }

    /// <summary>
    /// Uses the versioned layout, so the type can add, remove, rename and reorder members and still read data
    /// written by its older and newer versions. False (default) keeps the current layout.
    /// <para>
    /// Every member needs a permanent slot with <c>[BoisMember(slot)]</c>, starting at 1, or <c>[BoisMember(false)]</c>
    /// to exclude it. Members of base classes too. A removed member keeps its slot with <c>[BoisReserved(slot, typeof(OldType))]</c>.
    /// Never reuse a slot.
    /// </para>
    /// <para>
    /// Note: on a type that is not versioned, <c>[BoisMember(index)]</c> changes the member order. So adding slots
    /// to a base class also changes the data of other, non-versioned types that derive from it.
    /// </para>
    /// </summary>
    public bool Versioned { get; set; }

    /// <summary>
    /// Can be used for classes and structs to specify that the serializer should serialize fields or properties.
    /// </summary>
    /// <param name="fields">Specifies that fields should be serialized or not.</param>
    /// <param name="properties">Specifies that properties should be serialized or not.</param>
    public BoisContractAttribute(bool fields, bool properties)
    {
        Fields = fields;
        Properties = properties;
    }

    /// <summary>
    /// Can be used for classes and structs to specify that the serializer should serialize fields or properties.
    /// </summary>
    public BoisContractAttribute()
        : this(true, true)
    { }
}
