using System;
using System.Diagnostics;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Provides mathematical operations for points on twisted Edwards curves, including <br/>
    /// affine and projective arithmetic with optimized scalar multiplication algorithms.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supports multiple operation modes:
    /// <list type="bullet">
    /// <item><description><see cref="ECMode.EC_STANDARD_AFFINE"/>: Binary method in affine coordinates</description></item>
    /// <item><description><see cref="ECMode.EC_STANDARD_PROJECTIVE"/>: Binary method with projective coordinates</description></item>
    /// <item><description><see cref="ECMode.EC_FASTEST"/>: NAF fractional sliding window method with mixed coordinates</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// For complete curves (d non-square), unified addition formulas are used. For incomplete curves, <br/>
    /// dedicated formulas provide optimized performance with proper exceptional case
    /// handling.
    /// </para>
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class TwistedEdwardsMath
    {
        /// <summary>
        /// Performs scalar multiplication on a twisted Edwards curve point with comprehensive
        /// security validation and algorithm selection.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context containing parameters a, d, and field prime.</param>
        /// <param name="k">The scalar multiplier.</param>
        /// <param name="point">Base point to multiply in affine coordinates.</param>
        /// <param name="opMode">Execution mode selecting algorithm and coordinate system.</param>
        /// <param name="securityCheck">If true, validates point lies on curve and has appropriate order.</param>
        /// <returns>The resulting point k * point in affine coordinates.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="securityCheck"/> is enabled and the point does not lie
        /// on the curve, indicating a potential small-subgroup or invalid curve attack.
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// Thrown when <paramref name="opMode"/> is set to <see cref="ECMode.EC_SECURE"/> as this mode
        /// requires Montgomery ladder transformation not yet implemented.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Automatically selects the optimal coordinate system based on operation mode:
        /// <list type="bullet">
        /// <item><description><see cref="ECMode.EC_STANDARD_AFFINE"/>: Binary method in affine coordinates</description></item>
        /// <item><description><see cref="ECMode.EC_STANDARD_PROJECTIVE"/>: Binary method with projective coordinates</description></item>
        /// <item><description><see cref="ECMode.EC_FASTEST"/>: NAF fractional sliding window method with precomputation</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The security check validates point-on-curve membership to prevent invalid curve attacks. <br/>
        /// For production systems handling untrusted points, this check should be enabled.
        /// </para>
        /// </remarks>
        public static ECPoint Multiply(TwistedEdwardsCurve curve, BigInteger k, ECPoint point, ECMode opMode = ECMode.EC_STANDARD_AFFINE, bool securityCheck = false)
        {
            if (k == 0 || point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            string[] pointErrors = new string[]
            {
                "The specified point does not lie on the twisted Edwards curve.",
                "The specified point maps to the quadratic twist and is not in the correct subgroup.",
                "The specified point lies in a small-order subgroup and is unsafe for cryptographic operations."
            };

            if (securityCheck)
            {
                PointCheck checkResult = curve.ValidatePoint(point);
                int index = (int)checkResult;

                if (index < 1)
                    throw new ArgumentException(
                        pointErrors[index + 2],
                        nameof(point));
            }

            ECPoint affinePoint = point;
            BigInteger nk = k;

            if (k < 0)
            {
                affinePoint = TwistedEdwardsMath.Negate(curve, affinePoint);
                nk = k.Negate();
            }

            ECPoint result = ECPoint.POINT_INFINITY;
            int bitSize = nk.GetBits();

            if (opMode == ECMode.EC_STANDARD_AFFINE)
            {
                for (int j = bitSize - 1; j >= 0; j--)
                {
                    result = Add(curve, result, result);

                    if (nk.TestBit(j))
                        result = Add(curve, result, affinePoint);
                }
            }
            else if (opMode == ECMode.EC_STANDARD_PROJECTIVE)
            {
                ECPoint3 auxPoint = ECPoint3.POINT_INFINITY;
                var basePoint = curve.ToProjective(affinePoint);

                for (int j = bitSize - 1; j >= 0; j--)
                {
                    auxPoint = Ed3Math.UnifiedDoubling(curve, auxPoint);

                    if (nk.TestBit(j))
                        auxPoint = Ed3Math.UnifiedAdd(curve, auxPoint, basePoint);
                }

                result = curve.ToAffine(auxPoint);
            }
            else if (opMode == ECMode.EC_SECURE)
                throw new NotImplementedException(
                    "EC_SECURE mode is not available for twisted Edwards curves. " +
                    "Use EC_STANDARD_AFFINE, EC_STANDARD_PROJECTIVE, or EC_FASTEST instead.");
            else
            {
                int i, j, win;
                int bc, ubits = 0;

                int windowSize = 8;
                int tbits = 0;

                var table = new ECPoint4[windowSize];
                table[0] = curve.ToExtendedProjective(affinePoint);

                var squarePoint = Ed4Math.DedicatedDoubling(curve, table[0]);
                ECPoint4 auxPoint = ECPoint4.POINT_INFINITY;

                BigInteger exp3 = 3 * nk;
                bc = exp3.GetBits();

                /* compute the lookup table */
                for (i = 1; i < windowSize; i++)
                    table[i] = Ed4Math.Add(curve, table[i - 1], squarePoint);

                for (i = bc - 1; i >= 1;)
                {
                    win = WindowUtil.NAFWindow(nk, exp3, i, ref ubits, ref tbits, windowSize);
                    var projectivePoint = curve.ToProjective(auxPoint);

                    for (j = 0; j < ubits - 1; j++)
                        projectivePoint = Ed3Math.UnifiedDoubling(curve, projectivePoint);

                    if (ubits >= 1)
                    {
                        var tempPoint = curve.GetPointCopy(projectivePoint);
                        auxPoint = Ed4Math.DedicatedDoubling(curve, tempPoint);
                    }

                    if (win > 0)
                    {
                        var table_point = table[win >> 1];
                        auxPoint = Ed4Math.Add(curve, table_point, auxPoint);
                    }
                    if (win < 0)
                    {
                        var table_point = table[(-win) >> 1];
                        var negative_point = Ed4Math.Negate(curve, table_point);
                        auxPoint = Ed4Math.Add(curve, negative_point, auxPoint);
                    }

                    i -= ubits;

                    if (tbits != 0)
                    {
                        var lastPoint = curve.ToProjective(auxPoint);
                        i -= tbits;

                        for (j = 0; j < tbits - 1; j++)
                            lastPoint = Ed3Math.UnifiedDoubling(curve, lastPoint);

                        if (tbits >= 1)
                        {
                            var tempPoint = curve.GetPointCopy(lastPoint);
                            auxPoint = Ed4Math.DedicatedDoubling(curve, tempPoint);
                        }
                    }
                }

                result = curve.ToAffine(auxPoint);
            }

            return result;
        }

        /// <summary>
        /// Adds two affine points on a twisted Edwards curve, automatically selecting <br/>
        /// the optimal formula based on curve completeness and point equality.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="left">First point to add.</param>
        /// <param name="right">Second point to add.</param>
        /// <returns>The sum of the points in affine coordinates.</returns>
        /// <remarks>
        /// <para>
        /// Handles all special cases:
        /// <list type="bullet">
        /// <item><description>Point at infinity (identity element)</description></item>
        /// <item><description>P + (-P) = point at infinity (inverse pairs)</description></item>
        /// <item><description>Complete curves use unified addition</description></item>
        /// <item><description>Incomplete curves use dedicated formulas (P != Q) or doubling (P = Q)</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public static ECPoint Add(TwistedEdwardsCurve curve, ECPoint left, ECPoint right)
        {
            if (left == ECPoint.POINT_INFINITY && right == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            if (left == ECPoint.POINT_INFINITY)
                return right;

            if (right == ECPoint.POINT_INFINITY)
                return left;

            if (left == Negate(curve, right))
                return ECPoint.POINT_INFINITY;

            if(curve.isComplete) 
                return CompleteAdd(curve, left, right);

            if (left == right) return DedicatedDoubling(curve, left);
            return DedicatedAdd(curve, left, right);
        }

        /// <summary>
        /// Adds two affine points using the complete unified formula for twisted Edwards curves.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="left">First point to add.</param>
        /// <param name="right">Second point to add.</param>
        /// <returns>The sum of the points in affine coordinates.</returns>
        /// <remarks>
        /// Implements the complete addition formula that works for all points without exceptional cases. <br/>
        /// Used when the curve parameter d is a non-square, ensuring group law completeness.
        /// </remarks>
        public static ECPoint CompleteAdd(TwistedEdwardsCurve curve, ECPoint left, ECPoint right)
        {
            BigInteger p = curve.field;
            BigInteger A1 = BarrettReducer.MultMod(left.x, right.y);

            BigInteger A2 = BarrettReducer.MultMod(left.y, right.x);
            BigInteger A12 = BarrettReducer.MultMod(A1, A2);
            BigInteger A3 = BarrettReducer.MultMod(curve.d, A12);

            BigInteger tx = BarrettReducer.AddMod(A3, 1);
            if (tx == 0) return ECPoint.POINT_INFINITY;

            BigInteger ty = BarrettReducer.SubMod(1, A3);
            if (ty == 0) return ECPoint.POINT_INFINITY;

            BigInteger Axy = BarrettReducer.MultMod(tx, ty);
            BigInteger txy = BarrettReducer.InvMod(Axy);

            BigInteger x = BarrettReducer.AddMod(A1, A2);
            BigInteger t = BarrettReducer.MultMod(x, txy);
            x = BarrettReducer.MultMod(t, ty);

            BigInteger A4 = BarrettReducer.MultMod(left.y, right.y);
            BigInteger B1t = BarrettReducer.MultMod(left.x, right.x);
            BigInteger A5 = BarrettReducer.MultMod(curve.a, B1t);

            BigInteger y = BarrettReducer.SubMod(A4, A5);
            BigInteger B2t = BarrettReducer.MultMod(y, txy);

            y = BarrettReducer.MultMod(B2t, tx);
            return new ECPoint(x, y);
        }

        /// <summary>
        /// Adds two distinct affine points on an incomplete twisted Edwards curve using the dedicated formula.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="left">First point to add (must not equal right).</param>
        /// <param name="right">Second point to add (must not equal left).</param>
        /// <returns>The sum of the points in affine coordinates.</returns>
        /// <remarks>
        /// Optimized addition formula for curves where d is a square. This formula has exceptional <br/>
        /// cases that must be handled by the caller (ensuring points are distinct and not inverses).
        /// </remarks>
        public static ECPoint DedicatedAdd(TwistedEdwardsCurve curve, ECPoint left, ECPoint right)
        {
            BigInteger p = curve.field;
            BigInteger A1 = BarrettReducer.MultMod(left.x, left.y);

            BigInteger A2 = BarrettReducer.MultMod(right.x, right.y);
            BigInteger A3 = BarrettReducer.MultMod(left.y, right.y);

            BigInteger B1 = BarrettReducer.MultMod(left.x, right.x);
            BigInteger A4 = BarrettReducer.MultMod(curve.a, B1);

            BigInteger A5 = BarrettReducer.AddMod(A1, A2);
            BigInteger A6 = BarrettReducer.AddMod(A3, A4);

            if (A6 == 0) return ECPoint.POINT_INFINITY;
            BigInteger A7 = BarrettReducer.SubMod(A1, A2);

            BigInteger A8 = BarrettReducer.MultMod(left.x, right.y);
            BigInteger A9 = BarrettReducer.MultMod(left.y, right.x);

            BigInteger A10 = BarrettReducer.SubMod(A8, A9);
            if (A10 == 0) return ECPoint.POINT_INFINITY;

            BigInteger A11t = BarrettReducer.MultMod(A6, A10);
            BigInteger A11 = BarrettReducer.InvMod(A11t);

            BigInteger B2 = BarrettReducer.MultMod(A5, A10);
            BigInteger x = BarrettReducer.MultMod(B2, A11);

            BigInteger B3 = BarrettReducer.MultMod(A6, A7);
            BigInteger y = BarrettReducer.MultMod(B3, A11);
            return new ECPoint(x, y);
        }

        /// <summary>
        /// Doubles an affine point on a twisted Edwards curve using the dedicated doubling formula.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="point">The point to double.</param>
        /// <returns>The point doubled (2P) in affine coordinates.</returns>
        /// <remarks>
        /// Implements optimized point doubling for twisted Edwards curves. <br/>
        /// Returns the point at infinity when doubling a point of order 2.
        /// </remarks>
        public static ECPoint DedicatedDoubling(TwistedEdwardsCurve curve, ECPoint point)
        {
            BigInteger p = curve.field;
            BigInteger B1 = BarrettReducer.MultMod(point.x, point.y);
            BigInteger A1 = BarrettReducer.AddMod(B1, B1);

            BigInteger A2 = BarrettReducer.MultMod(point.y, point.y);
            BigInteger B2 = BarrettReducer.MultMod(point.x, point.x);
            BigInteger A3 = BarrettReducer.MultMod(curve.a, B2);

            BigInteger A4 = BarrettReducer.AddMod(A2, A3);
            if (A4 == 0) return ECPoint.POINT_INFINITY;

            BigInteger A5 = BarrettReducer.SubMod(2, A4);
            if (A5 == 0) return ECPoint.POINT_INFINITY;

            BigInteger A6t = BarrettReducer.MultMod(A4, A5);
            BigInteger A6 = BarrettReducer.InvMod(A6t);

            BigInteger B3 = BarrettReducer.MultMod(A1, A5);
            BigInteger x = BarrettReducer.MultMod(B3, A6);

            BigInteger y = BarrettReducer.SubMod(A2, A3);
            BigInteger B4 = BarrettReducer.MultMod(y, A4);

            y = BarrettReducer.MultMod(B4, A6);
            return new ECPoint(x, y);
        }

        /// <summary>
        /// Computes the additive inverse of an affine point on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="point">The point to negate.</param>
        /// <returns>The point -P such that P + (-P) = point at infinity.</returns>
        /// <remarks>
        /// For a twisted Edwards curve, the inverse of point (x, y) is (-x, y). <br/>
        /// The point at infinity is its own inverse.
        /// </remarks>
        public static ECPoint Negate(TwistedEdwardsCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            return new ECPoint(curve.field - point.x, point.y);
        }
    }
}
