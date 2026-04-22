using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
    [Collection("Sequential")]
    public class TwistedEdwardsMathTest
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
    }
}
