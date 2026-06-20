using System;

namespace Eduard.Security
{
    /// <summary>
    /// Implements optimized modular square root computation for prime fields using 
    /// Tonelli‑Shanks, Mueller (Cipolla‑Lehmer‑Mueller), and heuristic algorithms.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides three distinct algorithms with automatic selection based on field characteristics:
    /// </para>
    /// <list type="bullet">
    /// <item><description><b>Tonelli‑Shanks</b>: Standard probabilistic algorithm for general prime fields</description></item>
    /// <item><description><b>Mueller (Cipolla‑Lehmer‑Mueller)</b>: Probabilistic algorithm using Lucas sequences, 
    /// most efficient for fields with large 2‑adic order</description></item>
    /// <item><description><b>Heuristic ModSqrt</b>: Fallback using random quadratic equations when standard approaches fail</description></item>
    /// </list>
    /// <para>
    /// Algorithm selection criteria: Mueller is used when s(s‑1) > 8m + 20, where: <br/>
    /// - s is the 2‑adic order of p‑1 (p‑1 = 2^s * t, t odd) <br/>
    /// - m is the bit length of p
    /// </para>
    /// <para>
    /// The Mueller algorithm finds a suitable parameter by random search and then evaluates <br/>
    /// the Lucas V‑sequence to extract the square root. No precomputed tables are required.
    /// </para>
    /// </remarks>
    public static class ModularSqrt
    {
        static bool enableSpeedup;

        /// <summary>
        /// Determines whether the Mueller speed‑up is beneficial for the current field.
        /// </summary>
        /// <remarks>
        /// The decision is based solely on the field structure; no precomputation is performed.
        /// </remarks>
        public static void InitParams()
        {
            /* check whether optimizations can be used */
            var field = BarrettReducer.GetModulus();
            enableSpeedup = CanSpeedup(field);
        }

        /// <summary>
        /// Computes the modular square root of a value modulo the prime field.
        /// </summary>
        /// <param name="val">The value to compute the square root for.</param>
        /// <param name="forceOutput">If true, forces root computation using heuristic method when standard algorithms fail.</param>
        /// <returns>
        /// A square root r such that r^2 = val (mod p), or 0 if no root exists and forceOutput is <b>false</b>.
        /// </returns>
        /// <remarks>
        /// Algorithm selection:
        /// <list type="number">
        /// <item><description>If the Mueller speed‑up is enabled, uses the probabilistic Cipolla‑Lehmer‑Mueller method</description></item>
        /// <item><description>If forceOutput is true, uses heuristic ModSqrt to find a root</description></item>
        /// <item><description>Otherwise, uses standard Tonelli‑Shanks probabilistic algorithm</description></item>
        /// </list>
        /// </remarks>
        public static BigInteger Compute(BigInteger val, bool forceOutput = false)
        {
            /* speed up via probabilistic Mueller (Cipolla‑Lehmer‑Mueller) method */
            if (enableSpeedup) return MuellerSqrt(val);

            /* force solving random quadratic equations to find the real root */
            if (forceOutput) return ModSqrt(val);

            /* uses the standard Tonelli-Shanks algorithm */
            return TonelliShanks(val);
        }

        /// <summary>
        /// Determines if Mueller speed‑up is beneficial for the given field.
        /// </summary>
        /// <param name="field">Prime field modulus.</param>
        /// <returns><b>true</b> if the speed‑up is beneficial; otherwise <b>false</b>.</returns>
        private static bool CanSpeedup(BigInteger field)
        {
            int m = field.GetBits();
            BigInteger order = field - 1;
            int s = 0;

            while ((order & 1) == 0)
            {
                order >>= 1;
                s++;
            }

            long left = s * (long)(s - 1);
            long right = 8L * m + 20L;
            return left > right;
        }

        /// <summary>
        /// Tonelli-Shanks algorithm for modular square root extraction.
        /// </summary>
        /// <param name="val">Value to compute square root for.</param>
        /// <returns>Square root modulo p, or 0 if no root exists.</returns>
        private static BigInteger TonelliShanks(BigInteger val)
        {
            long e = 0, r, s;
            BigInteger p = BarrettReducer.GetModulus();
            BigInteger b = 0, bp = 0, q = p - 1, n = 0;
            BigInteger t = 0, x = 0, y = 0, z = 0;

            while ((q & 1) == 0)
            {
                e++;
                q >>= 1;
            }

            /* find a generator */
            int JSymbol = 0;

            do
            {
                n = SecureRandom.Range(2, p - 1);
                JSymbol = BigInteger.Jacobi(n, p);
            } while (JSymbol != -1);

            z = BigInteger.Pow(n, q, p);
            y = z;
            r = e;

            x = BigInteger.Pow(val, (q - 1) >> 1, p);
            BigInteger sx = BarrettReducer.MultMod(x, x);

            b = BarrettReducer.MultMod(val, sx);
            x = BarrettReducer.MultMod(val, x);

            while (true)
            {
                if (b == 1 || b == p - 1)
                    return x;

                s = 1;

                do
                {
                    bp = BigInteger.Pow(b, (long)Math.Pow(2, s), p);
                    s++;
                } while (bp != 1 && bp != p - 1 && s < r);

                /* has failed */
                if (s == r) return 0;
                t = BigInteger.Pow(y, (long)Math.Pow(2, r - s - 1), p);
                y = BarrettReducer.MultMod(t, t);

                x = BarrettReducer.MultMod(x, t);
                b = BarrettReducer.MultMod(b, y);
                r = s;
            }
        }

