using System;

namespace Eduard.Security
{
    /// <summary>
    /// Provides mathematical operations with affine points on the Weierstrass elliptic curve.
    /// </summary>
    public static class WeierstrassAffineExtensions
    {
        /// <summary>
        /// Add two affine points on the Weierstrass elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ECPoint Add(this EllipticCurve curve, ECPoint left, ECPoint right)
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

                if (xDiff < 0)
                    xDiff += curve.field;

                if (yDiff < 0)
                    yDiff += curve.field;

                if (xDiff == 0) return ECPoint.POINT_INFINITY;
                inv = xDiff.Inverse(curve.field);
                lambda = (inv * yDiff) % curve.field;
            }
            else
            {
                xDiff = (3 * left.x) % curve.field;
                xDiff = (xDiff * left.x) % curve.field;
                xDiff = (xDiff + curve.a) % curve.field;
                yDiff = (2 * left.y) % curve.field;
                inv = yDiff.Inverse(curve.field);
                lambda = (xDiff * inv) % curve.field;
            }

            BigInteger temp = (lambda * lambda) % curve.field;
            BigInteger delta = (left.x + right.x) % curve.field;

            BigInteger x = temp - delta;

            if (x < 0)
                x += curve.field;

            BigInteger y = left.x - x;

            if (y < 0)
                y += curve.field;

            y = (y * lambda) % curve.field;
            y -= left.y;

            if (y < 0)
                y += curve.field;

            return new ECPoint(x, y);
        }

        /// <summary>
        /// Multiply the affine point on the Weierstrass elliptic curve by a specified scalar.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="k"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint Multiply(this EllipticCurve curve, BigInteger k, ECPoint point)
        {
            if (k < 0)
                throw new ArgumentException("Bad input.");

            if (k == 0 || point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            ECPoint temp = point;
            ECPoint result = ECPoint.POINT_INFINITY;
            int length = k.GetBits();

            for (int j = 0; j < length; j++)
            {
                if (k.TestBit(j))
                    result = Add(curve, result, temp);

                temp = Add(curve, temp, temp);
            }

            return result;
        }

        /// <summary>
        /// Compute the additive inverse of the specified affine point on the Weierstrass curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint Negate(this EllipticCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            return new ECPoint(point.x, curve.field - point.y);
        }
    }
}
