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
		if (choice.KeyChar == '0' || choice.KeyChar == '1')
		{
			var runCount = PromptRunCount();
			var runner = new BenchRunner(runCount);
			if (choice.KeyChar == '0')
				runner.RunAll();
			else
				runner.RunSwitcher();
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
		Console.WriteLine("0 - Run all benchmarks");
		Console.WriteLine("1 - Select what to run");
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

	static int PromptRunCount()
	{
		const int defaultRunCount = 10_000;
		Console.Write($"Number of runs (default {defaultRunCount:N0}): ");
		var input = Console.ReadLine();
		if (string.IsNullOrWhiteSpace(input))
			return defaultRunCount;
		if (int.TryParse(input, out var count) && count > 0)
			return count;
		Console.WriteLine($"Invalid number, using default ({defaultRunCount:N0}).");
		return defaultRunCount;
	}

	[Conditional("DEBUG")]
	private static void Debug()
	{
		// Code to debug, uncomment
#if NET
		var bench = new ProtobufNetBenchmark<Test1_Arrays_Big>();
		bench.GlobalSetup();
		bench.Serialize();
		bench.Deserialize();
#endif
	}
}