#if RELEASE && USE_BENCHMARKING
using Eduard;
using Eduard.Security;
using BenchmarkDotNet.Attributes;
using CoreCC = System.Security.Cryptography;
#pragma warning disable

namespace BenchmarkTests.Core.Poly
{
    public class MultiBenchmark
    {
        [Params(160, 192, 256, 384, 512)]
        public int bitSize;

        [Params(16, 32, 64, 96, 128, 256, 512)]
        public int degree;

        private CoreCC.RandomNumberGenerator rand;
        private Polynomial left, right;
        private BigInteger field;

        [GlobalSetup]
        public void Setup()
        {
            rand = CoreCC.RandomNumberGenerator.Create();
            field = BigInteger.GenProbablePrime(rand, bitSize, 50);

            Polynomial.SetField(field);
            left = new Polynomial(degree);
            right = new Polynomial(degree);
            int i;

            for (i = 0; i <= degree - 3; i++)
                left.coeffs[i] = BigInteger.Next(rand, 1, field - 1);

            for (i = 0; i <= degree - 1; i++)
                right.coeffs[i] = BigInteger.Next(rand, 1, field - 1);
        }

        [Benchmark]
        public void PlainPolyMultiply()
        {
            PerfTuner.SetThreshold(PerfEntry.POLY_FFT_MULT, degree << 1);
            Polynomial res = left * right;
        }

        [Benchmark]
        public void FFTPolyMultiply()
        {
            PerfTuner.SetThreshold(PerfEntry.POLY_FFT_MULT, degree);
            Polynomial res = left * right;
        }

        [Benchmark]
        public void PlainPolySquaring()
        {
            PerfTuner.SetThreshold(PerfEntry.POLY_FFT_MULT, degree << 1);
            Polynomial res = right * right;
        }

        [Benchmark]
        public void FFTPolySquaring()
        {
            PerfTuner.SetThreshold(PerfEntry.POLY_FFT_MULT, degree);
            Polynomial res = right * right;
        }
    }
}
#endif