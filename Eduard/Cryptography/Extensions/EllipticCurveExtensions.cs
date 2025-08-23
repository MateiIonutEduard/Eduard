using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography.Extensions
{
    /// <summary>
    /// This class provides utilities to convert elliptic curves between families and map points to an isomorphic curve via isogenies.
    /// </summary>
    public static class EllipticCurveExtensions
    {
        /// <summary>
        /// Convert a valid Montgomery curve to its equivalent twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TwistedEdwardsCurve ToTwistedEdwardsCurve(this MontgomeryCurve curve)
        {
            if (curve.B == 0 || curve.A == 2 || curve.A == curve.field - 2  || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The Montgomery curve is invalid.");

            BigInteger order = curve.order;
            BigInteger cofactor = curve.cofactor;

            BigInteger field = curve.field;
            BigInteger B_inv = curve.B.Inverse(field);

            BigInteger a = ((curve.A + 2) * B_inv) % field;
            BigInteger d = ((curve.A - 2) * B_inv) % field;
            return new TwistedEdwardsCurve(a, d, field, order, cofactor);
        }

        /// <summary>
        /// Convert an affine point on a Montgomery curve to the equivalent affine point on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ECPoint ToTwistedEdwardsPoint(this MontgomeryCurve curve, ECPoint point)
        {
            if (curve.B == 0 || curve.A == 2 || curve.A == curve.field - 2 || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The Montgomery curve is invalid.");

            if (point.GetAffineX() == 0 && point.GetAffineY() == 0)
                throw new ArgumentException("This Montgomery curve point is exceptional and has no corresponding point on the equivalent twisted Edwards curve.");

            BigInteger p = curve.field;
            BigInteger B_root = curve.Sqrt(curve.B);

            BigInteger Xp = point.GetAffineX();
            BigInteger Yp = point.GetAffineY();

            BigInteger y_inv = Yp.Inverse(p);
            BigInteger x1_inv = ((Xp + 1) % p).Inverse(p);

            BigInteger X = (((B_root * Xp) % p) * y_inv) % p;
            BigInteger Y = ((p + Xp - 1) * x1_inv) % p;
            return new ECPoint(X, Y);
        }
    }
}
