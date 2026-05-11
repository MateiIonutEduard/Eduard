using Eduard;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eduard.Tests.Core
{
    [Collection("Sequential")]
    public class SieveTests
    {
        #region Constructor and Basic Properties

        [Fact]
        public void Constructor_LimitTwo_ProducesSinglePrime()
        {
            var sieve = new Sieve(2);
            Assert.Equal(1, sieve.Count);
            Assert.Equal(2, sieve[0]);
        }

        [Fact]
        public void Constructor_LimitThree_ProducesTwoPrimes()
        {
            var sieve = new Sieve(3);
            Assert.Equal(2, sieve.Count);
            Assert.Equal(2, sieve[0]);
            Assert.Equal(3, sieve[1]);
        }

        [Fact]
        public void Constructor_LimitTen_ProducesFourPrimes()
        {
            var sieve = new Sieve(10);
            Assert.Equal(4, sieve.Count);
            Assert.Equal(2, sieve[0]);

            Assert.Equal(3, sieve[1]);
            Assert.Equal(5, sieve[2]);
            Assert.Equal(7, sieve[3]);
        }

        [Fact]
        public void Constructor_LimitLessThanTwo_ThrowsArgumentOutOfRange()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Sieve(1));
            Assert.Contains("at least 2", ex.Message);
        }

        [Fact]
        public void Constructor_LimitNegative_ThrowsArgumentOutOfRange()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Sieve(-5));
            Assert.Contains("at least 2", ex.Message);
        }

        [Fact]
        public void Constructor_LimitExactlyTwo_CountIsOne()
        {
            var sieve = new Sieve(2);
            Assert.Equal(1, sieve.Count);
        }

        #endregion

        #region Primes Content Verification

        [Fact]
        public void Primes_LimitThirty_ContainsCorrectSequence()
        {
            var sieve = new Sieve(30);
            int[] expected = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };
            Assert.Equal(expected.Length, sieve.Count);
            for (int i = 0; i < expected.Length; i++)
                Assert.Equal(expected[i], sieve[i]);
        }

        [Fact]
        public void Primes_LimitOneHundred_CountIsTwentyFive()
        {
            var sieve = new Sieve(100);
            Assert.Equal(25, sieve.Count);
        }

        [Fact]
        public void Primes_LimitOneThousand_CountIs168()
        {
            var sieve = new Sieve(1000);
            Assert.Equal(168, sieve.Count);
        }

        [Fact]
        public void Primes_LargeLimits_PrimeCountsMatchKnownValues()
        {
            int[] limits = new int[] { 
                10000, 100000, 1000000, 
                10000000, 100000000
            };

            int[] count = new int[] {
                1229, 9592, 78498,
                664579, 5761455
            };

            for(int i = 0; i < limits.Length; i++)
            {
                var sieve = new Sieve(limits[i]);
                Assert.Equal(count[i], sieve.Count);
            }
        }

        [Fact]
        public void Primes_AllReturnedValuesArePrime()
        {
            var sieve = new Sieve(10000);

            for (int i = 0; i < sieve.Count; i++)
            {
                int p = sieve[i];
                Assert.True(IsPrime(p), $"Sieve returned {p}, which is not prime");
            }
        }

        [Fact]
        public void Primes_AllValuesWithinLimit()
        {
            int limit = 500;
            var sieve = new Sieve(limit);
            for (int i = 0; i < sieve.Count; i++)
            {
                int p = sieve[i];
                Assert.True(p >= 2, $"Prime {p} is less than 2");
                Assert.True(p < limit, $"Prime {p} exceeds limit {limit}");
            }
        }

        [Fact]
        public void Primes_NoPrimeMissedUpToOneHundred()
        {
            var sieve = new Sieve(101);
            var found = new HashSet<int>();

            for (int i = 0; i < sieve.Count; i++)
                found.Add(sieve[i]);

            int[] allPrimesUnder101 = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29,
                31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };

            foreach (int p in allPrimesUnder101)
                Assert.True(found.Contains(p), $"Missing prime {p}");
        }

        #endregion

        #region Indexer Tests

        [Fact]
        public void Indexer_FirstPrime_ReturnsTwo()
        {
            var sieve = new Sieve(101);
            Assert.Equal(2, sieve[0]);
        }

        [Fact]
        public void Indexer_LastPrime_LessThanLimit()
        {
            var sieve = new Sieve(1000);
            int last = sieve[sieve.Count - 1];
            Assert.True(last < 1000);
        }

        [Fact]
        public void Indexer_NegativeIndex_ThrowsArgumentOutOfRange()
        {
            var sieve = new Sieve(50);
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sieve[-1]);
            Assert.Contains("Index", ex.Message);
        }

        [Fact]
        public void Indexer_IndexEqualToCount_ThrowsArgumentOutOfRange()
        {
            var sieve = new Sieve(50);
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => 
                sieve[sieve.Count]);
            Assert.Contains("Index", ex.Message);
        }

        [Fact]
        public void Indexer_IndexExceedsCount_ThrowsArgumentOutOfRange()
        {
            var sieve = new Sieve(50);
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => 
                sieve[sieve.Count + 100]);
            Assert.Contains("Index", ex.Message);
        }

        #endregion

        #region Cryptographic Parameter Generation Scenarios

        [Fact]
        public void Sieve_P256Modulus_Near250BitLimits()
        {
            var sieve = new Sieve(100000);
            Assert.True(sieve.Count > 0);
            Assert.True(sieve.Count > 1000);
            Assert.Equal(7919, sieve[999]);
        }

        [Fact]
        public void Sieve_CountProperty_AccurateAfterConstruction()
        {
            var sieve = new Sieve(200);
            int count = sieve.Count;
            int actual = 0;
            for (int i = 2; i < 200; i++)
                if (IsPrime(i)) actual++;
            Assert.Equal(actual, count);
        }

        [Fact]
        public void Sieve_LargeLimit_NotExceedingIntBounds()
        {
            var sieve = new Sieve(10000);
            Assert.True(sieve.Count > 0);
            Assert.True(sieve[sieve.Count - 1] < 10000);
        }

        [Fact]
        public void Sieve_FirstFewPrimesMatchKnownSequence()
        {
            var sieve = new Sieve(50);
            int[] firstSix = { 2, 3, 5, 7, 11, 13 };

            for (int i = 0; i < 6; i++)
                Assert.Equal(firstSix[i], sieve[i]);
        }

        #endregion

        #region Helper

        private static bool IsPrime(int n)
        {
            if (n < 2) return false;
            if (n == 2) return true;

            if (n % 2 == 0) return false;
            int limit = (int)Math.Sqrt(n) + 1;

            for (int i = 3; i <= limit; i += 2)
                if (n % i == 0) return false;
            return true;
        }

        #endregion
    }
}
