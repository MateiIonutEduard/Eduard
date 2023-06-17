using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eduard
{
    /// <summary>
    /// Represents the sieve of Atkin.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class Sieve
    {
        private List<int> list;

        /// <summary>
        /// Creates new sieve using a specified limit.
        /// </summary>
        /// <param name="limit"></param>
        public Sieve(int limit)
        {
            list = new List<int>();
            int root = (int)Math.Sqrt(limit) + 1;
            bool[] sieve = new bool[limit];

            for (int x = 1; x < root; x++)
            {
                for (int y = 1; y < root; y++)
                {
                    int k = 4 * x * x + y * y;

                    if ((k < limit) && ((k % 12 == 1) || (k % 12 == 5)))
                        sieve[k] = !sieve[k];

                    k = 3 * x * x + y * y;
                    if ((k < limit) && (k % 12 == 7))
                        sieve[k] = !sieve[k];
                    if (x > y)
                    {
                        k = 3 * x * x - y * y;

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
        }

        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= list.Count)
                    throw new IndexOutOfRangeException("Index out of range.");
                else
                    return list[index];
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the sieve.
        /// </summary>
        public int Count
        { get { return list.Count; } }
    }
}
