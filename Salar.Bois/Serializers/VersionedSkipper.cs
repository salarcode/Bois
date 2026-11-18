using Salar.BinaryBuffers;
using Salar.Bois.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

/*
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 *
 * https://github.com/salarcode/Bois
 * Mozilla Public License v2
 */
namespace Salar.Bois.Serializers
{
	// Used only by the runtime serializer, to skip the reserved slots of versioned types
	// ([BoisReserved(slot, type)]). Normal types never call into this file.

	internal enum SkipKind : byte
	{
		/// <summary>A normal class or struct without members. Bois writes nothing for it.</summary>
		Empty,

		/// <summary>A basic value, read and dropped. Never allocates.</summary>
		Basic,

		/// <summary>A nullable length then that many bytes: string, byte[], Uri, Version.</summary>
		Bytes,

		/// <summary>decimal, double, float: a byte holding the value, or a byte with the length of the bytes after it.</summary>
		VarBinary,

		/// <summary>A length (0 or 16) then the bytes.</summary>
		Guid,

		/// <summary>A nullable count then the items: lists, sets, arrays.</summary>
		List,

		/// <summary>A nullable count then key and value pairs.</summary>
		Dictionary,

		/// <summary>A class or nullable struct member: <c>0x40</c> for null, otherwise a byte then the value.</summary>
		HasValue,

		/// <summary>A normal class (a member count prefix, <c>0x40</c> for null) or struct (no prefix), then its members.</summary>
		Object,

		/// <summary>A versioned type, or <see cref="BoisSkip.Contract"/> when there are no slots.</summary>
		Versioned,

		/// <summary>Read and dropped. They are too complex to skip by size.</summary>
		DataTable,
		DataSet,
	}

	/// <summary>
	/// How to skip one value of a type. Built once per type when the reader is generated.
	/// </summary>
	internal sealed class SkipNode
	{
		internal SkipKind Kind;
		internal EnBasicKnownType BasicType;
		internal bool Nullable;
		internal TypeCode EnumCode;

		/// <summary>When above zero, every value takes exactly this many bytes, so a list of them is jumped over at once.</summary>
		internal int FixedSize;

		/// <summary>List item, or dictionary key.</summary>
		internal SkipNode Item;

		/// <summary>Dictionary value, or the value after a has-value byte.</summary>
		internal SkipNode Value;

		/// <summary>Object members, or versioned slots in slot order. Null for <see cref="BoisSkip.Contract"/>.</summary>
		internal SkipNode[] Members;

		/// <summary>Object: writes a member count prefix (classes). Versioned: may be null (classes).</summary>
		internal bool IsClass;

		internal string TypeName;
	}

	/// <summary>
	/// Skips the value in a reserved slot by its type, without creating objects or strings.
	/// </summary>
	internal static class VersionedSkipper
	{
		private static readonly object _lock = new object();
		private static readonly Dictionary<(Type, bool), SkipNode> _builtNodes = new Dictionary<(Type, bool), SkipNode>();
		private static readonly Dictionary<Type, int> _registeredIds = new Dictionary<Type, int>();
		private static SkipNode[] _registered = new SkipNode[8];
		private static int _registeredCount;

		/// <summary>
		/// Returns the id the generated reader passes to <see cref="Skip"/> for a reserved slot of <paramref name="reservedType"/>.
		/// </summary>
		internal static int Register(Type reservedType)
		{
			lock (_lock)
			{
				if (_registeredIds.TryGetValue(reservedType, out var id))
					return id;

				var node = Build(reservedType, member: true);

				id = _registeredCount;
				var registered = _registered;
				if (id == registered.Length)
				{
					var grown = new SkipNode[registered.Length * 2];
					Array.Copy(registered, grown, registered.Length);
					registered = grown;
				}
				registered[id] = node;
				Volatile.Write(ref _registered, registered);
				_registeredCount = id + 1;
				_registeredIds.Add(reservedType, id);
				return id;
			}
		}

