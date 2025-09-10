using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography.Extensions
{
    public static class PointEncodingExtensions
    {
        /// <summary>
        /// Compresses an affine point on the Weierstrass curve into a byte array.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static byte[] CompressPoint(this EllipticCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY)
                throw new ArgumentException("Point at infinity cannot be compressed.");

            byte[] bytes = point.GetAffineX().ToByteArray();
            byte[] result = new byte[bytes.Length + 1];

            Array.Copy(bytes, 0, result, 0, bytes.Length);
            int sign = point.GetAffineY().TestBit(0) ? 1 : 0;
            int lastIndex = result.Length - 1;

            result[lastIndex] = (byte)(sign + 2);
            return result;
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

            if (bytes.Length == 0 || bytes.Length > n)
                throw new ArgumentException("Invalid byte array length or corrupted data.");

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
    }
}
