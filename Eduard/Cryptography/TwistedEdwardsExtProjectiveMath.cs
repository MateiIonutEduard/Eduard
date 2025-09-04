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
