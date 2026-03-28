using System;
using Eduard;
using System.Security.Cryptography;

namespace Eduard.Security
{
    /// <summary>
    /// Implements optimized modular square root computation for prime fields using 
    /// Tonelli-Shanks, Rotaru-Iftene, and heuristic algorithms.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Provides three distinct algorithms with automatic selection based on field characteristics:
    /// </para>
    /// <list type="bullet">
    /// <item><description><b>Tonelli-Shanks</b>: Standard probabilistic algorithm for general prime fields</description></item>
    /// <item><description><b>Rotaru-Iftene</b>: Efficient probabilistic algorithm optimized for fields with large 2-adic order</description></item>
    /// <item><description><b>Heuristic ModSqrt</b>: Fallback using random quadratic equations when standard approaches fail</description></item>
    /// </list>
    /// <para>
    /// Algorithm selection criteria: Rotaru-Iftene is used when s(s-1) > 8m + 20, where: <br/>
    /// - s is the 2-adic order of p-1 (p-1 = 2^s * t, t odd) <br/>
    /// - m is the bit length of p
    /// </para>
    /// <para>
    /// The Rotaru-Iftene algorithm trades determinism for efficiency, offering faster square root <br/>
    /// extraction with negligible probability of failure, suitable for cryptographic applications <br/>
    /// where occasional retries are acceptable.
    /// </para>
    /// </remarks>
    public static class ModSqrtUtil
    {
        static bool[] e;
        static int s, k, q;
        static int step, rem;

        static BigInteger p, t;
        static BigInteger a_modp, b_modp;
        static BigInteger d_prec, A_prec;

        static BigInteger aux_A, aux_ACC;
        static BigInteger[] D_modp, A_modp, ACC;
        static bool enableSpeedup;

        /// <summary>
        /// Initializes precomputed tables for the Rotaru-Iftene probabilistic algorithm.
        /// </summary>
        /// <param name="windowSize">Window size for exponentiation optimization. Default is 4.</param>
        /// <remarks>
        /// Determines whether speed optimizations are beneficial based on field size and structure. <br/>
        /// When enabled, precomputes powers of a quadratic non-residue for efficient square root extraction. <br/>
        /// The precomputation cost is amortized across multiple square root operations.
        /// </remarks>
        public static void InitParams(int windowSize = 4)
        {
            /* check whether optimizations can be used */
            p = BarrettReducer.GetModulus();
            enableSpeedup = CanSpeedup(p);

            if (enableSpeedup)
                Precompute(windowSize);
        }

        /// <summary>
        /// Computes the modular square root of a value modulo the prime field.
        /// </summary>
        /// <param name="val">The value to compute the square root for.</param>
        /// <param name="forceOutput">If true, forces root computation using heuristic method when standard algorithms fail.</param>
        /// <returns>
        /// A square root r such that r^2 = val (mod p), or 0 if no root exists and forceOutput is false.
        /// </returns>
        /// <remarks>
        /// Algorithm selection:
        /// <list type="number">
        /// <item><description>If precomputed optimizations are available, uses Rotaru-Iftene probabilistic method</description></item>
        /// <item><description>If forceOutput is true, uses heuristic ModSqrt to find a root</description></item>
        /// <item><description>Otherwise, uses standard Tonelli-Shanks probabilistic algorithm</description></item>
        /// </list>
        /// </remarks>
        public static BigInteger Sqrt(BigInteger val, bool forceOutput = false)
        {
            /* speed up via optimized Rotaru-Iftene method */
            if (enableSpeedup)
                return RotaruIftene(val);

            /* force solving random quadratic equations to find the real root */
            if (forceOutput) return ModSqrt(val);

            /* uses the standard Tonelli-Shanks algorithm */
            return TonelliShanks(val);
        }

        /// <summary>
        /// Determines if Rotaru-Iftene probabilistic optimization is beneficial for the given field.
        /// </summary>
        /// <param name="field">Prime field modulus.</param>
        /// <returns>True if probabilistic speedup is beneficial; otherwise false.</returns>
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
            if ((p & 3) == 3)
                return BigInteger.Pow(val, (p + 1) >> 2, p);

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
        /// Precomputes lookup tables for Rotaru-Iftene probabilistic algorithm.
        /// </summary>
        /// <param name="windowSize">Window size for exponentiation optimization.</param>
        private static void Precompute(int windowSize)
        {
            s = 0;
            BigInteger order = p - 1;

            while ((order & 1) == 0)
            {
                order >>= 1;
                s++;
            }

            t = order;
            k = windowSize;
            q = (int)Math.Floor((double)(s - 2) / (double)k);

            step = s - 2;
            rem = (s - 2) % k + 1;

            ACC = new BigInteger[k];
            e = new bool[s - 1];

            for (int i = 0; i <= s - 2; i++)
                e[i] = false;

            BigInteger aux_d = 0;
            int jSymbol = 0;

            do
            {
                aux_d = SecureRandom.Range(2, p - 2);
                jSymbol = BigInteger.Jacobi(aux_d, p);
            }
            while (jSymbol != -1);

            d_prec = aux_d;
            D_modp = new BigInteger[s];
            D_modp[0] = BigInteger.Pow(d_prec, t, p);

            for (int i = 1; i <= s - 1; i++)
                D_modp[i] = BarrettReducer.MultMod(D_modp[i - 1], D_modp[i - 1]);
        }

