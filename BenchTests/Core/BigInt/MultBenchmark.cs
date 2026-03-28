#if RELEASE && USE_BENCHMARKING
using Eduard;
using System;
using BenchmarkDotNet.Attributes;
#pragma warning disable

namespace BenchmarkTests.Core.BigInt
{
    public class MultBenchmark
    {
        [Params(8, 16, 32, 64, 128, 256, 512, 1024, 1152, 1280, 1408, 1536, 1664, 1792, 1920, 2048)]
        public int words;

        private BigInteger left, field;
        private BigInteger right;

        [GlobalSetup]
        public void Setup()
        {
            int bits = words << 5;
            left = SecureRandom.GenRandom(bits);
            right = SecureRandom.GenRandom(bits);
        }

        [Benchmark]
        public void StandardBigIntMultiplication()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words << 8);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_MULTIPLY, words << 4);
            BigInteger res = left * right;
        }

        [Benchmark]
        public void StandardBigIntSquaring()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words << 8);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_SQUARING, words << 4);
            BigInteger res = left * left;
        }

        [Benchmark]
        public void KaratsubaBigIntMultiplication()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words << 2);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_MULTIPLY, words);
            BigInteger res = left * right;
        }

        [Benchmark]
        public void KaratsubaBigIntSquaring()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words << 2);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_SQUARING, words);
            BigInteger res = left * left;
        }

        [Benchmark]
        public void NTTBigIntMultiplication()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_MULTIPLY, words >> 2);
            BigInteger res = left * right;
        }

        [Benchmark]
        public void NTTBigIntSquaring()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_SQUARING, words >> 2);
            BigInteger res = left * left;
        }
    }
}

#endif