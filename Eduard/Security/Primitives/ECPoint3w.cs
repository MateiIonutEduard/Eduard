using Eduard;
using System;
using System.Diagnostics;

namespace Eduard.Security.Primitives
{
    /// <summary>
    /// Represents a point on a Weierstrass elliptic curve in Jacobian projective coordinates (X, Y, Z).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Jacobian coordinates provide an efficient representation for point operations on Weierstrass curves.<br/>
    /// The affine point (x, y) is represented as (X, Y, Z) satisfying:
    /// <list type="bullet">
    /// <item><description>x = X / Z^2</description></item>
    /// <item><description>y = Y / Z^3</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The point at infinity is represented with Z = 0. Arithmetic in projective coordinates<br/>
    /// avoids expensive modular inversions, replacing them with multiplication operations.
    /// </para>
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public struct ECPoint3w : IEquatable<ECPoint3w>
    {
        /// <summary>
        /// The X-coordinate in Jacobian projective representation.
        /// </summary>
        public BigInteger x;

        /// <summary>
        /// The Y-coordinate in Jacobian projective representation.
        /// </summary>
        public BigInteger y;

        /// <summary>
        /// The Z-coordinate in Jacobian projective representation.
        /// </summary>
        /// <remarks>
        /// Z = 0 indicates the point at infinity. Otherwise, Z is non-zero <br/> and typically
        /// normalized to 1 for affine points after conversion.
        /// </remarks>
        public BigInteger z;

        /// <summary>
        /// Initializes a new Jacobian projective point with the specified coordinates.
        /// </summary>
        /// <param name="x">The projective X-coordinate.</param>
        /// <param name="y">The projective Y-coordinate.</param>
        /// <param name="z">The projective Z-coordinate (zero indicates point at infinity).</param>
        /// <exception cref="ArgumentNullException">Thrown when any coordinate is null.</exception>
        public ECPoint3w(BigInteger x, BigInteger y, BigInteger z)
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
        /// Gets the point at infinity for Weierstrass curves in Jacobian coordinates.
        /// </summary>
        /// <remarks>
        /// The point at infinity serves as the identity element in the elliptic curve group. <br/>
        /// It is uniquely represented with Z = 0. For consistency, coordinates are set to (1, 1, 0).
        /// </remarks>
        public static ECPoint3w POINT_INFINITY
        {
            get 
            {
                var infinity = new ECPoint3w(1, 1, 0);
                return infinity;
            }
        }

        /// <summary>
        /// Indicates whether the current point is equal to another Jacobian point.
        /// </summary>
        /// <param name="other">The point to compare with this point.</param>
        /// <returns>true if the points represent the same geometric point; otherwise false.</returns>
        /// <remarks>
        /// Two points are considered equal if:
        /// <list type="bullet">
        /// <item><description>Both are the point at infinity (Z = 0), regardless of X,Y coordinates</description></item>
        /// <item><description>Both are finite points with identical (X, Y, Z) representations</description></item>
        /// </list>
        /// </remarks>
        public bool Equals(ECPoint3w other)
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
        /// Determines whether the specified object is equal to the current Jacobian point.
        /// </summary>
        /// <param name="obj">The object to compare with the current point.</param>
        /// <returns>true if the object is an ECPoint3w with identical coordinates; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (!(obj is ECPoint3w))
                return false;

            ECPoint3w other = (ECPoint3w)obj;
            return Equals(other);
        }

        /// <summary>
        /// Equality operator for Jacobian projective points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns>true if the points have identical projective coordinates; otherwise false.</returns>
        public static bool operator ==(ECPoint3w left, ECPoint3w right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for Jacobian projective points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns>true if the points have different projective coordinates; otherwise false.</returns>
        public static bool operator !=(ECPoint3w left, ECPoint3w right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code for this Jacobian projective point.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>
        /// The hash code is derived from the X, Y, and Z coordinates to ensure uniqueness.<br/>
        /// Points at infinity (Z=0) all hash to the same value regardless of X and Y.
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
    }
}
