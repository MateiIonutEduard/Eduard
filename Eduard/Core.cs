﻿namespace Eduard
{
    public class Core
    {
        static uint[] primes;
        static uint[] inverse;
        static uint[][] roots;
        static uint[][] s1, s2;
        static int logN, count;

        static int degree;
        static uint[][] t;

        static BigInteger[] C;
        static BigInteger N;

        public static BigInteger[] fast_poly_mul(BigInteger[] x, BigInteger[] y, BigInteger field)
        {
            int i, j, newn, logn, np, degree;
            uint inv, p, fac;

            newn = 1; 
            logn = 0;

            int degx = x.Length - 1;
            int degy = y.Length - 1;
            degree = degx + degy;

            while (degree + 1 > newn)
            {
                newn <<= 1;
                logn++;
            }
            
            if (logN < logn)
                np = fft_init(logn, field, field);
            else np = count;

            uint[] wa = new uint[newn];
            
            for (i = 0; i < np; i++)
            {
                p = primes[i];

                for (j = 0; j <= degx; j++)
                    wa[j] = (uint)(x[j] % p);

                for (j = degx + 1; j < newn; j++)
                    wa[j] = 0;

                dif_fft(logn, i, wa);

                for (j = 0; j <= degy; j++)
                    t[i][j] = (uint)(y[j] % p);

                for (j = degy + 1; j < newn; j++)
                    t[i][j] = 0;

                dif_fft(logn, i, t[i]);

                for (j = 0; j < newn; j++)
                    muldiv(wa[j], t[i][j], 0, p, ref t[i][j]);

                dit_fft(logn, i, t[i]);
                inv = inverse[i];

                if (logN > logn)
                {
                    fac = (uint)1 << (logN - logn);
                    inv = smul(fac, inv, p);
                }

                for (j = 0; j <= degree; j++)
                    muldiv(t[i][j], inv, 0, p, ref t[i][j]);
            }

            BigInteger[] res = new BigInteger[degree + 1];

            for (j = 0; j <= degree; j++)
            {
                res[j] = 0;
                BigInteger coeff = 0;

                for(i = 0; i < np; i++)
                {
                    BigInteger val = (t[i][j] * C[i]) % N;
                    coeff += val;
                    if (coeff >= N) coeff -= N;
                }

                res[j] = Util.BarrettReduction(coeff, field); 
            }

            return res;
        }

        public static BigInteger[] fast_poly_sqr(BigInteger[] x, BigInteger field)
        {
            int i, j, newn, logn, np, degree;
            uint inv, p, fac;

            int degx = x.Length - 1;
            degree = degx << 1;
            newn = 1; logn = 0;

            while (degree + 1 > newn)
            {
                newn <<= 1;
                logn++;
            }

            if (logN < logn)
                np = fft_init(logn, field, field);
            else np = count;

            for (i = 0; i < np; i++)
            {
                p = primes[i];

                for (j = 0; j <= degx; j++)
                    t[i][j] = (uint)(x[j] % p);

                for (j = degx + 1; j < newn; j++) t[i][j] = 0;
                dif_fft(logn, i, t[i]);

                for (j = 0; j < newn; j++)
                    muldiv(t[i][j], t[i][j], 0, p, ref t[i][j]);

                dit_fft(logn, i, t[i]);
                inv = inverse[i];

                if (logN > logn)
                {
                    fac = (uint)1 << (logN - logn);
                    inv = smul(fac, inv, p);
                }

                for (j = 0; j <= degree; j++)
                    muldiv(t[i][j], inv, 0, p, ref t[i][j]);
            }

            BigInteger[] res = new BigInteger[degree + 1];

            for (j = 0; j <= degree; j++)
            {
                res[j] = 0;
                BigInteger coeff = 0;

                for (i = 0; i < np; i++)
                {
                    BigInteger val = (t[i][j] * C[i]) % N;
                    coeff += val;
                    if (coeff >= N) coeff -= N;
                }

                res[j] = Util.BarrettReduction(coeff, field);
            }

            return res;
        }

        public static bool fast_poly_rem(BigInteger[] G, BigInteger[] R, BigInteger field)
        {
            int i, j, newn, logn, np, n;
            uint p, inv, fac;

            n = degree;  /* degree of modulus */
            if (n == 0) return false;

            int dg = G.Length - 1;
            np = count;

            newn = 1;
            logn = 0;

            while (2 * n > newn)
            {
                newn <<= 1;
                logn++;
            }

            for (i = 0; i < np; i++)
            {
                p = primes[i];

                for (j = n; j <= dg; j++)
                    t[i][j - n] = (uint)(G[j] % p);

                for (j = dg - n + 1; j < newn; j++) 
                    t[i][j] = 0;

                dif_fft(logn, i, t[i]);

                for (j = 0; j < newn; j++)
                    muldiv(t[i][j], s1[i][j], 0, p, ref t[i][j]);

                dit_fft(logn, i, t[i]);
                inv = inverse[i];

                if (logN > logn)
                {
                    fac = (uint)1 << (logN - logn);
                    inv = smul(fac, inv, p);
                }

                for (j = 0; j < n; j++)
                    muldiv(t[i][j + n - 1], inv, 0, p, ref t[i][j + n - 1]);
            }

            for (j = 0; j < n; j++)
            {
                R[j] = 0;

                for (i = 0; i < np; i++)
                {
                    BigInteger ts = (t[i][j + n - 1] * C[i]) % N;
                    R[j] += ts;
                    if (R[j] >= N) R[j] -= N;
                }

                R[j] = Util.BarrettReduction(R[j], field);
            }

            for (i = 0; i < np; i++)
            {
                p = primes[i];

                for (j = 0; j < n; j++)
                    t[i][j] = (uint)(R[j] % p);

                for (j = n; j < 1 + newn / 2; j++)
                    t[i][j] = 0;

                dif_fft(logn - 1, i, t[i]);

                for (j = 0; j < newn / 2; j++)
                    muldiv(t[i][j], s2[i][j], 0, p, ref t[i][j]);

                dit_fft(logn - 1, i, t[i]);

                inv = inverse[i];

                if (logN > logn - 1)
                {
                    fac = (uint)1 << (logN - logn + 1);
                    inv = smul(fac, inv, p);
                }

                for (j = 0; j < n; j++)
                    muldiv(t[i][j], inv, 0, p, ref t[i][j]);
            }

            modxn_1(newn >> 1, dg, G, field);

            for (j = 0; j < n; j++)
            {
                R[j] = 0;

                for (i = 0; i < np; i++)
                {
                    BigInteger ts = (t[i][j] * C[i]) % N;
                    R[j] += ts;
                    if (R[j] >= N) R[j] -= N;
                }

                BigInteger diff = (G[j] - R[j]) % field;
                if (diff < 0) diff += field;
                R[j] = diff;
            }

            return true;
        }

        public static void polymod_set(int n, BigInteger[] rf, BigInteger[] f, BigInteger field)
        {
            int i, j, np, newn, logn, deg;
            BigInteger[] F;
            uint p;

            deg = 2 * n;
            newn = 1; logn = 0;

            while (deg > newn)
            {
                newn <<= 1;
                logn++;
            }

            if (logN < logn)
                np = fft_init(logn, field, field);
            else np = count;

            degree = n;
            s1 = new uint[np][];
            s2 = new uint[np][];
            F = new BigInteger[n + 1];

            for (i = 0; i <= n; i++)
                F[i] = f[i];

            modxn_1(newn >> 1, n, F, field);

            for (i = 0; i < np; i++)
            {
                s1[i] = new uint[newn];
                s2[i] = new uint[1 + (newn >> 1)];
                p = primes[i];

                for (j = 0; j < n; j++)
                    s1[i][j] = (uint)(rf[j] % p);

                dif_fft(logn, i, s1[i]);

                for (j = 0; j <= n; j++)
                    s2[i][j] = (uint)(F[j] % p);

                dif_fft(logn - 1, i, s2[i]);
            }
        }

        static void modxn_1(int n, int deg, BigInteger[] x, BigInteger field)
        {
            for (int i = 0; n + i <= deg; i++)
            {
                x[i] += x[n + i];
                if (x[i] >= field) x[i] -= field;
                x[n + i] = 0;
            }
        }

        static int fft_init(int logn, BigInteger m1, BigInteger m2)
        {
            uint newn = (uint)1 << logn;
            uint kk = (uint)1 << (31 - logn);
            int i, j;

            uint p = 0;
            BigInteger w5 = m1 * m2;
            int pr = 0;

            while(w5 > 0)
            {
                do
                {
                    kk--;
                    p = kk * newn + 1;
                }
                while (!BigInteger.IsProbablePrime(p));

                w5 /= p;
                pr++;
            }

            if (logn <= logN && count == pr) 
                return pr;

            primes = new uint[pr];
            inverse = new uint[pr];

            t = new uint[pr][];
            roots = new uint[pr][];
            kk = (uint)1 << (31 - logn);

            for(i = 0; i < pr; i++)
            {
                roots[i] = new uint[newn];
                t[i] = new uint[newn];

                do
                {
                    kk--;
                    p = kk * newn + 1;
                }
                while (!BigInteger.IsProbablePrime(p));

                primes[i] = p;
                uint root = p - 1;

                for (j = 1; j < logn; j++)
                    root = sqrmp(root, p);

                roots[i][0] = root;

                for(j = 1; j < newn; j++)
                    roots[i][j] = smul(roots[i][j - 1], root, p);

                inverse[i] = invers(newn, p);
            }

            logN = logn;
            count = pr;

            crt_init();
            return pr;
        }

        static void dif_fft(int logn, int pr, uint[] data)
        {
            int mmax, m, j, k, istep, i, ii, jj, newn, offset;
            uint w, temp, prime;

            prime = primes[pr];
            newn = 1 << logn;

            offset = logN - logn;
            mmax = newn;

            for (k = 0; k < logn; k++)
            {
                istep = mmax;
                mmax >>= 1;
                ii = newn;
                jj = newn / istep;
                ii -= jj;

                for (i = 0; i < newn; i += istep)
                {
                    j = i + mmax;
                    temp = diff(data[i], data[j], prime);
                    data[i] = add(data[i], data[j], prime);
                    data[j] = temp;
                }

                for (m = 1; m < mmax; m++)
                {

                    w = roots[pr][(ii << offset) - 1];
                    ii -= jj;

                    for (i = m; i < newn; i += istep)
                    {
                        j = i + mmax;
                        temp = diff(data[i], data[j], prime);
                        data[i] = add(data[i], data[j], prime);
                        muldiv(w, temp, 0, prime, ref data[j]);
                    }
                }

            }
        }

        static void dit_fft(int logn, int pr, uint[] data)
        {
            int mmax, m, j, k, i, istep, ii, jj, newn, offset;
            uint w, temp = 0, prime;

            prime = primes[pr];
            offset = logN - logn;

            newn = 1 << logn;
            mmax = 1;

            for (k = 0; k < logn; k++)
            {
                istep = mmax << 1;
                ii = 0;

                jj = newn / istep;
                ii += jj;

                for (i = 0; i < newn; i += istep)
                {
                    j = i + mmax;
                    temp = data[j];

                    data[j] = diff(data[i], temp, prime);
                    data[i] = add(data[i], temp, prime);
                }

                for (m = 1; m < mmax; m++)
                {
                    w = roots[pr][(ii << offset) - 1];
                    ii += jj;

                    for (i = m; i < newn; i += istep)
                    {
                        j = i + mmax;
                        muldiv(w, data[j], 0, prime, ref temp);

                        data[j] = diff(data[i], temp, prime);
                        data[i] = add(data[i], temp, prime);
                    }
                }

                mmax = istep;
            }
        }

        static void crt_init()
        {
            N = 1;
            C = new BigInteger[count];

            for (int i = 0; i < count; i++)
                N *= primes[i];

            for(int i = 0; i < count; i++)
            {
                BigInteger rev = N / primes[i];
                BigInteger inv = rev.Inverse(primes[i]);
                C[i] = (rev * inv) % N;
            }
        }

        static uint add(uint a, uint b, uint m)
        {
            long s = (long)a + b;
            if (s >= m) s -= m;
            return (uint)s;
        }

        static uint diff(uint a, uint b, uint m)
        {
            long s = (long)a - b;
            if (s < 0) s += m;
            return (uint)s;
        }

        public static uint sqrmp(uint x, uint m)
        {
            uint z, y, v, w, t, q;
            int i, e, n, r;

            if ((m & 3) == 3)
                return spmd(x, (m + 1) >> 2, m);

            if((m & 7) == 5)
            {
                t = spmd(x, (m - 1) >> 2, m);
                if (t == 1) return spmd(x, (m + 3) >> 3, m);

                if (t == m - 1)
                {
                    muldiv(4, x, 0, m, ref t);
                    t = spmd(t, (m + 3) >> 3, m);
                    muldiv(t, (m + 1) >> 1, 0, m, ref t);
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
                z = spmd((uint)r, q, m);
                if (z == 1) continue;

                t = z;
                pp = false;

                for (i = 1; i < e; i++)
                {
                    if (t == m - 1) pp = true;
                    muldiv(t, t, 0, m, ref t);
                    if (t == 1 && !pp) return 0;
                }

                if (t == m - 1) break;
                if (!pp) return 0;   /* m is not prime */
            }

            y = z;
            r = e;
            v = spmd(x, (q + 1) >> 1, m);
            w = spmd(x, q, m);

            while (w != 1)
            {
                t = w;
                for (n = 0; t != 1; n++) 
                    muldiv(t, t, 0, m, ref t);

                if (n >= r) return 0;
                y = spmd(y, (uint)1 << (r - n - 1), m);
                muldiv(v, y, 0, m, ref v);
                muldiv(y, y, 0, m, ref y);
                muldiv(w, y, 0, m, ref w);
                r = n;
            }

            return v;
        }

        static uint muldiv(uint a, uint b, uint c, uint m, ref uint rp)
        {
            uint q;
            ulong p = (ulong)a * b + c;
            q = (uint)(p / m);
            rp = (uint)(p - (ulong)q * m);
            return q;
        }

        static uint spmd(uint x, uint n, uint m)
        {
            ulong res = 1;
            ulong t = x;

            while(n > 0)
            {
                if ((n & 1) == 1)
                    res = (res * t) % m;

                n >>= 1;
                t = (t * t) % m;
            }

            return (uint)res;
        }

        static uint smul(uint x, uint y, uint n)
        {
            ulong val = (ulong)x * y;
            val %= n;
            return (uint)val;
        }

        static uint invers(uint val, uint field)
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
