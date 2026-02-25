using System;

namespace Eduard
{
    /// <summary>
    /// Provides Number Theoretic Transform (NTT) based algorithms for fast polynomial
    /// and integer arithmetic over finite fields.
    /// </summary>
    /// <remarks>
    /// Implements FFT-based multiplication for polynomials and integers using multiple
    /// prime moduli and <br/>Chinese Remainder Theorem (CRT) reconstruction.
    /// Enables O(n log n) complexity for large operands <br/>where classical methods become prohibitive.
    /// </remarks>
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

        static BigInteger[] C;
        static BigInteger N;

        /// <summary>
        /// Multiplies two polynomials using NTT with multiple prime moduli.
        /// </summary>
        /// <param name="x">Coefficients of first polynomial.</param>
        /// <param name="y">Coefficients of second polynomial.</param>
        /// <param name="field">The finite field modulus.</param>
        /// <returns>Product polynomial coefficients.</returns>
        /// <remarks>
        /// Transforms both polynomials to NTT domain, performs point-wise multiplication, <br/>
        /// inverse transform, and CRT reconstruction to recover exact integer coefficients <br/>
        /// modulo the target field. Automatically selects optimal transform size.
        /// </remarks>
        public static BigInteger[] FastPolyMult(BigInteger[] x, BigInteger[] y, BigInteger field)
        {
            int i, j, newn, logn;
            uint inv, p, maxn;
            int pc, degree;

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
                pc = InitFFT(logn, field, field);
            else pc = count;

            uint[] wa = new uint[newn];
            
            for (i = 0; i < pc; i++)
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
                inv = inverse[i];

                if (logN > logn)
                {
                    maxn = (uint)1 << (logN - logn);
                    inv = MulMod(maxn, inv, p);
                }

                for (j = 0; j <= degree; j++)
                    MulAdd(t[i][j], inv, 0, p, ref t[i][j]);
            }

            BigInteger[] res = new BigInteger[degree + 1];

            for (j = 0; j <= degree; j++)
            {
                res[j] = 0;
                BigInteger coeff = 0;

                for(i = 0; i < pc; i++)
                {
                    BigInteger val = (t[i][j] * C[i]) % N;
                    coeff += val;
                    if (coeff >= N) coeff -= N;
                }

                res[j] = BarrettReducer.Reduce(coeff, field); 
            }