		/// <summary>
		/// Skips one reserved slot. Called by the generated readers of versioned types.
		/// </summary>
		internal static void Skip<TReader>(ref TReader reader, int nodeId, Encoding encoding)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
		{
			SkipValue(ref reader, Volatile.Read(ref _registered)[nodeId], encoding);
		}

		private static void SkipValue<TReader>(ref TReader reader, SkipNode node, Encoding encoding)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
		{
			switch (node.Kind)
			{
				case SkipKind.Empty:
					return;

				case SkipKind.Basic:
					SkipBasic(ref reader, node);
					return;

				case SkipKind.Bytes:
				{
					var length = NumericSerializers.ReadVarUInt32Nullable(ref reader);
					if (length == null || length.Value == 0)
						return;
					VersionedReader.Skip(ref reader, PrimitiveReader.CheckedByteLength(ref reader, length.Value));
					return;
				}

				case SkipKind.VarBinary:
				{
					var first = reader.ReadByte();
					if (node.Nullable && first == NumericSerializers.FlagIsNull)
						return;
					// the high bit means the value is in this byte, otherwise this byte is the length
					if ((first & 0x80) != 0)
						return;
					VersionedReader.Skip(ref reader, first);
					return;
				}

				case SkipKind.Guid:
				{
					uint length;
					if (node.Nullable)
					{
						var nullableLength = NumericSerializers.ReadVarUInt32Nullable(ref reader);
						if (nullableLength == null)
							return;
						length = nullableLength.Value;
					}
					else
					{
						length = NumericSerializers.ReadVarUInt32(ref reader);
					}

					if (length != 0 && length != 16)
						throw new InvalidDataException("Invalid Guid length.");
					VersionedReader.Skip(ref reader, (int)length);
					return;
				}

				case SkipKind.List:
				{
					var count = NumericSerializers.ReadVarUInt32Nullable(ref reader);
					if (count == null)
						return;
					var itemCount = PrimitiveReader.CheckedCount(ref reader, count.Value);
					var item = node.Item;
					if (item.FixedSize > 0)
					{
						VersionedReader.Skip(ref reader, FixedLength(itemCount, item.FixedSize));
						return;
					}
					for (var i = 0; i < itemCount; i++)
						SkipValue(ref reader, item, encoding);
					return;
				}

				case SkipKind.Dictionary:
				{
					var count = NumericSerializers.ReadVarUInt32Nullable(ref reader);
					if (count == null)
						return;
					var itemCount = PrimitiveReader.CheckedCount(ref reader, count.Value);
					for (var i = 0; i < itemCount; i++)
					{
						SkipValue(ref reader, node.Item, encoding);
						SkipValue(ref reader, node.Value, encoding);
					}
					return;
				}

				case SkipKind.HasValue:
					if (reader.ReadByte() == NumericSerializers.FlagIsNull)
						return;
					SkipValue(ref reader, node.Value, encoding);
					return;

				case SkipKind.Object:
				{
					if (node.IsClass && NumericSerializers.ReadVarUInt32Nullable(ref reader) == null)
						return;
					var members = node.Members;
					for (var i = 0; i < members.Length; i++)
						SkipValue(ref reader, members[i], encoding);
					return;
				}

				case SkipKind.Versioned:
					SkipVersioned(ref reader, node, encoding);
					return;

				case SkipKind.DataTable:
					PrimitiveReader.ReadDataTable(ref reader, encoding);
					return;

				case SkipKind.DataSet:
					PrimitiveReader.ReadDataSet(ref reader, encoding);
					return;

				default:
					throw new InvalidOperationException($"Unknown skip kind {node.Kind}.");
			}
		}

