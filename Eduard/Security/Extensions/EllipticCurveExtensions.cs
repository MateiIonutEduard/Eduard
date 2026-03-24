using Eduard.Security.Curves;
using Eduard.Security.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Security.Extensions
{
    /// <summary>
    /// This class provides utilities to convert elliptic curves between families and map points to an isomorphic curve via isogenies.
    /// </summary>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class EllipticCurveExtensions
    {
        /// <summary>
        /// Convert a Weierstrass curve into its equivalent Montgomery curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static MontgomeryCurve ToMontgomeryCurve(this EllipticCurve curve)
        {
            if ((curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The Weierstrass curve is invalid.");

            BigInteger order = curve.order;
            BigInteger cofactor = curve.cofactor;

            Polynomial.SetField(curve.field);
            BigInteger p = curve.field;

            var roots = new List<BigInteger>();
            Polynomial W = new Polynomial(1, 0, curve.a, curve.b);

            /* find the roots of the polynomial associated with the Weierstrass curve */
            W.FindRoots(ref roots);
            Polynomial P = 1;

            for (int i = 0; i < roots.Count; i++)
            {
                Polynomial Q = new Polynomial(1, p - roots[i]);
                P *= Q;
            }

            W /= P;
            BigInteger A1 = 0;
            bool found = false;

            Polynomial.Solve(W, ref roots);
            BigInteger s = 0;

            for (int i = 0; i < roots.Count; i++)
            {
                A1 = roots[i];
                BigInteger A2 = BarrettReducer.MulMod(A1, A1);
                BigInteger A3 = BarrettReducer.MulMod(3, A2);
                s = BarrettReducer.AddMod(A3, curve.a);

                /* find the root corresponding to the x-coordinate of the 4-torsion point */
                if (BigInteger.Jacobi(s, p) == 1)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                throw new ArgumentException("Weierstrass curve cannot be converted to Montgomery form.");

            BigInteger t = curve.Sqrt(s, true).Inverse(p);
            BigInteger A4 = BarrettReducer.MulMod(A1, t);

            BigInteger A = BarrettReducer.MulMod(3, A4);
            return new MontgomeryCurve(A, t, p, order, cofactor);
        }

        /// <summary>
        /// Convert an affine point on a Weierstrass curve to the corresponding affine point on the Montgomery curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ECPoint ToMontgomeryPoint(this EllipticCurve curve, ECPoint point)
        {
            if ((curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The Weierstrass curve is invalid.");

            Polynomial.SetField(curve.field);
            BigInteger p = curve.field;

            /* map the point at infinity on a Montgomery curve to its equivalent on the Weierstrass curve */
            if (point == ECPoint.POINT_INFINITY) return ECPoint.POINT_INFINITY;

            var roots = new List<BigInteger>();
            Polynomial W = new Polynomial(1, 0, curve.a, curve.b);

            /* find the roots of the polynomial associated with the Weierstrass curve */
            W.FindRoots(ref roots);
            Polynomial P = 1;

            for (int i = 0; i < roots.Count; i++)
            {
                Polynomial Q = new Polynomial(1, p - roots[i]);
                P *= Q;
            }

            W /= P;
            BigInteger A1 = 0;
            bool found = false;

            Polynomial.Solve(W, ref roots);
            BigInteger s = 0;

            for (int i = 0; i < roots.Count; i++)
            {
                A1 = roots[i];
                BigInteger A2 = BarrettReducer.MulMod(A1, A1);
                BigInteger A3 = BarrettReducer.MulMod(3, A2);
                s = BarrettReducer.AddMod(A3, curve.a);

                /* find the root corresponding to the x-coordinate of the 4-torsion point */
                if (BigInteger.Jacobi(s, p) == 1)
                {
                    found = true;
                    break;
                }
            }

            /* if no 4-torsion point is found (x-coordinate is a root of the 4-division polynomial), the Weierstrass curve is likely not properly parameterized */
            if (!found) throw new ArgumentException("Weierstrass curve cannot be converted to Montgomery form.");

            BigInteger ts = curve.Sqrt(s, true).Inverse(p);
            BigInteger Xp = point.GetAffineX();

            BigInteger Yp = point.GetAffineY();
            BigInteger A4 = BarrettReducer.SubMod(Xp, A1);
            BigInteger X = BarrettReducer.MulMod(ts, A4);

            BigInteger Y = BarrettReducer.MulMod(ts, Yp);
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert a Weierstrass curve to its equivalent twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TwistedEdwardsCurve ToTwistedEdwardsCurve(this EllipticCurve curve)
        {
            if ((curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The Weierstrass curve is invalid.");

            BigInteger order = curve.order;
            BigInteger cofactor = curve.cofactor;

            Polynomial.SetField(curve.field);
            BigInteger p = curve.field;

            var roots = new List<BigInteger>();
            Polynomial W = new Polynomial(1, 0, curve.a, curve.b);

            /* find the roots of the polynomial associated with the Weierstrass curve */
            W.FindRoots(ref roots);
            Polynomial P = 1;

            for (int i = 0; i < roots.Count; i++)
            {
                Polynomial Q = new Polynomial(1, p - roots[i]);
                P *= Q;
            }

            W /= P;
            BigInteger A1 = 0;
            bool found = false;

            Polynomial.Solve(W, ref roots);
            BigInteger s = 0;

            for (int i = 0; i < roots.Count; i++)
            {
                A1 = roots[i];
                BigInteger A2 = BarrettReducer.MulMod(A1, A1);
                BigInteger A3 = BarrettReducer.MulMod(3, A2);
                s = BarrettReducer.AddMod(A3, curve.a);

                /* find the root corresponding to the x-coordinate of the 4-torsion point */
                if (BigInteger.Jacobi(s, p) == 1)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                throw new ArgumentException("Weierstrass curve cannot be converted to twisted Edwards form.");

            BigInteger t = curve.Sqrt(s, true).Inverse(p);
            BigInteger A4 = BarrettReducer.MulMod(A1, t);

            BigInteger A = BarrettReducer.MulMod(3, A4);
            BigInteger B = t;

            BigInteger B_inv = B.Inverse(p);
            BigInteger A5 = BarrettReducer.AddMod(A, 2);

            BigInteger a = BarrettReducer.MulMod(A5, B_inv);
            BigInteger A6 = BarrettReducer.SubMod(A, 2);

            BigInteger d = BarrettReducer.MulMod(A6, B_inv);
            return new TwistedEdwardsCurve(a, d, p, order, cofactor);
        }

        /// <summary>
        /// Convert an affine point on a Weierstrass curve to the corresponding affine point on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ECPoint ToTwistedEdwardsPoint(this EllipticCurve curve, ECPoint point)
        {
            if ((curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The Weierstrass curve is invalid.");

            Polynomial.SetField(curve.field);
            BigInteger p = curve.field;

            /* map the point at infinity on a Montgomery curve to its equivalent on the Weierstrass curve */
            if (point == ECPoint.POINT_INFINITY) return ECPoint.POINT_INFINITY;

            var roots = new List<BigInteger>();
            Polynomial W = new Polynomial(1, 0, curve.a, curve.b);

            /* find the roots of the polynomial associated with the Weierstrass curve */
            W.FindRoots(ref roots);
            Polynomial P = 1;

            for (int i = 0; i < roots.Count; i++)
            {
                Polynomial Q = new Polynomial(1, p - roots[i]);
                P *= Q;
            }

            W /= P;
            BigInteger A1 = 0;
            bool found = false;

            Polynomial.Solve(W, ref roots);
            BigInteger s = 0;

            for (int i = 0; i < roots.Count; i++)
            {
                A1 = roots[i];
                BigInteger A2 = BarrettReducer.MulMod(A1, A1);
                BigInteger A3 = BarrettReducer.MulMod(3, A2);
                s = BarrettReducer.AddMod(A3, curve.a);

                /* find the root corresponding to the x-coordinate of the 4-torsion point */
                if (BigInteger.Jacobi(s, p) == 1)
                {
                    found = true;
                    break;
                }
            }

            /* if no 4-torsion point is found (x-coordinate is a root of the 4-division polynomial), the Weierstrass curve is likely not properly parameterized */
            if (!found) throw new ArgumentException("Weierstrass curve cannot be converted to twisted Edwards form.");

            BigInteger ts = curve.Sqrt(s, true).Inverse(p);
            BigInteger Xp = point.GetAffineX();

            BigInteger Yp = point.GetAffineY();
            BigInteger A4 = BarrettReducer.SubMod(Xp, A1);

            BigInteger Xm = BarrettReducer.MulMod(ts, A4);
            BigInteger Ym = BarrettReducer.MulMod(ts, Yp);

            BigInteger y_inv = Ym.Inverse(p);
            BigInteger A5 = BarrettReducer.AddMod(Xm, 1);

            BigInteger x1_inv = A5.Inverse(p);
            BigInteger X = BarrettReducer.MulMod(Xm, y_inv);

            BigInteger A6 = BarrettReducer.SubMod(Xm, 1);
            BigInteger Y = BarrettReducer.MulMod(A6, x1_inv);
            return new ECPoint(X, Y);
        }

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
            BigInteger A1 = BarrettReducer.MulMod(curve.A, curve.A);

            BigInteger A2 = BarrettReducer.MulMod(curve.A, A1);
            BigInteger A3 = BarrettReducer.SubMod(3, A1);

            BigInteger At = BarrettReducer.AddMod(A2, A2);
            BigInteger At2 = BarrettReducer.MulMod(9, curve.A);

            BigInteger A4 = BarrettReducer.SubMod(At, At2);
            BigInteger B1 = BarrettReducer.MulMod(curve.B, curve.B);

            BigInteger B2 = BarrettReducer.MulMod(B1, curve.B);
            BigInteger Bt = BarrettReducer.MulMod(27, B2);

            BigInteger B3 = Bt.Inverse(p);
            BigInteger B4 = BarrettReducer.MulMod(9, curve.B);

            BigInteger B5 = BarrettReducer.MulMod(A3, B3);
            BigInteger a = BarrettReducer.MulMod(B5, B4);

            BigInteger b = BarrettReducer.MulMod(A4, B3);
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
            BigInteger Bt = BarrettReducer.MulMod(3, curve.B);
            BigInteger B3_inv = Bt.Inverse(p);

            BigInteger Xp = point.GetAffineX();
            BigInteger Yp = point.GetAffineY();

            BigInteger AB3 = BarrettReducer.MulMod(curve.A, B3_inv);
            BigInteger B_inv = BarrettReducer.MulMod(3, B3_inv);

            /* map the rational 2-torsion point (0, 0) from a Montgomery curve to its equivalent Weierstrass curve */
            if (Xp == 0 && Yp == 0) return new ECPoint(AB3, 0);
            BigInteger Xt = BarrettReducer.MulMod(Xp, B_inv);
            BigInteger X = BarrettReducer.AddMod(Xt, AB3);

            BigInteger Y = BarrettReducer.MulMod(Yp, B_inv);
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
            BigInteger ad = BarrettReducer.SubMod(curve.a, curve.d);

            BigInteger ad_inv = ad.Inverse(p);
            BigInteger B = BarrettReducer.MulMod(4, ad_inv);

            BigInteger Bt = BarrettReducer.AddMod(curve.a, curve.d);
            BigInteger A = BarrettReducer.AddMod(Bt, Bt);

            A = BarrettReducer.MulMod(A, ad_inv);
            BigInteger A1 = BarrettReducer.MulMod(A, A);

            BigInteger A2 = BarrettReducer.MulMod(A, A1);
            BigInteger A3 = BarrettReducer.SubMod(3, A1);

            BigInteger A4t = BarrettReducer.AddMod(A2, A2);
            BigInteger A4t2 = BarrettReducer.MulMod(9, A);

            BigInteger A4 = BarrettReducer.SubMod(A4t, A4t2);
            BigInteger B1 = BarrettReducer.MulMod(B, B);

            BigInteger B2 = BarrettReducer.MulMod(B1, B);
            BigInteger B2t = BarrettReducer.MulMod(27, B2);

            BigInteger B3 = B2t.Inverse(p);
            BigInteger B4 = BarrettReducer.MulMod(9, B);
            BigInteger B4t = BarrettReducer.MulMod(A3, B3);

            BigInteger a = BarrettReducer.MulMod(B4t, B4);
            BigInteger b = BarrettReducer.MulMod(A4, B3);
            return new EllipticCurve(a, b, p, order, cofactor);
        }

        /// <summary>
        /// Convert an affine point on a twisted Edwards curve to its equivalent affine point on the Weierstrass curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static ECPoint ToWeierstrassPoint(this TwistedEdwardsCurve curve, ECPoint point)
        {
            if (curve.a == curve.d || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The twisted Edwards curve is invalid.");

            /* map the point at infinity on a twisted Edwards curve to its equivalent on the Weierstrass curve */
            if (point.GetAffineX() == 0 && point.GetAffineY() == 1) return ECPoint.POINT_INFINITY;

            if (point.GetAffineX() == 0 || point.GetAffineY() == 1)
                throw new ArgumentException("This twisted Edwards curve point is exceptional and has no equivalent on the Weierstrass curve.");

            BigInteger Xp = point.GetAffineX();
            BigInteger Yp = point.GetAffineY();

            BigInteger p = curve.field;
            BigInteger u = BarrettReducer.AddMod(Yp, 1);

            BigInteger A1 = BarrettReducer.SubMod(1, Yp);
            BigInteger v = BarrettReducer.MulMod(A1, Xp).Inverse(p);

            BigInteger A2 = BarrettReducer.MulMod(Xp, v);
            BigInteger Xm = BarrettReducer.MulMod(u, A2);

            BigInteger Ym = BarrettReducer.MulMod(u, v);
            BigInteger ad = BarrettReducer.SubMod(curve.a, curve.d);

            BigInteger ad_inv = ad.Inverse(p);
            BigInteger B = BarrettReducer.MulMod(4, ad_inv);

            BigInteger A3 = BarrettReducer.AddMod(curve.a, curve.d);
            BigInteger A = BarrettReducer.AddMod(A3, A3);

            A = BarrettReducer.MulMod(A, ad_inv);
            BigInteger B3_inv = ((3 * B) % p).Inverse(p);

            BigInteger AB3 = BarrettReducer.MulMod(A, B3_inv);
            BigInteger B_inv = BarrettReducer.MulMod(3, B3_inv);

            /* map the rational 2-torsion point (0, 0) from a Montgomery curve to its equivalent Weierstrass curve */
            if (Xm == 0 && Ym == 0) return new ECPoint(AB3, 0);
            BigInteger A4 = BarrettReducer.MulMod(Xm, B_inv);

            BigInteger X = BarrettReducer.AddMod(A4, AB3);
            BigInteger Y = BarrettReducer.MulMod(Ym, B_inv);
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert a Montgomery curve to the equivalent twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TwistedEdwardsCurve ToTwistedEdwardsCurve(this MontgomeryCurve curve)
        {
            if (curve.B == 0 || curve.A == 2 || curve.A == curve.field - 2 || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("The Montgomery curve is invalid.");

            BigInteger order = curve.order;
            BigInteger cofactor = curve.cofactor;

            BigInteger field = curve.field;
            BigInteger B_inv = curve.B.Inverse(field);

            BigInteger A1 = BarrettReducer.AddMod(curve.A, 2);
            BigInteger a = BarrettReducer.MulMod(A1, B_inv);
            BigInteger A2 = BarrettReducer.SubMod(curve.A, 2);

            BigInteger d = BarrettReducer.MulMod(A2, B_inv);
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
            BigInteger B1 = BarrettReducer.AddMod(Xp, 1);

            BigInteger x1_inv = B1.Inverse(p);
            BigInteger X = BarrettReducer.MulMod(Xp, y_inv);

            BigInteger B2 = BarrettReducer.SubMod(Xp, 1);
            BigInteger Y = BarrettReducer.MulMod(B2, x1_inv);
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
            BigInteger ad = BarrettReducer.SubMod(curve.a, curve.d);

            BigInteger ad_inv = ad.Inverse(field);
            BigInteger B = BarrettReducer.MulMod(4, ad_inv);

            BigInteger At = BarrettReducer.AddMod(curve.a, curve.d);
            BigInteger A = BarrettReducer.AddMod(At, At);

            A = BarrettReducer.MulMod(A, ad_inv);
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
            BigInteger u = BarrettReducer.AddMod(Yp, 1);

            BigInteger B1 = BarrettReducer.SubMod(1, Yp);
            BigInteger v = BarrettReducer.MulMod(B1, Xp).Inverse(p);

            BigInteger B2 = BarrettReducer.MulMod(Xp, v);
            BigInteger X = BarrettReducer.MulMod(u, B2);

            BigInteger Y = BarrettReducer.MulMod(u, v);
            return new ECPoint(X, Y);
        }
    }
}
