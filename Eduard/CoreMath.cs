using System;
using System.Collections.Generic;
using System.Text;

namespace Eduard
{
    internal class CoreMath
    {
        internal static uint AddMod(uint a, uint b, uint m)
        {
            long s = (long)a + b;
            if (s >= m) s -= m;
            return (uint)s;
        }

        internal static uint DiffMod(uint a, uint b, uint m)
        {
            long s = (long)a - b;
            if (s < 0) s += m;
            return (uint)s;
        }

        internal static uint MultAdd(uint a, uint b, uint c, uint m, ref uint rem)
        {
            uint q;
            ulong p = (ulong)a * b + c;
            q = (uint)(p / m);
            rem = (uint)(p - (ulong)q * m);
            return q;
        }

        internal static uint ModPow(uint x, uint n, uint m)
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

        internal static uint MultMod(uint x, uint y, uint n)
        {
            ulong val = (ulong)x * y;
            val %= n;
            return (uint)val;
        }

        internal static uint MultDiv(uint a, uint b, uint c, ref uint rem)
        {
            ulong res = ((ulong)a * b) + c;
            uint mask = 0xFFFFFFFF;

            rem = (uint)(res & mask);
            return (uint)(res >> 32);
        }

        internal static uint ModSquareRoot(uint x, uint m)
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

        internal static uint Inverse(uint val, uint field)
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
