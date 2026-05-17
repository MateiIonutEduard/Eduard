using System;
using System.Collections.Generic;

namespace Eduard.Tests.Core
{
    public class WindowTests
    {
        #region Sliding Window Tests

        [Fact]
        public void Window_LeadingBitZero_ReturnsZero()
        {
            BigInteger exp = 8;
            int ubits = 0, tbits = 0;

            int result = WindowUtil.Window(exp, index: 2, 
                ref ubits, ref tbits, size: 5);

            Assert.Equal(0, result);
            Assert.Equal(1, ubits);
            Assert.Equal(0, tbits);
        }

        [Fact]
        public void Window_SingleBitExponent_ReturnsOne()
        {
            BigInteger exp = 1;
            int ubits = 0; 
            int tbits = 0;

            int result = WindowUtil.Window(exp, index: 0, 
                ref ubits, ref tbits, size: 5);

            Assert.Equal(1, result);
            Assert.Equal(1, ubits);
            Assert.Equal(0, tbits);
        }

        [Fact]
        public void Window_MultiBitWindow_ReturnsCorrectOddValue()
        {
            BigInteger exp = 13;
            int ubits = 0;
            int tbits = 0;

            int result = WindowUtil.Window(exp, index: 3, 
                ref ubits, ref tbits, size: 5);

            Assert.Equal(13, result);
            Assert.Equal(4, ubits);
            Assert.Equal(0, tbits);
        }

        [Fact]
        public void Window_TrailingZerosPattern_AdjustsWindowCorrectly()
        {
            BigInteger exp = 8;
            int ubits = 0; 
            int tbits = 0;

            int result = WindowUtil.Window(exp, index: 3, 
                ref ubits, ref tbits, size: 5);

            Assert.Equal(1, result);
            Assert.Equal(1, ubits);
            Assert.Equal(2, tbits);
        }

        [Fact]
        public void Window_SingleTrailingZero_AdjustedByFinalCleanup()
        {
            BigInteger exp = 2;
            int ubits = 0; 
            int tbits = 0;

            int result = WindowUtil.Window(exp, index: 1, 
                ref ubits, ref tbits, size: 5);

            Assert.Equal(1, result);
            Assert.Equal(1, ubits);
            Assert.Equal(1, tbits);
        }

        [Fact]
        public void Window_ShortenedWindow_DueToExponentLength()
        {
            BigInteger exp = 5;
            int ubits = 0; 
            int tbits = 0;

            int result = WindowUtil.Window(exp, index: 2, 
                ref ubits, ref tbits, size: 5);

            Assert.Equal(5, result);
            Assert.Equal(3, ubits);
            Assert.Equal(0, tbits);
        }

        [Fact]
        public void Window_SizeOne_ConsumesOnlyCurrentBit()
        {
            BigInteger exp = 13;
            int ubits = 0; 
            int tbits = 0;

            int result = WindowUtil.Window(exp, index: 3, 
                ref ubits, ref tbits, size: 1);

            Assert.Equal(1, result);
            Assert.Equal(1, ubits);
            Assert.Equal(0, tbits);
        }

        [Fact]
        public void Window_AllBitsSet_ReturnsMaximumOddValue()
        {
            BigInteger exp = 63;
            int ubits = 0, tbits = 0;

            int result = WindowUtil.Window(exp, index: 5, 
                ref ubits, ref tbits, size: 5);

            Assert.Equal(31, result);
            Assert.Equal(5, ubits);
            Assert.Equal(0, tbits);
        }

        #endregion

        #region Sliding Window Strong Tests

        [Fact]
        public void Window_SequentialExtraction_ConsumesAllBits()
        {
            var sizes = new[] { 256, 
                384, 521, 448 };

            foreach (int bitLength in sizes)
            {
                var exp = SecureRandom.GenRandom(bitLength);
                int bits = exp.GetBits();

                int i = bits - 1;
                int totalBitsConsumed = 0;

                while (i >= 0)
                {
                    int ubits = 0;
                    int tbits = 0;

                    int result = WindowUtil.Window(exp, 
                        i, ref ubits, ref tbits, 5);

                    if (exp.TestBit(i))
                    {
                        Assert.NotEqual(0, result);
                        Assert.True((result & 1) != 0);
                        Assert.True(ubits >= 1);
                    }
                    else
                    {
                        Assert.Equal(0, result);
                        Assert.Equal(1, ubits);
                        Assert.Equal(0, tbits);
                    }

                    Assert.True(tbits >= 0);
                    totalBitsConsumed += ubits;
                    i -= ubits;

                    if (tbits > 0)
                    {
                        totalBitsConsumed += tbits;
                        i -= tbits;
                    }
                }

                Assert.Equal(bits, totalBitsConsumed);
            }
        }

