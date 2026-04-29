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
            if (left == right) return Doubling(curve, left);
            if (left == ECPoint3w.POINT_INFINITY) return right;
            if (right == ECPoint3w.POINT_INFINITY) return left;

            BigInteger B1 = BarrettReducer.MultMod(right.z, right.z);
            BigInteger A1 = BarrettReducer.MultMod(left.x, B1);

            BigInteger B2 = BarrettReducer.MultMod(left.z, left.z);
            BigInteger A2 = BarrettReducer.MultMod(right.x, B2);

            BigInteger B3 = BarrettReducer.MultMod(B1, right.z);
            BigInteger A3 = BarrettReducer.MultMod(left.y, B3);

            BigInteger B4 = BarrettReducer.MultMod(B2, left.z);
            BigInteger A4 = BarrettReducer.MultMod(right.y, B4);

            BigInteger A5 = BarrettReducer.SubMod(A2, A1);
            BigInteger A6 = BarrettReducer.SubMod(A4, A3);

            BigInteger A7 = BarrettReducer.MultMod(A5, A5);
            BigInteger A8 = BarrettReducer.MultMod(A5, A7);

            BigInteger A9 = BarrettReducer.MultMod(A1, A7);
            BigInteger B5 = BarrettReducer.MultMod(A6, A6);

            BigInteger B6 = BarrettReducer.AddMod(A9, A9);
            BigInteger X = BarrettReducer.SubMod(B5, A8);

            X = BarrettReducer.SubMod(X, B6);
            BigInteger B7 = BarrettReducer.MultMod(A3, A8);

            BigInteger B8 = BarrettReducer.SubMod(A9, X);
            BigInteger B9 = BarrettReducer.MultMod(A6, B8);
            BigInteger Y = BarrettReducer.SubMod(B9, B7);

            BigInteger Z12 = BarrettReducer.MultMod(left.z, right.z);
            BigInteger Z = BarrettReducer.MultMod(A5, Z12);

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

            BigInteger A1 = BarrettReducer.MultMod(jacobianPoint.y, jacobianPoint.y);
            BigInteger B1 = BarrettReducer.MultMod(jacobianPoint.x, A1);

            BigInteger A2 = BarrettReducer.MultMod(4, B1);
            BigInteger B2 = BarrettReducer.MultMod(A1, A1);

            BigInteger A3 = BarrettReducer.MultMod(8, B2);
            BigInteger A4 = 0;

            if (curve.a != p - 3)
            {
                BigInteger X12 = BarrettReducer.MultMod(jacobianPoint.x, jacobianPoint.x);
                BigInteger Z12 = BarrettReducer.MultMod(jacobianPoint.z, jacobianPoint.z);
                BigInteger Z21 = BarrettReducer.MultMod(Z12, Z12);

                BigInteger B3 = BarrettReducer.MultMod(curve.a, Z21);
                BigInteger B4 = BarrettReducer.MultMod(3, X12); 
                A4 = BarrettReducer.AddMod(B4, B3);
            }
            else
            {
                /* special case when Weierstrass curve parameter a = -3 */
                BigInteger Z12 = BarrettReducer.MultMod(jacobianPoint.z, jacobianPoint.z);
                BigInteger B3 = BarrettReducer.SubMod(jacobianPoint.x, Z12);
                BigInteger B4 = BarrettReducer.AddMod(jacobianPoint.x, Z12);
                A4 = BarrettReducer.MultMod(B3, B4);
                A4 = BarrettReducer.MultMod(3, A4);
            }

            BigInteger A5 = BarrettReducer.MultMod(A4, A4);
            BigInteger At = BarrettReducer.AddMod(A2, A2);

            BigInteger X = BarrettReducer.SubMod(A5, At);
            BigInteger A6 = BarrettReducer.SubMod(A2, X);

            BigInteger B5 = BarrettReducer.MultMod(A4, A6);
            BigInteger Y = BarrettReducer.SubMod(B5, A3);

            BigInteger B6 = BarrettReducer.MultMod(jacobianPoint.y, jacobianPoint.z);
            BigInteger Z = BarrettReducer.AddMod(B6, B6);

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
