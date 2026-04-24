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

        #region Twisted Edwards Curve — Affine to Projective Conversions

        [Fact]
        public void ToProjective_FromAffine_ConvertsCorrectly()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();
            var projPoint = curve.ToProjective(G);

            Assert.NotEqual(ECPoint3.POINT_INFINITY, projPoint);
            Assert.Equal(G.GetAffineX(), projPoint.x);

            Assert.Equal(G.GetAffineY(), projPoint.y);
            Assert.Equal(1, projPoint.z);
        }

        [Fact]
        public void ToProjective_FromAffineInfinity_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var inf = ECPoint.POINT_INFINITY;
            var projPoint = curve.ToProjective(inf);
            Assert.Equal(ECPoint3.POINT_INFINITY, projPoint);
        }

        [Fact]
        public void ToProjective_FromAffineIdentity_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var identity = new ECPoint(0, 1);
            var projPoint = curve.ToProjective(identity);
            Assert.Equal(ECPoint3.POINT_INFINITY, projPoint);
        }

        [Fact]
        public void ToAffine_FromProjective_ConvertsCorrectly()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var projPoint = curve.ToProjective(G);
            var affinePoint = curve.ToAffine(projPoint);
            Assert.Equal(G, affinePoint);
        }

        [Fact]
        public void ToAffine_FromProjectiveInfinity_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var inf = ECPoint3.POINT_INFINITY;
            var affinePoint = curve.ToAffine(inf);
            Assert.Equal(ECPoint.POINT_INFINITY, affinePoint);
        }

        [Fact]
        public void ToAffine_FromProjectiveWithZeroZ_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();
            var projPoint = curve.ToProjective(G);

            var invalidPoint = new ECPoint3(projPoint.x, projPoint.y, 0);
            var affinePoint = curve.ToAffine(invalidPoint);
            Assert.Equal(ECPoint.POINT_INFINITY, affinePoint);
        }

        #endregion

        #region Twisted Edwards Curve — Affine to Extended Projective Conversions

        [Fact]
        public void ToExtendedProjective_FromAffine_ConvertsCorrectly()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var extPoint = curve.ToExtendedProjective(G);
            BigInteger p = curve.field;

            Assert.NotEqual(ECPoint4.POINT_INFINITY, extPoint);
            Assert.Equal(G.GetAffineX(), extPoint.x);

            Assert.Equal(G.GetAffineY(), extPoint.y);
            Assert.Equal(1, extPoint.z);

            var expectedT = (G.GetAffineX() * G.GetAffineY()) % p;
            Assert.Equal(expectedT, extPoint.t);
        }

        [Fact]
        public void ToExtendedProjective_FromAffineInfinity_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var inf = ECPoint.POINT_INFINITY;
            var extPoint = curve.ToExtendedProjective(inf);
            Assert.Equal(ECPoint4.POINT_INFINITY, extPoint);
        }

        [Fact]
        public void ToExtendedProjective_FromAffineIdentity_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var identity = new ECPoint(0, 1);
            var extPoint = curve.ToExtendedProjective(identity);
            Assert.Equal(ECPoint4.POINT_INFINITY, extPoint);
        }

        [Fact]
        public void ToAffine_FromExtendedProjective_ConvertsCorrectly()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var extPoint = curve.ToExtendedProjective(G);
            var affinePoint = curve.ToAffine(extPoint);
            Assert.Equal(G, affinePoint);
        }

        [Fact]
        public void ToAffine_FromExtendedProjectiveInfinity_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var inf = ECPoint4.POINT_INFINITY;
            var affinePoint = curve.ToAffine(inf);
            Assert.Equal(ECPoint.POINT_INFINITY, affinePoint);
        }

        [Fact]
        public void ToAffine_FromExtendedProjectiveWithZeroZ_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();
            var extPoint = curve.ToExtendedProjective(G);

            /* test invalid point init via constructor */
            Assert.Throws<InvalidOperationException>(() =>
            {
                var invalidPoint = new ECPoint4(extPoint.x, 
                    extPoint.y, extPoint.t, 0);
            });

            var invalidPoint = new ECPoint4(extPoint.x, 
                extPoint.y, extPoint.t, extPoint.z);

            /* mutate z coordinate externally */
            invalidPoint.z = 0;

            Assert.Throws<InvalidOperationException>(() => 
                curve.ToAffine(invalidPoint));

            var infinity = new ECPoint4(extPoint.x,
                extPoint.y, 0, 0);

            /* right representation for point at infinity */
            var affinePoint = curve.ToAffine(infinity);
            Assert.Equal(affinePoint, ECPoint.POINT_INFINITY);
        }

        #endregion

        #region Twisted Edwards Curve — Cross Coordinate System Conversions

        [Fact]
        public void ToExtendedProjective_FromProjective_ConvertsCorrectly()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var projPoint = curve.ToProjective(G);
            BigInteger p = curve.field;

            var extPoint = curve.ToExtendedProjective(projPoint);
            Assert.NotEqual(ECPoint4.POINT_INFINITY, extPoint);

            /* extended coordinates should be (XZ, YZ, XY, Z^2) */
            var expectedX = (projPoint.x * projPoint.z) % p;
            var expectedY = (projPoint.y * projPoint.z) % p;
            var expectedT = (projPoint.x * projPoint.y) % p;
            var expectedZ = (projPoint.z * projPoint.z) % p;

            Assert.Equal(expectedX, extPoint.x);
            Assert.Equal(expectedY, extPoint.y);
            Assert.Equal(expectedT, extPoint.t);
            Assert.Equal(expectedZ, extPoint.z);
        }

        [Fact]
        public void ToExtendedProjective_FromProjectiveInfinity_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var inf = ECPoint3.POINT_INFINITY;
            var extPoint = curve.ToExtendedProjective(inf);
            Assert.Equal(ECPoint4.POINT_INFINITY, extPoint);
        }

        [Fact]
        public void GetPointCopy_FromProjective_ConvertsCorrectly()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var projPoint = curve.ToProjective(G);
            var copyPoint = curve.GetPointCopy(projPoint);
            Assert.NotEqual(ECPoint4.POINT_INFINITY, copyPoint);

            Assert.Equal(projPoint.x, copyPoint.x);
            Assert.Equal(projPoint.y, copyPoint.y);
            Assert.Equal(projPoint.z, copyPoint.z);
            Assert.Equal(0, copyPoint.t);
        }

        [Fact]
        public void GetPointCopy_FromProjectiveInfinity_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var inf = ECPoint3.POINT_INFINITY;
            var copyPoint = curve.GetPointCopy(inf);
            Assert.Equal(ECPoint4.POINT_INFINITY, copyPoint);
        }

        [Fact]
        public void ToProjective_FromExtendedProjective_ConvertsCorrectly()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var extPoint = curve.ToExtendedProjective(G);
            var projPoint = curve.ToProjective(extPoint);
            Assert.NotEqual(ECPoint3.POINT_INFINITY, projPoint);

            Assert.Equal(extPoint.x, projPoint.x);
            Assert.Equal(extPoint.y, projPoint.y);
            Assert.Equal(extPoint.z, projPoint.z);
        }

        [Fact]
        public void ToProjective_FromExtendedProjectiveInfinity_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var inf = ECPoint4.POINT_INFINITY;
            var projPoint = curve.ToProjective(inf);
            Assert.Equal(ECPoint3.POINT_INFINITY, projPoint);
        }

        #endregion

        #region Round-Trip Conversion Tests (Weierstrass)

        [Fact]
        public void RoundTrip_AffineToJacobianToAffine_PreservesPoint()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var jacPoint = curve.ToJacobian(G);
            var recoveredPoint = curve.ToAffine(jacPoint);
            Assert.Equal(G, recoveredPoint);
        }

        [Fact]
        public void RoundTrip_AffineToModifiedJacobianToAffine_PreservesPoint()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var modJacPoint = curve.ToModifiedJacobian(G);
            var recoveredPoint = curve.ToAffine(modJacPoint);
            Assert.Equal(G, recoveredPoint);
        }

        [Fact]
        public void RoundTrip_AffineToJacobianChudnovskyToAffine_PreservesPoint()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var jcPoint = curve.ToJacobianChudnovsky(G);
            var recoveredPoint = curve.ToAffine(jcPoint);
            Assert.Equal(G, recoveredPoint);
        }

        [Fact]
        public void RoundTrip_JacobianToModifiedJacobianToJacobian_PreservesCoordinates()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var jacPoint = curve.ToJacobian(G);

            var modJacPoint = curve.ToModifiedJacobian(jacPoint);
            var recoveredJacPoint = curve.ToJacobian(modJacPoint);

            Assert.Equal(jacPoint.x, recoveredJacPoint.x);
            Assert.Equal(jacPoint.y, recoveredJacPoint.y);
            Assert.Equal(jacPoint.z, recoveredJacPoint.z);
        }

        [Fact]
        public void RoundTrip_JacobianChudnovskyToModifiedJacobianToAffine_PreservesPoint()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var jcPoint = curve.ToJacobianChudnovsky(G);

            var modJacPoint = curve.ToModifiedJacobian(jcPoint);
            var recoveredPoint = curve.ToAffine(modJacPoint);
            Assert.Equal(G, recoveredPoint);
        }

        #endregion

        #region Round-Trip Conversion Tests (Twisted Edwards)

        [Fact]
        public void RoundTrip_AffineToProjectiveToAffine_PreservesPoint()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var projPoint = curve.ToProjective(G);
            var recoveredPoint = curve.ToAffine(projPoint);
            Assert.Equal(G, recoveredPoint);
        }

        [Fact]
        public void RoundTrip_AffineToExtendedProjectiveToAffine_PreservesPoint()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var extPoint = curve.ToExtendedProjective(G);
            var recoveredPoint = curve.ToAffine(extPoint);
            Assert.Equal(G, recoveredPoint);
        }

        [Fact]
        public void RoundTrip_ProjectiveToExtendedProjectiveToProjective_PreservesCoordinates()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();
            var projPoint = curve.ToProjective(G);

            var extPoint = curve.ToExtendedProjective(projPoint);
            var recoveredProjPoint = curve.ToProjective(extPoint);

            Assert.Equal(projPoint.x, recoveredProjPoint.x);
            Assert.Equal(projPoint.y, recoveredProjPoint.y);
            Assert.Equal(projPoint.z, recoveredProjPoint.z);
        }

        #endregion

        #region Edge Cases and Validation

        [Fact]
        public void ToAffine_FromExtendedProjective_IdentityPoint_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);

            /* identity in extended coordinates: (0, 1, 0, 1) */
            var identityExt = new ECPoint4(0, 1, 0, 1);
            var affinePoint = curve.ToAffine(identityExt);
            Assert.Equal(ECPoint.POINT_INFINITY, affinePoint);
        }

        [Fact]
        public void ToAffine_FromProjective_IdentityPoint_ReturnsInfinity()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);

            /* identity in projective coordinates: (0, 1, 1) */
            var identityProj = new ECPoint3(0, 1, 1);
            var affinePoint = curve.ToAffine(identityProj);
            Assert.Equal(ECPoint.POINT_INFINITY, affinePoint);
        }

        [Fact]
        public void ToExtendedProjective_FromProjective_WithScaledCoordinates_ConvertsCorrectly()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            BigInteger p = curve.field;
            BigInteger lambda = 42;

            /* scale projective coordinates by lambda */
            var projPoint = curve.ToProjective(G);
            var scaledProjPoint = new ECPoint3(
                (projPoint.x * lambda) % p,
                (projPoint.y * lambda) % p,
                (projPoint.z * lambda) % p
            );

            var extPoint = curve.ToExtendedProjective(scaledProjPoint);
            var recoveredAffine = curve.ToAffine(extPoint);

            /* scaled coordinates should represent the same affine point */
            Assert.Equal(G, recoveredAffine);
        }

        [Fact]
        public void ToJacobian_FromModifiedJacobian_WithScaledCoordinates_ConvertsCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var modJacPoint = curve.ToModifiedJacobian(G);
            BigInteger p = curve.field, lambda = 7; 

            /* scale modified Jacobian coordinates by lambda */
            var lambda2 = (lambda * lambda) % p;
            var lambda3 = (lambda2 * lambda) % p;
            var lambda4 = (lambda2 * lambda2) % p;

            var scaledPoint = new ECPoint4w(
                (modJacPoint.x * lambda2) % p,
                (modJacPoint.y * lambda3) % p,
                (modJacPoint.z * lambda) % p,
                (modJacPoint.aZ4 * lambda4) % p
            );

            var jacPoint = curve.ToJacobian(scaledPoint);
            var recoveredAffine = curve.ToAffine(jacPoint);

            /* scaled coordinates should represent the same affine point */
            Assert.Equal(G, recoveredAffine);
        }

        #endregion
    }
}
