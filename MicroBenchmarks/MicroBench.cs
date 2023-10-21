using BenchmarkDotNet.Attributes;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MicroBenchmarks;

public class MicroBench
{
	private static byte[] ZeroByteArray = new byte[] { 0 };



	private static byte[] ConvertToVarBinary_1(float value, out byte length, out int position)
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

	[Benchmark]
	public int Bench_ConvertToVarBinary_1()
	{
		byte[] arr = null;
		for (int i = 0; i < 1000; i++)
		{
			arr = ConvertToVarBinary_1(10050.99f, out var length, out var position);
		}
		return arr.Length;
	}

	private static byte[] ConvertToVarBinary_2(float value, out byte length, out int position)
	{
		const byte arrayLength = 4;
		var bitsArray = new byte[arrayLength];

		Unsafe.As<byte, float>(ref bitsArray[0]) = value;

		for (int i = 0; i < arrayLength; i++)
		{
			if (bitsArray[i] > 0)
			{
				position = i;
				length = (byte)(arrayLength - position);

				return bitsArray;
			}
		}
		length = 1;
		position = 0;
		return ZeroByteArray;
	}

	[Benchmark]
	public int Bench_ConvertToVarBinary_2()
	{
		byte[] arr = null;
		for (int i = 0; i < 1000; i++)
		{
			arr = ConvertToVarBinary_2(10050.99f, out var length, out var position);
		}
		return arr.Length;
	}

	private static byte[] ConvertToVarBinary_3(float value, out byte length, out int position)
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

	//[Benchmark]
	public int Bench_ConvertToVarBinary_3()
	{
		byte[] arr = null;
		for (int i = 0; i < 1000; i++)
		{
			arr = ConvertToVarBinary_3(10050.99f, out var length, out var position);
		}
		return arr.Length;
	}



}

static class SharedArray
{
	[ThreadStatic] private static byte[] _array;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte[] Get()
	{
		return _array ??= new byte[16];
	}

	public static void ClearArray4()
	{
		_array[0] = 0;
		_array[1] = 0;
		_array[2] = 0;
		_array[3] = 0;
	}
	public static void ClearArray8()
	{
		_array[0] = 0;
		_array[1] = 0;
		_array[2] = 0;
		_array[3] = 0;
		_array[4] = 0;
		_array[5] = 0;
		_array[6] = 0;
		_array[7] = 0;
	}
}
