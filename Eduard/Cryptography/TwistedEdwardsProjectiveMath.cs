using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography
{
    public static class TwistedEdwardsProjectiveMath
    {
        /// <summary>
        /// Add two projective points on the twisted Edwards curve using the complete formula.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ProjectivePoint CompleteAdd(TwistedEdwardsCurve curve, ProjectivePoint left, ProjectivePoint right)
        {
            if (left == ProjectivePoint.POINT_INFINITY) return right;
            if (right == ProjectivePoint.POINT_INFINITY) return left;

            BigInteger p = curve.field;
            BigInteger A1 = (left.z * right.z) % p;

            BigInteger A2 = (A1 * A1) % p;
            BigInteger A3 = (left.x * right.x) % p;

            BigInteger A4 = (left.y * right.y) % p;
            BigInteger A5 = (curve.d * A3 * A4) % p;

            BigInteger A6 = (p + A2 - A5) % p;
            BigInteger A7 = (A2 + A5) % p;

            BigInteger A8 = ((left.x + left.y) * (right.x + right.y)) % p;
            BigInteger A9 = (p + A4 - ((curve.a * A3) % p)) % p;

            BigInteger A10 = (A8 - A3 - A4) % p;
            if (A10 < 0) A10 += p;

            BigInteger X = (((A1 * A6) % p) * A10) % p;
            BigInteger Y = (A1 * ((A7 * A9) % p)) % p;
            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ProjectivePoint.POINT_INFINITY;
            return new ProjectivePoint(X, Y, Z);
        }

        /// <summary>
        /// Double the given point in projective coordinates on the twisted Edwards curve using the complete formula.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ProjectivePoint CompleteDoubling(TwistedEdwardsCurve curve, ProjectivePoint point)
        {
            if (point == ProjectivePoint.POINT_INFINITY)
                return ProjectivePoint.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger A1 = ((point.x + point.y) * (point.x + point.y)) % p;

            BigInteger A2 = (point.x * point.x) % p;
            BigInteger A3 = (point.y * point.y) % p;

            BigInteger A4 = (curve.a * A2) % p;
            BigInteger A5 = (A4 + A3) % p;

            BigInteger A6 = (point.z * point.z) % p;
            BigInteger A7 = (p + A5 - ((2 * A6) % p)) % p;

            BigInteger X = (A7 * ((A1 - A2 - A3) % p)) % p;
            if(X < 0) X += p;

            BigInteger Y = (A5 * ((p + A4 - A3) % p)) % p;
            BigInteger Z = (A5 * A7) % p;

            if (Z == 0) return ProjectivePoint.POINT_INFINITY;
            return new ProjectivePoint(X, Y, Z);
        }


        /// <summary>
        /// Double the projective point on the twisted Edwards curve using the dedicated formula.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ProjectivePoint DedicatedDoubling(TwistedEdwardsCurve curve, ProjectivePoint point)
        {
            if (point == ProjectivePoint.POINT_INFINITY)
                return ProjectivePoint.POINT_INFINITY;

            BigInteger p = curve.field;
            BigInteger A1 = (point.x * point.x) % p;

            BigInteger A2 = (point.y * point.y) % p;
            BigInteger A3 = (2 * point.z * point.z) % p;

            BigInteger A4 = (curve.a * A1) % p;
            BigInteger A5 = ((((point.x + point.y) * (point.x + point.y)) % p) - A1 - A2) % p;

            if (A5 < 0) A5 += p;
            BigInteger A6 = (A2 + A4) % p;

            BigInteger A7 = (p + A6 - A3) % p;
            BigInteger A8 = (p + A4 - A2) % p;

            BigInteger X = (A5 * A7) % p;
            BigInteger Y = (A6 * A8) % p;

            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ProjectivePoint.POINT_INFINITY;
            return new ProjectivePoint(X, Y, Z);
        }

        /// <summary>
        /// Compute the additive inverse of a projective point on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ProjectivePoint Negate(TwistedEdwardsCurve curve, ProjectivePoint point)
        {
            BigInteger x = curve.field - point.x;
            return new ProjectivePoint(x, point.y, point.z);
        }
    }
}
