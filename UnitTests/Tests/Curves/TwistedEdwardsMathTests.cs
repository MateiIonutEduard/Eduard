using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
    [Collection("Sequential")]
    public class TwistedEdwardsMathTests
    {
        #region Point Negation Tests — All Coordinate Systems
        [Fact]
        public void Negate_Affine()
        {
            TwistedEdwardsCurveType curveType = TwistedEdwardsCurveType.Edwards25519;
            var curve = TwistedEdwardsCurve.GetNamedCurve(curveType);
            var p = curve.field;

            /* base point */
            var P = curve.GetBasePoint();
            var Q = TwistedEdwardsMath.Negate(curve, P);

            Assert.Equal(P.GetAffineY(), Q.GetAffineY());
            Assert.Equal((p - P.GetAffineX()) % p, Q.GetAffineX());

            /* P + (-P) = point at infinity */
            Assert.Equal(ECPoint.POINT_INFINITY,
                TwistedEdwardsMath.Add(curve, P, Q));

            /* double negation */
            Assert.Equal(P, TwistedEdwardsMath.Negate(curve, Q));

            /* point at infinity */
            Assert.Equal(ECPoint.POINT_INFINITY,
                TwistedEdwardsMath.Negate(curve, ECPoint.POINT_INFINITY));
        }

        [Fact]
        public void Negate_Projective()
        {
            TwistedEdwardsCurveType curveType = TwistedEdwardsCurveType.Edwards25519;
            var curve = TwistedEdwardsCurve.GetNamedCurve(curveType);
            var p = curve.field;

            /* base point */
            var G = curve.GetBasePoint();
            var P = curve.ToProjective(G);
            var negP = Ed3Math.Negate(curve, P);

            /* homogenous projective coordinate checks */
            Assert.Equal(P.y, negP.y);
            var expectedNegX = p - P.x;

            Assert.Equal(expectedNegX, negP.x);
            Assert.Equal(P.z, negP.z);

            /* affine consistency */
            var affineNegP = curve.ToAffine(negP);
            Assert.Equal(G.GetAffineY(), affineNegP.GetAffineY());
            Assert.Equal(p - G.GetAffineX(), affineNegP.GetAffineX());

            /* P + (-P) = point at infinity */
            var sum = curve.ToAffine(Ed3Math.UnifiedAdd(curve, P, negP));
            Assert.Equal(ECPoint.POINT_INFINITY, sum);

            /* double negation (involution) */
            var negNegP = Ed3Math.Negate(curve, negP);
            Assert.Equal(P, negNegP);

            /* point at infinity */
            var inf = ECPoint3.POINT_INFINITY;
            Assert.Equal(inf, Ed3Math.Negate(curve, inf));
        }

        [Fact]
        public void Negate_Extended_Projective()
        {
            TwistedEdwardsCurveType curveType = TwistedEdwardsCurveType.Edwards25519;
            var curve = TwistedEdwardsCurve.GetNamedCurve(curveType);
            var p = curve.field;

            /* base point */
            var G = curve.GetBasePoint();
            var P = curve.ToExtendedProjective(G);
            var negP = Ed4Math.Negate(curve, P);

            /* extended projective coordinate checks */
            Assert.Equal(P.y, negP.y);
            var expectedNegX = p - P.x;

            Assert.Equal(expectedNegX, negP.x);
            BigInteger expectedNegT = p - P.t;

            Assert.Equal(expectedNegT, negP.t);
            Assert.Equal(P.z, negP.z);

            /* affine consistency */
            var affineNegP = curve.ToAffine(negP);
            Assert.Equal(G.GetAffineY(), affineNegP.GetAffineY());
            Assert.Equal(p - G.GetAffineX(), affineNegP.GetAffineX());

            /* P + (-P) = point at infinity */
            var sum = curve.ToAffine(Ed4Math.UnifiedAdd(curve, P, negP));
            Assert.Equal(ECPoint.POINT_INFINITY, sum);

            /* double negation (involution) */
            var negNegP = Ed4Math.Negate(curve, negP);
            Assert.Equal(P, negNegP);

            /* point at infinity */
            var inf = ECPoint4.POINT_INFINITY;
            Assert.Equal(inf, Ed4Math.Negate(curve, inf));
        }
        #endregion

        #region Point Doubling — All Coordinate Systems
        [Fact]
        public void Double_Affine()
        {
            TwistedEdwardsCurveType curveType = TwistedEdwardsCurveType.Edwards25519;
            var curve = TwistedEdwardsCurve.GetNamedCurve(curveType);

            /* base point */
            var G = curve.GetBasePoint();
            var doubleG = TwistedEdwardsMath.Add(curve, G, G);

            /* 2G should equal G + G */
            var GplusG = TwistedEdwardsMath.Add(curve, G, G);
            Assert.Equal(GplusG, doubleG);

            /* algebraic identity: 2P = P + P */
            var P = curve.GetBasePoint();
            var doubleP = TwistedEdwardsMath.Add(curve, P, P);

            var PplusP = TwistedEdwardsMath.Add(curve, P, P);
            Assert.Equal(PplusP, doubleP);

            /* (P + P) - P = P */
            var negP = TwistedEdwardsMath.Negate(curve, P);
            var doubleThenSubtract = TwistedEdwardsMath.Add(curve, doubleP, negP);
            Assert.Equal(P, doubleThenSubtract);

            /* doubling the point at infinity returns infinity */
            var inf = ECPoint.POINT_INFINITY;
            var doubleInf = TwistedEdwardsMath.Add(curve, inf, inf);
            Assert.Equal(inf, doubleInf);

            /* 2 * (-P) = -(2P) */
            var negDoubleP = TwistedEdwardsMath.Negate(curve, doubleP);
            var doubleNegP = TwistedEdwardsMath.Add(curve, negP, negP);
            Assert.Equal(negDoubleP, doubleNegP);
        }

        [Fact]
        public void Double_Projective()
        {
            TwistedEdwardsCurveType curveType = TwistedEdwardsCurveType.Edwards25519;
            var curve = TwistedEdwardsCurve.GetNamedCurve(curveType);

            /* base point */
            var G = curve.GetBasePoint();
            var P = curve.ToProjective(G);
            var doubleP = Ed3Math.UnifiedDoubling(curve, P);

            /* affine consistency: 2P in homogenous projective matches affine 2G */
            var affineDoubleP = curve.ToAffine(doubleP);
            var expectedDoubleG = TwistedEdwardsMath.Add(curve, G, G);
            Assert.Equal(expectedDoubleG, affineDoubleP);

            /* Homogenous projective doubling should equal P + P (projective) */
            var PplusP = Ed3Math.UnifiedDoubling(curve, P);
            Assert.Equal(PplusP, doubleP);

            /* algebraic identity: Double(-P) = -Double(P) */
            var negP = Ed3Math.Negate(curve, P);
            var doubleNegP = Ed3Math.UnifiedDoubling(curve, negP);

            var negDoubleP = Ed3Math.Negate(curve, doubleP);
            var adoubleNegP = curve.ToAffine(doubleNegP);

            var anegDoubleP = curve.ToAffine(negDoubleP);
            Assert.Equal(anegDoubleP, adoubleNegP);

            /* point at infinity */
            var inf = ECPoint3.POINT_INFINITY;
            var doubleInf = Ed3Math.UnifiedDoubling(curve, inf);
            Assert.Equal(inf, doubleInf);

            /* random point: compare affine results */
            var randomPoint = curve.GetBasePoint();
            var R = curve.ToProjective(randomPoint);

            var doubleR = Ed3Math.UnifiedDoubling(curve, R);
            var affineDoubleR = curve.ToAffine(doubleR);

            var expectedDoubleR = TwistedEdwardsMath.Add(curve, 
                randomPoint, randomPoint);
            Assert.Equal(expectedDoubleR, affineDoubleR);
        }

        [Fact]
        public void Double_Extended_Projective()
        {
            TwistedEdwardsCurveType curveType = TwistedEdwardsCurveType.Edwards25519;
            var curve = TwistedEdwardsCurve.GetNamedCurve(curveType);

            /* base point */
            var G = curve.GetBasePoint();
            var P = curve.ToExtendedProjective(G);
            var doubleP = Ed4Math.DedicatedDoubling(curve, P);

            /* affine consistency: 2P in extended projective matches affine 2G */
            var affineDoubleP = curve.ToAffine(doubleP);
            var expectedDoubleG = TwistedEdwardsMath.Add(curve, G, G);
            Assert.Equal(expectedDoubleG, affineDoubleP);

            /* Extended projective doubling should equal P + P */
            var PplusP = Ed4Math.DedicatedDoubling(curve, P);
            Assert.Equal(PplusP, doubleP);

            /* algebraic identity: Double(-P) = -Double(P) */
            var negP = Ed4Math.Negate(curve, P);
            var doubleNegP = Ed4Math.DedicatedDoubling(curve, negP);

            var negDoubleP = Ed4Math.Negate(curve, doubleP);
            var adoubleNegP = curve.ToAffine(doubleNegP);

            var anegDoubleP = curve.ToAffine(negDoubleP);
            Assert.Equal(anegDoubleP, adoubleNegP);

            /* point at infinity */
            var inf = ECPoint4.POINT_INFINITY;
            var doubleInf = Ed4Math.DedicatedDoubling(curve, inf);
            Assert.Equal(inf, doubleInf);

            /* random point: compare affine results */
            var randomPoint = curve.GetBasePoint();
            var R = curve.ToExtendedProjective(randomPoint);

            var doubleR = Ed4Math.DedicatedDoubling(curve, R);
            var affineDoubleR = curve.ToAffine(doubleR);

            var expectedDoubleR = TwistedEdwardsMath.Add(curve,
                randomPoint, randomPoint);
            Assert.Equal(expectedDoubleR, affineDoubleR);
        }
        #endregion

        #region #region Point Addition — All Coordinate Systems
        [Fact]
        public void Add_Affine()
        {
            TwistedEdwardsCurveType curveType = TwistedEdwardsCurveType.Edwards25519;
            var curve = TwistedEdwardsCurve.GetNamedCurve(curveType);

            /* base point addition: P + Q should equal Q + P */
            var P = curve.GetBasePoint();
            var Q = curve.GetBasePoint();

            var R = TwistedEdwardsMath.Add(curve, P, Q);
            var R_commutative = TwistedEdwardsMath.Add(curve, Q, P);
            Assert.Equal(R, R_commutative);

            /* algebraic identity: (P + Q) - Q = P */
            var negQ = TwistedEdwardsMath.Negate(curve, Q);
            var T = TwistedEdwardsMath.Add(curve, R, negQ);
            Assert.Equal(P, T);

            /* identity element: P + O = P */
            var inf = ECPoint.POINT_INFINITY;
            var P_plus_inf = TwistedEdwardsMath.Add(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* identity element: O + Q = Q */
            var inf_plus_Q = TwistedEdwardsMath.Add(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = O */
            var negP = TwistedEdwardsMath.Negate(curve, P);
            var P_plus_negP = TwistedEdwardsMath.Add(curve, P, negP);
            Assert.Equal(inf, P_plus_negP);

            /* random points commutative property */
            var randomA = curve.GetBasePoint();
            var randomB = curve.GetBasePoint();

            var sumAB = TwistedEdwardsMath.Add(curve, randomA, randomB);
            var sumBA = TwistedEdwardsMath.Add(curve, randomB, randomA);
            Assert.Equal(sumAB, sumBA);
        }

        [Fact]
        public void Add_Projective()
        {
            TwistedEdwardsCurveType curveType = TwistedEdwardsCurveType.Edwards25519;
            var curve = TwistedEdwardsCurve.GetNamedCurve(curveType);

            /* base point addition: P + Q = R */
            var G = curve.GetBasePoint();

            var P = curve.ToProjective(G);
            var Q = curve.ToProjective(G);

            var R = Ed3Math.UnifiedAdd(curve, P, Q);
            var affineR = curve.ToAffine(R);

            var expectedR = TwistedEdwardsMath.Add(curve, G, G);
            Assert.Equal(expectedR, affineR);

            /* identity element: P + O = P */
            var inf = ECPoint3.POINT_INFINITY;
            var P_plus_inf = Ed3Math.UnifiedAdd(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* identity element: O + Q = Q */
            var inf_plus_Q = Ed3Math.UnifiedAdd(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = O */
            var negP = Ed3Math.Negate(curve, P);
            var P_plus_negP = Ed3Math.UnifiedAdd(curve, P, negP);

            var PPn = curve.ToAffine(P_plus_negP);
            Assert.Equal(ECPoint.POINT_INFINITY, PPn);

            /* commutative property in projective coordinates */
            var randomPoint = curve.GetBasePoint();
            var R1 = curve.ToProjective(randomPoint);

            var R2 = curve.ToProjective(curve.GetBasePoint());
            var sum12 = Ed3Math.UnifiedAdd(curve, R1, R2);

            var sum21 = Ed3Math.UnifiedAdd(curve, R2, R1);
            var affineSum12 = curve.ToAffine(sum12);

            var affineSum21 = curve.ToAffine(sum21);
            Assert.Equal(affineSum12, affineSum21);

            /* random point addition consistency with affine */
            var randomA = curve.GetBasePoint();
            var randomB = curve.GetBasePoint();

            var projA = curve.ToProjective(randomA);
            var projB = curve.ToProjective(randomB);

            var projSum = Ed3Math.UnifiedAdd(curve, projA, projB);
            var affineProjSum = curve.ToAffine(projSum);

            var expectedAffineSum = TwistedEdwardsMath.Add(curve, randomA, randomB);
            Assert.Equal(expectedAffineSum, affineProjSum);
        }

        [Fact]
        public void Add_Extended_Projective()
        {
            TwistedEdwardsCurveType curveType = TwistedEdwardsCurveType.Edwards25519;
            var curve = TwistedEdwardsCurve.GetNamedCurve(curveType);

            /* base point addition: P + Q = R */
            var G = curve.GetBasePoint();

            var P = curve.ToExtendedProjective(G);
            var Q = curve.ToExtendedProjective(G);

            var R = Ed4Math.Add(curve, P, Q);
            var affineR = curve.ToAffine(R);

            var expectedR = TwistedEdwardsMath.Add(curve, G, G);
            Assert.Equal(expectedR, affineR);

            /* identity element: P + O = P */
            var inf = ECPoint4.POINT_INFINITY;
            var P_plus_inf = Ed4Math.Add(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* identity element: O + Q = Q */
            var inf_plus_Q = Ed4Math.Add(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = O */
            var negP = Ed4Math.Negate(curve, P);
            var P_plus_negP = Ed4Math.Add(curve, P, negP);

            var PPn = curve.ToAffine(P_plus_negP);
            Assert.Equal(ECPoint.POINT_INFINITY, PPn);

            /* commutative property in extended projective coordinates */
            var randomPoint = curve.GetBasePoint();
            var R1 = curve.ToExtendedProjective(randomPoint);

            var R2 = curve.ToExtendedProjective(curve.GetBasePoint());
            var sum12 = Ed4Math.Add(curve, R1, R2);

            var sum21 = Ed4Math.Add(curve, R2, R1);
            var affineSum12 = curve.ToAffine(sum12);

            var affineSum21 = curve.ToAffine(sum21);
            Assert.Equal(affineSum12, affineSum21);

            /* random point addition consistency with affine */
            var randomA = curve.GetBasePoint();
            var randomB = curve.GetBasePoint();

            var eprojA = curve.ToExtendedProjective(randomA);
            var eprojB = curve.ToExtendedProjective(randomB);

            var eprojSum = Ed4Math.Add(curve, eprojA, eprojB);
            var affineEProjSum = curve.ToAffine(eprojSum);

            var expectedAffineSum = TwistedEdwardsMath.Add(curve, randomA, randomB);
            Assert.Equal(expectedAffineSum, affineEProjSum);
        }
        #endregion
    }
}
