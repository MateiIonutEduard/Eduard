#pragma warning disable

namespace Eduard.Security
{
    /// <summary>
    /// Represents a projective Jacobian point (x, y, z) that maps to the affine elliptic curve point (x/z^2, y/z^3).
    /// </summary>
    public class JacobianPoint
    {
        public BigInteger x;
        public BigInteger y;
        public BigInteger z;

        /// <summary>
        /// Creates a <seealso cref="JacobianPoint"/> equal to the point at infinity.
        /// </summary>
        public JacobianPoint()
        {
            this.x = null;
            this.y = null;
            this.z = null;
        }

        /// <summary>
        /// Creates a <seealso cref="JacobianPoint"/> from the specified projective x-coordinate, y-coordinate, and z-coordinate.
        /// </summary>
        /// <param name="x">The projective x-coordinate.</param>
        /// <param name="y">The projective y-coordinate.</param>
        /// <param name="z">The projective z-coordinate.</param>
        /// <exception cref="NullReferenceException"></exception>
        public JacobianPoint(BigInteger x, BigInteger y, BigInteger z)
        {
            if (object.ReferenceEquals(x, null))
                throw new NullReferenceException("The affine x-coordinate cannot be null.");

            if (object.ReferenceEquals(null, y))
                throw new NullReferenceException("The affine y-coordinate cannot be null.");

            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Represents the point at infinity on the Weierstrass curve.
        /// </summary>
        public static JacobianPoint POINT_INFINITY
        {
            get { return new JacobianPoint(); }
        }

        /// <summary>
        /// Compares this elliptic curve projective point to the specified object for equality.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            try
            {
                JacobianPoint other = (JacobianPoint)obj;

                if (object.ReferenceEquals(x, other.x) && object.ReferenceEquals(y, other.y) && object.ReferenceEquals(z, other.z))
                    return true;

                if (x == other.x && y == other.y && z == other.z)
                    return true;

                return false;
            }
            catch (Exception)
            { return false; }
        }

        /// <summary>
        /// Returns a value that indicates whether the Jacobian projective coordinates of two <seealso cref="JacobianPoint"/> objects are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(JacobianPoint left, JacobianPoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether the Jacobian projective coordinates of two <seealso cref="JacobianPoint"/> objects have different values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(JacobianPoint left, JacobianPoint right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code value for this elliptic curve Jacobian point.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ((object)this).GetHashCode();
        }
    }
}
