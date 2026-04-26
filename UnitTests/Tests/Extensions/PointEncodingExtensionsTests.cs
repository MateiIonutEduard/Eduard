using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Extensions
{
    [Collection("Sequential")]
    public class PointEncodingExtensionsTests
    {
        #region Weierstrass Curve - Compression Tests

        [Fact]
        public void Weierstrass_CompressPoint_CompressedMode_ValidPoint_ReturnsCorrectPrefix()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var isYOdd = G.GetAffineY().TestBit(0);

            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);
            var prefix = compressed.Last();

            var expectedPrefix = (byte)(isYOdd ? 3 : 2);
            Assert.Equal(expectedPrefix, prefix);
            Assert.Equal(curve.field.ToByteArray().Length + 1, compressed.Length);
        }

        [Fact]
        public void Weierstrass_CompressPoint_UncompressedMode_ValidPoint_ReturnsCorrectLength()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            int fieldByteLength = curve.field.ToByteArray().Length;

            var uncompressed = curve.CompressPoint(G, CompressionMode.EC_POINT_UNCOMPRESSED);
            var prefix = uncompressed.Last();

            Assert.Equal(4, prefix);
            Assert.Equal(2 * fieldByteLength + 1, uncompressed.Length);
        }

        [Fact]
        public void Weierstrass_CompressPoint_PointAtInfinity_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var inf = ECPoint.POINT_INFINITY;

            Assert.Throws<ArgumentException>(() =>
                curve.CompressPoint(inf, CompressionMode.EC_POINT_COMPRESSED));

            Assert.Throws<ArgumentException>(() =>
                curve.CompressPoint(inf, CompressionMode.EC_POINT_UNCOMPRESSED));
        }

        [Fact]
        public void Weierstrass_CompressPoint_NullCurve_ThrowsArgumentNullException()
        {
            EllipticCurve nullCurve = null;
            var G = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256).GetBasePoint();

            Assert.Throws<ArgumentNullException>(() =>
                nullCurve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED));
        }

        #endregion
    }
}
