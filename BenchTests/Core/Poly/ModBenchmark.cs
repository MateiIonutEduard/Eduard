#if RELEASE && USE_BENCHMARKING
using Eduard;
using Eduard.Security;
using BenchmarkDotNet.Attributes;
#pragma warning disable

namespace Eduard.BenchTests.Poly
{
    public class ModBenchmark
    {
        [Params(160, 192, 256, 384, 512)]
        public int bitSize;

        [Params(16, 32, 64, 96, 128, 256)]
        public int degree;

        private Polynomial left, mod;
        private BigInteger field;

        [GlobalSetup]
        public void Setup()
        {
            field = SecureRandom.GenProbablePrime(bitSize);
            Polynomial.SetField(field);
            mod = new Polynomial(degree);

            for (int i = 0; i <= degree; i++)
                mod.coeffs[i] = SecureRandom.Range(1, field - 1);

            int maxDegree = 2 * (degree - 1);
            left = new Polynomial(maxDegree);

            for (int i = 0; i <= left.degree; i++)
                left.coeffs[i] = SecureRandom.Range(1, field - 1);

            PerfTuner.SetThreshold(PerfEntry.POLY_FFT_MOD, degree);
            Polynomial.SetPolyMod(mod);
        }

        [Benchmark(Description = "Standard reduction")]
        public void Standard()
        {
            PerfTuner.SetThreshold(PerfEntry.POLY_FFT_MOD, degree << 1);
            Polynomial rem = Polynomial.Reduce(left, mod);
        }

        [Benchmark(Description = "FFT-based reduction")]
        public void FFT()
        {
            PerfTuner.SetThreshold(PerfEntry.POLY_FFT_MOD, degree);
            Polynomial remainder = Polynomial.Reduce(left, mod);
        }
    }
}
#endif
