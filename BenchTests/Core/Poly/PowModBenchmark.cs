#if RELEASE && USE_BENCHMARKING
using Eduard;
using Eduard.Security;
using BenchmarkDotNet.Attributes;
using CoreCC = System.Security.Cryptography;
#pragma warning disable

namespace BenchmarkTests.Core.Poly
{
    public class PowModBenchmark
    {
        [Params(160, 192, 256, 384, 512)]
        public int bitSize;

        [Params(16, 32, 64, 96, 128, 256, 512)]
        public int degree;

        private CoreCC.RandomNumberGenerator rand;
        private Polynomial mod;
        private BigInteger field;

        [GlobalSetup]
        public void Setup()
        {
            rand = CoreCC.RandomNumberGenerator.Create();
            field = BigInteger.GenProbablePrime(rand, bitSize, 50);

            Polynomial.SetField(field);
            mod = new Polynomial(degree);

            for (int i = 0; i <= degree; i++)
                mod.coeffs[i] = BigInteger.Next(rand, 1, field - 1);
        }

        [Benchmark]
        public void PolyPowMod()
        {
            Polynomial X = new Polynomial(1, 0);
            Polynomial XP = Polynomial.Pow(X, field, mod);
        }
    }
}
#endif