#if RELEASE && USE_BENCHMARKING
using Eduard;
using System;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Primitives;
using BenchmarkDotNet.Attributes;
#pragma warning disable

namespace Eduard.BenchTests.Curves
{
    public class EdwardsCurveBenchmark
    {
        [Params(TwistedEdwardsCurveType.Edwards25519, TwistedEdwardsCurveType.Edwards448)]
        public TwistedEdwardsCurveType curveType;

        private TwistedEdwardsCurve curve;
        private ECPoint G;

        [GlobalSetup]
        public void Setup()
        {
            curve = TwistedEdwardsCurve.GetNamedCurve(curveType);
            BigInteger k = SecureRandom.Range(1, curve.order - 1);
            G = curve.GetBasePoint();
        }

        [Benchmark(Description = "Binary (affine)")]
        public void Binary_Affine()
        {
            ECPoint kG = TwistedEdwardsMath.Multiply(curve, curve.order, G);
        }

        [Benchmark(Description = "Binary (standard projective)")]
        public void Binary_Projective()
        {
            ECPoint kG = TwistedEdwardsMath.Multiply(curve, curve.order,
                G, ECMode.EC_STANDARD_PROJECTIVE);
        }

        [Benchmark(Description = "Double-base chain (Hisil)")]
        public void DoubleBaseChain()
        {
            ECPoint kG = TwistedEdwardsMath.Multiply(curve, curve.order,
                G, ECMode.EC_FASTEST);
        }
    }
}
#endif