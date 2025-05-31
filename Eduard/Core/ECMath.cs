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
            {
                int i, j, n;
                int nb, nbs = 0, nzs = 0;
                int windowSize = 8;

                var table = new JacobianChudnovskyPoint[windowSize];
                table[0] = curve.ToJacobianChudnovsky(point);

                var squarePoint = JacobianChudnovskyMath.Doubling(curve, table[0]);
                JacobianPoint auxPoint = JacobianPoint.POINT_INFINITY;

                BigInteger k3 = 3 * k;
                nb = k3.GetBits();

                /* compute the lookup table */
                for (i = 1; i < windowSize; i++)
                    table[i] = JacobianChudnovskyMath.Add(curve, table[i - 1], squarePoint);

                for (i = nb - 1; i >= 1;)
                {
                    n = WindowUtil.NAFWindow(k, k3, i, ref nbs, ref nzs, windowSize);
                    var auxModifiedJacobianPoint = curve.ToModifiedJacobian(auxPoint);

                    for (j = 0; j < nbs - 1; j++)
                        auxModifiedJacobianPoint = ModifiedJacobianMath.Doubling(curve, auxModifiedJacobianPoint);

                    if(nbs >= 1)
                        auxPoint = JacobianMath.Doubling(curve, curve.ToJacobian(auxModifiedJacobianPoint));

                    if(n > 0)
                    {
                        var table_point = curve.ToJacobian(table[n >> 1]);
                        auxPoint = JacobianMath.Add(curve, table_point, auxPoint);
                    }
                    if(n < 0)
                    {
                        var table_point = curve.ToJacobian(table[(-n) >> 1]);
                        table_point.y = curve.field - table_point.y;
                        auxPoint = JacobianMath.Add(curve, table_point, auxPoint);
                    }

                    i -= nbs;

                    if (nzs != 0)
                    {
                        var lastPoint = curve.ToModifiedJacobian(auxPoint);
                        i -= nzs;

                        for (j = 0; j < nzs - 1; j++)
                            lastPoint = ModifiedJacobianMath.Doubling(curve, lastPoint);

                        if(nzs >= 1)
                        {
                            auxPoint = curve.ToJacobian(lastPoint);
                            auxPoint = JacobianMath.Doubling(curve, auxPoint);
                        }
                        
                    }
                }

                result = curve.ToAffine(auxPoint);
            }

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
