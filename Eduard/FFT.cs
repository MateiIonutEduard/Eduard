using System;

namespace Eduard
{
    public class FFT
    {
        static uint[] primes;
        static uint[] inverse;
        static uint[][] roots;

        static uint[][] s1, s2;
        static int logN, count;

        static uint w1, w2;
        static uint w3, msw, lsw;

        static int degree;
        static uint[][] t;

        static BigInteger[] Cres;
        static BigInteger cmod;

        public static BigInteger[] FastPolyMult(BigInteger[] x, BigInteger[] y, BigInteger field)
        {
            int i, j, newn, logn;
            int primesCount, degree;
            uint cinv, p, npt;

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
                primesCount = InitFFT(logn, field, field);
            else primesCount = count;

            uint[] wa = new uint[newn];
            
            for (i = 0; i < primesCount; i++)
            {
                p = primes[i];

                for (j = 0; j <= degx; j++)
                    wa[j] = (uint)(x[j] % p);

                for (j = degx + 1; j < newn; j++)
                    wa[j] = 0;

                DFT(logn, i, wa);

                for (j = 0; j <= degy; j++)
                    t[i][j] = (uint)(y[j] % p);

                for (j = degy + 1; j < newn; j++)
                    t[i][j] = 0;

                DFT(logn, i, t[i]);

                for (j = 0; j < newn; j++)
                    MulAdd(wa[j], t[i][j], 0, p, ref t[i][j]);

                iDFT(logn, i, t[i]);
                cinv = inverse[i];

                if (logN > logn)
                {
                    npt = (uint)1 << (logN - logn);
                    cinv = MulMod(npt, cinv, p);
                }

                for (j = 0; j <= degree; j++)
                    MulAdd(t[i][j], cinv, 0, p, ref t[i][j]);
            }

            BigInteger[] res = new BigInteger[degree + 1];

            for (j = 0; j <= degree; j++)
            {
                res[j] = 0;
                BigInteger coeff = 0;

                for(i = 0; i < primesCount; i++)
                {
                    BigInteger val = (t[i][j] * Cres[i]) % cmod;
                    coeff += val;
                    if (coeff >= cmod) coeff -= cmod;
                }

                res[j] = Util.BarrettReduction(coeff, field); 
            }

            return res;
        }

        public static BigInteger[] FastPolySquare(BigInteger[] x, BigInteger field)
        {
            int i, j, newn, logn;
            int primesCount, degree;
            uint cinv, p, npt;

            int degxPoly = x.Length - 1;
            degree = degxPoly << 1;
            newn = 1; logn = 0;

            while (degree + 1 > newn)
            {
                newn <<= 1;
                logn++;
            }

            if (logN < logn)
                primesCount = InitFFT(logn, field, field);
            else primesCount = count;

            for (i = 0; i < primesCount; i++)
            {
                p = primes[i];

                for (j = 0; j <= degxPoly; j++)
                    t[i][j] = (uint)(x[j] % p);

                for (j = degxPoly + 1; j < newn; j++) t[i][j] = 0;
                DFT(logn, i, t[i]);

                for (j = 0; j < newn; j++)
                    MulAdd(t[i][j], t[i][j], 0, p, ref t[i][j]);

                iDFT(logn, i, t[i]);
                cinv = inverse[i];

                if (logN > logn)
                {
                    npt = (uint)1 << (logN - logn);
                    cinv = MulMod(npt, cinv, p);
                }

                for (j = 0; j <= degree; j++)
                    MulAdd(t[i][j], cinv, 0, p, ref t[i][j]);
            }

            BigInteger[] res = new BigInteger[degree + 1];

            for (j = 0; j <= degree; j++)
            {
                res[j] = 0;
                BigInteger coeff = 0;

                for (i = 0; i < primesCount; i++)
                {
                    BigInteger val = (t[i][j] * Cres[i]) % cmod;
                    coeff += val;
                    if (coeff >= cmod) coeff -= cmod;
                }

                res[j] = Util.BarrettReduction(coeff, field);
            }

            return res;
        }

