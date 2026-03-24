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
    /// Provides isogeny-based curve transformations and point mappings between Weierstrass, <br/>
    /// Montgomery, and twisted Edwards curve representations.
    /// </summary>
    /// <remarks>
    /// This utility class implements the standard isomorphism mappings between elliptic curve <br/>
    /// forms commonly used in cryptographic implementations. All operations leverage <br/>
    /// <see cref="BarrettReducer"/> for constant-time modular arithmetic.
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class EllipticCurveExtensions
    {
        /// <summary>
        /// Transforms a Weierstrass curve to an isomorphic Montgomery curve representation.
        /// </summary>
        /// <param name="curve">The source Weierstrass curve in short form y^2 = x^3 + a*x + b over Fp.</param>
        /// <returns>A Montgomery curve in the form B*y^2 = x^3 + A*x^2 + x that is isomorphic to the source curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The curve's cofactor is not divisible by 4
        /// - No suitable 4-torsion point exists for the transformation (curve may not be properly parameterized)
        /// </exception>
        /// <remarks>
        /// The transformation follows the standard algorithm for converting Weierstrass curves to Montgomery form, <br/>
        /// requiring the existence of a point of order 4. The resulting Montgomery curve parameters (A, B) are <br/>
        /// derived from the 4-torsion point's x-coordinate and a quadratic residue condition.
        /// </remarks>
        public static MontgomeryCurve ToMontgomeryCurve(this EllipticCurve curve)
        {
            if ((curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Cofactor must be multiple of 4.");

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
                throw new ArgumentException("No suitable 4-torsion point found.");

            BigInteger t = curve.Sqrt(s, true).Inverse(p);
            BigInteger A4 = BarrettReducer.MulMod(A1, t);

            BigInteger A = BarrettReducer.MulMod(3, A4);
            return new MontgomeryCurve(A, t, p, order, cofactor);
        }

        /// <summary>
        /// Maps an affine point from a Weierstrass curve to its isomorphic image on the corresponding Montgomery curve.
        /// </summary>
        /// <param name="curve">Source Weierstrass curve containing the point.</param>
        /// <param name="point">Affine point on the Weierstrass curve to be mapped.</param>
        /// <returns>The corresponding affine point on the isomorphic Montgomery curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The curve's cofactor is not divisible by 4
        /// - No suitable 4-torsion point exists for the transformation
        /// </exception>
        /// <remarks>
        /// The mapping preserves the group law and is invertible. The point at infinity maps to itself. <br/>
        /// The transformation is derived from the same 4-torsion point used in the curve conversion, <br/>
        /// ensuring consistency between curve and point mappings.
        /// </remarks>
        public static ECPoint ToMontgomeryPoint(this EllipticCurve curve, ECPoint point)
        {
            if ((curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Cofactor must be multiple of 4.");

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
            if (!found) throw new ArgumentException("No suitable 4-torsion point found.");

            BigInteger ts = curve.Sqrt(s, true).Inverse(p);
            BigInteger Xp = point.GetAffineX();

            BigInteger Yp = point.GetAffineY();
            BigInteger A4 = BarrettReducer.SubMod(Xp, A1);
            BigInteger X = BarrettReducer.MulMod(ts, A4);

            BigInteger Y = BarrettReducer.MulMod(ts, Yp);
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Transforms a Weierstrass curve to an isomorphic twisted Edwards curve representation.
        /// </summary>
        /// <param name="curve">The source Weierstrass curve in short form y^2 = x^3 + a*x + b over Fp.</param>
        /// <returns>A twisted Edwards curve in the form a*x^2 + y^2 = 1 + d*x^2*y^2 isomorphic to the source curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The curve's cofactor is not divisible by 4
        /// - No suitable 4-torsion point exists for the transformation
        /// </exception>
        /// <remarks>
        /// This transformation composes the Weierstrass, Montgomery, twisted Edwards conversions, <br/>
        /// producing optimal parameters for Ed25519-style implementations. The resulting twisted Edwards <br/>
        /// curve will have a = (A+2)/B and d = (A-2)/B where (A,B) are the intermediate Montgomery parameters.
        /// </remarks>
        public static TwistedEdwardsCurve ToTwistedEdwardsCurve(this EllipticCurve curve)
        {
            if ((curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Cofactor must be multiple of 4.");

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
                throw new ArgumentException("No suitable 4-torsion point found.");

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
        /// Maps an affine point from a Weierstrass curve to its isomorphic image on the corresponding twisted Edwards curve.
        /// </summary>
        /// <param name="curve">Source Weierstrass curve containing the point.</param>
        /// <param name="point">Affine point on the Weierstrass curve to be mapped.</param>
        /// <returns>The corresponding affine point on the isomorphic twisted Edwards curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The curve's cofactor is not divisible by 4
        /// - No suitable 4-torsion point exists for the transformation
        /// - The point maps to an exceptional case (e.g., points requiring division by zero)
        /// </exception>
        /// <remarks>
        /// The mapping is obtained by composing the Weierstrass, Montgomery and twisted Edwards point mappings. <br/>
        /// Points with x = 0 or y = 1 on the intermediate Montgomery curve represent exceptional cases that <br/>
        /// cannot be mapped to twisted Edwards form and will trigger an exception.
        /// </remarks>
        public static ECPoint ToTwistedEdwardsPoint(this EllipticCurve curve, ECPoint point)
        {
            if ((curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Cofactor must be multiple of 4.");

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
            if (!found) throw new ArgumentException("No suitable 4-torsion point found.");

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
        /// Transforms a Montgomery curve to an isomorphic Weierstrass curve representation.
        /// </summary>
        /// <param name="curve">The source Montgomery curve in the form B*y^2 = x^3 + A*x^2 + x over Fp.</param>
        /// <returns>A Weierstrass curve in short form y^2 = x^3 + a*x + b isomorphic to the source curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The Montgomery curve parameters are invalid.
        /// - The curve's cofactor is not divisible by 4.
        /// </exception>
        /// <remarks>
        /// The transformation is derived from the standard Weierstrass form conversion: <br/>
        /// a = (3 - A^2) / (3B^2) and b = (2A^3 - 9A) / (27B^3). The resulting parameters are <br/>
        /// normalized to ensure proper isomorphism.
        /// </remarks>
        public static EllipticCurve ToWeierstrassCurve(this MontgomeryCurve curve)
        {
            if (curve.B == 0 || curve.A == 2 || curve.A == curve.field - 2 || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Invalid Montgomery curve parameters.");

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
        /// Maps an affine point from a Montgomery curve to its isomorphic image on the corresponding Weierstrass curve.
        /// </summary>
        /// <param name="curve">Source Montgomery curve containing the point.</param>
        /// <param name="point">Affine point on the Montgomery curve to be mapped.</param>
        /// <returns>The corresponding affine point on the isomorphic Weierstrass curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The Montgomery curve parameters are invalid.
        /// - The curve's cofactor is not divisible by 4.
        /// </exception>
        /// <remarks>
        /// The mapping is an isomorphism that preserves the group law.
        /// The point at infinity maps to <br/> itself, and the rational 2-torsion point (0,0) 
        /// maps to (A/3B, 0) on the Weierstrass curve.
        /// </remarks>
        public static ECPoint ToWeierstrassPoint(this MontgomeryCurve curve, ECPoint point)
        {
            if (curve.B == 0 || curve.A == 2 || curve.A == curve.field - 2 || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Invalid Montgomery curve parameters.");

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
        /// Transforms a twisted Edwards curve to an isomorphic Weierstrass curve representation.
        /// </summary>
        /// <param name="curve">The source twisted Edwards curve in the form a*x^2 + y^2 = 1 + d*x^2*y^2 over Fp.</param>
        /// <returns>A Weierstrass curve in short form y^2 = x^3 + a*x + b isomorphic to the source curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The twisted Edwards curve parameters are invalid (a = d, causing singularities).
        /// - The curve's cofactor is not divisible by 4.
        /// </exception>
        /// <remarks>
        /// This transformation composes the twisted Edwards, Montgomery, Weierstrass conversions, <br/>
        /// producing standard Weierstrass parameters suitable for generic elliptic curve operations. <br/>
        /// The intermediate Montgomery parameters are derived as A = 2(a+d)/(a-d) and B = 4/(a-d).
        /// </remarks>
        public static EllipticCurve ToWeierstrassCurve(this TwistedEdwardsCurve curve)
        {
            if (curve.a == curve.d || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Invalid twisted Edwards curve parameters.");

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
        /// Maps an affine point from a twisted Edwards curve to its isomorphic image on the corresponding Weierstrass curve.
        /// </summary>
        /// <param name="curve">Source twisted Edwards curve containing the point.</param>
        /// <param name="point">Affine point on the twisted Edwards curve to be mapped.</param>
        /// <returns>The corresponding affine point on the isomorphic Weierstrass curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The twisted Edwards curve parameters are invalid (a = d).
        /// - The point maps to an exceptional case requiring division by zero.
        /// </exception>
        /// <remarks>
        /// The mapping is obtained by composing the twisted Edwards, Montgomery, Weierstrass point mappings. <br/>
        /// Points with x = 0 or y = 1 on the twisted Edwards curve represent exceptional cases that cannot be <br/>
        /// mapped and will trigger an exception. The point at infinity maps to itself.
        /// </remarks>
        public static ECPoint ToWeierstrassPoint(this TwistedEdwardsCurve curve, ECPoint point)
        {
            if (curve.a == curve.d || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Invalid twisted Edwards curve parameters.");

            /* map the point at infinity on a twisted Edwards curve to its equivalent on the Weierstrass curve */
            if (point.GetAffineX() == 0 && point.GetAffineY() == 1) return ECPoint.POINT_INFINITY;

            if (point.GetAffineX() == 0 || point.GetAffineY() == 1)
                throw new ArgumentException("Exceptional point has no Weierstrass equivalent.");

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
        /// Transforms a Montgomery curve to an isomorphic twisted Edwards curve representation.
        /// </summary>
        /// <param name="curve">The source Montgomery curve in the form B*y^2 = x^3 + A*x^2 + x over Fp.</param>
        /// <returns>A twisted Edwards curve in the form a*x^2 + y^2 = 1 + d*x^2*y^2 isomorphic to the source curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The Montgomery curve parameters are invalid.
        /// - The curve's cofactor is not divisible by 4.
        /// </exception>
        /// <remarks>
        /// The transformation uses the standard conversion:
        /// a = (A + 2)/B and d = (A - 2)/B. <br/>This mapping is particularly useful for
        /// converting Montgomery curves like Curve25519<br/> to twisted Edwards form (Ed25519).
        /// </remarks>
        public static TwistedEdwardsCurve ToTwistedEdwardsCurve(this MontgomeryCurve curve)
        {
            if (curve.B == 0 || curve.A == 2 || curve.A == curve.field - 2 || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Invalid Montgomery curve parameters.");

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
        /// Maps an affine point from a Montgomery curve to its isomorphic image on the corresponding twisted Edwards curve.
        /// </summary>
        /// <param name="curve">Source Montgomery curve containing the point.</param>
        /// <param name="point">Affine point on the Montgomery curve to be mapped.</param>
        /// <returns>The corresponding affine point on the isomorphic twisted Edwards curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The Montgomery curve parameters are invalid.
        /// - The point maps to an exceptional case requiring division by zero.
        /// </exception>
        /// <remarks>
        /// The mapping follows the standard Montgomery to twisted Edwards conversion. <br/>
        /// The point at infinity maps to itself, and points with x = 0 or y = -1 represent <br/>
        /// exceptional cases that cannot be mapped.
        /// </remarks>
        public static ECPoint ToTwistedEdwardsPoint(this MontgomeryCurve curve, ECPoint point)
        {
            if (curve.B == 0 || curve.A == 2 || curve.A == curve.field - 2 || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Invalid Montgomery curve parameters.");

            /* map the point at infinity on a Montgomery curve to its equivalent on the twisted Edwards curve */
            if (point == ECPoint.POINT_INFINITY) return ECPoint.POINT_INFINITY;

            if (point.GetAffineX() == 0 || point.GetAffineY() == curve.field - 1)
                throw new ArgumentException("Exceptional point has no twisted Edwards equivalent.");

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
        /// Transforms a twisted Edwards curve to an isomorphic Montgomery curve representation.
        /// </summary>
        /// <param name="curve">The source twisted Edwards curve in the form a*x^2 + y^2 = 1 + d*x^2*y^2 over Fp.</param>
        /// <returns>A Montgomery curve in the form B*y^2 = x^3 + A*x^2 + x isomorphic to the source curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The twisted Edwards curve parameters are invalid (a = d, causing singularities).
        /// - The curve's cofactor is not divisible by 4.
        /// </exception>
        /// <remarks>
        /// The transformation follows the standard conversion:
        /// A = 2(a+d)/(a-d) and B = 4/(a-d).<br/> This is the inverse of the Montgomery to twisted Edwards conversion
        /// and is particularly <br/>useful for implementing X25519-style operations.
        /// </remarks>
        public static MontgomeryCurve ToMontgomeryCurve(this TwistedEdwardsCurve curve)
        {
            if (curve.a == curve.d || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Invalid twisted Edwards curve parameters.");

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
        /// Maps an affine point from a twisted Edwards curve to its isomorphic image on the corresponding Montgomery curve.
        /// </summary>
        /// <param name="curve">Source twisted Edwards curve containing the point.</param>
        /// <param name="point">Affine point on the twisted Edwards curve to be mapped.</param>
        /// <returns>The corresponding affine point on the isomorphic Montgomery curve.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when:
        /// - The twisted Edwards curve parameters are invalid (a = d).
        /// - The point maps to an exceptional case requiring division by zero.
        /// </exception>
        /// <remarks>
        /// The mapping follows the standard twisted Edwards to Montgomery conversion. <br/>
        /// The point at infinity maps to itself, and points with x = 0 or y = 1 represent exceptional <br/>
        /// cases that cannot be mapped and will trigger an exception.
        /// </remarks>
        public static ECPoint ToMontgomeryPoint(this TwistedEdwardsCurve curve, ECPoint point)
        {
            if (curve.a == curve.d || (curve.cofactor & 0x3) != 0)
                throw new ArgumentException("Invalid twisted Edwards curve parameters.");

            /* map the point at infinity on a twisted Edwards curve to its equivalent on the Montgomery curve */
            if (point.GetAffineX() == 0 && point.GetAffineY() == 1) return ECPoint.POINT_INFINITY;

            if (point.GetAffineX() == 0 || point.GetAffineY() == 1)
                throw new ArgumentException("Exceptional point has no Montgomery equivalent.");

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
