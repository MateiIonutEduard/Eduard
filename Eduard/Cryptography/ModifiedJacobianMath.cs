namespace Eduard.Cryptography
{
    /* Cohen–Miyaji–Ono (1998) "Efficient elliptic curve exponentiation using mixed coordinates" */
    public static class ModifiedJacobianMath
    {
        public static ModifiedJacobianPoint Add(EllipticCurve curve, ModifiedJacobianPoint left, ModifiedJacobianPoint right)
        {
            if (left == ModifiedJacobianPoint.POINT_INFINITY) return right;
            if (right == ModifiedJacobianPoint.POINT_INFINITY) return left;

            BigInteger p = curve.field;
            BigInteger A1 = (left.z * left.z) % p;
            BigInteger A2 = (right.z * right.z) % p;

            BigInteger A3 = (left.x * A2) % p;
            BigInteger A4 = (right.x * A1) % p;

            BigInteger A5 = (left.y * right.z * A2) % p;
            BigInteger A6 = (right.y * left.z * A1) % p;

            BigInteger A7 = (A4 - A3) % p;
            if (A7 < 0) A7 += p;

            BigInteger A8 = (A7 * A7) % p;
            BigInteger A9 = (A7 * A8) % p;

            BigInteger A10 = (A6 - A5) % p;
            if (A10 < 0) A10 += p;
            BigInteger A11 = (A3 * A8) % p;

            BigInteger X = (A10 * A10 - A9 - 2 * A11) % p;
            if (X < 0) X += p;

            BigInteger Y = (A10 * (A11 - X) - A5 * A9) % p;
            if (Y < 0) Y += p;

            BigInteger Z = (left.z * right.z * A7) % p;
            if (Z == 0) return ModifiedJacobianPoint.POINT_INFINITY;
            BigInteger Z2 = (Z * Z) % p;

            BigInteger aZ4 = (curve.a * Z2) % p;
            return new ModifiedJacobianPoint(X, Y, Z, aZ4);
        }

        public static ModifiedJacobianPoint Doubling(EllipticCurve curve, ModifiedJacobianPoint jacobianPoint)
        {
            if (jacobianPoint == ModifiedJacobianPoint.POINT_INFINITY) 
                return ModifiedJacobianPoint.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger A1 = (jacobianPoint.x * jacobianPoint.x) % p;

            BigInteger A2 = (jacobianPoint.y * jacobianPoint.y) % p;
            BigInteger A3 = (8 * A2 * A2) % p;

            BigInteger A4 = (4 * jacobianPoint.x * A2) % p;
            BigInteger A5 = (3 * A1 + jacobianPoint.aZ4) % p;

            BigInteger X = (A5 * A5 - 2 * A4) % p;
            if (X < 0) X += p;

            BigInteger Y = (A5 * (A4 - X) - A3) % p;
            if (Y < 0) Y += p;

            BigInteger Z = (2 * jacobianPoint.y * jacobianPoint.z) % p;
            if (Z == 0) return ModifiedJacobianPoint.POINT_INFINITY;

            BigInteger aZ4 = (2 * A3 * jacobianPoint.aZ4) % p;
            return new ModifiedJacobianPoint(X, Y, Z, aZ4);
        }

        public static ModifiedJacobianPoint Negate(EllipticCurve curve, ModifiedJacobianPoint point)
        {
            if (point == ModifiedJacobianPoint.POINT_INFINITY) return ModifiedJacobianPoint.POINT_INFINITY;
            return new ModifiedJacobianPoint(point.x, curve.field - point.y, point.z, point.aZ4);
        }
    }

}
