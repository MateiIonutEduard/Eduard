using Eduard;
using Eduard.Security;
using System.Diagnostics;
using Eduard.Security.Primitives;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Implements arithmetic operations for twisted Edwards curves using projective <br/>
    /// coordinates (X, Y, Z), based on the unified addition formulas from Bernstein et al. (2008).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Reference: Bernstein, D.J., Birkner, P., Joye, M., Lange, T. and Peters, C. (2008). <br/>
    /// "Twisted Edwards Curves". International Conference on Cryptology in Africa (AFRICACRYPT), <br/>
    /// pp. 389-405. Springer Berlin Heidelberg.
    /// </para>
    /// <para>
    /// The unified addition formulas work for both point addition and doubling, providing <br/>
    /// complete addition on twisted Edwards curves without exceptional cases. This property <br/>
    /// is particularly valuable for side-channel resistant implementations.
    /// </para>
    /// <para>
    /// For a twisted Edwards curve of the form a * x^2 + y^2 = 1 + d * x^2 * y^2, points are <br/>
    /// represented in projective coordinates (X, Y, Z) where the affine point (x, y) is recovered as <br/>
    /// x = X/Z, y = Y/Z when Z != 0. The point at infinity is represented with Z = 0.
    /// </para>
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class Ed3Math
    {
        /// <summary>
        /// Adds two projective points on a twisted Edwards curve using the unified formula.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context containing parameters a, d, and field prime.</param>
        /// <param name="left">First point in projective coordinates.</param>
        /// <param name="right">Second point in projective coordinates.</param>
        /// <returns>The sum of the two points in projective coordinates.</returns>
        /// <remarks>
        /// <para>
        /// Implements the unified addition formula from Bernstein et al. (2008) <br/>
        /// which works for both addition of distinct points and point doubling.
        /// </para>
        /// <para>
        /// Handles point at infinity cases and returns the point at infinity <br/>
        /// when the result is the identity element (Z3 = 0).
        /// </para>
        /// </remarks>
        public static ECPoint3 UnifiedAdd(TwistedEdwardsCurve curve, ECPoint3 left, ECPoint3 right)
        {
            if (left == ECPoint3.POINT_INFINITY) return right;
            if (right == ECPoint3.POINT_INFINITY) return left;

            BigInteger p = curve.field;
            BigInteger A1 = BarrettReducer.MulMod(left.z, right.z);

            BigInteger A2 = BarrettReducer.MulMod(A1, A1);
            BigInteger A3 = BarrettReducer.MulMod(left.x, right.x);

            BigInteger A4 = BarrettReducer.MulMod(left.y, right.y);
            BigInteger A5t = BarrettReducer.MulMod(A3, A4);
            BigInteger A5 = BarrettReducer.MulMod(curve.d, A5t);

            BigInteger A6 = BarrettReducer.SubMod(A2, A5);
            BigInteger A7 = BarrettReducer.AddMod(A2, A5);

            BigInteger A8x = BarrettReducer.AddMod(left.x, left.y);
            BigInteger A8y = BarrettReducer.AddMod(right.x, right.y);

            BigInteger A8 = BarrettReducer.MulMod(A8x, A8y);
            BigInteger aA3 = BarrettReducer.MulMod(curve.a, A3);

            BigInteger A9 = BarrettReducer.SubMod(A4, aA3);
            BigInteger A83 = BarrettReducer.SubMod(A8, A3);
            BigInteger A10 = BarrettReducer.SubMod(A83, A4);

            BigInteger B1 = BarrettReducer.MulMod(A1, A6);
            BigInteger X = BarrettReducer.MulMod(B1, A10);

            BigInteger B2 = BarrettReducer.MulMod(A7, A9);
            BigInteger Y = BarrettReducer.MulMod(A1, B2);
            BigInteger Z = BarrettReducer.MulMod(A6, A7);

            if (Z == 0) return ECPoint3.POINT_INFINITY;
            return new ECPoint3(X, Y, Z);
        }

        /// <summary>
        /// Doubles a projective point on a twisted Edwards curve using the unified formula.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context containing parameters a, d, and field prime.</param>
        /// <param name="point">The point to double in projective coordinates.</param>
        /// <returns>The doubled point (2P) in projective coordinates.</returns>
        /// <remarks>
        /// Implements the doubling formula specialized from the unified addition when both <br/>
        /// operands are equal. Returns the point at infinity when doubling a point of order 2 (Z3 = 0).
        /// </remarks>
        public static ECPoint3 UnifiedDoubling(TwistedEdwardsCurve curve, ECPoint3 point)
        {
            if (point == ECPoint3.POINT_INFINITY)
                return ECPoint3.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger B1 = BarrettReducer.AddMod(point.x, point.y);

            BigInteger A1 = BarrettReducer.MulMod(B1, B1);
            BigInteger A2 = BarrettReducer.MulMod(point.x, point.x);

            BigInteger A3 = BarrettReducer.MulMod(point.y, point.y);
            BigInteger A4 = BarrettReducer.MulMod(curve.a, A2);

            BigInteger A5 = BarrettReducer.AddMod(A4, A3);
            BigInteger A6 = BarrettReducer.MulMod(point.z, point.z);
            BigInteger B2 = BarrettReducer.AddMod(A6, A6);

            BigInteger A7 = BarrettReducer.SubMod(A5, B2);
            BigInteger B3 = BarrettReducer.SubMod(A1, A2);
            B3 = BarrettReducer.SubMod(B3, A3);

            BigInteger X = BarrettReducer.MulMod(A7, B3);
            BigInteger B4 = BarrettReducer.SubMod(A4, A3);

            BigInteger Y = BarrettReducer.MulMod(A5, B4);
            BigInteger Z = BarrettReducer.MulMod(A5, A7);

            if (Z == 0) return ECPoint3.POINT_INFINITY;
            return new ECPoint3(X, Y, Z);
        }

        /// <summary>
        /// Computes the additive inverse of a projective point on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context containing the field prime.</param>
        /// <param name="point">The point to negate in projective coordinates.</param>
        /// <returns>The point -P such that P + (-P) = point at infinity.</returns>
        /// <remarks>
        /// For a twisted Edwards curve, the inverse of point (X, Y, Z) is (-X, Y, Z). <br/>
        /// The X-coordinate is negated modulo the field prime while Y and Z <br/>
        /// remain unchanged. The point at infinity is its own inverse.
        /// </remarks>
        public static ECPoint3 Negate(TwistedEdwardsCurve curve, ECPoint3 point)
        {
            BigInteger x = curve.field - point.x;
            return new ECPoint3(x, point.y, point.z);
        }
    }
}
