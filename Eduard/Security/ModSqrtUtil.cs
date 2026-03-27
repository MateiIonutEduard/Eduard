using System;
using Eduard;
using System.Security.Cryptography;

namespace Eduard.Security
{
    internal class ModSqrtUtil
    {
        public static void InitParams(BigInteger field, int windowSize = 4)
        {
            /* check whether optimizations can be used */
            bool enableSpeedup = CanSpeedup(field);

            if (enableSpeedup)
                OptimizedRotaruIftene.Precompute(field, windowSize);
        }

        public static bool CanSpeedup(BigInteger field)
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

        public static BigInteger TonelliShanks(BigInteger val, BigInteger field)
        {
            long e = 0, r, s;
            BigInteger b = 0, bp = 0, q = field - 1, n = 0;
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
                n = SecureRandom.Range(2, field - 1);
                JSymbol = BigInteger.Jacobi(n, field);
            } while (JSymbol != -1);

            z = BigInteger.Pow(n, q, field);
            y = z;
            r = e;

            x = BigInteger.Pow(val, (q - 1) >> 1, field);
            BigInteger sx = BarrettReducer.MulMod(x, x);

            b = BarrettReducer.MulMod(val, sx);
            x = BarrettReducer.MulMod(val, x);

            while (true)
            {
                if (b == 1 || b == field - 1)
                    return x;

                s = 1;

                do
                {
                    bp = BigInteger.Pow(b, (long)Math.Pow(2, s), field);
                    s++;
                } while (bp != 1 && bp != field - 1 && s < r);

                /* has failed */
                if (s == r) return 0;
                t = BigInteger.Pow(y, (long)Math.Pow(2, r - s - 1), field);
                y = BarrettReducer.MulMod(t, t);

                x = BarrettReducer.MulMod(x, t);
                b = BarrettReducer.MulMod(b, y);
                r = s;
            }
        }

        public static BigInteger Sqrt(BigInteger val, BigInteger field)
        {
            if ((field & 3) == 3)
                return BigInteger.Pow(val, (field + 1) >> 2, field);

            BigInteger root = 0;
            BigInteger delta = BarrettReducer.MulMod(field - 4, field - val);

            BigInteger temp = 1;
            BigInteger qnr = 0;

            BigInteger buf = 0;
            BigInteger test = 0;
            int uid = 1;

            switch (uid)
            {
                case 1:

                    root = TonelliShanks(val, field);
                    test = BarrettReducer.MulMod(root, root);

                    if (val == test)
                        return root;

                    goto case 2;

                case 2:

                    qnr = SecureRandom.Range(2, field - 1);

                    if (BigInteger.Jacobi(qnr, field) != -1)
                        goto case 2;

                    BigInteger square = BarrettReducer.MulMod(qnr, qnr);
                    delta = BarrettReducer.MulMod(delta, square);
                    temp = BarrettReducer.MulMod(temp, qnr);

                    buf = TonelliShanks(delta, field);
                    test = BarrettReducer.MulMod(buf, buf);

                    if (delta != test)
                        goto case 2;
                    goto case 3;

                case 3:

                    BigInteger vtemp = BarrettReducer.AddMod(temp, temp);
                    BigInteger inv = BarrettReducer.InvMod(vtemp);
                    root = BarrettReducer.MulMod(buf, inv);
                    break;
            }

            return root;
        }
    }
}
