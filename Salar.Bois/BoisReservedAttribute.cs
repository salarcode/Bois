using System;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois;

/// <summary>
/// Keeps the slot of a member that was removed from a versioned type
/// (<c>[BoisContract(Versioned = true)]</c>), so data written with the old member can still be read.
/// <para>
/// <paramref name="type"/> is how the removed value was written: use the removed member's exact type, e.g.
/// <c>typeof(string)</c> or <c>typeof(Guid?)</c>. Use <c>typeof(BoisSkip.Contract)</c> when the removed member was a
/// versioned object, so its class can be deleted.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public sealed class BoisReservedAttribute : Attribute
{
	/// <summary>
	/// Keeps the slot of a removed member.
	/// </summary>
	/// <param name="index">The removed member's slot.</param>
	/// <param name="type">How the removed value was written: the removed member's exact type, or <c>typeof(BoisSkip.Contract)</c>.</param>
	public BoisReservedAttribute(int index, Type type)
	{
		Index = index;
		Type = type;
	}

	/// <summary>
	/// The removed member's slot.
	/// </summary>
	public int Index { get; }

	/// <summary>
	/// How the removed value was written.
	/// </summary>
	public Type Type { get; }
}
