using Eduard.Security.Curves;
using Eduard.Security.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Security.Extensions
{
    /// <summary>
    /// Provides point compression and decompression methods for Weierstrass and twisted Edwards curves, <br/>
    /// following standard encoding formats (compressed with 0x02/0x03 prefix, uncompressed with 0x04).
    /// </summary>
    /// <remarks>
    /// Compressed points store only the x-coordinate (Weierstrass) or y-coordinate (twisted Edwards) <br/>
    /// plus a sign bit, recovering the full point via the curve equation during decompression.
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class PointEncodingExtensions
    {
        /// <summary>
        /// Compresses an affine point on a Weierstrass curve into its byte representation.
        /// </summary>
        /// <param name="curve">The Weierstrass curve containing the point.</param>
        /// <param name="point">The affine point to compress. Cannot be the point at infinity.</param>
        /// <param name="mode">Compression mode. Defaults to <see cref="CompressionMode.EC_POINT_COMPRESSED"/>.</param>
        /// <returns>
        /// Compressed: x-coordinate with trailing prefix 0x02 (y even) or 0x03 (y odd). <br/>
        /// Uncompressed: x-coordinate followed by y-coordinate with trailing prefix 0x04.
        /// </returns>
        /// <exception cref="ArgumentNullException">Curve is null.</exception>
        /// <exception cref="ArgumentException">Point is the point at infinity, which lacks affine coordinates.</exception>
        public static byte[] CompressPoint(this EllipticCurve curve, ECPoint point, CompressionMode mode = CompressionMode.EC_POINT_COMPRESSED)
        {
            if (curve == null)
                throw new ArgumentNullException(
                    nameof(curve), "The elliptic " + 
                    "curve instance cannot be null.");

            if (point == ECPoint.POINT_INFINITY)
                throw new ArgumentException(
                    "The point at infinity cannot be compressed "
                    + "or encoded in affine coordinates.",
                    nameof(point));

            if (mode == CompressionMode.EC_POINT_COMPRESSED)
            {
                /* compressed form */
                byte[] bytes = point.GetAffineX().ToByteArray();
                int n = curve.field.ToByteArray().Length;

                byte[] result = new byte[n + 1];
                int startIndex = n - bytes.Length;

                Array.Copy(bytes, 0, result, startIndex, bytes.Length);
                int sign = point.GetAffineY().TestBit(0) ? 1 : 0;
                int lastIndex = result.Length - 1;

                result[lastIndex] = (byte)(sign + 2);
                return result;
            }
            else
            {
                /* uncompressed form */
                int n = curve.field.ToByteArray().Length;
                byte[] buffer = new byte[2 * n + 1];

                byte[] xbuffer = point.GetAffineX().ToByteArray();
                byte[] ybuffer = point.GetAffineY().ToByteArray();

                int startXIndex = n - xbuffer.Length;
                int startYIndex = n - ybuffer.Length;

                Array.Copy(xbuffer, 0, buffer, startXIndex, xbuffer.Length);
                Array.Copy(ybuffer, 0, buffer, n + startYIndex, ybuffer.Length);

                buffer[2 * n] = 4;
                return buffer;
            }
        }

        /// <summary>
        /// Compresses an affine point on a twisted Edwards curve into its byte representation.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve containing the point.</param>
        /// <param name="point">The affine point to compress. Cannot be the point at infinity.</param>
        /// <param name="mode">Compression mode. Defaults to <see cref="CompressionMode.EC_POINT_COMPRESSED"/>.</param>
        /// <returns>
        /// Compressed: y-coordinate with trailing prefix 0x02 (x even) or 0x03 (x odd). <br/>
        /// Uncompressed: x-coordinate followed by y-coordinate with trailing prefix 0x04.
        /// </returns>
        /// <exception cref="ArgumentNullException">Curve is null.</exception>
        /// <exception cref="ArgumentException">Point is the point at infinity, which lacks affine coordinates.</exception>
        /// <remarks>For twisted Edwards curves, the y-coordinate is stored in compressed form for efficient square root recovery.</remarks>
        public static byte[] CompressPoint(this TwistedEdwardsCurve curve, ECPoint point, CompressionMode mode = CompressionMode.EC_POINT_COMPRESSED)
        {
            if (curve == null)
                throw new ArgumentNullException(nameof(curve), 
                    "The Twisted Edwards curve instance " + 
                    "cannot be null.");

            if (point == ECPoint.POINT_INFINITY)
                throw new ArgumentException("The point at " 
                    + "infinity cannot be compressed or " + 
                    "encoded in affine coordinates.", 
                    nameof(point));

            if (mode == CompressionMode.EC_POINT_COMPRESSED)
            {
                /* compressed form */
                byte[] bytes = point.GetAffineY().ToByteArray();
                int n = curve.field.ToByteArray().Length;

                byte[] result = new byte[n + 1];
                int startIndex = n - bytes.Length;

                Array.Copy(bytes, 0, result, startIndex, bytes.Length);
                int sign = point.GetAffineX().TestBit(0) ? 1 : 0;
                int lastIndex = result.Length - 1;

                result[lastIndex] = (byte)(sign + 2);
                return result;
            }
            else
            {
                /* uncompressed form */
                int n = curve.field.ToByteArray().Length;
                byte[] buffer = new byte[2 * n + 1];

                byte[] xbuffer = point.GetAffineX().ToByteArray();
                byte[] ybuffer = point.GetAffineY().ToByteArray();

                int startXIndex = n - xbuffer.Length;
                int startYIndex = n - ybuffer.Length;

                Array.Copy(xbuffer, 0, buffer, startXIndex, xbuffer.Length);
                Array.Copy(ybuffer, 0, buffer, n + startYIndex, ybuffer.Length);

                buffer[2 * n] = 4;
                return buffer;
            }
        }

        /// <summary>
        /// Decompresses a byte array into an affine point on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve on which the point lies.</param>
        /// <param name="bytes">Encoded byte array with trailing prefix indicating compression type.</param>
        /// <returns>The recovered affine point.</returns>
        /// <exception cref="ArgumentNullException">Curve or byte array is null.</exception>
        /// <exception cref="ArgumentException">
        /// Byte array is empty, exceeds expected length, or contains an invalid prefix byte.
        /// </exception>
        /// <remarks>
        /// Compressed points recover x from y using the curve equation and the sign bit in the prefix. <br/>
        /// Uncompressed points extract both coordinates directly.
        /// </remarks>
        public static ECPoint DecompressPoint(this TwistedEdwardsCurve curve, byte[] bytes)
        {
            if (curve == null)
                throw new ArgumentNullException(nameof(curve), 
                    "The Twisted Edwards curve instance " + 
                    "cannot be null.");

            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes), 
                    "The encoded point byte array cannot be null.");

            if (bytes.Length == 0)
                throw new ArgumentException("The encoded " 
                    + "point byte array cannot be empty.", 
                    nameof(bytes));

            int n = curve.field.ToByteArray().Length;

            int index = bytes.Length - 1;
            bool isCompressed = (bytes[index] == 2 || bytes[index] == 3);

            bool valid = isCompressed || (bytes[index] == 4);
            int len = (isCompressed ? n : 2 * n) + 1;

            if (bytes.Length > len)
                throw new ArgumentException(
                    "Invalid byte array length. Expected at most " + 
                    $"{len} bytes for {(isCompressed ? "compressed" : "uncompressed")} " 
                    + $"encoding, but received {bytes.Length} bytes.", nameof(bytes));

            /* compressed form of affine point */
            if (bytes[index] == 2 || bytes[index] == 3)
            {
                byte[] data = new byte[bytes.Length - 1];
                Array.Copy(bytes, data, bytes.Length - 1);

                BigInteger Yp = new BigInteger(data);
                BigInteger Xp = ModSqrtUtil.Sqrt(curve.Evaluate(Yp), true);

                int sign = bytes[bytes.Length - 1] - 2;
                int x_sign = Xp.TestBit(0) ? 1 : 0;

                BigInteger p = curve.field;
                if (x_sign != sign) Xp = p - Xp;

                return new ECPoint(Xp, Yp);
            }
            else if (bytes[index] == 4)
            {
                /* uncompressed form */
                byte[] xbuffer = new byte[n];

                byte[] ybuffer = new byte[n];
                Array.Copy(bytes, 0, xbuffer, 0, n);

                Array.Copy(bytes, n, ybuffer, 0, n);
                BigInteger Xp = new BigInteger(xbuffer);

                BigInteger Yp = new BigInteger(ybuffer);
                return new ECPoint(Xp, Yp);
            }
            else
            {
                /* invalid encoding of the affine point on the twisted Edwards curve */
                throw new ArgumentException(
                    $"Invalid point encoding prefix byte: 0x{bytes[index]:X2}." 
                    + " Expected 0x02 or 0x03 for compressed format, or 0x04 " 
                    + "for uncompressed format.", nameof(bytes));
            }
        }

        /// <summary>
        /// Decompresses a byte array into an affine point on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The Weierstrass curve on which the point lies.</param>
        /// <param name="bytes">Encoded byte array with trailing prefix indicating compression type.</param>
        /// <returns>The recovered affine point.</returns>
        /// <exception cref="ArgumentNullException">Curve or byte array is null.</exception>
        /// <exception cref="ArgumentException">
        /// Byte array is empty, exceeds expected length, or contains an invalid prefix byte.
        /// </exception>
        /// <remarks>
        /// Compressed points recover y from x using the Weierstrass equation and the sign bit in the prefix. <br/>
        /// Uncompressed points extract both coordinates directly.
        /// </remarks>
        public static ECPoint DecompressPoint(this EllipticCurve curve, byte[] bytes)
        {
            if (curve == null)
                throw new ArgumentNullException(nameof(curve), 
                    "The elliptic curve instance cannot be null.");

            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes), 
                    "The encoded point byte array cannot be null.");

            if (bytes.Length == 0)
                throw new ArgumentException(
                    "The encoded point byte array" 
                    + " cannot be empty.", 
                    nameof(bytes));

            int n = curve.field.ToByteArray().Length;

            int index = bytes.Length - 1;
            bool isCompressed = (bytes[index] == 2 || bytes[index] == 3);

            bool valid = isCompressed || (bytes[index] == 4);
            int len = (isCompressed ? n : 2 * n) + 1;

            if (bytes.Length > len)
                throw new ArgumentException(
                    $"Invalid byte array length. Expected at most " 
                    + $"{len} bytes for {(isCompressed ? "compressed" : "uncompressed")} " 
                    + $"encoding, but received {bytes.Length} bytes.", nameof(bytes));

            /* compressed form of affine point */
            if (bytes[index] == 2 || bytes[index] == 3)
            {
                byte[] data = new byte[bytes.Length - 1];
                Array.Copy(bytes, data, bytes.Length - 1);

                BigInteger Xp = new BigInteger(data);
                BigInteger Yp = ModSqrtUtil.Sqrt(curve.Evaluate(Xp), true);

                int sign = bytes[bytes.Length - 1] - 2;
                int y_sign = Yp.TestBit(0) ? 1 : 0;

                BigInteger p = curve.field;
                if (y_sign != sign) Yp = p - Yp;

                return new ECPoint(Xp, Yp);
            }
            else if (bytes[index] == 4)
            {
                /* uncompressed form */
                byte[] xbuffer = new byte[n];

                byte[] ybuffer = new byte[n];
                Array.Copy(bytes, 0, xbuffer, 0, n);

                Array.Copy(bytes, n, ybuffer, 0, n);
                BigInteger Xp = new BigInteger(xbuffer);

                BigInteger Yp = new BigInteger(ybuffer);
                return new ECPoint(Xp, Yp);
            }
            else
            {
                /* invalid encoding of the affine point on the elliptic curve */
                throw new ArgumentException(
                    $"Invalid point encoding prefix byte: "
                    + $"0x{bytes[index]:X2}. Expected 0x02 or "
                    + "0x03 for compressed format, or 0x04 for" 
                    + " uncompressed format.", nameof(bytes));
            }
        }
    }
}
