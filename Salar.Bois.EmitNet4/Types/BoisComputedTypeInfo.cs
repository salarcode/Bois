using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Salar.Bois.Types
{
	internal delegate void SerializeDelegate<T>(BinaryWriter writer, T instance, Encoding encoding);

	internal delegate T DeserializeDelegate<T>(BinaryReader reader, Encoding encoding);


	class BoisComputedTypeInfo
	{
		internal Delegate WriterDelegate { get; set; }

		internal Delegate ReaderDelegate { get; set; }

		internal void InvokeWriter<T>(BinaryWriter writer, T instance, Encoding encoding)
		{
			((SerializeDelegate<T>)WriterDelegate).Invoke(writer, instance, encoding);
		}

		internal T InvokeReader<T>(BinaryReader reader, Encoding encoding)
		{
			return ((DeserializeDelegate<T>)ReaderDelegate).Invoke(reader, encoding);
		}
	}
}
