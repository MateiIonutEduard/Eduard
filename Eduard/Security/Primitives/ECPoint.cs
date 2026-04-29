using System;
using System.Diagnostics;

namespace Eduard.Security.Primitives
{
    /// <summary>
    /// Represents a point on an elliptic curve in affine coordinates (x, y).
    /// </summary>
    /// <remarks>
    /// This struct encapsulates an affine point on an elliptic curve over a prime field. <br/>
    /// Points can be either ordinary curve points satisfying the curve equation, or the <br/>
    /// point at infinity which serves as the identity element in the elliptic curve group. <br/>
    /// The point at infinity is represented with coordinates (0,1) and the isInfinity flag set.
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public struct ECPoint : IEquatable<ECPoint>
    {
        internal BigInteger x;
        internal BigInteger y;
        internal bool isInfinity;

        /// <summary>
        /// Initializes a new elliptic curve point with the specified affine coordinates.
        /// </summary>
        /// <param name="x">The affine x-coordinate (must satisfy the curve equation).</param>
        /// <param name="y">The affine y-coordinate (must satisfy the curve equation).</param>
        /// <exception cref="ArgumentNullException">Thrown when x or y is null.</exception>
        public ECPoint(BigInteger x, BigInteger y) : this(x, y, false)
        { }

        /// <summary>
        /// Initializes a new elliptic curve point with the specified affine coordinates and infinity flag.
        /// </summary>
        /// <param name="x">The affine x-coordinate.</param>
        /// <param name="y">The affine y-coordinate.</param>
        /// <param name="isInfinity">Indicates whether this point represents the point at infinity.</param>
        /// <exception cref="ArgumentNullException">Thrown when x or y is null.</exception>
        /// <remarks>
        /// For the point at infinity, the coordinates are conventionally set to (0,1) with the <br/>
        /// infinity flag set to true. The curve equation is not enforced for the point at infinity.
        /// </remarks>
        public ECPoint(BigInteger x, BigInteger y, bool isInfinity)
        {
            if (ReferenceEquals(x, null))
                throw new ArgumentNullException(nameof(x), 
                    "The affine x-coordinate cannot be null.");

            if (ReferenceEquals(null, y))
                throw new ArgumentNullException(nameof(y), 
                    "The affine y-coordinate cannot be null.");

            this.isInfinity = isInfinity;
            this.x = x; this.y = y;
        }

        /// <summary>
        /// Gets the point at infinity (additive identity) for elliptic curve groups.
        /// </summary>
        /// <remarks>
        /// The point at infinity is represented with coordinates (0,1) and the infinity flag set to true.
        /// </remarks>
        public static ECPoint POINT_INFINITY
        {
            get
            {
                var infinity = new ECPoint(0, 1, true);
                return infinity;
            }
        }

        /// <summary>
        /// Gets the affine x-coordinate of this point.
        /// </summary>
        /// <returns>The x-coordinate as a BigInteger.</returns>
        public BigInteger GetAffineX()
        {
            return x;
        }

        /// <summary>
        /// Gets the affine y-coordinate of this point.
        /// </summary>
        /// <returns>The y-coordinate as a BigInteger.</returns>
        public BigInteger GetAffineY()
        {
            return y;
        }

        /// <summary>
        /// Indicates whether the current point is equal to another point.
        /// </summary>
        /// <param name="other">The point to compare with this point.</param>
        /// <returns>true if the points have identical affine coordinates; otherwise false.</returns>
        /// <remarks>
        /// Two points are considered equal if they have the same x and y <br/> coordinates.
        /// The point at infinity (0,1,true) is only equal to itself.
        /// </remarks>
        public bool Equals(ECPoint other)
        {
            if (isInfinity != other.isInfinity)
                return false;

            bool sameXCoord = x == other.x;
            bool sameYCoord = y == other.y;
            return sameXCoord && sameYCoord;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current point.
        /// </summary>
        /// <param name="obj">The object to compare with the current point.</param>
        /// <returns>true if the object is an ECPoint with identical coordinates; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (!(obj is ECPoint))
                return false;

            ECPoint other = (ECPoint)obj;
            return Equals(other);
        }

        /// <summary>
        /// Equality operator for elliptic curve points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns>true if the points have identical coordinates; otherwise false.</returns>
        public static bool operator ==(ECPoint left, ECPoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for elliptic curve points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns>true if the points have different coordinates; otherwise false.</returns>
        public static bool operator !=(ECPoint left, ECPoint right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code for this elliptic curve point.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>
        /// The hash code is computed by XORing the hash codes of the x and y coordinates. <br/>
        /// This ensures that points with identical coordinates produce the same hash code, <br/>
        /// maintaining consistency with the equality semantics of the struct.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                int xHash = x.GetHashCode();
                int yHash = y.GetHashCode();
                return xHash ^ yHash;
            }
        }
    }
}
