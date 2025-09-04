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
