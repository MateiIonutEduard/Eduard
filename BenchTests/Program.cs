#if RELEASE && USE_BENCHMARKING
using BenchmarkDotNet.Running;
using BenchmarkTests.BenchCore.BigInt;
#else
using System;
#endif

namespace BenchTests
{
    public class Program
    {
        static void Main(string[] args)
        {
#if RELEASE && USE_BENCHMARKING
            BenchmarkRunner.Run<MultBenchmark>();
#endif
        }
    }
}
