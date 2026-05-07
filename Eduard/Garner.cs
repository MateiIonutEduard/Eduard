using System;
using System.Collections.Generic;
using System.Text;

namespace Eduard
{
    /// <summary>
    /// Implements Garner's algorithm for CRT reconstruction from mixed-radix residues.
    /// </summary>
    /// <remarks>
    /// Recovers an integer from its residues modulo a set of pairwise-coprime moduli.<br/>
    /// Optional negative mode reconstructs the unique representative in (-N/2, N/2],<br/>
    /// useful for signed arithmetic over finite fields (Schoof, SEA).
    /// </remarks>
    public class Garner
    {
        static uint[][] c;
        static uint[] m;

        static BigInteger N;
        static BigInteger halfN;

        static bool signed;
        static int n;

        /// <summary>
        /// Initializes Garner's algorithm with a set of pairwise-coprime moduli.
        /// </summary>
        /// <param name="moduli">Array of prime or pairwise-coprime moduli.</param>
        /// <param name="negative">When true, reconstruction returns representatives in (-N/2, N/2].</param>
        public static void Init(uint[] moduli, bool negative = false)
        {
            n = moduli.Length;
            c = new uint[n][];
            int i;

            m = new uint[n];
            uint product;
            N = 1;

            for (i = 0; i < n; i++)
            {
                c[i] = new uint[n];
                m[i] = moduli[i];
                product = 1;

                if (negative)
                    N *= moduli[i];

                for (int j = 0; j < i; j++)
                    product = CoreMath.MultMod(product, 
                        moduli[j], moduli[i]);
            }

            signed = negative;
            halfN = N >> 1;
        }

        /// <summary>
        /// Reconstructs the integer from a set of residues using mixed-radix conversion.
        /// </summary>
        /// <param name="residues">Residues modulo the moduli specified in <see cref="Init"/>.</param>
        /// <returns>The reconstructed integer, optionally normalized to signed range.</returns>
        public static BigInteger GetInteger(uint[] residues)
        {
            uint[] v = new uint[n];
            uint product, sum;

            v[0] = residues[0] % m[0];
            if (v[0] < 0) v[0] += m[0];

            for (int i = 1; i < n; i++)
            {
                sum = v[0];
                product = 1;

                for (int j = 1; j < i; j++)
                {
                    product = CoreMath.MultMod(product, m[j - 1], m[i]);
                    CoreMath.MultAdd(v[j], product, sum, m[i], ref sum);
                }

                uint diff = CoreMath.DiffMod(residues[i], sum, m[i]);
                product = CoreMath.MultMod(product, m[i - 1], m[i]);

                uint inverse = CoreMath.Inverse(product, m[i]);
                v[i] = CoreMath.MultMod(diff, inverse, m[i]);
            }

            BigInteger res = v[0];
            BigInteger weight = 1;

            for (int i = 1; i < n; i++)
            {
                weight *= m[i - 1];
                res += v[i] * weight;
            }

            if (signed)
            {
                if (res > halfN) 
                    res -= N;
            }

            return res;
        }
    }
}
