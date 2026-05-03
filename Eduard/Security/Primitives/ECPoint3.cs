using System;

namespace Eduard.Security.Primitives
{
    /// <summary>
    /// Represents a point on a twisted Edwards curve in projective coordinates (X, Y, Z).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Projective coordinates (X, Y, Z) provide an efficient representation for point operations on <br/>
    /// twisted Edwards curves. The affine point (x, y) is recovered as x = X/Z, y = Y/Z when Z != 0.
    /// </para>
    /// <para>
    /// The point at infinity is represented with Z = 0. This representation eliminates <br/>
    /// modular inversions in point addition, replacing them with multiplication operations <br/>
    /// for improved performance in scalar multiplication algorithms.
    /// </para>
    /// </remarks>
    public struct ECPoint3 : IEquatable<ECPoint3>
    {
        /// <summary>
        /// The projective X-coordinate.
        /// </summary>
        public BigInteger x;

        /// <summary>
        /// The projective Y-coordinate.
        /// </summary>
        public BigInteger y;

        /// <summary>
        /// The projective Z-coordinate.
        /// </summary>
        /// <remarks>
        /// Z = 0 indicates the point at infinity. <br/>
        /// For finite points, Z is non-zero.
        /// </remarks>
        public BigInteger z;

        /// <summary>
        /// Initializes a new projective point with the specified coordinates.
        /// </summary>
        /// <param name="x">The projective X-coordinate.</param>
        /// <param name="y">The projective Y-coordinate.</param>
        /// <param name="z">The projective Z-coordinate (zero indicates point at infinity).</param>
        /// <exception cref="ArgumentNullException">Thrown when any coordinate is null.</exception>
        public ECPoint3(BigInteger x, BigInteger y, BigInteger z)
        {
            if (ReferenceEquals(x, null))
                throw new ArgumentNullException(nameof(x),
                    "The projective X-coordinate cannot be null.");

            if (ReferenceEquals(null, y))
                throw new ArgumentNullException(nameof(y),
                    "The projective Y-coordinate cannot be null.");

            if (ReferenceEquals(z, null))
                throw new ArgumentNullException(nameof(z),
                    "The projective Z-coordinate cannot be null.");

            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Gets the point at infinity for twisted Edwards curves in projective coordinates.
        /// </summary>
        /// <remarks>
        /// The point at infinity serves as the identity element in the elliptic curve group. <br/>
        /// It is represented with Z = 0. Coordinates are set to (0, 1, 0) for consistency, <br/>
        /// though X and Y values are irrelevant when Z = 0.
        /// </remarks>
        public static ECPoint3 POINT_INFINITY
        {
            get
            {
                var infinity = new ECPoint3(0, 1, 0);
                return infinity;
            }
        }

        /// <summary>
        /// Equality operator for projective points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns>true if the points represent the same geometric point; otherwise false.</returns>
        public static bool operator ==(ECPoint3 left, ECPoint3 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for projective points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns>true if the points represent different geometric points; otherwise false.</returns>
        public static bool operator !=(ECPoint3 left, ECPoint3 right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code for this projective point.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>
        /// The hash code is derived from the X, Y, and Z coordinates to ensure uniqueness. <br/>
        /// Points at infinity (Z = 0) all hash to the same value regardless of X and Y.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                if (z == 0) return 0;
                int xHash = x.GetHashCode();
                int yHash = y.GetHashCode();

                int zHash = z.GetHashCode();
                return (xHash << 2) ^ (yHash << 1) ^ zHash;
            }
        }

        /// <summary>
        /// Indicates whether the current point is equal to another projective point.
        /// </summary>
        /// <param name="other">The point to compare with this point.</param>
        /// <returns>true if the points represent the same geometric point; otherwise false.</returns>
        /// <remarks>
        /// Two points are considered equal if:
        /// <list type="bullet">
        /// <item><description>Both are the point at infinity (Z = 0)</description></item>
        /// <item><description>Both are finite points with identical (X, Y, Z) representations</description></item>
        /// </list>
        /// </remarks>
        public bool Equals(ECPoint3 other)
        {
            bool isInfinitySelf = z == 0;
            bool isInfinityOther = other.z == 0;

            if (isInfinitySelf != isInfinityOther)
                return false;

            if (isInfinitySelf && isInfinityOther)
                return true;

            bool sameXCoord = x == other.x;
            bool sameYCoord = y == other.y;

            bool sameZCoord = z == other.z;
            return sameXCoord && sameYCoord
                && sameZCoord;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current projective point.
        /// </summary>
        /// <param name="obj">The object to compare with the current point.</param>
        /// <returns>true if the object is an ECPoint3 with identical coordinates; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ECPoint3))
                return false;

            ECPoint3 other = (ECPoint3)obj;
            return Equals(other);
        }
    }
}
