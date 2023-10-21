using System;
using System.Diagnostics;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using Salar.Bois.BenchBois;
using Salar.Bois.BenchmarksBase;
using Salar.Bois.BenchmarksObjects.TestObjects;
using Salar.Bois.BenchProtobufNet;

namespace Salar.Bois.BenchmarksRun;

internal class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Welcome to serializers benchmark!");
#if DEBUG
		Console.WriteLine("**********************************************");		
		Console.WriteLine("You are in DEBUG mode please use Release mode.");		
		Console.WriteLine("**********************************************");		
#endif
		var choice = Menu();
		if (choice.KeyChar == '0')
		{
			new BenchRunner().RunAll();
		}
		else if (choice.KeyChar == '1')
		{
			new BenchRunner().RunSwitcher();
		}
		else if (choice.Key == ConsoleKey.Q)
		{
			return;
		}
		else if (choice.Key == ConsoleKey.D)
		{
			Debug();
		}
		else
		{
			Console.WriteLine("Invalid choice!");
		}
	}

	static ConsoleKeyInfo Menu()
	{
		Console.WriteLine("0 - Run all benchmarks 10_000 times");
		Console.WriteLine("1 - Select what to run 10_000 times");
#if DEBUG
		Console.WriteLine("d - Debug");
#endif
		Console.WriteLine("q - Quit");
		Console.Write("Please enter your choice: ");
		try
		{
			return Console.ReadKey();
		}
		finally
		{
			Console.WriteLine();
		}
	}

	[Conditional("DEBUG")]
	private static void Debug()
	{
		// Code to debug, uncomment
#if NET
		var bench = new BoisBufferBenchmark<Test1_Arrays_Big>();
		bench.GlobalSetup();
		bench.Serialize();
		bench.Deserialize();
#endif
	}
}