using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Salar.Bois.Types
{
	delegate void SerializeDelegate<T>(BinaryWriter writer, T instance, Encoding encoding);
	delegate T DeserializeDelegate<T>(BinaryReader reader, Encoding encoding);


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

		//internal void InvokeWriter(BinaryWriter writer, object instance, Encoding encoding)
		//{
		//	WriterDelegate.DynamicInvoke(new object[] {writer, instance, encoding});

		//	WriterDelegate.Method.Invoke(null, BindingFlags.Default, Type.DefaultBinder, new object[] { writer, instance, encoding }, CultureInfo.CurrentCulture);

		//	SerializerMethod.Invoke(null, BindingFlags.Default,Type.DefaultBinder, new object[] {writer, instance, encoding},CultureInfo.CurrentCulture);
		//}

		//internal void InvokeWriterBenchmark<T>(BinaryWriter writer, T instance, Encoding encoding)
		//{
		//	var sw = new Stopwatch();

		//	sw.Start();
		//	for (int i = 0; i < 1000; i++)
		//	{
		//		((SerializeDelegate<T>)WriterDelegate).Invoke(writer, instance, encoding);
		//	}
		//	var castCall = sw.Elapsed;

		//	sw.Start();
		//	for (int i = 0; i < 1000; i++)
		//	{
		//		SerializerMethod.Invoke(null, BindingFlags.Default, Type.DefaultBinder, new object[] { writer, instance, encoding }, CultureInfo.CurrentCulture);
		//	}
		//	var dynCall = sw.Elapsed;

		//}


	}
}
