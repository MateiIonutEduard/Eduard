#if RELEASE && USE_BENCHMARKING
using Eduard.Security;
using BenchmarkDotNet.Attributes;

namespace Eduard.BenchTests.Poly
{
    public class PowModBenchmark
    {
        [Params(160, 192, 256, 384, 512)]
        public int bitSize;

        [Params(16, 32, 64, 96, 128, 256, 512)]
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

        [Benchmark(Description = "X^p mod f(X)")]
        public void PolyPowMod()
        {
            Polynomial X = new Polynomial(1, 0);
            Polynomial XP = Polynomial.Pow(X, field, mod);
        }
    }
}
#endif