        /// <summary>
        /// Heuristic modular square root using random quadratic equation solving.
        /// </summary>
        /// <param name="val">Value to compute square root for.</param>
        /// <returns>Square root modulo p.</returns>
        /// <remarks>
        /// Uses a randomized approach when standard algorithms fail to find a root. <br/>
        /// Exploits the identity: if delta = (p-4)*(p-val) is a quadratic residue, then a root exists.
        /// </remarks>
        private static BigInteger ModSqrt(BigInteger val)
        {
            BigInteger p = BarrettReducer.GetModulus();
            BigInteger p14 = (p + 1) >> 2;

            if ((p & 3) == 3)
                return BigInteger.Pow(val, p14, p);

            BigInteger root = 0;
            BigInteger delta = BarrettReducer.MultMod(p - 4, p - val);

            BigInteger temp = 1;
            BigInteger qnr = 0;

            BigInteger buf = 0;
            BigInteger test = 0;
            int uid = 1;

            switch (uid)
            {
                case 1:

                    root = TonelliShanks(val);
                    test = BarrettReducer.MultMod(root, root);

                    if (val == test)
                        return root;

                    goto case 2;

                case 2:

                    qnr = SecureRandom.Range(2, p - 1);

                    if (BigInteger.Jacobi(qnr, p) != -1)
                        goto case 2;

                    BigInteger square = BarrettReducer.MultMod(qnr, qnr);
                    delta = BarrettReducer.MultMod(delta, square);
                    temp = BarrettReducer.MultMod(temp, qnr);

                    buf = TonelliShanks(delta);
                    test = BarrettReducer.MultMod(buf, buf);

                    if (delta != test)
                        goto case 2;
                    goto case 3;

                case 3:

                    BigInteger vtemp = BarrettReducer.AddMod(temp, temp);
                    BigInteger inv = BarrettReducer.InvMod(vtemp);
                    root = BarrettReducer.MultMod(buf, inv);
                    break;
            }

            return root;
        }

        /// <summary>
        /// Mueller (Cipolla‑Lehmer‑Mueller) probabilistic modular square root algorithm.
        /// </summary>
        /// <param name="val">Value to compute square root for.</param>
        /// <returns>Square root modulo p, or 0 if no quadratic residue.</returns>
        /// <remarks>
        /// Uses random search to locate a parameter <c>t</c> such that <c>val * t^2 - 4</c> is a <br/>
        /// quadratic non‑residue, then evaluates the Lucas V‑sequence to extract the root. <br/>
        /// The algorithm is probabilistic, with expected polynomial running time.
        /// </remarks>
        private static BigInteger MuellerSqrt(BigInteger val)
        {
            BigInteger field = BarrettReducer.GetModulus();
            BigInteger p14 = (field + 1) >> 2;

            if (BigInteger.Jacobi(val, field) != 1) 
                return 0;

            if (val == 4) 
                return 2;

            if ((field & 3) == 3) 
                return BigInteger.Pow(
                    val, p14, field);

            BigInteger t = 1;
            BigInteger P = 0;

            BigInteger eval = 0;
            BigInteger t2, qt2;

            eval = BarrettReducer.SubMod(val, 4);
            int jSymbol = BigInteger.Jacobi(eval, field);

            if (jSymbol == -1)
                t = 1;
            else
            {
                while (jSymbol != -1)
                {
                    t = SecureRandom.Range(2, field - 2);
                    t2 = BarrettReducer.MultMod(t, t);
                    qt2 = BarrettReducer.MultMod(val, t2);

                    eval = BarrettReducer.SubMod(qt2, 4);
                    jSymbol = BigInteger.Jacobi(eval, field);
                }
            }

            t2 = BarrettReducer.MultMod(t, t);
            qt2 = BarrettReducer.MultMod(val, t2);

            P = BarrettReducer.SubMod(qt2, 2);
            BigInteger root = LucasV(p14, P);

            BigInteger ti = BarrettReducer.InvMod(t);
            return BarrettReducer.MultMod(root, ti);
        }

        /// <summary>
        /// Evaluates the Lucas V‑sequence modulo the field prime.
        /// </summary>
        /// <param name="n">Exponent for V_n(P).</param>
        /// <param name="P">Sequence parameter.</param>
        /// <returns>V_n(P) mod p.</returns>
        private static BigInteger LucasV(BigInteger n, BigInteger P)
        {
            BigInteger d1 = 2, d2 = P, m = n - 1;
            BigInteger field = BarrettReducer.GetModulus();
            int nbits = m.GetBits();

            for (int i = nbits - 1; i >= 0; i--)
            {
                if (m.TestBit(i))
                {
                    BigInteger d12 = BarrettReducer.MultMod(d1, d2);
                    BigInteger d1t = BarrettReducer.SubMod(d12, P);

                    BigInteger d24 = BarrettReducer.MultMod(d2, d2);
                    BigInteger d2t = BarrettReducer.SubMod(d24, 2);
                    d1 = d1t; d2 = d2t;
                }
                else
                {
                    BigInteger d21 = BarrettReducer.MultMod(d2, d1);
                    BigInteger d2t = BarrettReducer.SubMod(d21, P);

                    BigInteger d14 = BarrettReducer.MultMod(d1, d1);
                    BigInteger d1t = BarrettReducer.SubMod(d14, 2);
                    d1 = d1t; d2 = d2t;
                }
            }

            return d2;
        }
    }
}
