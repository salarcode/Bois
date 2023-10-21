using Salar.Bois;
using Salar.Bois.BenchBois;
using Salar.Bois.BenchmarksObjects.TestObjects;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ConsoleGen;

internal class Program
{
	static int Iterations = 1_000_000;
	static void Main(string[] args)
	{
		RunBench();
	}

	private static void RunBench()
	{
		var sw = Stopwatch.StartNew();

		Console.WriteLine("Running RunBench_Bois...");
		//RunBench_Bois();

		sw.Stop();
		Console.WriteLine($"Finished RunBench_Bois, elapsed: {sw.Elapsed}");
	}
	
	//[MethodImpl(MethodImplOptions.NoInlining)]
	//static int RunBench_Bois()
	//{
	//	var bench = new BoisBenchmark<Test1_Arrays_Small>();
	//	bench.IterationCount = Iterations;
	//	bench.GlobalSetup();
	//	bench.Serialize();

	//	return bench.IterationCount;
	//}

	//static void Test()
	//{
	//	Console.WriteLine("Hello, World!");
	//	var bois = new BoisSerializer();
	//	var buff = new byte[1020];
	//	var mem = new MemStream();

	//	var aa = GetBinaryBufferReader(mem);

	//	Console.WriteLine("Hello " + aa.Length);


	//	var memStream = new MemoryStream(buff, 10, 50);

	//	memStream.Position = 0;
	//	bois.Serialize(mem, memStream);
	//	memStream.Position = 0;
	//	var memCopy = bois.Deserialize<MemStream>(memStream);


	//	//var d = MemoryStreamReader.ComputeDelegate();
	//	//var reader = d.Invoke(memStream);

	//}

	private static BinaryBufferReader GetBinaryBufferReader(MemStream mem)
	{
		return new BinaryBufferReader(mem._buffer, mem._origin, mem._length - mem._origin);
	}
}

internal class MemStream
{
	internal byte[] _buffer;    // Either allocated internally or externally.
	internal readonly int _origin;       // For user-provided arrays, start at this origin
	internal int _position;     // read/write head.
	internal int _length;       // Number of bytes within the memory stream

	public byte[] Data { get; set; }
	public int Offset { get; set; }
	public int Length { get; set; }
}



public sealed class BinaryBufferReader
{
	public BinaryBufferReader(byte[] data, int offset, int length)
	{
		Data = data;
		Offset = offset;
		Length = length;
	}

	public byte[] Data { get; }
	public int Offset { get; }
	public int Length { get; }
}