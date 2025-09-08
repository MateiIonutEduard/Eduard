namespace Eduard.Cryptography.Extensions
{
    /// <summary>
    /// This class is a utility for converting projective points to affine form and vice versa.
    /// </summary>
    public static class ECPointExtensions
    {
        /// <summary>
        /// Convert a given point in Jacobian projective form to an affine point on the elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="jacobianPoint"></param>
        /// <returns></returns>
        public static ECPoint ToAffine(this EllipticCurve curve, JacobianPoint jacobianPoint)
        {
            if (jacobianPoint == JacobianPoint.POINT_INFINITY || jacobianPoint.z == 0) 
                return ECPoint.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger Z2 = (jacobianPoint.z * jacobianPoint.z) % p;

            BigInteger Z3 = (Z2 * jacobianPoint.z) % p;
            BigInteger X = (jacobianPoint.x * Z2.Inverse(p)) % p;

            BigInteger Y = (jacobianPoint.y * Z3.Inverse(p)) % p;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert an extended projective point to an affine point on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint ToAffine(this TwistedEdwardsCurve curve, ExtendedProjectivePoint point)
        {
            if(point == ExtendedProjectivePoint.POINT_INFINITY || point.z == 0)
                return ECPoint.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger inv_Z = point.z.Inverse(p);

            BigInteger X = (point.x * inv_Z) % p;
            BigInteger Y = (point.y * inv_Z) % p;

            if (X == 0 && Y == 1) return ECPoint.POINT_INFINITY;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert a projective point to an affine point on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint ToAffine(this TwistedEdwardsCurve curve, ProjectivePoint point)
        {
            if (point == ProjectivePoint.POINT_INFINITY || point.z == 0)
                return ECPoint.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger inv_Z = point.z.Inverse(p);

            BigInteger X = (point.x * inv_Z) % p;
            BigInteger Y = (point.y * inv_Z) % p;

            if(X == 0 && Y == 1) return ECPoint.POINT_INFINITY;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert an affine point on the twisted Edwards curve to extended projective coordinates.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ExtendedProjectivePoint ToExtendedProjective(this TwistedEdwardsCurve curve, ECPoint point)
        {
            if(point == ECPoint.POINT_INFINITY || (point.x == 0 && point.y == 1)) 
                return ExtendedProjectivePoint.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger t = (point.x * point.y) % p;
            return new ExtendedProjectivePoint(point.x, point.y, t, 1);
        }

        /// <summary>
        /// Convert a point from homogeneous projective coordinates to extended projective coordinates on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ExtendedProjectivePoint ToExtendedProjective(this TwistedEdwardsCurve curve, ProjectivePoint point)
        {
            if (point == ProjectivePoint.POINT_INFINITY)
                return ExtendedProjectivePoint.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger xz = (point.x * point.z) % p;

            BigInteger yz = (point.y * point.z) % p;
            BigInteger xy = (point.x * point.y) % p;

            BigInteger z2 = (point.z * point.z) % p;
            return new ExtendedProjectivePoint(xz, yz, xy, z2);
        }

        /// <summary>
        /// Convert a point from homogeneous to extended projective coordinates on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ExtendedProjectivePoint GetPointCopy(this TwistedEdwardsCurve curve, ProjectivePoint point)
        {
            /* point at infinity */
            if(point == ProjectivePoint.POINT_INFINITY)
                return ExtendedProjectivePoint.POINT_INFINITY;

            var res = new ExtendedProjectivePoint();
            res.x = point.x; res.y = point.y;

            /* T is not used in point doubling formula */
            res.z = point.z; res.t = 0;
            return res;
        }

        /// <summary>
        /// Convert an affine point on the twisted Edwards curve to homogeneous projective coordinates.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ProjectivePoint ToProjective(this TwistedEdwardsCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY || (point.x == 0 && point.y == 1))
                return ProjectivePoint.POINT_INFINITY;

            return new ProjectivePoint(point.x, point.y, 1);
        }

        /// <summary>
        /// Convert a point from extended to homogeneous projective coordinates on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ProjectivePoint ToProjective(this TwistedEdwardsCurve curve, ExtendedProjectivePoint point)
        {
            if (point == ExtendedProjectivePoint.POINT_INFINITY)
                return ProjectivePoint.POINT_INFINITY;

            return new ProjectivePoint(point.x, point.y, point.z);
        }

        /// <summary>
        /// Convert an affine point on the elliptic curve to Jacobian projective form.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="affinePoint"></param>
        /// <returns></returns>
        public static JacobianPoint ToJacobian(this EllipticCurve curve, ECPoint affinePoint)
        {
            if (affinePoint == ECPoint.POINT_INFINITY) 
                return JacobianPoint.POINT_INFINITY;

            JacobianPoint jacobianPoint = new JacobianPoint(affinePoint.GetAffineX(),
                affinePoint.GetAffineY(), 1);

            return jacobianPoint;
        }

        /// <summary>
        /// Convert a given point in modified Jacobian projective coordinates into a Jacobian projective point on the elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="modifiedPoint"></param>
        /// <returns></returns>
        public static JacobianPoint ToJacobian(this EllipticCurve curve, ModifiedJacobianPoint modifiedPoint)
        {
            if (modifiedPoint == ModifiedJacobianPoint.POINT_INFINITY) 
                return JacobianPoint.POINT_INFINITY;

            JacobianPoint jacobianPoint = new JacobianPoint(modifiedPoint.x,
                modifiedPoint.y, modifiedPoint.z);

            return jacobianPoint;
        }

        /// <summary>
        /// Convert a given point in extended Jacobian-Chudnovsky projective coordinates into a Jacobian projective point on the elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="jacobianChudnovskyPoint"></param>
        /// <returns></returns>
        public static JacobianPoint ToJacobian(this EllipticCurve curve, JacobianChudnovskyPoint jacobianChudnovskyPoint)
        {
            if (jacobianChudnovskyPoint == JacobianChudnovskyPoint.POINT_INFINITY)
                return JacobianPoint.POINT_INFINITY;

            JacobianPoint jacobianPoint = new JacobianPoint(jacobianChudnovskyPoint.x,
                jacobianChudnovskyPoint.y, jacobianChudnovskyPoint.z);

            return jacobianPoint;
        }

        /// <summary>
        /// Convert a given point in extended Jacobian-Chudnovsky projective coordinates into an affine point on the elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="jacobianChudnovskyPoint"></param>
        /// <returns></returns>
        public static ECPoint ToAffine(this EllipticCurve curve, JacobianChudnovskyPoint jacobianChudnovskyPoint)
        {
            if (jacobianChudnovskyPoint == JacobianChudnovskyPoint.POINT_INFINITY || jacobianChudnovskyPoint.z == 0) return ECPoint.POINT_INFINITY;
            BigInteger p = curve.field;

            BigInteger X = (jacobianChudnovskyPoint.x * jacobianChudnovskyPoint.z2.Inverse(p)) % p;
            BigInteger Y = (jacobianChudnovskyPoint.y * jacobianChudnovskyPoint.z3.Inverse(p)) % p;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert an affine point on the elliptic curve to the extended Jacobian-Chudnovsky projective form.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="affinePoint"></param>
        /// <returns></returns>
        public static JacobianChudnovskyPoint ToJacobianChudnovsky(this EllipticCurve curve, ECPoint affinePoint)
        {
            if (affinePoint == ECPoint.POINT_INFINITY) return JacobianChudnovskyPoint.POINT_INFINITY;
            JacobianChudnovskyPoint jacobianChudnovskyPoint = new JacobianChudnovskyPoint(affinePoint.GetAffineX(), affinePoint.GetAffineY(), 1, 1, 1);
            return jacobianChudnovskyPoint;
        }

        /// <summary>
        /// Convert a given point in modified Jacobian projective coordinates into an affine point on the elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="modifiedJacobianPoint"></param>
        /// <returns></returns>
        public static ECPoint ToAffine(this EllipticCurve curve, ModifiedJacobianPoint modifiedJacobianPoint)
        {
            if (modifiedJacobianPoint == ModifiedJacobianPoint.POINT_INFINITY || modifiedJacobianPoint.z == 0) return ECPoint.POINT_INFINITY;
            BigInteger p = curve.field;

            BigInteger Z2 = (modifiedJacobianPoint.z * modifiedJacobianPoint.z) % p;
            BigInteger Z3 = (Z2 * modifiedJacobianPoint.z) % p;
            BigInteger X = (modifiedJacobianPoint.x * Z2.Inverse(p)) % p;

            BigInteger Y = (modifiedJacobianPoint.y * Z3.Inverse(p)) % p;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert an affine point on the elliptic curve into a given point in modified Jacobian projective coordinates.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="affinePoint"></param>
        /// <returns></returns>
        public static ModifiedJacobianPoint ToModifiedJacobian(this EllipticCurve curve, ECPoint affinePoint)
        {
            if (affinePoint == ECPoint.POINT_INFINITY) return ModifiedJacobianPoint.POINT_INFINITY;
            ModifiedJacobianPoint modifiedJacobianPoint = new ModifiedJacobianPoint(affinePoint.GetAffineX(), affinePoint.GetAffineY(), 1, curve.a);
            return modifiedJacobianPoint;
        }

        /// <summary>
        /// Convert a point from Jacobian-Chudnovsky form on the elliptic curve into a given point in modified Jacobian projective coordinates.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="jacobianChudnovskyPoint"></param>
        /// <returns></returns>
        public static ModifiedJacobianPoint ToModifiedJacobian(this EllipticCurve curve, JacobianChudnovskyPoint jacobianChudnovskyPoint)
        {
            if (jacobianChudnovskyPoint == JacobianChudnovskyPoint.POINT_INFINITY) return ModifiedJacobianPoint.POINT_INFINITY;
            BigInteger Z4 = (jacobianChudnovskyPoint.z2 * jacobianChudnovskyPoint.z2) % curve.field;

            BigInteger aZ4 = (curve.a * Z4) % curve.field;
            ModifiedJacobianPoint modifiedJacobianPoint = new ModifiedJacobianPoint(jacobianChudnovskyPoint.x, jacobianChudnovskyPoint.y, jacobianChudnovskyPoint.z, aZ4);
            return modifiedJacobianPoint;
        }

        /// <summary>
        /// Convert a point from Jacobian form on the elliptic curve into a given point in modified Jacobian projective coordinates.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="jacobianPoint"></param>
        /// <returns></returns>
        public static ModifiedJacobianPoint ToModifiedJacobian(this EllipticCurve curve, JacobianPoint jacobianPoint)
        {
            if (jacobianPoint == JacobianPoint.POINT_INFINITY) return ModifiedJacobianPoint.POINT_INFINITY;
            BigInteger Z2 = (jacobianPoint.z * jacobianPoint.z) % curve.field;
            BigInteger Z4 = (Z2 * Z2) % curve.field;

            BigInteger aZ4 = (curve.a * Z4) % curve.field;
            ModifiedJacobianPoint modifiedJacobianPoint = new ModifiedJacobianPoint(jacobianPoint.x, jacobianPoint.y, jacobianPoint.z, aZ4);
            return modifiedJacobianPoint;
        }
    }
}
