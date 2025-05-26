using System.Security.Cryptography;
#pragma warning disable

namespace Eduard.Security
{
    /* The Tonelli-Shanks-Bernstein algorithm for computing the modular square root. */
    internal class TonelliShanksBernstein
    {
        static BigInteger p, t, a, b;
        static int s, d, l, n, w;
        static int[] digits;

        static BigInteger[] p1_table, p2_table;
        static BigInteger[][] n_table;

        static BigInteger a_modp, b_modp, r_modp;
        static BigInteger v_modp, g_modp, e_modp, g_to_e_modp;

        public static void Precompute(RandomNumberGenerator rand, BigInteger field, int windowSize)
        {
            p = field; w = windowSize;
            BigInteger order = field - 1;
            s = 0;

            while ((order & 1) == 0)
            {
                order >>= 1;
                s++;
            }

            t = order;
            n = s;

            l = n / w - 1;
            digits = new int[l + 1];

            /* find quadratic non-residue modulo p */
            r_modp = 0;
            int jSymbol = 0;

            do
            {
                r_modp = BigInteger.Next(rand, 2, field - 2);
                jSymbol = BigInteger.Jacobi(r_modp, field);
            } while (jSymbol != -1);

            int totalSize = 1 << w;
            p1_table = new BigInteger[totalSize];

            p2_table = new BigInteger[l + 1];
            n_table = new BigInteger[l + 1][];
            g_modp = BigInteger.Pow(r_modp, t, p);

            /* compute the hash tables */
            BigInteger aux = g_modp;
            p1_table[0] = 1;

            for (int i = 1; i <= l * w; i++)
                aux = (aux * aux) % p;

            for (int i = 1; i < totalSize; i++)
                p1_table[i] = (p1_table[i - 1] * aux) % p;

            aux = g_modp.Inverse(p);

            for (int i = 0; i <= l; i++)
            {
                n_table[i] = new BigInteger[totalSize];
                n_table[i][0] = 1;

                for (int j = 1; j < totalSize; j++)
                    n_table[i][j] = (n_table[i][j - 1] * aux) % p;

                aux = (n_table[i][totalSize - 1] * aux) % p;
            }
        }

        public static BigInteger Sqrt(BigInteger val)
        {
            int i, j;
            BigInteger aux_modp, aux2_modp;

            a_modp = val % p;
            v_modp = BigInteger.Pow(a_modp, (t - 1) >> 1, p);

            g_to_e_modp = (a_modp * v_modp * v_modp) % p;
            BigInteger aux = g_to_e_modp;
            p2_table[0] = aux;

            for (i = 1; i <= l; i++)
            {
                for (j = 1; j <= w; j++)
                    aux = (aux * aux) % p;

                p2_table[i] = aux;
            }

            BigInteger aux_digits = 0;
            BigInteger aux_mask = ((BigInteger)1 << w) - 1;

            for (i = 0; i <= l; i++)
            {
                aux_modp = p2_table[l - i];

                for (j = 0; j < i; j++)
                    aux_modp = (aux_modp * n_table[l - i + j][digits[j]]) % p;

                digits[i] = FindExponent(aux_modp);
            }

            for (i = l; i >= 0; i--)
                aux_digits = (aux_digits << w) + digits[i];

            aux_digits >>= 1;
            i = 0;

            while (aux_digits != 0)
            {
                digits[i] = (int)(aux_digits & aux_mask);
                aux_digits >>= w;
                i++;
            }

            aux_modp = 1;

            for (i = 0; i <= l; i++)
                aux_modp = (aux_modp * n_table[i][digits[i]]) % p;

            aux_modp = (aux_modp * v_modp * a_modp) % p;
            return aux_modp;
        }

        static int FindExponent(BigInteger val)
        {
            int i = 0;
            int totalSize = 1 << w;

            for (i = 0; i < totalSize; i++)
            {
                if (p1_table[i] == val)
                    return i;
            }

            return 0;
        }
    }
}