        public static bool FastPolyMod(BigInteger[] G, BigInteger[] R, BigInteger field)
        {
            int i, j, newn, logn;
            uint q, cinv, npt;
            int primesCount, degn;

            /* degree of modulus polynomial */
            degn = degree;

            if (degn == 0) 
                return false;

            int degG = G.Length - 1;
            primesCount = count;

            newn = 1;
            logn = 0;

            while (2 * degn > newn)
            {
                newn <<= 1;
                logn++;
            }

            for (i = 0; i < primesCount; i++)
            {
                q = primes[i];

                for (j = degn; j <= degG; j++)
                    t[i][j - degn] = (uint)(G[j] % q);

                for (j = degG - degn + 1; j < newn; j++) 
                    t[i][j] = 0;

                DFT(logn, i, t[i]);

                for (j = 0; j < newn; j++)
                    MulAdd(t[i][j], s1[i][j], 0, q, ref t[i][j]);

                iDFT(logn, i, t[i]);
                cinv = inverse[i];

                if (logN > logn)
                {
                    npt = (uint)1 << (logN - logn);
                    cinv = MulMod(npt, cinv, q);
                }

                for (j = 0; j < degn; j++)
                    MulAdd(t[i][j + degn - 1], cinv, 0, q, ref t[i][j + degn - 1]);
            }

            for (j = 0; j < degn; j++)
            {
                R[j] = 0;

                for (i = 0; i < primesCount; i++)
                {
                    BigInteger ts = (t[i][j + degn - 1] * Cres[i]) % cmod;
                    R[j] += ts;
                    if (R[j] >= cmod) R[j] -= cmod;
                }

                R[j] = Util.BarrettReduction(R[j], field);
            }

            for (i = 0; i < primesCount; i++)
            {
                q = primes[i];

                for (j = 0; j < degn; j++)
                    t[i][j] = (uint)(R[j] % q);

                for (j = degn; j < 1 + newn / 2; j++)
                    t[i][j] = 0;

                DFT(logn - 1, i, t[i]);

                for (j = 0; j < newn / 2; j++)
                    MulAdd(t[i][j], s2[i][j], 0, q, ref t[i][j]);

                iDFT(logn - 1, i, t[i]);

                cinv = inverse[i];

                if (logN > logn - 1)
                {
                    npt = (uint)1 << (logN - logn + 1);
                    cinv = MulMod(npt, cinv, q);
                }

                for (j = 0; j < degn; j++)
                    MulAdd(t[i][j], cinv, 0, q, ref t[i][j]);
            }

            Modxn(newn >> 1, degG, G, field);

            for (j = 0; j < degn; j++)
            {
                R[j] = 0;

                for (i = 0; i < primesCount; i++)
                {
                    BigInteger ts = (t[i][j] * Cres[i]) % cmod;
                    R[j] += ts;
                    if (R[j] >= cmod) R[j] -= cmod;
                }

                BigInteger diff = (G[j] - R[j]) % field;
                if (diff < 0) diff += field;
                R[j] = diff;
            }

            return true;
        }

        public static void SetPolyMod(int degn, BigInteger[] rf, BigInteger[] f, BigInteger field)
        {
            int i, j, newn, logn;
            int primesCount, maxDegree;
            BigInteger[] F;
            uint q;

            maxDegree = 2 * degn;
            newn = 1; logn = 0;

            while (maxDegree > newn)
            {
                newn <<= 1;
                logn++;
            }

            if (logN < logn)
                primesCount = InitFFT(logn, field, field);
            else primesCount = count;

            degree = degn;
            s1 = new uint[primesCount][];
            s2 = new uint[primesCount][];
            F = new BigInteger[degn + 1];

            for (i = 0; i <= degn; i++)
                F[i] = f[i];

            Modxn(newn >> 1, degn, F, field);

            for (i = 0; i < primesCount; i++)
            {
                s1[i] = new uint[newn];
                s2[i] = new uint[1 + (newn >> 1)];
                q = primes[i];

                for (j = 0; j < degn; j++)
                    s1[i][j] = (uint)(rf[j] % q);

                DFT(logn, i, s1[i]);

                for (j = 0; j <= degn; j++)
                    s2[i][j] = (uint)(F[j] % q);

                DFT(logn - 1, i, s2[i]);
            }
        }

