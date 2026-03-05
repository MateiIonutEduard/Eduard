using Eduard;
using System.Diagnostics;
using Eduard.Security.Primitives;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Implements arithmetic operations for Weierstrass elliptic curves using <br/>
    /// Jacobian projective coordinates (X, Y, Z) where the affine point (x, y) <br/>
    /// is represented as (X/Z^2, Y/Z^3).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Based on "Guide to Elliptic Curve Cryptography" by Hankerson, Vanstone, and Menezes (2004). <br/>
    /// Jacobian coordinates eliminate modular inversions in point addition and doubling, <br/>
    /// replacing them with cheaper multiplication operations. The point at infinity is <br/>
    /// represented with Z = 0.
    /// </para>
    /// <para>
    /// Optimized formulas are used for curves with a = -3, reducing the number of <br/>
    /// field multiplications in point doubling operations.
    /// </para>
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class Wei3Math
    {
        /// <summary>
        /// Adds two Jacobian projective points on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context containing field parameters.</param>
        /// <param name="left">First point in Jacobian coordinates.</param>
        /// <param name="right">Second point in Jacobian coordinates.</param>
        /// <returns>The sum of the two points in Jacobian coordinates.</returns>
        /// <remarks>
        /// Implements the mixed Jacobian addition formula from Algorithm 3.22 in <br/>
        /// "Guide to Elliptic Curve Cryptography". Handles point at infinity cases <br/>
        /// and returns infinity when the result is the point at infinity (Z = 0).
        /// </remarks>
        public static ECPoint3w Add(EllipticCurve curve, ECPoint3w left, ECPoint3w right)
        {
            if (left == ECPoint3w.POINT_INFINITY) return right;
            if (right == ECPoint3w.POINT_INFINITY) return left;
            BigInteger p = curve.field;

            BigInteger A1 = (left.x * ((right.z * right.z) % p)) % p;
            BigInteger A2 = (right.x * ((left.z * left.z) % p)) % p;

            BigInteger A3 = (left.y * (((right.z * right.z) % p) * right.z) % p) % p;
            BigInteger A4 = (right.y * (((left.z * left.z) % p) * left.z) % p) % p;

            BigInteger A5 = A2 - A1;
            if (A5 < 0) A5 += p;

            BigInteger A6 = A4 - A3;
            if (A6 < 0) A6 += p;

            BigInteger A7 = (A5 * A5) % p;
            BigInteger A8 = (A5 * A7) % p;

            BigInteger A9 = (A1 * A7) % p;
            BigInteger X = (((A6 * A6) % p) - A8 - 2 * A9) % p;
            if (X < 0) X += p;

            BigInteger Y = (((A6 * (A9 - X)) % p) - A3 * A8) % p;
            if (Y < 0) Y += p;

            BigInteger Z = (((left.z * right.z) % p) * A5) % p;
            if (Z == 0) return ECPoint3w.POINT_INFINITY;
            return new ECPoint3w(X, Y, Z);
        }

        /// <summary>
        /// Doubles a Jacobian projective point on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context containing field parameters and coefficient 'a'.</param>
        /// <param name="jacobianPoint">The point to double in Jacobian coordinates.</param>
        /// <returns>The point doubled (2P) in Jacobian coordinates.</returns>
        /// <remarks>
        /// <para>
        /// Implements optimized doubling formulas from Algorithm 3.21 in <br/>
        /// "Guide to Elliptic Curve Cryptography". When the curve parameter a = -3, <br/>
        /// a specialized formula is used that reduces the number of field multiplications <br/>
        /// from 8 to 6, providing significant performance improvements for curves <br/>
        /// like NIST P-256, P-384, and P-521.
        /// </para>
        /// <para>
        /// Returns the point at infinity if the result is the point at infinity (Z = 0), <br/>
        /// which occurs when doubling a point of order 2.
        /// </para>
        /// </remarks>
        public static ECPoint3w Doubling(EllipticCurve curve, ECPoint3w jacobianPoint)
        {
            if (jacobianPoint == ECPoint3w.POINT_INFINITY) return ECPoint3w.POINT_INFINITY;
            BigInteger p = curve.field;

            BigInteger A1 = (jacobianPoint.y * jacobianPoint.y) % p;
            BigInteger A2 = (4 * jacobianPoint.x * A1) % p;

            BigInteger A3 = (8 * A1 * A1) % p;
            BigInteger A4 = 0;

            if (curve.a != p - 3)
                A4 = ((3 * jacobianPoint.x * jacobianPoint.x) % p + (curve.a * ((jacobianPoint.z * jacobianPoint.z) % p) * ((jacobianPoint.z * jacobianPoint.z) % p)) % p) % p;
            else
            {
                /* special case when Weierstrass curve parameter a = -3 */
                BigInteger Z12 = (jacobianPoint.z * jacobianPoint.z) % p;
                A4 = (3 * (jacobianPoint.x - Z12) * (jacobianPoint.x + Z12)) % p;
                if (A4 < 0) A4 += p;
            }

            BigInteger X = ((A4 * A4) % p - 2 * A2) % p;
            if (X < 0) X += p;

            BigInteger Y = (A4 * (A2 - X) - A3) % p;
            if (Y < 0) Y += p;

            BigInteger Z = (2 * jacobianPoint.y * jacobianPoint.z) % p;
            if (Z == 0) return ECPoint3w.POINT_INFINITY;
            return new ECPoint3w(X, Y, Z);
        }

        /// <summary>
        /// Computes the additive inverse of a Jacobian projective point.
        /// </summary>
        /// <param name="curve">The elliptic curve context containing field parameters.</param>
        /// <param name="point">The point to negate in Jacobian coordinates.</param>
        /// <returns>The point -P such that P + (-P) = point at infinity.</returns>
        /// <remarks>
        /// For a point P = (X, Y, Z) in Jacobian coordinates, its inverse is -P = (X, -Y, Z). <br/>
        /// The Y-coordinate is negated modulo the field prime. The point at infinity
        /// is its own inverse.
        /// </remarks>
        public static ECPoint3w Negate(EllipticCurve curve, ECPoint3w point)
        {
            if (point == ECPoint3w.POINT_INFINITY) return ECPoint3w.POINT_INFINITY;
            return new ECPoint3w(point.x, curve.field - point.y, point.z);
        }
    }

}
