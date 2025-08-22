using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography
{
    /// <summary>
    /// Represents an elliptic curve given in twisted Edwards form.
    /// </summary>
    public sealed class TwistedEdwardsCurve
    {
        public BigInteger a, d;
        public BigInteger field, order;

        public BigInteger cofactor;
        private ECPoint basePoint;

        private static RandomNumberGenerator rand;
        private static bool enableSpeedup;

        internal bool computeOnTwist;
        internal BigInteger kt, aroot;
        internal bool isComplete;

        /// <summary>
        /// Creates a twisted Edwards curve with given coefficients.
        /// </summary>
        /// <param name="args"></param>
        public TwistedEdwardsCurve(params BigInteger[] args)
        {
            if (args.Length > 5)
                throw new ArgumentException("Too many arguments.");

            rand = RandomNumberGenerator.Create();
            a = args[0];
            d = args[1];

            field = args[2]; order = args[3];
            basePoint = ECPoint.POINT_INFINITY;
            cofactor = args[4];

            isComplete = (BigInteger.Jacobi(a, field) == 1
                && BigInteger.Jacobi(d, field) == -1);

            enableSpeedup = ModSqrtUtil.CanSpeedup(field);
            ModSqrtUtil.InitParams(field);

            computeOnTwist = false;
            kt = aroot = 0;

            /* see Hisil et al. (2008) "Twisted Edwards curves revisited." pp. 326-343 */
            if (BigInteger.Jacobi(field - a, field) == 1 && isComplete)
            {
                BigInteger inv_a = (field - a).Inverse(field);
                kt = (2 * (field - d) * inv_a) % field;

                aroot = Sqrt(field - a, true);
                computeOnTwist = true;
            }
        }

        /// <summary>
        /// Evaluates the twisted Edwards curve at a given x-coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public BigInteger Evaluate(BigInteger x)
        {
            BigInteger A1 = (x * x) % field;
            BigInteger A2 = (a * A1) % field;

            BigInteger A3 = (d * A1) % field;
            BigInteger Y2 = (field + 1 - A2) % field;

            BigInteger temp = (field + 1 - A3) % field;
            Y2 = (Y2 * temp.Inverse(field)) % field;
            return Y2;
        }

        /// <summary>
        /// Computes the modular square root of an integer over the prime field.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="forceOutput"></param>
        /// <returns></returns>
        public BigInteger Sqrt(BigInteger val, bool forceOutput = false)
        {
            /* compute the modular square root using the optimized Rotaru-Iftene method */
            if (enableSpeedup)
                return OptimizedRotaruIftene.Sqrt(val);

            /* if the correct output is required, the algorithm will solve random quadratic equations to find the real root */
            if (forceOutput) return ModSqrtUtil.Sqrt(val, field);

            /* uses the standard Tonelli-Shanks algorithm to obtain the modular square root */
            return ModSqrtUtil.TonelliShanks(val, field);
        }
    }
}