        static void Modxn(int degn, int maxDeg, BigInteger[] x, BigInteger field)
        {
            for (int i = 0; degn + i <= maxDeg; i++)
            {
                x[i] += x[degn + i];

                if (x[i] >= field) 
                    x[i] -= field;

                x[degn + i] = 0;
            }
        }

        static int InitFFT(int logn, BigInteger m1, BigInteger m2)
        {
            uint newn = (uint)1 << logn;
            uint kmask = (uint)1 << (31 - logn);
            int i, j;

            uint qprime = 0;
            BigInteger temp = m1 * m2;
            int primes = 0;

            while(temp > 0)
            {
                do
                {
                    kmask--;
                    qprime = kmask * newn + 1;
                }
                while (!BigInteger.IsProbablePrime(qprime));

                temp /= qprime;
                primes++;
            }

            if (logn <= logN && count == primes) 
                return primes;

            FFT.primes = new uint[primes];
            inverse = new uint[primes];

            t = new uint[primes][];
            roots = new uint[primes][];
            kmask = (uint)1 << (31 - logn);

            for(i = 0; i < primes; i++)
            {
                roots[i] = new uint[newn];
                t[i] = new uint[newn];

                do
                {
                    kmask--;
                    qprime = kmask * newn + 1;
                }
                while (!BigInteger.IsProbablePrime(qprime));

                FFT.primes[i] = qprime;
                uint root = qprime - 1;

                for (j = 1; j < logn; j++)
                    root = ModSquareRoot(root, qprime);

                roots[i][0] = root;

                for(j = 1; j < newn; j++)
                    roots[i][j] = MulMod(roots[i][j - 1], root, qprime);

                inverse[i] = Inverse(newn, qprime);
            }

            logN = logn;
            count = primes;

            InitCRT();
            return primes;
        }

        static bool InitBigIntFFT(int logn)
        {
            BigInteger maxc = (BigInteger)1 << 32;

            if (InitFFT(logn, maxc, maxc) != 3)
                return false;

            w1 = Inverse(primes[0], primes[1]);
            w2 = Inverse(primes[0], primes[2]);
            w3 = Inverse(primes[1], primes[2]);

            ulong tw = (ulong)primes[0] * (ulong)primes[1];
            lsw = (uint)(tw & 0xFFFFFFFF);

            msw = (uint)(tw >> 32);
            return true;
        }

        static uint MultDiv(uint a, uint b, uint c, ref uint rp)
        {
            ulong res = ((ulong)a * b) + c;
            uint mask = 0xFFFFFFFF;

            rp = (uint)(res & mask);
            return (uint)(res >> 32);
        }

