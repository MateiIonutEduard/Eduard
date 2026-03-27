using System.Security.Cryptography;

namespace Eduard
{
    /// <summary>
    /// Provides cryptographically secure random number generation for cryptographic operations.
    /// </summary>
    public static class SecureRandom
    {
        static readonly RandomNumberGenerator rand;
        static readonly object locker = new object();

        static SecureRandom()
        {
            rand = RandomNumberGenerator.Create();
        }

        /// <summary>
        /// Generates a strong probable prime of the specified bit length.
        /// </summary>
        /// <param name="bits">The bit length of the prime.</param>
        /// <param name="trials">The number of Miller-Rabin primality tests. Default is 50.</param>
        /// <returns>A probable prime number.</returns>
        public static BigInteger GenProbablePrime(int bits, int trials = 50)
        {
            lock (locker)
            {
                BigInteger prime = BigInteger.GenProbablePrime(rand, bits, trials);
                return prime;
            }
        }

        /// <summary>
        /// Generates a cryptographically secure random integer within the specified range [min, max].
        /// </summary>
        /// <param name="min">The inclusive lower bound.</param>
        /// <param name="max">The inclusive upper bound.</param>
        /// <returns>A random integer between min and max inclusive.</returns>
        public static BigInteger Range(BigInteger min, BigInteger max)
        {
            lock (locker)
            {
                BigInteger val = BigInteger.Next(rand, min, max);
                return val;
            }
        }

        /// <summary>
        /// Generates a cryptographically secure random integer with the specified bit length.
        /// </summary>
        /// <param name="bits">The exact bit length of the random integer.</param>
        /// <returns>A random integer of exactly <paramref name="n"/> bits.</returns>
        public static BigInteger GenRandom(int n)
        {
            lock (locker)
            {
                BigInteger val = new BigInteger(n, rand);
                return val;
            }
        }

        /// <summary>
        /// Generates a cryptographically secure random byte array of the specified length.
        /// </summary>
        /// <param name="count">The number of random bytes to generate.</param>
        /// <returns>A byte array filled with cryptographically strong random values.</returns>
        public static byte[] GetBytes(int count)
        {
            var bytes = new byte[count];

            lock (locker)
            {
                rand.GetBytes(bytes);
                return bytes;
            }
        }
    }
}