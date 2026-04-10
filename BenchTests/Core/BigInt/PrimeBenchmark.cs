#if RELEASE && USE_BENCHMARKING
using System;
using Eduard;
using BenchmarkDotNet.Attributes;
#pragma warning disable

namespace BenchTests.Core.BigInt
{
    public class PrimeBenchmark
    {
        [Params(256, 320, 384, 512, 768, 1024)]
        public int bits;

        [Benchmark]
        public void GenProbablePrime()
        {
            BigInteger field = SecureRandom.GenProbablePrime(bits);
        }
    }
}

#endif