using System;
using System.Collections.Generic;
using System.Text;

namespace Eduard
{
    /// <summary>
    /// Implements Garner's algorithm for CRT reconstruction using Knuth's mixed-radix method.
    /// </summary>
    /// <remarks>
    /// Recovers an integer from its residues modulo a set of pairwise-coprime moduli. <br/>
    /// Precomputes modular inverses in the constructor for efficient reconstruction. <br/>
    /// Optional negative mode reconstructs the unique representative in (-N/2, N/2], <br/>
    /// useful for signed arithmetic over finite fields (Schoof, SEA).
    /// </remarks>
    public struct Garner
    {
        private uint[] c;
        private uint[] m;
        private uint[] v;

        private BigInteger N;
        private BigInteger halfN;

        private bool signed;
        private int n;

        /// <summary>
        /// Initializes a new Garner instance with a set of pairwise-coprime moduli.
        /// </summary>
        /// <param name="moduli">Array of prime or pairwise-coprime moduli.</param>
        /// <param name="negative">When true, reconstruction returns representatives in (-N/2, N/2].</param>
        /// <exception cref="ArgumentNullException">Thrown when moduli is null.</exception>
        /// <exception cref="ArgumentException">Thrown when moduli is empty.</exception>
        /// <remarks>
        /// Precomputes all pairwise modular inverses for Knuth's mixed-radix conversion. <br/>
        /// Assumes moduli are pairwise coprime. Non-coprime moduli produce incorrect reconstruction.
        /// </remarks>
        public Garner(uint[] moduli, bool negative = false)
        {
            if (ReferenceEquals(moduli, null))
                throw new ArgumentNullException(nameof(moduli),
                    "Moduli array must not be null.");

            if (moduli.Length == 0)
                throw new ArgumentException(
                    "Moduli array must not" +
                    " be empty.", nameof(moduli));

            n = moduli.Length;
            int inversesCount = n * (n - 1) / 2;
            c = new uint[inversesCount];

            m = new uint[n];
            v = new uint[n];

            int k = 0;
            N = 1;

            for (int i = 0; i < n; i++)
            {
                m[i] = moduli[i];

                if (negative)
                    N *= moduli[i];

                for (int j = 0; j < i; j++, k++)
                    c[k] = CoreMath.Inverse(m[j], m[i]);
            }

            signed = negative;
            halfN = N >> 1;
        }

        /// <summary>
        /// Reconstructs the integer from a set of residues using Knuth's mixed-radix conversion.
        /// </summary>
        /// <param name="residues">Residues modulo the moduli specified in the constructor.</param>
        /// <returns>The reconstructed integer, optionally normalized to centered range.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the instance has not been properly initialized.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when residues is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when residues length does not match the number of moduli.
        /// </exception>
        public BigInteger GetInteger(uint[] residues)
        {
            if (ReferenceEquals(residues, null))
                throw new ArgumentNullException(nameof(residues), 
                    "Residues array must not be null.");

            if (ReferenceEquals(m, null))
                throw new InvalidOperationException(
                    "Garner not initialized. Construct" +
                    " an instance with new Garner(moduli).");

            if (residues.Length != n)
                throw new ArgumentException(
                    "Residues length must match " 
                    + "the number of moduli.", 
                    nameof(residues));

            v[0] = residues[0] % m[0];
            if (v[0] < 0) v[0] += m[0];
            int k = 0;

            for (int i = 1; i < n; i++)
            {
                v[i] = CoreMath.DiffMod(residues[i], v[0], m[i]);
                v[i] = CoreMath.MultMod(v[i], c[k], m[i]);
                k++;

                for (int j = 1; j < i; j++, k++)
                {
                    v[i] = CoreMath.DiffMod(v[i], v[j], m[i]);
                    v[i] = CoreMath.MultMod(v[i], c[k], m[i]);
                }
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
