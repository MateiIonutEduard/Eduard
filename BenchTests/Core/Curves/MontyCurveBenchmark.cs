#if RELEASE && USE_BENCHMARKING
using BenchmarkDotNet.Attributes;
using Eduard;
using System;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Primitives;
using CoreCC = System.Security.Cryptography;
using Eduard.Security.Extensions;
#pragma warning disable

namespace BenchTests.Core.Curves
{
    public class MontyCurveBenchmark
    {
        [Params(MontyCurveType.Curve25519, MontyCurveType.Curve448)]
        public MontyCurveType curveType;

        private CoreCC.RandomNumberGenerator rand;
        private MontgomeryCurve curve;
        private ECPoint G;

        [GlobalSetup]
        public void Setup()
        {
            curve = MontgomeryCurve.GetNamedCurve(curveType);
            rand = CoreCC.RandomNumberGenerator.Create();

            BigInteger k = BigInteger.Next(rand, 1, curve.order - 1);
            var edwardsCurve = curve.ToTwistedEdwardsCurve();

            /* random point via twisted Edwards to Montgomery map */
            ECPoint edwardsPoint = edwardsCurve.GetBasePoint();
            G = edwardsCurve.ToMontgomeryPoint(edwardsPoint);
        }

        [Benchmark]
        public void BinaryScalarMultiplication()
        {
            ECPoint kG = MontyMath.Multiply(curve, curve.order, G);
        }
    }
}
#endif