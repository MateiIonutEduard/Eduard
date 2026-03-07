using Eduard;
using Eduard.Security.Primitives;
using System.Diagnostics;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Implements optimized arithmetic for twisted Edwards curves using extended projective <br/>
    /// coordinates (X, Y, T, Z), based on the improved formulas from Hisil et al. (2008).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reference: Hisil, H., Wong, K.K.H., Carter, G., Dawson, E. (2008). "Twisted Edwards Curves Revisited". <br/>
    /// Advances in Cryptology - ASIACRYPT 2008, pp. 326-343. Springer Berlin Heidelberg.
    /// </para>
    /// <para>
    /// Extended projective coordinates (X, Y, T, Z) introduce the auxiliary coordinate T = (X*Y)/Z, <br/>
    /// reducing point addition from 11 to 9 field multiplications while maintaining the completeness <br/>
    /// properties of twisted Edwards curves. This representation is particularly efficient for <br/>
    /// scalar multiplication algorithms.
    /// </para>
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class Ed4Math
    {
        /// <summary>
        /// Adds two extended projective points on a twisted Edwards curve, <br/>
        /// automatically selecting the optimal addition formula based on <br/>
        /// curve parameters.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="left">First point in extended coordinates.</param>
        /// <param name="right">Second point in extended coordinates.</param>
        /// <returns>The sum of the points in extended coordinates.</returns>
        /// <remarks>
        /// Automatically routes to the appropriate addition formula:
        /// <list type="bullet">
        /// <item><description>Complete curves (d non-square) -> unified addition</description></item>
        /// <item><description>Incomplete curves -> dedicated addition</description></item>
        /// <item><description>Isomorphic twist handling when computeOnTwist is enabled</description></item>
        /// </list>
        /// Handles point at infinity and P + (-P) = O cases.
        /// </remarks>
        public static ECPoint4 Add(TwistedEdwardsCurve curve, ECPoint4 left, ECPoint4 right)
        {
            if (left == ECPoint4.POINT_INFINITY && right == ECPoint4.POINT_INFINITY)
                return ECPoint4.POINT_INFINITY;

            if (left == ECPoint4.POINT_INFINITY) return right;
            if (right == ECPoint4.POINT_INFINITY) return left;

            if (left == Negate(curve, right))
                return ECPoint4.POINT_INFINITY;

            if (curve.isComplete)
            {
                if (curve.computeOnTwist) return TwistUnifiedAdd(curve, left, right);
                else return UnifiedAdd(curve, left, right);
            }

            if (curve.computeOnTwist) return TwistDedicatedAdd(curve, left, right);
            return DedicatedAdd(curve, left, right);
        }

        /// <summary>
        /// Adds two extended projective points using the complete unified formula for twisted Edwards curves.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="left">First point in extended coordinates.</param>
        /// <param name="right">Second point in extended coordinates.</param>
        /// <returns>The sum of the points in extended coordinates.</returns>
        /// <remarks>
        /// Implements Algorithm 1 from Hisil et al. (2008) requiring 9M + 1S + 1D operations. <br/>
        /// Works for all points when d is a non-square in the field, providing complete <br/>
        /// addition without exceptional cases.
        /// </remarks>
        public static ECPoint4 UnifiedAdd(TwistedEdwardsCurve curve, ECPoint4 left, ECPoint4 right)
        {
            if (left == ECPoint4.POINT_INFINITY) return right;
            if (right == ECPoint4.POINT_INFINITY) return left;

            BigInteger p = curve.field;
            BigInteger A1 = (left.x * right.x) % p;

            BigInteger A2 = (left.y * right.y) % p;
            BigInteger A3 = (curve.d * ((left.t * right.t) % p)) % p;

            BigInteger A4 = (left.z * right.z) % p;
            BigInteger A5 = ((((left.x + left.y) * (right.x + right.y)) % p) - A1 - A2) % p;

            if (A5 < 0) A5 += p;
            BigInteger A6 = (A4 - A3) % p;

            if (A6 < 0) A6 += p;
            BigInteger A7 = (A4 + A3) % p;

            BigInteger A8 = (A2 - ((curve.a * A1) % p)) % p;
            if (A8 < 0) A8 += p;

            BigInteger X = (A5 * A6) % p;
            BigInteger Y = (A7 * A8) % p;

            BigInteger T = (A5 * A8) % p;
            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ECPoint4.POINT_INFINITY;
            return new ECPoint4(X, Y, T, Z);
        }

        /// <summary>
        /// Adds two extended projective points on the isomorphic twisted Edwards curve using the unified formula.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context with twist parameters.</param>
        /// <param name="left">First point in extended coordinates.</param>
        /// <param name="right">Second point in extended coordinates.</param>
        /// <returns>The sum of the points in extended coordinates.</returns>
        /// <remarks>
        /// Implements unified addition for curves isogenous to the original twisted Edwards curve. <br/>
        /// Used when working on the quadratic twist for optimized implementations.
        /// </remarks>
        public static ECPoint4 TwistUnifiedAdd(TwistedEdwardsCurve curve, ECPoint4 left, ECPoint4 right)
        {
            if (left == ECPoint4.POINT_INFINITY) return right;
            if (right == ECPoint4.POINT_INFINITY) return left;

            BigInteger p = curve.field;
            BigInteger A1 = ((left.y - left.x) * (right.y - right.x)) % p;
            if (A1 < 0) A1 += p;

            BigInteger A2 = ((left.y + left.x) * (right.y + right.x)) % p;
            if (A2 < 0) A2 += p;

            BigInteger A3 = (curve.kt * left.t * right.t) % p;
            BigInteger A4 = (2 * left.z * right.z) % p;

            BigInteger A5 = (p + A2 - A1) % p;
            BigInteger A6 = (p + A4 - A3) % p;

            BigInteger A7 = (A4 + A3) % p;
            BigInteger A8 = (A2 + A1) % p;

            BigInteger X = (A5 * A6) % p;
            BigInteger Y = (A7 * A8) % p;

            BigInteger T = (A5 * A8) % p;
            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ECPoint4.POINT_INFINITY;
            return new ECPoint4(X, Y, T, Z);
        }

        /// <summary>
        /// Adds two extended projective points using the optimized dedicated formula for twisted Edwards curves.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="left">First point in extended coordinates.</param>
        /// <param name="right">Second point in extended coordinates.</param>
        /// <returns>The sum of the points in extended coordinates.</returns>
        /// <remarks>
        /// Implements dedicated addition for curves where d is a square, requiring 8M operations. <br/>
        /// This formula is faster but has exceptional cases that must be handled separately.
        /// </remarks>
        public static ECPoint4 DedicatedAdd(TwistedEdwardsCurve curve, ECPoint4 left, ECPoint4 right)
        {
            if (left == ECPoint4.POINT_INFINITY) return right;
            if (right == ECPoint4.POINT_INFINITY) return left;

            BigInteger p = curve.field;
            BigInteger A1 = (left.x * right.x) % p;

            BigInteger A2 = (left.y * right.y) % p;
            BigInteger A3 = (left.z * right.t) % p;

            BigInteger A4 = (left.t * right.z) % p;
            BigInteger A5 = (A4 + A3) % p;

            BigInteger A6 = ((((left.x - left.y) * (right.x + right.y)) % p) + A2 - A1) % p;
            if (A6 < 0) A6 += p;

            BigInteger A7 = (A2 + curve.a * A1) % p;
            BigInteger A8 = (p + A4 - A3) % p;

            BigInteger X = (A5 * A6) % p;
            BigInteger Y = (A7 * A8) % p;

            BigInteger T = (A5 * A8) % p;
            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ECPoint4.POINT_INFINITY;
            return new ECPoint4(X, Y, T, Z);
        }

        /// <summary>
        /// Adds two extended projective points on the curve isomorphic to the twisted Edwards curve using the dedicated formula.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context with twist parameters.</param>
        /// <param name="left">First point in extended coordinates.</param>
        /// <param name="right">Second point in extended coordinates.</param>
        /// <returns>The sum of the points in extended coordinates.</returns>
        /// <remarks>
        /// Optimized addition for the quadratic twist of a twisted Edwards curve. <br/>
        /// Used in coordinate systems that work on the twist for improved performance.
        /// </remarks>
        public static ECPoint4 TwistDedicatedAdd(TwistedEdwardsCurve curve, ECPoint4 left, ECPoint4 right)
        {
            if (left == ECPoint4.POINT_INFINITY) return right;
            if (right == ECPoint4.POINT_INFINITY) return left;
            BigInteger p = curve.field;

            BigInteger A1 = ((left.y - left.x) * (right.y + right.x)) % p;
            if (A1 < 0) A1 += p;

            BigInteger A2 = ((left.y + left.x) * (right.y - right.x)) % p;
            if (A2 < 0) A2 += p;

            BigInteger A3 = (2 * left.z * right.t) % p;
            BigInteger A4 = (2 * left.t * right.z) % p;

            BigInteger A5 = (A3 + A4) % p;
            BigInteger A6 = (p + A2 - A1) % p;

            BigInteger A7 = (A2 + A1) % p;
            BigInteger A8 = (p + A4 - A3) % p;

            BigInteger X = (A5 * A6) % p;
            BigInteger Y = (A7 * A8) % p;

            BigInteger T = (A5 * A8) % p;
            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ECPoint4.POINT_INFINITY;
            return new ECPoint4(X, Y, T, Z);
        }

        /// <summary>
        /// Doubles an extended projective point on a twisted Edwards curve using the dedicated formula.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="point">The point to double in extended coordinates.</param>
        /// <returns>The doubled point (2P) in extended coordinates.</returns>
        /// <remarks>
        /// Implements the doubling formula for extended coordinates requiring 4M + 4S + 1D operations. <br/>
        /// The computation reuses the T coordinate to eliminate redundant operations.
        /// </remarks>
        public static ECPoint4 DedicatedDoubling(TwistedEdwardsCurve curve, ECPoint4 point)
        {
            if (point == ECPoint4.POINT_INFINITY)
                return ECPoint4.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger A1 = (point.x * point.x) % p;

            BigInteger A2 = (point.y * point.y) % p;
            BigInteger A3 = (2 * point.z * point.z) % p;

            BigInteger A4 = (curve.a * A1) % p;
            BigInteger A5 = ((((point.x + point.y) * (point.x + point.y)) % p) - A1 - A2) % p;

            if (A5 < 0) A5 += p;
            BigInteger A6 = (A2 + A4) % p;

            BigInteger A7 = (p + A6 - A3) % p;
            BigInteger A8 = (p + A4 - A2) % p;

            BigInteger X = (A5 * A7) % p;
            BigInteger Y = (A6 * A8) % p;

            BigInteger T = (A5 * A8) % p;
            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ECPoint4.POINT_INFINITY;
            return new ECPoint4(X, Y, T, Z);
        }

        /// <summary>
        /// Computes the additive inverse of a point in extended projective coordinates on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="point">The point to negate in extended coordinates.</param>
        /// <returns>The point -P such that P + (-P) = point at infinity.</returns>
        /// <remarks>
        /// For a point P = (X, Y, T, Z) in extended coordinates, its inverse is -P = (-X, Y, -T, Z). <br/>
        /// Both X and T coordinates are negated modulo the field prime, maintaining the <br/>
        /// relation T = (X*Y)/Z. The point at infinity is its own inverse.
        /// </remarks>
        public static ECPoint4 Negate(TwistedEdwardsCurve curve, ECPoint4 point)
        {
            BigInteger Xp = curve.field - point.x;
            BigInteger Tp = curve.field - point.t;
            return new ECPoint4(Xp, point.y, Tp, point.z);
        }
    }
}
