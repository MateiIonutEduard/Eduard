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
        public void Montgomery_ToWeierstrassCurve_InvalidParameters_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                /* tiny JubJub curve in Montgomery form */
                var montyCurve = new MontgomeryCurve(6, 7, 13, 5, 4);

                /* externally mutate parameter B */
                montyCurve.B = 0;

                montyCurve.ToWeierstrassCurve();
            });
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

        #region Twisted Edwards — Weierstrass Conversions

        [Fact]
        public void TwistedEdwards_ToWeierstrassCurve_ValidCurve_ReturnsWeierstrassCurve()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edwCurve = weiCurve.ToTwistedEdwardsCurve();

            var recoveredCurve = edwCurve.ToWeierstrassCurve();
            Assert.Equal(weiCurve.field, recoveredCurve.field);

            Assert.Equal(weiCurve.order, recoveredCurve.order);
            Assert.Equal(weiCurve.cofactor, recoveredCurve.cofactor);
        }

        [Fact]
        public void TwistedEdwards_ToWeierstrassCurve_InvalidParameters_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => {
                /* tiny JubJub curve in twisted Edwards form */
                var edwardsCurve = new TwistedEdwardsCurve(3, 8, 13, 5, 4);

                /* intentionally change parameter a */
                edwardsCurve.a = 0;

                edwardsCurve.ToWeierstrassCurve();
            });
        }

        [Fact]
        public void TwistedEdwards_ToWeierstrassPoint_ValidPoint_MapsCorrectly()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();

            var edwCurve = weiCurve.ToTwistedEdwardsCurve();
            var edwPoint = weiCurve.ToTwistedEdwardsPoint(G);

            var recoveredPoint = edwCurve.ToWeierstrassPoint(edwPoint);
            Assert.Equal(G, recoveredPoint);
        }

        [Fact]
        public void TwistedEdwards_ToWeierstrassPoint_Identity_MapsToInfinity()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edwCurve = weiCurve.ToTwistedEdwardsCurve();
            var point = edwCurve.ToWeierstrassPoint(new ECPoint(0, 1));
            Assert.Equal(ECPoint.POINT_INFINITY, point);
        }

        [Fact]
        public void TwistedEdwards_ToWeierstrassPoint_ExceptionalPoint_ThrowsException()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edwCurve = weiCurve.ToTwistedEdwardsCurve();

            Assert.Throws<ArgumentException>(() =>
                edwCurve.ToWeierstrassPoint(new ECPoint(0, 2)));
        }

        #endregion

        #region Twisted Edwards — Montgomery Conversions

        [Fact]
        public void TwistedEdwards_ToMontgomeryCurve_ValidCurve_ReturnsMontgomeryCurve()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edwCurve = weiCurve.ToTwistedEdwardsCurve();

            var montyCurve = edwCurve.ToMontgomeryCurve();
            Assert.Equal(weiCurve.field, montyCurve.field);

            Assert.Equal(weiCurve.order, montyCurve.order);
            Assert.Equal(weiCurve.cofactor, montyCurve.cofactor);
        }

        [Fact]
        public void TwistedEdwards_ToMontgomeryPoint_ValidPoint_MapsCorrectly()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();

            var edwPoint = weiCurve.ToTwistedEdwardsPoint(G);
            var edwCurve = weiCurve.ToTwistedEdwardsCurve();

            var montyPoint = edwCurve.ToMontgomeryPoint(edwPoint);
            Assert.NotEqual(ECPoint.POINT_INFINITY, montyPoint);
        }

        [Fact]
        public void TwistedEdwards_ToMontgomeryPoint_Identity_MapsToInfinity()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edwCurve = weiCurve.ToTwistedEdwardsCurve();
            var montyPoint = edwCurve.ToMontgomeryPoint(new ECPoint(0, 1));
            Assert.Equal(ECPoint.POINT_INFINITY, montyPoint);
        }

        [Fact]
        public void TwistedEdwards_ToMontgomeryPoint_ExceptionalPoint_ThrowsException()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edwCurve = weiCurve.ToTwistedEdwardsCurve();

            Assert.Throws<ArgumentException>(() =>
                edwCurve.ToMontgomeryPoint(new ECPoint(0, 2)));
        }

        #endregion

        #region Cross-Conversion Round-Trip Tests

        [Fact]
        public void RoundTrip_WeierstrassToMontgomeryToWeierstrass_PreservesCurve()
        {
            var original = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var montyCurve = original.ToMontgomeryCurve();

            var recovered = montyCurve.ToWeierstrassCurve();
            Assert.Equal(original.field, recovered.field);

            Assert.Equal(original.order, recovered.order);
            Assert.Equal(original.cofactor, recovered.cofactor);
        }

        [Fact]
        public void RoundTrip_WeierstrassToTwistedEdwardsToWeierstrass_PreservesCurve()
        {
            var original = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edwardsCurve = original.ToTwistedEdwardsCurve();

            var recovered = edwardsCurve.ToWeierstrassCurve();
            Assert.Equal(original.field, recovered.field);

            Assert.Equal(original.order, recovered.order);
            Assert.Equal(original.cofactor, recovered.cofactor);
        }

        [Fact]
        public void RoundTrip_MontgomeryToTwistedEdwardsToMontgomery_PreservesCurve()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var original = weiCurve.ToMontgomeryCurve();

            var edwardsCurve = original.ToTwistedEdwardsCurve();
            var recovered = edwardsCurve.ToMontgomeryCurve();

            Assert.Equal(original.field, recovered.field);
            Assert.Equal(original.order, recovered.order);
            Assert.Equal(original.cofactor, recovered.cofactor);

            Assert.Equal(original.A, recovered.A);
            Assert.Equal(original.B, recovered.B);
        }

        [Fact]
        public void RoundTrip_TwistedEdwardsToMontgomeryToTwistedEdwards_PreservesCurve()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var original = weiCurve.ToTwistedEdwardsCurve();
            var montyCurve = original.ToMontgomeryCurve();

            var recovered = montyCurve.ToTwistedEdwardsCurve();
            Assert.Equal(original.field, recovered.field);

            Assert.Equal(original.order, recovered.order);
            Assert.Equal(original.cofactor, recovered.cofactor);
        }

        #endregion

        #region Point Mapping Consistency Tests

        [Fact]
        public void PointMapping_WeierstrassToMontgomeryToTwistedEdwards_Consistent()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();

            var directEdwPoint = weiCurve.ToTwistedEdwardsPoint(G);
            var montyPoint = weiCurve.ToMontgomeryPoint(G);
            var montyCurve = weiCurve.ToMontgomeryCurve();

            var indirectEdPoint = montyCurve.ToTwistedEdwardsPoint(montyPoint);
            Assert.Equal(directEdwPoint, indirectEdPoint);
        }

        [Fact]
        public void PointMapping_TwistedEdwardsToMontgomeryToWeierstrass_Consistent()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = weiCurve.GetBasePoint();

            var edwCurve = weiCurve.ToTwistedEdwardsCurve();
            var edwPoint = weiCurve.ToTwistedEdwardsPoint(G);

            var directWeiPoint = edwCurve.ToWeierstrassPoint(edwPoint);
            var montyPoint = edwCurve.ToMontgomeryPoint(edwPoint);
            var montyCurve = edwCurve.ToMontgomeryCurve();

            var indirectWeiPoint = montyCurve.ToWeierstrassPoint(montyPoint);
            Assert.Equal(directWeiPoint, indirectWeiPoint);
        }

        #endregion

        #region Edge Cases and Validation

        [Fact]
        public void Weierstrass_ToMontgomeryPoint_MultipleRandomPoints_AllMapCorrectly()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);

            for (int i = 0; i < 10; i++)
            {
                var k = SecureRandom.Range(1, weiCurve.order - 1);
                var point = ECMath.Multiply(weiCurve, k, weiCurve.GetBasePoint(), ECMode.EC_FASTEST);

                var montyPoint = weiCurve.ToMontgomeryPoint(point);
                var montyCurve = weiCurve.ToMontgomeryCurve();

                var recovered = montyCurve.ToWeierstrassPoint(montyPoint);
                Assert.Equal(point, recovered);
            }
        }

        [Fact]
        public void TwistedEdwards_ToWeierstrassPoint_MultipleRandomPoints_AllMapCorrectly()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var edwCurve = weiCurve.ToTwistedEdwardsCurve();

            for (int i = 0; i < 10; i++)
            {
                var k = SecureRandom.Range(1, edwCurve.order - 1);
                var point = TwistedEdwardsMath.Multiply(edwCurve, k, 
                    edwCurve.GetBasePoint(), ECMode.EC_FASTEST);

                var weiPoint = edwCurve.ToWeierstrassPoint(point);
                var recovered = weiCurve.ToTwistedEdwardsPoint(weiPoint);
                Assert.Equal(point, recovered);
            }
        }

        [Fact]
        public void Montgomery_ToTwistedEdwardsPoint_WithWei448_HandlesLargeField()
        {
            var weiCurve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei448);
            var montyCurve = weiCurve.ToMontgomeryCurve();

            var G = weiCurve.GetBasePoint();
            var montyPoint = weiCurve.ToMontgomeryPoint(G);

            var edwPoint = montyCurve.ToTwistedEdwardsPoint(montyPoint);
            Assert.NotEqual(ECPoint.POINT_INFINITY, edwPoint);
        }

        #endregion
    }
}