		private static void SkipVersioned<TReader>(ref TReader reader, SkipNode node, Encoding encoding)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
		{
			var slots = node.Members;
			var format = VersionedReader.ReadPrefix(ref reader, slots?.Length ?? int.MaxValue, node.IsClass, node.TypeName, out var value);
			switch (format)
			{
				case VersionedReader.PrefixNull:
					return;

				case VersionedReader.PrefixSize:
					VersionedReader.Skip(ref reader, value);
					return;

				default:
					if (slots == null)
						throw new InvalidDataException(
							$"Invalid data for '{node.TypeName}': BoisSkip.Contract can only skip versioned objects, but this one was written in the non-versioned format. Reserve the slot with the removed class's type instead.");
					for (var i = 0; i < value; i++)
						SkipValue(ref reader, slots[i], encoding);
					return;
			}
		}

		private static void SkipBasic<TReader>(ref TReader reader, SkipNode node)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
		{
			var nullable = node.Nullable;
			switch (node.BasicType)
			{
				case EnBasicKnownType.Bool:
				case EnBasicKnownType.Byte:
					if (nullable) NumericSerializers.ReadVarByteNullable(ref reader);
					else reader.ReadByte();
					return;

				case EnBasicKnownType.SByte:
					if (nullable) NumericSerializers.ReadVarSByteNullable(ref reader);
					else reader.ReadByte();
					return;

				case EnBasicKnownType.DbNull:
					reader.ReadByte();
					return;

				case EnBasicKnownType.Char:
					if (nullable) NumericSerializers.ReadVarUInt16Nullable(ref reader);
					else reader.ReadUInt16();
					return;

				case EnBasicKnownType.Int16:
					if (nullable) NumericSerializers.ReadVarInt16Nullable(ref reader);
					else NumericSerializers.ReadVarInt16(ref reader);
					return;

				case EnBasicKnownType.Int32:
				case EnBasicKnownType.Color:
					if (nullable) NumericSerializers.ReadVarInt32Nullable(ref reader);
					else NumericSerializers.ReadVarInt32(ref reader);
					return;

				case EnBasicKnownType.Int64:
				case EnBasicKnownType.TimeSpan:
					if (nullable) NumericSerializers.ReadVarInt64Nullable(ref reader);
					else NumericSerializers.ReadVarInt64(ref reader);
					return;

				case EnBasicKnownType.UInt16:
					if (nullable) NumericSerializers.ReadVarUInt16Nullable(ref reader);
					else NumericSerializers.ReadVarUInt16(ref reader);
					return;

				case EnBasicKnownType.UInt32:
					if (nullable) NumericSerializers.ReadVarUInt32Nullable(ref reader);
					else NumericSerializers.ReadVarUInt32(ref reader);
					return;

				case EnBasicKnownType.UInt64:
					if (nullable) NumericSerializers.ReadVarUInt64Nullable(ref reader);
					else NumericSerializers.ReadVarUInt64(ref reader);
					return;

				case EnBasicKnownType.DateTime:
					if (nullable) PrimitiveReader.ReadDateTimeNullable(ref reader);
					else PrimitiveReader.ReadDateTime(ref reader);
					return;

				case EnBasicKnownType.DateTimeOffset:
					if (nullable) PrimitiveReader.ReadDateTimeOffsetNullable(ref reader);
					else PrimitiveReader.ReadDateTimeOffset(ref reader);
					return;

#if NET6_0_OR_GREATER
				case EnBasicKnownType.DateOnly:
					if (nullable) PrimitiveReader.ReadDateOnlyNullable(ref reader);
					else PrimitiveReader.ReadDateOnly(ref reader);
					return;

				case EnBasicKnownType.TimeOnly:
					if (nullable) PrimitiveReader.ReadTimeOnlyNullable(ref reader);
					else PrimitiveReader.ReadTimeOnly(ref reader);
					return;
#endif

				case EnBasicKnownType.Enum:
					SkipEnum(ref reader, node.EnumCode, nullable);
					return;

				default:
					throw new InvalidOperationException($"Can't skip a value of type {node.BasicType}.");
			}
		}

