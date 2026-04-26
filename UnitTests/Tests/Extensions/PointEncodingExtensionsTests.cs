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

        #region Weierstrass Curve - Decompression Tests

        [Fact]
        public void Weierstrass_DecompressPoint_CompressedRoundTrip_ReturnsOriginalPoint()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);
            var decompressed = curve.DecompressPoint(compressed);

            Assert.Equal(G, decompressed);
        }

        [Fact]
        public void Weierstrass_DecompressPoint_UncompressedRoundTrip_ReturnsOriginalPoint()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var uncompressed = curve.CompressPoint(G, CompressionMode.EC_POINT_UNCOMPRESSED);
            var decompressed = curve.DecompressPoint(uncompressed);

            Assert.Equal(G, decompressed);
        }

        [Fact]
        public void Weierstrass_DecompressPoint_PointWithEvenY_ReturnsCorrectSign()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);
            var decompressed = curve.DecompressPoint(compressed);

            Assert.Equal(G.GetAffineX(), decompressed.GetAffineX());
            Assert.Equal(G.GetAffineY(), decompressed.GetAffineY());

            bool decompressedParityBit = decompressed.GetAffineY().TestBit(0);
            bool originalParityBit = G.GetAffineY().TestBit(0);
            Assert.True(originalParityBit == decompressedParityBit);
        }

        [Fact]
        public void Weierstrass_DecompressPoint_InvalidPrefix_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);

            var invalidBytes = compressed.ToArray();
            invalidBytes[^1] = 0xFF;

            Assert.Throws<ArgumentException>(() =>
                curve.DecompressPoint(invalidBytes));
        }

        [Fact]
        public void Weierstrass_DecompressPoint_EmptyByteArray_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var emptyBytes = Array.Empty<byte>();

            Assert.Throws<ArgumentException>(() =>
                curve.DecompressPoint(emptyBytes));
        }

        [Fact]
        public void Weierstrass_DecompressPoint_NullBytes_ThrowsArgumentNullException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            byte[] nullBytes = null;

            Assert.Throws<ArgumentNullException>(() =>
                curve.DecompressPoint(nullBytes));
        }

        [Fact]
        public void Weierstrass_DecompressPoint_ExcessiveLength_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();
            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);

            var excessiveBytes = compressed.Concat(new byte[] { 0x00, 0x00 }).ToArray();

            Assert.Throws<ArgumentException>(() =>
                curve.DecompressPoint(excessiveBytes));
        }

        #endregion

        #region Twisted Edwards Curve - Compression Tests

        [Fact]
        public void TwistedEdwards_CompressPoint_CompressedMode_ValidPoint_ReturnsCorrectPrefix()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();
            var isXOdd = G.GetAffineX().TestBit(0);

            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);
            var prefix = compressed.Last();

            var expectedPrefix = (byte)(isXOdd ? 3 : 2);
            Assert.Equal(expectedPrefix, prefix);
            Assert.Equal(curve.field.ToByteArray().Length + 1, compressed.Length);
        }

        [Fact]
        public void TwistedEdwards_CompressPoint_UncompressedMode_ValidPoint_ReturnsCorrectLength()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();
            int fieldByteLength = curve.field.ToByteArray().Length;

            var uncompressed = curve.CompressPoint(G, CompressionMode.EC_POINT_UNCOMPRESSED);

            Assert.Equal(4, uncompressed.Last());
            Assert.Equal(2 * fieldByteLength + 1, uncompressed.Length);
        }

        [Fact]
        public void TwistedEdwards_CompressPoint_PointAtInfinity_ThrowsArgumentException()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);
            var inf = ECPoint.POINT_INFINITY;

            Assert.Throws<ArgumentException>(() =>
                curve.CompressPoint(inf, CompressionMode.EC_POINT_COMPRESSED));

            Assert.Throws<ArgumentException>(() =>
                curve.CompressPoint(inf, CompressionMode.EC_POINT_UNCOMPRESSED));
        }

        [Fact]
        public void TwistedEdwards_CompressPoint_NullCurve_ThrowsArgumentNullException()
        {
            TwistedEdwardsCurve nullCurve = null;
            var G = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519).GetBasePoint();

            Assert.Throws<ArgumentNullException>(() =>
                nullCurve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED));
        }

        #endregion

        #region Twisted Edwards Curve - Decompression Tests

        [Fact]
        public void TwistedEdwards_DecompressPoint_CompressedRoundTrip_ReturnsOriginalPoint()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);
            var decompressed = curve.DecompressPoint(compressed);

            Assert.Equal(G, decompressed);
        }

        [Fact]
        public void TwistedEdwards_DecompressPoint_UncompressedRoundTrip_ReturnsOriginalPoint()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var uncompressed = curve.CompressPoint(G, CompressionMode.EC_POINT_UNCOMPRESSED);
            var decompressed = curve.DecompressPoint(uncompressed);

            Assert.Equal(G, decompressed);
        }

        [Fact]
        public void TwistedEdwards_DecompressPoint_PointWithEvenX_ReturnsCorrectSign()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();
            var originalX = G.GetAffineX();

            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);
            var decompressed = curve.DecompressPoint(compressed);

            Assert.Equal(originalX, decompressed.GetAffineX());
            Assert.Equal(G.GetAffineY(), decompressed.GetAffineY());
            bool originalParityBit = G.GetAffineX().TestBit(0);

            bool decompressedParityBit = decompressed.GetAffineX().TestBit(0);
            Assert.True(originalParityBit == decompressedParityBit);
        }

        [Fact]
        public void TwistedEdwards_DecompressPoint_InvalidPrefix_ThrowsArgumentException()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();
            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);

            var invalidBytes = compressed.ToArray();
            invalidBytes[^1] = 0xFF;

            Assert.Throws<ArgumentException>(() =>
                curve.DecompressPoint(invalidBytes));
        }

        [Fact]
        public void TwistedEdwards_DecompressPoint_EmptyByteArray_ThrowsArgumentException()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);
            var emptyBytes = Array.Empty<byte>();

            Assert.Throws<ArgumentException>(() =>
                curve.DecompressPoint(emptyBytes));
        }

        [Fact]
        public void TwistedEdwards_DecompressPoint_NullBytes_ThrowsArgumentNullException()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);
            byte[] nullBytes = null;

            Assert.Throws<ArgumentNullException>(() =>
                curve.DecompressPoint(nullBytes));
        }

        [Fact]
        public void TwistedEdwards_DecompressPoint_ExcessiveLength_ThrowsArgumentException()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();
            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);

            var excessiveBytes = compressed.Concat(new byte[] { 0x00, 0x00 }).ToArray();

            Assert.Throws<ArgumentException>(() =>
                curve.DecompressPoint(excessiveBytes));
        }

        #endregion

        #region Cross-Curve Edge Cases

        [Fact]
        public void Weierstrass_DecompressPoint_EdgeCase_YEqualsZero_CompressesCorrectly()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);
            var decompressed = curve.DecompressPoint(compressed);

            Assert.Equal(G, decompressed);
            Assert.Equal(G.GetAffineY(), decompressed.GetAffineY());
        }

        [Fact]
        public void Weierstrass_CompressDecompress_RandomPoints_AlwaysSucceeds()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            for (int i = 0; i < 10; i++)
            {
                var k = SecureRandom.Range(1, curve.order - 1);
                var point = ECMath.Multiply(curve, k, curve.GetBasePoint(), ECMode.EC_FASTEST);

                var compressed = curve.CompressPoint(point, CompressionMode.EC_POINT_COMPRESSED);
                var decompressed = curve.DecompressPoint(compressed);

                Assert.Equal(point, decompressed);
            }
        }

        [Fact]
        public void TwistedEdwards_CompressDecompress_RandomPoints_AlwaysSucceeds()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards25519);

            for (int i = 0; i < 10; i++)
            {
                var k = SecureRandom.Range(1, curve.order - 1);
                var point = TwistedEdwardsMath.Multiply(curve, k, curve.GetBasePoint(), ECMode.EC_FASTEST);

                var compressed = curve.CompressPoint(point, CompressionMode.EC_POINT_COMPRESSED);
                var decompressed = curve.DecompressPoint(compressed);

                Assert.Equal(point, decompressed);
            }
        }

        [Fact]
        public void Weierstrass_CompressPoint_Wei448_HandlesLargeField()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei448);
            var G = curve.GetBasePoint();

            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);
            var decompressed = curve.DecompressPoint(compressed);

            Assert.Equal(G, decompressed);
            Assert.Equal(curve.field.ToByteArray().Length + 1, compressed.Length);
        }

        [Fact]
        public void TwistedEdwards_CompressPoint_Edwards448_HandlesLargeField()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(TwistedEdwardsCurveType.Edwards448);
            var G = curve.GetBasePoint();

            var compressed = curve.CompressPoint(G, CompressionMode.EC_POINT_COMPRESSED);
            var decompressed = curve.DecompressPoint(compressed);

            Assert.Equal(G, decompressed);
            Assert.Equal(curve.field.ToByteArray().Length + 1, compressed.Length);
        }

        #endregion
    }
}
