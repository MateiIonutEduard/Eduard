using Eduard.Security.Curves;
using Eduard.Security.Primitives;

namespace Eduard.Security.Extensions
{
    /// <summary>
    /// Provides point validation for Weierstrass and twisted Edwards curves, preventing 
    /// invalid curve, twist, and small-subgroup attacks.
    /// </summary>
    internal static class SafeCurveExtensions
    {
        /// <summary>
        /// Validates a point on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The Weierstrass curve.</param>
        /// <param name="point">The affine point to validate.</param>
        /// <returns>
        /// A <see cref="PointCheck"/> value: <see cref="PointCheck.EC_VALID"/> if the point lies on the curve <br/>
        /// and is not in a small subgroup; otherwise, a specific failure classification.
        /// </returns>
        /// <remarks>
        /// Performs three critical checks:
        /// <list type="bullet">
        /// <item><description>Curve equation compliance: y^2 = x^3 + a*x + b (mod p)</description></item>
        /// <item><description>Twist resistance: rejects points on the quadratic twist</description></item>
        /// <item><description>Small-subgroup elimination: verifies cofactor multiplication</description></item>
        /// </list>
        /// Uses modified Jacobian coordinates for constant-time cofactor multiplication.
        /// </remarks>
        internal static PointCheck ValidatePoint(this EllipticCurve curve, ECPoint point)
        {
            var Y2 = curve.Evaluate(point.GetAffineX());
            int jSymbol = BigInteger.Jacobi(Y2, curve.field);

            /* check if y-coordinate is defined */
            if (jSymbol != 1 && Y2 > 0)
                return PointCheck.EC_INVALID;
            else
            {
                BigInteger y = point.GetAffineY();
                BigInteger Yp2 = BarrettReducer.MultMod(y, y);

                /* point not on this Weierstrass curve; likely on the twist */
                if (Yp2 != Y2) return PointCheck.EC_TWIST;
            }

            ECPoint result = ECPoint.POINT_INFINITY;
            int t = curve.cofactor.GetBits();
            BigInteger k = curve.cofactor;

            /* check if the point generates a small-order subgroup */
            ECPoint3w auxPoint = ECPoint3w.POINT_INFINITY;
            var basePoint = curve.ToModifiedJacobian(point);

            for (int j = 0; j < t; j++)
            {
                if (k.TestBit(j))
                    auxPoint = Wei3Math.Add(curve, auxPoint, 
                        curve.ToJacobian(basePoint));

                basePoint = Wei4Math.Doubling(curve, basePoint);
            }

            result = curve.ToAffine(auxPoint);

            return (result != ECPoint.POINT_INFINITY) 
                ? PointCheck.EC_VALID : 
                PointCheck.EC_SMALL_SUBGROUP;
        }

        /// <summary>
        /// Validates a point on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve.</param>
        /// <param name="point">The affine point to validate.</param>
        /// <returns><c>true</c> if the point lies on the curve and is not in a small subgroup; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Performs three critical checks:
        /// <list type="bullet">
        /// <item><description>Curve equation compliance: a*x^2 + y^2 = 1 + d*x^2*y^2 (mod p)</description></item>
        /// <item><description>Twist resistance: rejects points on the quadratic twist</description></item>
        /// <item><description>Small-subgroup elimination: verifies cofactor multiplication</description></item>
        /// </list>
        /// Uses projective coordinates with unified addition formulas for constant-time <br/>
        /// operation. Essential for Ed25519/Ed448 public key validation.
        /// </remarks>
        internal static PointCheck ValidatePoint(this TwistedEdwardsCurve curve, ECPoint point)
        {
            var X2 = curve.Evaluate(point.GetAffineY());
            int jSymbol = BigInteger.Jacobi(X2, curve.field);

            /* check if x-coordinate is well defined */
            if (jSymbol != 1 && X2 > 0)
                return PointCheck.EC_INVALID;
            else
            {
                BigInteger x = point.GetAffineX();
                BigInteger Xp2 = BarrettReducer.MultMod(x, x);

                /* the affine point does not lie on the twisted Edwards curve */
                if (Xp2 != X2) return PointCheck.EC_TWIST;
            }

            ECPoint result = ECPoint.POINT_INFINITY;
            int t = curve.cofactor.GetBits();
            BigInteger k = curve.cofactor;

            /* check if the point generates a small-order subgroup */
            ECPoint3 auxPoint = ECPoint3.POINT_INFINITY;
            var basePoint = curve.ToProjective(point);

            for (int j = 0; j < t; j++)
            {
                if (k.TestBit(j))
                    auxPoint = Ed3Math.UnifiedAdd(curve, auxPoint, basePoint);

                basePoint = Ed3Math.UnifiedDoubling(curve, basePoint);
            }

            result = curve.ToAffine(auxPoint);

            return (result != ECPoint.POINT_INFINITY) 
                ? PointCheck.EC_VALID : 
                PointCheck.EC_SMALL_SUBGROUP;
        }
    }
}
