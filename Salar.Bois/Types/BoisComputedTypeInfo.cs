using Salar.BinaryBuffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Salar.Bois.Types
{
	delegate void SerializeDelegate<T, TWriter>(ref TWriter writer, T instance, Encoding encoding)
		where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
		, allows ref struct
#endif
		;

	delegate T DeserializeDelegate<T, TReader>(ref TReader reader, Encoding encoding)
		where TReader : IBufferReader
#if NET9_0_OR_GREATER
		, allows ref struct
#endif
		;


	class BoisComputedTypeInfo
	{
		internal Delegate WriterDelegate;

		internal Delegate ReaderDelegate;

		internal MethodInfo WriterMethod;

		internal MethodInfo ReaderMethod;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void InvokeWriter<T, TWriter>(ref TWriter writer, T instance, Encoding encoding)
			where TWriter : IBufferWriter
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
		{
			((SerializeDelegate<T, TWriter>)WriterDelegate).Invoke(ref writer, instance, encoding);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal T InvokeReader<T, TReader>(ref TReader reader, Encoding encoding)
			where TReader : IBufferReader
#if NET9_0_OR_GREATER
			, allows ref struct
#endif
		{
			return ((DeserializeDelegate<T, TReader>)ReaderDelegate).Invoke(ref reader, encoding);
		}
	}
}
