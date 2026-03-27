#if RELEASE && USE_BENCHMARKING
using Eduard;
using System;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Primitives;
using BenchmarkDotNet.Attributes;
#pragma warning disable

namespace BenchTests.Core.Curves
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

        [Benchmark]
        public void BinaryScalarMultiplication()
        {
            ECPoint kG = ECMath.Multiply(curve, curve.order, G);
        }

        [Benchmark]
        public void MixedBinaryScalarMultiplication()
        {
            ECPoint kG = ECMath.Multiply(curve, curve.order, 
                G, ECMode.EC_STANDARD_PROJECTIVE);
        }

        [Benchmark]
        public void MontgomeryLadderMultiplication()
        {
            ECPoint kG = ECMath.Multiply(curve, curve.order,
                G, ECMode.EC_SECURE);
        }

        [Benchmark]
        public void FastestScalarMultiplication()
        {
            ECPoint kG = ECMath.Multiply(curve, curve.order,
                G, ECMode.EC_FASTEST);
        }
    }
}
#endif
