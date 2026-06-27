using System;

namespace Eduard.Security.Primitives
{
    /// <summary>
    /// Represents a point on a twisted Edwards curve in extended projective coordinates (X, Y, T, Z).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Extended projective coordinates (X, Y, T, Z) provide an efficient representation for point operations <br/>
    /// on twisted Edwards curves, where T = (X*Y)/Z. This representation eliminates modular inversions <br/>
    /// and reduces field multiplications in
    /// point addition from 11 to 9, as described by Hisil et al. (2008).
    /// </para>
    /// <para>
    /// The affine point (x, y) is recovered as x = X/Z, y = Y/Z when Z != 0, with T = (X*Y)/Z
    /// satisfying T = x * y. <br/>The point at infinity is represented with Z = 0, which also forces T = 0.
    /// </para>
    /// </remarks>
    public struct ECPoint4 : IEquatable<ECPoint4>
    {
        /// <summary>
        /// The projective X-coordinate.
        /// </summary>
        public BigInteger X
        {
            get { return isOnCurve ? x : 0; }
        }

        /// <summary>
        /// The projective Y-coordinate.
        /// </summary>
        public BigInteger Y
        {
            get { return isOnCurve ? y : 1; }
        }

        /// <summary>
        /// The projective T-coordinate, where T = (X*Y)/Z.
        /// </summary>
        /// <remarks>
        /// This coordinate caches the product (X*Y)/Z to optimize point <br/>
        /// addition. For the point at infinity, T must be 0 when Z = 0.
        /// </remarks>
        public BigInteger T
        {
            get { return isOnCurve ? t : 0; }
        }

        /// <summary>
        /// The projective Z-coordinate.
        /// </summary>
        /// <remarks>
        /// Z = 0 indicates the point at infinity. <br/>
        /// For finite points, Z is non-zero.
        /// </remarks>
        public BigInteger Z
        {
            get { return isOnCurve ? z : 0; }
        }

        internal BigInteger x, y, t, z;
        internal bool isOnCurve;

        /// <summary>
        /// Initializes a new extended projective point with the specified coordinates.
        /// </summary>
        /// <param name="x">The projective X-coordinate.</param>
        /// <param name="y">The projective Y-coordinate.</param>
        /// <param name="t">The projective T-coordinate (must be 0 for point at infinity).</param>
        /// <param name="z">The projective Z-coordinate (zero indicates point at infinity).</param>
        /// <exception cref="ArgumentNullException">Thrown when any coordinate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when point at infinity has non-zero T (Z=0 but T != 0).
        /// </exception>
        public ECPoint4(BigInteger x, BigInteger y, BigInteger t, BigInteger z)
        {
            if (ReferenceEquals(x, null))
                throw new ArgumentNullException(nameof(x),
                    "The projective X-coordinate cannot be null.");

            if (ReferenceEquals(null, y))
                throw new ArgumentNullException(nameof(y),
                    "The projective Y-coordinate cannot be null.");

            if (ReferenceEquals(t, null))
                throw new ArgumentNullException(nameof(t),
                    "The projective T-coordinate cannot be null.");

            if (ReferenceEquals(z, null))
                throw new ArgumentNullException(nameof(z),
                    "The projective Z-coordinate cannot be null.");

            /* validate point at infinity invariant */
            if (z == 0 && t != 0)
                throw new InvalidOperationException(
                    "Point at infinity must have T = 0 when Z = 0.");

            this.x = x; this.y = y;
            this.t = t; this.z = z;
            isOnCurve = z != 0;
        }

        /// <summary>
        /// Gets whether this point is the point at infinity.
        /// </summary>
        /// <returns><c>true</c> if the point is at infinity (Z = 0 and T = 0); otherwise <c>false</c>.</returns>
        /// <remarks>
        /// The point at infinity serves as the identity element in the elliptic curve group.
        /// </remarks>
        public bool IsInfinity
        {
            get { return !isOnCurve; }
        }

        /// <summary>
        /// Gets the point at infinity for twisted Edwards curves in extended projective coordinates.
        /// </summary>
        /// <remarks>
        /// The point at infinity serves as the identity element in the elliptic curve group. <br/>
        /// It is represented with Z = 0 and T = 0. Coordinates are set to (0, 1, 0, 0) for <br/>
        /// consistency, though X and Y values are irrelevant when Z = 0.
        /// </remarks>
        public static ECPoint4 POINT_INFINITY
        {
            get 
            {
                var infinity = new ECPoint4(0, 1, 0, 0);
                return infinity;
            }
        }

        /// <summary>
        /// Equality operator for extended projective points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns><c>true</c> if the points represent the same geometric point; otherwise <c>false</c>.</returns>
        public static bool operator ==(ECPoint4 left, ECPoint4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for extended projective points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns><c>true</c> if the points represent different geometric points; otherwise <c>false</c>.</returns>
        public static bool operator !=(ECPoint4 left, ECPoint4 right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code for this extended projective point.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>
        /// The hash code is derived from the X, Y, and Z coordinates to ensure uniqueness. <br/>
        /// The T coordinate is derived from X, Y, Z and therefore not included separately. <br/>
        /// Points at infinity (Z = 0) all hash to the same value regardless of X and Y.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                /* all points at infinity share the same hash */
                if (!isOnCurve) return 0;
                int hash = 17;

                /* combine all coordinates */
                hash = hash * 31 + x.GetHashCode();
                hash = hash * 31 + y.GetHashCode();

                hash = hash * 31 + t.GetHashCode();
                hash = hash * 31 + z.GetHashCode();

                hash ^= hash >> 16;
                hash *= (int)0x85EBCA6B;

                hash ^= hash >> 13;
                hash *= (int)0xC2B2AE35;

                hash ^= hash >> 16;
                return hash;
            }

        }

        /// <summary>
        /// Indicates whether the current point is equal to another extended projective point.
        /// </summary>
        /// <param name="other">The point to compare with this point.</param>
        /// <returns><c>true</c> if the points represent the same geometric point; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Two points are considered equal if:
        /// <list type="bullet">
        /// <item><description>Both are the point at infinity (Z = 0, T = 0)</description></item>
        /// <item><description>Both are finite points with identical (X, Y, Z) representations</description></item>
        /// </list>
        /// The T coordinate must be consistent with X, Y, Z and <br/>
        /// is not compared directly as it is derived from them.
        /// </remarks>
        public bool Equals(ECPoint4 other)
        {
            bool isInfinitySelf = !isOnCurve;
            bool isInfinityOther = !other.isOnCurve;

            if (isInfinitySelf != isInfinityOther)
                return false;

            if (isInfinitySelf && isInfinityOther)
                return true;

            return x == other.x && 
                y == other.y && 
                t == other.t &&
                z == other.z;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current extended projective point.
        /// </summary>
        /// <param name="obj">The object to compare with the current point.</param>
        /// <returns><c>true</c> if the object is an ECPoint4 with identical coordinates; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ECPoint4))
                return false;

            ECPoint4 other = (ECPoint4)obj;
            return Equals(other);
        }
    }
}
