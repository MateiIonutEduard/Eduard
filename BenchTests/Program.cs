#if RELEASE && USE_BENCHMARKING
using BenchmarkDotNet.Running;
using BenchTests.Core.Curves;
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
            BenchmarkRunner.Run<EdCurveBenchmark>();
#endif
        }
    }
}
