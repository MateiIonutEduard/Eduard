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
                throw new ArgumentException("Point at infinity cannot be compressed");

            byte[] bytes = point.GetAffineX().ToByteArray();
            byte[] result = new byte[bytes.Length + 1];

            Array.Copy(bytes, 0, result, 0, bytes.Length);
            int sign = point.GetAffineY().TestBit(0) ? 1 : 0;
            int lastIndex = result.Length - 1;

            result[lastIndex] = (byte)(sign + 2);
            return result;
        }
    }
}
