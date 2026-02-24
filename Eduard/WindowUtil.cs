using System;
using System.Diagnostics;

namespace Eduard
{
    /// <summary>
    /// Implements sliding window algorithms for efficient exponentiation.
    /// </summary>
    /// <remarks>
    /// Provides both standard sliding window and fractional sliding window (NAF-based) <br/>
    /// methods for processing exponents in modular exponentiation. These algorithms <br/>
    /// reduce the number of multiplications by processing multiple bits at once.
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public class WindowUtil
    {
        /// <summary>
        /// Extracts the next window value from an exponent using standard sliding window.
        /// </summary>
        /// <param name="x">The exponent being processed.</param>
        /// <param name="i">Current bit position (starting from MSB).</param>
        /// <param name="nbs">Returns the number of bits consumed for the window.</param>
        /// <param name="nzs">Returns the number of trailing zeros detected.</param>
        /// <param name="size">Maximum window size (default 5).</param>
        /// <returns>The odd window value to multiply by, or 0 if current bit is zero.</returns>
        /// <remarks>
        /// Scans the exponent from position i backwards to extract an odd-valued window <br/>
        /// of at most 'size' bits. Returns the window value and the number of bits consumed. <br/>
        /// Trailing zeros are handled separately to optimize squaring operations.
        /// </remarks>
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
                r <<= 1;

                if (x.TestBit(j)) r |= 1;

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
        /// Extracts the next window value using fractional sliding window with NAF representation.
        /// </summary>
        /// <param name="x">The exponent in standard binary representation.</param>
        /// <param name="x3">Precomputed triple of the exponent (3*x) for NAF conversion.</param>
        /// <param name="i">Current bit position (starting from MSB).</param>
        /// <param name="nbs">Returns the number of bits consumed for the window.</param>
        /// <param name="nzs">Returns the number of additional trailing zeros detected.</param>
        /// <param name="size">Maximum window size.</param>
        /// <returns>The signed window value (can be negative) for multiplication.</returns>
        /// <remarks>
        /// Implements fractional sliding windows using Non-Adjacent Form (NAF) representation. <br/>
        /// The x3 parameter allows efficient NAF generation without explicit conversion. <br/>
        /// Returns signed values that reduce the Hamming weight compared to standard binary. <br/>
        /// Used in high-performance elliptic curve point scalar multiplication.
        /// </remarks>
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

            biggest = (size << 1) - 1;
            r = (nb > 0) ? 1 : -1;

            /* scans the exponent starting from the i-th bit */
            for (j = i - 1; j > 0; j--)
            {
                nbs++;
                r <<= 1;

                int x3b = x3.TestBit(j) ? 1 : 0;
                int xb = x.TestBit(j) ? 1 : 0;
                nb = x3b - xb;

                if (nb > 0) r++;
                if (nb < 0) r--;

                int absr = Math.Abs(r);
                if (absr > biggest) break;
            }

            /* backtrack the last bit */
            if ((r & 1) != 0 && j != 0)
            {
                if (nb > 0) r = (r - 1) >> 1;
                if (nb < 0) r = (r + 1)  >> 1;
                nbs--;
            }

            /* remove the trailing zeros */
            while ((r & 1) == 0)
            {
                r >>= 1;
                nzs++;
                nbs--;
            }

            return r;
        }
    }
}
