namespace Eduard.Security
{
    /// <summary>
    /// Provides optimal features for the use of sliding windows and fractional sliding windows with NAF representation.
    /// </summary>
    public class WindowUtil
    {
        /// <summary>
        /// Returns the value of the sliding window with a maximum size in bits given by the parameter size.
        /// </summary>
        /// <param name="x">Represents the exponent that is partitioned using the sliding window.</param>
        /// <param name="i">Represents the i-th bit where the partitioning of exponent x begins.</param>
        /// <param name="nbs">Represents the number of processed bits.</param>
        /// <param name="nzs">Represents the number of additional trailing zeros detected.</param>
        /// <param name="size">Represents the sliding window maximum size.</param>
        /// <returns></returns>
        public static int Window(BigInteger x, int i, ref int nbs, ref int nzs, int size = 5)
        {
            int j, r, w;
            w = size;

            nbs = 1;
            nzs = 0;

            if (!x.TestBit(i)) return 0;
            if (i - w + 1 < 0) w = i + 1;

            r = 1;
            for (j = i - 1; j > i - w; j--)
            {
                nbs++;
                r *= 2;
                if (x.TestBit(j)) r += 1;

                if ((r & 0x3) == 0)
                {
                    r >>= 2;
                    nbs -= 2;
                    nzs = 2;
                    break;
                }
            }

            if ((r & 0x1) == 0)
            {
                r >>= 1;
                nzs = 1;
                nbs--;
            }

            return r;
        }

        /// <summary>
        /// Returns the sliding window value, using fractional windows.
        /// </summary>
        /// <param name="x">Represents the exponent that is partitioned using the fractional sliding window.</param>
        /// <param name="x3">The parameter x3 represents the triple of the exponent x, that is, x3 = 3*x.</param>
        /// <param name="i">Represents the i-th bit where the partitioning of exponent x begins.</param>
        /// <param name="nbs">Represents the number of processed bits.</param>
        /// <param name="nzs">Represents the number of additional trailing zeros detected.</param>
        /// <param name="size">Represents the fractional sliding window maximum size.</param>
        /// <returns></returns>
        public static int NAFWindow(BigInteger x, BigInteger x3, int i, ref int nbs, ref int nzs, int size)
        {
            int nb, j, r;
            int biggest;

            nb = x3.TestBit(i) ? 1 : 0;
            int nbl = x.TestBit(i) ? 1 : 0;
            nb -= nbl;

            nbs = 1;
            nzs = 0;

            if (nb == 0) return 0;
            if (i == 0) return nb;

            biggest = 2 * size - 1;
            r = (nb > 0) ? 1 : -1;

            /* scans the exponent starting from the i-th bit */
            for (j = i - 1; j > 0; j--)
            {
                nbs++;
                r *= 2;

                int x3b = x3.TestBit(j) ? 1 : 0;
                int xb = x.TestBit(j) ? 1 : 0;
                nb = x3b - xb;

                if (nb > 0) r += 1;
                if (nb < 0) r -= 1;

                int absr = Math.Abs(r);
                if (absr > biggest) break;
            }

            /* backtrack the last bit */
            if (r % 2 != 0 && j != 0)
            {
                if (nb > 0) r = (r - 1) / 2;
                if (nb < 0) r = (r + 1) / 2;
                nbs--;
            }

            /* remove the trailing zeros */
            while (r % 2 == 0)
            {
                r /= 2;
                nzs++;
                nbs--;
            }

            return r;
        }
    }
}
