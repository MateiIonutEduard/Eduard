#if RELEASE && USE_BENCHMARKING
using BenchmarkDotNet.Attributes;

namespace Eduard.BenchTests.BigInt
{
    public class ExpoBenchmark
    {
        [Params(128, 160, 192, 256, 320, 384, 512, 768, 1024)]
        public int bits;

        private BigInteger val;
        private BigInteger field;

        [GlobalSetup]
        public void Setup()
        {
            field = SecureRandom.GenProbablePrime(bits);
            val = SecureRandom.Range(1, field - 1);
        }

        [Benchmark(Description = "Binary (square-and-multiply)")]
        public void Binary()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_WORDS_THRESHOLD, bits >> 4);
            BigInteger res = BigInteger.Pow(val, (field - 1) >> 1, field);
        }

        [Benchmark(Description = "Sliding window")]
        public void SlidingWindow()
        {
            PerfTuner.SetThreshold(PerfEntry.BIGINT_WORDS_THRESHOLD, bits >> 5);
            BigInteger res = BigInteger.Pow(val, (field - 1) >> 1, field);
        }
    }
}

#endif