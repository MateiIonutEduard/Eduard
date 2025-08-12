namespace Eduard.Cryptography
{
    /// <summary>
    /// Represents a Jacobian-Chudnovsky projective point (x, y, z, z^2, z^3) that maps to the affine elliptic curve point (x/z^2, y/z^3).
    /// </summary>
    public class JacobianChudnovskyPoint
    {
        public BigInteger x;
        public BigInteger y;
        public BigInteger z;

        public BigInteger z2;
        public BigInteger z3;

        /// <summary>
        /// Creates a <seealso cref="JacobianChudnovskyPoint"/> equal to the point at infinity.
        /// </summary>
        public JacobianChudnovskyPoint()
        {
            this.x = null;
            this.y = null;
            this.z = null;

            this.z2 = null;
            this.z3 = null;
        }

        /// <summary>
        /// Creates a <seealso cref="JacobianChudnovskyPoint"/> from the extended Jacobian projective coordinates.
        /// </summary>
        /// <param name="x">The projective x-coordinate.</param>
        /// <param name="y">The projective y-coordinate.</param>
        /// <param name="z">The projective z-coordinate.</param>
        /// <exception cref="NullReferenceException"></exception>
        public JacobianChudnovskyPoint(BigInteger x, BigInteger y, BigInteger z, BigInteger z2, BigInteger z3)
        {
            if (object.ReferenceEquals(x, null))
                throw new NullReferenceException("The affine x-coordinate cannot be null.");

            if (object.ReferenceEquals(null, y))
                throw new NullReferenceException("The affine y-coordinate cannot be null.");

            this.x = x;
            this.y = y;
            this.z = z;

            this.z2 = z2;
            this.z3 = z3;
        }

        /// <summary>
        /// Represents the point at infinity on the Weierstrass curve.
        /// </summary>
        public static JacobianChudnovskyPoint POINT_INFINITY
        {
            get { return new JacobianChudnovskyPoint(); }
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
                JacobianChudnovskyPoint other = (JacobianChudnovskyPoint)obj;

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
        /// Returns a value that indicates whether the Jacobian-Chudnovsky projective coordinates of two <seealso cref="JacobianPoint"/> objects are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(JacobianChudnovskyPoint left, JacobianChudnovskyPoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether the Jacobian-Chudnovsky projective coordinates of two <seealso cref="JacobianPoint"/> objects have different values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(JacobianChudnovskyPoint left, JacobianChudnovskyPoint right)
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
