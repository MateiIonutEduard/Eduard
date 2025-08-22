namespace Eduard.Cryptography
{
    /// <summary>
    /// Represents an extended Jacobian projective point (X, Y, Z, aZ^4) corresponding to the affine elliptic curve point (X/Z^2, Y/Z^3).
    /// </summary>
    public class ModifiedJacobianPoint
    {
        public BigInteger x;
        public BigInteger y;
        public BigInteger z;
        public BigInteger aZ4;

        /// <summary>
        /// Creates a <seealso cref="ModifiedJacobianPoint"/> equal to the point at infinity.
        /// </summary>
        public ModifiedJacobianPoint()
        {
            this.x = null;
            this.y = null;

            this.z = null;
            this.aZ4 = null;
        }

        /// <summary>
        /// Creates a <seealso cref="ModifiedJacobianPoint"/> from the modified projective coordinates.
        /// </summary>
        /// <param name="x">The projective X-coordinate.</param>
        /// <param name="y">The projective Y-coordinate.</param>
        /// <param name="z">The projective Z-coordinate.</param>
        /// <param name="aZ4">The projective aZ^4-coordinate.</param>
        /// <exception cref="NullReferenceException"></exception>
        public ModifiedJacobianPoint(BigInteger x, BigInteger y, BigInteger z, BigInteger aZ4)
        {
            if (object.ReferenceEquals(x, null))
                throw new NullReferenceException("The projective X-coordinate cannot be null.");

            if (object.ReferenceEquals(y, null))
                throw new NullReferenceException("The projective Y-coordinate cannot be null.");

            if (object.ReferenceEquals(z, null))
                throw new NullReferenceException("The projective Z-coordinate cannot be null.");

            if (object.ReferenceEquals(aZ4, null))
                throw new NullReferenceException("The projective aZ^4-coordinate cannot be null.");

            this.x = x;
            this.y = y;

            this.z = z;
            this.aZ4 = aZ4;
        }

        /// <summary>
        /// Represents the point at infinity on the Weierstrass curve.
        /// </summary>
        public static ModifiedJacobianPoint POINT_INFINITY
        {
            get { return new ModifiedJacobianPoint(); }
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
                ModifiedJacobianPoint other = (ModifiedJacobianPoint)obj;

                if (object.ReferenceEquals(x, other.x) && object.ReferenceEquals(y, other.y) 
                    && object.ReferenceEquals(z, other.z) && object.ReferenceEquals(aZ4, other.aZ4))
                    return true;

                if (x == other.x && y == other.y && z == other.z && aZ4 == other.aZ4)
                    return true;

                return false;
            }
            catch (Exception)
            { return false; }
        }

        /// <summary>
        /// Returns a value that indicates whether the Jacobian projective coordinates of two <seealso cref="ModifiedJacobianPoint"/> objects are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ModifiedJacobianPoint left, ModifiedJacobianPoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether the Jacobian projective coordinates of two <seealso cref="ModifiedJacobianPoint"/> objects have different values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ModifiedJacobianPoint left, ModifiedJacobianPoint right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code value for this elliptic curve modified Jacobian point.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ((object)this).GetHashCode();
        }
    }
}
