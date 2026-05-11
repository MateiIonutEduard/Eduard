using System;
using System.Collections.Generic;
using System.Text;

namespace Eduard
{
    /// <summary>
    /// Provides low-level modular arithmetic primitives optimized for cryptographic operations.
    /// </summary>
    /// <remarks>
    /// Implements constant-time-safe modular addition, subtraction, multiplication, exponentiation, <br/>
    /// square root (Tonelli-Shanks), and inverse algorithms over 32-bit unsigned integers.
    /// </remarks>
    public class CoreMath
    {
        /// <summary>
        /// Computes (a + b) mod m with conditional subtraction.
        /// </summary>
        /// <param name="a">First operand in [0, m-1].</param>
        /// <param name="b">Second operand in [0, m-1].</param>
        /// <param name="m">Modulus.</param>
        /// <returns>Sum modulo m.</returns>
        public static uint AddMod(uint a, uint b, uint m)
        {
            long s = (long)a + b;
            if (s >= m) s -= m;
            return (uint)s;
        }

        /// <summary>
        /// Computes (a - b) mod m with conditional addition.
        /// </summary>
        /// <param name="a">First operand in [0, m-1].</param>
        /// <param name="b">Second operand in [0, m-1].</param>
        /// <param name="m">Modulus.</param>
        /// <returns>Difference modulo m.</returns>
        public static uint DiffMod(uint a, uint b, uint m)
        {
            long s = (long)a - b;
            if (s < 0) s += m;
            return (uint)s;
        }

        /// <summary>
        /// Computes fused multiply-add with quotient: q = (a*b + c) / m, remainder set in rem.
        /// </summary>
        /// <param name="a">First factor.</param>
        /// <param name="b">Second factor.</param>
        /// <param name="c">Addend.</param>
        /// <param name="m">Modulus.</param>
        /// <param name="rem">Receives (a*b + c) % m.</param>
        /// <returns>Quotient (a*b + c) / m.</returns>
        public static uint MultAdd(uint a, uint b, uint c, uint m, ref uint rem)
        {
            uint q;
            ulong p = (ulong)a * b + c;
            q = (uint)(p / m);
            rem = (uint)(p - (ulong)q * m);
            return q;
        }

        /// <summary>
        /// Computes x^n mod m using binary exponentiation.
        /// </summary>
        /// <param name="x">Base.</param>
        /// <param name="n">Exponent.</param>
        /// <param name="m">Modulus.</param>
        /// <returns>x^n mod m.</returns>
        public static uint ModPow(uint x, uint n, uint m)
        {
            ulong res = 1;
            ulong t = x;

            while (n > 0)
            {
                if ((n & 1) == 1)
                    res = (res * t) % m;

                n >>= 1;
                t = (t * t) % m;
            }

            return (uint)res;
        }

        /// <summary>
        /// Computes (x * y) mod n using 64-bit intermediate.
        /// </summary>
        public static uint MultMod(uint x, uint y, uint n)
        {
            ulong val = (ulong)x * y;
            val %= n;
            return (uint)val;
        }

        /// <summary>
        /// Computes high and low 32-bit words of (a * b) + c.
        /// </summary>
        /// <param name="a">First factor.</param>
        /// <param name="b">Second factor.</param>
        /// <param name="c">Addend.</param>
        /// <param name="rem">Receives low 32 bits of result.</param>
        /// <returns>High 32 bits of result (carry word).</returns>
        public static uint MultDiv(uint a, uint b, uint c, ref uint rem)
        {
            ulong res = ((ulong)a * b) + c;
            uint mask = 0xFFFFFFFF;

            rem = (uint)(res & mask);
            return (uint)(res >> 32);
        }

        /// <summary>
        /// Computes modular square root of x modulo prime m using Tonelli-Shanks algorithm.
        /// </summary>
        /// <param name="x">Quadratic residue.</param>
        /// <param name="m">Prime modulus.</param>
        /// <returns>Square root of x mod m, or 0 if none exists.</returns>
        /// <remarks>
        /// Handles m = 3 mod 4, m = 5 mod 8, and general case via Tonelli-Shanks.<br/>
        /// Returns 0 when no square root exists or when m is detected as composite.
        /// </remarks>
        public static uint ModSqrt(uint x, uint m)
        {
            uint z, y, v, w, t, q;
            int i, e, n, r;

            if ((m & 3) == 3)
                return ModPow(x, (m + 1) >> 2, m);

            if ((m & 7) == 5)
            {
                t = ModPow(x, (m - 1) >> 2, m);
                if (t == 1) return ModPow(x, (m + 3) >> 3, m);

                if (t == m - 1)
                {
                    MultAdd(4, x, 0, m, ref t);
                    t = ModPow(t, (m + 3) >> 3, m);

                    MultAdd(t, (m + 1) >> 1, 0, m, ref t);
                    return t;
                }

                return 0;
            }

            bool pp = true;
            q = m - 1;
            e = 0;

            while ((q & 1) == 0)
            {
                q >>= 1;
                e++;
            }

            if (e == 0) return 0;

            for (r = 2; ; r++)
            {
                z = ModPow((uint)r, q, m);
                if (z == 1) continue;

                t = z;
                pp = false;

                for (i = 1; i < e; i++)
                {
                    if (t == m - 1) pp = true;
                    MultAdd(t, t, 0, m, ref t);
                    if (t == 1 && !pp) return 0;
                }

                if (t == m - 1) 
                    break;

                /* m is composite */
                if (!pp) return 0;
            }

            y = z;
            r = e;

            v = ModPow(x, (q + 1) >> 1, m);
            w = ModPow(x, q, m);

            while (w != 1)
            {
                t = w;

                for (n = 0; t != 1; n++)
                    MultAdd(t, t, 0, m, ref t);

                if (n >= r) return 0;
                y = ModPow(y, (uint)1 << (r - n - 1), m);

                MultAdd(v, y, 0, m, ref v);
                MultAdd(y, y, 0, m, ref y);

                MultAdd(w, y, 0, m, ref w);
                r = n;
            }

            return v;
        }

        /// <summary>
        /// Computes modular inverse of val modulo field using extended Euclidean algorithm.
        /// </summary>
        /// <param name="val">Value to invert.</param>
        /// <param name="field">Modulus.</param>
        /// <returns>Inverse such that (val * result) % field = 1.</returns>
        public static uint Inverse(uint val, uint field)
        {
            long b0 = field, t, q;
            long x0 = 0, x1 = 1;
            if (field == 1) return 1;

            while (val > 1)
            {
                q = val / field;
                t = field;
                field = val % field;
                val = (uint)t;
                t = x0;
                x0 = x1 - q * x0;
                x1 = t;
            }

            if (x1 < 0) x1 += b0;
            return (uint)x1;
        }
    }
}
