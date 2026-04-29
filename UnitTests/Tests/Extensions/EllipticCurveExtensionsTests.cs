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
        public void Weierstrass_ToMontgomeryCurve_ValidWei25519_ReturnsMontgomeryCurve()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montyCurve = weiCurve.ToMontgomeryCurve();

            Assert.NotNull(montyCurve);
            Assert.Equal(weiCurve.field, montyCurve.field);

            Assert.Equal(weiCurve.order, montyCurve.order);
            Assert.Equal(weiCurve.cofactor, montyCurve.cofactor);

            Assert.NotEqual(0, montyCurve.A);
            Assert.NotEqual(0, montyCurve.B);
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
            var montyPoint = weiCurve.ToMontgomeryPoint(G);
            Assert.NotEqual(ECPoint.POINT_INFINITY, montyPoint);
        }

        [Fact]
        public void Weierstrass_ToMontgomeryPoint_Infinity_MapsToInfinity()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montyPoint = weiCurve.ToMontgomeryPoint(ECPoint.POINT_INFINITY);
            Assert.Equal(ECPoint.POINT_INFINITY, montyPoint);
        }

        [Fact]
        public void Weierstrass_ToMontgomeryPoint_RoundTrip_ReturnsOriginalPoint()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();

            var montyPoint = weiCurve.ToMontgomeryPoint(G);
            var montyCurve = weiCurve.ToMontgomeryCurve();

            var recoveredPoint = montyCurve.ToWeierstrassPoint(montyPoint);
            Assert.Equal(G, recoveredPoint);
        }

        #endregion

        #region Weierstrass — Twisted Edwards Conversions

        [Fact]
        public void Weierstrass_ToTwistedEdwardsCurve_ValidWei25519_ReturnsTwistedEdwardsCurve()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edwCurve = weiCurve.ToTwistedEdwardsCurve();
            Assert.NotNull(edwCurve);

            Assert.Equal(weiCurve.field, edwCurve.field);
            Assert.Equal(weiCurve.order, edwCurve.order);

            Assert.Equal(weiCurve.cofactor, edwCurve.cofactor);
            Assert.NotEqual(0, edwCurve.a);

            Assert.NotEqual(0, edwCurve.d);
            Assert.NotEqual(edwCurve.a, edwCurve.d);
        }

        [Fact]
        public void Weierstrass_ToTwistedEdwardsPoint_ValidPoint_MapsCorrectly()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();
            var edwPoint = weiCurve.ToTwistedEdwardsPoint(G);
            Assert.NotEqual(ECPoint.POINT_INFINITY, edwPoint);
        }

        [Fact]
        public void Weierstrass_ToTwistedEdwardsPoint_Infinity_MapsToInfinity()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edwPoint = weiCurve.ToTwistedEdwardsPoint(ECPoint.POINT_INFINITY);
            Assert.Equal(ECPoint.POINT_INFINITY, edwPoint);
        }

        [Fact]
        public void Weierstrass_ToTwistedEdwardsPoint_RoundTrip_ReturnsOriginalPoint()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();

            var edwPoint = weiCurve.ToTwistedEdwardsPoint(G);
            var edwCurve = weiCurve.ToTwistedEdwardsCurve();

            var recoveredPoint = edwCurve.ToWeierstrassPoint(edwPoint);
            Assert.Equal(G, recoveredPoint);
        }

        #endregion

        #region Montgomery — Weierstrass Conversions

        [Fact]
        public void Montgomery_ToWeierstrassCurve_ValidCurve_ReturnsWeierstrassCurve()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montyCurve = weiCurve.ToMontgomeryCurve();

            var recoveredCurve = montyCurve.ToWeierstrassCurve();
            Assert.Equal(weiCurve.field, recoveredCurve.field);

            Assert.Equal(weiCurve.order, recoveredCurve.order);
            Assert.Equal(weiCurve.cofactor, recoveredCurve.cofactor);
        }

        [Fact]
        public void Montgomery_ToWeierstrassPoint_ValidPoint_MapsCorrectly()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();

            var montyCurve = weiCurve.ToMontgomeryCurve();
            var montPoint = weiCurve.ToMontgomeryPoint(G);

            var recoveredPoint = montyCurve.ToWeierstrassPoint(montPoint);
            Assert.Equal(G, recoveredPoint);
        }

        [Fact]
        public void Montgomery_ToWeierstrassPoint_Infinity_MapsToInfinity()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montyCurve = weiCurve.ToMontgomeryCurve();
            var point = montyCurve.ToWeierstrassPoint(ECPoint.POINT_INFINITY);
            Assert.Equal(ECPoint.POINT_INFINITY, point);
        }

        [Fact]
        public void Montgomery_ToWeierstrassPoint_TorsionPoint_MapsCorrectly()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montyCurve = weiCurve.ToMontgomeryCurve();
            var torsionPoint = montyCurve.ToWeierstrassPoint(new ECPoint(0, 0));
            Assert.NotEqual(ECPoint.POINT_INFINITY, torsionPoint);
        }

        #endregion

        #region Montgomery — Twisted Edwards Conversions

        [Fact]
        public void Montgomery_ToTwistedEdwardsCurve_ValidCurve_ReturnsTwistedEdwardsCurve()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montyCurve = weiCurve.ToMontgomeryCurve();
            var edwCurve = montyCurve.ToTwistedEdwardsCurve();

            Assert.Equal(weiCurve.field, edwCurve.field);
            Assert.Equal(weiCurve.order, edwCurve.order);
            Assert.Equal(weiCurve.cofactor, edwCurve.cofactor);
        }

        [Fact]
        public void Montgomery_ToTwistedEdwardsPoint_ValidPoint_MapsCorrectly()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();

            var montyPoint = weiCurve.ToMontgomeryPoint(G);
            var montyCurve = weiCurve.ToMontgomeryCurve();

            var edwPoint = montyCurve.ToTwistedEdwardsPoint(montyPoint);
            Assert.NotEqual(ECPoint.POINT_INFINITY, edwPoint);
        }

        [Fact]
        public void Montgomery_ToTwistedEdwardsPoint_Infinity_MapsToInfinity()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montyCurve = weiCurve.ToMontgomeryCurve();
            var edwPoint = montyCurve.ToTwistedEdwardsPoint(ECPoint.POINT_INFINITY);
            Assert.Equal(ECPoint.POINT_INFINITY, edwPoint);
        }

        [Fact]
        public void Montgomery_ToTwistedEdwardsPoint_ExceptionalPoint_ThrowsException()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montyCurve = weiCurve.ToMontgomeryCurve();

            Assert.Throws<ArgumentException>(() =>
                montyCurve.ToTwistedEdwardsPoint(new ECPoint(0, 1)));
        }

        #endregion
    }
}
