using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eduard
{
    /// <summary>
    /// Generates prime numbers up to a specified limit using the Sieve of Atkin, <br/>
    /// optimized for cryptographic parameter generation where prime lists are <br/>
    /// frequently needed for field and curve selection.
    /// </summary>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public sealed class Sieve
    {
        /// <summary>
        /// Cached list of primes discovered during sieving.
        /// </summary>
        private List<int> list;

        /// <summary>
        /// Initializes the sieve and generates all primes up to the specified limit.
        /// </summary>
        /// <param name="limit">The exclusive upper bound for prime generation.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when limit is less than 2.</exception>
        public Sieve(int limit)
        {
            if (limit < 2)
                throw new ArgumentOutOfRangeException(nameof(limit),
                    "Limit must be at least 2 to generate primes.");

            GenPrimeList(limit);
        }

#if USE_BENCHMARKING
        public static int[] GenPrimeListStandard(int limit)
        {
            var list = new List<int>();
            int root = (int)Math.Sqrt(limit) + 1;
            bool[] sieve = new bool[limit];

            for (int x = 1; x < root; x++)
            {
                for (int y = 1; y < root; y++)
                {
                    long k = 4L * x * x + y * y;

                    if ((k < limit) && ((k % 12 == 1) || (k % 12 == 5)))
                        sieve[k] = !sieve[k];

                    k = 3L * x * x + y * y;
                    if ((k < limit) && (k % 12 == 7))
                        sieve[k] = !sieve[k];
                    if (x > y)
                    {
                        k = 3L * x * x - y * y;

                        if ((k < limit) && (k % 12 == 11))
                            sieve[k] = !sieve[k];
                    }
                }
            }

            sieve[2] = true;
            sieve[3] = true;

            for (int n = 5; n <= root; n++)
            {
                if (sieve[n])
                {
                    int square = n * n;

                    for (int t = square; t < limit; t += square)
                        sieve[t] = false;
                }
            }

            list.Add(2);

            for (int k = 3; k < limit; k += 2)
            {
                if (sieve[k])
                    list.Add(k);
            }

            return list.ToArray();
        }

        public static int[] GenPrimeListOptimized(int limit)
        {
            var list = new List<int>();
            int root = (int)Math.Sqrt(limit) + 1;
            byte[] sieve = new byte[limit];

            for (int x = 1; x < root; x++)
            {
                long x2 = (long)x * x;
                long x24 = x2 << 2;
                long x23 = x24 - x2;

                for (int y = 1; y < root; y++)
                {
                    long y2 = (long)y * y;
                    long k = x24 + y2;
                    int kmod = (int)(k % 12);

                    if ((k < limit) && (kmod == 1 || kmod == 5))
                        sieve[k] ^= 1;

                    k = x23 + y2;

                    if (k < limit && kmod == 7)
                        sieve[k] ^= 1;

                    if (x > y)
                    {
                        k = x23 - y2;

                        if (k < limit && kmod == 11)
                            sieve[k] ^= 1;
                    }
                }
            }

            sieve[2] = 1;
            sieve[3] = 1;

            for (int n = 5; n <= root; n++)
            {
                if (sieve[n] == 1)
                {
                    int square = n * n;

                    for (int t = square; t < limit; t += square)
                        sieve[t] = 0;
                }
            }

            list.Add(2);

            for (int k = 3; k < limit; k += 2)
            {
                if (sieve[k] == 1)
                    list.Add(k);
            }

            return list.ToArray();
        }
#endif

        private void GenPrimeList(int limit)
        {
            list = new List<int>();
            int root = (int)Math.Sqrt(limit) + 1;
            byte[] sieve = new byte[limit];

            for (int x = 1; x < root; x++)
            {
                long x2 = (long)x * x;
                long x24 = x2 << 2;
                long x23 = x24 - x2;

                for (int y = 1; y < root; y++)
                {
                    long y2 = (long)y * y;
                    long k = x24 + y2;
                    int kmod = (int)(k % 12);

                    if ((k < limit) && (kmod == 1 || kmod == 5))
                        sieve[k] ^= 1;

                    k = x23 + y2;

                    if (k < limit && kmod == 7)
                        sieve[k] ^= 1;

                    if (x > y)
                    {
                        k = x23 - y2;

                        if (k < limit && kmod == 11)
                            sieve[k] ^= 1;
                    }
                }
            }

            sieve[2] = 1;
            sieve[3] = 1;

            for (int n = 5; n <= root; n++)
            {
                if (sieve[n] == 1)
                {
                    int square = n * n;

                    for (int t = square; t < limit; t += square)
                        sieve[t] = 0;
                }
            }

            list.Add(2);

            for (int k = 3; k < limit; k += 2)
            {
                if (sieve[k] == 1)
                    list.Add(k);
            }
        }

        /// <summary>
        /// Retrieves the prime at the specified zero-based index.
        /// </summary>
        /// <param name="index">The zero-based index of the prime to retrieve.</param>
        /// <returns>The prime number at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when index is negative or exceeds the number of primes generated.
        /// </exception>
        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= list.Count)
                    throw new ArgumentOutOfRangeException(nameof(index),
                        "Index must be non-negative and less than the " 
                        + "total prime count.");

                return list[index];
            }
        }

        /// <summary>
        /// The total number of primes generated by the sieve.
        /// </summary>
        public int Count
        { get { return list.Count; } }
    }
}