        public static BigInteger FastBigMult(BigInteger x, BigInteger y)
        {
            int i, primeIndex, xwLen, ywLen;
            int zwLen, newn, logn;
            uint v1, v2, v3, qprime;

            uint npt, inv;
            uint carry1, carry2, icarry;

            uint[] w = new uint[3];
            newn = 1; logn = 0;

            xwLen = x.data.Used;
            ywLen = y.data.Used;
            zwLen = xwLen + ywLen;

            while (zwLen > newn)
            {
                newn <<= 1;
                logn++;
            }

            uint[] fptr = new uint[newn];
            uint[] sptr = new uint[newn];

            if (logn > logN)
            {
                if (!InitBigIntFFT(logn))
                    throw new OutOfMemoryException(
                        "Numbers too big for FFT multiplication.");
            }

            for (primeIndex = 0; primeIndex < 3; primeIndex++)
            {
                qprime = primes[primeIndex];
                inv = inverse[primeIndex];

                for (i = 0; i < xwLen; i++)
                    sptr[i] = x.data[i] % qprime;

                for (i = xwLen; i < newn; i++)
                    sptr[i] = 0;

                DFT(logn, primeIndex, sptr);

                if (x != y)
                {
                    for (i = 0; i < ywLen; i++)
                        fptr[i] = y.data[i] % qprime;

                    for (i = ywLen; i < newn; i++)
                        fptr[i] = 0;

                    DFT(logn, primeIndex, fptr);
                }
                else
                {
                    for (i = 0; i < newn; i++)
                        fptr[i] = sptr[i];
                }

                for (i = 0; i < newn; i++)
                    MulAdd(sptr[i], fptr[i], 0, qprime, ref sptr[i]);

                iDFT(logn, primeIndex, sptr);

                if (logN > logn)
                {
                    npt = (uint)1 << (logN - logn);
                    inv = MulMod(npt, inv, qprime);
                }

                for (i = 0; i < newn; i++)
                {
                    MulAdd(sptr[i], inv, 0, qprime, ref t[primeIndex][i]);
                    long diff = 0;

                    if (primeIndex == 1)
                    {
                        diff = (long)t[1][i] - t[0][i];

                        while (diff < 0)
                            diff += primes[1];

                        t[1][i] = (uint)((diff * w1) % primes[1]);
                    }

                    if (primeIndex == 2)
                    {
                        diff = (long)t[2][i] - t[0][i];

                        while (diff < 0)
                            diff += primes[2];

                        diff = (uint)((diff * w2) % primes[2]);
                        diff -= t[1][i];

                        while (diff < 0)
                            diff += primes[2];

                        t[2][i] = (uint)((diff * w3) % primes[2]);
                    }
                }
            }

            uint[] result = new uint[zwLen];
            carry1 = carry2 = 0;

            /* propagate the carries */
            for (i = 0; i < zwLen; i++)
            {
                v1 = t[0][i];
                v2 = t[1][i];
                v3 = t[2][i];

                v2 = MultDiv(v2, primes[0], v1, ref v1);
                carry1 += v1;

                if (carry1 < v1)
                    v2++;

                icarry = carry2 + MultDiv(lsw, v3, (uint)carry1, ref result[i]);
                uint temp_c = (uint)carry1;

                carry2 = MultDiv(msw, v3, (uint)icarry, ref temp_c);
                carry1 = temp_c;
                carry1 += v2;

                if (carry1 < v2)
                    carry2++;
            }

            bool sign = x.data.IsNegative
                && y.data.IsNegative;

            Data data = new Data(result);
            BigInteger res = new BigInteger(data);
            return sign ? -res : res;
        }

        static void DFT(int logn, int primeIndex, uint[] data)
        {
            int mmax, m, j, k, istep;
            int i, ti, tj, newn, offset;
            uint w, temp, qprime;

            qprime = primes[primeIndex];
            newn = 1 << logn;

            offset = logN - logn;
            mmax = newn;

            for (k = 0; k < logn; k++)
            {
                istep = mmax;
                mmax >>= 1;
                ti = newn;
                tj = newn / istep;
                ti -= tj;

                for (i = 0; i < newn; i += istep)
                {
                    j = i + mmax;
                    temp = DiffMod(data[i], data[j], qprime);
                    data[i] = AddMod(data[i], data[j], qprime);
                    data[j] = temp;
                }

                for (m = 1; m < mmax; m++)
                {

                    w = roots[primeIndex][(ti << offset) - 1];
                    ti -= tj;

                    for (i = m; i < newn; i += istep)
                    {
                        j = i + mmax;
                        temp = DiffMod(data[i], data[j], qprime);
                        data[i] = AddMod(data[i], data[j], qprime);
                        MulAdd(w, temp, 0, qprime, ref data[j]);
                    }
                }

            }
        }

