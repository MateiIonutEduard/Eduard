using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
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
    }
}
