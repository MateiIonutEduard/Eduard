using System;

namespace Eduard.Security
{
    /// <summary>
    /// Provides mathematical operations for points on the Weierstrass elliptic curve.
    /// </summary>
    public static class ECMath
    {
        /// <summary>
        /// Add two affine points on the Weierstrass elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ECPoint Add(EllipticCurve curve, ECPoint left, ECPoint right)
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
        /// <param name="opMode"></param>
        /// <returns></returns>
        public static ECPoint Multiply(EllipticCurve curve, BigInteger k, ECPoint point, ECMode opMode=ECMode.EC_STANDARD)
        {
            if (k < 0) throw new ArgumentException("Bad input.");

            if (k == 0 || point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            ECPoint temp = point;
            ECPoint result = ECPoint.POINT_INFINITY;
            int t = k.GetBits();

            if (opMode == ECMode.EC_STANDARD)
            {
                for (int j = 0; j < t; j++)
                {
                    if (k.TestBit(j))
                        result = Add(curve, result, temp);

                    temp = Add(curve, temp, temp);
                }
            }
            else if (opMode == ECMode.EC_SECURE)
            {
                JacobianPoint R0 = JacobianPoint.POINT_INFINITY;
                JacobianPoint R1 = curve.ToJacobian(temp);

                for (int j = t - 1; j >= 0; j--)
                {
                    if(!k.TestBit(j))
                    {
                        R1 = JacobianMath.Add(curve, R0, R1);
                        R0 = JacobianMath.Doubling(curve, R0);
                    }
                    else
                    {
                        R0 = JacobianMath.Add(curve, R0, R1);
                        R1 = JacobianMath.Doubling(curve, R1);
                    }
                }

                result = curve.ToAffine(R0);
            }
            else
                throw new NotImplementedException();

            return result;
        }

        /// <summary>
        /// Compute the additive inverse of the specified affine point on the Weierstrass curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint Negate(EllipticCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            return new ECPoint(point.x, curve.field - point.y);
        }
    }
}
