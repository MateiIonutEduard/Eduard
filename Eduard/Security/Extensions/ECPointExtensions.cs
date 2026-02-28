using Eduard.Security.Curves;
using Eduard.Security.Primitives;
using System.Diagnostics;

namespace Eduard.Security.Extensions
{
    /// <summary>
    /// This class is a utility for converting projective points to affine form and vice versa.
    /// </summary>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class ECPointExtensions
    {
        /// <summary>
        /// Convert a given point in Jacobian projective form to an affine point on the elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint ToAffine(this EllipticCurve curve, ECPoint3w point)
        {
            if (point == ECPoint3w.POINT_INFINITY || point.z == 0) 
                return ECPoint.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger Z2 = (point.z * point.z) % p;

            BigInteger Z3 = (Z2 * point.z) % p;
            BigInteger X = (point.x * Z2.Inverse(p)) % p;

            BigInteger Y = (point.y * Z3.Inverse(p)) % p;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert an extended projective point to an affine point on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint ToAffine(this TwistedEdwardsCurve curve, ECPoint4 point)
        {
            if(point == ECPoint4.POINT_INFINITY || point.z == 0)
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
        public static ECPoint ToAffine(this TwistedEdwardsCurve curve, ECPoint3 point)
        {
            if (point == ECPoint3.POINT_INFINITY || point.z == 0)
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
        public static ECPoint4 ToExtendedProjective(this TwistedEdwardsCurve curve, ECPoint point)
        {
            if(point == ECPoint.POINT_INFINITY || (point.x == 0 && point.y == 1)) 
                return ECPoint4.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger t = (point.x * point.y) % p;
            return new ECPoint4(point.x, point.y, t, 1);
        }

        /// <summary>
        /// Convert a point from homogeneous projective coordinates to extended projective coordinates on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint4 ToExtendedProjective(this TwistedEdwardsCurve curve, ECPoint3 point)
        {
            if (point == ECPoint3.POINT_INFINITY)
                return ECPoint4.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger xz = (point.x * point.z) % p;

            BigInteger yz = (point.y * point.z) % p;
            BigInteger xy = (point.x * point.y) % p;

            BigInteger z2 = (point.z * point.z) % p;
            return new ECPoint4(xz, yz, xy, z2);
        }

        /// <summary>
        /// Convert a point from homogeneous to extended projective coordinates on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint4 GetPointCopy(this TwistedEdwardsCurve curve, ECPoint3 point)
        {
            /* point at infinity */
            if(point == ECPoint3.POINT_INFINITY)
                return ECPoint4.POINT_INFINITY;

            var res = new ECPoint4();
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
        public static ECPoint3 ToProjective(this TwistedEdwardsCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY || (point.x == 0 && point.y == 1))
                return ECPoint3.POINT_INFINITY;

            return new ECPoint3(point.x, point.y, 1);
        }

        /// <summary>
        /// Convert a point from extended to homogeneous projective coordinates on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint3 ToProjective(this TwistedEdwardsCurve curve, ECPoint4 point)
        {
            if (point == ECPoint4.POINT_INFINITY)
                return ECPoint3.POINT_INFINITY;

            return new ECPoint3(point.x, point.y, point.z);
        }

        /// <summary>
        /// Convert an affine point on the elliptic curve to Jacobian projective form.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint3w ToJacobian(this EllipticCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY) 
                return ECPoint3w.POINT_INFINITY;

            ECPoint3w jacobianPoint = new ECPoint3w(point.GetAffineX(),
                point.GetAffineY(), 1);

            return jacobianPoint;
        }

        /// <summary>
        /// Convert a given point in modified Jacobian projective coordinates into a Jacobian projective point on the elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint3w ToJacobian(this EllipticCurve curve, ECPoint4w point)
        {
            if (point == ECPoint4w.POINT_INFINITY) 
                return ECPoint3w.POINT_INFINITY;

            ECPoint3w jacobianPoint = new ECPoint3w(point.x,
                point.y, point.z);

            return jacobianPoint;
        }

        /// <summary>
        /// Convert a given point in extended Jacobian-Chudnovsky projective coordinates into a Jacobian projective point on the elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint3w ToJacobian(this EllipticCurve curve, ECPoint5w point)
        {
            if (point == ECPoint5w.POINT_INFINITY)
                return ECPoint3w.POINT_INFINITY;

            ECPoint3w jacobianPoint = new ECPoint3w(point.x,
                point.y, point.z);

            return jacobianPoint;
        }

        /// <summary>
        /// Convert a given point in extended Jacobian-Chudnovsky projective coordinates into an affine point on the elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint ToAffine(this EllipticCurve curve, ECPoint5w point)
        {
            if (point == ECPoint5w.POINT_INFINITY || point.z == 0) return ECPoint.POINT_INFINITY;
            BigInteger p = curve.field;

            BigInteger X = (point.x * point.z2.Inverse(p)) % p;
            BigInteger Y = (point.y * point.z3.Inverse(p)) % p;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert an affine point on the elliptic curve to the extended Jacobian-Chudnovsky projective form.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint5w ToJacobianChudnovsky(this EllipticCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY) return ECPoint5w.POINT_INFINITY;
            ECPoint5w jacobianChudnovskyPoint = new ECPoint5w(point.GetAffineX(), point.GetAffineY(), 1, 1, 1);
            return jacobianChudnovskyPoint;
        }

        /// <summary>
        /// Convert a given point in modified Jacobian projective coordinates into an affine point on the elliptic curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint ToAffine(this EllipticCurve curve, ECPoint4w point)
        {
            if (point == ECPoint4w.POINT_INFINITY || point.z == 0) return ECPoint.POINT_INFINITY;
            BigInteger p = curve.field;

            BigInteger Z2 = (point.z * point.z) % p;
            BigInteger Z3 = (Z2 * point.z) % p;
            BigInteger X = (point.x * Z2.Inverse(p)) % p;

            BigInteger Y = (point.y * Z3.Inverse(p)) % p;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Convert an affine point on the elliptic curve into a given point in modified Jacobian projective coordinates.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint4w ToModifiedJacobian(this EllipticCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY) return ECPoint4w.POINT_INFINITY;
            ECPoint4w modifiedJacobianPoint = new ECPoint4w(point.GetAffineX(), point.GetAffineY(), 1, curve.a);
            return modifiedJacobianPoint;
        }

        /// <summary>
        /// Convert a point from Jacobian-Chudnovsky form on the elliptic curve into a given point in modified Jacobian projective coordinates.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint4w ToModifiedJacobian(this EllipticCurve curve, ECPoint5w point)
        {
            if (point == ECPoint5w.POINT_INFINITY) return ECPoint4w.POINT_INFINITY;
            BigInteger Z4 = (point.z2 * point.z2) % curve.field;

            BigInteger aZ4 = (curve.a * Z4) % curve.field;
            ECPoint4w modifiedJacobianPoint = new ECPoint4w(point.x, point.y, point.z, aZ4);
            return modifiedJacobianPoint;
        }

        /// <summary>
        /// Convert a point from Jacobian form on the elliptic curve into a given point in modified Jacobian projective coordinates.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint4w ToModifiedJacobian(this EllipticCurve curve, ECPoint3w point)
        {
            if (point == ECPoint3w.POINT_INFINITY) return ECPoint4w.POINT_INFINITY;
            BigInteger Z2 = (point.z * point.z) % curve.field;
            BigInteger Z4 = (Z2 * Z2) % curve.field;

            BigInteger aZ4 = (curve.a * Z4) % curve.field;
            ECPoint4w modifiedJacobianPoint = new ECPoint4w(point.x, point.y, point.z, aZ4);
            return modifiedJacobianPoint;
        }
    }
}
