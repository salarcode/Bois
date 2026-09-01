using Salar.BinaryBuffers;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Salar.Bois.Serializers
{
    internal static class NumericSerializers
    {
        /// <summary>
        /// 0000 0000 = 0 -- No flags enabled
        /// </summary>
        internal const byte FlagNone = 0b0_0_0_0_0_0_0_0;

        /// <summary>
        /// 0100 0000 = 64
        /// </summary>
        internal const byte FlagIsNull = 0b0_1_0_0_0_0_0_0;

        /// <summary>
        /// 1000 0000 = 128
        /// </summary>
        private const byte FlagEmbedded = 0b1_0_0_0_0_0_0_0;

        /// <summary>
        /// 0111 1111 = 127
        /// </summary>
        private const byte EmbeddedMaxNumInByte = 0b0_1_1_1_1_1_1_1;

        /// <summary>
        /// 0011 1111 = 63
        /// </summary>
        private const byte EmbeddedNullableMaxNumInByte = 0b0_0_1_1_1_1_1_1;

        /// <summary>
        /// 0111 1111 = 127 
        /// </summary>
        private const byte MaskEmbedded = 0b0_1_1_1_1_1_1_1;

        /// <summary>
        /// 0011 1111 = 63 
        /// </summary>
        private const byte MaskEmbeddedNullable = 0b0_0_1_1_1_1_1_1;

        #region Array Pool

        private static byte[] ZeroByteArray = new byte[] { 0 };

        private static class SharedArray
        {
            [ThreadStatic]
            private static byte[]? _array;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static byte[] Get() => _array ??= new byte[16];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ClearArray4()
            {
                byte[] arr = Get(); // Fetch TLS reference once
                Unsafe.As<byte, int>(ref arr[0]) = 0; // Clears 4 bytes in a single instruction
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void ClearArray8()
            {
                byte[] arr = Get(); // Fetch TLS reference once
                Unsafe.As<byte, long>(ref arr[0]) = 0; // Clears 8 bytes in a single instruction
            }
        }

        #endregion

        #region Readers

        internal static sbyte? ReadVarSByteNullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                // number is embedded
                return (sbyte)(input & MaskEmbeddedNullable);
            }
            else
            {
                return reader.ReadSByte();
            }
        }

        internal static short? ReadVarInt16Nullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                // number is embedded
                return (short)(input & MaskEmbeddedNullable);
            }
            else
            {
                return ReadInt16Zigzag(ref reader);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static short ReadVarInt16<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            return ReadInt16Zigzag(ref reader);
        }

        internal static ushort? ReadVarUInt16Nullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                // number is embedded
                return (ushort)(input & MaskEmbeddedNullable);
            }
            else
            {
                return ReadUInt16Zigzag(ref reader);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ushort ReadVarUInt16<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            return ReadUInt16Zigzag(ref reader);
        }

        internal static int? ReadVarInt32Nullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                // number is embedded
                return input & MaskEmbeddedNullable;
            }
            else
            {
                return ReadInt32Zigzag(ref reader);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ReadVarInt32<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            return ReadInt32Zigzag(ref reader);
        }

        internal static uint? ReadVarUInt32Nullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                // number is embedded
                return (uint)(input & MaskEmbeddedNullable);
            }
            else
            {
                return ReadUInt32Zigzag(ref reader);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint ReadVarUInt32<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            return ReadUInt32Zigzag(ref reader);
        }

        internal static long? ReadVarInt64Nullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                // number is embedded
                return input & MaskEmbeddedNullable;
            }
            else
            {
                return ReadInt64Zigzag(ref reader);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long ReadVarInt64<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            return ReadInt64Zigzag(ref reader);
        }

        internal static ulong? ReadVarUInt64Nullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                // number is embedded
                return (ulong)(input & MaskEmbeddedNullable);
            }
            else
            {
                return ReadUInt64Zigzag(ref reader);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong ReadVarUInt64<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            return ReadUInt64Zigzag(ref reader);
        }

        internal static decimal? ReadVarDecimalNullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            var embedded = (input & FlagEmbedded) == FlagEmbedded;
            if (embedded)
            {
                return (input & MaskEmbeddedNullable);
            }

            int length = input;

#if NETFRAMEWORK
            var numBuff = reader.ReadBytes(length);
            return ConvertFromVarBinaryDecimal(numBuff);
#else
            var numBuff = reader.ReadSpan(length);
            return ConvertFromVarBinaryDecimal(numBuff);
#endif
        }

        internal static decimal ReadVarDecimal<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var input = reader.ReadByte();

            var embedded = (input & FlagEmbedded) == FlagEmbedded;
            if (embedded)
            {
                return (input & MaskEmbedded);
            }

            int length = input;

#if NETFRAMEWORK
            var numBuff = reader.ReadBytes(length);
            return ConvertFromVarBinaryDecimal(numBuff);
#else
            var numBuff = reader.ReadSpan(length);
            return ConvertFromVarBinaryDecimal(numBuff);
