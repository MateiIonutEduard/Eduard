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
    public class WeiCurveBenchmark
    {
        [Params(WeiCurveType.NistP192, WeiCurveType.NistP224, WeiCurveType.NistP256, WeiCurveType.NistP384, WeiCurveType.NistP521, WeiCurveType.Wei25519, WeiCurveType.Wei448)]
        public WeiCurveType curveType;

        private EllipticCurve curve;
        private ECPoint G;

        [GlobalSetup]
        public void Setup()
        {
            curve = EllipticCurve.GetNamedCurve(curveType);
            BigInteger k = SecureRandom.Range(1, curve.order - 1);
            G = curve.GetBasePoint();
        }

        [Benchmark(Description = "Binary (affine)")]
        public void Binary_Affine()
        {
            ECPoint kG = ECMath.Multiply(curve, curve.order, G);
        }

        [Benchmark(Description = "Binary (projective)")]
        public void Binary_Projective()
        {
            ECPoint kG = ECMath.Multiply(curve, curve.order, 
                G, ECMode.EC_STANDARD_PROJECTIVE);
        }

        [Benchmark(Description = "Montgomery ladder")]
        public void MontgomeryLadder()
        {
            ECPoint kG = ECMath.Multiply(curve, curve.order,
                G, ECMode.EC_SECURE);
        }

        [Benchmark(Description = "wNAF sliding window")]
        public void SlidingWindow()
        {
            ECPoint kG = ECMath.Multiply(curve, curve.order,
                G, ECMode.EC_FASTEST);
        }
    }
}
#endif
