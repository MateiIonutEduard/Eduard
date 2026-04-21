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
            if (left == right) return Doubling(curve, right);
            if (left == ECPoint4w.POINT_INFINITY) return right;
            if (right == ECPoint4w.POINT_INFINITY) return left;

            BigInteger A1 = BarrettReducer.MultMod(left.z, left.z);
            BigInteger A2 = BarrettReducer.MultMod(right.z, right.z);

            BigInteger A3 = BarrettReducer.MultMod(left.x, A2);
            BigInteger A4 = BarrettReducer.MultMod(right.x, A1);

            BigInteger B1 = BarrettReducer.MultMod(left.y, right.z);
            BigInteger A5 = BarrettReducer.MultMod(B1, A2);

            BigInteger B2 = BarrettReducer.MultMod(right.y, left.z);
            BigInteger A6 = BarrettReducer.MultMod(B2, A1);

            BigInteger A7 = BarrettReducer.SubMod(A4, A3);
            BigInteger A8 = BarrettReducer.MultMod(A7, A7);
            BigInteger A9 = BarrettReducer.MultMod(A7, A8);

            BigInteger A10 = BarrettReducer.SubMod(A6, A5);
            BigInteger A11 = BarrettReducer.MultMod(A3, A8);

            BigInteger B3 = BarrettReducer.MultMod(A10, A10);
            BigInteger B4 = BarrettReducer.SubMod(B3, A9);

            BigInteger B5 = BarrettReducer.AddMod(A11, A11);
            BigInteger X = BarrettReducer.SubMod(B4, B5);

            BigInteger B6 = BarrettReducer.SubMod(A11, X);
            BigInteger B7 = BarrettReducer.MultMod(A10, B6);

            BigInteger B8 = BarrettReducer.MultMod(A5, A9);
            BigInteger Y = BarrettReducer.SubMod(B7, B8);

            BigInteger B9 = BarrettReducer.MultMod(left.z, right.z);
            BigInteger Z = BarrettReducer.MultMod(A7, B9);

            if (Z == 0) return ECPoint4w.POINT_INFINITY;
            BigInteger Z2 = BarrettReducer.MultMod(Z, Z);

            BigInteger aZ4 = BarrettReducer.MultMod(curve.a, Z2);
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

            BigInteger A1 = BarrettReducer.MultMod(jacobianPoint.x, jacobianPoint.x);
            BigInteger A2 = BarrettReducer.MultMod(jacobianPoint.y, jacobianPoint.y);

            BigInteger A3t = BarrettReducer.MultMod(A2, A2);
            BigInteger A3 = BarrettReducer.MultMod(8, A3t);

            BigInteger B1 = BarrettReducer.MultMod(jacobianPoint.x, A2);
            BigInteger A4 = BarrettReducer.MultMod(4, B1);

            BigInteger B2 = BarrettReducer.MultMod(3, A1);
            BigInteger A5 = BarrettReducer.AddMod(B2, jacobianPoint.aZ4);

            BigInteger A6 = BarrettReducer.MultMod(A5, A5);
            BigInteger Xt = BarrettReducer.AddMod(A4, A4);

            BigInteger X = BarrettReducer.SubMod(A6, Xt);
            BigInteger Yt = BarrettReducer.SubMod(A4, X);

            BigInteger Y = BarrettReducer.MultMod(A5, Yt);
            Y = BarrettReducer.SubMod(Y, A3);

            BigInteger YZ = BarrettReducer.MultMod(jacobianPoint.y, jacobianPoint.z);
            BigInteger Z = BarrettReducer.AddMod(YZ, YZ);

            if (Z == 0) return ECPoint4w.POINT_INFINITY;
            BigInteger Zt = BarrettReducer.MultMod(A3, jacobianPoint.aZ4);

            BigInteger aZ4 = BarrettReducer.AddMod(Zt, Zt);
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