#endif
        }

        internal static double? ReadVarDoubleNullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
        {
            byte input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                // Shift byte directly into the top byte position (8th byte of 64-bit double)
                long embeddedBits = (long)(input & MaskEmbeddedNullable) << 56;
                return Unsafe.As<long, double>(ref embeddedBits);
            }

            int length = input;
            ReadOnlySpan<byte> span = reader.ReadSpan(length);

            long rawBits = length switch
            {
                8 => Unsafe.ReadUnaligned<long>(ref MemoryMarshal.GetReference(span)),
                7 => (long)span[0] << 8 | (long)span[1] << 16 | (long)span[2] << 24 | (long)span[3] << 32 | (long)span[4] << 40 | (long)span[5] << 48 | (long)span[6] << 56,
                6 => (long)span[0] << 16 | (long)span[1] << 24 | (long)span[2] << 32 | (long)span[3] << 40 | (long)span[4] << 48 | (long)span[5] << 56,
                5 => (long)span[0] << 24 | (long)span[1] << 32 | (long)span[2] << 40 | (long)span[3] << 48 | (long)span[4] << 56,
                4 => (long)span[0] << 32 | (long)span[1] << 40 | (long)span[2] << 48 | (long)span[3] << 56,
                3 => (long)span[0] << 40 | (long)span[1] << 48 | (long)span[2] << 56,
                2 => (long)span[0] << 48 | (long)span[1] << 56,
                1 => (long)span[0] << 56,
                _ => 0L
            };

            return Unsafe.As<long, double>(ref rawBits);
        }

        internal static double ReadVarDouble<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            byte input = reader.ReadByte();

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                long embeddedBits = (long)(input & MaskEmbedded) << 56;
                return Unsafe.As<long, double>(ref embeddedBits);
            }

            int length = input;
            ReadOnlySpan<byte> span = reader.ReadSpan(length);

            long rawBits = length switch
            {
                8 => Unsafe.ReadUnaligned<long>(ref MemoryMarshal.GetReference(span)),
                7 => (long)span[0] << 8 | (long)span[1] << 16 | (long)span[2] << 24 | (long)span[3] << 32 | (long)span[4] << 40 | (long)span[5] << 48 | (long)span[6] << 56,
                6 => (long)span[0] << 16 | (long)span[1] << 24 | (long)span[2] << 32 | (long)span[3] << 40 | (long)span[4] << 48 | (long)span[5] << 56,
                5 => (long)span[0] << 24 | (long)span[1] << 32 | (long)span[2] << 40 | (long)span[3] << 48 | (long)span[4] << 56,
                4 => (long)span[0] << 32 | (long)span[1] << 40 | (long)span[2] << 48 | (long)span[3] << 56,
                3 => (long)span[0] << 40 | (long)span[1] << 48 | (long)span[2] << 56,
                2 => (long)span[0] << 48 | (long)span[1] << 56,
                1 => (long)span[0] << 56,
                _ => 0L
            };

            return Unsafe.As<long, double>(ref rawBits);
        }

        internal static float? ReadVarSingleNullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
        {
            byte input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                int embeddedBits = (input & MaskEmbeddedNullable) << 24;
                return Unsafe.As<int, float>(ref embeddedBits);
            }

            int length = input;
            ReadOnlySpan<byte> span = reader.ReadSpan(length);

            int rawBits = length switch
            {
                4 => Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(span)),
                3 => (span[0] << 8) | (span[1] << 16) | (span[2] << 24),
                2 => (span[0] << 16) | (span[1] << 24),
                1 => span[0] << 24,
                _ => 0
            };

            return Unsafe.As<int, float>(ref rawBits);
        }

        internal static float ReadVarSingle<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
        {
            byte input = reader.ReadByte();

            if ((input & FlagEmbedded) == FlagEmbedded)
            {
                int embeddedBits = (input & MaskEmbedded) << 24;
                return Unsafe.As<int, float>(ref embeddedBits);
            }

            int length = input;
            ReadOnlySpan<byte> span = reader.ReadSpan(length);

            int rawBits = length switch
            {
                4 => Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(span)),
                3 => (span[0] << 8) | (span[1] << 16) | (span[2] << 24),
                2 => (span[0] << 16) | (span[1] << 24),
                1 => span[0] << 24,
                _ => 0
            };

            return Unsafe.As<int, float>(ref rawBits);
        }

        internal static byte? ReadVarByteNullable<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
        {
            var input = reader.ReadByte();
            if (input == FlagIsNull)
                return null;

            var embedded = (input & FlagEmbedded) == FlagEmbedded;
            if (embedded)
            {
                return (byte)(input & MaskEmbeddedNullable);
            }

            return reader.ReadByte();
        }

        #endregion

        #region Writers

        /// <summary>
        /// [int data as zigzag] not embeddable
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="num"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteVarInt<TWriter>(ref TWriter writer, int num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            WriteZigzag(ref writer, num);
        }


        /// <summary>
        /// [EmbedIndicator-NullIndicator-0-0-0-0-0-0] [optional data]  0..63 can be embedded
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="num"></param>
        internal static void WriteVarInt<TWriter>(ref TWriter writer, int? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            if (num == null)
            {
                // null flag
                writer.Write(FlagIsNull);
                return;
            }

            if (num > EmbeddedNullableMaxNumInByte || num < 0)
            {
                // number is not embeddable 

                writer.Write(FlagNone);
                WriteZigzag(ref writer, num.Value);
            }
            else
            {
                byte numByte = (byte)num;

                // set the flag of inside
                numByte = (byte)(numByte | FlagEmbedded);
                writer.Write(numByte);
            }
        }

        /// <summary>
        /// [uint data as zigzag] not embeddable
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="num"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteVarInt<TWriter>(ref TWriter writer, uint num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            WriteZigzag(ref writer, num);
        }

        /// <summary>
        /// [EmbedIndicator-NullIndicator-0-0-0-0-0-0] [optional data]  0..TODO can be embedded
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="num"></param>
        internal static void WriteVarInt<TWriter>(ref TWriter writer, uint? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            if (num == null)
            {
                // null flag
                writer.Write(FlagIsNull);
                return;
            }

            if (num > EmbeddedNullableMaxNumInByte)
            {
                // number is not embeddable 

                writer.Write(FlagNone);
                WriteZigzag(ref writer, num.Value);
            }
            else unchecked
                {
                    byte numByte = (byte)num;

                    // set the flag of inside
                    numByte = (byte)(numByte | FlagEmbedded);
                    writer.Write(numByte);
                }
        }

        /// <summary>
        /// Same as "uint?" except that it doesn't store null value, but still preserves null flag.
        /// To be used to store member counts
        /// </summary>
        /// <remarks>
        /// The value stored as member count can be 'null', but since this method is called where it is obvious that 
        /// the don't have null value, there is no point creating Nullable object to convert it
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteUIntNullableMemberCount<TWriter>(ref TWriter writer, uint num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            // NOTE:
            // Member count can be null, but not the place this method is being called
            // Hence why i'm storing null flag

            if (num > EmbeddedNullableMaxNumInByte)
            {
                // number is not embeddable 

                writer.Write(FlagNone);
                WriteZigzag(ref writer, num);
            }
            else
            {
                byte numByte = (byte)num;

                // set the flag of inside
                numByte = (byte)(numByte | FlagEmbedded);
                writer.Write(numByte);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteVarInt<TWriter>(ref TWriter writer, short num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            WriteZigzag(ref writer, num);
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void WriteVarInt<TWriter>(ref TWriter writer, short? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            if (num == null)
            {
                // null flag
                writer.Write(FlagIsNull);
                return;
            }

            if (num > EmbeddedNullableMaxNumInByte || num < 0)
            {
                // number is not embeddable 

                writer.Write(FlagNone);
                WriteZigzag(ref writer, num.Value);
            }
            else
            {
                byte numByte = (byte)num;

                // set the flag of inside
                numByte = (byte)(numByte | FlagEmbedded);
                writer.Write(numByte);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteVarInt<TWriter>(ref TWriter writer, ushort num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            WriteZigzag(ref writer, num);
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void WriteVarInt<TWriter>(ref TWriter writer, ushort? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            if (num == null)
            {
                // null flag
                writer.Write(FlagIsNull);
                return;
            }

            if (num > EmbeddedNullableMaxNumInByte || num < 0)
            {
                // number is not embeddable 

                writer.Write(FlagNone);
                WriteZigzag(ref writer, num.Value);
            }
            else
            {
                byte numByte = (byte)num;

                // set the flag of inside
                numByte = (byte)(numByte | FlagEmbedded);
                writer.Write(numByte);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteVarInt<TWriter>(ref TWriter writer, long num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            WriteZigzag(ref writer, num);
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void WriteVarInt<TWriter>(ref TWriter writer, long? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            if (num == null)
            {
                // null flag
                writer.Write(FlagIsNull);
                return;
            }

            if (num > EmbeddedNullableMaxNumInByte || num < 0)
            {
                // number is not embeddable 

                writer.Write(FlagNone);
                WriteZigzag(ref writer, num.Value);
            }
            else
            {
                byte numByte = (byte)num;

                // set the flag of inside
                numByte = (byte)(numByte | FlagEmbedded);
                writer.Write(numByte);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void WriteVarInt<TWriter>(ref TWriter writer, ulong num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            WriteZigzag(ref writer, num);
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void WriteVarInt<TWriter>(ref TWriter writer, ulong? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            if (num == null)
            {
                // null flag
                writer.Write(FlagIsNull);
                return;
            }

            if (num > EmbeddedNullableMaxNumInByte || num < 0)
            {
                // number is not embeddable 

                writer.Write(FlagNone);
                WriteZigzag(ref writer, num.Value);
            }
            else
            {
                byte numByte = (byte)num;

                // set the flag of inside
                numByte = (byte)(numByte | FlagEmbedded);
                writer.Write(numByte);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void WriteVarInt<TWriter>(ref TWriter writer, byte? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            if (num == null)
            {
                // null flag
                writer.Write(FlagIsNull);
                return;
            }

            if (num > EmbeddedNullableMaxNumInByte || num < 0)
            {
                // number is not embeddable 

                writer.Write(FlagNone);
                writer.Write(num.Value);
            }
            else
            {
                byte numByte = (byte)num;

                // set the flag of inside
                numByte = (byte)(numByte | FlagEmbedded);
                writer.Write(numByte);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void WriteVarInt<TWriter>(ref TWriter writer, sbyte? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            if (num == null)
            {
                // null flag
                writer.Write(FlagIsNull);
                return;
            }

            if (num > EmbeddedNullableMaxNumInByte || num < 0)
            {
                // number is not embeddable 

                writer.Write(FlagNone);
                writer.Write(num.Value);
            }
            else
            {
                byte numByte = (byte)num;

                // set the flag of inside
                numByte = (byte)(numByte | FlagEmbedded);
                writer.Write(numByte);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void WriteVarDecimal<TWriter>(ref TWriter writer, float num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            uint bits = Unsafe.As<float, uint>(ref num);

            if (bits == 0)
            {
                // 0 is represented as length 1, single 0 byte (embedded)
                writer.Write((byte)FlagEmbedded);
                return;
            }

            // Number of trailing 0 bytes (0 to 3)
#if NET6_0_OR_GREATER
            int trailingZeroBytes = BitOperations.TrailingZeroCount(bits) >> 3;
#else
            int trailingZeroBytes = GetTrailingZeroBytesFallback(bits);
#endif

            // Position of first non-zero byte (0 to 3) and actual encoded byte length (1 to 4)
            int position = trailingZeroBytes;
            int numLen = 4 - position;

            // Shift out trailing zero bytes so the first non-zero byte is at bits & 0xFF
            uint shiftedBits = bits >> (position * 8);
            byte firstByte = (byte)shiftedBits;

            if (numLen == 1 && firstByte <= EmbeddedMaxNumInByte)
            {
                // Embedded branch: direct single byte write
                writer.Write((byte)(firstByte | FlagEmbedded));
            }
            else
            {
                // Write byte length header
                writer.Write((byte)numLen);

                unsafe
                {
                    // Allocate 4 bytes directly on the execution stack via raw pointer
                    byte* ptr = stackalloc byte[4];

                    // Zero-bounds-check pointer offsets
                    ptr[0] = firstByte;
                    if (numLen > 1) ptr[1] = (byte)(shiftedBits >> 8);
                    if (numLen > 2) ptr[2] = (byte)(shiftedBits >> 16);
                    if (numLen > 3) ptr[3] = (byte)(shiftedBits >> 24);

                    // Create span directly from pointer; clears compiler ref-escape checks
                    writer.Write(new ReadOnlySpan<byte>(ptr, numLen));
                }
            }
        }

#if !NET6_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetTrailingZeroBytesFallback(uint v)
        {
            if ((v & 0xFF) != 0) return 0;
            if ((v & 0xFFFF) != 0) return 1;
            if ((v & 0xFFFFFF) != 0) return 2;
            return 3;
        }
#endif

        internal static void WriteVarDecimal<TWriter>(ref TWriter writer, float? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            if (!num.HasValue)
            {
                // Null marker
                writer.Write(FlagIsNull);
                return;
            }

            float value = num.GetValueOrDefault();
            uint bits = Unsafe.As<float, uint>(ref value);

            if (bits == 0)
            {
                // 0 is represented as length 1, single 0 byte (embedded)
                writer.Write((byte)FlagEmbedded);
                return;
            }

            // Number of trailing 0 bytes (0 to 3)
#if NET6_0_OR_GREATER
            int trailingZeroBytes = BitOperations.TrailingZeroCount(bits) >> 3;
#else
            int trailingZeroBytes = GetTrailingZeroBytesFallback(bits);
#endif

            // Position of first non-zero byte (0 to 3) and actual encoded byte length (1 to 4)
            int position = trailingZeroBytes;
            int numLen = 4 - position;

            // Shift out trailing zero bytes so the first non-zero byte is at bits & 0xFF
            uint shiftedBits = bits >> (position * 8);
            byte firstByte = (byte)shiftedBits;

            if (numLen == 1 && firstByte <= EmbeddedNullableMaxNumInByte)
            {
                // Embedded branch: direct single byte write
                writer.Write((byte)(firstByte | FlagEmbedded));
            }
            else
            {
                // Write byte length header
                writer.Write((byte)numLen);

                unsafe
                {
                    // Allocate 4 bytes directly on execution stack via raw pointer
                    byte* ptr = stackalloc byte[4];

                    // Zero-bounds-check pointer offsets
                    ptr[0] = firstByte;
                    if (numLen > 1) ptr[1] = (byte)(shiftedBits >> 8);
                    if (numLen > 2) ptr[2] = (byte)(shiftedBits >> 16);
                    if (numLen > 3) ptr[3] = (byte)(shiftedBits >> 24);

                    // Create span directly from pointer; bypasses ref-escape rules
                    writer.Write(new ReadOnlySpan<byte>(ptr, numLen));
                }
            }
        }
        internal static void WriteVarDecimal<TWriter>(ref TWriter writer, double num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
        {
            ulong bits = Unsafe.As<double, ulong>(ref num);

            if (bits == 0)
            {
                // 0 is represented as length 1, single 0 byte (embedded)
                writer.Write((byte)FlagEmbedded);
                return;
            }

            // Number of trailing 0 bytes (0 to 7)
#if NET6_0_OR_GREATER
            int trailingZeroBytes = BitOperations.TrailingZeroCount(bits) >> 3;
#else
            int trailingZeroBytes = GetTrailingZeroBytesFallback(bits);
#endif

            // Position of first non-zero byte (0 to 7) and actual encoded byte length (1 to 8)
            int position = trailingZeroBytes;
            int numLen = 8 - position;

            // Shift out trailing zero bytes so the first non-zero byte is at bits & 0xFF
            ulong shiftedBits = bits >> (position * 8);
            byte firstByte = (byte)shiftedBits;

            if (numLen == 1 && firstByte <= EmbeddedMaxNumInByte)
            {
                // Embedded branch: direct single byte write
                writer.Write((byte)(firstByte | FlagEmbedded));
            }
            else
            {
                // Write byte length header
                writer.Write((byte)numLen);

                unsafe
                {
                    // Allocate 8 bytes directly on execution stack via raw pointer
                    byte* ptr = stackalloc byte[8];

                    // Zero-bounds-check pointer offsets
                    ptr[0] = firstByte;
                    if (numLen > 1) ptr[1] = (byte)(shiftedBits >> 8);
                    if (numLen > 2) ptr[2] = (byte)(shiftedBits >> 16);
                    if (numLen > 3) ptr[3] = (byte)(shiftedBits >> 24);
                    if (numLen > 4) ptr[4] = (byte)(shiftedBits >> 32);
                    if (numLen > 5) ptr[5] = (byte)(shiftedBits >> 40);
                    if (numLen > 6) ptr[6] = (byte)(shiftedBits >> 48);
                    if (numLen > 7) ptr[7] = (byte)(shiftedBits >> 56);

                    // Create span directly from pointer; bypasses ref-escape rules
                    writer.Write(new ReadOnlySpan<byte>(ptr, numLen));
                }
            }
        }

        internal static void WriteVarDecimal<TWriter>(ref TWriter writer, double? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
        {
            if (!num.HasValue)
            {
                // Null marker
                writer.Write(FlagIsNull);
                return;
            }

            double value = num.GetValueOrDefault();
            ulong bits = Unsafe.As<double, ulong>(ref value);

            if (bits == 0)
            {
                // 0 is represented as length 1, single 0 byte (embedded)
                writer.Write((byte)FlagEmbedded);
                return;
            }

            // Number of trailing 0 bytes (0 to 7)
#if NET6_0_OR_GREATER
            int trailingZeroBytes = BitOperations.TrailingZeroCount(bits) >> 3;
#else
            int trailingZeroBytes = GetTrailingZeroBytesFallback(bits);
#endif

            // Position of first non-zero byte (0 to 7) and actual encoded byte length (1 to 8)
            int position = trailingZeroBytes;
            int numLen = 8 - position;

            // Shift out trailing zero bytes so the first non-zero byte is at bits & 0xFF
            ulong shiftedBits = bits >> (position * 8);
            byte firstByte = (byte)shiftedBits;

            if (numLen == 1 && firstByte <= EmbeddedNullableMaxNumInByte)
            {
                // Embedded branch: direct single byte write
                writer.Write((byte)(firstByte | FlagEmbedded));
            }
            else
            {
                // Write byte length header
                writer.Write((byte)numLen);

                unsafe
                {
                    // Allocate 8 bytes directly on execution stack via raw pointer
                    byte* ptr = stackalloc byte[8];

                    // Zero-bounds-check pointer offsets
                    ptr[0] = firstByte;
                    if (numLen > 1) ptr[1] = (byte)(shiftedBits >> 8);
                    if (numLen > 2) ptr[2] = (byte)(shiftedBits >> 16);
                    if (numLen > 3) ptr[3] = (byte)(shiftedBits >> 24);
                    if (numLen > 4) ptr[4] = (byte)(shiftedBits >> 32);
                    if (numLen > 5) ptr[5] = (byte)(shiftedBits >> 40);
                    if (numLen > 6) ptr[6] = (byte)(shiftedBits >> 48);
                    if (numLen > 7) ptr[7] = (byte)(shiftedBits >> 56);

                    // Create span directly from pointer; bypasses ref-escape rules
                    writer.Write(new ReadOnlySpan<byte>(ptr, numLen));
                }
            }
        }

#if !NET6_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetTrailingZeroBytesFallback(ulong v)
        {
            if ((v & 0xFFUL) != 0) return 0;
            if ((v & 0xFFFFUL) != 0) return 1;
            if ((v & 0xFFFFFFUL) != 0) return 2;
            if ((v & 0xFFFFFFFFUL) != 0) return 3;
            if ((v & 0xFFFFFFFFFFUL) != 0) return 4;
            if ((v & 0xFFFFFFFFFFFFUL) != 0) return 5;
            if ((v & 0xFFFFFFFFFFFFFFUL) != 0) return 6;
            return 7;
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        internal static void WriteVarDecimal<TWriter>(ref TWriter writer, decimal num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            unsafe
            {
                // 16 bytes stack memory for the decimal payload
                byte* ptr = stackalloc byte[16];
                Span<int> bitsSpan = new Span<int>(ptr, 4);

#if NET6_0_OR_GREATER
                // Zero-allocation extraction into stack span (.NET 5+)
                decimal.GetBits(num, bitsSpan);
#else
                // Fallback for older targets: rereinterpret decimal memory directly without allocation
                bitsSpan[0] = Unsafe.Add(ref Unsafe.As<decimal, int>(ref num), 0);
                bitsSpan[1] = Unsafe.Add(ref Unsafe.As<decimal, int>(ref num), 1);
                bitsSpan[2] = Unsafe.Add(ref Unsafe.As<decimal, int>(ref num), 2);
                bitsSpan[3] = Unsafe.Add(ref Unsafe.As<decimal, int>(ref num), 3);
#endif

                // Calculate actual non-zero byte length (1 to 16)
                int numLen = GetDecimalEncodedLength(ptr);
                byte firstByte = ptr[0];

                if (numLen == 1 && firstByte <= EmbeddedMaxNumInByte)
                {
                    // Embedded branch: single-byte write
                    writer.Write((byte)(firstByte | FlagEmbedded));
                }
                else
                {
                    // Write length header
                    writer.Write((byte)numLen);

                    // Direct zero-copy write via ReadOnlySpan<byte>; raw ptr satisfies ref-escape checks
                    writer.Write(new ReadOnlySpan<byte>(ptr, numLen));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int GetDecimalEncodedLength(byte* ptr)
        {
            // Reinterpret 16 bytes as two 64-bit uints for fast trailing-non-zero byte search
            ulong high = *(ulong*)(ptr + 8);
            ulong low = *(ulong*)ptr;

            if (high != 0)
            {
#if NET6_0_OR_GREATER
                // (64 - LeadingZeroCount(high)) gives highest non-zero bit position
                int highestBit = 64 - BitOperations.LeadingZeroCount(high);
                return 8 + ((highestBit + 7) >> 3);
#else
                return 8 + GetByteLengthFallback(high);
#endif
            }

            if (low != 0)
            {
#if NET6_0_OR_GREATER
                int highestBit = 64 - BitOperations.LeadingZeroCount(low);
                return (highestBit + 7) >> 3;
#else
                return GetByteLengthFallback(low);
#endif
            }

            // Default: zero decimal encodes as 1 byte (value 0)
            return 1;
        }

#if !NET6_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetByteLengthFallback(ulong v)
        {
            if ((v & 0xFFFFFFFFFFFFFFFFUL) >= 0x0001000000000000UL) return 8;
            if ((v & 0x0000FFFFFFFFFFFFUL) >= 0x0000000100000000UL) return 7;
            if ((v & 0x000000FFFFFFFFFFUL) >= 0x0000000001000000UL) return 6;
            if ((v & 0x00000000FFFFFFFFUL) >= 0x0000000000010000UL) return 5;
            if ((v & 0x0000000000FFFFFFUL) >= 0x0000000000000100UL) return 4;
            if ((v & 0x000000000000FFFFUL) >= 0x0000000000000001UL) return 3;
            if ((v & 0x00000000000000FFUL) != 0) return 1;
            return 2;
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        internal static void WriteVarDecimal<TWriter>(ref TWriter writer, decimal? num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
        {
            if (!num.HasValue)
            {
                // Null marker
                writer.Write(FlagIsNull);
                return;
            }

            decimal value = num.GetValueOrDefault();

            unsafe
            {
                // 16 bytes stack memory for the decimal payload
                byte* ptr = stackalloc byte[16];
                Span<int> bitsSpan = new Span<int>(ptr, 4);

#if NET6_0_OR_GREATER
                // Zero-allocation extraction into stack span (.NET 5+)
                decimal.GetBits(value, bitsSpan);
#else
                // Fallback for older targets: reinterpret decimal memory directly without allocation
                bitsSpan[0] = Unsafe.Add(ref Unsafe.As<decimal, int>(ref value), 0);
                bitsSpan[1] = Unsafe.Add(ref Unsafe.As<decimal, int>(ref value), 1);
                bitsSpan[2] = Unsafe.Add(ref Unsafe.As<decimal, int>(ref value), 2);
                bitsSpan[3] = Unsafe.Add(ref Unsafe.As<decimal, int>(ref value), 3);
#endif

                // Calculate actual non-zero byte length (1 to 16)
                int numLen = GetDecimalEncodedLength(ptr);
                byte firstByte = ptr[0];

                if (numLen == 1 && firstByte <= EmbeddedNullableMaxNumInByte)
                {
                    // Embedded branch: single-byte write
                    writer.Write((byte)(firstByte | FlagEmbedded));
                }
                else
                {
                    // Write length header
                    writer.Write((byte)numLen);

                    // Direct zero-copy write via ReadOnlySpan<byte>; raw ptr satisfies ref-escape checks
                    writer.Write(new ReadOnlySpan<byte>(ptr, numLen));
                }
            }
        }
        #endregion

        #region Binary Converters & Writers

        private static short ReadInt16Zigzag<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            uint result = 0;
            int shift = 0;
            uint b;

            do
            {
                if (shift == 21) // 3 bytes * 7 bits = 21 max shift for 16-bit int
                    throw new InvalidDataException("Invalid integer value in the input stream.");

                b = reader.ReadByte();
                result |= (b & 0x7FU) << shift;
                shift += 7;

            } while ((b & 0x80U) != 0);

            // Simplified ZigZag decoding (sar + xor in 2 CPU instructions)
            return (short)((int)(result >> 1) ^ -(int)(result & 1U));
        }

        private static ushort ReadUInt16Zigzag<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            uint result = 0;
            int shift = 0;
            uint b;

            do
            {
                if (shift == 21) // 3 bytes * 7 bits = 21 max shift for 16-bit uint
                    throw new InvalidDataException("Invalid integer value in the input stream.");

                b = reader.ReadByte();
                result |= (b & 0x7FU) << shift;
                shift += 7;

            } while ((b & 0x80U) != 0);

            return (ushort)result;
        }

        private static int ReadInt32Zigzag<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            uint result = 0;
            int shift = 0;
            uint b;

            do
            {
                if (shift == 35) // 5 bytes * 7 bits = 35 max shift for 32-bit int
                    throw new InvalidDataException("Invalid integer value in the input stream.");

                b = reader.ReadByte();
                result |= (b & 0x7FU) << shift;
                shift += 7;

            } while ((b & 0x80U) != 0);

            // Simplified ZigZag decoding (2 assembly instructions: SAR + XOR)
            return (int)(result >> 1) ^ -(int)(result & 1);
        }

        private static uint ReadUInt32Zigzag<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            uint result = 0;
            int shift = 0;
            uint b;

            do
            {
                if (shift == 35) // 5 bytes * 7 bits = 35 max shift for 32-bit uint
                    throw new InvalidDataException("Invalid integer value in the input stream.");

                b = reader.ReadByte();
                result |= (b & 0x7FU) << shift;
                shift += 7;

            } while ((b & 0x80U) != 0);

            return result;
        }

        private static long ReadInt64Zigzag<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            ulong result = 0;
            int shift = 0;
            ulong b;

            do
            {
                if (shift == 70) // 10 bytes * 7 bits = 70 max shift for 64-bit long
                    throw new InvalidDataException("Invalid integer long in the input stream.");

                b = reader.ReadByte();
                result |= (b & 0x7FUL) << shift;
                shift += 7;

            } while ((b & 0x80UL) != 0);

            // Simplified ZigZag decoding (sar + xor in 2 CPU instructions)
            return (long)(result >> 1) ^ -(long)(result & 1UL);
        }

        private static ulong ReadUInt64Zigzag<TReader>(ref TReader reader)
            where TReader : IBufferReader
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            ulong result = 0;
            int shift = 0;
            ulong b;

            do
            {
                if (shift == 70) // 10 bytes * 7 bits = 70 max shift for 64-bit uint/ulong
                    throw new InvalidDataException("Invalid integer long in the input stream.");

                b = reader.ReadByte();
                result |= (b & 0x7FUL) << shift;
                shift += 7;

            } while ((b & 0x80UL) != 0);

            return result;
        }


        private static void WriteZigzag<TWriter>(ref TWriter writer, long num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var zigZagEncoded = unchecked((ulong)((num << 1) ^ (num >> 63)));
            while ((zigZagEncoded & ~0x7FUL) != 0)
            {
                writer.Write((byte)((zigZagEncoded | 0x80) & 0xFF));
                zigZagEncoded >>= 7;
            }
            writer.Write((byte)zigZagEncoded);
        }

        private static void WriteZigzag<TWriter>(ref TWriter writer, ulong num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            while ((num & ~0x7FUL) != 0)
            {
                writer.Write((byte)((num | 0x80) & 0xFF));
                num >>= 7;
            }
            writer.Write((byte)num);
        }

        private static void WriteZigzag<TWriter>(ref TWriter writer, int num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            uint v = unchecked((uint)((num << 1) ^ (num >> 31)));

            // 1-Byte Path (Values 0..127) -> 1 Write call
            if (v < (1u << 7))
            {
                writer.Write((byte)v);
                return;
            }

            // 2-Byte Path (Values 128..16,383) -> 1 Write call
            if (v < (1u << 14))
            {
                ushort packed = (ushort)((v & 0x7F) | 0x80 | ((v >> 7) << 8));
                writer.Write(packed);
                return;
            }

            // 3-Byte Path -> 2 Write calls (ushort + byte)
            if (v < (1u << 21))
            {
                ushort lower = (ushort)((v & 0x7F) | 0x80 | (((v >> 7) & 0x7F) | 0x80) << 8);
                byte upper = (byte)(v >> 14);

                writer.Write(lower);
                writer.Write(upper);
                return;
            }

            // 4-Byte Path -> 1 Write call (uint)
            if (v < (1u << 28))
            {
                uint packed = (v & 0x7F) | 0x80
                            | (((v >> 7) & 0x7F) | 0x80) << 8
                            | (((v >> 14) & 0x7F) | 0x80) << 16
                            | ((v >> 21) << 24);

                writer.Write(packed);
                return;
            }

            // 5-Byte Path -> 2 Write calls (uint + byte)
            uint lower5 = (v & 0x7F) | 0x80
                        | (((v >> 7) & 0x7F) | 0x80) << 8
                        | (((v >> 14) & 0x7F) | 0x80) << 16
                        | (((v >> 21) & 0x7F) | 0x80) << 24;
            byte upper5 = (byte)(v >> 28);

            writer.Write(lower5);
            writer.Write(upper5);
        }

        private static void WriteZigzag<TWriter>(ref TWriter writer, uint num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            while ((num & ~0x7F) != 0)
            {
                writer.Write((byte)((num | 0x80) & 0xFF));
                num >>= 7;
            }
            writer.Write((byte)num);
        }

        private static void WriteZigzag<TWriter>(ref TWriter writer, short num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            var zigZagEncoded = unchecked((ushort)((num << 1) ^ (num >> 15)));
            while ((zigZagEncoded & ~0x7F) != 0)
            {
                writer.Write((byte)((zigZagEncoded | 0x80) & 0xFF));
                zigZagEncoded >>= 7;
            }
            writer.Write((byte)zigZagEncoded);
        }

        private static void WriteZigzag<TWriter>(ref TWriter writer, ushort num)
            where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
                , allows ref struct
#endif
        {
            while ((num & ~0x7F) != 0)
            {
                writer.Write((byte)((num | 0x80) & 0xFF));
                num >>= 7;
            }
            writer.Write((byte)num);
        }

        #endregion

        #region Binary Converters

        private static short ConvertFromVarBinaryInt16(byte[] numBuff)
        {
            short result = numBuff[0];

            if (numBuff.Length == 1)
                return result;

            result = unchecked((short)((ushort)result | (numBuff[1] << 8)));
            return result;
        }

        private static ushort ConvertFromVarBinaryUInt16(byte[] numBuff)
        {
            ushort result;
            if (numBuff.Length == 4)
            {
                result = numBuff[0];
                result = unchecked((ushort)(short)(result | (numBuff[1] << 8)));
            }
            else
            {
                var len = numBuff.Length;

                result = numBuff[0];
                if (len == 1)
                    return result;

                result = unchecked((ushort)(short)(result | (numBuff[1] << 8)));
                if (len == 2)
                    return result;

                result = unchecked((ushort)(short)(result | (numBuff[2] << 16)));
                if (len == 3)
                    return result;

                result = unchecked((ushort)(short)(result | (numBuff[3] << 24)));
            }
            return result;
        }

        private static int ConvertFromVarBinaryInt32(byte[] numBuff, int len)
        {
            int result;
            if (len == 4)
            {
                result = numBuff[0];
                result = unchecked(result | (numBuff[1] << 8));
                result = unchecked(result | (numBuff[2] << 16));
                result = unchecked(result | (numBuff[3] << 24));
            }
            else
            {
                result = numBuff[0];
                if (len == 1)
                    return result;

                result = unchecked(result | (numBuff[1] << 8));
                if (len == 2)
                    return result;

                result = unchecked(result | (numBuff[2] << 16));
                if (len == 3)
                    return result;

                result = unchecked(result | (numBuff[3] << 24));
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ConvertFromVarBinaryInt32StartIndex(ReadOnlySpan<byte> numBuff, int startIndex)
        {
            return ((numBuff[startIndex + 0] | (numBuff[startIndex + 1] << 8)) | (numBuff[startIndex + 2] << 16)) | (numBuff[startIndex + 3] << 24);
        }

        private static uint ConvertFromVarBinaryUInt32(byte[] numBuff, int len)
        {
            uint result;
            if (len == 4)
            {
                result = numBuff[0];
                result = unchecked((uint)((int)result | (numBuff[1] << 8)));
                result = unchecked((uint)((int)result | (numBuff[2] << 16)));
                result = unchecked((uint)((int)result | (numBuff[3] << 24)));
            }
            else
            {
                result = numBuff[0];
                if (len == 1)
                    return result;

                result = unchecked((uint)((int)result | (numBuff[1] << 8)));
                if (len == 2)
                    return result;

                result = unchecked((uint)((int)result | (numBuff[2] << 16)));
                if (len == 3)
                    return result;

                result = unchecked((uint)((int)result | (numBuff[3] << 24)));
            }
            return result;
        }

        private static long ConvertFromVarBinaryInt64(byte[] numBuff)
        {
            uint num = unchecked((uint)(((numBuff[0] | (numBuff[1] << 8)) | (numBuff[2] << 16)) | (numBuff[3] << 24)));
            uint num2 = unchecked((uint)(((numBuff[4] | (numBuff[5] << 8)) | (numBuff[6] << 16)) | (numBuff[7] << 24)));
            return unchecked((long)((((ulong)num2) << 32) | ((ulong)num)));
        }

        private static ulong ConvertFromVarBinaryUInt64(byte[] numBuff)
        {
            uint num = unchecked((uint)(((numBuff[0] | (numBuff[1] << 8)) | (numBuff[2] << 16)) | (numBuff[3] << 24)));
            uint num2 = unchecked((uint)(((numBuff[4] | (numBuff[5] << 8)) | (numBuff[6] << 16)) | (numBuff[7] << 24)));
            return unchecked(((ulong)num2 << 32) | (ulong)num);

        }

        private static decimal ConvertFromVarBinaryDecimal(ReadOnlySpan<byte> numBuff)
        {
            if (numBuff.Length == 16)
            {
                return new decimal(
#if NET6_0_OR_GREATER
                    stackalloc
#else
                    new
#endif
                int[4]
                {
                    ConvertFromVarBinaryInt32StartIndex(numBuff, 4 * 0),
                    ConvertFromVarBinaryInt32StartIndex(numBuff, 4 * 1),
                    ConvertFromVarBinaryInt32StartIndex(numBuff, 4 * 2),
                    ConvertFromVarBinaryInt32StartIndex(numBuff, 4 * 3)
                });
            }

            // when the stored size is smaller than 16 bytes

            Span<byte> buff =
#if NET6_0_OR_GREATER
                    stackalloc
#else
                    new
#endif
                byte[16];

            numBuff.CopyTo(buff);

            return new decimal(
#if NET6_0_OR_GREATER
                stackalloc
#else
                new
#endif
            int[4]
            {
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 0),
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 1),
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 2),
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 3)
            });
        }

        private static decimal ConvertFromVarBinaryDecimal(byte[] numBuff)
        {
            Span<byte> buff = numBuff.Length < 16 ? // 16 is required
#if NET6_0_OR_GREATER
                stackalloc
#else
                new
#endif
                byte[16] : numBuff;
            if (numBuff.Length < 16)
            {
                // TODO: check why SharedArray.Get() decreases performance by 3x times
                numBuff.AsSpan().CopyTo(buff);
            }
            else
            {
                buff = numBuff;
            }
            return new decimal(
#if NET6_0_OR_GREATER
                stackalloc
#else
                new
#endif
            int[4]
            {
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 0),
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 1),
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 2),
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 3)
            });
        }

        [Obsolete("Use alternative methods")]
        private static decimal ConvertFromVarBinaryDecimal_OLD(byte[] numBuff)
        {
            byte[] buff;
            if (numBuff.Length < 16)
            {
                // TODO: check why SharedArray.Get() decreases performance by 3x times
                buff = new byte[16]; // 16 required
                Array.Copy(numBuff, 0, buff, 0, numBuff.Length);
            }
            else
            {
                buff = numBuff;
            }
            return new decimal(
#if NET6_0_OR_GREATER
                stackalloc
#else
                new
#endif
            int[4]
            {
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 0),
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 1),
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 2),
                ConvertFromVarBinaryInt32StartIndex(buff, 4 * 3)
            });
        }

        [Obsolete("Use alternative methods")]
        private static double ConvertFromVarBinaryDouble(byte[] numBuff)
        {
            if (numBuff.Length == 8)
            {
                return BitConverter.ToDouble(numBuff, 0);
            }
            else
            {
                var doubleBuff = new byte[8];

                Array.Copy(numBuff, 0, doubleBuff, 8 - numBuff.Length, numBuff.Length);
                return BitConverter.ToDouble(doubleBuff, 0);
            }
        }

        [Obsolete("Use alternative methods")]
        private static float ConvertFromVarBinarySingle(byte[] numBuff)
        {
            if (numBuff.Length == 4)
            {
                return BitConverter.ToSingle(numBuff, 0);
            }
            else
            {
                var doubleBuff = new byte[4];

                Array.Copy(numBuff, 0, doubleBuff, 4 - numBuff.Length, numBuff.Length);
                return BitConverter.ToSingle(doubleBuff, 0);
            }
        }

        /// <summary>
        /// TODO: is always the result 4 bytes?
        /// </summary>
        private static byte[] ConvertToVarBinaryZigzag(int value, out byte length)
        {
            var buff = SharedArray.Get();
            length = 0;

            var zigZagEncoded = unchecked((uint)((value << 1) ^ (value >> 31)));

            while ((zigZagEncoded & ~0x7F) != 0)
            {
                buff[length++] = (byte)((zigZagEncoded | 0x80) & 0xFF);
                zigZagEncoded >>= 7;
            }
            buff[length++] = (byte)zigZagEncoded;

            return buff;
        }

        /// <summary>
        /// TODO: is always the result 4 bytes?
        /// </summary>
        private static byte[] ConvertToVarBinaryZigzag(long value, out byte length)
        {
            var buff = SharedArray.Get();
            length = 0;

            var zigZagEncoded = unchecked((ulong)((value << 1) ^ (value >> 63)));

            while ((zigZagEncoded & ~0x7FUL) != 0)
            {
                buff[length++] = (byte)((zigZagEncoded | 0x80) & 0xFF);
                zigZagEncoded >>= 7;
            }
            buff[length++] = (byte)zigZagEncoded;

            return buff;
        }

        private static byte[] ConvertToVarBinary(uint value, out byte length)
        {
            if (value == 0)
            {
                length = 1;
                return ZeroByteArray;
            }

            var buff = SharedArray.Get();
            SharedArray.ClearArray4();

            var num1 = (byte)value;
            var num2 = unchecked((byte)(value >> 8));
            var num3 = unchecked((byte)(value >> 16));
            var num4 = unchecked((byte)(value >> 24));


            buff[0] = num1;

            if (num2 > 0)
            {
                buff[1] = num2;
            }
            else if (num3 == 0 && num4 == 0)
            {
                length = 1;
                return buff;
            }

            if (num3 > 0)
            {
                buff[2] = num3;
            }
            else if (num4 == 0)
            {
                length = 2;
                return buff;
            }

            if (num4 > 0)
                buff[3] = num4;
            else
            {
                length = 3;
                return buff;
            }
            length = 4;
            return buff;
        }

        //private static byte[] ConvertToVarBinary(long value, out byte length)
        //{
        //	var buff = SharedArray.Get();
        //	buff[0] = (byte)value;
        //	buff[1] = unchecked((byte)(value >> 8));
        //	buff[2] = unchecked((byte)(value >> 16));
        //	buff[3] = unchecked((byte)(value >> 24));
        //	buff[4] = unchecked((byte)(value >> 32));
        //	buff[5] = unchecked((byte)(value >> 40));
        //	buff[6] = unchecked((byte)(value >> 48));
        //	buff[7] = unchecked((byte)(value >> 56));

        //	for (int i = 8 - 1; i >= 0; i--)
        //	{
        //		if (buff[i] > 0)
        //		{
        //			length = (byte)(i + 1);
        //			//if (length != 8)
        //			//	Array.Resize(ref buff, length);
        //			return buff;
        //		}
        //	}

        //	length = 1;
        //	return ZeroByteArray;
        //}

        private static byte[] ConvertToVarBinary(ulong value, out byte length)
        {
            var buff = SharedArray.Get();
            buff[0] = (byte)value;
            buff[1] = unchecked((byte)(value >> 8));
            buff[2] = unchecked((byte)(value >> 16));
            buff[3] = unchecked((byte)(value >> 24));
            buff[4] = unchecked((byte)(value >> 32));
            buff[5] = unchecked((byte)(value >> 40));
            buff[6] = unchecked((byte)(value >> 48));
            buff[7] = unchecked((byte)(value >> 56));

            for (int i = 8 - 1; i >= 0; i--)
            {
                if (buff[i] > 0)
                {
                    length = (byte)(i + 1);
                    return buff;
                }
            }

            length = 1;
            return ZeroByteArray;
        }

        [Obsolete("Use alternative methods")]
        private static byte[] ConvertToVarBinary(short value, out byte length)
        {
            if (value < 0)
            {
                length = 2;
                var buff = new byte[2];
                buff[0] = (byte)value;
                buff[1] = unchecked((byte)(value >> 8));
                return buff;
            }
            else
            {
                var buff = new byte[2];
                var num1 = (byte)value;
                var num2 = unchecked((byte)(value >> 8));

                buff[0] = num1;

                if (num2 > 0)
                    buff[1] = num2;
                else
                {
                    length = 1;
                    return buff;
                }

                length = 2;
                return buff;
            }
        }

        [Obsolete("Use alternative methods")]
        private static byte[] ConvertToVarBinary(ushort value, out byte length)
        {
            var buff = new byte[2];
            var num1 = (byte)value;
            var num2 = unchecked((byte)(value >> 8));

            buff[0] = num1;

            if (num2 > 0)
                buff[1] = num2;
            else
            {
                length = 1;
                return buff;
            }

            length = 2;
            return buff;
        }

        private static byte[] ConvertToVarBinary(float value, out byte length, out int position)
        {
            var bitsArray = BitConverter.GetBytes(value);
            for (int i = 0; i < 4; i++)
            {
                if (bitsArray[i] > 0)
                {
                    position = i;
                    length = (byte)(4 - position);

                    return bitsArray;
                }
            }
            length = 1;
            position = 0;
            return ZeroByteArray;
        }

        private static byte[] ConvertToVarBinary(float value, out byte length)
        {
            // Float & double numeric formats stores valuable bytes from right to left

            var valueBuff = BitConverter.GetBytes(value);
            var num1 = valueBuff[0];
            var num2 = valueBuff[1];
            var num3 = valueBuff[2];
            var num4 = valueBuff[3];

            // zero
            if (num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
            {
                length = 1;
                return ZeroByteArray;
            }

            if (num3 == 0 && num2 == 0 && num1 == 0)
            {
                length = 1;
                return new byte[] { num4 };
            }

            if (num2 == 0 && num1 == 0)
            {
                length = 2;
                return new byte[] { num3, num4 };
            }

            if (num1 == 0)
            {
                length = 3;
                return new byte[] { num2, num3, num4 };
            }

            // no zeros
            length = 4;

            return valueBuff;
        }

        private static byte[] ConvertToVarBinary(double value, out byte length, out int position)
        {
            var bitsArray = BitConverter.GetBytes(value);
            for (int i = 0; i < 8; i++)
            {
                if (bitsArray[i] > 0)
                {
                    position = i;
                    length = (byte)(8 - position);

                    return bitsArray;
                }
            }
            length = 1;
            position = 0;
            return ZeroByteArray;
        }

        [Obsolete("Use alternative methods")]
        private static byte[] ConvertToVarBinary(double value, out byte length)
        {
            // Float  &double numeric formats stores valuable bytes from right to left

            var valueBuff = BitConverter.GetBytes(value);
            var num1 = valueBuff[0];
            var num2 = valueBuff[1];
            var num3 = valueBuff[2];
            var num4 = valueBuff[3];
            var num5 = valueBuff[4];
            var num6 = valueBuff[5];
            var num7 = valueBuff[6];
            var num8 = valueBuff[7];

            // zero
            if (num8 == 0 && num7 == 0 && num6 == 0 && num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
            {
                length = 1;
                return ZeroByteArray;
            }

            if (num7 == 0 && num6 == 0 && num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
            {
                length = 1;
                return new byte[] { num8 };
            }

            if (num6 == 0 && num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
            {
                length = 2;
                return new byte[] { num7, num8 };
            }

            if (num5 == 0 && num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
            {
                length = 3;
                return new byte[] { num6, num7, num8 };
            }

            if (num4 == 0 && num3 == 0 && num2 == 0 && num1 == 0)
            {
                length = 4;
                return new byte[] { num5, num6, num7, num8 };
            }

            if (num3 == 0 && num2 == 0 && num1 == 0)
            {
                length = 5;
                return new byte[] { num4, num5, num6, num7, num8 };
            }

            if (num2 == 0 && num1 == 0)
            {
                length = 6;
                return new byte[] { num3, num4, num5, num6, num7, num8 };
            }

            if (num1 == 0)
            {
                length = 7;
                return new byte[] { num2, num3, num4, num5, num6, num7, num8 };
            }

            // no zeros
            length = 8;

            return valueBuff;
        }

        private static byte[] ConvertToVarBinary(decimal value, out byte length)
        {
            var bits = decimal.GetBits(value);
            var bitsArray = SharedArray.Get();

            for (byte i = 0; i < bits.Length; i++)
            {
                var bytes = BitConverter.GetBytes(bits[i]);
                Array.Copy(bytes, 0, bitsArray, i * 4, 4);
            }

            // finding the empty characters
            for (int i = bitsArray.Length - 1; i >= 0; i--)
            {
                if (bitsArray[i] > 0)
                {
                    length = (byte)(i + 1);

                    return bitsArray;
                }
            }
            length = 1;
            return ZeroByteArray;
        }

        #endregion


    }
}