		private static void SkipEnum<TReader>(ref TReader reader, TypeCode code, bool nullable)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
		{
			// the same layout PrimitiveWriter.WriteEnumGeneric writes
			switch (code)
			{
				case TypeCode.Byte:
					if (nullable) NumericSerializers.ReadVarByteNullable(ref reader);
					else reader.ReadByte();
					return;
				case TypeCode.SByte:
					if (nullable) NumericSerializers.ReadVarSByteNullable(ref reader);
					else reader.ReadByte();
					return;
				case TypeCode.Int16:
					if (nullable) NumericSerializers.ReadVarInt16Nullable(ref reader);
					else NumericSerializers.ReadVarInt16(ref reader);
					return;
				case TypeCode.UInt16:
					if (nullable) NumericSerializers.ReadVarUInt16Nullable(ref reader);
					else NumericSerializers.ReadVarUInt16(ref reader);
					return;
				case TypeCode.Int32:
					if (nullable) NumericSerializers.ReadVarInt32Nullable(ref reader);
					else NumericSerializers.ReadVarInt32(ref reader);
					return;
				case TypeCode.UInt32:
					if (nullable) NumericSerializers.ReadVarUInt32Nullable(ref reader);
					else NumericSerializers.ReadVarUInt32(ref reader);
					return;
				case TypeCode.Int64:
					if (nullable) NumericSerializers.ReadVarInt64Nullable(ref reader);
					else NumericSerializers.ReadVarInt64(ref reader);
					return;
				case TypeCode.UInt64:
					if (nullable) NumericSerializers.ReadVarUInt64Nullable(ref reader);
					else NumericSerializers.ReadVarUInt64(ref reader);
					return;
				default:
					throw new InvalidOperationException($"Can't skip an enum based on {code}.");
			}
		}

		private static int FixedLength(int count, int size)
		{
			var length = (long)count * size;
			if (length > int.MaxValue)
				throw new InvalidDataException($"The declared item count {count} is too large.");
			return (int)length;
		}

		#region Building

		/// <param name="type">The type of the value to skip.</param>
		/// <param name="member">The value is a member of an object, where class values get a has-value byte first.</param>
		private static SkipNode Build(Type type, bool member)
		{
			if (_builtNodes.TryGetValue((type, member), out var built))
				return built;

			var node = new SkipNode { TypeName = BoisContractInfo.TypeName(type) };
			// added before the children are built, so types that contain themselves work
			_builtNodes.Add((type, member), node);

			if (type == typeof(BoisSkip.Contract))
			{
				if (member)
				{
					node.Kind = SkipKind.HasValue;
					node.Value = Build(type, member: false);
				}
				else
				{
					node.Kind = SkipKind.Versioned;
					node.IsClass = true;
				}
				return node;
			}

			var basic = BoisTypeCache.GetBasicType(type);
			if (basic.KnownType != EnBasicKnownType.Unknown)
			{
				BuildBasic(node, type, basic);
				return node;
			}

			var complex = BoisTypeCache.GetComplexTypeUnCached(type);
			switch (complex.ComplexKnownType)
			{
				case EnComplexKnownType.Collection:
				case EnComplexKnownType.ISet:
					node.Kind = SkipKind.List;
					node.Item = Build(ItemType(ReflectionHelper.FindUnderlyingGenericElementType(type), type), member: false);
					return node;

				case EnComplexKnownType.UnknownArray:
					node.Kind = SkipKind.List;
					node.Item = Build(ItemType(type.GetElementType(), type), member: false);
					return node;

				case EnComplexKnownType.Dictionary:
				{
					var keyValue = ReflectionHelper.FindUnderlyingGenericDictionaryElementType(type)
								   ?? ReflectionHelper.FindUnderlyingGenericElementType(type)?.GetGenericArguments();
					if (keyValue == null || keyValue.Length != 2)
						throw new InvalidTypeException($"Can't skip '{node.TypeName}': only generic dictionaries are supported.");
					node.Kind = SkipKind.Dictionary;
					node.Item = Build(keyValue[0], member: false);
					node.Value = Build(keyValue[1], member: false);
					return node;
				}

				case EnComplexKnownType.NameValueColl:
					node.Kind = SkipKind.Dictionary;
					node.Item = Build(typeof(string), member: false);
					node.Value = node.Item;
					return node;
			}

			var nullableOf = System.Nullable.GetUnderlyingType(type);
			if (nullableOf != null)
			{
				node.Kind = SkipKind.HasValue;
				node.Value = Build(nullableOf, member: false);
				return node;
			}

			if (member && !type.IsValueType)
			{
				// the parent writes a has-value byte before a class member
				node.Kind = SkipKind.HasValue;
				node.Value = Build(type, member: false);
				return node;
			}

			node.IsClass = !type.IsValueType;
			if (complex.VersionedSlots != null)
			{
				node.Kind = SkipKind.Versioned;
				var slots = complex.VersionedSlots;
				node.Members = new SkipNode[slots.Length];
				for (var i = 0; i < slots.Length; i++)
				{
					var slot = slots[i];
					var slotType = slot.Member != null
						? MemberType((System.Reflection.MemberInfo)slot.Member.Source)
						: ((BoisReservedAttribute)slot.Reservation.Source).Type;
					node.Members[i] = Build(slotType, member: true);
				}
				return node;
			}

			if (complex.Members == null || complex.Members.Length == 0)
			{
				node.Kind = SkipKind.Empty;
				return node;
			}

			node.Kind = SkipKind.Object;
			node.Members = new SkipNode[complex.Members.Length];
			for (var i = 0; i < complex.Members.Length; i++)
				node.Members[i] = Build(MemberType(complex.Members[i]), member: true);
			return node;
		}

