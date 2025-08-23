using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography
{
    /// <summary>
    /// Provides mathematical operations for points on the Montgomery curve.
    /// </summary>
    public static class MontyMath
    {
        /// <summary>
        /// Add two affine points on the Montgomery curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ECPoint Add(MontgomeryCurve curve, ECPoint left, ECPoint right)
        {
            if (left == ECPoint.POINT_INFINITY && right == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            if (left == ECPoint.POINT_INFINITY)
                return right;

            if (right == ECPoint.POINT_INFINITY)
                return left;

            if (left == Negate(curve, right))
                return ECPoint.POINT_INFINITY;

            BigInteger lambda = -1;
            BigInteger xDiff = 0;
            BigInteger yDiff = 0;
            BigInteger inv = 0;


            if (left != right)
            {
                xDiff = right.x - left.x;
                yDiff = right.y - left.y;

                if (xDiff < 0) xDiff += curve.field;
                if (yDiff < 0) yDiff += curve.field;

                if (xDiff == 0) return ECPoint.POINT_INFINITY;
                inv = xDiff.Inverse(curve.field);
                lambda = (inv * yDiff) % curve.field;
            }
            else
            {
                xDiff = (left.x * left.x) % curve.field;
                xDiff = (3 * xDiff) % curve.field;

                BigInteger Ax = (curve.A * left.x) % curve.field;
                Ax = (2 * Ax + 1) % curve.field;
                xDiff = (xDiff + Ax) % curve.field;

                yDiff = (curve.B * left.y) % curve.field;
                yDiff = (2 * yDiff) % curve.field;

                inv = yDiff.Inverse(curve.field);
                lambda = (xDiff * inv) % curve.field;
            }

            BigInteger temp = (lambda * lambda) % curve.field;
            temp = (curve.B * temp) % curve.field;

            BigInteger delta = (curve.A + left.x + right.x) % curve.field;
            BigInteger x = temp - delta;

            if (x < 0) x += curve.field;
            BigInteger y = left.x - x;

            if (y < 0) y += curve.field;
            y = (y * lambda) % curve.field;
            y -= left.y;

            if (y < 0) y += curve.field;
            return new ECPoint(x, y);
        }

        /// <summary>
        /// Multiply an affine point on the Montgomery curve by a specified scalar.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="k"></param>
        /// <param name="point"></param>
        /// <param name="opMode"></param>
        /// <returns></returns>
        public static ECPoint Multiply(MontgomeryCurve curve, BigInteger k, ECPoint point, ECMode opMode = ECMode.EC_STANDARD_AFFINE)
        {
            if (k < 0) throw new ArgumentException("Bad input.");

            if (k == 0 || point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            ECPoint temp = point;
            ECPoint result = ECPoint.POINT_INFINITY;
            int t = k.GetBits();

            if (opMode == ECMode.EC_STANDARD_AFFINE)
            {
                for (int j = 0; j < t; j++)
                {
                    if (k.TestBit(j))
                        result = Add(curve, result, temp);

                    temp = Add(curve, temp, temp);
                }
            }

            return result;
        }

        /// <summary>
        /// Compute the additive inverse of an affine point on the Montgomery curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint Negate(MontgomeryCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            return new ECPoint(point.x, curve.field - point.y);
        }
    }
}
