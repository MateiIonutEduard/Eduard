using System.Security.Cryptography;

namespace Eduard.Cryptography
{
    /* complete generalization of Atkin's modular square root algorithm */
    internal class OptimizedRotaruIftene
    {
        static bool[] e;
        static int s, k, q;
        static int step, rem;

        static BigInteger p, t, a, b, norm;
        static BigInteger a_modp, b_modp, alpha;

        static BigInteger beta, d_prec, A_prec;
        static BigInteger aux_A, aux_ACC;

        static BigInteger[] D_modp, A_modp, ACC;

        public static void Precompute(RandomNumberGenerator rand, BigInteger field, int windowSize)
        {
            p = field; s = 0;
            BigInteger order = field - 1;

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
                aux_d = BigInteger.Next(rand, 2, p - 2);
                jSymbol = BigInteger.Jacobi(aux_d, field);
            }
            while (jSymbol != -1);

            d_prec = aux_d;
            D_modp = new BigInteger[s];
            D_modp[0] = BigInteger.Pow(d_prec, t, p);

            for (int i = 1; i <= s - 1; i++)
                D_modp[i] = (D_modp[i - 1] * D_modp[i - 1]) % field;
        }

        public static BigInteger Sqrt(BigInteger val)
        {
            a_modp = val;
            step = s - 2;

            BigInteger aux = 0;
            int index_check, i, j;

            aux_A = BigInteger.Pow(2 * a_modp, (t - 1) >> 1, p);
            A_prec = (2 * a_modp * ((aux_A * aux_A) % p)) % p;
            aux = A_prec;

            A_modp = new BigInteger[q + 1];
            A_modp[0] = A_prec;
            j = 1;

            for (i = 1; i <= s - 2; i++)
            {
                aux = (aux * aux) % p;
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

        static void FinalAccumulatorUpdateAndInnerLoop()
        {
            BigInteger minus_one_modp = p - 1;
            BigInteger one_modp = 1;
            aux_ACC = one_modp;


            for (int j = step; j <= s - 2; j++)
            {
                if (e[j])
                    aux_ACC = (aux_ACC * D_modp[j - step]) % p;
            }

            ACC[0] = (A_modp[0] * ((aux_ACC * aux_ACC) % p)) % p;

            for (int h = 1; h <= rem - 1; h++)
                ACC[h] = (ACC[h - 1] * ACC[h - 1]) % p;

            for (int j = rem; j >= 3; j--)
            {
                if (ACC[j - 1] == minus_one_modp)
                {
                    aux_ACC = (aux_ACC * D_modp[s - 1 - j]) % p;
                    ACC[0] = (A_modp[0] * ((aux_ACC * aux_ACC) % p)) % p;

                    for (int h = 1; h <= j - 2; h++)
                        ACC[h] = (ACC[h - 1] * ACC[h - 1]) % p;

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
                        aux_ACC = (aux_ACC * D_modp[s - 3]) % p;
                        e[s - 3] = true;
                    }

                    else
                    {
                        aux_ACC = (aux_ACC * D_modp[s - 3] * D_modp[s - 2] * D_modp[s - 1]) % p;
                        e[s - 3] = false;
                    }
                }

                DisplayDigits();
            }
            else
            {
                if (ACC[1] == one_modp)
                {
                    aux_ACC = (aux_ACC * D_modp[s - 3]) % p;
                    e[s - 2] = true;
                }


                for (int h = 0; h < s - 2; h++)
                    e[h] = e[h + 1];

                e[s - 2] = false;
                DisplayDigits();
            }
        }

        static void CompleteAccumulatorUpdate()
        {
            int index = IndexConversion(step - k + 1);
            ACC[0] = A_modp[index];

            for (int j = step; j <= s - 2; j++)
            {
                if (e[j])
                    ACC[0] = (ACC[0] * D_modp[j - k + 2]) % p;
            }

            for (int j = 1; j <= k - 1; j++)
                ACC[j] = (ACC[j - 1] * ACC[j - 1]) % p;
        }

        static void CompleteInnerLoop()
        {
            int j, h;
            BigInteger minus_one_modp = p - 1;

            for (j = k; j >= 1; j--)
            {
                if (ACC[j - 1] == minus_one_modp)
                {
                    ACC[0] = (ACC[0] * D_modp[s - j]) % p;

                    for (h = 1; h <= j - 2; h++)
                        ACC[h] = (ACC[h - 1] * ACC[h - 1]) % p;

                    e[s - 2] = true;
                }

                for (h = 0; h <= s - 3; h++)
                    e[h] = e[h + 1];

                e[s - 2] = false;
                step--;

                DisplayDigits();
            }
        }

        static int IndexConversion(int index)
        {
            return q + 1 - ((s - 1 - index) / k);
        }

        static void DisplayDigits()
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
