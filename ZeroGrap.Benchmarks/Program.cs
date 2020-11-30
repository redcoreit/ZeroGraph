#pragma warning disable CA1707 

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ZeroGraph.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
