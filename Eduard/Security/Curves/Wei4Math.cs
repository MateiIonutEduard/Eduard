using Eduard;
using System.Diagnostics;
using Eduard.Security.Primitives;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Implements arithmetic operations for Weierstrass elliptic curves using <br/>
    /// modified Jacobian coordinates (X, Y, Z, aZ^4) as described by Cohen, <br/>
    /// Miyaji, and Ono (1998).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modified Jacobian coordinates extend standard Jacobian representation by <br/>
    /// pre-computing and caching the value aZ^4, where 'a' is the curve parameter <br/>
    /// from the Weierstrass equation y^2 = x^3 + ax + b. This optimization reduces <br/>
    /// the number of field multiplications in point doubling from 8 to 6, <br/>
    /// providing significant performance improvements in scalar multiplication.
    /// </para>
    /// <para>
    /// Based on the seminal paper: Cohen, H., Miyaji, A., Ono, T. (1998). <br/>
    /// "Efficient elliptic curve exponentiation using mixed coordinates". <br/>
    /// Advances in Cryptology - ASIACRYPT'98.
    /// </para>
    /// <para>
    /// The affine point (x, y) is represented as (X, Y, Z, aZ^4) satisfying:
    /// <list type="bullet">
    /// <item><description>x = X / Z^2</description></item>
    /// <item><description>y = Y / Z^3</description></item>
    /// <item><description>aZ^4 = a * Z^4 (pre-computed)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class Wei4Math
    {
        /// <summary>
        /// Adds two modified Jacobian points on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context containing field parameters.</param>
        /// <param name="left">First point in modified Jacobian coordinates.</param>
        /// <param name="right">Second point in modified Jacobian coordinates.</param>
        /// <returns>The sum of the two points in modified Jacobian coordinates.</returns>
        public static ECPoint4w Add(EllipticCurve curve, ECPoint4w left, ECPoint4w right)
        {
            if (left == ECPoint4w.POINT_INFINITY) return right;
            if (right == ECPoint4w.POINT_INFINITY) return left;

            BigInteger p = curve.field;
            BigInteger A1 = (left.z * left.z) % p;
            BigInteger A2 = (right.z * right.z) % p;

            BigInteger A3 = (left.x * A2) % p;
            BigInteger A4 = (right.x * A1) % p;

            BigInteger A5 = (left.y * right.z * A2) % p;
            BigInteger A6 = (right.y * left.z * A1) % p;

            BigInteger A7 = (A4 - A3) % p;
            if (A7 < 0) A7 += p;

            BigInteger A8 = (A7 * A7) % p;
            BigInteger A9 = (A7 * A8) % p;

            BigInteger A10 = (A6 - A5) % p;
            if (A10 < 0) A10 += p;
            BigInteger A11 = (A3 * A8) % p;

            BigInteger X = (A10 * A10 - A9 - 2 * A11) % p;
            if (X < 0) X += p;

            BigInteger Y = (A10 * (A11 - X) - A5 * A9) % p;
            if (Y < 0) Y += p;

            BigInteger Z = (left.z * right.z * A7) % p;
            if (Z == 0) return ECPoint4w.POINT_INFINITY;
            BigInteger Z2 = (Z * Z) % p;

            BigInteger aZ4 = (curve.a * Z2) % p;
            return new ECPoint4w(X, Y, Z, aZ4);
        }

        /// <summary>
        /// Doubles a modified Jacobian point on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context containing field parameters.</param>
        /// <param name="jacobianPoint">The point to double in modified Jacobian coordinates.</param>
        /// <returns>The point doubled (2P) in modified Jacobian coordinates.</returns>
        /// <remarks>
        /// This optimization provides approximately 25% faster point doubling <br/>
        /// compared to standard Jacobian coordinates, significantly accelerating <br/>
        /// scalar multiplication in ECDH and ECDSA operations.
        /// </remarks>
        public static ECPoint4w Doubling(EllipticCurve curve, ECPoint4w jacobianPoint)
        {
            if (jacobianPoint == ECPoint4w.POINT_INFINITY) 
                return ECPoint4w.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger A1 = (jacobianPoint.x * jacobianPoint.x) % p;

            BigInteger A2 = (jacobianPoint.y * jacobianPoint.y) % p;
            BigInteger A3 = (8 * A2 * A2) % p;

            BigInteger A4 = (4 * jacobianPoint.x * A2) % p;
            BigInteger A5 = (3 * A1 + jacobianPoint.aZ4) % p;

            BigInteger X = (A5 * A5 - 2 * A4) % p;
            if (X < 0) X += p;

            BigInteger Y = (A5 * (A4 - X) - A3) % p;
            if (Y < 0) Y += p;

            BigInteger Z = (2 * jacobianPoint.y * jacobianPoint.z) % p;
            if (Z == 0) return ECPoint4w.POINT_INFINITY;

            BigInteger aZ4 = (2 * A3 * jacobianPoint.aZ4) % p;
            return new ECPoint4w(X, Y, Z, aZ4);
        }

        /// <summary>
        /// Computes the additive inverse of a modified Jacobian point.
        /// </summary>
        /// <param name="curve">The elliptic curve context containing field parameters.</param>
        /// <param name="point">The point to negate in modified Jacobian coordinates.</param>
        /// <returns>The point -P such that P + (-P) = point at infinity.</returns>
        /// <remarks>
        /// For a point P = (X, Y, Z, aZ^4), its inverse is -P = (X, -Y, Z, aZ^4). <br/>
        /// The Y-coordinate is negated modulo the field prime while the cached <br/>
        /// aZ^4 value remains unchanged as it depends only on Z.
        /// </remarks>
        public static ECPoint4w Negate(EllipticCurve curve, ECPoint4w point)
        {
            if (point == ECPoint4w.POINT_INFINITY) return ECPoint4w.POINT_INFINITY;
            return new ECPoint4w(point.x, curve.field - point.y, point.z, point.aZ4);
        }
    }

}
