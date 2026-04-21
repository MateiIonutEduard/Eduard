#if RELEASE && USE_BENCHMARKING
using Eduard;
using Eduard.Security;
using BenchmarkDotNet.Attributes;
#pragma warning disable

namespace Eduard.BenchTests.Poly
{
    public class ComposeModBenchmark
    {
        [Params(160, 192, 256, 384, 512)]
        public int bitSize;

        [Params(64, 72, 80, 88, 96, 128)]
        public int degree;

        private Polynomial mod;
        private BigInteger field;
        private Polynomial XP;

        [GlobalSetup]
        public void Setup()
        {
            field = SecureRandom.GenProbablePrime(bitSize);
            Polynomial.SetField(field);

            mod = new Polynomial(degree);
            Polynomial X = new Polynomial(1, 0);

            for (int i = 0; i <= degree; i++)
                mod.coeffs[i] = SecureRandom.Range(1, field - 1);

            XP = Polynomial.Pow(X, field, mod);
        }

        [Benchmark(Description = "Horner (standard)")]
        public void Horner_Standard()
        {
            PerfTuner.SetThreshold(PerfEntry.POLY_DEGREE_FAST_HORNER, degree << 1);
            Polynomial XPP = Polynomial.Compose(XP, XP, mod, false);
        }

        [Benchmark(Description = "Horner (FFT-accelerated)")]
        public void Horner_FFT()
        {
            PerfTuner.SetThreshold(PerfEntry.POLY_DEGREE_FAST_HORNER, degree);
            Polynomial XPP = Polynomial.Compose(XP, XP, mod, false);
        }
    }
}
#endif