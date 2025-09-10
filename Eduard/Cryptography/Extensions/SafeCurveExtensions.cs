using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography.Extensions
{
    internal static class SafeCurveExtensions
    {
        internal static bool ValidatePoint(this EllipticCurve curve, ECPoint point)
        {
            var Y2 = curve.Evaluate(point.GetAffineX());
            int jSymbol = BigInteger.Jacobi(Y2, curve.field);

            /* check if y-coordinate is defined */
            if (jSymbol != 1 && Y2 > 0)
                return false;
            else
            {
                BigInteger y = point.GetAffineY();
                BigInteger Yp2 = (y * y) % curve.field;

                /* point not on this Weierstrass curve; likely on the twist */
                if (Yp2 != Y2) return false;
            }

            ECPoint result = ECPoint.POINT_INFINITY;
            int t = curve.cofactor.GetBits();
            BigInteger k = curve.cofactor;

            /* check if the point generates a small-order subgroup */
            JacobianPoint auxPoint = JacobianPoint.POINT_INFINITY;
            var basePoint = curve.ToModifiedJacobian(point);

            for (int j = 0; j < t; j++)
            {
                if (k.TestBit(j))
                    auxPoint = JacobianMath.Add(curve, auxPoint, curve.ToJacobian(basePoint));

                basePoint = ModifiedJacobianMath.Doubling(curve, basePoint);
            }

            result = curve.ToAffine(auxPoint);
            return (result != ECPoint.POINT_INFINITY);
        }

        internal static bool ValidatePoint(this TwistedEdwardsCurve curve, ECPoint point)
        {
            var X2 = curve.Evaluate(point.GetAffineY());
            int jSymbol = BigInteger.Jacobi(X2, curve.field);

            /* check if x-coordinate is well defined */
            if (jSymbol != 1 && X2 > 0)
                return false;
            else
            {
                BigInteger x = point.GetAffineX();
                BigInteger Xp2 = (x * x) % curve.field;

                /* the affine point does not lie on the twisted Edwards curve */
                if (Xp2 != X2) return false;
            }

            ECPoint result = ECPoint.POINT_INFINITY;
            int t = curve.cofactor.GetBits();
            BigInteger k = curve.cofactor;

            /* check if the point generates a small-order subgroup */
            ProjectivePoint auxPoint = ProjectivePoint.POINT_INFINITY;
            var basePoint = curve.ToProjective(point);

            for (int j = 0; j < t; j++)
            {
                if (k.TestBit(j))
                    auxPoint = TwistedEdwardsProjectiveMath.UnifiedAdd(curve, auxPoint, basePoint);

                basePoint = TwistedEdwardsProjectiveMath.UnifiedDoubling(curve, basePoint);
            }

            result = curve.ToAffine(auxPoint);
            return (result != ECPoint.POINT_INFINITY);
        }
    }
}
