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
        /// Convert a Montgomery curve to the equivalent Weierstrass curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static EllipticCurve ToWeierstrassCurve(this MontgomeryCurve curve)
        {
            if (curve.B == 0 || curve.A == 2 || curve.A == curve.field - 2 || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The Montgomery curve is invalid.");

            BigInteger order = curve.order;
            BigInteger cofactor = curve.cofactor;

            BigInteger p = curve.field;
            BigInteger A1 = (curve.A * curve.A) % p;

            BigInteger A2 = (curve.A * A1) % p;
            BigInteger A3 = (p + 3 - A1) % p;

            BigInteger A4 = (p + ((2 * A2) % p) - ((9 * curve.A) % p)) % p;
            BigInteger B1 = (curve.B * curve.B) % p;

            BigInteger B2 = (B1 * curve.B) % p;
            BigInteger B3 = ((27 * B2) % p).Inverse(p);

            BigInteger B4 = (9 * curve.B) % p;
            BigInteger a = (((A3 * B3) % p) * B4) % p;

            BigInteger b = (A4 * B3) % p;
            return new EllipticCurve(a, b, p, order, cofactor);
        }

        /// <summary>
        /// Convert an affine point on a Montgomery curve to its equivalent affine point on the Weierstrass curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ECPoint ToWeierstrassPoint(this MontgomeryCurve curve, ECPoint point)
        {
            if (curve.B == 0 || curve.A == 2 || curve.A == curve.field - 2 || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The Montgomery curve is invalid.");

            /* map the point at infinity on a Montgomery curve to its equivalent on the Weierstrass curve */
            if (point == ECPoint.POINT_INFINITY) return ECPoint.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger B3_inv = ((3 * curve.B) % p).Inverse(p);

            BigInteger Xp = point.GetAffineX();
            BigInteger Yp = point.GetAffineY();

            BigInteger AB3 = (curve.A * B3_inv) % p;
            BigInteger B_inv = (3 * B3_inv) % p;

            /* map the rational 2-torsion point (0, 0) from a Montgomery curve to its equivalent Weierstrass curve */
            if (Xp == 0 && Yp == 0) return new ECPoint(AB3, 0);
            BigInteger X = (((Xp * B_inv) % p) + AB3) % p;
            
            BigInteger Y = (Yp * B_inv) % p;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert a twisted Edwards curve to the equivalent Weierstrass curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static EllipticCurve ToWeierstrassCurve(this TwistedEdwardsCurve curve)
        {
            if (curve.a == curve.d || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The twisted Edwards curve is invalid.");

            BigInteger order = curve.order;
            BigInteger cofactor = curve.cofactor;

            BigInteger p = curve.field;
            BigInteger ad = (p + curve.a - curve.d) % p;

            BigInteger ad_inv = ad.Inverse(p);
            BigInteger B = (4 * ad_inv) % p;

            BigInteger A = (2 * (curve.a + curve.d)) % p;
            A = (A * ad_inv) % p;

            BigInteger A1 = (A * A) % p;

            BigInteger A2 = (A * A1) % p;
            BigInteger A3 = (p + 3 - A1) % p;

            BigInteger A4 = (p + ((2 * A2) % p) - ((9 * A) % p)) % p;
            BigInteger B1 = (B * B) % p;

            BigInteger B2 = (B1 * B) % p;
            BigInteger B3 = ((27 * B2) % p).Inverse(p);

            BigInteger B4 = (9 * B) % p;
            BigInteger a = (((A3 * B3) % p) * B4) % p;

            BigInteger b = (A4 * B3) % p;
            return new EllipticCurve(a, b, p, order, cofactor);
        }

        /// <summary>
        /// Convert a Montgomery curve to the equivalent twisted Edwards curve.
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

            /* map the point at infinity on a Montgomery curve to its equivalent on the twisted Edwards curve */
            if (point == ECPoint.POINT_INFINITY) return ECPoint.POINT_INFINITY;

            if (point.GetAffineX() == 0 || point.GetAffineY() == curve.field - 1)
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

        /// <summary>
        /// Convert a twisted Edwards curve to the equivalent Montgomery curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static MontgomeryCurve ToMontgomeryCurve(this TwistedEdwardsCurve curve)
        {
            if (curve.a == curve.d || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The twisted Edwards curve is invalid.");

            BigInteger order = curve.order;
            BigInteger cofactor = curve.cofactor;

            BigInteger field = curve.field;
            BigInteger ad = (field + curve.a - curve.d) % field;

            BigInteger ad_inv = ad.Inverse(field);
            BigInteger B = (4 * ad_inv) % field;

            BigInteger A = (2 * (curve.a + curve.d)) % field;
            A = (A * ad_inv) % field;
            return new MontgomeryCurve(A, B, field, order, cofactor);
        }

        /// <summary>
        /// Convert an affine point on a twisted Edwards curve to the equivalent affine point on the Montgomery curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ECPoint ToMontgomeryPoint(this TwistedEdwardsCurve curve, ECPoint point)
        {
            if (curve.a == curve.d || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The twisted Edwards curve is invalid.");

            /* map the point at infinity on a twisted Edwards curve to its equivalent on the Montgomery curve */
            if (point.GetAffineX() == 0 && point.GetAffineY() == 1) return ECPoint.POINT_INFINITY;

            if (point.GetAffineX() == 0 || point.GetAffineY() == 1)
                throw new ArgumentException("This twisted Edwards curve point is exceptional and has no equivalent on the Montgomery curve.");

            BigInteger Xp = point.GetAffineX();
            BigInteger Yp = point.GetAffineY();

            BigInteger p = curve.field;
            BigInteger u = (Yp + 1) % p;

            BigInteger v = (((p + 1 - Yp) % p) * Xp).Inverse(p);
            BigInteger X = (u * (((Xp * v) % p) % p)) % p;

            BigInteger Y = (u * v) % p;
            return new ECPoint(X, Y);
        }
    }
}
