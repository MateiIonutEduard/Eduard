using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography.Extensions
{
    /// <summary>
    /// This class provides methods to compress and decompress affine points on the elliptic curve, needed for elliptic curve–based protocols.
    /// </summary>
    public static class PointEncodingExtensions
    {
        /// <summary>
        /// Compresses an affine point on the Weierstrass curve into a byte array.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static byte[] CompressPoint(this EllipticCurve curve, ECPoint point, ECPointCompressionMode mode = ECPointCompressionMode.EC_POINT_COMPRESSED)
        {
            if (point == ECPoint.POINT_INFINITY)
                throw new ArgumentException("Point at infinity cannot be compressed.");

            if(mode == ECPointCompressionMode.EC_POINT_COMPRESSED)
            {
                /* compressed form */
                byte[] bytes = point.GetAffineX().ToByteArray();
                int n = curve.field.ToByteArray().Length;

                byte[] result = new byte[n + 1];
                Array.Copy(bytes, 0, result, 0, bytes.Length);

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

                Array.Copy(xbuffer, 0, buffer, 0, xbuffer.Length);
                Array.Copy(ybuffer, 0, buffer, xbuffer.Length, ybuffer.Length);

                buffer[2 * n] = 4;
                return buffer;
            }
        }

        /// <summary>
        /// Compresses an affine point on the twisted Edwards curve into a byte array.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static byte[] CompressPoint(this TwistedEdwardsCurve curve, ECPoint point, ECPointCompressionMode mode = ECPointCompressionMode.EC_POINT_COMPRESSED)
        {
            if (point == ECPoint.POINT_INFINITY)
                throw new ArgumentException("Point at infinity cannot be compressed.");

            if (mode == ECPointCompressionMode.EC_POINT_COMPRESSED)
            {
                /* compressed form */
                byte[] bytes = point.GetAffineY().ToByteArray();
                int n = curve.field.ToByteArray().Length;

                byte[] result = new byte[n + 1];
                Array.Copy(bytes, 0, result, 0, bytes.Length);

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

                Array.Copy(xbuffer, 0, buffer, 0, xbuffer.Length);
                Array.Copy(ybuffer, 0, buffer, xbuffer.Length, ybuffer.Length);

                buffer[2 * n] = 4;
                return buffer;
            }
        }

        /// <summary>
        /// Decompresses a byte array into an affine point on the Weierstrass curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ECPoint DecompressPoint(this EllipticCurve curve, byte[] bytes)
        {
            if(bytes == null) throw new ArgumentNullException("The byte array cannot be null.");
            int n = curve.field.ToByteArray().Length;

            int index = bytes.Length - 1;
            bool isCompressed = (bytes[index] == 2 || bytes[index] == 3);

            bool valid = isCompressed || (bytes[index] == 4);
            int len = (isCompressed ? n : 2 * n) + 1;

            if (bytes.Length == 0 || bytes.Length > len)
                throw new ArgumentException("Invalid byte array length or corrupted data.");

            /* compressed form of affine point */
            if (bytes[index] == 2 || bytes[index] == 3)
            {
                byte[] data = new byte[bytes.Length - 1];
                Array.Copy(bytes, data, bytes.Length - 1);

                BigInteger Xp = new BigInteger(data);
                BigInteger Yp = curve.Sqrt(curve.Evaluate(Xp), true);

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
                throw new ArgumentException("Invalid encoding of the affine point on the Weierstrass curve.");
            }
        }
    }
}
