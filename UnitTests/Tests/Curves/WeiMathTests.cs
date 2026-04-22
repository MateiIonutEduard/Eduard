using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
    public class WeiMathTests
    {
        #region Point Negation Tests — All Coordinate Systems
        [Fact]
        public void Negate_Affine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var p = curve.field;

            /* base point */
            var P = curve.GetBasePoint();
            var Q = ECMath.Negate(curve, P);

            Assert.Equal(P.GetAffineX(), Q.GetAffineX());
            Assert.Equal((p - P.GetAffineY()) % p, Q.GetAffineY());

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
        public void Negate_Jacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var p = curve.field;

            /* Base point */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobian(G);
            var negP = Wei3Math.Negate(curve, P);

            /* Jacobian coordinate checks */
            Assert.Equal(P.x, negP.x);
            var expectedNegY = (P.y % p == 0) ? 0 : p - (P.y % p);
            Assert.Equal(expectedNegY, negP.y % p);
            Assert.Equal(P.z, negP.z);

            /* Affine consistency */
            var affineNegP = curve.ToAffine(negP);
            Assert.Equal(G.GetAffineX(), affineNegP.GetAffineX());
            Assert.Equal((p - G.GetAffineY()) % p, affineNegP.GetAffineY());

            /* P + (-P) = point at infinity */
            var sum = Wei3Math.Add(curve, P, negP);
            Assert.Equal(ECPoint3w.POINT_INFINITY, sum);

            /* Double negation (involution) */
            var negNegP = Wei3Math.Negate(curve, negP);
            Assert.Equal(P, negNegP);

            /* Point at infinity */
            var inf = ECPoint3w.POINT_INFINITY;
            Assert.Equal(inf, Wei3Math.Negate(curve, inf));
        }

        [Fact]
        public void Negate_ModifiedJacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var p = curve.field;

            /* Base point */
            var G = curve.GetBasePoint();
            var P = curve.ToModifiedJacobian(G);
            var negP = Wei4Math.Negate(curve, P);

            /* Modified Jacobian coordinate checks */
            Assert.Equal(P.x, negP.x);
            var expectedNegY = (P.y % p == 0) ? 0 : p - (P.y % p);
            Assert.Equal(expectedNegY, negP.y % p);
            Assert.Equal(P.z, negP.z);

            /* Affine consistency */
            var affineNegP = curve.ToAffine(negP);
            Assert.Equal(G.GetAffineX(), affineNegP.GetAffineX());
            Assert.Equal((p - G.GetAffineY()) % p, affineNegP.GetAffineY());

            /* P + (-P) = point at infinity */
            var sum = Wei4Math.Add(curve, P, negP);
            Assert.Equal(ECPoint4w.POINT_INFINITY, sum);

            /* Double negation (involution) */
            var negNegP = Wei4Math.Negate(curve, negP);
            Assert.Equal(P, negNegP);

            /* Point at infinity */
            var inf = ECPoint4w.POINT_INFINITY;
            Assert.Equal(inf, Wei4Math.Negate(curve, inf));
        }

        [Fact]
        public void Negate_JacobianChudnovsky()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var p = curve.field;

            /* Base point */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobianChudnovsky(G);
            var negP = Wei5Math.Negate(curve, P);

            /* Jacobian-Chudnovsky coordinate checks */
            Assert.Equal(P.x, negP.x);
            var expectedNegY = (P.y % p == 0) ? 0 : p - (P.y % p);
            Assert.Equal(expectedNegY, negP.y % p);
            Assert.Equal(P.z, negP.z);

            /* Affine consistency */
            var affineNegP = curve.ToAffine(negP);
            Assert.Equal(G.GetAffineX(), affineNegP.GetAffineX());
            Assert.Equal((p - G.GetAffineY()) % p, affineNegP.GetAffineY());

            /* P + (-P) = point at infinity */
            var sum = Wei5Math.Add(curve, P, negP);
            Assert.Equal(ECPoint5w.POINT_INFINITY, sum);

            /* Double negation (involution) */
            var negNegP = Wei5Math.Negate(curve, negP);
            Assert.Equal(P, negNegP);

            /* Point at infinity */
            var inf = ECPoint5w.POINT_INFINITY;
            Assert.Equal(inf, Wei5Math.Negate(curve, inf));
        }
        #endregion

        #region Point Doubling — All Coordinate Systems
        [Fact]
        public void Double_Affine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* Base point */
            var G = curve.GetBasePoint();
            var doubleG = ECMath.Add(curve, G, G);

            /* 2G should equal G + G */
            var GplusG = ECMath.Add(curve, G, G);
            Assert.Equal(GplusG, doubleG);

            /* Algebraic identity: 2P = P + P */
            var P = curve.GetBasePoint();
            var doubleP = ECMath.Add(curve, P, P);

            var PplusP = ECMath.Add(curve, P, P);
            Assert.Equal(PplusP, doubleP);

            /* (P + P) - P = P */
            var negP = ECMath.Negate(curve, P);
            var doubleThenSubtract = ECMath.Add(curve, doubleP, negP);
            Assert.Equal(P, doubleThenSubtract);

            /* Doubling the point at infinity returns infinity */
            var inf = ECPoint.POINT_INFINITY;
            var doubleInf = ECMath.Add(curve, inf, inf);
            Assert.Equal(inf, doubleInf);

            /* 2 * (-P) = -(2P) */
            var negDoubleP = ECMath.Negate(curve, doubleP);
            var doubleNegP = ECMath.Add(curve, negP, negP);
            Assert.Equal(negDoubleP, doubleNegP);
        }

        [Fact]
        public void Double_Jacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* Base point */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobian(G);
            var doubleP = Wei3Math.Doubling(curve, P);

            /* Affine consistency: 2P in Jacobian matches affine 2G */
            var affineDoubleP = curve.ToAffine(doubleP);
            var expectedDoubleG = ECMath.Add(curve, G, G);
            Assert.Equal(expectedDoubleG, affineDoubleP);

            /* Jacobian doubling should equal P + P (both in Jacobian) */
            var PplusP = Wei3Math.Add(curve, P, P);
            Assert.Equal(PplusP, doubleP);

            /* Algebraic identity: Double(-P) = -Double(P) */
            var negP = Wei3Math.Negate(curve, P);
            var doubleNegP = Wei3Math.Doubling(curve, negP);

            var negDoubleP = Wei3Math.Negate(curve, doubleP);
            var adoubleNegP = curve.ToAffine(doubleNegP);

            var anegDoubleP = curve.ToAffine(negDoubleP);
            Assert.Equal(anegDoubleP, adoubleNegP);

            /* Point at infinity */
            var inf = ECPoint3w.POINT_INFINITY;
            var doubleInf = Wei3Math.Doubling(curve, inf);
            Assert.Equal(inf, doubleInf);

            /* Random point: compare affine results, not Jacobian coordinates */
            var randomPoint = curve.GetBasePoint();
            var R = curve.ToJacobian(randomPoint);

            var doubleR = Wei3Math.Doubling(curve, R);
            var affineDoubleR = curve.ToAffine(doubleR);

            var expectedDoubleR = ECMath.Add(curve, randomPoint, randomPoint);
            Assert.Equal(expectedDoubleR, affineDoubleR);
        }

        [Fact]
        public void Double_ModifiedJacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* Base point */
            var G = curve.GetBasePoint();
            var P = curve.ToModifiedJacobian(G);
            var doubleP = Wei4Math.Doubling(curve, P);

            /* Affine consistency */
            var affineDoubleP = curve.ToAffine(doubleP);
            var expectedDoubleG = ECMath.Add(curve, G, G);
            Assert.Equal(expectedDoubleG, affineDoubleP);

            /* Modified Jacobian doubling should equal P + P */
            var PplusP = Wei4Math.Add(curve, P, P);
            Assert.Equal(PplusP, doubleP);

            /* Algebraic identity: Double(-P) = -Double(P) */
            var negP = Wei4Math.Negate(curve, P);
            var doubleNegP = Wei4Math.Doubling(curve, negP);

            var negDoubleP = Wei4Math.Negate(curve, doubleP);
            var anegDoubleP = curve.ToAffine(negDoubleP);

            var adoubleNegP = curve.ToAffine(doubleNegP);
            Assert.Equal(anegDoubleP, adoubleNegP);

            /* Point at infinity */
            var inf = ECPoint4w.POINT_INFINITY;
            var doubleInf = Wei4Math.Doubling(curve, inf);
            Assert.Equal(inf, doubleInf);

            /* Random point: compare affine results */
            var randomPoint = curve.GetBasePoint();
            var R = curve.ToModifiedJacobian(randomPoint);

            var doubleR = Wei4Math.Doubling(curve, R);
            var affineDoubleR = curve.ToAffine(doubleR);

            var expectedDoubleR = ECMath.Add(curve, randomPoint, randomPoint);
            Assert.Equal(expectedDoubleR, affineDoubleR);
        }

        [Fact]
        public void Double_JacobianChudnovsky()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* Base point */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobianChudnovsky(G);
            var doubleP = Wei5Math.Doubling(curve, P);

            /* Affine consistency */
            var affineDoubleP = curve.ToAffine(doubleP);
            var expectedDoubleG = ECMath.Add(curve, G, G);
            Assert.Equal(expectedDoubleG, affineDoubleP);

            /* Jacobian-Chudnovsky doubling should equal P + P */
            var PplusP = Wei5Math.Add(curve, P, P);
            Assert.Equal(PplusP, doubleP);

            /* Algebraic identity: Double(-P) = -Double(P) */
            var negP = Wei5Math.Negate(curve, P);
            var doubleNegP = Wei5Math.Doubling(curve, negP);

            var negDoubleP = Wei5Math.Negate(curve, doubleP);
            var adoubleNegP = curve.ToAffine(doubleNegP);

            var anegDoubleP = curve.ToAffine(negDoubleP);
            Assert.Equal(anegDoubleP, adoubleNegP);

            /* Point at infinity */
            var inf = ECPoint5w.POINT_INFINITY;
            var doubleInf = Wei5Math.Doubling(curve, inf);
            Assert.Equal(inf, doubleInf);

            /* Random point: compare affine results */
            var randomPoint = curve.GetBasePoint();
            var R = curve.ToJacobianChudnovsky(randomPoint);

            var doubleR = Wei5Math.Doubling(curve, R);
            var affineDoubleR = curve.ToAffine(doubleR);

            var expectedDoubleR = ECMath.Add(curve, randomPoint, randomPoint);
            Assert.Equal(expectedDoubleR, affineDoubleR);
        }
        #endregion

        #region #region Point Addition — All Coordinate Systems
        [Fact]
        public void Add_Affine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* Base point addition: P + Q should equal Q + P */
            var P = curve.GetBasePoint();
            var Q = curve.GetBasePoint(); // Same point for simplicity

            var R = ECMath.Add(curve, P, Q);
            var R_commutative = ECMath.Add(curve, Q, P);
            Assert.Equal(R, R_commutative);

            /* Algebraic identity: (P + Q) - Q = P */
            var negQ = ECMath.Negate(curve, Q);
            var T = ECMath.Add(curve, R, negQ);
            Assert.Equal(P, T);

            /* Identity element: P + ∞ = P */
            var inf = ECPoint.POINT_INFINITY;
            var P_plus_inf = ECMath.Add(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* Identity element: ∞ + Q = Q */
            var inf_plus_Q = ECMath.Add(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = ∞ */
            var negP = ECMath.Negate(curve, P);
            var P_plus_negP = ECMath.Add(curve, P, negP);
            Assert.Equal(inf, P_plus_negP);

            /* Random points commutative property */
            var randomA = curve.GetBasePoint();
            var randomB = curve.GetBasePoint();

            var sumAB = ECMath.Add(curve, randomA, randomB);
            var sumBA = ECMath.Add(curve, randomB, randomA);
            Assert.Equal(sumAB, sumBA);
        }

        [Fact]
        public void Add_Jacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* Base point addition: P + Q = R */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobian(G);
            var Q = curve.ToJacobian(G); // Same point: 2G

            var R = Wei3Math.Add(curve, P, Q);
            var affineR = curve.ToAffine(R);
            var expectedR = ECMath.Add(curve, G, G); // 2G
            Assert.Equal(expectedR, affineR);

            /* Identity element: P + ∞ = P */
            var inf = ECPoint3w.POINT_INFINITY;
            var P_plus_inf = Wei3Math.Add(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* Identity element: ∞ + Q = Q */
            var inf_plus_Q = Wei3Math.Add(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = ∞ */
            var negP = Wei3Math.Negate(curve, P);
            var P_plus_negP = Wei3Math.Add(curve, P, negP);
            Assert.Equal(inf, P_plus_negP);

            /* Commutative property in Jacobian coordinates */
            var randomPoint = curve.GetBasePoint();
            var R1 = curve.ToJacobian(randomPoint);

            var R2 = curve.ToJacobian(curve.GetBasePoint());
            var sum12 = Wei3Math.Add(curve, R1, R2);

            var sum21 = Wei3Math.Add(curve, R2, R1);
            var affineSum12 = curve.ToAffine(sum12);

            var affineSum21 = curve.ToAffine(sum21);
            Assert.Equal(affineSum12, affineSum21);

            /* Random point addition consistency with affine */
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
        public void Add_ModifiedJacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* Base point addition: P + Q = R */
            var G = curve.GetBasePoint();
            var P = curve.ToModifiedJacobian(G);
            var Q = curve.ToModifiedJacobian(G);

            var R = Wei4Math.Add(curve, P, Q);
            var affineR = curve.ToAffine(R);

            var expectedR = ECMath.Add(curve, G, G);
            Assert.Equal(expectedR, affineR);

            /* Identity element: P + ∞ = P */
            var inf = ECPoint4w.POINT_INFINITY;
            var P_plus_inf = Wei4Math.Add(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* Identity element: O + Q = Q */
            var inf_plus_Q = Wei4Math.Add(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = O */
            var negP = Wei4Math.Negate(curve, P);
            var P_plus_negP = Wei4Math.Add(curve, P, negP);
            Assert.Equal(inf, P_plus_negP);

            /* Commutative property */
            var randomPoint = curve.GetBasePoint();
            var R1 = curve.ToModifiedJacobian(randomPoint);

            var R2 = curve.ToModifiedJacobian(curve.GetBasePoint());
            var sum12 = Wei4Math.Add(curve, R1, R2);

            var sum21 = Wei4Math.Add(curve, R2, R1);
            var affineSum12 = curve.ToAffine(sum12);

            var affineSum21 = curve.ToAffine(sum21);
            Assert.Equal(affineSum12, affineSum21);

            /* Random point addition consistency with affine */
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
        public void Add_JacobianChudnovsky()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            /* Base point addition: P + Q = R */
            var G = curve.GetBasePoint();
            var P = curve.ToJacobianChudnovsky(G);
            var Q = curve.ToJacobianChudnovsky(G);

            var R = Wei5Math.Add(curve, P, Q);
            var affineR = curve.ToAffine(R);

            var expectedR = ECMath.Add(curve, G, G);
            Assert.Equal(expectedR, affineR);

            /* Identity element: P + O = P */
            var inf = ECPoint5w.POINT_INFINITY;
            var P_plus_inf = Wei5Math.Add(curve, P, inf);
            Assert.Equal(P, P_plus_inf);

            /* Identity element: O + Q = Q */
            var inf_plus_Q = Wei5Math.Add(curve, inf, Q);
            Assert.Equal(Q, inf_plus_Q);

            /* P + (-P) = ∞ */
            var negP = Wei5Math.Negate(curve, P);
            var P_plus_negP = Wei5Math.Add(curve, P, negP);
            Assert.Equal(inf, P_plus_negP);

            /* Commutative property */
            var randomPoint = curve.GetBasePoint();
            var R1 = curve.ToJacobianChudnovsky(randomPoint);

            var R2 = curve.ToJacobianChudnovsky(curve.GetBasePoint());
            var sum12 = Wei5Math.Add(curve, R1, R2);

            var sum21 = Wei5Math.Add(curve, R2, R1);
            var affineSum12 = curve.ToAffine(sum12);

            var affineSum21 = curve.ToAffine(sum21);
            Assert.Equal(affineSum12, affineSum21);

            /* Random point addition consistency with affine */
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
    }
}
