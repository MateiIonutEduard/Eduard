#if RELEASE && USE_BENCHMARKING
using BenchmarkDotNet.Attributes;

namespace Eduard.BenchTests.BigInt
{
    public class ReduceBenchmark
    {
        [Params(128, 160, 192, 256, 320, 384, 512, 768, 1024)]
        public int bits;

        private BigInteger val, square, field;
        private BigInteger constant;

        [GlobalSetup]
        public void Setup()
        {
            field = SecureRandom.GenProbablePrime(bits);
            constant = BigInteger.BarrettConstant(field);
            val = SecureRandom.Range(1, field - 1);
            square = val * val;
        }

        [Benchmark(Description = "Barrett reduction")]
        public void Barrett()
        {
            BigInteger res = BigInteger.BarrettReduction(square, field, constant);
        }

        [Benchmark(Description = "Division remainder")]
        public void Division()
        {
            BigInteger res = square % field;
        }
    }
}

#endif