using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography
{
    /// <summary>
    /// Represents a projective point (X, Y, Z) corresponding to the affine point (X/Z, Y/Z) on the elliptic curve.
    /// </summary>
    public class ProjectivePoint
    {
        public BigInteger x;
        public BigInteger y;
        public BigInteger z;

        /// <summary>
        /// Creates a <seealso cref="ProjectivePoint"/> representing the point at infinity.
        /// </summary>
        public ProjectivePoint()
        {
            this.x = null;
            this.y = null;
            this.z = null;
        }

        /// <summary>
        /// Creates a <seealso cref="ProjectivePoint"/> from the given X, Y, and Z projective coordinates.
        /// </summary>
        /// <param name="x">The projective X-coordinate.</param>
        /// <param name="y">The projective Y-coordinate.</param>
        /// <param name="z">The projective Z-coordinate.</param>
        /// <exception cref="NullReferenceException"></exception>
        public ProjectivePoint(BigInteger x, BigInteger y, BigInteger z)
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
        /// Represents the elliptic curve point at infinity.
        /// </summary>
        public static ProjectivePoint POINT_INFINITY
        {
            get { return new ProjectivePoint(); }
        }

        /// <summary>
        /// Determines whether the projective coordinates of two <seealso cref="ProjectivePoint"/> objects are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(ProjectivePoint left, ProjectivePoint right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether the projective coordinates of two <seealso cref="ProjectivePoint"/> objects differ.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(ProjectivePoint left, ProjectivePoint right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the hash code for this projective elliptic curve point.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ((object)this).GetHashCode();
        }

        /// <summary>
        /// Compares a projective point on the elliptic curve with the specified object for equality.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            try
            {
                ProjectivePoint other = (ProjectivePoint)obj;

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
