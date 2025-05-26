namespace Eduard.Security
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

        public static ECPoint ToAffine(this EllipticCurve curve, JacobianChudnovskyPoint jacobianChudnovskyPoint)
        {
            if (jacobianChudnovskyPoint == JacobianChudnovskyPoint.POINT_INFINITY) return ECPoint.POINT_INFINITY;
            BigInteger p = curve.field;

            BigInteger X = (jacobianChudnovskyPoint.x * jacobianChudnovskyPoint.z2.Inverse(p)) % p;
            BigInteger Y = (jacobianChudnovskyPoint.y * jacobianChudnovskyPoint.z3.Inverse(p)) % p;
            return new ECPoint(X, Y);
        }


        public static JacobianChudnovskyPoint ToJacobianChudnovsky(this EllipticCurve curve, ECPoint affinePoint)
        {
            JacobianChudnovskyPoint jacobianChudnovskyPoint = new JacobianChudnovskyPoint(affinePoint.GetAffineX(), affinePoint.GetAffineY(), 1, 1, 1);
            return jacobianChudnovskyPoint;
        }

        public static ECPoint ToAffine(this EllipticCurve curve, ModifiedJacobianPoint modifiedJacobianPoint)
        {
            if (modifiedJacobianPoint == ModifiedJacobianPoint.POINT_INFINITY) return ECPoint.POINT_INFINITY;
            BigInteger p = curve.field;

            BigInteger Z2 = (modifiedJacobianPoint.z * modifiedJacobianPoint.z) % p;
            BigInteger Z3 = (Z2 * modifiedJacobianPoint.z) % p;
            BigInteger X = (modifiedJacobianPoint.x * Z2.Inverse(p)) % p;

            BigInteger Y = (modifiedJacobianPoint.y * Z3.Inverse(p)) % p;
            return new ECPoint(X, Y);
        }

        public static ModifiedJacobianPoint ToModifiedJacobian(this EllipticCurve curve, ECPoint affinePoint)
        {
            ModifiedJacobianPoint modifiedJacobianPoint = new ModifiedJacobianPoint(affinePoint.GetAffineX(), affinePoint.GetAffineY(), curve.a, 1);
            return modifiedJacobianPoint;
        }
    }
}