        [Fact]
        public void Window_CryptographicPrimes_ValidWindows()
        {
            var primes = new Dictionary<string, BigInteger>();

            primes["P-256"] = BigInteger.Pow(2, 256) - BigInteger.Pow(2, 224) +
                BigInteger.Pow(2, 192) + BigInteger.Pow(2, 96) - 1;
            primes["Ed25519"] = BigInteger.Pow(2, 255) - 19;

            primes["P-384"] = BigInteger.Pow(2, 384) - BigInteger.Pow(2, 128) -
                BigInteger.Pow(2, 96) + BigInteger.Pow(2, 32) - 1;
            primes["P-521"] = BigInteger.Pow(2, 521) - 1;

            primes["Ed448"] = BigInteger.Pow(2, 448) - 
                BigInteger.Pow(2, 224) - 1;

            foreach (var kvp in primes)
            {
                var p = kvp.Value;
                int bits = p.GetBits();

                for (int i = bits - 1; i >= 0; i--)
                {
                    int ubits = 0;
                    int tbits = 0;

                    int result = WindowUtil.Window(p, 
                        i, ref ubits, ref tbits, 5);

                    if (p.TestBit(i))
                    {
                        Assert.NotEqual(0, result);
                        Assert.True((result & 1) != 0);
                        Assert.True(result >= 1);

                        Assert.True(result <= 31);
                        Assert.True(ubits >= 1);
                    }
                    else
                    {
                        Assert.Equal(0, result);
                        Assert.Equal(1, ubits);
                        Assert.Equal(0, tbits);
                    }

                    Assert.True(tbits >= 0);
                }
            }
        }

        [Fact]
        public void Window_MaximumWindowSize_Boundary()
        {
            BigInteger exp = 31;
            int ubits = 0;
            int tbits = 0;

            int result = WindowUtil.Window(exp,
                4, ref ubits, ref tbits, 5);

            Assert.Equal(31, result);
            Assert.Equal(5, ubits);
            Assert.Equal(0, tbits);

            exp = 63;
            ubits = 0;
            tbits = 0;

            result = WindowUtil.Window(exp,
                5, ref ubits, ref tbits, 5);

            Assert.Equal(31, result);
            Assert.Equal(5, ubits);
            Assert.Equal(0, tbits);

            exp = 24;
            ubits = 0;
            tbits = 0;

            result = WindowUtil.Window(exp,
                4, ref ubits, ref tbits, 5);

            Assert.Equal(3, result);
            Assert.Equal(2, ubits);
            Assert.Equal(2, tbits);
        }

        [Fact]
        public void Window_RandomExponents_ResultOddAndInRange()
        {
            var sizes = new[] { 128, 
                192, 256, 320 };

            foreach (int bitLength in sizes)
            {
                var exp = SecureRandom.GenRandom(bitLength);
                int bits = exp.GetBits();

                for (int i = bits - 1; i >= 0; i -= 32)
                {
                    int ubits = 0;
                    int tbits = 0;

                    int result = WindowUtil.Window(exp, 
                        i, ref ubits, ref tbits, 5);

                    if (exp.TestBit(i))
                    {
                        Assert.NotEqual(0, result);
                        Assert.True((result & 1) != 0);

                        Assert.True(result >= 1);
                        Assert.True(result <= 31);

                        Assert.True(ubits >= 1);
                        Assert.True(ubits <= i + 1);
                    }
                    else
                    {
                        Assert.Equal(0, result);
                        Assert.Equal(1, ubits);
                        Assert.Equal(0, tbits);
                    }

                    Assert.True(tbits >= 0);
                }
            }
        }

        #endregion

        #region NAF Fractional Sliding Window Tests

        [Fact]
        public void NAFWindow_LeadingBitZero_ReturnsZero()
        {
            BigInteger exp = 4;
            var exp3 = exp * 3;

            int ubits = 0;
            int tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3, 
                index: 2, ref ubits, ref tbits, size: 4);

            Assert.Equal(0, result);
            Assert.Equal(1, ubits);
            Assert.Equal(0, tbits);
        }

