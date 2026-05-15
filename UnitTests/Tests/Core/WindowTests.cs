using System;
using System.Collections.Generic;
using System.Text;

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

            int ubits = 0;
            int tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3, 
                index: 4, ref ubits, ref tbits, size: 3);

            Assert.NotEqual(0, result);
            Assert.True((result & 1) != 0);
            Assert.True(Math.Abs(result) <= (3 << 1) - 1);

            Assert.True(ubits > 0);
            Assert.True(tbits >= 0);
        }

        [Fact]
        public void NAFWindow_MultipleTrailingZerosAfterConstruction()
        {
            BigInteger exp = 55;
            var exp3 = exp * 3;

            int ubits = 0;
            int tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3, 
                index: 5, ref ubits, ref tbits, size: 4);

            Assert.True(result != 0);
            Assert.True((result & 1) != 0);

            Assert.True(tbits > 0);
            Assert.Equal(result & 1, 1);
        }

        [Fact]
        public void NAFWindow_LargestPossibleWindow_ReturnsMaxOddValue()
        {
            BigInteger exp = 9;
            var exp3 = exp * 3;

            int ubits = 0;
            int tbits = 0;

            int result = WindowUtil.NAFWindow(exp, exp3, 
                index: 3, ref ubits, ref tbits, size: 3);

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
    }
}
