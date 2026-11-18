using System;

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois
{
	/// <summary>
	/// Specifies a field or property settings for serialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class BoisMemberAttribute : Attribute
	{
		/// <summary>
		/// In which order should this member be serialized.
		/// <para>
		/// On a type that is not versioned, this is an ordering hint: the member is inserted at this position.
		/// On a type with <c>[BoisContract(Versioned = true)]</c>, this is the member's permanent slot, starting at 1.
		/// </para>
		/// </summary>
		public int Index { get; private set; }

		/// <summary>
		/// Specifies that should this member be included in serialization.
		/// </summary>
		public bool Included { get; private set; }

		/// <summary>
		/// Specifies a field or property settings for serialization.
		/// </summary>
		/// <param name="index">In which order should this member be serialized.</param>
		/// <param name="included">Specifies that should this member be included in serialization.</param>
		public BoisMemberAttribute(int index, bool included)
		{
			Index = index;
			Included = included;
		}

		/// <summary>
		/// Specifies a field or property settings for serialization.
		/// </summary>
		public BoisMemberAttribute()
			: this(-1, true)
		{ }

		/// <summary>
		/// Specifies a field or property settings for serialization.
		/// </summary>
		/// <param name="index">In which order should this member be serialized.</param>
		public BoisMemberAttribute(int index)
			: this(index, true)
		{ }

		/// <summary>
		/// Specifies a field or property settings for serialization.
		/// </summary>
		/// <param name="included">Specifies that should this member be included in serialization.</param>
		public BoisMemberAttribute(bool included)
			: this(-1, included)
		{ }
	}
}