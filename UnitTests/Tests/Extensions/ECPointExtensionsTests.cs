using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Extensions
{
    [Collection("Sequential")]
    public class ECPointExtensionsTests
    {
        #region Weierstrass Curve — Affine to Jacobian Conversions

        [Fact]
        public void ToJacobian_FromAffine_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var jacPoint = curve.ToJacobian(G);
            Assert.NotEqual(ECPoint3w.POINT_INFINITY, jacPoint);
            Assert.Equal(G.GetAffineX(), jacPoint.x);

            Assert.Equal(G.GetAffineY(), jacPoint.y);
            Assert.Equal(1, jacPoint.z);
        }

        [Fact]
        public void ToJacobian_FromAffineInfinity_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint.POINT_INFINITY;
            var jacPoint = curve.ToJacobian(inf);
            Assert.Equal(ECPoint3w.POINT_INFINITY, jacPoint);
        }

        [Fact]
        public void ToAffine_FromJacobian_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var jacPoint = curve.ToJacobian(G);

            var affinePoint = curve.ToAffine(jacPoint);
            Assert.Equal(G, affinePoint);
        }

        [Fact]
        public void ToAffine_FromJacobianInfinity_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint3w.POINT_INFINITY;
            var affinePoint = curve.ToAffine(inf);
            Assert.Equal(ECPoint.POINT_INFINITY, affinePoint);
        }

        [Fact]
        public void ToAffine_FromJacobianWithZeroZ_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var jacPoint = curve.ToJacobian(G);

            /* create invalid point with Z = 0 */
            var invalidPoint = new ECPoint3w(jacPoint.x, jacPoint.y, 0);
            var affinePoint = curve.ToAffine(invalidPoint);
            Assert.Equal(ECPoint.POINT_INFINITY, affinePoint);
        }

        #endregion

        #region Weierstrass Curve — Affine to Modified Jacobian Conversions

        [Fact]
        public void ToModifiedJacobian_FromAffine_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var modJacPoint = curve.ToModifiedJacobian(G);
            Assert.NotEqual(ECPoint4w.POINT_INFINITY, modJacPoint);

            Assert.Equal(G.GetAffineX(), modJacPoint.x);
            Assert.Equal(G.GetAffineY(), modJacPoint.y);

            Assert.Equal(1, modJacPoint.z);
            Assert.Equal(curve.a, modJacPoint.aZ4);
        }

        [Fact]
        public void ToModifiedJacobian_FromAffineInfinity_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint.POINT_INFINITY;
            var modJacPoint = curve.ToModifiedJacobian(inf);
            Assert.Equal(ECPoint4w.POINT_INFINITY, modJacPoint);
        }

        [Fact]
        public void ToAffine_FromModifiedJacobian_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var modJacPoint = curve.ToModifiedJacobian(G);
            var affinePoint = curve.ToAffine(modJacPoint);
            Assert.Equal(G, affinePoint);
        }

        [Fact]
        public void ToAffine_FromModifiedJacobianInfinity_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint4w.POINT_INFINITY;
            var affinePoint = curve.ToAffine(inf);
            Assert.Equal(ECPoint.POINT_INFINITY, affinePoint);
        }

        [Fact]
        public void ToAffine_FromModifiedJacobianWithZeroZ_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var modJacPoint = curve.ToModifiedJacobian(G);

            /* test security check inside the constructor */
            Assert.Throws<InvalidOperationException>(() =>
            {
                var invalidPoint = new ECPoint4w(modJacPoint.x,
                    modJacPoint.y, 0, modJacPoint.aZ4);
            });

            /* mutate data outside of struct */
            var invalidPoint = new ECPoint4w(modJacPoint.x,
                modJacPoint.y, modJacPoint.z, modJacPoint.aZ4);
            invalidPoint.z = 0;

            /* throw it successfully */
            Assert.Throws<InvalidOperationException>(() => 
                curve.ToAffine(invalidPoint));

            /* mutate again to fit point at infinity */
            invalidPoint.aZ4 = 0;
            Assert.Equal(invalidPoint, 
                ECPoint4w.POINT_INFINITY);
        }

        #endregion

        #region Weierstrass Curve — Affine to Jacobian-Chudnovsky Conversions

        [Fact]
        public void ToJacobianChudnovsky_FromAffine_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var jcPoint = curve.ToJacobianChudnovsky(G);
            Assert.NotEqual(ECPoint5w.POINT_INFINITY, jcPoint);

            Assert.Equal(G.GetAffineX(), jcPoint.x);
            Assert.Equal(G.GetAffineY(), jcPoint.y);

            Assert.Equal(1, jcPoint.z);
            Assert.Equal(1, jcPoint.z2);
            Assert.Equal(1, jcPoint.z3);
        }

        [Fact]
        public void ToJacobianChudnovsky_FromAffineInfinity_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint.POINT_INFINITY;
            var jcPoint = curve.ToJacobianChudnovsky(inf);
            Assert.Equal(ECPoint5w.POINT_INFINITY, jcPoint);
        }

        [Fact]
        public void ToAffine_FromJacobianChudnovsky_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var jcPoint = curve.ToJacobianChudnovsky(G);
            var affinePoint = curve.ToAffine(jcPoint);
            Assert.Equal(G, affinePoint);
        }

        [Fact]
        public void ToAffine_FromJacobianChudnovskyInfinity_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint5w.POINT_INFINITY;
            var affinePoint = curve.ToAffine(inf);
            Assert.Equal(ECPoint.POINT_INFINITY, affinePoint);
        }

        [Fact]
        public void ToAffine_FromJacobianChudnovskyWithZeroZ_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var jcPoint = curve.ToJacobianChudnovsky(G);

            /* security checks inside constructor */
            Assert.Throws<InvalidOperationException>(() => {
                var invalidPoint = new ECPoint5w(jcPoint.x,
                    jcPoint.y, 0, jcPoint.z2, jcPoint.z3);           
            });

            /* struct mutation outside of constructor */
            Assert.Throws<InvalidOperationException>(() =>
            {
                jcPoint.z = 0;
                var affinePoint = curve.ToAffine(jcPoint);
            });

            var validPointInfinity = new ECPoint5w(
                jcPoint.x, jcPoint.y, 0, 0, 0);

            var affinePoint = curve.ToAffine(validPointInfinity);
            Assert.True(affinePoint == ECPoint.POINT_INFINITY);
        }

        #endregion

        #region Weierstrass Curve — Cross Coordinate System Conversions

        [Fact]
        public void ToJacobian_FromModifiedJacobian_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var modJacPoint = curve.ToModifiedJacobian(G);
            var jacPoint = curve.ToJacobian(modJacPoint);

            Assert.NotEqual(ECPoint3w.POINT_INFINITY, jacPoint);
            Assert.Equal(modJacPoint.x, jacPoint.x);

            Assert.Equal(modJacPoint.y, jacPoint.y);
            Assert.Equal(modJacPoint.z, jacPoint.z);
        }

        [Fact]
        public void ToJacobian_FromModifiedJacobianInfinity_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint4w.POINT_INFINITY;
            var jacPoint = curve.ToJacobian(inf);
            Assert.Equal(ECPoint3w.POINT_INFINITY, jacPoint);
        }

        [Fact]
        public void ToJacobian_FromJacobianChudnovsky_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var jcPoint = curve.ToJacobianChudnovsky(G);
            var jacPoint = curve.ToJacobian(jcPoint);

            Assert.NotEqual(ECPoint3w.POINT_INFINITY, jacPoint);
            Assert.Equal(jcPoint.x, jacPoint.x);

            Assert.Equal(jcPoint.y, jacPoint.y);
            Assert.Equal(jcPoint.z, jacPoint.z);
        }

        [Fact]
        public void ToJacobian_FromJacobianChudnovskyInfinity_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint5w.POINT_INFINITY;
            var jacPoint = curve.ToJacobian(inf);
            Assert.Equal(ECPoint3w.POINT_INFINITY, jacPoint);
        }

        [Fact]
        public void ToModifiedJacobian_FromJacobian_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var jacPoint = curve.ToJacobian(G);
            var modJacPoint = curve.ToModifiedJacobian(jacPoint);
            BigInteger p = curve.field;

            Assert.NotEqual(ECPoint4w.POINT_INFINITY, modJacPoint);
            Assert.Equal(jacPoint.x, modJacPoint.x);
            Assert.Equal(jacPoint.y, modJacPoint.y);
            Assert.Equal(jacPoint.z, modJacPoint.z);

            /* verify aZ^4 is correctly computed */
            var expectedZ2 = (jacPoint.z * jacPoint.z) % p;
            var expectedZ4 = (expectedZ2 * expectedZ2) % p;
            var expectedAZ4 = (curve.a * expectedZ4) % p;
            Assert.Equal(expectedAZ4, modJacPoint.aZ4);
        }

        [Fact]
        public void ToModifiedJacobian_FromJacobianInfinity_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint3w.POINT_INFINITY;
            var modJacPoint = curve.ToModifiedJacobian(inf);
            Assert.Equal(ECPoint4w.POINT_INFINITY, modJacPoint);
        }

        [Fact]
        public void ToModifiedJacobian_FromJacobianChudnovsky_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var jcPoint = curve.ToJacobianChudnovsky(G);
            var modJacPoint = curve.ToModifiedJacobian(jcPoint);
            BigInteger p = curve.field;

            Assert.NotEqual(ECPoint4w.POINT_INFINITY, modJacPoint);
            Assert.Equal(jcPoint.x, modJacPoint.x);
            Assert.Equal(jcPoint.y, modJacPoint.y);
            Assert.Equal(jcPoint.z, modJacPoint.z);

            /* verify aZ^4 = a * (Z^2)^2 using precomputed Z^2 */
            var expectedZ4 = (jcPoint.z2 * jcPoint.z2) % p;
            var expectedAZ4 = (curve.a * expectedZ4) % p;
            Assert.Equal(expectedAZ4, modJacPoint.aZ4);
        }

        [Fact]
        public void ToModifiedJacobian_FromJacobianChudnovskyInfinity_ReturnsInfinity()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint5w.POINT_INFINITY;
            var modJacPoint = curve.ToModifiedJacobian(inf);
            Assert.Equal(ECPoint4w.POINT_INFINITY, modJacPoint);
        }

        #endregion
    }
}
