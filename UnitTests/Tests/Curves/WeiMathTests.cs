using System;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
    [Collection("Sequential")]
    public class WeiMathTests
    {
        #region Point Negation Tests — All Coordinate Systems

        [Fact]
        public void Negate_Affine_BasePoint_Properties()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var p = curve.field;

            /* base point */
            var P = curve.GetBasePoint();
            var Q = ECMath.Negate(curve, P);

            Assert.Equal(P.GetAffineX(), Q.GetAffineX());
            Assert.Equal(p - P.GetAffineY(), Q.GetAffineY());

            /* P + (-P) = point at infinity */
            Assert.Equal(ECPoint.POINT_INFINITY, 
                ECMath.Add(curve, P, Q));

            /* double negation */
            Assert.Equal(P, ECMath.Negate(curve, Q));

            /* point at infinity */
            Assert.Equal(ECPoint.POINT_INFINITY, 
                ECMath.Negate(curve, ECPoint.POINT_INFINITY));
        }

        [Fact]
        public void Negate_Jacobian_BasePoint_Properties()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var p = curve.field;

            /* base point */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobian(G);
            var negP = Wei3Math.Negate(curve, P);

            /* Jacobian coordinate checks */
            Assert.Equal(P.X, negP.X);
            var expectedNegY = p - P.Y;

            Assert.Equal(expectedNegY, negP.Y);
            Assert.Equal(P.Z, negP.Z);

            /* affine consistency */
            var affineNegP = curve.ToAffine(negP);
            Assert.Equal(G.GetAffineX(), affineNegP.GetAffineX());
            Assert.Equal(p - G.GetAffineY(), affineNegP.GetAffineY());

            /* P + (-P) = point at infinity */
            var sum = Wei3Math.Add(curve, P, negP);
            Assert.Equal(ECPoint3w.POINT_INFINITY, sum);

            /* double negation (involution) */
            var negNegP = Wei3Math.Negate(curve, negP);
            Assert.Equal(P, negNegP);

            /* point at infinity */
            var inf = ECPoint3w.POINT_INFINITY;
            Assert.Equal(inf, Wei3Math.Negate(curve, inf));
        }

        [Fact]
        public void Negate_ModifiedJacobian_BasePoint_Properties()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var p = curve.field;

            /* base point */
            var G = curve.GetBasePoint();
            var P = curve.ToModifiedJacobian(G);
            var negP = Wei4Math.Negate(curve, P);

            /* modified Jacobian coordinate checks */
            Assert.Equal(P.X, negP.X);
            var expectedNegY = p - P.Y;

            Assert.Equal(expectedNegY, negP.Y);
            Assert.Equal(P.Z, negP.Z);

            /* affine consistency */
            var affineNegP = curve.ToAffine(negP);
            Assert.Equal(G.GetAffineX(), affineNegP.GetAffineX());
            Assert.Equal(p - G.GetAffineY(), affineNegP.GetAffineY());

            /* P + (-P) = point at infinity */
            var sum = Wei4Math.Add(curve, P, negP);
            Assert.Equal(ECPoint4w.POINT_INFINITY, sum);

            /* double negation (involution) */
            var negNegP = Wei4Math.Negate(curve, negP);
            Assert.Equal(P, negNegP);

            /* point at infinity */
            var inf = ECPoint4w.POINT_INFINITY;
            Assert.Equal(inf, Wei4Math.Negate(curve, inf));
        }

        [Fact]
        public void Negate_JacobianChudnovsky_BasePoint_Properties()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var p = curve.field;

            /* base point */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobianChudnovsky(G);
            var negP = Wei5Math.Negate(curve, P);

            /* Jacobian-Chudnovsky coordinate checks */
            Assert.Equal(P.X, negP.X);
            var expectedNegY = p - P.Y;

            Assert.Equal(expectedNegY, negP.Y);
            Assert.Equal(P.Z, negP.Z);

            /* affine consistency */
            var affineNegP = curve.ToAffine(negP);
            Assert.Equal(G.GetAffineX(), affineNegP.GetAffineX());
            Assert.Equal(p - G.GetAffineY(), affineNegP.GetAffineY());

            /* P + (-P) = point at infinity */
            var sum = Wei5Math.Add(curve, P, negP);
            Assert.Equal(ECPoint5w.POINT_INFINITY, sum);

            /* double negation (involution) */
            var negNegP = Wei5Math.Negate(curve, negP);
            Assert.Equal(P, negNegP);

            /* point at infinity */
            var inf = ECPoint5w.POINT_INFINITY;
            Assert.Equal(inf, Wei5Math.Negate(curve, inf));
        }

        #endregion

        #region Point Doubling — All Coordinate Systems

        [Fact]
        public void Double_Affine_BasePoint_Properties()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* base point */
            var G = curve.GetBasePoint();
            var doubleG = ECMath.Add(curve, G, G);

            /* 2G should equal G + G */
            var GplusG = ECMath.Add(curve, G, G);
            Assert.Equal(GplusG, doubleG);

            /* algebraic identity: 2P = P + P */
            var P = curve.GetBasePoint();
            var doubleP = ECMath.Add(curve, P, P);

            var PplusP = ECMath.Add(curve, P, P);
            Assert.Equal(PplusP, doubleP);

            /* (P + P) - P = P */
            var negP = ECMath.Negate(curve, P);
            var doubleThenSubtract = ECMath.Add(curve, doubleP, negP);
            Assert.Equal(P, doubleThenSubtract);

            /* doubling the point at infinity returns infinity */
            var inf = ECPoint.POINT_INFINITY;
            var doubleInf = ECMath.Add(curve, inf, inf);
            Assert.Equal(inf, doubleInf);

            /* 2 * (-P) = -(2P) */
            var negDoubleP = ECMath.Negate(curve, doubleP);
            var doubleNegP = ECMath.Add(curve, negP, negP);
            Assert.Equal(negDoubleP, doubleNegP);
        }

        [Fact]
        public void Double_Jacobian_BasePoint_ConsistentWithAffine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* base point */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobian(G);
            var doubleP = Wei3Math.Doubling(curve, P);

            /* affine consistency: 2P in Jacobian matches affine 2G */
            var affineDoubleP = curve.ToAffine(doubleP);
            var expectedDoubleG = ECMath.Add(curve, G, G);
            Assert.Equal(expectedDoubleG, affineDoubleP);

            /* Jacobian doubling should equal P + P (both in Jacobian) */
            var PplusP = Wei3Math.Add(curve, P, P);
            Assert.Equal(PplusP, doubleP);

            /* algebraic identity: Double(-P) = -Double(P) */
            var negP = Wei3Math.Negate(curve, P);
            var doubleNegP = Wei3Math.Doubling(curve, negP);

            var negDoubleP = Wei3Math.Negate(curve, doubleP);
            var adoubleNegP = curve.ToAffine(doubleNegP);

            var anegDoubleP = curve.ToAffine(negDoubleP);
            Assert.Equal(anegDoubleP, adoubleNegP);

            /* point at infinity */
            var inf = ECPoint3w.POINT_INFINITY;
            var doubleInf = Wei3Math.Doubling(curve, inf);
            Assert.Equal(inf, doubleInf);

            /* random point: compare affine results, not Jacobian coordinates */
            var randomPoint = curve.GetBasePoint();
            var R = curve.ToJacobian(randomPoint);

            var doubleR = Wei3Math.Doubling(curve, R);
            var affineDoubleR = curve.ToAffine(doubleR);

            var expectedDoubleR = ECMath.Add(curve, randomPoint, randomPoint);
            Assert.Equal(expectedDoubleR, affineDoubleR);
        }

        [Fact]
        public void Double_ModifiedJacobian_BasePoint_ConsistentWithAffine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* base point */
            var G = curve.GetBasePoint();
            var P = curve.ToModifiedJacobian(G);
            var doubleP = Wei4Math.Doubling(curve, P);

            /* affine consistency */
            var affineDoubleP = curve.ToAffine(doubleP);
            var expectedDoubleG = ECMath.Add(curve, G, G);
            Assert.Equal(expectedDoubleG, affineDoubleP);

            /* modified Jacobian doubling should equal P + P */
            var PplusP = Wei4Math.Add(curve, P, P);
            Assert.Equal(PplusP, doubleP);

            /* algebraic identity: Double(-P) = -Double(P) */
            var negP = Wei4Math.Negate(curve, P);
            var doubleNegP = Wei4Math.Doubling(curve, negP);

            var negDoubleP = Wei4Math.Negate(curve, doubleP);
            var anegDoubleP = curve.ToAffine(negDoubleP);

            var adoubleNegP = curve.ToAffine(doubleNegP);
            Assert.Equal(anegDoubleP, adoubleNegP);

            /* point at infinity */
            var inf = ECPoint4w.POINT_INFINITY;
            var doubleInf = Wei4Math.Doubling(curve, inf);
            Assert.Equal(inf, doubleInf);

            /* random point: compare affine results */
            var randomPoint = curve.GetBasePoint();
            var R = curve.ToModifiedJacobian(randomPoint);

            var doubleR = Wei4Math.Doubling(curve, R);
            var affineDoubleR = curve.ToAffine(doubleR);

            var expectedDoubleR = ECMath.Add(curve, randomPoint, randomPoint);
            Assert.Equal(expectedDoubleR, affineDoubleR);
        }

        [Fact]
        public void Double_JacobianChudnovsky_BasePoint_ConsistentWithAffine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* base point */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobianChudnovsky(G);
            var doubleP = Wei5Math.Doubling(curve, P);

            /* affine consistency */
            var affineDoubleP = curve.ToAffine(doubleP);
            var expectedDoubleG = ECMath.Add(curve, G, G);
            Assert.Equal(expectedDoubleG, affineDoubleP);

            /* Jacobian-Chudnovsky doubling should equal P + P */
            var PplusP = Wei5Math.Add(curve, P, P);
            Assert.Equal(PplusP, doubleP);

            /* algebraic identity: Double(-P) = -Double(P) */
            var negP = Wei5Math.Negate(curve, P);
            var doubleNegP = Wei5Math.Doubling(curve, negP);

            var negDoubleP = Wei5Math.Negate(curve, doubleP);
            var adoubleNegP = curve.ToAffine(doubleNegP);

            var anegDoubleP = curve.ToAffine(negDoubleP);
            Assert.Equal(anegDoubleP, adoubleNegP);

            /* point at infinity */
            var inf = ECPoint5w.POINT_INFINITY;
            var doubleInf = Wei5Math.Doubling(curve, inf);
            Assert.Equal(inf, doubleInf);

            /* random point: compare affine results */
            var randomPoint = curve.GetBasePoint();
            var R = curve.ToJacobianChudnovsky(randomPoint);

            var doubleR = Wei5Math.Doubling(curve, R);
            var affineDoubleR = curve.ToAffine(doubleR);

            var expectedDoubleR = ECMath.Add(curve, randomPoint, randomPoint);
            Assert.Equal(expectedDoubleR, affineDoubleR);
        }

        #endregion

        #region Point Addition — All Coordinate Systems

        [Fact]
        public void Add_Affine_BasePoint_VerifiesGroupLaws()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* base point addition: P + Q should equal Q + P */
            var P = curve.GetBasePoint();
            var Q = curve.GetBasePoint();

            var R = ECMath.Add(curve, P, Q);
            var R_commutative = ECMath.Add(curve, Q, P);
            Assert.Equal(R, R_commutative);

            /* algebraic identity: (P + Q) - Q = P */
            var negQ = ECMath.Negate(curve, Q);
            var T = ECMath.Add(curve, R, negQ);
            Assert.Equal(P, T);

            /* identity element: P + O = P */
            var inf = ECPoint.POINT_INFINITY;
            var P_plus_inf = ECMath.Add(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* identity element: O + Q = Q */
            var inf_plus_Q = ECMath.Add(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = O */
            var negP = ECMath.Negate(curve, P);
            var P_plus_negP = ECMath.Add(curve, P, negP);
            Assert.Equal(inf, P_plus_negP);

            /* random points commutative property */
            var randomA = curve.GetBasePoint();
            var randomB = curve.GetBasePoint();

            var sumAB = ECMath.Add(curve, randomA, randomB);
            var sumBA = ECMath.Add(curve, randomB, randomA);
            Assert.Equal(sumAB, sumBA);
        }

        [Fact]
        public void Add_Jacobian_BasePoint_ConsistentWithAffine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* base point addition: P + Q = R */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobian(G);
            var Q = curve.ToJacobian(G);

            var R = Wei3Math.Add(curve, P, Q);
            var affineR = curve.ToAffine(R);
            var expectedR = ECMath.Add(curve, G, G);
            Assert.Equal(expectedR, affineR);

            /* identity element: P + O = P */
            var inf = ECPoint3w.POINT_INFINITY;
            var P_plus_inf = Wei3Math.Add(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* identity element: O + Q = Q */
            var inf_plus_Q = Wei3Math.Add(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = O */
            var negP = Wei3Math.Negate(curve, P);
            var P_plus_negP = Wei3Math.Add(curve, P, negP);
            Assert.Equal(inf, P_plus_negP);

            /* commutative property in Jacobian coordinates */
            var randomPoint = curve.GetBasePoint();
            var R1 = curve.ToJacobian(randomPoint);

            var R2 = curve.ToJacobian(curve.GetBasePoint());
            var sum12 = Wei3Math.Add(curve, R1, R2);

            var sum21 = Wei3Math.Add(curve, R2, R1);
            var affineSum12 = curve.ToAffine(sum12);

            var affineSum21 = curve.ToAffine(sum21);
            Assert.Equal(affineSum12, affineSum21);

            /* random point addition consistency with affine */
            var randomA = curve.GetBasePoint();
            var randomB = curve.GetBasePoint();

            var jacA = curve.ToJacobian(randomA);
            var jacB = curve.ToJacobian(randomB);

            var jacSum = Wei3Math.Add(curve, jacA, jacB);
            var affineJacSum = curve.ToAffine(jacSum);

            var expectedAffineSum = ECMath.Add(curve, randomA, randomB);
            Assert.Equal(expectedAffineSum, affineJacSum);
        }

        [Fact]
        public void Add_ModifiedJacobian_BasePoint_ConsistentWithAffine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* base point addition: P + Q = R */
            var G = curve.GetBasePoint();
            var P = curve.ToModifiedJacobian(G);
            var Q = curve.ToModifiedJacobian(G);

            var R = Wei4Math.Add(curve, P, Q);
            var affineR = curve.ToAffine(R);

            var expectedR = ECMath.Add(curve, G, G);
            Assert.Equal(expectedR, affineR);

            /* identity element: P + O = P */
            var inf = ECPoint4w.POINT_INFINITY;
            var P_plus_inf = Wei4Math.Add(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* identity element: O + Q = Q */
            var inf_plus_Q = Wei4Math.Add(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = O */
            var negP = Wei4Math.Negate(curve, P);
            var P_plus_negP = Wei4Math.Add(curve, P, negP);
            Assert.Equal(inf, P_plus_negP);

            /* commutative property */
            var randomPoint = curve.GetBasePoint();
            var R1 = curve.ToModifiedJacobian(randomPoint);

            var R2 = curve.ToModifiedJacobian(curve.GetBasePoint());
            var sum12 = Wei4Math.Add(curve, R1, R2);

            var sum21 = Wei4Math.Add(curve, R2, R1);
            var affineSum12 = curve.ToAffine(sum12);

            var affineSum21 = curve.ToAffine(sum21);
            Assert.Equal(affineSum12, affineSum21);

            /* random point addition consistency with affine */
            var randomA = curve.GetBasePoint();
            var randomB = curve.GetBasePoint();

            var modJacA = curve.ToModifiedJacobian(randomA);
            var modJacB = curve.ToModifiedJacobian(randomB);

            var modJacSum = Wei4Math.Add(curve, modJacA, modJacB);
            var affineModJacSum = curve.ToAffine(modJacSum);

            var expectedAffineSum = ECMath.Add(curve, randomA, randomB);
            Assert.Equal(expectedAffineSum, affineModJacSum);
        }

        [Fact]
        public void Add_JacobianChudnovsky_BasePoint_ConsistentWithAffine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* base point addition: P + Q = R */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobianChudnovsky(G);
            var Q = curve.ToJacobianChudnovsky(G);

            var R = Wei5Math.Add(curve, P, Q);
            var affineR = curve.ToAffine(R);

            var expectedR = ECMath.Add(curve, G, G);
            Assert.Equal(expectedR, affineR);

            /* identity element: P + O = P */
            var inf = ECPoint5w.POINT_INFINITY;
            var P_plus_inf = Wei5Math.Add(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* identity element: O + Q = Q */
            var inf_plus_Q = Wei5Math.Add(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = ∞ */
            var negP = Wei5Math.Negate(curve, P);
            var P_plus_negP = Wei5Math.Add(curve, P, negP);
            Assert.Equal(inf, P_plus_negP);

            /* commutative property */
            var randomPoint = curve.GetBasePoint();
            var R1 = curve.ToJacobianChudnovsky(randomPoint);

            var R2 = curve.ToJacobianChudnovsky(curve.GetBasePoint());
            var sum12 = Wei5Math.Add(curve, R1, R2);

            var sum21 = Wei5Math.Add(curve, R2, R1);
            var affineSum12 = curve.ToAffine(sum12);

            var affineSum21 = curve.ToAffine(sum21);
            Assert.Equal(affineSum12, affineSum21);

            /* random point addition consistency with affine */
            var randomA = curve.GetBasePoint();
            var randomB = curve.GetBasePoint();

            var jcA = curve.ToJacobianChudnovsky(randomA);
            var jcB = curve.ToJacobianChudnovsky(randomB);

            var jcSum = Wei5Math.Add(curve, jcA, jcB);
            var affineJcSum = curve.ToAffine(jcSum);

            var expectedAffineSum = ECMath.Add(curve, randomA, randomB);
            Assert.Equal(expectedAffineSum, affineJcSum);
        }

        #endregion

        #region Point Multi-Addition - Affine Coordinate System

        [Fact]
        public void AddBatch_NullLeftArray_ThrowsArgumentNullException()
        {
            var curve = EllipticCurve.GetNamedCurve(
                WeiCurveType.NistP256);

            ECPoint[] points = new ECPoint[5];
            int j, k;

            for (j = 0; j < points.Length; j++)
                points[j] = ECPoint.POINT_INFINITY;

            Assert.Throws<ArgumentNullException>(() => 
                ECMath.Add(curve, points, null));
        }

        [Fact]
        public void AddBatch_NullRightArray_ThrowsArgumentNullException()
        {
            var curve = EllipticCurve.GetNamedCurve(
                WeiCurveType.NistP256);

            ECPoint[] points = new ECPoint[5];
            int j, k;

            for (j = 0; j < points.Length; j++)
                points[j] = ECPoint.POINT_INFINITY;

            Assert.Throws<ArgumentNullException>(() =>
                ECMath.Add(curve, null, points));
        }

        [Fact]
        public void AddBatch_MismatchedLengths_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(
                WeiCurveType.NistP256);

            ECPoint[] left = new ECPoint[4];
            ECPoint[] right = new ECPoint[6];
            int j, k;

            for(j = 0; j < 4; j++)
            {
                left[j] = ECPoint.POINT_INFINITY;
                right[j] = ECPoint.POINT_INFINITY;
            }

            for (j = 4; j < 6; j++)
                right[j] = ECPoint.POINT_INFINITY;

            Assert.Throws<ArgumentException>(() =>
                ECMath.Add(curve, left, right));
        }

        [Fact]
        public void AddBatch_EmptyArrays_Succeeds()
        {
            var curve = EllipticCurve.GetNamedCurve(
                WeiCurveType.NistP256);

            ECPoint[] left = new ECPoint[0];
            ECPoint[] right = new ECPoint[0];

            ECMath.Add(curve, left, right);
            Assert.True(right.Length == 0);
        }

        [Fact]
        public void AddBatch_MatchesScalarAdd_ForAllBoundaryCases()
        {
            var curve = EllipticCurve.GetNamedCurve(
                WeiCurveType.NistP256);

            ECPoint[] left = new ECPoint[5];
            ECPoint[] right = new ECPoint[5];

            ECPoint[] sum = new ECPoint[5];
            int j, k;

            left[0] = ECPoint.POINT_INFINITY;
            right[0] = curve.GetBasePoint();

            left[1] = curve.GetBasePoint();
            right[1] = ECPoint.POINT_INFINITY;

            left[2] = ECPoint.POINT_INFINITY;
            right[2] = ECPoint.POINT_INFINITY;

            left[3] = curve.GetBasePoint();
            right[3] = left[1];

            left[4] = curve.GetBasePoint();
            right[4] = ECMath.Negate(curve, 
                left[4]);

            for (j = 0; j < 5; j++)
                sum[j] = ECMath.Add(curve,
                    left[j], right[j]);

            ECMath.Add(curve, left, right);

            for (k = 0; k < 5; k++)
                Assert.Equal(right[k], sum[k]);
        }

        [Fact]
        public void AddBatch_MatchesScalarAdd()
        {
            var curve = EllipticCurve.GetNamedCurve(
                WeiCurveType.NistP256);

            ECPoint[] left = new ECPoint[5];
            ECPoint[] right = new ECPoint[5];

            ECPoint[] sum = new ECPoint[5];
            int j, k;

            for (j = 0; j < 5; j++)
            {
                left[j] = curve.GetBasePoint();
                right[j] = curve.GetBasePoint();

                sum[j] = ECMath.Add(curve,
                    left[j], right[j]);
            }

            ECMath.Add(curve, left, right);

            for (k = 0; k < 5; k++)
                Assert.Equal(right[k], sum[k]);
        }

        #endregion
    }
}