        [Fact]
        public void NAFWindow_NonZeroDigitAtMSB_ReturnsSignedWindow()
        {
            BigInteger exp = 3;
            var exp3 = exp * 3;

            int ubits = 0;
            int tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3, 
                index: 1, ref ubits, ref tbits, size: 4);

            Assert.Equal(-1, result);
            Assert.Equal(1, ubits);
            Assert.Equal(0, tbits);
        }

        [Fact]
        public void NAFWindow_IndexZeroAlwaysReturnsZero()
        {
            BigInteger exp = 7;
            var exp3 = exp * 3;

            int ubits = 0;
            int tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3, 
                index: 0, ref ubits, ref tbits, size: 4);

            Assert.Equal(0, result);
            Assert.Equal(1, ubits);
            Assert.Equal(0, tbits);
        }

        [Fact]
        public void NAFWindow_WindowExceedsMaxSize_BreaksAndBacktracks()
        {
            BigInteger exp = 11;
            var exp3 = exp * 3;

            int ubits = 0;
            int tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3, 
                index: 3, ref ubits, ref tbits, size: 2);

            Assert.Equal(-1, result);
            Assert.Equal(1, ubits);
            Assert.Equal(1, tbits);
        }

        [Fact]
        public void NAFWindow_BacktrackWhenResultOddAndNotReachedZero()
        {
            BigInteger exp = 19;
            var exp3 = exp * 3;
            int ubits = 0, tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3,
                index: 5, ref ubits, ref tbits, size: 4);

            Assert.NotEqual(0, result);
            Assert.True((result & 1) != 0);
            Assert.True(Math.Abs(result) <= (4 << 1) - 1);

            Assert.True(ubits > 0);
            Assert.True(tbits >= 0);
        }

        [Fact]
        public void NAFWindow_MultipleTrailingZerosAfterConstruction()
        {
            BigInteger exp = 85;
            var exp3 = exp * 3;

            int ubits = 0;
            int tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3,
                index: 7, ref ubits, ref tbits, size: 4);

            Assert.NotEqual(0, result);
            Assert.True((result & 1) == 1);

            Assert.True(tbits > 0, $"Expected tbits > 0 but got {tbits}");
            Assert.True(Math.Abs(result) <= (4 << 1) - 1);
        }

        [Fact]
        public void NAFWindow_LargestPossibleWindow_ReturnsMaxOddValue()
        {
            BigInteger exp = 11;
            var exp3 = exp * 3;

            int ubits = 0, tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3,
                index: 3, ref ubits, ref tbits, size: 3);

            Assert.NotEqual(0, result);
            Assert.True(Math.Abs(result) <= 5);
            Assert.True((result & 1) != 0);
        }

        [Fact]
        public void NAFWindow_SignIsConsistentWithInitialDigit()
        {
            BigInteger exp = 3;
            var exp3 = exp * 3;

            int ubits = 0; 
            int tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3, 
                index: 1, ref ubits, ref tbits, size: 4);
            Assert.True(result < 0);
        }

        #endregion

        #region Combined Consistency & Edge Cases

        [Fact]
        public void Window_And_NAFWindow_HandlesLargeExponents()
        {
            var exp = BigInteger.Pow(2, 255) - 19;
            int ubits = 0, tbits = 0;

            int win = WindowUtil.Window(exp, index: 255, 
                ref ubits, ref tbits, size: 5);

            Assert.True(win >= 0);
            Assert.Equal(1, ubits);
            Assert.True(tbits >= 0);

            var exp3 = exp * 3;
            ubits = tbits = 0;

            int nafWin = WindowUtil.NAFWindow(exp, exp3, 
                index: 255, ref ubits, ref tbits, size: 5);
            Assert.True(nafWin == 0 || (nafWin & 1) != 0);
        }

        #endregion

        #region Cryptographic Prime Tests

        [Fact]
        public void NAFWindow_LargeRandomExponents_ConsistentBitConsumption()
        {
            var sizes = new[] { 256, 
                384, 521, 448 };

            foreach (int bitLength in sizes)
            {
                var exp = SecureRandom.GenRandom(bitLength);
                var exp3 = exp * 3;

                int totalBits = bitLength;
                int currentBit = totalBits - 1;

                int windowsProcessed = 0;
                int bitsConsumedTotal = 0;

                while (currentBit >= 0)
                {
                    int ubits = 0;
                    int tbits = 0;

                    int result = WindowUtil.NAFWindow(exp, exp3, 
                        currentBit, ref ubits, ref tbits, 5);

                    Assert.True(ubits >= 1);

                    if (result != 0)
                    {
                        Assert.True((result & 1) != 0);
                        Assert.True(Math.Abs(result) <= 9);
                    }

                    bitsConsumedTotal += ubits;
                    currentBit -= ubits;
                    windowsProcessed++;
                }

                Assert.Equal(totalBits, bitsConsumedTotal);
                Assert.True(windowsProcessed > 0);
            }
        }

        [Fact]
        public void NAFWindow_CryptographicPrimes_AlwaysReturnsValidWindow()
        {
            var primes = new Dictionary<string, BigInteger>();
            primes["P-256"] = BigInteger.Pow(2, 256) - BigInteger.Pow(2, 224) + 
                BigInteger.Pow(2, 192) + BigInteger.Pow(2, 96) - 1;

            primes["Ed25519"] = BigInteger.Pow(2, 255) - 19;

            primes["P-384"] = BigInteger.Pow(2, 384) - BigInteger.Pow(2, 128) - 
                BigInteger.Pow(2, 96) + BigInteger.Pow(2, 32) - 1;

            primes["P-521"] = BigInteger.Pow(2, 521) - 1;
            primes["Ed448"] = BigInteger.Pow(2, 448) 
                - BigInteger.Pow(2, 224) - 1;

            foreach (var kvp in primes)
            {
                var p = kvp.Value;
                var p3 = p * 3;

                int totalBits = p.GetBits();

                for (int i = totalBits - 1; i >= 0; i--)
                {
                    int ubits = 0;
                    int tbits = 0;

                    int result = WindowUtil.NAFWindow(p, p3, i, ref ubits, ref tbits, 5);

                    if (result != 0)
                    {
                        Assert.True((result & 1) != 0);
                        Assert.True(Math.Abs(result) <= 9);
                        Assert.True(ubits >= 1);
                        Assert.True(tbits >= 0);
                    }
                    else
                    {
                        Assert.Equal(1, ubits);
                        Assert.Equal(0, tbits);
                    }

                    i -= (ubits - 1);
                }
            }
        }

        [Fact]
        public void Window_And_NAFWindow_CryptographicPrimes_ConsistentStructure()
        {
            var primes = new Dictionary<string, BigInteger>();

            primes["P-256"] = BigInteger.Pow(2, 256) - BigInteger.Pow(2, 224) +
                BigInteger.Pow(2, 192) + BigInteger.Pow(2, 96) - 1;
            primes["Ed25519"] = BigInteger.Pow(2, 255) - 19;

            primes["P-384"] = BigInteger.Pow(2, 384) - BigInteger.Pow(2, 128) -
                BigInteger.Pow(2, 96) + BigInteger.Pow(2, 32) - 1;

            primes["P-521"] = BigInteger.Pow(2, 521) - 1;
            primes["Ed448"] = BigInteger.Pow(2, 448) -
                BigInteger.Pow(2, 224) - 1;

            foreach (var kvp in primes)
            {
                var name = kvp.Key;
                var p = kvp.Value;

                BigInteger p3 = p * 3;
                int bits = p.GetBits();

                int i = bits - 1;
                int totalBitsConsumed = 0;

                while (i >= 1)
                {
                    int ubits_w = 0;
                    int tbits_w = 0;

                    int winResult = WindowUtil.Window(p, i, 
                        ref ubits_w, ref tbits_w, 5);

                    int ubits_n = 0;
                    int tbits_n = 0;

                    int nafResult = WindowUtil.NAFWindow(p, p3, 
                        i, ref ubits_n, ref tbits_n, 5);

                    if (winResult != 0)
                        Assert.True((winResult & 1) != 0);

                    if (nafResult != 0)
                        Assert.True((nafResult & 1) != 0);

                    if (winResult != 0)
                        Assert.True(ubits_w >= 1);

                    Assert.True(tbits_w >= 0);
                    Assert.True(tbits_n >= 0);

                    totalBitsConsumed += ubits_n;
                    i -= ubits_n;

                    if (tbits_n != 0)
                    {
                        totalBitsConsumed += tbits_n;
                        i -= tbits_n;
                    }
                }

                Assert.Equal(bits - 1, totalBitsConsumed);
            }
        }

        #endregion
    }
}
