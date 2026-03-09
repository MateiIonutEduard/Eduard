#if RELEASE && USE_BENCHMARKING
using BenchmarkDotNet.Attributes;
using System;
using Eduard;
using CoreCC = System.Security.Cryptography;
#pragma warning disable

namespace BenchmarkTests.Core.BigInt
{
    public class PrimeBenchmark
    {
        [Params(256, 320, 384, 512, 768, 1024)]
        public int bits;
        private CoreCC.RandomNumberGenerator rand;

        [GlobalSetup]
        public void Setup()
        {
            rand = CoreCC.RandomNumberGenerator.Create();
        }

        [Benchmark]
        public void GenProbablePrime()
        {
            BigInteger field = BigInteger.GenProbablePrime(rand, bits, 50);
        }
    }
}

#endif