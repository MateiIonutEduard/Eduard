using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography
{
    /* Hisil, H., Wong, K. K. H., Carter, G., & Dawson, E. (2008, December). Twisted Edwards curves revisited. 
     * In International Conference on the Theory and Application of Cryptology and Information Security 
     * (pp. 326-343). Springer Berlin Heidelberg.*/
    public static class TwistedEdwardsExtProjectiveMath
    {
        /// <summary>
        /// Add two extended projective points on the twisted Edwards curve using the unified formula.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ExtendedProjectivePoint UnifiedAdd(TwistedEdwardsCurve curve, ExtendedProjectivePoint left, ExtendedProjectivePoint right)
        {
            if (left == ExtendedProjectivePoint.POINT_INFINITY) return right;
            if (right == ExtendedProjectivePoint.POINT_INFINITY) return left;

            BigInteger p = curve.field;
            BigInteger A1 = (left.x * right.x) % p;

            BigInteger A2 = (left.y * right.y) % p;
            BigInteger A3 = (curve.d * ((left.t * right.t) % p)) % p;

            BigInteger A4 = (left.z * right.z) % p;
            BigInteger A5 = ((((left.x + left.y) * (right.x + right.y)) % p) - A1 - A2) % p;

            if (A5 < 0) A5 += p;
            BigInteger A6 = (A4 - A3) % p;

            if (A6 < 0) A6 += p;
            BigInteger A7 = (A4 + A3) % p;

            BigInteger A8 = (A2 - ((curve.a * A1) % p)) % p;
            if (A8 < 0) A8 += p;

            BigInteger X = (A5 * A6) % p;
            BigInteger Y = (A7 * A8) % p;

            BigInteger T = (A5 * A8) % p;
            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ExtendedProjectivePoint.POINT_INFINITY;
            return new ExtendedProjectivePoint(X, Y, T, Z);
        }

        /// <summary>
        /// Add two extended projective points on the twisted Edwards curve using the dedicated formula.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ExtendedProjectivePoint DedicatedAdd(TwistedEdwardsCurve curve, ExtendedProjectivePoint left, ExtendedProjectivePoint right)
        {
            if (left == ExtendedProjectivePoint.POINT_INFINITY) return right;
            if (right == ExtendedProjectivePoint.POINT_INFINITY) return left;

            BigInteger p = curve.field;
            BigInteger A1 = (left.x * right.x) % p;

            BigInteger A2 = (left.y * right.y) % p;
            BigInteger A3 = (left.z * right.t) % p;

            BigInteger A4 = (left.t * right.z) % p;
            BigInteger A5 = (A4 + A3) % p;

            BigInteger A6 = ((((left.x - left.y) * (right.x + right.y)) % p) + A2 - A1) % p;
            if (A6 < 0) A6 += p;

            BigInteger A7 = (A2 + curve.a * A1) % p;
            BigInteger A8 = (p + A4 - A3) % p;

            BigInteger X = (A5 * A6) % p;
            BigInteger Y = (A7 * A8) % p;

            BigInteger T = (A5 * A8) % p;
            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ExtendedProjectivePoint.POINT_INFINITY;
            return new ExtendedProjectivePoint(X, Y, T, Z);
        }

        /// <summary>
        /// Add two extended projective points on the curve isomorphic to the twisted Edwards curve using the dedicated formula.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ExtendedProjectivePoint TwistDedicatedAdd(TwistedEdwardsCurve curve, ExtendedProjectivePoint left, ExtendedProjectivePoint right)
        {
            if (left == ExtendedProjectivePoint.POINT_INFINITY) return right;
            if (right == ExtendedProjectivePoint.POINT_INFINITY) return left;
            BigInteger p = curve.field;

            BigInteger A1 = ((left.y - left.x) * (right.y + right.x)) % p;
            if (A1 < 0) A1 += p;

            BigInteger A2 = ((left.y + left.x) * (right.y - right.x)) % p;
            if (A2 < 0) A2 += p;

            BigInteger A3 = (2 * left.z * right.t) % p;
            BigInteger A4 = (2 * left.t * right.z) % p;

            BigInteger A5 = (A3 + A4) % p;
            BigInteger A6 = (p + A2 - A1) % p;

            BigInteger A7 = (A2 + A1) % p;
            BigInteger A8 = (p + A4 - A3) % p;

            BigInteger X = (A5 * A6) % p;
            BigInteger Y = (A7 * A8) % p;

            BigInteger T = (A5 * A8) % p;
            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ExtendedProjectivePoint.POINT_INFINITY;
            return new ExtendedProjectivePoint(X, Y, T, Z);
        }

        /// <summary>
        /// Double the given extended projective point on the twisted Edwards curve using the dedicated formula.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ExtendedProjectivePoint DedicatedDoubling(TwistedEdwardsCurve curve, ExtendedProjectivePoint point)
        {
            if (point == ExtendedProjectivePoint.POINT_INFINITY)
                return ExtendedProjectivePoint.POINT_INFINITY;

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

            BigInteger T = (A5 * A8) % p;
            BigInteger Z = (A6 * A7) % p;

            if (Z == 0) return ExtendedProjectivePoint.POINT_INFINITY;
            return new ExtendedProjectivePoint(X, Y, T, Z);
        }

        /// <summary>
        /// Compute the additive inverse of a point in extended projective coordinates on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ExtendedProjectivePoint Negate(TwistedEdwardsCurve curve, ExtendedProjectivePoint point)
        {
            BigInteger Xp = curve.field - point.x;
            BigInteger Tp = curve.field - point.t;
            return new ExtendedProjectivePoint(Xp, point.y, Tp, point.z);
        }
    }
}