        /// <summary>
        /// Rotaru-Iftene efficient probabilistic modular square root algorithm.
        /// </summary>
        /// <param name="val">Value to compute square root for.</param>
        /// <returns>Square root modulo p.</returns>
        /// <remarks>
        /// Implements the probabilistic algorithm described in "A Fast Algorithm for Computing Square Roots <br/>
        /// in Finite Fields" by Rotaru and Iftene. Optimized for fields where the 2-adic order of p-1 <br/>
        /// is sufficiently large relative to the bit length. The algorithm succeeds with overwhelming <br/>
        /// probability and is significantly faster than Tonelli-Shanks for large s.
        /// </remarks>
        private static BigInteger RotaruIftene(BigInteger val)
        {
            a_modp = val;
            step = s - 2;

            BigInteger aux = 0;
            int index_check, i, j;

            aux_A = BigInteger.Pow(2 * a_modp, (t - 1) >> 1, p);
            BigInteger A1 = BarrettReducer.MultMod(aux_A, aux_A);

            BigInteger A2 = BarrettReducer.MultMod(a_modp, A1);
            A_prec = BarrettReducer.AddMod(A2, A2);
            aux = A_prec;

            A_modp = new BigInteger[q + 1];
            A_modp[0] = A_prec;
            j = 1;

            for (i = 1; i <= s - 2; i++)
            {
                aux = BarrettReducer.MultMod(aux, aux);
                index_check = s - 1 - i;

                if ((index_check % k == 0) && (index_check / k >= 1) && (index_check / k <= q))
                {
                    A_modp[j] = aux;
                    j++;
                }
            }

            for (i = 1; i <= q; i++)
            {
                CompleteAccumulatorUpdate();
                CompleteInnerLoop();
            }

            FinalAccumulatorUpdateAndInnerLoop();
            b_modp = (a_modp * aux_A * aux_ACC * (A_modp[0] * ((aux_ACC * aux_ACC) % p) - 1)) % p;
            return b_modp;
        }

        private static void FinalAccumulatorUpdateAndInnerLoop()
        {
            BigInteger minus_one_modp = p - 1;
            BigInteger one_modp = 1;
            aux_ACC = one_modp;


            for (int j = step; j <= s - 2; j++)
            {
                if (e[j])
                    aux_ACC = BarrettReducer.MultMod(aux_ACC, D_modp[j - step]);
            }

            BigInteger At = BarrettReducer.MultMod(aux_ACC, aux_ACC);
            ACC[0] = BarrettReducer.MultMod(A_modp[0], At);

            for (int h = 1; h <= rem - 1; h++)
                ACC[h] = BarrettReducer.MultMod(ACC[h - 1], ACC[h - 1]);

            for (int j = rem; j >= 3; j--)
            {
                if (ACC[j - 1] == minus_one_modp)
                {
                    aux_ACC = BarrettReducer.MultMod(aux_ACC, D_modp[s - 1 - j]);
                    At = BarrettReducer.MultMod(aux_ACC, aux_ACC);
                    ACC[0] = BarrettReducer.MultMod(A_modp[0], At);

                    for (int h = 1; h <= j - 2; h++)
                        ACC[h] = BarrettReducer.MultMod(ACC[h - 1], ACC[h - 1]);

                    e[s - 2] = true;
                }

                for (int h = 0; h <= s - 3; h++)
                    e[h] = e[h + 1];

                e[s - 2] = false;
                DisplayDigits();
            }

            if (rem == 1)
            {
                if (s == 2)
                    aux_ACC = one_modp;

                else
                {
                    if (!e[s - 3])
                    {
                        aux_ACC = BarrettReducer.MultMod(aux_ACC, D_modp[s - 3]);
                        e[s - 3] = true;
                    }

                    else
                    {
                        At = BarrettReducer.MultMod(aux_ACC, D_modp[s - 3]);
                        BigInteger At2 = BarrettReducer.MultMod(D_modp[s - 2], D_modp[s - 1]);
                        aux_ACC = BarrettReducer.MultMod(At, At2);
                        e[s - 3] = false;
                    }
                }

                DisplayDigits();
            }
            else
            {
                if (ACC[1] == one_modp)
                {
                    aux_ACC = BarrettReducer.MultMod(aux_ACC, D_modp[s - 3]);
                    e[s - 2] = true;
                }


                for (int h = 0; h < s - 2; h++)
                    e[h] = e[h + 1];

                e[s - 2] = false;
                DisplayDigits();
            }
        }

        private static void CompleteAccumulatorUpdate()
        {
            int index = IndexConversion(step - k + 1);
            ACC[0] = A_modp[index];

            for (int j = step; j <= s - 2; j++)
            {
                if (e[j])
                    ACC[0] = BarrettReducer.MultMod(ACC[0], D_modp[j - k + 2]);
            }

            for (int j = 1; j <= k - 1; j++)
                ACC[j] = BarrettReducer.MultMod(ACC[j - 1], ACC[j - 1]);
        }

        private static void CompleteInnerLoop()
        {
            int j, h;
            BigInteger minus_one_modp = p - 1;

            for (j = k; j >= 1; j--)
            {
                if (ACC[j - 1] == minus_one_modp)
                {
                    ACC[0] = BarrettReducer.MultMod(ACC[0], D_modp[s - j]);

                    for (h = 1; h <= j - 2; h++)
                        ACC[h] = BarrettReducer.MultMod(ACC[h - 1], ACC[h - 1]);

                    e[s - 2] = true;
                }

                for (h = 0; h <= s - 3; h++)
                    e[h] = e[h + 1];

                e[s - 2] = false;
                step--;

                DisplayDigits();
            }
        }

        private static int IndexConversion(int index)
        {
            return q + 1 - ((s - 1 - index) / k);
        }

        private static void DisplayDigits()
        {
            int i, val_norm = 0;

            for (i = s - 2; i >= 0; i--)
            {
                if (e[i]) val_norm++;
                val_norm <<= 1;
            }

            val_norm >>= 1;
        }
    }
}
