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
            BigInteger A1 = BarrettReducer.MulMod(left.x, right.x);

            BigInteger A2 = BarrettReducer.MulMod(left.y, right.y);
            BigInteger B1t = BarrettReducer.MulMod(left.t, right.t);

            BigInteger A3 = BarrettReducer.MulMod(curve.d, B1t);
            BigInteger A4 = BarrettReducer.MulMod(left.z, right.z);

            BigInteger lxy = BarrettReducer.AddMod(left.x, left.y);
            BigInteger rxy = BarrettReducer.AddMod(right.x, right.y);

            BigInteger B2 = BarrettReducer.MulMod(lxy, rxy);
            BigInteger A5 = BarrettReducer.SubMod(B2, A1);
            A5 = BarrettReducer.SubMod(A5, A2);

            BigInteger A6 = BarrettReducer.SubMod(A4, A3);
            BigInteger A7 = BarrettReducer.AddMod(A4, A3);

            BigInteger B3 = BarrettReducer.MulMod(curve.a, A1);
            BigInteger A8 = BarrettReducer.SubMod(A2, B3);

            BigInteger X = BarrettReducer.MulMod(A5, A6);
            BigInteger Y = BarrettReducer.MulMod(A7, A8);

            BigInteger T = BarrettReducer.MulMod(A5, A8);
            BigInteger Z = BarrettReducer.MulMod(A6, A7);

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

            BigInteger B1 = BarrettReducer.SubMod(left.y, left.x);
            BigInteger B2 = BarrettReducer.SubMod(right.y, right.x);

            BigInteger A1 = BarrettReducer.MulMod(B1, B2);
            BigInteger B3 = BarrettReducer.AddMod(left.y, left.x);

            BigInteger B4 = BarrettReducer.AddMod(right.y, right.x);
            BigInteger A2 = BarrettReducer.MulMod(B3, B4);

            BigInteger B5 = BarrettReducer.MulMod(left.t, right.t);
            BigInteger A3 = BarrettReducer.MulMod(curve.kt, B5);

            BigInteger A4t = BarrettReducer.MulMod(left.z, right.z);
            BigInteger A4 = BarrettReducer.AddMod(A4t, A4t);

            BigInteger A5 = BarrettReducer.SubMod(A2, A1);
            BigInteger A6 = BarrettReducer.SubMod(A4, A3);

            BigInteger A7 = BarrettReducer.AddMod(A3, A4);
            BigInteger A8 = BarrettReducer.AddMod(A2, A1);

            BigInteger X = BarrettReducer.MulMod(A5, A6);
            BigInteger Y = BarrettReducer.MulMod(A7, A8);

            BigInteger T = BarrettReducer.MulMod(A5, A8);
            BigInteger Z = BarrettReducer.MulMod(A6, A7);

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
            BigInteger A1 = BarrettReducer.MulMod(left.x, right.x);

            BigInteger A2 = BarrettReducer.MulMod(left.y, right.y);
            BigInteger A3 = BarrettReducer.MulMod(left.z, right.t);

            BigInteger A4 = BarrettReducer.MulMod(left.t, right.z);
            BigInteger A5 = BarrettReducer.AddMod(A4, A3);

            BigInteger B1 = BarrettReducer.SubMod(left.x, left.y);
            BigInteger B2 = BarrettReducer.AddMod(right.x, right.y);

            BigInteger B3 = BarrettReducer.MulMod(B1, B2);
            BigInteger B4 = BarrettReducer.AddMod(A2, B3);

            BigInteger A6 = BarrettReducer.SubMod(B4, A1);
            BigInteger B5 = BarrettReducer.MulMod(curve.a, A1);

            BigInteger A7 = BarrettReducer.AddMod(A2, B5);
            BigInteger A8 = BarrettReducer.SubMod(A4, A3);

            BigInteger X = BarrettReducer.MulMod(A5, A6);
            BigInteger Y = BarrettReducer.MulMod(A7, A8);

            BigInteger T = BarrettReducer.MulMod(A5, A8);
            BigInteger Z = BarrettReducer.MulMod(A6, A7);

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

            BigInteger B1 = BarrettReducer.SubMod(left.y, left.x);
            BigInteger B2 = BarrettReducer.AddMod(right.y, right.x);

            BigInteger A1 = BarrettReducer.MulMod(B1, B2);
            BigInteger B3 = BarrettReducer.AddMod(left.y, left.x);

            BigInteger B4 = BarrettReducer.SubMod(right.y, right.x);
            BigInteger A2 = BarrettReducer.MulMod(B3, B4);

            BigInteger A3t = BarrettReducer.MulMod(left.z, right.t);
            BigInteger A3 = BarrettReducer.AddMod(A3t, A3t);

            BigInteger A4t = BarrettReducer.MulMod(left.t, right.z);
            BigInteger A4 = BarrettReducer.AddMod(A4t, A4t);

            BigInteger A5 = BarrettReducer.AddMod(A3, A4);
            BigInteger A6 = BarrettReducer.SubMod(A2, A1);

            BigInteger A7 = BarrettReducer.AddMod(A2, A1);
            BigInteger A8 = BarrettReducer.SubMod(A4, A3);

            BigInteger X = BarrettReducer.MulMod(A5, A6);
            BigInteger Y = BarrettReducer.MulMod(A7, A8);

            BigInteger T = BarrettReducer.MulMod(A5, A8);
            BigInteger Z = BarrettReducer.MulMod(A6, A7);

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

            BigInteger A1 = BarrettReducer.MulMod(point.x, point.x);
            BigInteger A2 = BarrettReducer.MulMod(point.y, point.y);

            BigInteger A3t = BarrettReducer.MulMod(point.z, point.z);
            BigInteger A3 = BarrettReducer.AddMod(A3t, A3t);

            BigInteger A4 = BarrettReducer.MulMod(curve.a, A1);
            BigInteger txy = BarrettReducer.AddMod(point.x, point.y);

            BigInteger Bxy = BarrettReducer.MulMod(txy, txy);
            BigInteger A5t = BarrettReducer.SubMod(Bxy, A1);

            BigInteger A5 = BarrettReducer.SubMod(A5t, A2);
            BigInteger A6 = BarrettReducer.AddMod(A2, A4);

            BigInteger A7 = BarrettReducer.SubMod(A6, A3);
            BigInteger A8 = BarrettReducer.SubMod(A4, A2);

            BigInteger X = BarrettReducer.MulMod(A5, A7);
            BigInteger Y = BarrettReducer.MulMod(A6, A8);

            BigInteger T = BarrettReducer.MulMod(A5, A8);
            BigInteger Z = BarrettReducer.MulMod(A6, A7);

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
