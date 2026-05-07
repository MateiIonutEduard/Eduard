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
        /// <exception cref="ArgumentNullException">Thrown when moduli is null.</exception>
        /// <exception cref="ArgumentException">Thrown when moduli is empty.</exception>
        /// <remarks>
        /// Assumes moduli are pairwise coprime. Non-coprime moduli produce incorrect reconstruction.
        /// </remarks>
        public static void Init(uint[] moduli, bool negative = false)
        {
            if (ReferenceEquals(moduli, null))
                throw new ArgumentNullException(nameof(moduli), 
                    "Moduli array must not be null.");

            if (moduli.Length == 0)
                throw new ArgumentException(
                    "Moduli array must not" + 
                    " be empty.", nameof(moduli));

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
        /// <returns>The reconstructed integer, optionally normalized to centered range.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="Init"/> has not been called.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when residues is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when residues length does not match the number of moduli.
        /// </exception>
        public static BigInteger GetInteger(uint[] residues)
        {
            if (ReferenceEquals(residues, null))
                throw new ArgumentNullException(nameof(residues), 
                    "Residues array must not be null.");

            if (ReferenceEquals(m, null))
                throw new InvalidOperationException(
                    "Garner not initialized. Call" + 
                    " Init() first.");

            if (residues.Length != n)
                throw new ArgumentException(
                    "Residues length must match " 
                    + "the number of moduli.", 
                    nameof(residues));

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
