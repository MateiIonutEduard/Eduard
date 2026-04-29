using System;
using System.Diagnostics;

namespace Eduard.Security.Primitives
{
    /// <summary>
    /// Represents a point on a Weierstrass elliptic curve in Jacobian-Chudnovsky coordinates (X, Y, Z, Z^2, Z^3).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Jacobian-Chudnovsky coordinates extend standard Jacobian representation by pre-computing <br/>
    /// the powers Z^2 and Z^3. This representation optimizes mixed coordinate operations by <br/>
    /// eliminating redundant squaring and cubing operations during point addition and doubling.
    /// </para>
    /// <para>
    /// The affine point (x, y) is represented as:
    /// <list type="bullet">
    /// <item><description>x = X / Z^2</description></item>
    /// <item><description>y = Y / Z^3</description></item>
    /// <item><description>Z^2 = Z^2 (pre-computed)</description></item>
    /// <item><description>Z^3 = Z^3 (pre-computed)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The point at infinity is represented with Z = 0, Z^2 = 0, Z^3 = 0. This representation <br/>
    /// is particularly efficient in algorithms like P-256 and P-384 where repeated <br/>
    /// point operations benefit from cached coordinate powers.
    /// </para>
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public struct ECPoint5w : IEquatable<ECPoint5w>
    {
        /// <summary>
        /// The X-coordinate in Jacobian-Chudnovsky representation.
        /// </summary>
        public BigInteger x;

        /// <summary>
        /// The Y-coordinate in Jacobian-Chudnovsky representation.
        /// </summary>
        public BigInteger y;

        /// <summary>
        /// The Z-coordinate in Jacobian-Chudnovsky representation.
        /// </summary>
        /// <remarks>
        /// Z = 0 indicates the point at infinity. For finite points, Z is non-zero.
        /// </remarks>
        public BigInteger z;

        /// <summary>
        /// The pre-computed value Z^2.
        /// </summary>
        /// <remarks>
        /// This cached value eliminates redundant squaring operations.<br/>
        /// Must satisfy z2 = z * z (mod p) for finite points.
        /// </remarks>
        public BigInteger z2;

        /// <summary>
        /// The pre-computed value Z^3.
        /// </summary>
        /// <remarks>
        /// This cached value eliminates redundant cubing operations.<br/>
        /// Must satisfy z3 = z * z * z (mod p) for finite points.
        /// </remarks>
        public BigInteger z3;

        /// <summary>
        /// Initializes a new Jacobian-Chudnovsky point with the specified coordinates.
        /// </summary>
        /// <param name="x">The projective X-coordinate.</param>
        /// <param name="y">The projective Y-coordinate.</param>
        /// <param name="z">The projective Z-coordinate (zero indicates point at infinity).</param>
        /// <param name="z2">The pre-computed value Z^2 (must be zero for point at infinity).</param>
        /// <param name="z3">The pre-computed value Z^3 (must be zero for point at infinity).</param>
        /// <exception cref="ArgumentNullException">Thrown when any coordinate is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when point at infinity has non-zero Z^2 or Z^3, or when consistency checks fail.
        /// </exception>
        public ECPoint5w(BigInteger x, BigInteger y, BigInteger z, BigInteger z2, BigInteger z3)
        {
            if (ReferenceEquals(x, null))
                throw new ArgumentNullException(nameof(x),
                    "The projective X-coordinate cannot be null.");

            if (ReferenceEquals(y, null))
                throw new ArgumentNullException(nameof(y),
                    "The projective Y-coordinate cannot be null.");

            if (ReferenceEquals(z, null))
                throw new ArgumentNullException(nameof(z),
                    "The projective Z-coordinate cannot be null.");

            if (ReferenceEquals(z2, null))
                throw new ArgumentNullException(nameof(z2),
                    "The projective Z^2-coordinate cannot be null.");

            if (ReferenceEquals(z3, null))
                throw new ArgumentNullException(nameof(z3),
                    "The projective Z^3-coordinate cannot be null.");

            /* validate point at infinity invariant */
            if (z == 0 && (z2 != 0 || z3 != 0))
                throw new InvalidOperationException(
                    "Point at infinity must have Z^2 = 0 and Z^3 = 0.");

            this.x = x;
            this.y = y;
            this.z = z;

            this.z2 = z2;
            this.z3 = z3;
        }

        /// <summary>
        /// Gets the point at infinity for Weierstrass curves in Jacobian-Chudnovsky coordinates.
        /// </summary>
        /// <remarks>
        /// The point at infinity serves as the identity element in the elliptic curve group.<br/>
        /// It is uniquely represented with Z = 0, Z^2 = 0, Z^3 = 0. Coordinates are set to <br/>
        /// (1, 1, 0, 0, 0) for consistency, though X and Y values are irrelevant when Z = 0.
        /// </remarks>
        public static ECPoint5w POINT_INFINITY
        {
            get
            {
                var infinity = new ECPoint5w(1, 1, 0, 0, 0);
                return infinity;
            }
        }

        /// <summary>
        /// Indicates whether the current point is equal to another Jacobian-Chudnovsky point.
        /// </summary>
        /// <param name="other">The point to compare with this point.</param>
        /// <returns>true if the points represent the same geometric point; otherwise false.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if either point violates the invariant (Z=0 but Z^2 != 0 or Z^3 != 0).
        /// </exception>
        /// <remarks>
        /// Two points are considered equal if:
        /// <list type="bullet">
        /// <item><description>Both are the point at infinity (Z = 0, Z^2 = 0, Z^3 = 0)</description></item>
        /// <item><description>Both are finite points with identical (X, Y, Z, Z^2, Z^3) representations</description></item>
        /// </list>
        /// </remarks>
        public bool Equals(ECPoint5w other)
        {
            bool isInfinitySelf = z == 0;
            bool isInfinityOther = other.z == 0;

            /* validate invariants */
            if (isInfinitySelf && (z2 != 0 || z3 != 0))
                throw new InvalidOperationException(
                    "Current point at infinity has non-zero Z^2 or Z^3.");

            if (isInfinityOther && (other.z2 != 0 || other.z3 != 0))
                throw new InvalidOperationException(
                    "Other point at infinity has non-zero Z^2 or Z^3.");

            /* different infinity status */
            if (isInfinitySelf != isInfinityOther)
                return false;

            /* both at infinity: equal regardless of X,Y */
            if (isInfinitySelf && isInfinityOther)
                return true;

            /* compare all coordinates */
            return x == other.x && y == other.y &&
                   z == other.z && z2 == other.z2 
                   && z3 == other.z3;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current Jacobian-Chudnovsky point.
        /// </summary>
        /// <param name="obj">The object to compare with the current point.</param>
        /// <returns>true if the object is an ECPoint5w with identical coordinates; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (!(obj is ECPoint5w))
                return false;

            return Equals((ECPoint5w)obj);
        }

        /// <summary>
        /// Equality operator for Jacobian-Chudnovsky projective points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns>true if the points represent the same geometric point; otherwise false.</returns>
        public static bool operator ==(ECPoint5w left, ECPoint5w right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for Jacobian-Chudnovsky projective points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns>true if the points represent different geometric points; otherwise false.</returns>
        public static bool operator !=(ECPoint5w left, ECPoint5w right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code for this Jacobian-Chudnovsky point.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>
        /// The hash code is derived from the X, Y, Z, Z^2, and Z^3 coordinates to ensure uniqueness.<br/>
        /// Points at infinity (Z=0, Z^2=0, Z^3=0) all hash to the same value regardless of X and Y.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                /* point at infinity: constant hash regardless of X,Y */
                if (z == 0 && z2 == 0 && z3 == 0)
                    return 0;

                /* for normal points, combine all coordinates */
                int hash = 17;

                hash = hash * 31 + x.GetHashCode();
                hash = hash * 31 + y.GetHashCode();
                hash = hash * 31 + z.GetHashCode();

                hash = hash * 31 + z2.GetHashCode();
                hash = hash * 31 + z3.GetHashCode();
                return hash;
            }
        }
    }
}
