#if RELEASE && USE_BENCHMARKING
using BenchmarkDotNet.Attributes;
using System;
using Eduard;
using CoreCC = System.Security.Cryptography;
#pragma warning disable

namespace BenchmarkTests.Core.BigInt
{
    public class ReduceBenchmark
    {
        [Params(128, 160, 192, 256, 320, 384, 512, 768, 1024)]
        public int bits;

        private CoreCC.RandomNumberGenerator rand;
        private BigInteger val, square, field;
        private BigInteger constant;

        [GlobalSetup]
        public void Setup()
        {
            rand = CoreCC.RandomNumberGenerator.Create();
            field = BigInteger.GenProbablePrime(rand, bits, 50);

            constant = BigInteger.BarrettConstant(field);
            val = BigInteger.Next(rand, 1, field - 1);
            square = val * val;
        }

        [Benchmark]
        public void BarrettReduction()
        {
            BigInteger res = BigInteger.BarrettReduction(square, field, constant);
        }

        [Benchmark]
        public void StandardReduction()
        {
            BigInteger res = square % field;
        }
    }
}

#endif