        static void iDFT(int logn, int primeIndex, uint[] data)
        {
            int mmax, m, j, k, i, istep;
            int ti, tj, newn, offset;
            uint w, temp = 0, qprime;

            qprime = primes[primeIndex];
            offset = logN - logn;

            newn = 1 << logn;
            mmax = 1;

            for (k = 0; k < logn; k++)
            {
                istep = mmax << 1;
                ti = 0;

                tj = newn / istep;
                ti += tj;

                for (i = 0; i < newn; i += istep)
                {
                    j = i + mmax;
                    temp = data[j];

                    data[j] = DiffMod(data[i], temp, qprime);
                    data[i] = AddMod(data[i], temp, qprime);
                }

                for (m = 1; m < mmax; m++)
                {
                    w = roots[primeIndex][(ti << offset) - 1];
                    ti += tj;

                    for (i = m; i < newn; i += istep)
                    {
                        j = i + mmax;
                        MulAdd(w, data[j], 0, qprime, ref temp);

                        data[j] = DiffMod(data[i], temp, qprime);
                        data[i] = AddMod(data[i], temp, qprime);
                    }
                }

                mmax = istep;
            }
        }

        static void InitCRT()
        {
            cmod = 1;
            Cres = new BigInteger[count];

            for (int i = 0; i < count; i++)
                cmod *= primes[i];

            for(int i = 0; i < count; i++)
            {
                BigInteger rev = cmod / primes[i];
                BigInteger inv = rev.Inverse(primes[i]);
                Cres[i] = (rev * inv) % cmod;
            }
        }

        static uint AddMod(uint a, uint b, uint m)
        {
            long s = (long)a + b;
            if (s >= m) s -= m;
            return (uint)s;
        }

        static uint DiffMod(uint a, uint b, uint m)
        {
            long s = (long)a - b;
            if (s < 0) s += m;
            return (uint)s;
        }

        public static uint ModSquareRoot(uint val, uint mod)
        {
            uint z, y, v, w, t, q;
            int i, e, n, r;

            if ((mod & 3) == 3)
                return pow(val, (mod + 1) >> 2, mod);

            if((mod & 7) == 5)
            {
                t = pow(val, (mod - 1) >> 2, mod);
                if (t == 1) return pow(val, (mod + 3) >> 3, mod);

                if (t == mod - 1)
                {
                    MulAdd(4, val, 0, mod, ref t);
                    t = pow(t, (mod + 3) >> 3, mod);
                    MulAdd(t, (mod + 1) >> 1, 0, mod, ref t);
                    return t;
                }

                return 0;
            }

            bool pp = true;
            q = mod - 1;
            e = 0;

            while ((q & 1) == 0)
            {
                q >>= 1;
                e++;
            }

            if (e == 0) return 0;

            for (r = 2; ; r++)
            {
                z = pow((uint)r, q, mod);
                if (z == 1) continue;

                t = z;
                pp = false;

                for (i = 1; i < e; i++)
                {
                    if (t == mod - 1) pp = true;
                    MulAdd(t, t, 0, mod, ref t);
                    if (t == 1 && !pp) return 0;
                }

                if (t == mod - 1) break;
                if (!pp) return 0;   /* m is not prime */
            }

            y = z;
            r = e;
            v = pow(val, (q + 1) >> 1, mod);
            w = pow(val, q, mod);

            while (w != 1)
            {
                t = w;
                for (n = 0; t != 1; n++) 
                    MulAdd(t, t, 0, mod, ref t);

                if (n >= r) return 0;
                y = pow(y, (uint)1 << (r - n - 1), mod);
                MulAdd(v, y, 0, mod, ref v);
                MulAdd(y, y, 0, mod, ref y);
                MulAdd(w, y, 0, mod, ref w);
                r = n;
            }

            return v;
        }

        static uint MulAdd(uint x, uint y, uint z, uint m, ref uint resm)
        {
            uint q;
            ulong p = (ulong)x * y + z;
            q = (uint)(p / m);
            resm = (uint)(p - (ulong)q * m);
            return q;
        }

        static uint pow(uint a, uint e, uint m)
        {
            ulong res = 1;
            ulong t = a;

            while(e > 0)
            {
                if ((e & 1) == 1)
                    res = (res * t) % m;

                e >>= 1;
                t = (t * t) % m;
            }

            return (uint)res;
        }

        static uint MulMod(uint x, uint y, uint n)
        {
            ulong val = (ulong)x * y;
            val %= n;
            return (uint)val;
        }

        static uint Inverse(uint val, uint field)
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