            return res;
        }

        /// <summary>
        /// Squares a polynomial using NTT for improved performance.
        /// </summary>
        /// <param name="x">Coefficients of input polynomial.</param>
        /// <param name="field">The finite field modulus.</param>
        /// <returns>Squared polynomial coefficients.</returns>
        /// <remarks>
        /// Optimized version of polynomial multiplication for squaring operations. <br/>
        /// Requires only one forward transform of the input polynomial, reducing <br/>
        /// computational cost by approximately 30% compared to generic multiplication.
        /// </remarks>
        public static BigInteger[] FastPolySquare(BigInteger[] x, BigInteger field)
        {
            int i, j, newn, logn;
            uint inv, p, maxn;
            int pc, degree;

            int degx = x.Length - 1;
            degree = degx << 1;
            newn = 1; logn = 0;

            while (degree + 1 > newn)
            {
                newn <<= 1;
                logn++;
            }

            if (logN < logn)
                pc = InitFFT(logn, field, field);
            else pc = count;

            for (i = 0; i < pc; i++)
            {
                p = primes[i];

                for (j = 0; j <= degx; j++)
                    t[i][j] = (uint)(x[j] % p);

                for (j = degx + 1; j < newn; j++) t[i][j] = 0;
                DFT(logn, i, t[i]);

                for (j = 0; j < newn; j++)
                    MulAdd(t[i][j], t[i][j], 0, p, ref t[i][j]);

                iDFT(logn, i, t[i]);
                inv = inverse[i];

                if (logN > logn)
                {
                    maxn = (uint)1 << (logN - logn);
                    inv = MulMod(maxn, inv, p);
                }

                for (j = 0; j <= degree; j++)
                    MulAdd(t[i][j], inv, 0, p, ref t[i][j]);
            }

            BigInteger[] res = new BigInteger[degree + 1];

            for (j = 0; j <= degree; j++)
            {
                res[j] = 0;
                BigInteger coeff = 0;

                for (i = 0; i < pc; i++)
                {
                    BigInteger val = (t[i][j] * C[i]) % N;
                    coeff += val;
                    if (coeff >= N) coeff -= N;
                }

                res[j] = BarrettReducer.Reduce(coeff, field);
            }

            return res;
        }

        /// <summary>
        /// Computes polynomial remainder using FFT-based division algorithm.
        /// </summary>
        /// <param name="G">Dividend polynomial coefficients (modified in-place).</param>
        /// <param name="R">Output remainder polynomial coefficients.</param>
        /// <param name="field">The finite field modulus.</param>
        /// <returns>true if reduction was performed, false if modulus is zero.</returns>
        /// <remarks>
        /// Implements fast polynomial modulus using precomputed reciprocals. <br/>
        /// Operates in-place on G to minimize memory allocations. Used internally <br/>
        /// by Polynomial.Reduce() for large-degree moduli.
        /// </remarks>
        public static bool FastPolyMod(BigInteger[] G, BigInteger[] R, BigInteger field)
        {
            int i, j, newn, logn;
            uint p, inv, maxn;
            int pc, degn;

            /* degree of modulus polynomial */
            degn = degree;

            if (degn == 0) 
                return false;

            int degG = G.Length - 1;
            pc = count;

            newn = 1;
            logn = 0;

            while (2 * degn > newn)
            {
                newn <<= 1;
                logn++;
            }

            for (i = 0; i < pc; i++)
            {
                p = primes[i];

                for (j = degn; j <= degG; j++)
                    t[i][j - degn] = (uint)(G[j] % p);

                for (j = degG - degn + 1; j < newn; j++) 
                    t[i][j] = 0;

                DFT(logn, i, t[i]);

                for (j = 0; j < newn; j++)
                    MulAdd(t[i][j], s1[i][j], 0, p, ref t[i][j]);

                iDFT(logn, i, t[i]);
                inv = inverse[i];

                if (logN > logn)
                {
                    maxn = (uint)1 << (logN - logn);
                    inv = MulMod(maxn, inv, p);
                }

                for (j = 0; j < degn; j++)
                    MulAdd(t[i][j + degn - 1], inv, 0, p, ref t[i][j + degn - 1]);
            }

            for (j = 0; j < degn; j++)
            {
                R[j] = 0;

                for (i = 0; i < pc; i++)
                {
                    BigInteger ts = (t[i][j + degn - 1] * C[i]) % N;
                    R[j] += ts;
                    if (R[j] >= N) R[j] -= N;
                }

                R[j] = BarrettReducer.Reduce(R[j], field);
            }

            for (i = 0; i < pc; i++)
            {
                p = primes[i];

                for (j = 0; j < degn; j++)
                    t[i][j] = (uint)(R[j] % p);

                for (j = degn; j < 1 + newn / 2; j++)
                    t[i][j] = 0;

                DFT(logn - 1, i, t[i]);

                for (j = 0; j < newn / 2; j++)
                    MulAdd(t[i][j], s2[i][j], 0, p, ref t[i][j]);

                iDFT(logn - 1, i, t[i]);

                inv = inverse[i];

                if (logN > logn - 1)
                {
                    maxn = (uint)1 << (logN - logn + 1);
                    inv = MulMod(maxn, inv, p);
                }

                for (j = 0; j < degn; j++)
                    MulAdd(t[i][j], inv, 0, p, ref t[i][j]);
            }

            Modxn(newn >> 1, degG, G, field);

            for (j = 0; j < degn; j++)
            {
                R[j] = 0;

                for (i = 0; i < pc; i++)
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

        /// <summary>
        /// Precomputes FFT parameters for a given modulus polynomial.
        /// </summary>
        /// <param name="degn">Degree of the modulus polynomial.</param>
        /// <param name="rf">Reciprocal polynomial coefficients.</param>
        /// <param name="f">Modulus polynomial coefficients.</param>
        /// <param name="field">The finite field modulus.</param>
        /// <remarks>
        /// Computes and stores the NTT of both the modulus and its reciprocal <br/>
        /// for efficient repeated modular reductions. Called automatically when <br/>
        /// a new modulus polynomial is encountered in reduction operations.
        /// </remarks>
        public static void SetPolyMod(int degn, BigInteger[] rf, BigInteger[] f, BigInteger field)
        {
            int i, j, pc, newn;
            int logn, deg;

            BigInteger[] F;
            uint p;

            deg = 2 * degn;
            newn = 1; logn = 0;

            while (deg > newn)
            {
                newn <<= 1;
                logn++;
            }

            if (logN < logn)
                pc = InitFFT(logn, field, field);
            else pc = count;

            degree = degn;
            s1 = new uint[pc][];
            s2 = new uint[pc][];
            F = new BigInteger[degn + 1];

            for (i = 0; i <= degn; i++)
                F[i] = f[i];

            Modxn(newn >> 1, degn, F, field);

            for (i = 0; i < pc; i++)
            {
                s1[i] = new uint[newn];
                s2[i] = new uint[1 + (newn >> 1)];
                p = primes[i];

                for (j = 0; j < degn; j++)
                    s1[i][j] = (uint)(rf[j] % p);

                DFT(logn, i, s1[i]);

                for (j = 0; j <= degn; j++)
                    s2[i][j] = (uint)(F[j] % p);

                DFT(logn - 1, i, s2[i]);
            }
        }

        static void Modxn(int degn, int deg, BigInteger[] x, BigInteger field)
        {
            for (int i = 0; degn + i <= deg; i++)
            {
                x[i] += x[degn + i];
                if (x[i] >= field) x[i] -= field;
                x[degn + i] = 0;
            }
        }

        static int InitFFT(int logn, BigInteger m1, BigInteger m2)
        {
            uint newn = (uint)1 << logn;
            uint kmask = (uint)1 << (31 - logn);

            int i, j;
            uint p = 0;

            BigInteger m12 = m1 * m2;
            int pr = 0;

            while(m12 > 0)
            {
                do
                {
                    kmask--;
                    p = kmask * newn + 1;
                }
                while (!BigInteger.IsProbablePrime(p));

                m12 /= p;
                pr++;
            }

            if (logn <= logN && count == pr) 
                return pr;

            primes = new uint[pr];
            inverse = new uint[pr];

            t = new uint[pr][];
            roots = new uint[pr][];
            kmask = (uint)1 << (31 - logn);

            for(i = 0; i < pr; i++)
            {
                roots[i] = new uint[newn];
                t[i] = new uint[newn];

                do
                {
                    kmask--;
                    p = kmask * newn + 1;
                }
                while (!BigInteger.IsProbablePrime(p));

                primes[i] = p;
                uint root = p - 1;

                for (j = 1; j < logn; j++)
                    root = ModSquareRoot(root, p);

                roots[i][0] = root;

                for(j = 1; j < newn; j++)
                    roots[i][j] = MulMod(roots[i][j - 1], root, p);

                inverse[i] = Inverse(newn, p);
            }

            logN = logn;
            count = pr;

            InitCRT();
            return pr;
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

        static uint MultDiv(uint a, uint b, uint c, ref uint rem)
        {
            ulong res = ((ulong)a * b) + c;
            uint mask = 0xFFFFFFFF;

            rem = (uint)(res & mask);
            return (uint)(res >> 32);
        }

        /// <summary>
        /// Multiplies two large integers using FFT and CRT reconstruction.
        /// </summary>
        /// <param name="x">First integer operand.</param>
        /// <param name="y">Second integer operand.</param>
        /// <returns>Product of x and y.</returns>
        /// <remarks>
        /// Implements Schönhage-Strassen style multiplication using three prime moduli <br/>
        /// and Garner's algorithm for CRT reconstruction. Provides O(n log n) complexity <br/>
        /// for integers exceeding Karatsuba threshold. Used internally by BigInteger <br/>
        /// multiplication for large operands.
        /// </remarks>
        /// <exception cref="OutOfMemoryException">Thrown when operands exceed maximum supported size.</exception>
        public static BigInteger FastBigMult(BigInteger x, BigInteger y)
        {
            int i, index, xlen, ylen;
            int zlen, newn, logn;
            uint v1, v2, v3, p;

            uint maxn, inv;
            uint carry1, carry2;
            uint icarry;

            newn = 1; 
            logn = 0;

            xlen = x.data.Used;
            ylen = y.data.Used;
            zlen = xlen + ylen;

            while (zlen > newn)
            {
                newn <<= 1;
                logn++;
            }

            uint[] wptr = new uint[newn];
            uint[] dptr = new uint[newn];

            if (logn > logN)
            {
                if (!InitBigIntFFT(logn))
                    throw new OutOfMemoryException(
                        "Numbers too big for FFT multiplication.");
            }

            for (index = 0; index < 3; index++)
            {
                p = primes[index];
                inv = inverse[index];

                for (i = 0; i < xlen; i++)
                    dptr[i] = x.data[i] % p;

                for (i = xlen; i < newn; i++)
                    dptr[i] = 0;

                DFT(logn, index, dptr);

                if (x != y)
                {
                    for (i = 0; i < ylen; i++)
                        wptr[i] = y.data[i] % p;

                    for (i = ylen; i < newn; i++)
                        wptr[i] = 0;

                    DFT(logn, index, wptr);
                }
                else
                {
                    for (i = 0; i < newn; i++)
                        wptr[i] = dptr[i];
                }

                for (i = 0; i < newn; i++)
                    MulAdd(dptr[i], wptr[i], 0, p, ref dptr[i]);

                iDFT(logn, index, dptr);

                if (logN > logn)
                {
                    maxn = (uint)1 << (logN - logn);
                    inv = MulMod(maxn, inv, p);
                }

                for (i = 0; i < newn; i++)
                {
                    MulAdd(dptr[i], inv, 0, p, ref t[index][i]);
                    long diff = 0;

                    if (index == 1)
                    {
                        diff = (long)t[1][i] - t[0][i];

                        while (diff < 0)
                            diff += primes[1];

                        t[1][i] = (uint)((diff * w1) % primes[1]);
                    }

                    if (index == 2)
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

            uint[] result = new uint[zlen];
            carry1 = carry2 = 0;

            /* propagate the carries */
            for (i = 0; i < zlen; i++)
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

        static void DFT(int logn, int pr, uint[] data)
        {
            int mmax, m, j, k, istep, i;
            int ti, tj, newn, offset;
            uint w, temp, prime;

            prime = primes[pr];
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
                    temp = DiffMod(data[i], data[j], prime);
                    data[i] = AddMod(data[i], data[j], prime);
                    data[j] = temp;
                }

                for (m = 1; m < mmax; m++)
                {

                    w = roots[pr][(ti << offset) - 1];
                    ti -= tj;

                    for (i = m; i < newn; i += istep)
                    {
                        j = i + mmax;
                        temp = DiffMod(data[i], data[j], prime);
                        data[i] = AddMod(data[i], data[j], prime);
                        MulAdd(w, temp, 0, prime, ref data[j]);
                    }
                }

            }
        }

        static void iDFT(int logn, int pr, uint[] data)
        {
            int mmax, m, j, k, i, istep;
            int ti, tj, newn, offset;
            uint w, temp = 0, prime;

            prime = primes[pr];
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

                    data[j] = DiffMod(data[i], temp, prime);
                    data[i] = AddMod(data[i], temp, prime);
                }

                for (m = 1; m < mmax; m++)
                {
                    w = roots[pr][(ti << offset) - 1];
                    ti += tj;

                    for (i = m; i < newn; i += istep)
                    {
                        j = i + mmax;
                        MulAdd(w, data[j], 0, prime, ref temp);

                        data[j] = DiffMod(data[i], temp, prime);
                        data[i] = AddMod(data[i], temp, prime);
                    }
                }

                mmax = istep;
            }
        }

        static void InitCRT()
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

        static uint ModSquareRoot(uint x, uint m)
        {
            uint z, y, v, w, t, q;
            int i, e, n, r;

            if ((m & 3) == 3)
                return pow(x, (m + 1) >> 2, m);

            if((m & 7) == 5)
            {
                t = pow(x, (m - 1) >> 2, m);
                if (t == 1) return pow(x, (m + 3) >> 3, m);

                if (t == m - 1)
                {
                    MulAdd(4, x, 0, m, ref t);
                    t = pow(t, (m + 3) >> 3, m);
                    MulAdd(t, (m + 1) >> 1, 0, m, ref t);
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
                z = pow((uint)r, q, m);
                if (z == 1) continue;

                t = z;
                pp = false;

                for (i = 1; i < e; i++)
                {
                    if (t == m - 1) pp = true;
                    MulAdd(t, t, 0, m, ref t);
                    if (t == 1 && !pp) return 0;
                }

                if (t == m - 1) break;
                if (!pp) return 0;   /* m is not prime */
            }

            y = z;
            r = e;
            v = pow(x, (q + 1) >> 1, m);
            w = pow(x, q, m);

            while (w != 1)
            {
                t = w;
                for (n = 0; t != 1; n++) 
                    MulAdd(t, t, 0, m, ref t);

                if (n >= r) return 0;
                y = pow(y, (uint)1 << (r - n - 1), m);
                MulAdd(v, y, 0, m, ref v);
                MulAdd(y, y, 0, m, ref y);
                MulAdd(w, y, 0, m, ref w);
                r = n;
            }

            return v;
        }

        static uint MulAdd(uint a, uint b, uint c, uint m, ref uint rem)
        {
            uint q;
            ulong p = (ulong)a * b + c;
            q = (uint)(p / m);
            rem = (uint)(p - (ulong)q * m);
            return q;
        }

        static uint pow(uint x, uint n, uint m)
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
