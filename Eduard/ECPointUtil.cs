using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eduard.Security;

namespace Eduard
{
    /// <summary>
    /// This class is a utility for converting projective points to affine form and vice versa.
    /// </summary>
    public static class ECPointUtil
    {
        public static ECPoint ToAffine(this EllipticCurve curve, JacobianPoint jacobianPoint)
        {
            if (jacobianPoint == JacobianPoint.POINT_INFINITY) return ECPoint.POINT_INFINITY;
            BigInteger p = curve.field;
            BigInteger Z2 = (jacobianPoint.z * jacobianPoint.z) % p;

            BigInteger Z3 = (Z2 * jacobianPoint.z) % p;
            BigInteger X = (jacobianPoint.x * Z2.Inverse(p)) % p;

            BigInteger Y = (jacobianPoint.y * Z3.Inverse(p)) % p;
            return new ECPoint(X, Y);
        }

        public static JacobianPoint ToJacobian(this EllipticCurve curve, ECPoint affinePoint)
        {
            JacobianPoint jacobianPoint = new JacobianPoint(affinePoint.GetAffineX(), 
                affinePoint.GetAffineY(), 1);

            return jacobianPoint;
        }
    }

}
