using System;

namespace Eduard.Security
{
    /// <summary>
    /// Represents a point on an elliptic curve in affine coordinates.
    /// </summary>
    public class ECPoint
    {
        internal BigInteger x;
        internal BigInteger y;

        /// <summary>
        /// Creates an <seealso cref="ECPoint"/> equals with point at infinity.
        /// </summary>
        public ECPoint()
        {
            this.x = null;
            this.y = null;
        }

        /// <summary>
        /// Creates an <seealso cref="ECPoint"/> from the specified affine x-coordinate and affine y-coordinate.
        /// </summary>
        /// <param name="x">The affine x-coordinate.</param>
        /// <param name="y">The affine y-coordinate.</param>
        /// <exception cref="NullReferenceException"></exception>
        public ECPoint(BigInteger x, BigInteger y)
        {
            if (object.ReferenceEquals(x, null))
                throw new NullReferenceException("The affine x-coordinate cannot be null.");

            if (object.ReferenceEquals(null, y))
                throw new NullReferenceException("The affine y-coordinate cannot be null.");

            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// This defines the point at infinity.
        /// </summary>
        public static ECPoint POINT_INFINITY
        {
            get
            {
                return new ECPoint();
            }
        }

        /// <summary>
        /// Returns the affine x-coordinate.
        /// </summary>
        /// <returns></returns>
        public BigInteger GetAffineX()
        {
            return x;
        }

        /// <summary>
        /// Returns the affine y-coordinate.
        /// </summary>
        /// <returns></returns>
        public BigInteger GetAffineY()
        {
            return y;
        }

        /// <summary>
        /// Compares this elliptic curve point for equality with the specified object.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            try
            {
                ECPoint other = (ECPoint)obj;

                if (object.ReferenceEquals(x, other.x) && object.ReferenceEquals(y, other.y))
                    return true;

                if (x == other.x && y == other.y)
                    return true;

                return false;
            }
            catch (Exception)
            { return false; }
        }

        /// <summary>
        /// Returns a value that indicates if the affine coordinates of two <seealso cref="ECPoint"/> objects are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ECPoint left, ECPoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates if the affine coordinates of two <seealso cref="ECPoint"/> objects have different values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ECPoint left, ECPoint right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code value for this elliptic curve point.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ((object)this).GetHashCode();
        }
    }
}
