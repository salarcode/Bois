using Salar.Bois.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois;

/// <summary>
/// Describes how Bois lays out a type.
/// </summary>
public static class BoisContractInfo
{
	private static readonly Dictionary<Type, string> Keywords = new()
    {
		{ typeof(bool), "bool" },
		{ typeof(byte), "byte" },
		{ typeof(sbyte), "sbyte" },
		{ typeof(char), "char" },
		{ typeof(decimal), "decimal" },
		{ typeof(double), "double" },
		{ typeof(float), "float" },
		{ typeof(int), "int" },
		{ typeof(uint), "uint" },
		{ typeof(long), "long" },
		{ typeof(ulong), "ulong" },
		{ typeof(short), "short" },
		{ typeof(ushort), "ushort" },
		{ typeof(object), "object" },
		{ typeof(string), "string" },
	};

	/// <summary>
	/// Returns the layout of a class or struct as stable text, one line per member.
	/// <para>
	/// For a versioned type (<c>[BoisContract(Versioned = true)]</c>) it lists every slot: the member or the
	/// reservation, and its type. Snapshot-test this text for each versioned type, so a removed or reused slot
	/// shows up as a change in review.
	/// </para>
	/// <para>
	/// For other types it lists the members in the order they are written today. Use it to number the slots
	/// when switching a type to versioned: slot 1 for the first member, and so on.
	/// </para>
	/// </summary>
	/// <param name="type">A class or struct.</param>
	/// <exception cref="ArgumentException">The type is not a class or struct with members, e.g. a collection or a number.</exception>
	/// <exception cref="InvalidTypeException">The type is versioned and breaks a rule.</exception>
	public static string Describe(Type type)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		var typeInfo = BoisTypeCache.GetComplexTypeUnCached(type);
		if (typeInfo == null || typeInfo.ComplexKnownType != EnComplexKnownType.Unknown || typeInfo.Members == null)
			throw new ArgumentException($"Type '{TypeName(type)}' is not a class or struct with members.", nameof(type));

		if (typeInfo.VersionedSlots != null)
			return VersionedContractRules.Describe(TypeName(type), typeInfo.VersionedSlots);

		var text = new StringBuilder();
		text.Append(TypeName(type)).Append(" (not versioned)");
		for (var i = 0; i < typeInfo.Members.Length; i++)
		{
			var member = typeInfo.Members[i];
			var memberType = member is PropertyInfo property ? property.PropertyType : ((FieldInfo)member).FieldType;
			text.Append('\n').Append(i + 1).Append(": ").Append(member.Name).Append(' ').Append(TypeName(memberType));
		}
		return text.ToString();
	}

	/// <summary>
	/// A short, stable C# name: keywords for built-in types, no namespaces, e.g. <c>List&lt;int?&gt;</c>.
	/// The source generator produces the same names.
	/// </summary>
	internal static string TypeName(Type type)
	{
		if (Keywords.TryGetValue(type, out var keyword))
			return keyword;

		if (type.IsArray)
			return TypeName(type.GetElementType()) + "[" + new string(',', type.GetArrayRank() - 1) + "]";

		var nullableOf = Nullable.GetUnderlyingType(type);
		if (nullableOf != null)
			return TypeName(nullableOf) + "?";

		var name = type.Name;
		var tick = name.IndexOf('`');
		if (tick >= 0)
			name = name.Substring(0, tick);

		if (type.IsGenericType)
		{
			// arguments of the containing types come first, then this type's own
			var arguments = type.GetGenericArguments();
			var ownCount = arguments.Length - (type.IsNested ? type.DeclaringType.GetGenericArguments().Length : 0);
			name += "<" + string.Join(", ", arguments.Skip(arguments.Length - ownCount).Select(TypeName)) + ">";
		}

		if (type.IsNested && !type.IsGenericParameter)
			name = TypeName(type.DeclaringType) + "." + name;

		return name;
	}
}
