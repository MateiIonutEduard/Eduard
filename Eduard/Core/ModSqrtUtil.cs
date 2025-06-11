using System.Security.Cryptography;
#pragma warning disable

namespace Eduard.Security
{
    internal class ModSqrtUtil
    {
        static RandomNumberGenerator rand;

        public static void InitParams(BigInteger field, int windowSize = 4)
        {
            /* check whether optimizations can be used */
            bool enableSpeedup = CanSpeedup(field);
            rand = RandomNumberGenerator.Create();

            if (enableSpeedup)
                OptimizedRotaruIftene.Precompute(rand, field, windowSize);
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
                n = BigInteger.Next(rand, 2, field - 1);
                JSymbol = BigInteger.Jacobi(n, field);
            } while (JSymbol != -1);

            z = BigInteger.Pow(n, q, field);
            y = z;
            r = e;

            x = BigInteger.Pow(val, (q - 1) / 2, field);
            b = (((val * x) % field) * x) % field;
            x = (val * x) % field;

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
                y = (t * t) % field;

                x = (x * t) % field;
                b = (b * y) % field;
                r = s;
            }
        }

        public static BigInteger Sqrt(BigInteger val, BigInteger field)
        {
            if ((field & 3) == 3)
                return BigInteger.Pow(val, (field + 1) >> 2, field);

            BigInteger root = 0;
            BigInteger delta = ((field - 4) * (field - val)) % field;

            BigInteger temp = 1;
            BigInteger qnr = 0;

            BigInteger buf = 0;
            int uid = 1;

            switch (uid)
            {
                case 1:

                    root = TonelliShanks(val, field);

                    if (val == (root * root) % field)
                        return root;

                    goto case 2;

                case 2:

                    qnr = BigInteger.Next(rand, 2, field - 1);

                    if (BigInteger.Jacobi(qnr, field) != -1)
                        goto case 2;

                    BigInteger square = (qnr * qnr) % field;
                    delta = (delta * square) % field;

                    temp = (temp * qnr) % field;
                    buf = TonelliShanks(delta, field);

                    if (delta != (buf * buf) % field)
                        goto case 2;
                    goto case 3;

                case 3:

                    BigInteger vtemp = (2 * temp) % field;
                    BigInteger inv = vtemp.Inverse(field);

                    root = (buf * inv) % field;
                    break;
            }

            return root;
        }
    }
}