		private static void BuildBasic(SkipNode node, Type type, BoisBasicTypeInfo basic)
		{
			node.Nullable = basic.IsNullable;
			switch (basic.KnownType)
			{
				case EnBasicKnownType.String:
				case EnBasicKnownType.ByteArray:
				case EnBasicKnownType.Uri:
				case EnBasicKnownType.Version:
					node.Kind = SkipKind.Bytes;
					return;

				case EnBasicKnownType.Decimal:
				case EnBasicKnownType.Double:
				case EnBasicKnownType.Single:
					node.Kind = SkipKind.VarBinary;
					return;

				case EnBasicKnownType.Guid:
					node.Kind = SkipKind.Guid;
					return;

				case EnBasicKnownType.KnownTypeArray:
					node.Kind = SkipKind.List;
					node.Item = Build(type.GetElementType(), member: false);
					return;

				case EnBasicKnownType.DataTable:
					node.Kind = SkipKind.DataTable;
					return;

				case EnBasicKnownType.DataSet:
					node.Kind = SkipKind.DataSet;
					return;
			}

			node.Kind = SkipKind.Basic;
			node.BasicType = basic.KnownType;
			if (basic.KnownType == EnBasicKnownType.Enum)
			{
				var enumType = System.Nullable.GetUnderlyingType(type) ?? type;
				node.EnumCode = Type.GetTypeCode(Enum.GetUnderlyingType(enumType));
			}

			if (!node.Nullable)
			{
				switch (basic.KnownType)
				{
					case EnBasicKnownType.Bool:
					case EnBasicKnownType.Byte:
					case EnBasicKnownType.SByte:
					case EnBasicKnownType.DbNull:
						node.FixedSize = 1;
						break;
					case EnBasicKnownType.Char:
						node.FixedSize = 2;
						break;
					case EnBasicKnownType.Enum when node.EnumCode == TypeCode.Byte || node.EnumCode == TypeCode.SByte:
						node.FixedSize = 1;
						break;
				}
			}
		}

		private static Type ItemType(Type itemType, Type collectionType)
		{
			if (itemType == null)
				throw new InvalidTypeException($"Can't skip '{BoisContractInfo.TypeName(collectionType)}': only generic collections are supported.");
			return itemType;
		}

		private static Type MemberType(System.Reflection.MemberInfo member)
		{
			return member is System.Reflection.PropertyInfo property
				? property.PropertyType
				: ((System.Reflection.FieldInfo)member).FieldType;
		}

		#endregion
	}
}
