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
            ECPoint result = ECPoint.POINT_INFINITY;
            int t = curve.cofactor.GetBits();
            BigInteger k = curve.cofactor;

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
    }
}
