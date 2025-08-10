#pragma warning disable

namespace Eduard.Security
{
    /* Hankerson, D.R., Vanstone, S.A., Menezes, A.J. (2004): Guide to elliptic curve cryptography. Springer, New York, NY. */
    public static class JacobianMath
    {
        public static JacobianPoint Add(EllipticCurve curve, JacobianPoint left, JacobianPoint right)
        {
            if (left == JacobianPoint.POINT_INFINITY) return right;
            if (right == JacobianPoint.POINT_INFINITY) return left;
            BigInteger p = curve.field;

            BigInteger A1 = (left.x * ((right.z * right.z) % p)) % p;
            BigInteger A2 = (right.x * ((left.z * left.z) % p)) % p;

            BigInteger A3 = (left.y * (((right.z * right.z) % p) * right.z) % p) % p;
            BigInteger A4 = (right.y * (((left.z * left.z) % p) * left.z) % p) % p;

            BigInteger A5 = A2 - A1;
            if (A5 < 0) A5 += p;

            BigInteger A6 = A4 - A3;
            if (A6 < 0) A6 += p;

            BigInteger A7 = (A5 * A5) % p;
            BigInteger A8 = (A5 * A7) % p;

            BigInteger A9 = (A1 * A7) % p;
            BigInteger X = (((A6 * A6) % p) - A8 - 2 * A9) % p;
            if (X < 0) X += p;

            BigInteger Y = (((A6 * (A9 - X)) % p) - A3 * A8) % p;
            if (Y < 0) Y += p;

            BigInteger Z = (((left.z * right.z) % p) * A5) % p;
            return new JacobianPoint(X, Y, Z);
        }

        public static JacobianPoint Doubling(EllipticCurve curve, JacobianPoint jacobianPoint)
        {
            if (jacobianPoint == JacobianPoint.POINT_INFINITY) return JacobianPoint.POINT_INFINITY;
            BigInteger p = curve.field;

            BigInteger A1 = (jacobianPoint.y * jacobianPoint.y) % p;
            BigInteger A2 = (4 * jacobianPoint.x * A1) % p;

            BigInteger A3 = (8 * A1 * A1) % p;
            BigInteger A4 = 0;

            if (curve.a != p - 3)
                A4 = ((3 * jacobianPoint.x * jacobianPoint.x) % p + (curve.a * ((jacobianPoint.z * jacobianPoint.z) % p) * ((jacobianPoint.z * jacobianPoint.z) % p)) % p) % p;
            else
            {
                /* special case when Weierstrass curve parameter a = -3 */
                BigInteger Z12 = (jacobianPoint.z * jacobianPoint.z) % p;
                A4 = (3 * (jacobianPoint.x - Z12) * (jacobianPoint.x - Z12)) % p;
            }

            BigInteger X = ((A4 * A4) % p - 2 * A2) % p;
            if (X < 0) X += p;

            BigInteger Y = (A4 * (A2 - X) - A3) % p;
            if (Y < 0) Y += p;

            BigInteger Z = (2 * jacobianPoint.y * jacobianPoint.z) % p;
            return new JacobianPoint(X, Y, Z);
        }

        public static JacobianPoint Negate(EllipticCurve curve, JacobianPoint point)
        {
            if (point == JacobianPoint.POINT_INFINITY) return JacobianPoint.POINT_INFINITY;
            return new JacobianPoint(point.x, curve.field - point.y, point.z);
        }
    }

}
