#if RELEASE && USE_BENCHMARKING
using BenchmarkDotNet.Attributes;
using System;
using Eduard;
using CoreCC = System.Security.Cryptography;
#pragma warning disable

namespace BenchmarkTests.Core.BigInt
{
    public class ExpoBenchmark
    {
        [Params(128, 160, 192, 256, 320, 384, 512, 768, 1024)]
        public int bits;

        private CoreCC.RandomNumberGenerator rand;
        private BigInteger val, field;

        [GlobalSetup]
        public void Setup()
        {
            rand = CoreCC.RandomNumberGenerator.Create();
            field = BigInteger.GenProbablePrime(rand, bits, 50);
            val = BigInteger.Next(rand, 1, field - 1);
        }

        [Benchmark]
        public void BinaryExponentiation()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_WORDS_THRESHOLD, bits >> 4);
            BigInteger res = BigInteger.Pow(val, (field - 1) >> 1, field);
        }

        [Benchmark]
        public void SlidingWindowExponentiation()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_WORDS_THRESHOLD, bits >> 5);
            BigInteger res = BigInteger.Pow(val, (field - 1) >> 1, field);
        }
    }
}

#endif