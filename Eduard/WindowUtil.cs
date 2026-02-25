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
        /// <param name="exp">The exponent being processed.</param>
        /// <param name="index">Current bit position (starting from MSB).</param>
        /// <param name="ubits">Returns the number of bits consumed for the window.</param>
        /// <param name="tbits">Returns the number of trailing zeros detected.</param>
        /// <param name="size">Maximum window size (default 5).</param>
        /// <returns>The odd window value to multiply by, or 0 if current bit is zero.</returns>
        /// <remarks>
        /// Scans the exponent from position i backwards to extract an odd-valued window <br/>
        /// of at most 'size' bits. Returns the window value and the number of bits consumed. <br/>
        /// Trailing zeros are handled separately to optimize squaring operations.
        /// </remarks>
        public static int Window(BigInteger exp, int index, ref int ubits, ref int tbits, int size = 5)
        {
            int j, r, w;
            w = size;

            ubits = 1;
            tbits = 0;

            if (!exp.TestBit(index)) return 0;
            if (index - w + 1 < 0) w = index + 1;

            r = 1;
            for (j = index - 1; j > index - w; j--)
            {
                ubits++;
                r <<= 1;

                if (exp.TestBit(j)) r |= 1;

                if ((r & 0x3) == 0)
                {
                    r >>= 2;
                    ubits -= 2;
                    tbits = 2;
                    break;
                }
            }

            if ((r & 0x1) == 0)
            {
                r >>= 1;
                tbits = 1;
                ubits--;
            }

            return r;
        }

        /// <summary>
        /// Extracts the next window value using fractional sliding window with NAF representation.
        /// </summary>
        /// <param name="exp">The exponent in standard binary representation.</param>
        /// <param name="exp3">Precomputed triple of the exponent (3*exp) for NAF conversion.</param>
        /// <param name="index">Current bit position (starting from MSB).</param>
        /// <param name="ubits">Returns the number of bits consumed for the window.</param>
        /// <param name="tbits">Returns the number of additional trailing zeros detected.</param>
        /// <param name="size">Maximum window size.</param>
        /// <returns>The signed window value (can be negative) for multiplication.</returns>
        /// <remarks>
        /// Implements fractional sliding windows using Non-Adjacent Form (NAF) representation. <br/>
        /// The exp3 parameter allows efficient NAF generation without explicit conversion. <br/>
        /// Returns signed values that reduce the Hamming weight compared to standard binary. <br/>
        /// Used in high-performance elliptic curve point scalar multiplication.
        /// </remarks>
        public static int NAFWindow(BigInteger exp, BigInteger exp3, int index, ref int ubits, ref int tbits, int size)
        {
            int nb, j, r;
            int biggestW;

            nb = exp3.TestBit(index) ? 1 : 0;
            int nbl = exp.TestBit(index) ? 1 : 0;
            nb -= nbl;

            ubits = 1;
            tbits = 0;

            if (nb == 0) return 0;
            if (index == 0) return nb;

            biggestW = (size << 1) - 1;
            r = (nb > 0) ? 1 : -1;

            /* scans the exponent starting from the i-th bit */
            for (j = index - 1; j > 0; j--)
            {
                ubits++;
                r <<= 1;

                int e3b = exp3.TestBit(j) ? 1 : 0;
                int exb = exp.TestBit(j) ? 1 : 0;
                nb = e3b - exb;

                if (nb > 0) r++;
                if (nb < 0) r--;

                int absr = Math.Abs(r);
                if (absr > biggestW) break;
            }

            /* backtrack the last bit */
            if ((r & 1) != 0 && j != 0)
            {
                if (nb > 0) r = (r - 1) >> 1;
                if (nb < 0) r = (r + 1)  >> 1;
                ubits--;
            }

            /* remove the trailing zeros */
            while ((r & 1) == 0)
            {
                r >>= 1;
                tbits++;
                ubits--;
            }

            return r;
        }
    }
}
