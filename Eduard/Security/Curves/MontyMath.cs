using System;
using System.Diagnostics;
using Eduard.Security.Primitives;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Provides mathematical operations for points on Montgomery curves in affine coordinates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implements arithmetic for Montgomery curves of the form: B * y^2 = x^3 + A * x^2 + x (mod p). <br/>
    /// This representation is particularly efficient for constant-time scalar multiplication using the <br/>
    /// Montgomery ladder, though only affine operations are currently implemented.
    /// </para>
    /// <para>
    /// Montgomery curves are widely used in protocols like X25519 (Curve25519) due to their <br/>
    /// resistance to timing attacks and efficient ladder-based scalar multiplication.
    /// </para>
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class MontyMath
    {
        /// <summary>
        /// Adds two affine points on a Montgomery curve using standard affine coordinates.
        /// </summary>
        /// <param name="curve">The Montgomery curve context containing parameters A, B, and field prime.</param>
        /// <param name="left">First point to add in affine coordinates.</param>
        /// <param name="right">Second point to add in affine coordinates.</param>
        /// <returns>The sum of the points in affine coordinates.</returns>
        /// <remarks>
        /// Handles all special cases including point at infinity, point doubling, <br/>
        /// and vertical line scenarios (resulting in point at infinity).
        /// </remarks>
        public static ECPoint Add(MontgomeryCurve curve, ECPoint left, ECPoint right)
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
                lambda = BarrettReducer.MultMod(inv, yDiff);
            }
            else
            {
                xDiff = BarrettReducer.MultMod(left.x, left.x);
                xDiff = BarrettReducer.MultMod(3, xDiff);

                BigInteger Ax = BarrettReducer.MultMod(curve.A, left.x);
                Ax = BarrettReducer.AddMod(Ax, Ax);

                Ax = BarrettReducer.AddMod(Ax, 1);
                xDiff = BarrettReducer.AddMod(xDiff, Ax);

                yDiff = BarrettReducer.MultMod(curve.B, left.y);
                yDiff = BarrettReducer.AddMod(yDiff, yDiff);
                if (yDiff == 0) return ECPoint.POINT_INFINITY;

                inv = BarrettReducer.InvMod(yDiff);
                lambda = BarrettReducer.MultMod(xDiff, inv);
            }

            BigInteger temp = BarrettReducer.MultMod(lambda, lambda);
            temp = BarrettReducer.MultMod(curve.B, temp);
            BigInteger delta = BarrettReducer.AddMod(curve.A, left.x);

            delta = BarrettReducer.AddMod(delta, right.x);
            BigInteger x = BarrettReducer.SubMod(temp, delta);

            BigInteger y = BarrettReducer.SubMod(left.x, x);
            y = BarrettReducer.MultMod(y, lambda);

            y = BarrettReducer.SubMod(y, left.y);
            return new ECPoint(x, y);
        }

        /// <summary>
        /// Performs scalar multiplication on a Montgomery curve point using affine coordinates.
        /// </summary>
        /// <param name="curve">The Montgomery curve context.</param>
        /// <param name="k">The scalar multiplier.</param>
        /// <param name="point">Base point to multiply in affine coordinates.</param>
        /// <param name="opMode">Execution mode (only affine mode is currently supported).</param>
        /// <returns>The resulting point k * point in affine coordinates.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="k"/> is negative.
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// Thrown when <paramref name="opMode"/> is not <see cref="ECMode.EC_STANDARD_AFFINE"/>,
        /// as projective coordinate implementations are not yet available.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Currently only supports affine binary method. For production use with Montgomery curves, <br/>
        /// consider implementing the Montgomery ladder in projective coordinates for better <br/>
        /// performance and side-channel resistance.
        /// </para>
        /// <para>
        /// Note: The Montgomery ladder (EC_SECURE mode) is not yet implemented, though it would <br/>
        /// provide the constant-time guarantees that make Montgomery curves attractive for <br/>
        /// protocols like X25519.
        /// </para>
        /// </remarks>
        public static ECPoint Multiply(MontgomeryCurve curve, BigInteger k, ECPoint point, ECMode opMode = ECMode.EC_STANDARD_AFFINE)
        {
            if (k < 0)
                throw new ArgumentException(
                    "Scalar multiplier must be non-negative.", 
                    nameof(k));

            if (k == 0 || point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            ECPoint affinePoint = point;
            BigInteger nk = k;

            if(k < 0)
            {
                affinePoint = MontyMath.Negate(curve, affinePoint);
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
                throw new NotImplementedException(
                    "Projective coordinate arithmetic for Montgomery curves is not yet implemented. " +
                    "Use EC_STANDARD_AFFINE mode instead.");
            else if (opMode == ECMode.EC_SECURE)
                throw new NotImplementedException(
                    "Montgomery ladder (constant-time) implementation is not yet available. " +
                    "This feature is planned for future releases.");
            else
                throw new NotImplementedException(
                    $"Operation mode {opMode} is not supported for Montgomery curves. " +
                    $"Only {ECMode.EC_STANDARD_AFFINE} is currently implemented.");

            return result;
        }

        /// <summary>
        /// Computes the additive inverse of an affine point on a Montgomery curve.
        /// </summary>
        /// <param name="curve">The Montgomery curve context.</param>
        /// <param name="point">The point to negate in affine coordinates.</param>
        /// <returns>The point -P such that P + (-P) = point at infinity.</returns>
        /// <remarks>
        /// For a Montgomery curve, the inverse of point (x, y) is (x, -y mod p). <br/>
        /// The point at infinity is its own inverse.
        /// </remarks>
        public static ECPoint Negate(MontgomeryCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            return new ECPoint(point.x, curve.field - point.y);
        }
    }
}
