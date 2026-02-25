using System;

namespace Eduard
{
    /// <summary>
    /// Provides optimized modular reduction using Barrett's algorithm with modulus caching.
    /// </summary>
    public class BarrettReducer
    {
        /// <summary>
        /// Cached modulus from last operation.
        /// </summary>
        public static BigInteger mod;

        /// <summary>
        /// Pre-computed Barrett constant μ = ⌊2^(2k)/mod⌋.
        /// </summary>
        public static BigInteger k;

        /// <summary>
        /// Initializes the reducer with a new modulus and pre-computes its Barrett constant.
        /// </summary>
        /// <param name="field">The modulus for subsequent reductions.</param>
        public static void SetModulus(BigInteger field)
        {
            k = BigInteger.BarrettConstant(field);
            mod = field;
        }

        /// <summary>
        /// Reduces a value modulo the specified field using Barrett's algorithm. <br/>
        /// Automatically caches the modulus on first use for subsequent calls.
        /// </summary>
        /// <param name="val">The value to reduce.</param>
        /// <param name="field">The modulus. Cached after first invocation.</param>
        /// <returns>val mod field in range [0, field-1].</returns>
        public static BigInteger Reduce(BigInteger val, BigInteger field)
        {
            if (mod == null || field == mod) SetModulus(field);
            return BigInteger.BarrettReduction(val, mod, k);
        }
    }
}
