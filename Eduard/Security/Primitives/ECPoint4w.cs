using Eduard;
using System;
using System.Diagnostics;

namespace Eduard.Security.Primitives
{
    /// <summary>
    /// Represents a point on a Weierstrass elliptic curve in modified Jacobian coordinates (X, Y, Z, aZ^4).
    /// <remarks>
    /// <para>
    /// Modified Jacobian coordinates extend standard Jacobian representation by pre-computing<br/>
    /// the value aZ^4 where 'a' is the curve parameter from the Weierstrass curve.
    /// </para>
    /// <para>
    /// The affine point (x, y) is represented as:
    /// <list type="bullet">
    /// <item><description>x = X / Z^2</description></item>
    /// <item><description>y = Y / Z^3</description></item>
    /// <item><description>aZ⁴ = a * Z^4 (pre-computed for efficiency)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The point at infinity is represented with Z = 0 and aZ^4 = 0. This representation <br/>
    /// reduces the number of field multiplications in point doubling from 8 to 6, providing <br/> 
    /// significant performance benefits in scalar multiplication algorithms.
    /// </para>
    /// </remarks>
    /// </summary>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public struct ECPoint4w : IEquatable<ECPoint4w>
    {
        /// <summary>
        /// The X-coordinate in modified Jacobian representation.
        /// </summary>
        public BigInteger x;

        /// <summary>
        /// The Y-coordinate in modified Jacobian representation.
        /// </summary>
        public BigInteger y;

        /// <summary>
        /// The Z-coordinate in modified Jacobian representation.
        /// </summary>
        /// <remarks>
        /// Z = 0 indicates the point at infinity. For finite points, Z is non-zero.
        /// </remarks>
        public BigInteger z;

        /// <summary>
        /// The pre-computed value aZ^4 where 'a' is the Weierstrass curve parameter.
        /// </summary>
        /// <remarks>
        /// This cached value eliminates redundant computations in point doubling.<br/>
        /// For the point at infinity, this must be 0 to maintain consistency.
        /// </remarks>
        public BigInteger aZ4;

        /// <summary>
        /// Initializes a new modified Jacobian point with the specified coordinates.
        /// </summary>
        /// <param name="x">The projective X-coordinate.</param>
        /// <param name="y">The projective Y-coordinate.</param>
        /// <param name="z">The projective Z-coordinate (zero indicates point at infinity).</param>
        /// <param name="aZ4">The pre-computed value aZ^4 (must be zero for point at infinity).</param>
        /// <exception cref="NullReferenceException">Thrown when any coordinate is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when point at infinity has non-zero aZ⁴.</exception>
        public ECPoint4w(BigInteger x, BigInteger y, BigInteger z, BigInteger aZ4)
        {
            if (ReferenceEquals(x, null))
                throw new NullReferenceException("The projective X-coordinate cannot be null.");

            if (ReferenceEquals(y, null))
                throw new NullReferenceException("The projective Y-coordinate cannot be null.");

            if (ReferenceEquals(z, null))
                throw new NullReferenceException("The projective Z-coordinate cannot be null.");

            if (ReferenceEquals(aZ4, null))
                throw new NullReferenceException("The projective aZ⁴-coordinate cannot be null.");

            /* validate point at infinity invariant */
            if (z == 0 && aZ4 != 0)
                throw new InvalidOperationException(
                    "Point at infinity must have aZ⁴ = 0.");

            this.x = x;
            this.y = y;
            this.z = z;
            this.aZ4 = aZ4;
        }

        /// <summary>
        /// Gets the point at infinity for Weierstrass curves in modified Jacobian coordinates.
        /// </summary>
        /// <remarks>
        /// The point at infinity serves as the identity element in the elliptic curve group.<br/>
        /// It is uniquely represented with Z = 0 and aZ^4 = 0. Coordinates are set to <br/>
        /// (1, 1, 0, 0) for consistency, though X and Y values are irrelevant when Z = 0.
        /// </remarks>
        public static ECPoint4w POINT_INFINITY
        {
            get
            {
                var infinity = new ECPoint4w(1, 1, 0, 0);
                return infinity;
            }
        }

        /// <summary>
        /// Indicates whether the current point is equal to another modified Jacobian point.
        /// </summary>
        /// <param name="other">The point to compare with this point.</param>
        /// <returns>true if the points represent the same geometric point; otherwise false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if either point violates the invariant (Z=0 but aZ⁴≠0).
        /// </exception>
        /// <remarks>
        /// Two points are considered equal if:
        /// <list type="bullet">
        /// <item><description>Both are the point at infinity (Z = 0, aZ^4 = 0)</description></item>
        /// <item><description>Both are finite points with identical (X, Y, Z, aZ^4) representations</description></item>
        /// </list>
        /// </remarks>
        public bool Equals(ECPoint4w other)
        {
            bool isInfinitySelf = z == 0;
            bool isInfinityOther = other.z == 0;

            if (isInfinitySelf && aZ4 != 0)
                throw new InvalidOperationException(
                    "Current point at infinity has non-zero aZ⁴.");

            if (isInfinityOther && other.aZ4 != 0)
                throw new InvalidOperationException(
                    "Other point at infinity has non-zero aZ⁴.");

            if (isInfinitySelf != isInfinityOther)
                return false;

            if (isInfinitySelf && isInfinityOther)
                return true;

            /* compare all coordinates */
            return x == other.x && y == other.y &&
                   z == other.z && aZ4 == other.aZ4;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current modified Jacobian point.
        /// </summary>
        /// <param name="obj">The object to compare with the current point.</param>
        /// <returns>true if the object is an ECPoint4w with identical coordinates; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ECPoint4w))
                return false;

            ECPoint4w other = (ECPoint4w)obj;
            return Equals(other);
        }

        /// <summary>
        /// Equality operator for modified Jacobian projective points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns>true if the points represent the same geometric point; otherwise false.</returns>
        public static bool operator ==(ECPoint4w left, ECPoint4w right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for modified Jacobian projective points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns>true if the points represent different geometric points; otherwise false.</returns>
        public static bool operator !=(ECPoint4w left, ECPoint4w right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code for this modified Jacobian point.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>
        /// The hash code is derived from the X, Y, Z, and aZ^4 coordinates to ensure uniqueness.<br/>
        /// Points at infinity (Z=0, aZ^4=0) all hash to the same value regardless of X and Y.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                if (z == 0 && aZ4 == 0)
                    return 0;

                int hash = 17;
                hash = hash * 31 + x.GetHashCode();
                hash = hash * 31 + y.GetHashCode();

                hash = hash * 31 + z.GetHashCode();
                hash = hash * 31 + aZ4.GetHashCode();
                return hash;
            }
        }
    }
}
