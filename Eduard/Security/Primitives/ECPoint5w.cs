using System;
using Eduard;

namespace Eduard.Security.Primitives
{
    /// <summary>
    /// Represents a Jacobian-Chudnovsky projective point (X, Y, Z, Z^2, Z^3) that maps to the affine elliptic curve point (X/Z^2, Y/Z^3).
    /// </summary>
    public class ECPoint5w
    {
        public BigInteger x;
        public BigInteger y;
        public BigInteger z;

        public BigInteger z2;
        public BigInteger z3;

        /// <summary>
        /// Creates a <seealso cref="ECPoint5w"/> equal to the point at infinity.
        /// </summary>
        public ECPoint5w()
        {
            this.x = null;
            this.y = null;
            this.z = null;

            this.z2 = null;
            this.z3 = null;
        }

        /// <summary>
        /// Creates a <seealso cref="ECPoint5w"/> from the extended Jacobian projective coordinates.
        /// </summary>
        /// <param name="x">The projective X-coordinate.</param>
        /// <param name="y">The projective Y-coordinate.</param>
        /// <param name="z">The projective Z-coordinate.</param>
        /// <param name="z2">The projective Z^2-coordinate.</param>
        /// <param name="z3">The projective Z^3-coordinate.</param>
        /// <exception cref="NullReferenceException"></exception>
        public ECPoint5w(BigInteger x, BigInteger y, BigInteger z, BigInteger z2, BigInteger z3)
        {
            if (object.ReferenceEquals(x, null))
                throw new NullReferenceException("The projective X-coordinate cannot be null.");

            if (object.ReferenceEquals(null, y))
                throw new NullReferenceException("The projective Y-coordinate cannot be null.");

            if (object.ReferenceEquals(z, null))
                throw new NullReferenceException("The projective Z-coordinate cannot be null.");

            if (object.ReferenceEquals(z2, null))
                throw new NullReferenceException("The projective Z^2-coordinate cannot be null.");

            if (object.ReferenceEquals(z3, null))
                throw new NullReferenceException("The projective Z^3-coordinate cannot be null.");

            this.x = x;
            this.y = y;
            this.z = z;

            this.z2 = z2;
            this.z3 = z3;
        }

        /// <summary>
        /// Represents the point at infinity on the Weierstrass curve.
        /// </summary>
        public static ECPoint5w POINT_INFINITY
        {
            get { return new ECPoint5w(); }
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
                ECPoint5w other = (ECPoint5w)obj;

                if (object.ReferenceEquals(x, other.x) && object.ReferenceEquals(y, other.y) 
                    && object.ReferenceEquals(z, other.z) && object.ReferenceEquals(z2, other.z2)
                    && object.ReferenceEquals(z3, other.z3))
                    return true;

                if (x == other.x && y == other.y && z == other.z && z2 == other.z2 && z3 == other.z3)
                    return true;

                return false;
            }
            catch (Exception)
            { return false; }
        }

        /// <summary>
        /// Returns a value that indicates whether the Jacobian-Chudnovsky projective coordinates of two <seealso cref="ECPoint5w"/> objects are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ECPoint5w left, ECPoint5w right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether the Jacobian-Chudnovsky projective coordinates of two <seealso cref="ECPoint5w"/> objects have different values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ECPoint5w left, ECPoint5w right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code value for this elliptic curve Jacobian-Chudnovsky point.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ((object)this).GetHashCode();
        }
    }
}
