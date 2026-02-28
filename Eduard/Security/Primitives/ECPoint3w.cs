using System;
using Eduard;

namespace Eduard.Security.Primitives
{
    /// <summary>
    /// Represents a projective Jacobian point (X, Y, Z) that maps to the affine elliptic curve point (X/Z^2, Y/Z^3).
    /// </summary>
    public class ECPoint3w
    {
        public BigInteger x;
        public BigInteger y;
        public BigInteger z;

        /// <summary>
        /// Creates a <seealso cref="ECPoint3w"/> equal to the point at infinity.
        /// </summary>
        public ECPoint3w()
        {
            this.x = null;
            this.y = null;
            this.z = null;
        }

        /// <summary>
        /// Creates a <seealso cref="ECPoint3w"/> from the specified projective X-coordinate, Y-coordinate, and Z-coordinate.
        /// </summary>
        /// <param name="x">The projective X-coordinate.</param>
        /// <param name="y">The projective Y-coordinate.</param>
        /// <param name="z">The projective Z-coordinate.</param>
        /// <exception cref="NullReferenceException"></exception>
        public ECPoint3w(BigInteger x, BigInteger y, BigInteger z)
        {
            if (object.ReferenceEquals(x, null))
                throw new NullReferenceException("The projective X-coordinate cannot be null.");

            if (object.ReferenceEquals(null, y))
                throw new NullReferenceException("The projective Y-coordinate cannot be null.");

            if (object.ReferenceEquals(z, null))
                throw new NullReferenceException("The projective Z-coordinate cannot be null.");

            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Represents the point at infinity on the Weierstrass curve.
        /// </summary>
        public static ECPoint3w POINT_INFINITY
        {
            get { return new ECPoint3w(); }
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
                ECPoint3w other = (ECPoint3w)obj;

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
        /// Returns a value that indicates whether the Jacobian projective coordinates of two <seealso cref="ECPoint3w"/> objects are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ECPoint3w left, ECPoint3w right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether the Jacobian projective coordinates of two <seealso cref="ECPoint3w"/> objects have different values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ECPoint3w left, ECPoint3w right)
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
