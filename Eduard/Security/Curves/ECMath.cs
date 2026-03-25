using System;
using Eduard;
using System.Diagnostics;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Provides mathematical operations for points on Weierstrass elliptic curves.
    /// </summary>
    /// <remarks>
    /// Implements affine and projective point arithmetic for Weierstrass curves in short Weierstrass form: <br/>
    /// y^2 = x^3 + ax + b (mod p). Supports multiple coordinate systems and optimized scalar multiplication <br/>
    /// algorithms including window methods and Montgomery ladder for side-channel resistance.
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class ECMath
    {
        /// <summary>
        /// Adds two affine points on the Weierstrass elliptic curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context containing field parameters.</param>
        /// <param name="left">First point to add.</param>
        /// <param name="right">Second point to add.</param>
        /// <returns>The sum of the two points in affine coordinates.</returns>
        /// <remarks>
        /// Handles all special cases including point at infinity, point doubling, <br/>
        /// and vertical line scenarios (resulting in point at infinity).
        /// </remarks>
        public static ECPoint Add(EllipticCurve curve, ECPoint left, ECPoint right)
        {
            if (left == ECPoint.POINT_INFINITY && right == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            if (left == ECPoint.POINT_INFINITY)
                return right;

            if (right == ECPoint.POINT_INFINITY)
                return left;

            BigInteger lambda = -1;
            BigInteger xDiff = 0;
            BigInteger yDiff = 0;
            BigInteger inv = 0;


            if (left != right)
            {
                xDiff = BarrettReducer.SubMod(right.x, left.x);
                yDiff = BarrettReducer.SubMod(right.y, left.y);
                if (xDiff == 0) return ECPoint.POINT_INFINITY;

                inv = BarrettReducer.InvMod(xDiff);
                lambda = BarrettReducer.MulMod(inv, yDiff);
            }
            else
            {
                xDiff = BarrettReducer.MulMod(3, left.x);
                xDiff = BarrettReducer.MulMod(xDiff, left.x);

                xDiff = BarrettReducer.AddMod(xDiff, curve.a);
                yDiff = BarrettReducer.AddMod(left.y, left.y);
                if (yDiff == 0) return ECPoint.POINT_INFINITY;

                inv = BarrettReducer.InvMod(yDiff);
                lambda = BarrettReducer.MulMod(xDiff, inv);
            }

            BigInteger temp = BarrettReducer.MulMod(lambda, lambda);
            BigInteger delta = BarrettReducer.AddMod(left.x, right.x);

            BigInteger x = BarrettReducer.SubMod(temp, delta);
            BigInteger y = BarrettReducer.SubMod(left.x, x);

            y = BarrettReducer.MulMod(y, lambda);
            y = BarrettReducer.SubMod(y, left.y);
            return new ECPoint(x, y);
        }

        /// <summary>
        /// Performs scalar multiplication on a Weierstrass curve point.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="k">Scalar multiplier (non-negative integer).</param>
        /// <param name="point">Base point to multiply.</param>
        /// <param name="opMode">Execution mode selecting algorithm and coordinate system.</param>
        /// <param name="securityCheck">If true, validates point order before multiplication.</param>
        /// <returns>The resulting point k * point.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when k is negative, or when security check fails on invalid point.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Supports multiple operation modes:
        /// <list type="bullet">
        /// <item><description><see cref="ECMode.EC_STANDARD_AFFINE"/>: Binary method in affine coordinates</description></item>
        /// <item><description><see cref="ECMode.EC_STANDARD_PROJECTIVE"/>: Binary method with mixed coordinates</description></item>
        /// <item><description><see cref="ECMode.EC_SECURE"/>: Montgomery ladder for side-channel resistance</description></item>
        /// <item><description><see cref="ECMode.EC_FASTEST"/>: NAF fractional sliding window method with precomputation</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The security check parameter validates that the point lies on the curve and <br/>
        /// has appropriate order to prevent small-subgroup attacks.
        /// </para>
        /// </remarks>
        public static ECPoint Multiply(EllipticCurve curve, BigInteger k, ECPoint point, ECMode opMode = ECMode.EC_STANDARD_AFFINE, bool securityCheck = false)
        {
            if (k < 0)
                throw new ArgumentException(
                    "Scalar multiplier must be non-negative.",
                    nameof(k));

            if (k == 0 || point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            if (!curve.ValidatePoint(point) && securityCheck)
                throw new ArgumentException(
                    "Point validation failed: the point does not lie on the Weierstrass curve. " +
                    "This may indicate a small-subgroup attack or invalid curve parameters.",
                    nameof(point));

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
            else if (opMode == ECMode.EC_STANDARD_PROJECTIVE)
            {
                ECPoint3w auxPoint = ECPoint3w.POINT_INFINITY;
                var basePoint = curve.ToModifiedJacobian(temp);

                for (int j = 0; j < t; j++)
                {
                    if (k.TestBit(j))
                        auxPoint = Wei3Math.Add(curve, auxPoint, curve.ToJacobian(basePoint));

                    basePoint = Wei4Math.Doubling(curve, basePoint);
                }

                result = curve.ToAffine(auxPoint);
            }
            else if (opMode == ECMode.EC_SECURE)
            {
                ECPoint3w R0 = ECPoint3w.POINT_INFINITY;
                ECPoint3w R1 = curve.ToJacobian(temp);

                for (int j = t - 1; j >= 0; j--)
                {
                    if (!k.TestBit(j))
                    {
                        R1 = Wei3Math.Add(curve, R0, R1);
                        R0 = Wei3Math.Doubling(curve, R0);
                    }
                    else
                    {
                        R0 = Wei3Math.Add(curve, R0, R1);
                        R1 = Wei3Math.Doubling(curve, R1);
                    }
                }

                result = curve.ToAffine(R0);
            }
            else
            {
                int i, j, win;
                int bc, ubits = 0;

                int windowSize = 8;
                int tbits = 0;

                var table = new ECPoint5w[windowSize];
                table[0] = curve.ToJacobianChudnovsky(point);

                var squarePoint = Wei5Math.Doubling(curve, table[0]);
                ECPoint3w auxPoint = ECPoint3w.POINT_INFINITY;

                BigInteger exp3 = 3 * k;
                bc = exp3.GetBits();

                /* compute the lookup table */
                for (i = 1; i < windowSize; i++)
                    table[i] = Wei5Math.Add(curve, table[i - 1], squarePoint);

                for (i = bc - 1; i >= 1;)
                {
                    win = WindowUtil.NAFWindow(k, exp3, i, ref ubits, ref tbits, windowSize);
                    var auxModifiedJacobianPoint = curve.ToModifiedJacobian(auxPoint);

                    for (j = 0; j < ubits - 1; j++)
                        auxModifiedJacobianPoint = Wei4Math.Doubling(curve, auxModifiedJacobianPoint);

                    if (ubits >= 1)
                        auxPoint = Wei3Math.Doubling(curve, curve.ToJacobian(auxModifiedJacobianPoint));

                    if (win > 0)
                    {
                        var table_point = curve.ToJacobian(table[win >> 1]);
                        auxPoint = Wei3Math.Add(curve, table_point, auxPoint);
                    }
                    if (win < 0)
                    {
                        var table_point = curve.ToJacobian(table[(-win) >> 1]);
                        var negative_point = Wei3Math.Negate(curve, table_point);
                        auxPoint = Wei3Math.Add(curve, negative_point, auxPoint);
                    }

                    i -= ubits;

                    if (tbits != 0)
                    {
                        var lastPoint = curve.ToModifiedJacobian(auxPoint);
                        i -= tbits;

                        for (j = 0; j < tbits - 1; j++)
                            lastPoint = Wei4Math.Doubling(curve, lastPoint);

                        if (tbits >= 1)
                        {
                            auxPoint = curve.ToJacobian(lastPoint);
                            auxPoint = Wei3Math.Doubling(curve, auxPoint);
                        }

                    }
                }

                result = curve.ToAffine(auxPoint);
            }

            return result;
        }

        /// <summary>
        /// Computes the additive inverse of a point on the Weierstrass curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="point">The point to negate.</param>
        /// <returns>The point -P such that P + (-P) = point at infinity.</returns>
        /// <remarks>
        /// For a point P = (x, y) on the curve y^2 = x^3 + ax + b, its inverse <br/>
        /// is -P = (x, -y mod p). The point at infinity is its own inverse.
        /// </remarks>
        public static ECPoint Negate(EllipticCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            return new ECPoint(point.x, curve.field - point.y);
        }
    }
}
