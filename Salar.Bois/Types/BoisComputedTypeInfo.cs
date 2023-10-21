﻿using Salar.BinaryBuffers;
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
	delegate void SerializeDelegate<T>(BufferWriterBase writer, T instance, Encoding encoding);

	delegate T DeserializeDelegate<T>(BinaryBufferReader reader, Encoding encoding);


	class BoisComputedTypeInfo
	{
		internal Delegate WriterDelegate;

		internal Delegate ReaderDelegate;

		internal MethodInfo WriterMethod;

		internal MethodInfo ReaderMethod;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void InvokeWriter<T>(BufferWriterBase writer, T instance, Encoding encoding)
		{
			((SerializeDelegate<T>)WriterDelegate).Invoke(writer, instance, encoding);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal T InvokeReader<T>(BinaryBufferReader reader, Encoding encoding)
		{
			return ((DeserializeDelegate<T>)ReaderDelegate).Invoke(reader, encoding);
		}
	}
}
