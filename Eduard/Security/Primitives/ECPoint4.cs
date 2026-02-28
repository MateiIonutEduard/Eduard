using System;
using Eduard;

namespace Eduard.Security.Primitives
{
    /// <summary>
    /// Represents an extended projective point (X, Y, T, Z) corresponding to the affine point (X/Z, Y/Z, T/Z) on the elliptic curve.
    /// </summary>
    public class ECPoint4
    {
        public BigInteger x;
        public BigInteger y;

        public BigInteger t;
        public BigInteger z;

        /// <summary>
        /// Creates a <seealso cref="ECPoint4"/> representing the point at infinity.
        /// </summary>
        public ECPoint4()
        {
            this.x = null; this.y = null;
            this.t = null; this.z = null;
        }

        /// <summary>
        /// Creates a <seealso cref="ECPoint4"/> from the specified projective coordinates X, Y, T, and Z.
        /// </summary>
        /// <param name="x">The projective X-coordinate.</param>
        /// <param name="y">The projective Y-coordinate.</param>
        /// <param name="t">The projective T-coordinate.</param>
        /// <param name="z">The projective Z-coordinate.</param>
        /// <exception cref="NullReferenceException"></exception>
        public ECPoint4(BigInteger x, BigInteger y, BigInteger t, BigInteger z)
        {
            if (object.ReferenceEquals(x, null))
                throw new NullReferenceException("The projective X-coordinate cannot be null.");

            if (object.ReferenceEquals(null, y))
                throw new NullReferenceException("The projective Y-coordinate cannot be null.");

            if (object.ReferenceEquals(t, null))
                throw new NullReferenceException("The projective T-coordinate cannot be null.");

            if (object.ReferenceEquals(z, null))
                throw new NullReferenceException("The projective Z-coordinate cannot be null.");

            this.x = x; this.y = y;
            this.t = t; this.z = z;
        }

        /// <summary>
        /// Represents the elliptic curve point at infinity.
        /// </summary>
        public static ECPoint4 POINT_INFINITY
        {
            get { return new ECPoint4(); }
        }

        /// <summary>
        /// Determines whether the projective coordinates of two <seealso cref="ECPoint4"/> objects are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ECPoint4 left, ECPoint4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether the projective coordinates of two <seealso cref="ECPoint4"/> objects differ.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ECPoint4 left, ECPoint4 right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the hash code of this extended projective elliptic curve point.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ((object)this).GetHashCode();
        }

        /// <summary>
        /// Compares an extended projective point with a specified object for equality.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            try
            {
                ECPoint4 other = (ECPoint4)obj;

                if (object.ReferenceEquals(x, other.x) && object.ReferenceEquals(y, other.y) && object.ReferenceEquals(z, other.z))
                    return true;

                if (x == other.x && y == other.y && z == other.z)
                    return true;

                return false;
            }
            catch (Exception)
            { return false; }
        }
    }
}
