using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography
{
    /// <summary>
    /// Represents an elliptic curve given in Montgomery form.
    /// </summary>
    public sealed class MontgomeryCurve
    {
        public BigInteger field, order;
        public BigInteger A, B, BInv, A24;

        private static RandomNumberGenerator rand;
        private static bool enableSpeedup;

        /// <summary>
        /// Create a Montgomery curve with specified coefficients.
        /// </summary>
        /// <param name="args"></param>
        public MontgomeryCurve(params BigInteger[] args)
        {
            if (args.Length > 4)
                throw new ArgumentException("Too many arguments.");

            rand = RandomNumberGenerator.Create();
            A = args[0];
            B = args[1];

            field = args[2];
            order = args[3];

            BInv = B.Inverse(field);
            BigInteger temp = new BigInteger(4).Inverse(field);
            A24 = ((A + 2) * temp) % field;

            enableSpeedup = ModSqrtUtil.CanSpeedup(field);
            ModSqrtUtil.InitParams(field);
        }

        /// <summary>
        /// Evaluate the Montgomery curve equation at a given x-coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public BigInteger Evaluate(BigInteger x)
        {
            BigInteger X2 = (x * x) % field;
            BigInteger result = (x * X2) % field;

            BigInteger temp = (A * X2 + x) % field;
            result += temp;

            if (result >= field)
                result -= field;

            return (result * BInv) % field;
        }

        /// <summary>
        /// Computes the modular square root of an integer in a prime field.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="forceOutput"></param>
        /// <returns></returns>
        private BigInteger Sqrt(BigInteger val, bool forceOutput = false)
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
