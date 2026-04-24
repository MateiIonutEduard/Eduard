#if RELEASE && USE_BENCHMARKING
using System;
using Eduard;
using BenchmarkDotNet.Attributes;
#pragma warning disable

namespace Eduard.BenchTests.BigInt
{
    public class PrimeBenchmark
    {
        [Params(256, 320, 384, 512, 768, 1024)]
        public int bits;

        [Benchmark(Description = "Miller-Rabin (50 rounds)")]
        public void ProbablePrime()
        {
            BigInteger field = SecureRandom.GenProbablePrime(bits);
        }
    }
}

#endif