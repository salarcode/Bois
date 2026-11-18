using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

/*
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 *
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois.Types
{
	// This file is shared by the runtime serializer (Salar.Bois) and the source generator
	// (Salar.Bois.CodeGen.Generator), so both apply exactly the same rules and messages to versioned types.
	// It only knows names and numbers; each side reads them from reflection or Roslyn symbols.

	/// <summary>
	/// A field or property of a versioned type, as found by the serializer.
	/// </summary>
	internal sealed class VersionedMember
	{
		public VersionedMember(string name, string typeName, int slot, bool excluded, bool canWrite, object? source)
		{
			Name = name;
			TypeName = typeName;
			Slot = slot;
			Excluded = excluded;
			CanWrite = canWrite;
			Source = source;
		}

		public string Name { get; }

		/// <summary>The member's type, for <see cref="VersionedContractRules.Describe"/>.</summary>
		public string TypeName { get; }

		/// <summary>The index from <c>[BoisMember(index)]</c>, or -1 when there is none.</summary>
		public int Slot { get; }

		/// <summary>Has <c>[BoisMember(false)]</c>.</summary>
		public bool Excluded { get; }

		/// <summary>A field that is not readonly, or a property with a public setter that is not init-only.</summary>
		public bool CanWrite { get; }

		/// <summary>The reflection member or Roslyn symbol, used to locate errors.</summary>
		public object? Source { get; }
	}

	/// <summary>
	/// A <c>[BoisReserved(slot, type)]</c> on a versioned type or one of its base classes.
	/// </summary>
	internal sealed class VersionedReservation
	{
		public VersionedReservation(int slot, string? typeName, bool typeIsValid, object? source)
		{
			Slot = slot;
			TypeName = typeName;
			TypeIsValid = typeIsValid;
			Source = source;
		}

		public int Slot { get; }

		/// <summary>The reserved type, for messages and <see cref="VersionedContractRules.Describe"/>. Null when it is missing.</summary>
		public string? TypeName { get; }

		public bool TypeIsValid { get; }

		/// <summary>The attribute, used to locate errors.</summary>
		public object? Source { get; }
	}

	/// <summary>
	/// One slot of a valid versioned type: either a member or a reservation.
	/// </summary>
	internal sealed class VersionedSlot
	{
		public VersionedSlot(int slot, VersionedMember? member, VersionedReservation? reservation)
		{
			Slot = slot;
			Member = member;
			Reservation = reservation;
		}

		public int Slot { get; }
		public VersionedMember? Member { get; }
		public VersionedReservation? Reservation { get; }
	}

	/// <summary>
	/// A rule a versioned type breaks.
	/// </summary>
	internal sealed class VersionedProblem
	{
		public VersionedProblem(string message, object? source)
		{
			Message = message;
			Source = source;
		}

		public string Message { get; }

		/// <summary>Where the problem is: a member, a reservation, or null for the type itself.</summary>
		public object? Source { get; }

		public override string ToString() => Message;
	}

	internal static class VersionedContractRules
	{
		public static string ReservedOnNonVersionedType(string typeName)
			=> $"Type '{typeName}' has [BoisReserved] but is not versioned. Add [BoisContract(Versioned = true)] or remove [BoisReserved].";

		public static string InvalidContractMessage(string typeName, List<VersionedProblem> problems)
		{
			var message = new StringBuilder();
			message.Append("Type '").Append(typeName).Append("' is not a valid versioned contract:");
			foreach (var problem in problems)
				message.AppendLine().Append("- ").Append(problem.Message);
			return message.ToString();
		}

		/// <summary>
		/// Checks a versioned type and builds its slots 1..N. Adds every broken rule to <paramref name="problems"/>.
		/// The slots are only valid when no problem was added.
		/// </summary>
		public static VersionedSlot[] Build(
			string typeName,
			List<VersionedMember> members,
			List<VersionedReservation> reservations,
			List<VersionedProblem> problems)
		{
			var bySlot = new SortedDictionary<int, VersionedSlot>();
			var memberCount = 0;

			foreach (var member in members)
			{
				if (member.Excluded)
					continue;

				if (member.Slot < 1)
				{
					// members that can't be written (computed properties, readonly fields) are not data
					if (!member.CanWrite)
						continue;

					problems.Add(new VersionedProblem(
						$"Member '{member.Name}' needs a slot: add [BoisMember(slot)] with a slot of 1 or higher, or [BoisMember(false)] to exclude it.",
						member.Source));
					continue;
				}

				if (!member.CanWrite)
				{
					problems.Add(new VersionedProblem(
						$"Member '{member.Name}' in slot {member.Slot} can't be written. Use a field that is not readonly, or a property with a public setter.",
						member.Source));
					continue;
				}

				memberCount++;
				if (bySlot.TryGetValue(member.Slot, out var existing))
				{
					problems.Add(new VersionedProblem(
						$"Slot {member.Slot} is used by both {SlotOwner(existing)} and member '{member.Name}'. Each slot can be used once.",
						member.Source));
					continue;
				}
				bySlot.Add(member.Slot, new VersionedSlot(member.Slot, member, null));
			}

			foreach (var reservation in reservations)
			{
				if (reservation.Slot < 1)
				{
					problems.Add(new VersionedProblem(
						$"Reserved slot {reservation.Slot} is invalid. Slots start at 1.",
						reservation.Source));
					continue;
				}

				if (!reservation.TypeIsValid)
				{
					problems.Add(new VersionedProblem(
						reservation.TypeName == null
							? $"Reserved slot {reservation.Slot} needs the removed member's type, e.g. [BoisReserved({reservation.Slot}, typeof(string))]."
							: $"Reserved slot {reservation.Slot} has type '{reservation.TypeName}', which can't be serialized. Use the removed member's exact type, or typeof(BoisSkip.Contract).",
						reservation.Source));
					continue;
				}

				if (bySlot.TryGetValue(reservation.Slot, out var existing))
				{
					problems.Add(new VersionedProblem(
						$"Slot {reservation.Slot} is used by both {SlotOwner(existing)} and a reservation. Each slot can be used once; never reuse a reserved slot.",
						reservation.Source));
					continue;
				}
				bySlot.Add(reservation.Slot, new VersionedSlot(reservation.Slot, null, reservation));
			}

			if (memberCount == 0)
				problems.Add(new VersionedProblem($"Type '{typeName}' needs at least one member with [BoisMember(slot)].", null));

			var highest = 0;
			foreach (var slot in bySlot.Keys)
				highest = slot;

			var slots = new VersionedSlot[highest];
			for (var slot = 1; slot <= highest; slot++)
			{
				if (bySlot.TryGetValue(slot, out var found))
				{
					slots[slot - 1] = found;
					continue;
				}

				problems.Add(new VersionedProblem(
					$"Slot {slot} is missing. If a member was removed, add [BoisReserved({slot}, typeof(<removed member's type>))].",
					null));
			}

			return slots;
		}

		/// <summary>
		/// The slot list as stable text, one line per slot, for snapshot tests and code review.
		/// </summary>
		public static string Describe(string typeName, VersionedSlot[] slots)
		{
			var text = new StringBuilder();
			text.Append(typeName).Append(" (versioned)");
			foreach (var slot in slots)
			{
				text.Append('\n').Append(slot.Slot).Append(": ");
				if (slot.Member != null)
					text.Append(slot.Member.Name).Append(' ').Append(slot.Member.TypeName);
				else
					text.Append("reserved ").Append(slot.Reservation!.TypeName);
			}
			return text.ToString();
		}

		private static string SlotOwner(VersionedSlot slot)
			=> slot.Member != null ? $"member '{slot.Member.Name}'" : "a reservation";
	}
}
