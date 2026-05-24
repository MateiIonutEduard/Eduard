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
    /// The point at infinity is represented with isOnCurve set to false; its coordinates <br/>
    /// are normalized to (0,1) via the accessor methods.
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public struct ECPoint : IEquatable<ECPoint>
    {
        internal BigInteger x;
        internal BigInteger y;
        internal bool isOnCurve;

        /// <summary>
        /// Initializes a new elliptic curve point with the specified affine coordinates.
        /// </summary>
        /// <param name="x">The affine x-coordinate (must satisfy the curve equation).</param>
        /// <param name="y">The affine y-coordinate (must satisfy the curve equation).</param>
        /// <exception cref="ArgumentNullException">Thrown when x or y is null.</exception>
        public ECPoint(BigInteger x, BigInteger y) : this(x, y, true)
        { }

        /// <summary>
        /// Initializes a new elliptic curve point with the specified affine coordinates and curve membership flag.
        /// </summary>
        /// <param name="x">The affine x-coordinate.</param>
        /// <param name="y">The affine y-coordinate.</param>
        /// <param name="isOnCurve">Whether this point lies on the curve. Set to false for the point at infinity.</param>
        /// <exception cref="ArgumentNullException">Thrown when x or y is null.</exception>
        /// <remarks>
        /// For the point at infinity, use <see cref="POINT_INFINITY"/> or pass isOnCurve = false. <br/>
        /// The curve equation is not enforced for points with isOnCurve set to false.
        /// </remarks>
        public ECPoint(BigInteger x, BigInteger y, bool isOnCurve)
        {
            if (ReferenceEquals(x, null))
                throw new ArgumentNullException(nameof(x), 
                    "The affine x-coordinate cannot be null.");

            if (ReferenceEquals(null, y))
                throw new ArgumentNullException(nameof(y), 
                    "The affine y-coordinate cannot be null.");

            this.isOnCurve = isOnCurve;
            this.x = x; this.y = y;
        }

        /// <summary>
        /// Gets whether this point is the point at infinity.
        /// </summary>
        /// <returns><c>true</c> if the point is at infinity; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// The point at infinity serves as the identity element in the elliptic curve group.
        /// </remarks>
        public bool IsInfinity
        {
            get { return !isOnCurve; }
        }

        /// <summary>
        /// Gets the point at infinity (additive identity) for elliptic curve groups.
        /// </summary>
        /// <remarks>
        /// Represented with isOnCurve = false, normalized to (0,1) by the accessor methods.
        /// </remarks>
        public static ECPoint POINT_INFINITY
        {
            get
            {
                var infinity = new ECPoint(0, 1, false);
                return infinity;
            }
        }

        /// <summary>
        /// Gets the affine x-coordinate of this point.
        /// </summary>
        /// <returns>The x-coordinate as a BigInteger, or 0 for the point at infinity.</returns>
        public BigInteger GetAffineX()
        {
            return isOnCurve ? x : 0;
        }

        /// <summary>
        /// Gets the affine y-coordinate of this point.
        /// </summary>
        /// <returns>The y-coordinate as a BigInteger, or 1 for the point at infinity.</returns>
        public BigInteger GetAffineY()
        {
            return isOnCurve ? y : 1;
        }

        /// <summary>
        /// Indicates whether the current point is equal to another point.
        /// </summary>
        /// <param name="other">The point to compare with this point.</param>
        /// <returns><c>true</c> if the points have identical normalized affine coordinates; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Equality is determined on normalized coordinates. The point at infinity is only equal to itself.
        /// </remarks>
        public bool Equals(ECPoint other)
        {
            bool isInfinitySelf = !isOnCurve;
            bool isInfinityOther = !other.isOnCurve;

            if (isInfinitySelf != isInfinityOther)
                return false;

            if (isInfinitySelf && isInfinityOther)
                return true;

            bool sameXCoord = x == other.x;
            bool sameYCoord = y == other.y;
            return sameXCoord && sameYCoord;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current point.
        /// </summary>
        /// <param name="obj">The object to compare with the current point.</param>
        /// <returns><c>true</c> if the object is an ECPoint with identical normalized coordinates; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
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
        /// <returns><c>true</c> if the points have identical coordinates; otherwise <c>false</c>.</returns>
        public static bool operator ==(ECPoint left, ECPoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for elliptic curve points.
        /// </summary>
        /// <param name="left">The first point to compare.</param>
        /// <param name="right">The second point to compare.</param>
        /// <returns><c>true</c> if the points have different coordinates; otherwise <c>false</c>.</returns>
        public static bool operator !=(ECPoint left, ECPoint right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code for this elliptic curve point.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>
        /// The point at infinity returns a hash code of 0. For affine points, the hash code <br/>
        /// is computed by XORing the hash codes of the x and y coordinates. This ensures that <br/>
        /// points with identical coordinates produce the same hash code, maintaining consistency <br/>
        /// with the equality semantics of the struct.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                if (!isOnCurve) return 0;
                int xHash = x.GetHashCode();
                int yHash = y.GetHashCode();
                return xHash ^ yHash;
            }
        }
    }
}
