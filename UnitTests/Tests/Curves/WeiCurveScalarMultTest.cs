using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
    public class WeiCurveScalarMultTest
    {
        [Fact]
        public void Negate_Affine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            ECPoint P = curve.GetBasePoint();

            ECPoint Q = ECMath.Negate(curve, P);
            BigInteger Xp = P.GetAffineX();

            BigInteger Xq = Q.GetAffineX();
            Assert.Equal(Xp, Xq);

            BigInteger p = curve.field;
            BigInteger Yq = Q.GetAffineY();

            BigInteger Yp = P.GetAffineY();
            Assert.Equal(Yp, p - Yq);

            P = ECPoint.POINT_INFINITY;
            Q = ECMath.Negate(curve, P);
            Assert.Equal(P, Q);
        }

        [Fact]
        public void Double_Affine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            ECPoint P = curve.GetBasePoint();

            ECPoint Q = ECMath.Add(curve, P, P);
            ECPoint iP = ECMath.Negate(curve, P);

            ECPoint R = ECMath.Add(curve, Q, iP);
            Assert.Equal(R, P);

            P = ECPoint.POINT_INFINITY;
            Q = ECMath.Add(curve, P, P);
            Assert.True(Q == ECPoint.POINT_INFINITY);
        }

        [Fact]
        public void Add_Affine()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            ECPoint P = curve.GetBasePoint();

            ECPoint Q = curve.GetBasePoint();
            ECPoint R = ECMath.Add(curve, P, Q);

            ECPoint iQ = ECMath.Negate(curve, Q);
            ECPoint T = ECMath.Add(curve, R, iQ);
            Assert.Equal(P, T);

            P = ECPoint.POINT_INFINITY;
            R = ECMath.Add(curve, P, Q);
            Assert.True(R == Q);

            P = curve.GetBasePoint();
            Q = ECPoint.POINT_INFINITY;

            R = ECMath.Add(curve, P, Q);
            Assert.True(P == R);
        }

        [Fact]
        public void Negate_Jacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            ECPoint basePoint = curve.GetBasePoint();
            ECPoint3w P = curve.ToJacobian(basePoint);

            ECPoint3w Q = Wei3Math.Negate(curve, P);
            Assert.Equal(P.x, Q.x);

            BigInteger p = curve.field;
            Assert.Equal(P.y, p - Q.y);

            Assert.True(P.z == Q.z);
            P = ECPoint3w.POINT_INFINITY;

            Q = Wei3Math.Negate(curve, P);
            Assert.Equal(P, Q);
        }

        [Fact]
        public void Double_Jacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            ECPoint basePoint = curve.GetBasePoint();
            ECPoint3w P = curve.ToJacobian(basePoint);

            ECPoint3w Q = Wei3Math.Doubling(curve, P);
            ECPoint Aq = curve.ToAffine(Q);

            ECPoint R = ECMath.Add(curve, basePoint, basePoint);
            Assert.Equal(Aq, R);

            P = ECPoint3w.POINT_INFINITY;
            Q = Wei3Math.Doubling(curve, P);
            Assert.True(Q == ECPoint3w.POINT_INFINITY);
        }

        [Fact]
        public void Add_Jacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            ECPoint P1 = curve.GetBasePoint();
            ECPoint P2 = curve.GetBasePoint();

            ECPoint3w P = curve.ToJacobian(P1);
            ECPoint3w Q = curve.ToJacobian(P2);

            ECPoint3w R = Wei3Math.Add(curve, P, Q);
            ECPoint P3 = ECMath.Add(curve, P1, P2);

            ECPoint Rp = curve.ToAffine(R);
            Assert.Equal(P3, Rp);

            P = ECPoint3w.POINT_INFINITY;
            R = Wei3Math.Add(curve, P, Q);
            Assert.Equal(Q, R);

            P = curve.ToJacobian(P1);
            Q = ECPoint3w.POINT_INFINITY;

            R = Wei3Math.Add(curve, P, Q);
            Assert.Equal(P, R);

            R = Wei3Math.Add(curve, P, P);
            P3 = ECMath.Add(curve, P1, P1);

            Rp = curve.ToAffine(R);
            Assert.Equal(Rp, P3);
        }

        [Fact]
        public void Negate_ModifiedJacobian() 
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            ECPoint basePoint = curve.GetBasePoint();
            ECPoint4w P = curve.ToModifiedJacobian(basePoint);

            ECPoint4w Q = Wei4Math.Negate(curve, P);
            Assert.Equal(P.x, Q.x);

            BigInteger p = curve.field;
            Assert.Equal(P.y, p - Q.y);

            Assert.True(P.z == Q.z);
            P = ECPoint4w.POINT_INFINITY;

            Q = Wei4Math.Negate(curve, P);
            Assert.Equal(P, Q);
        }

        [Fact]
        public void Double_ModifiedJacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            ECPoint basePoint = curve.GetBasePoint();
            ECPoint4w P = curve.ToModifiedJacobian(basePoint);

            ECPoint4w Q = Wei4Math.Doubling(curve, P);
            ECPoint Aq = curve.ToAffine(Q);

            ECPoint R = ECMath.Add(curve, basePoint, basePoint);
            Assert.Equal(Aq, R);

            P = ECPoint4w.POINT_INFINITY;
            Q = Wei4Math.Doubling(curve, P);
            Assert.True(Q == ECPoint4w.POINT_INFINITY);
        }

        [Fact]
        public void Add_ModifiedJacobian()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            ECPoint P1 = curve.GetBasePoint();
            ECPoint P2 = curve.GetBasePoint();

            ECPoint4w P = curve.ToModifiedJacobian(P1);
            ECPoint4w Q = curve.ToModifiedJacobian(P2);

            ECPoint4w R = Wei4Math.Add(curve, P, Q);
            ECPoint P3 = ECMath.Add(curve, P1, P2);

            ECPoint Rp = curve.ToAffine(R);
            Assert.Equal(P3, Rp);

            P = ECPoint4w.POINT_INFINITY;
            R = Wei4Math.Add(curve, P, Q);
            Assert.Equal(Q, R);

            P = curve.ToModifiedJacobian(P1);
            Q = ECPoint4w.POINT_INFINITY;

            R = Wei4Math.Add(curve, P, Q);
            Assert.Equal(P, R);

            R = Wei4Math.Add(curve, P, P);
            P3 = ECMath.Add(curve, P1, P1);

            Rp = curve.ToAffine(R);
            Assert.Equal(Rp, P3);
        }
    }
}
