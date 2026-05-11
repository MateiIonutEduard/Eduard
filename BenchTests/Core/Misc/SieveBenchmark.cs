#if RELEASE && USE_BENCHMARKING

using BenchmarkDotNet.Attributes;

namespace Eduard.BenchTests.Misc
{
    public class SieveBenchmark
    {
        [Params(100, 1000, 10000, 100000, 1000000, 10000000, 100000000)]
        public int limit;

        [Benchmark(Description = "Standard Atkin sieve (managed)")]
        public void StandardAtkinSieving()
        {
            int[] primes = Sieve.GenPrimeListStandard(limit);
        }

        [Benchmark(Description = "Optimized Atkin sieve (managed)")]
        public void OptimizedAtkinSieving()
        {
            int[] primes = Sieve.GenPrimeListOptimized(limit);
        }
    }
}

#endif