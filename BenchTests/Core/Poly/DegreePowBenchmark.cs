#if RELEASE && USE_BENCHMARKING
using Eduard.Security;
using BenchmarkDotNet.Attributes;

namespace Eduard.BenchTests.Poly
{
    public class DegreePowBenchmark
    {
        [Params(160, 192, 256, 384, 512)]
        public int bitSize;

        [Params(16, 32, 64, 96, 128)]
        public int degree;

        private Polynomial mod;
        private BigInteger field;

        [GlobalSetup]
        public void Setup()
        {
            field = SecureRandom.GenProbablePrime(bitSize);
            Polynomial.SetField(field);
            mod = new Polynomial(degree);

            for (int i = 0; i <= degree; i++)
                mod.coeffs[i] = SecureRandom.Range(1, field - 1);
        }

        [Benchmark(Description = "Binary exponentiation")]
        public void BinaryPolyPowMod()
        {
            PerfTuner.SetThreshold(PerfEntry.POLY_DEGREE_POW_MOD, degree << 1);
            Polynomial X = new Polynomial(1, 0);
            Polynomial XP = Polynomial.Pow(X, field, mod);
        }

        [Benchmark(Description = "Sliding window")]
        public void ImprovedPolyPowMod()
        {
            PerfTuner.SetThreshold(PerfEntry.POLY_DEGREE_POW_MOD, degree);
            Polynomial X = new Polynomial(1, 0);
            Polynomial XP = Polynomial.Pow(X, field, mod);
        }
    }
}
#endif