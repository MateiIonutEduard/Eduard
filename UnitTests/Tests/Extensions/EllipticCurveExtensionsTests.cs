using System;
using Eduard.Security.Curves;
using Eduard.Security.Primitives;
using Eduard.Security.Extensions;

namespace Eduard.Tests.Extensions
{
    [Collection("Sequential")]
    public class EllipticCurveExtensionsTests
    {
        #region Weierstrass — Montgomery Conversions

        [Fact]
        public void Weierstrass_ToMontgomeryCurve_ValidNistP256_ReturnsMontgomeryCurve()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montCurve = weiCurve.ToMontgomeryCurve();

            Assert.NotNull(montCurve);
            Assert.Equal(weiCurve.field, montCurve.field);

            Assert.Equal(weiCurve.order, montCurve.order);
            Assert.Equal(weiCurve.cofactor, montCurve.cofactor);

            Assert.NotEqual(0, montCurve.A);
            Assert.NotEqual(0, montCurve.B);
        }

        [Fact]
        public void Weierstrass_ToMontgomeryCurve_CofactorNotMultipleOf4_ThrowsException()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var modifiedCurve = new EllipticCurve(weiCurve.a, weiCurve.b,
                weiCurve.field, weiCurve.order, 2);

            Assert.Throws<ArgumentException>(() =>
                modifiedCurve.ToMontgomeryCurve());
        }

        [Fact]
        public void Weierstrass_ToMontgomeryPoint_ValidPoint_MapsCorrectly()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();
            var montPoint = weiCurve.ToMontgomeryPoint(G);
            Assert.NotEqual(ECPoint.POINT_INFINITY, montPoint);
        }

        [Fact]
        public void Weierstrass_ToMontgomeryPoint_Infinity_MapsToInfinity()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montPoint = weiCurve.ToMontgomeryPoint(ECPoint.POINT_INFINITY);
            Assert.Equal(ECPoint.POINT_INFINITY, montPoint);
        }

        [Fact]
        public void Weierstrass_ToMontgomeryPoint_RoundTrip_ReturnsOriginalPoint()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();

            var montPoint = weiCurve.ToMontgomeryPoint(G);
            var montCurve = weiCurve.ToMontgomeryCurve();

            var recoveredPoint = montCurve.ToWeierstrassPoint(montPoint);
            Assert.Equal(G, recoveredPoint);
        }

        #endregion

        #region Weierstrass — Twisted Edwards Conversions

        [Fact]
        public void Weierstrass_ToTwistedEdwardsCurve_ValidNistP256_ReturnsTwistedEdwardsCurve()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edCurve = weiCurve.ToTwistedEdwardsCurve();
            Assert.NotNull(edCurve);

            Assert.Equal(weiCurve.field, edCurve.field);
            Assert.Equal(weiCurve.order, edCurve.order);

            Assert.Equal(weiCurve.cofactor, edCurve.cofactor);
            Assert.NotEqual(0, edCurve.a);

            Assert.NotEqual(0, edCurve.d);
            Assert.NotEqual(edCurve.a, edCurve.d);
        }

        [Fact]
        public void Weierstrass_ToTwistedEdwardsPoint_ValidPoint_MapsCorrectly()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();
            var edPoint = weiCurve.ToTwistedEdwardsPoint(G);
            Assert.NotEqual(ECPoint.POINT_INFINITY, edPoint);
        }

        [Fact]
        public void Weierstrass_ToTwistedEdwardsPoint_Infinity_MapsToInfinity()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edPoint = weiCurve.ToTwistedEdwardsPoint(ECPoint.POINT_INFINITY);
            Assert.Equal(ECPoint.POINT_INFINITY, edPoint);
        }

        [Fact]
        public void Weierstrass_ToTwistedEdwardsPoint_RoundTrip_ReturnsOriginalPoint()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();

            var edPoint = weiCurve.ToTwistedEdwardsPoint(G);
            var edCurve = weiCurve.ToTwistedEdwardsCurve();

            var recoveredPoint = edCurve.ToWeierstrassPoint(edPoint);
            Assert.Equal(G, recoveredPoint);
        }

        #endregion
    }
}
