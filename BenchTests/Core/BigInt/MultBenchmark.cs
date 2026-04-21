#if RELEASE && USE_BENCHMARKING
using Eduard;
using System;
using BenchmarkDotNet.Attributes;
#pragma warning disable

namespace Eduard.BenchTests.BigInt
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

        [BenchmarkCategory("Multiplication")]
        [Benchmark(Description = "Schoolbook O(n^2)")]
        public void Multiply_Schoolbook()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words << 8);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_MULTIPLY, words << 4);
            BigInteger res = left * right;
        }

        [BenchmarkCategory("Multiplication")]
        [Benchmark(Description = "Karatsuba O(n^1.585)")]
        public void Multiply_Karatsuba()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words << 2);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_MULTIPLY, words);
            BigInteger res = left * right;
        }

        [BenchmarkCategory("Multiplication")]
        [Benchmark(Description = "NTT (FFT-based)")]
        public void Multiply_NTT()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_MULTIPLY, words >> 2);
            BigInteger res = left * right;
        }

        [BenchmarkCategory("Squaring")]
        [Benchmark(Description = "Schoolbook O(n^2)")]
        public void Square_Schoolbook()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words << 8);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_SQUARING, words << 4);
            BigInteger res = left * left;
        }

        [BenchmarkCategory("Squaring")]
        [Benchmark(Description = "Karatsuba O(n^1.585)")]
        public void Square_Karatsuba()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words << 2);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_SQUARING, words);
            BigInteger res = left * left;
        }

        [BenchmarkCategory("Squaring")]
        [Benchmark(Description = "NTT (FFT-based)")]
        public void Square_NTT()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_FFT, words);
            PerfTuner.SetThreshold(PerfEntry.BIGINT_KARATSUBA_SQUARING, words >> 2);
            BigInteger res = left * left;
        }
    }
}

#endif