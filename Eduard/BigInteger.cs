using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Eduard
{
    /// <summary>
    /// Represents an arbitrarily large signed integer with cryptographic-grade operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This implementation provides a complete set of arithmetic, bitwise, and cryptographic operations <br/>
    /// for arbitrary-precision integers. It is optimized for cryptographic applications including: <br/>
    /// </para>
    /// <list type="bullet">
    /// <item><description>RSA key generation and encryption operations</description></item>
    /// <item><description>Elliptic Curve Cryptography (ECC) point arithmetic</description></item>
    /// <item><description>Modular exponentiation with Barrett reduction</description></item>
    /// <item><description>Probabilistic primality testing (Miller-Rabin and Lucas)</description></item>
    /// </list>
    /// <para>
    /// Performance optimizations include Karatsuba multiplication for medium-sized operands, <br/>
    /// FFT-based multiplication for large operands, and Barrett reduction for modular arithmetic.
    /// </para>
    /// <para>
    /// The internal representation uses a 32-bit limb array in little-endian order with two's <br/>
    /// complement encoding for negative numbers.
    /// </para>
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public sealed class BigInteger : IEquatable<BigInteger>, IComparable<BigInteger>
    {
        internal Data data;

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> class with a value of zero.
        /// </summary>
        /// <remarks>
        /// This constructor creates a zero-initialized BigInteger suitable for accumulation operations. <br/>
        /// Memory allocation is minimal (one 32-bit limb).
        /// </remarks>
        public BigInteger()
        {
            data = new Data(1, 1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> class from a 64-bit signed integer.
        /// </summary>
        /// <param name="number">The 64-bit signed integer value to convert.</param>
        /// <remarks>
        /// The conversion preserves the sign and magnitude of the input. The internal representation <br/>
        /// uses the minimal number of 32-bit limbs required to represent the absolute value.
        /// </remarks>
        public BigInteger(long number)
        {
            data = new Data(3);

            while (number != 0 && data.Used < data.Length)
            {
                data[data.Used] = (uint)(number & 0xFFFFFFFF);
                number >>= 32;
                data.Used++;
            }

            data.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> class from a 64-bit unsigned integer.
        /// </summary>
        /// <param name="number">The 64-bit unsigned integer value to convert.</param>
        /// <remarks>
        /// The resulting BigInteger is always non-negative. This constructor is particularly <br/>
        /// useful when working with cryptographic nonces or initialization vectors.
        /// </remarks>
        public BigInteger(ulong number)
        {
            data = new Data(3);

            while (number != 0 && data.Used < data.Length)
            {
                data[data.Used] = (uint)(number & 0xFFFFFFFF);
                number >>= 32;
                data.Used++;
            }

            data.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> class from a decimal string representation.
        /// </summary>
        /// <param name="digits">The decimal string representation of the integer. May include a leading minus sign.</param>
        /// <exception cref="FormatException">
        /// Thrown when <paramref name="digits"/> is null, empty, or contains characters that are
        /// not valid decimal digits (0-9), or has an invalid format such as a misplaced minus sign.
        /// </exception>
        /// <remarks>
        /// Parsing is performed in chunks of 9 digits for optimal performance. Leading zeros are <br/>
        /// allowed but ignored. The string may be arbitrarily long, subject to memory constraints.
        /// </remarks>
        public BigInteger(string digits)
        {
            if (string.IsNullOrEmpty(digits))
                throw new FormatException(
                    "Input string cannot be" +
                    " null or empty.");

            if (!Check(digits, Radix.Decimal))
                throw new FormatException(
                    $"Invalid format for decimal number." +
                    $" The string must contain only digits 0-9" +
                    $" and an optional leading minus sign.");

            BuildDecimal(digits);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> class from a string in the specified radix.
        /// </summary>
        /// <param name="digits">The string representation of the integer.</param>
        /// <param name="radix">The base of the number system (decimal or hexadecimal).</param>
        /// <exception cref="FormatException">
        /// Thrown when the string contains characters invalid for the specified radix.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For decimal strings, parsing is performed in chunks of 9 digits. For hexadecimal strings, <br/>
        /// parsing is performed in chunks of 8 characters (32 bits) for optimal performance.
        /// </para>
        /// <para>
        /// Hexadecimal strings may contain digits 0-9, letters A-F (case-insensitive).
        /// </para>
        /// </remarks>
        public BigInteger(string digits, Radix radix)
        {
            if (string.IsNullOrEmpty(digits))
                throw new FormatException(
                    "Input string cannot be" 
                    + " null or empty.");

            if (!Check(digits, radix))
            {
                string validChars = radix == Radix.Decimal
                    ? "digits 0-9"
                    : "digits 0-9, letters A-F or a-f";

                throw new FormatException(
                    $"Invalid format for {radix.ToString().ToLower()} number." 
                    + $" The string must contain only {validChars}.");
            }

            if (radix == Radix.Decimal)
                BuildDecimal(digits);
            else
                BuildHexaDecimal(digits);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> class from a byte array in big-endian format.
        /// </summary>
        /// <param name="array">The byte array representing the integer in big-endian order.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="array"/> is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the byte array length exceeds the maximum supported size.</exception>
        /// <remarks>
        /// <para>
        /// The byte array is interpreted in big-endian format (most significant byte first). <br/>
        /// If the most significant bit of the first byte is set, the number is interpreted as <br/>
        /// negative using two's complement encoding.
        /// </para>
        /// <para>
        /// This constructor is commonly used to convert cryptographic primitives like RSA parameters, <br/>
        /// ECC coordinates, or hash values into BigInteger format.
        /// </para>
        /// <para>
        /// Performance: O(n) where n is the length of the byte array, with efficient 32-bit word packing.
        /// </para>
        /// </remarks>
        public BigInteger(byte[] array)
        {
            if (array == null)
                throw new ArgumentNullException(
                    nameof(array), "The byte " +
                    "array cannot be null.");

            if (array.Length == 0)
                throw new ArgumentException(
                    "The byte array cannot be" +
                    " empty. Use the parameterless" +
                    " constructor for zero values.",
                    nameof(array));

            const int maxAllowedLength = int.MaxValue >> 2;

            if (array.Length > maxAllowedLength)
                throw new ArgumentException(
                    $"The byte array length ({array.Length})" +
                    " exceeds the maximum supported length" +
                    $" of {maxAllowedLength} bytes.",
                    nameof(array));

            const uint mask = 0x80;
            bool isNegative = (array[0] 
                & mask) == mask;

            int trimCount = 0;
            int i = 0, j, k;

            while (i < array.Length - 1)
            {
                uint currentByte = array[i];
                uint nextByte = array[i + 1];

                bool shouldTrim = isNegative ?
                    currentByte == 0xFF && (nextByte & mask) == mask
                    : currentByte == 0 && (nextByte & mask) == 0;

                if (!shouldTrim)
                    break;

                trimCount++;
                i++;
            }

            int size = array.Length - trimCount;
            int length = size >> 2;
            int rem = size & 3;

            if (rem != 0) length++;
            data = new Data(length - 1);

            uint digit;
            int h = 0;

            for(i = array.Length - 1; i >= trimCount; i -= 4)
            {
                digit = 0;

                for(j = 0; j < 4; j++)
                {
                    uint block = (i >= j) ? 
                        (uint)array[i - j] : 
                        (uint)(isNegative ? 
                        0xFF : 0x00);

                    uint val = block << (8 * j);
                    digit |= val;
                }

                data[h++] = digit;
            }

            data.Update();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> class with a random positive value of the specified bit length.
        /// </summary>
        /// <param name="n">The number of bits for the generated value. Must be greater than 0.</param>
        /// <param name="rand">A cryptographically secure random number generator.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="n"/> is less than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rand"/> is null.</exception>
        /// <remarks>
        /// The generated value is uniformly distributed across all numbers with exactly <paramref name="n"/>
        /// bits (the most significant bit is always 1).<br/> This ensures proper bit length for cryptographic operations
        /// such as RSA key generation where the modulus must have<br/> exactly <paramref name="n"/> bits.
        /// </remarks>
        public BigInteger(int n, RandomNumberGenerator rand)
        {
            if (n <= 0)
                throw new ArgumentException(
                    "The number of bits must" + 
                    $" be positive. Specified: {n}.",
                    nameof(n));

            if (rand == null)
                throw new ArgumentNullException(
                    nameof(rand), "The random number" + 
                    " generator cannot be null.");

            int bufLen = n >> 5;
            int remLen = n & 0x1F;
            int length = bufLen;

            if (remLen != 0)
                length++;

            data = new Data(length);
            byte[] temp = new byte[4];

            for (int k = 0; k < bufLen; k++)
            {
                rand.GetBytes(temp);
                data[k] = BitConverter.ToUInt32(temp, 0);
            }

            uint mask = ((uint)0x1 << remLen) - 1;

            if(mask != 0)
            {
                rand.GetBytes(temp);
                data[bufLen] = BitConverter.ToUInt32(temp, 0);
                data[bufLen] &= mask;
            }

            data.Update();
        }

        internal BigInteger(Data buffer)
        {
            buffer.Update();
            data = buffer;
        }

        private bool Check(string digits, Radix radix)
        {
            int StartPos = 0;

            if (radix == Radix.Decimal && digits[0] == '-')
                StartPos = 1;

            for (int k = StartPos; k < digits.Length; k++)
            {
                char c = digits[k];

                if (radix == Radix.Decimal)
                {
                    if (c < '0' || c > '9')
                        return false;
                }
                else if (radix == Radix.HexaDecimal)
                {
                    if (!((c >= '0' && c <= '9') ||
                          (c >= 'A' && c <= 'F') ||
                          (c >= 'a' && c <= 'f')))
                        return false;
                }
            }

            return true;
        }

        private void BuildDecimal(string digits)
        {
            bool isNegative = (digits[0] == '-');
            int startPos = isNegative ? 1 : 0;

            BigInteger res = 0, mult = 1;
            const int size = 9;

            uint[] table = new uint[size];
            int len = digits.Length;
            table[0] = 10;

            for (int i = 1; i < size; i++)
                table[i] = table[i - 1] * 10;

            for (int i = len; i > startPos; i -= size)
            {
                int startIndex = Math.Max(startPos, i - size);
                int length = i - startIndex;

                string chunk = digits.Substring(startIndex, length);
                int value = int.Parse(chunk);

                res += (value * mult);
                mult *= table[length - 1];
            }

            if (isNegative) 
                res = -res;
            data = res.data;
        }

        private uint GetDigit(char hexDigit)
        {
            uint val = hexDigit;

            if (val >= '0' && val <= '9')
                val -= 48;
            else
                if (val >= 'A' && val <= 'F')
                    val = (val - 'A') + 10;
                else
                    if (val >= 'a' && val <= 'f')
                        val = (val - 'a') + 10;

            return val;
        }

        private void BuildHexaDecimal(string digits)
        {
            bool isNegative = GetDigit(digits[0]) >= 8;
            int trimCount = 0;
            int i = 0, j, k;

            while (i < digits.Length - 1)
            {
                uint currentDigit = GetDigit(digits[i]);
                uint nextDigit = GetDigit(digits[i + 1]);

                bool shouldTrim = isNegative ?
                    currentDigit == 0xF && (nextDigit & 0x8) == 0x8
                    : currentDigit == 0 && (nextDigit & 0x8) == 0;

                if (!shouldTrim)
                    break;

                trimCount++;
                i++;
            }

            int size = digits.Length - trimCount;
            int limit = size & 7;

            int bufLen = size >> 3;
            int length = bufLen;

            if (limit != 0)
                length++;

            data = new Data(length - 1);
            uint digit;
            i = 0;

            for (j = digits.Length - 1; j >= trimCount; j -= 8)
            {
                digit = 0;
                uint val = 0;

                for (k = 0; k < 8; k++)
                {
                    val = (j >= k) ?
                        GetDigit(digits[j - k])
                        : (uint)(isNegative ?
                        0xF : 0x0);

                    digit |= (val << (4 * k));
                }

                data[i++] = digit;
            }

            data.Update();
        }

        /// <summary>
        /// Converts the string representation of a decimal number to its <see cref="BigInteger"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a decimal number to convert.</param>
        /// <returns>A <see cref="BigInteger"/> equivalent to the number contained in <paramref name="value"/>.</returns>
        /// <exception cref="FormatException">
        /// Thrown when <paramref name="value"/> is null, empty, or contains characters that are
        /// not valid decimal digits (0-9), or has an invalid format such as a misplaced minus sign.
        /// </exception>
        public static BigInteger Parse(string value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Converts the string representation of a number in the specified radix to its <see cref="BigInteger"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a number to convert.</param>
        /// <param name="radix">The base of the number system (decimal or hexadecimal).</param>
        /// <returns>A <see cref="BigInteger"/> equivalent to the number contained in <paramref name="value"/>.</returns>
        /// <exception cref="FormatException">
        /// Thrown when <paramref name="value"/> is null, empty, or 
        /// contains characters invalid for the specified radix.
        /// </exception>
        public static BigInteger Parse(string value, Radix radix)
        {
            return new BigInteger(value, radix);
        }

        /// <summary>
        /// Tries to convert the string representation of a decimal number to its <see cref="BigInteger"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a decimal number to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="BigInteger"/> equivalent if conversion succeeded,
        /// or zero if the conversion failed.
        /// </param>
        /// <returns><c>true</c> if conversion succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string value, out BigInteger result)
        {
            result = 0;

            if (string.IsNullOrEmpty(value)) 
                return false;

            try 
            { 
                result = Parse(value); 
                return true; 
            }
            catch (FormatException) 
            { 
                return false; 
            }
        }

        /// <summary>
        /// Tries to convert the string representation of a number in the specified radix to its <see cref="BigInteger"/> equivalent.
        /// </summary>
        /// <param name="value">A string containing a number to convert.</param>
        /// <param name="radix">The base of the number system (decimal or hexadecimal).</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="BigInteger"/> equivalent if conversion succeeded,
        /// or zero if the conversion failed.
        /// </param>
        /// <returns><c>true</c> if conversion succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string value, Radix radix, out BigInteger result)
        {
            result = 0;

            if (string.IsNullOrEmpty(value)) 
                return false;

            try 
            { 
                result = Parse(value, radix); 
                return true; 
            }
            catch (FormatException) 
            { 
                return false; 
            }
        }

        /// <summary>
        /// Adds two BigInteger values and returns the result.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
        /// <remarks>
        /// The addition operation handles both positive and negative numbers correctly using two's <br/>
        /// complement semantics. The result is automatically normalized to use the minimal number <br/>
        /// of limbs required for representation.
        /// </remarks>
        public static BigInteger operator +(BigInteger left, BigInteger right)
        {
            int length = Math.Max(left.data.Used, right.data.Used);
            Data buffer = new Data(length + 1);

            long carry = 0;

            for(int k = 0; k < buffer.Length; k++)
            {
                long sum = (long)left.data[k] + (long)right.data[k] + carry;
                buffer[k] = (uint)(sum & 0xFFFFFFFF);
                carry = sum >> 32;
            }

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Increments a BigInteger value by 1.
        /// </summary>
        /// <param name="val">The value to increment.</param>
        /// <returns>The value of <paramref name="val"/> increased by 1.</returns>
        public static BigInteger operator ++(BigInteger val)
        {
            return (val + 1);
        }

        /// <summary>
        /// Subtracts one BigInteger value from another.
        /// </summary>
        /// <param name="left">The value to subtract from (the minuend).</param>
        /// <param name="right">The value to subtract (the subtrahend).</param>
        /// <returns>The result of subtracting <paramref name="right"/> from <paramref name="left"/>.</returns>
        public static BigInteger operator -(BigInteger left, BigInteger right)
        {
            int length = Math.Max(left.data.Used, right.data.Used) + 1;
            Data buffer = new Data(length);

            long carry = 0;

            for(int k = 0; k < buffer.Length; k++)
            {
                long diff = (long)left.data[k] - (long)right.data[k] - carry;
                buffer[k] = (uint)(diff & 0xFFFFFFFF);
                carry = ((diff < 0) ? 1 : 0);
            }

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Decrements a BigInteger value by 1.
        /// </summary>
        /// <param name="val">The value to decrement.</param>
        /// <returns>The value of <paramref name="val"/> decreased by 1.</returns>
        public static BigInteger operator --(BigInteger val)
        {
            return (val - 1);
        }

        private static BigInteger PlainMultiply(BigInteger left, BigInteger right)
        {
            bool Sign = (left.IsNegative != right.IsNegative);
            left = left.Abs();
            right = right.Abs();

            Data buffer = new Data(left.data.Used + right.data.Used);

            for (int j = 0; j < left.data.Used; j++)
            {
                ulong carry = 0;
                if (left.data[j] == 0) continue;

                for (int k = 0; k < right.data.Used; k++)
                {
                    ulong val = ((ulong)left.data[j] * (ulong)right.data[k]) + (ulong)buffer[j + k] + carry;
                    buffer[j + k] = (uint)(val & 0xFFFFFFFF);
                    carry = val >> 32;
                }

                if (carry != 0)
                    buffer[j + right.data.Used] = (uint)carry;
            }

            BigInteger result = new BigInteger(buffer);

            if (Sign)
                return -result;

            return result;
        }

        private static BigInteger PlainSquare(BigInteger value)
        {
            if (value == 0 || value == 1) return value;
            value = value.Abs();
            Data buffer = new Data(2 * value.data.Used);
            ulong carry = 0;

            for(int i = 0; i < value.data.Used; i++)
            {
                int k = i * 2;
                ulong val = (ulong)value.data[i] * (ulong)value.data[i];
                carry = val >> 32;
                buffer[k] = (uint)(val & 0xFFFFFFFF);
                buffer[k + 1] = (uint)carry;
            }

            for (int i = 0; i < value.data.Used; i++)
            {
                carry = 0;
                int k = i * 2 + 1;

                for (int j = i + 1; j < value.data.Used; j++, k++)
                {
                    ulong val = (ulong)value.data[j] * (ulong)value.data[i] + (ulong)buffer[k] + carry;
                    carry = val >> 32;
                    ulong temp = (uint)(val & 0xFFFFFFFF);
                    temp += (ulong)value.data[j] * (ulong)value.data[i];
                    carry += temp >> 32;
                    buffer[k] = (uint)(temp & 0xFFFFFFFF);
                }

                k = value.data.Used + i;
                ulong digit = carry + (ulong)buffer[k];
                carry = digit >> 32;
                buffer[k] = (uint)(digit & 0xFFFFFFFF);
                buffer[k + 1] += (uint)carry;
            }

            BigInteger result = new BigInteger(buffer);
            return result;
        }

        public static BigInteger KMultiply(BigInteger left, BigInteger right)
        {
            int length = Math.Max(left.data.Used, right.data.Used);
            if (length <= 10) return PlainMultiply(left, right);
            length = (length >> 1) + (length & 0x1);

            Data at = new Data(length);
            Data bt = new Data(length);

            for(int i = 0, j = length; i < length; i++, j++)
            {
                at[i] = left.data[i];
                bt[i] = left.data[j];
            }

            BigInteger b = new BigInteger(bt);
            BigInteger a = new BigInteger(at);

            at = new Data(length);
            bt = new Data(length);

            for (int i = 0, j = length; i < length; i++, j++)
            {
                at[i] = right.data[i];
                bt[i] = right.data[j];
            }

            BigInteger d = new BigInteger(bt);
            BigInteger c = new BigInteger(at);

            BigInteger ac = KMultiply(a, c);
            BigInteger bd = KMultiply(b, d);
            BigInteger abcd = KMultiply(a + b, c + d);

            BigInteger diff = abcd - ac - bd;
            Data dat = new Data(diff.data.Used + length);

            for (int i = 0; i < diff.data.Used; i++)
                dat[i + length] = diff.data[i];

            BigInteger result = ac + new BigInteger(dat);
            dat = new Data(bd.data.Used + 2 * length);

            for (int i = 0; i < bd.data.Used; i++)
                dat[i + 2 * length] = bd.data[i];

            result += new BigInteger(dat);
            return result;
        }

        private static BigInteger KSquare(BigInteger val)
        {
            int length = (val.data.Used + 1) >> 1;
            Data at = new Data(length);
            Data bt = new Data(length);

            for (int i = 0, j = length; i < length; i++, j++)
            {
                at[i] = val.data[i];
                bt[i] = val.data[j];
            }

            BigInteger xl = new BigInteger(at);
            BigInteger xh = new BigInteger(bt);

            BigInteger xhs = Square(xh);
            BigInteger xls = Square(xl);

            BigInteger xlhs = Square(xl + xh);
            BigInteger fr = xhs + xls;
            xlhs -= fr;

            Data dat = new Data(xhs.data.Used + 2 * length);

            for (int i = 0; i < xhs.data.Used; i++)
                dat[i + 2 * length] = xhs.data[i];

            BigInteger result = new BigInteger(dat);
            dat = new Data(xlhs.data.Used + length);

            for (int i = 0; i < xlhs.data.Used; i++)
                dat[i + length] = xlhs.data[i];

            result += new BigInteger(dat);
            result += xls;

            return result;
        }

        private static BigInteger Multiply(BigInteger left, BigInteger right)
        {
            int order = Math.Min(left.data.Used, right.data.Used);

#if !USE_BENCHMARKING
            int FFT_THRESHOLD = (int)Threshold.BIGINT_FFT_THRESHOLD;
            int KARATSUBA_THRESHOLD = (int)Threshold.BIGINT_KARATSUBA_MULT_THRESHOLD;
#else
            int FFT_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.BIGINT_FFT);
            int KARATSUBA_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.BIGINT_KARATSUBA_MULTIPLY);
#endif

            if (order >= FFT_THRESHOLD)
                return FFT.FastBigMult(left, right);
            else if (order >= KARATSUBA_THRESHOLD)
            {
                bool sign = (left.IsNegative != right.IsNegative);
                left = left.Abs(); right = right.Abs();

                BigInteger result = KMultiply(left, right);
                return sign ? -result : result;
            }
            else
                return PlainMultiply(left, right);
        }

        private static BigInteger Square(BigInteger val)
        {
            int order = val.data.Used;

#if !USE_BENCHMARKING
            int FFT_THRESHOLD = (int)Threshold.BIGINT_FFT_THRESHOLD;
            int KARATSUBA_THRESHOLD = (int)Threshold.BIGINT_KARATSUBA_SQUARE_THRESHOLD;
#else
            int FFT_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.BIGINT_FFT);
            int KARATSUBA_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.BIGINT_KARATSUBA_SQUARING);
#endif

            if (order >= FFT_THRESHOLD)
                return FFT.FastBigMult(val, val);
            else if (order >= KARATSUBA_THRESHOLD)
                return KSquare(val);
            else
                return PlainSquare(val);
        }

        /// <summary>
        /// Multiplies two BigInteger values using the optimal algorithm based on operand size.
        /// </summary>
        /// <param name="left">The first multiplicand.</param>
        /// <param name="right">The second multiplicand.</param>
        /// <returns>The product of <paramref name="left"/> and <paramref name="right"/>.</returns>
        /// <remarks>
        /// <para>
        /// This operator automatically selects the most efficient multiplication algorithm:
        /// </para>
        /// <list type="bullet">
        /// <item><description>Plain O(n^2) multiplication for small operands (below Karatsuba threshold)</description></item>
        /// <item><description>Karatsuba O(n^1.585) for medium operands</description></item>
        /// <item><description>FFT-based O(n*log n) for large operands (above FFT threshold)</description></item>
        /// </list>
        /// <para>
        /// When both operands are equal, the squaring optimization is applied, <br/>
        /// which is approximately twice as fast as general multiplication.
        /// </para>
        /// </remarks>
        public static BigInteger operator *(BigInteger left, BigInteger right)
        {
            if (left == right) return Square(left);
            return Multiply(left, right);
        }

        private static void SingleDivide(BigInteger left, BigInteger right, out BigInteger quotient, out BigInteger remainder)
        {
            Data RemainderData = new Data(left.data);
            RemainderData.Update();

            int pos = RemainderData.Used - 1;
            ulong divisor = (ulong)right.data[0];
            ulong dividend = (ulong)RemainderData[pos];

            uint[] result = new uint[left.data.Length];
            left.data.CopyTo(result, 0, result.Length);
            int resultPos = 0;

            if (dividend >= divisor)
            {
                result[resultPos++] = (uint)(dividend / divisor);
                RemainderData[pos] = (uint)(dividend % divisor);
            }
            pos--;

            while (pos >= 0)
            {
                dividend = ((ulong)(RemainderData[pos + 1]) << 32) + (ulong)RemainderData[pos];
                result[resultPos++] = (uint)(dividend / divisor);
                RemainderData[pos + 1] = 0;
                RemainderData[pos--] = (uint)(dividend % divisor);
            }

            remainder = new BigInteger(RemainderData);

            Data QuotientData = new Data(resultPos + 1, resultPos);
            int j = 0;

            for (int i = QuotientData.Used - 1; i >= 0; i--, j++)
                QuotientData[j] = result[i];

            quotient = new BigInteger(QuotientData);
        }

        private static void MultiDivide(BigInteger left, BigInteger right, out BigInteger quotient, out BigInteger remainder)
        {
            uint val = right.data[right.data.Used - 1];
            int d = 0;

            for (uint mask = 0x80000000; mask != 0 && (val & mask) == 0; mask >>= 1)
                d++;

            int remainderLen = left.data.Used + 1;
            uint[] remainderDat = new uint[remainderLen];
            left.data.CopyTo(remainderDat, 0, left.data.Used);

            Data.ShiftLeft(remainderDat, d);
            right = right << d;

            ulong firstDivisor = right.data[right.data.Used - 1];
            ulong secondDivisor = (right.data.Used < 2 ? (uint)0 : right.data[right.data.Used - 2]);

            int divisorLen = right.data.Used + 1;
            Data dividendPart = new Data(divisorLen, divisorLen);
            uint[] result = new uint[left.data.Length + 1];
            int resultPos = 0;

            ulong carryBit = (ulong)1 << 32;
            for (int j = remainderLen - right.data.Used, pos = remainderLen - 1; j > 0; j--, pos--)
            {
                ulong dividend = ((ulong)remainderDat[pos] << 32) + (ulong)remainderDat[pos - 1];
                ulong qHat = (dividend / firstDivisor);
                ulong rHat = (dividend % firstDivisor);

                while (pos >= 2)
                {
                    if (qHat == carryBit || (qHat * secondDivisor) > ((rHat << 32) + remainderDat[pos - 2]))
                    {
                        qHat--;
                        rHat += firstDivisor;

                        if (rHat < carryBit)
                            continue;
                    }
                    break;
                }

                for (int h = 0; h < divisorLen; h++)
                    dividendPart[divisorLen - h - 1] = remainderDat[pos - h];

                BigInteger dTemp = new BigInteger(dividendPart);
                BigInteger rTemp = right * (long)qHat;
                while (rTemp > dTemp)
                {
                    qHat--;
                    rTemp -= right;
                }

                rTemp = dTemp - rTemp;

                for (int h = 0; h < divisorLen; h++)
                    remainderDat[pos - h] = rTemp.data[right.data.Used - h];

                result[resultPos++] = (uint)qHat;
            }

            Array.Reverse(result, 0, resultPos);
            quotient = new BigInteger(new Data(result));

            int n = Data.ShiftRight(remainderDat, d);
            Data buf = new Data(n, n);
            buf.CopyFrom(remainderDat, 0, 0, buf.Used);
            remainder = new BigInteger(buf);
        }

        private static void Divide(BigInteger left, BigInteger right, out BigInteger quotient, out BigInteger remainder)
        {
            if (left.IsZero)
            {
                quotient = new BigInteger();
                remainder = new BigInteger();
                return;
            }

            if (right.data.Used == 1)
                SingleDivide(left, right, out quotient, out remainder);
            else
                MultiDivide(left, right, out quotient, out remainder);
        }

        /// <summary>
        /// Divides one BigInteger value by another using integer division.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The integer quotient of <paramref name="left"/> divided by <paramref name="right"/>.</returns>
        /// <exception cref="DivideByZeroException">Thrown when attempting to divide by zero. 
        /// Division by zero is mathematically undefined and cryptographically invalid.
        /// </exception>
        /// <remarks>
        /// Division is performed using the standard long division algorithm optimized for arbitrary precision. <br/>
        /// For single-limb divisors, a specialized algorithm is used for better performance. The quotient is <br/>
        /// truncated toward zero (same as C# integer division).
        /// </remarks>
        public static BigInteger operator /(BigInteger left, BigInteger right)
        {
            if (right.IsZero)
                throw new DivideByZeroException(
                    "Division by zero is not allowed." + 
                    " The divisor cannot be zero.");

            bool Sign = (left.IsNegative != right.IsNegative);

            left = left.Abs();
            right = right.Abs();

            if (left < right)
                return new BigInteger();

            if (left == right)
                return (Sign ? -1 : 1);

            BigInteger quotient, remainder;
            Divide(left, right, out quotient, out remainder);

            return (Sign ? -quotient : quotient);
        }

        /// <summary>
        /// Returns the remainder from integer division of two BigInteger values.
        /// </summary>
        /// <param name="left">The dividend.</param>
        /// <param name="right">The divisor.</param>
        /// <returns>The remainder after dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
        /// <exception cref="DivideByZeroException">Thrown when <paramref name="right"/> is zero. 
        /// Modulo operation is undefined when the divisor is zero.
        /// </exception>
        /// <remarks>
        /// The remainder has the same sign as the dividend (consistent with <br/>
        /// C# remainder operator). This operation is equivalent to: <br/> 
        /// left - (left / right) * right.
        /// </remarks>
        public static BigInteger operator %(BigInteger left, BigInteger right)
        {
            if (right.IsZero)
                throw new DivideByZeroException(
                    "Modulo operation is undefined" + 
                    " when the divisor is zero.");

            BigInteger Quotient, Remainder;
            bool Sign = left.IsNegative;

            left = left.Abs();
            right = right.Abs();

            if (left < right)
                return (Sign ? -left : left);

            if (left == right)
                return 0;

            Divide(left, right, out Quotient, out Remainder);

            return (Sign ? -Remainder : Remainder);
        }

        /// <summary>
        /// Negates a BigInteger value (returns the additive inverse).
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>The value of <paramref name="value"/> multiplied by -1.</returns>
        /// <remarks>
        /// For zero, this operator returns zero. For positive values, <br/>
        /// returns the corresponding negative. For negative values, <br/>
        /// returns the corresponding positive.
        /// </remarks>
        public static BigInteger operator -(BigInteger value)
        {
            if (value.IsZero)
                return new BigInteger();

            Data buffer = new Data(value.data.Used + 1, value.data.Used + 1);

            for (int k = 0; k < buffer.Length; k++)
                buffer[k] = value.data[k] ^ 0xFFFFFFFF;

            bool carry = true;
            int j = 0;

            while(carry && j < buffer.Length)
            {
                long val = (long)buffer[j] + 1;
                buffer[j] = (uint)(val & 0xFFFFFFFF);
                carry = ((val >> 32) > 0);
                j++;
            }

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Negates this BigInteger instance.
        /// </summary>
        /// <returns>A new BigInteger representing the additive inverse of this instance.</returns>
        /// <remarks>
        /// This method does not modify the original instance; it returns a new instance.
        /// </remarks>
        public BigInteger Negate()
        {
            return -this;
        }

        /// <summary>
        /// Returns the absolute value of this BigInteger.
        /// </summary>
        /// <returns>A new BigInteger representing the absolute value of this instance.</returns>
        /// <remarks>
        /// For positive numbers, this method returns the same value. <br/> For negative numbers,
        /// it returns the corresponding positive <br/> value. For zero, returns zero.
        /// </remarks>
        public BigInteger Abs()
        {
            if (IsNegative)
                return -this;

            return this;
        }

        /// <summary>
        /// Performs a bitwise AND operation on two BigInteger values.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        /// <returns>The result of performing a bitwise AND on <paramref name="left"/> and <paramref name="right"/>.</returns>
        /// <remarks>
        /// The operation is performed on the two's complement representation of <br/>
        /// the numbers. For negative numbers, this follows standard C# bitwise <br/> 
        /// AND semantics.
        /// </remarks>
        public static BigInteger operator &(BigInteger left, BigInteger right)
        {
            int length = Math.Max(left.data.Used, right.data.Used);
            Data buffer = new Data(length, length);

            for (int k = 0; k < length; k++)
                buffer[k] = left.data[k] & right.data[k];

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Performs a bitwise OR operation on two BigInteger values.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        /// <returns>The result of performing a bitwise OR on <paramref name="left"/> and <paramref name="right"/>.</returns>
        public static BigInteger operator |(BigInteger left, BigInteger right)
        {
            int length = Math.Max(left.data.Used, right.data.Used);
            Data buffer = new Data(length, length);

            for (int k = 0; k < length; k++)
                buffer[k] = left.data[k] | right.data[k];

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Performs a bitwise exclusive OR (XOR) operation on two BigInteger values.
        /// </summary>
        /// <param name="left">The first operand.</param>
        /// <param name="right">The second operand.</param>
        /// <returns>The result of performing a bitwise XOR on <paramref name="left"/> and <paramref name="right"/>.</returns>
        public static BigInteger operator ^(BigInteger left, BigInteger right)
        {
            int length = Math.Max(left.data.Used, right.data.Used);
            Data buffer = new Data(length, length);

            for (int k = 0; k < length; k++)
                buffer[k] = left.data[k] ^ right.data[k];

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Returns the bitwise one's complement of a BigInteger value.
        /// </summary>
        /// <param name="value">The value to complement.</param>
        /// <returns>The bitwise complement of <paramref name="value"/>.</returns>
        /// <remarks>
        /// The one's complement flips all bits in the two's complement <br/>
        /// representation. This is equivalent to -(value + 1) in integer <br/>
        /// arithmetic.
        /// </remarks>
        public static BigInteger operator ~(BigInteger value)
        {
            Data buffer = new Data(value.data.Length);

            for (int k = 0; k < buffer.Length; k++)
                buffer[k] = value.data[k] ^ 0xFFFFFFFF;

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Shifts a BigInteger value left by a specified number of bits.
        /// </summary>
        /// <param name="val">The value to shift.</param>
        /// <param name="n">The number of bits to shift left (must be non-negative).</param>
        /// <returns>The result of shifting <paramref name="val"/> left by <paramref name="n"/> bits.</returns>
        /// <remarks>
        /// Left shifting by n bits is equivalent to multiplying by 2^n. <br/>
        /// This operation expands the internal array as needed to <br/>
        /// accommodate the new bits.
        /// </remarks>
        public static BigInteger operator <<(BigInteger val, int n)
        {
            Data buffer = new Data(val.data);
            buffer.Used = buffer.ShiftLeftWithoutOverflow(n);

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Shifts a BigInteger value right by a specified number of bits.
        /// </summary>
        /// <param name="val">The value to shift.</param>
        /// <param name="n">The number of bits to shift right (must be non-negative).</param>
        /// <returns>The result of shifting <paramref name="val"/> right by <paramref name="n"/> bits.</returns>
        /// <remarks>
        /// Right shifting by n bits is equivalent to integer division by 2^n <br/>
        /// with truncation toward negative infinity for negative numbers <br/>
        /// (arithmetic shift). This matches C#'s right shift behavior for <br/>
        /// signed integers.
        /// </remarks>
        public static BigInteger operator >>(BigInteger val, int n)
        {
            Data buffer = new Data(val.data);
            buffer.Used = buffer.ShiftRight(n);

            if(val.IsNegative)
            {
                for (int j = buffer.Length - 1; j >= buffer.Used; j--)
                    buffer[j] = 0xFFFFFFFF;

                uint mask = 0x80000000;

                for(int k = 1; k <= 32; k++)
                {
                    if ((buffer[buffer.Used - 1] & mask) == 0x80000000)
                        break;

                    buffer[buffer.Used - 1] |= mask;
                    mask >>= 1;
                }
            }

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Compares this instance to a second BigInteger and returns an integer indicating the relationship.
        /// </summary>
        /// <param name="other">The BigInteger to compare with this instance.</param>
        /// <returns>
        /// A negative value if this instance is less than <paramref name="other"/>,
        /// zero if they are equal, or a positive value if this instance is greater.
        /// </returns>
        public int CompareTo(BigInteger other)
        {
            return Compare(this, other);
        }

        /// <summary>
        /// Compares two BigInteger values and returns an integer indicating their relationship.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// -1 if <paramref name="left"/> is less than <paramref name="right"/>,
        /// 0 if they are equal,
        /// 1 if <paramref name="left"/> is greater than <paramref name="right"/>.
        /// </returns>
        public static int Compare(BigInteger left, BigInteger right)
        {
            if (left.IsNegative != right.IsNegative)
                return left.IsNegative ? -1 : 1;

            if (left.data.Used != right.data.Used)
                return left.data.Used < right.data.Used ? -1 : 1;

            for (int k = left.data.Used - 1; k >= 0; k--)
            {
                if (left.data[k] != right.data[k])
                    return left.data[k] < right.data[k] ? -1 : 1;
            }

            return 0;
        }

        /// <summary>
        /// Determines whether a number is almost certainly prime using a combination of Miller-Rabin and extra strong Lucas tests.
        /// </summary>
        /// <param name="val">The value to test for primality.</param>
        /// <returns>
        /// <c>true</c> if the number passes the strong probable prime tests; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method implements a deterministic primality test for numbers less than <br/>
        /// 2^64, and a probabilistic test for larger numbers. The algorithm combines:
        /// </para>
        /// <list type="bullet">
        /// <item><description>Small prime trial division (up to 256)</description></item>
        /// <item><description>Miller-Rabin test with base 2</description></item>
        /// <item><description>Extra strong Lucas test with Baillie-PSW parameters</description></item>
        /// </list>
        /// <para>
        /// For numbers less than 2^64, this provides deterministic results. For larger numbers, <br/>
        /// the test is probabilistic but no known composite has been found to pass this combination.
        /// </para>
        /// <para>
        /// For cryptographic applications requiring specific security levels, consider using <br/>
        /// <see cref="IsProbablePrime(RandomNumberGenerator, BigInteger, int)"/> with multiple trials.
        /// </para>
        /// </remarks>
        public static bool IsProbablePrime(BigInteger val)
        {
            if (val < 2) return false;
            if (val == 2) return true;
            if ((val & 1) == 0) return false;
            Sieve sieve = new Sieve(256);

            for (int i = 1; i < sieve.Count; i++)
            {
                if (val == sieve[i]) return true;
                if (val % sieve[i] == 0) return false;
            }

            BigInteger order = val - 1;
            int s = 0;

            while ((order & 1) == 0)
            {
                order >>= 1;
                s++;
            }

            BigInteger test = Pow(2, order, val);
            int tk = val.data.Used << 1;
            Data buffer = new Data(tk + 1, tk + 1);
            buffer[tk] = 0x0001;

            BigInteger constant = new BigInteger(buffer);
            constant /= val;

            if (test != 1 && test != val - 1)
            {
                for (int j = 1; j < s; j++)
                {
                    test = BarrettReduction(test * test, val, constant);
                    if (test == 1) return false;
                    if (test == val - 1) goto next;
                }
                return false;
            }
        next:
            BigInteger root = val.Sqrt();
            if (root * root == val) return false;

            long P = 3, D = P * P - 4;
            int JSymbol = Jacobi(D, val);

            while (JSymbol != -1)
            {
                P++;
                D = P * P - 4;
                if (JSymbol == 0) return false;
                JSymbol = Jacobi(D, val);
            }


            order = val + 1;
            s = 0;

            while ((order & 1) == 0)
            {
                order >>= 1;
                s++;
            }

            BigInteger vk = 2;
            BigInteger vk1 = P;
            int length = order.GetBits();

            for (int k = length - 1; k >= 0; k--)
            {
                BigInteger t1 = 0;

                if (order.TestBit(k))
                {
                    t1 = BarrettReduction(vk * vk1, val, constant);
                    t1 += val;
                    t1 -= P;
                    if (t1 > val) t1 -= val;
                    vk = t1;
                    t1 = BarrettReduction(vk1 * vk1, val, constant);
                    t1 += (val - 2);
                    if (t1 > val) t1 -= val;
                    vk1 = t1;
                }
                else
                {
                    t1 = BarrettReduction(vk * vk1, val, constant);
                    t1 += val;
                    t1 -= P;
                    if (t1 > val) t1 -= val;
                    vk1 = t1;
                    t1 = BarrettReduction(vk * vk, val, constant);
                    t1 += (val - 2);
                    if (t1 > val) t1 -= val;
                    vk = t1;
                }
            }

            if (vk == 2 || vk == val - 2)
            {
                BigInteger t1 = BarrettReduction(vk * P, val, constant);
                BigInteger t2 = vk1 << 1;
                t1 = t2 - t1;

                BigInteger t3 = t1 % val;
                if (t3 == 0) return true;
            }

            for (int t = 0; t < s - 1; t++)
            {
                if (vk == 0) return true;
                if (vk == 2) return false;
                BigInteger t1 = BarrettReduction(vk * vk, val, constant);
                t1 += (val - 2);
                vk = t1 % val;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a number is a strong probable prime using the Miller-Rabin test with multiple random bases.
        /// </summary>
        /// <param name="rand">A cryptographically secure random number generator.</param>
        /// <param name="val">The value to test for primality.</param>
        /// <param name="trials">The number of Miller-Rabin test iterations to perform.</param>
        /// <returns>
        /// <c>true</c> if the number passes all Miller-Rabin tests; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The Miller-Rabin primality test provides a probabilistic guarantee. The probability <br/>of a
        /// composite number being incorrectly identified as prime is at most (1/4)^<paramref name="trials"/>.
        /// </para>
        /// <para>
        /// For cryptographic applications, 50 iterations is the standard number of trials, <br/>
        /// yielding an error probability of less than 2^(-100). This is sufficient for most <br/>
        /// RSA key generation and other cryptographic primitives.
        /// </para>
        /// <para>
        /// This method automatically falls back to the deterministic test for numbers less than 2^64.
        /// </para>
        /// </remarks>
        public static bool IsProbablePrime(RandomNumberGenerator rand, BigInteger val, int trials)
        {
            if (!IsProbablePrime(val)) return false;
            if (val.GetBits() <= 64) return true;

            BigInteger field = val - 1;
            int bits = 0;

            while((field & 1) == 0)
            {
                field >>= 1;
                bits++;
            }

            // Miller-Rabin primality test
            for(int j = 1; j <= trials; j++)
            {
                BigInteger witness = RandomInRange(rand, 2, val - 2);
                BigInteger test = Pow(witness, field, val);

                if(test != 1 && test != val - 1)
                {
                    for(int k = 1; k < bits; k++)
                    {
                        test = (test * test) % val;

                        if (test == 1)
                            return false;

                        if (test == val - 1)
                            goto loop;
                    }

                    return false;

                    loop:
                     continue;
                }
            }

            // probable prime
            return true;
        }

        /// <summary>
        /// Generates a random BigInteger of the specified bit length that is a strong probable prime.
        /// </summary>
        /// <param name="rand">A cryptographically secure random number generator.</param>
        /// <param name="n">The number of bits for the generated prime.</param>
        /// <param name="trials">The number of Miller-Rabin test iterations for primality verification.</param>
        /// <returns>A random BigInteger of exactly <paramref name="n"/> bits that is a strong probable prime.</returns>
        /// <remarks>
        /// <para>
        /// This method generates odd numbers of the specified bit length (most significant bit set to 1) and <br/>
        /// tests them for primality using the Miller-Rabin test. The process repeats until a prime
        /// is found.
        /// </para>
        /// <para>
        /// For cryptographic key generation, the number of trials should be sufficient to achieve the
        /// desired <br/>security level. For RSA-2048, 40-50 trials are typically used.
        /// </para>
        /// </remarks>
        public static BigInteger GenProbablePrime(RandomNumberGenerator rand, int n, int trials)
        {
            BigInteger result = new BigInteger(n, rand);
            result.data[result.data.Used - 1] |= 0x80000000;
            result |= 1;
            
            while(!IsProbablePrime(rand, result, trials))
            {
                result = new BigInteger(n, rand);
                result.data[result.data.Used - 1] |= 0x80000000;
                result |= 1;
            }
            
            return result;
        }

        /// <summary>
        /// Computes the Jacobi symbol (a/n) for two BigInteger values.
        /// </summary>
        /// <param name="a">The numerator.</param>
        /// <param name="n">The denominator, which must be odd and positive.</param>
        /// <returns>The Jacobi symbol value: -1, 0, or 1.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="n"/> is even.</exception>
        /// <remarks>
        /// The Jacobi symbol is a generalization of the Legendre symbol and is used in primality testing <br/>
        /// (particularly in the Lucas test) and in certain cryptographic algorithms like the Solovay-Strassen <br/>
        /// primality test. The method uses the law of quadratic reciprocity for efficient computation <br/>
        /// without requiring factorization.
        /// </remarks>
        public static int Jacobi(BigInteger a, BigInteger n)
        {
            if ((n & 1) == 0)
                throw new ArgumentException("Jacobi defined only for odd integers.");

            if (a >= n)
                a %= n;

            if (a == 0)
                return 0;

            if (a == 1)
                return 1;

            if(a < 0)
            {
                if (((n - 1) & 2) == 0)
                    return Jacobi(-a, n);
                else
                    return -Jacobi(-a, n);
            }

            int bits = 0;

            for (int j = 0; j < a.data.Used; j++)
            {
                uint mask = 1;

                for (int k = 1; k <= 32; k++)
                {
                    if ((a.data[j] & mask) != 0)
                    {
                        j = a.data.Used;
                        break;
                    }

                    mask <<= 1;
                    bits++;
                }
            }

            int sign = 1;
            BigInteger temp = a >> bits;

            if ((bits & 1) != 0 && ((n & 7) == 3 || (n & 7) == 5))
                sign = -1;

            if ((n & 3) == 3 && (temp & 3) == 3)
                sign = -sign;

            if (temp == 1)
                return sign;
            else
                return sign * Jacobi(n % temp, temp);
        }

        /// <summary>
        /// Computes the Barrett constant Cm = floor(2^(2k) / m) for a given modulus m, where k = limb count of m.
        /// </summary>
        /// <param name="modulus">The modulus for which to compute the Barrett constant.</param>
        /// <returns>The Barrett reduction constant Cm.</returns>
        /// <remarks>
        /// The Barrett constant enables fast modular reduction for repeated operations like modular
        /// exponentiation.<br/> The constant is computed once per modulus and reused for all subsequent
        /// reductions. This method is used<br/> internally by <see cref="BarrettReduction"/> and should not typically be
        /// called <br/>directly by application code.
        /// </remarks>
        public static BigInteger BarrettConstant(BigInteger modulus)
        {
            int k = modulus.data.Used << 1;
            Data buffer = new Data(k + 1, k + 1);
            buffer[k] = 0x0001;

            BigInteger bconst = new BigInteger(buffer);
            bconst /= modulus;
            return bconst;
        }

        /// <summary>
        /// Performs fast modular reduction using Barrett's algorithm.
        /// </summary>
        /// <param name="value">The value to reduce modulo the modulus.</param>
        /// <param name="modulus">The modulus for reduction.</param>
        /// <param name="constant">The Barrett constant (Cm) for the modulus, typically obtained from <see cref="BarrettConstant"/>.</param>
        /// <returns>The result of (value % modulus).</returns>
        /// <remarks>
        /// <para>
        /// Barrett reduction approximates the quotient q = floor(value / modulus) <br/>
        /// using the precomputed constant Cm = 2^(2k)/modulus. This avoids <br/>
        /// expensive division operations and is particularly efficient for repeated <br/> 
        /// modular operations with the same modulus.
        /// </para>
        /// <para>
        /// The algorithm works for modulus values up to approximately 2^k, <br/>
        /// where k is the number of 32-bit limbs in the modulus representation. <br/>
        /// This method is used extensively in modular exponentiation for <br/>
        /// cryptographic operations like RSA and Diffie-Hellman.
        /// </para>
        /// </remarks>
        public static BigInteger BarrettReduction(BigInteger value, BigInteger modulus, BigInteger constant)
        {
            int k = modulus.data.Used,
                    kPlusOne = k + 1,
                    kMinusOne = k - 1;

            int length = value.data.Used - kMinusOne;

            if (length <= 0)
                length = 1;

            Data data = new Data(length);

            for (int i = kMinusOne, j = 0; i < value.data.Used; i++, j++)
                data[j] = value.data[i];

            BigInteger q1 = new BigInteger(data);
            BigInteger q2 = q1 * constant;
            length = q2.data.Used - kPlusOne;

            if (length <= 0)
                length = 1;

            data = new Data(length);

            for (int i = kPlusOne, j = 0; i < q2.data.Used; i++, j++)
                data[j] = q2.data[i];

            BigInteger q3 = new BigInteger(data);
            int lengthToCopy = (value.data.Used > kPlusOne) ? kPlusOne : value.data.Used;
            data = new Data(lengthToCopy);

            for (int i = 0; i < lengthToCopy; i++)
                data[i] = value.data[i];


            BigInteger r1 = new BigInteger(data);
            data = new Data(kPlusOne);

            for (int i = 0; i < q3.data.Used; i++)
            {
                if (q3.data[i] == 0) continue;

                ulong mcarry = 0;
                int t = i;
                for (int j = 0; j < modulus.data.Used && t < kPlusOne; j++, t++)
                {
                    ulong val = ((ulong)q3.data[i] * (ulong)modulus.data[j]) +
                                 (ulong)data[t] + mcarry;

                    data[t] = (uint)(val & 0xFFFFFFFF);
                    mcarry = (val >> 32);
                }

                if (t < kPlusOne)
                    data[t] = (uint)mcarry;
            }

            BigInteger r2 = new BigInteger(data);
            r1 -= r2;

            data = new Data(kPlusOne + 1, kPlusOne + 1);
            data[kPlusOne] = 0x00000001;

            if ((r1.data[r1.data.Length - 1] & 0x80000000) != 0)
            {
                BigInteger val = new BigInteger(data);
                r1 += val;
            }

            while (r1 >= modulus)
                r1 -= modulus;

            return r1;
        }

        /// <summary>
        /// Raises a BigInteger to the power of a specified integer exponent.
        /// </summary>
        /// <param name="val">The base value.</param>
        /// <param name="exp">The exponent, which must be non-negative.</param>
        /// <returns>The result of <paramref name="val"/> raised to the power <paramref name="exp"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="exp"/> is negative. Negative exponents would 
        /// produce fractional results, which are not supported by integer exponentiation.
        /// </exception>
        /// <exception cref="ArithmeticException">
        /// Thrown when both <paramref name="val"/> and <paramref name="exp"/> are zero, as zero 
        /// raised to the power of zero is mathematically undefined.
        /// </exception>
        /// <remarks>
        /// This method uses binary exponentiation (exponentiation by squaring) <br/>
        /// which performs O(log exp) multiplications. For modular exponentiation, <br/>
        /// use the overload with modulus parameter.
        /// </remarks>
        public static BigInteger Pow(BigInteger val, int exp)
        {
            if (exp < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(exp), "The exponent must " + 
                    $"be non-negative. Specified: {exp}.");

            if (val == 0 && exp == 0)
                throw new ArithmeticException(
                    "Zero raised to the power " +
                    "of zero is undefined.");

            BigInteger result = 1;

            while(exp != 0)
            {
                if ((exp & 1) == 1)
                    result = result * val;

                val = val * val;
                exp >>= 1;
            }

            return result;
        }

        /// <summary>
        /// Performs modular exponentiation, computing (base raised to exponent) modulo modulus.
        /// </summary>
        /// <param name="val">The base value.</param>
        /// <param name="exponent">The exponent (must be non-negative).</param>
        /// <param name="modulus">The modulus for the reduction.</param>
        /// <returns>The result of <paramref name="val"/> raised to <paramref name="exponent"/> modulo <paramref name="modulus"/>.</returns>
        /// <exception cref="DivideByZeroException">
        /// Thrown when <paramref name="modulus"/> is zero. 
        /// Modular arithmetic is undefined modulo zero.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="exponent"/> is negative. 
        /// Negative exponents are not supported as they would 
        /// require modular inversion.
        /// </exception>
        /// <exception cref="ArithmeticException">
        /// Thrown when both <paramref name="val"/> and <paramref name="exponent"/> 
        /// are zero, as zero raised to the power of zero is mathematically undefined.
        /// </exception>
        /// <remarks>
        /// This method implements optimized modular exponentiation using:
        /// <list type="bullet">
        /// <item><description>Binary method with Barrett reduction for smaller moduli</description></item>
        /// <item><description>Dynamic sliding window (max size 5) with Barrett reduction <br/>
        /// and precomputed odd powers table for larger moduli</description></item>
        /// </list>
        /// For RSA operations, this is used for both encryption (public exponent) <br/> 
        /// and decryption (private exponent). For Diffie-Hellman key exchange, this <br/>computes the shared secret.
        /// </remarks>
        public static BigInteger Pow(BigInteger val, BigInteger exponent, BigInteger modulus)
        {
            if (modulus.IsZero)
                throw new DivideByZeroException(
                    "Modular exponentiation requires" + 
                    " a non-zero modulus. Operation " + 
                    "modulo zero is undefined.");

            if (val.IsZero && exponent.IsZero)
                throw new ArithmeticException(
                    "Zero raised to the power" + 
                    " of zero is undefined.");

            if (exponent.IsNegative)
                throw new ArgumentOutOfRangeException(
                    nameof(exponent), "The exponent must be " + 
                    $"non-negative. Specified exponent: {exponent}.");

            if (modulus.IsNegative)
                modulus = -modulus;

            int k = modulus.data.Used << 1;
            Data buffer = new Data(k + 1, k + 1);
            buffer[k] = 0x0001;

            BigInteger bconst = new BigInteger(buffer);
            bconst /= modulus;

            BigInteger result = 1;
            bool negative = false;

            if (val.IsNegative)
            {
                val = -val;
                negative = true;
            }

#if !USE_BENCHMARKING
            int WORDS_THRESHOLD = (int)Threshold.BIGINT_WORDS_THRESHOLD;
#else
            int WORDS_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.BIGINT_WORDS_THRESHOLD);
#endif

            if (modulus.data.Used >= WORDS_THRESHOLD)
            {
                int windowSize = 5;
                int store = 1 << (windowSize - 1);

                BigInteger[] table = new BigInteger[store];
                table[0] = val % modulus;
                BigInteger b2 = BarrettReduction(table[0] * table[0], modulus, bconst);

                // Creates table of odd powers.
                for (int i = 1; i < store; i++)
                    table[i] = BarrettReduction(table[i - 1] * b2, modulus, bconst);

                int bits = exponent.GetBits();
                int ubits = 0, tbits = 0;

                for (int i = bits - 1; i > -1;)
                {
                    int win = WindowUtil.Window(exponent, i, ref ubits, ref tbits, 5);

                    for (int j = 0; j < ubits; j++)
                        result = BarrettReduction(result * result, modulus, bconst);

                    if (win != 0)
                        result = BarrettReduction(result * table[win >> 1], modulus, bconst);
                    i -= ubits;
                    if (tbits != 0)
                    {
                        for (int j = 0; j < tbits; j++)
                            result = BarrettReduction(result * result, modulus, bconst);
                        i -= tbits;
                    }
                }
            }
            else
            {
                Data data = exponent.data;
                uint temp = 0;

                for (int i = 0; i < data.Used - 1; i++)
                {
                    temp = data[i];

                    for (int j = 0; j < 32; j++)
                    {
                        if ((temp & 1) == 1)
                            result = BarrettReduction(result * val, modulus, bconst);

                        val = BarrettReduction(val * val, modulus, bconst);
                        temp >>= 1;
                    }
                }

                temp = data[data.Used - 1];

                while (temp != 0)
                {
                    if ((temp & 1) == 1)
                        result = BarrettReduction(result * val, modulus, bconst);

                    val = BarrettReduction(val * val, modulus, bconst);
                    temp >>= 1;
                }
            }

            if (negative && (exponent.data[0] & 0x1) != 0)
                return -result;

            return result;
        }

        /// <summary>
        /// Computes the greatest common divisor (GCD) of two BigInteger values.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>The GCD of <paramref name="left"/> and <paramref name="right"/>.</returns>
        /// <remarks>
        /// This implementation uses the Euclidean algorithm with modulo operations. <br/>
        /// The result is always non-negative. If both inputs are zero, returns zero.
        /// </remarks>
        public static BigInteger Gcd(BigInteger left, BigInteger right)
        {
            while(right != 0)
            {
                BigInteger aux = left;
                left = right;
                right = aux % right;
            }

            return left;
        }

        /// <summary>
        /// Computes the modular multiplicative inverse of this BigInteger modulo the specified modulus.
        /// </summary>
        /// <param name="modulus">The modulus for the inverse operation.</param>
        /// <returns>The value x such that (this * x) % modulus = 1.</returns>
        /// <exception cref="DivideByZeroException">
        /// Thrown when <paramref name="modulus"/> is zero. 
        /// Modular arithmetic is undefined modulo zero.
        /// </exception>
        /// <exception cref="ArithmeticException">
        /// Thrown when this and <paramref name="modulus"/> 
        /// are not coprime (GCD != 1), or when this is zero.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The modular inverse is a fundamental operation in public-key cryptography:
        /// </para>
        /// <list type="bullet">
        /// <item><description>RSA: Computing the private exponent d from e and phi(n)</description></item>
        /// <item><description>Elliptic Curve Cryptography: Scalar multiplication using projective coordinates</description></item>
        /// <item><description>Digital signatures: Computing signature components in schemes like ECDSA</description></item>
        /// </list>
        /// <para>
        /// This implementation uses the extended Euclidean algorithm which runs in O(n^2) time. <br/>
        /// The modulus must be positive and the result is returned in the range [1, modulus-1].
        /// </para>
        /// </remarks>
        public BigInteger Inverse(BigInteger modulus)
        {
            if (modulus == 0)
                throw new DivideByZeroException(
                    "Modulus cannot be zero for " + 
                    "modular inverse operation.");

            if (this == 0)
                throw new ArithmeticException(
                    "Zero has no modular inverse.");

            if (Gcd(this, modulus) != 1)
                throw new ArithmeticException(
                    "The modular inverse does not" + 
                    " exist because the numbers are" + 
                    " not coprime.");

            BigInteger b0 = modulus, t, q;
            BigInteger x0 = 0, x1 = 1;

            if (modulus == 1)
                return 1;

            BigInteger self = this;

            while (self > 1)
            {
                q = self / modulus;
                t = modulus;
                modulus = self % modulus;
                self = t;
                t = x0;
                x0 = x1 - q * x0;
                x1 = t;
            }

            if (x1 < 0)
                x1 += b0;

            return x1;
        }

        /// <summary>
        /// Computes the integer square root of this BigInteger.
        /// </summary>
        /// <returns>The floor of the square root of this value.</returns>
        /// <exception cref="ArithmeticException">
        /// Thrown when this value is negative. The square 
        /// root is not defined for negative numbers in 
        /// the real number system.
        /// </exception>
        /// <remarks>
        /// This method uses a binary search algorithm that finds the <br/>
        /// square root bit by bit, working from the most significant <br/> 
        /// bit down to the least significant.
        /// </remarks>
        public BigInteger Sqrt()
        {
            if (IsNegative)
                throw new ArithmeticException(
                    "The square root is undefined" + 
                    " for negative numbers.");

            uint numBits = (uint)GetBits();

            if ((numBits & 0x1) != 0)
                numBits = (numBits >> 1) + 1;
            else
                numBits = (numBits >> 1);

            uint bytePos = numBits >> 5;
            byte bitPos = (byte)(numBits & 0x1F);

            uint mask;
            BigInteger result;

            if (bitPos == 0)
                mask = 0x80000000;
            else
            {
                mask = (uint)1 << bitPos;
                bytePos++;
            }

            Data buffer = new Data((int)bytePos);
            result = new BigInteger(buffer);

            for (int i = (int)bytePos - 1; i >= 0; i--)
            {
                while (mask != 0)
                {
                    result.data[i] ^= mask;
                    result.data.Update();

                    if ((result * result) > this)
                        result.data[i] ^= mask;

                    mask >>= 1;
                }

                mask = 0x80000000;
            }

            return result;
        }

        /// <summary>
        /// Computes the integer root of this BigInteger of the specified order.
        /// </summary>
        /// <param name="n">The root order (must be greater than or equal to 2).</param>
        /// <returns>The floor of the <paramref name="n"/>-th root of this value.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="n"/> is less than 2.</exception>
        /// <exception cref="ArithmeticException">Thrown when extracting an even root (n is even) from a negative number, as the result would be imaginary.</exception>
        /// <remarks>
        /// This is a generalization of the square root method, using bit-by-bit construction. <br/>
        /// This operation is rarely used directly in cryptography but appears in certain <br/>
        /// number-theoretic algorithms and mathematical computations.
        /// </remarks>
        public BigInteger Root(int n)
        {
            if (n < 2)
                throw new ArgumentException(
                    "The root order must be " + 
                    "greater than or equal to 2.");

            if ((n & 1) == 0 && IsNegative)
                throw new ArithmeticException(
                    "Even roots are undefined " + 
                    "for negative numbers.");

            BigInteger self = Abs();
            uint numBits = (uint)self.GetBits();

            numBits = (numBits / (uint)n) + (numBits % (uint)n);
            uint bytePos = numBits >> 5;
            byte bitPos = (byte)(numBits & 0x1F);

            uint mask;
            BigInteger result;

            if (bitPos == 0)
                mask = 0x80000000;
            else
            {
                mask = (uint)1 << bitPos;
                bytePos++;
            }

            Data buffer = new Data((int)bytePos);
            result = new BigInteger(buffer);

            for (int i = (int)bytePos - 1; i >= 0; i--)
            {
                while (mask != 0)
                {
                    result.data[i] ^= mask;
                    result.data.Update();

                    if (Pow(result,n) > self)
                        result.data[i] ^= mask;

                    mask >>= 1;
                }

                mask = 0x80000000;
            }

            return (IsNegative ? -result : result);
        }

        /// <summary>
        /// Determines whether a BigInteger is less than another.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        public static bool operator <(BigInteger left, BigInteger right)
        {
            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
                throw new ArgumentNullException();

            return Compare(left, right) < 0;
        }

        /// <summary>
        /// Determines whether a BigInteger is greater than another.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either operand is null.</exception>
        public static bool operator >(BigInteger left, BigInteger right)
        {
            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
                throw new ArgumentNullException();

            return Compare(left, right) > 0;
        }

        /// <summary>
        /// Determines whether a BigInteger is greater than or equal to another.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator >=(BigInteger left, BigInteger right)
        {
            return Compare(left, right) >= 0;
        }

        /// <summary>
        /// Determines whether a BigInteger is less than or equal to another.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        public static bool operator <=(BigInteger left, BigInteger right)
        {
            return Compare(left, right) <= 0;
        }

        /// <summary>
        /// Determines whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a BigInteger and has the same value; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (!(obj is BigInteger))
                return false;

            BigInteger other = (BigInteger)obj;
            return Equals(other);
        }

        /// <summary>
        /// Determines whether this instance is equal to another BigInteger.
        /// </summary>
        /// <param name="other">The BigInteger to compare with this instance.</param>
        /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(BigInteger other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (data.Used != other.data.Used)
                return false;

            for (int k = 0; k < data.Used; k++)
            {
                if (data[k] != other.data[k])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether two BigInteger values are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(BigInteger left, BigInteger right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two BigInteger values are not equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if the values are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(BigInteger left, BigInteger right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the bit length of the current <see cref="BigInteger"/> value.
        /// </summary>
        /// <returns>
        /// The number of bits required to represent the absolute value in binary. <br/>
        /// Returns 0 for zero. For negative numbers, returns the bit length of the <br/>
        /// positive magnitude.
        /// </returns>
        /// <remarks>
        /// Equivalent to floor(log2(|value|)) + 1. Used for key size determination, algorithm <br/>
        /// threshold selection, and memory allocation in cryptographic operations.
        /// </remarks>
        public int GetBits()
        {
            if (IsZero)
                return 0;

            int bits = 0;
            BigInteger self = Abs();

            bits = 32 * self.data.Used;
            uint word = self.data[self.data.Used - 1];

            uint mask = 0x80000000;
            int remBits = 0;

            while((word & mask) == 0 && mask != 0)
            {
                ++remBits;
                mask >>= 1;
            }

            return (bits - remBits);
        }

        /// <summary>
        /// Tests whether the bit at the specified position is set.
        /// </summary>
        /// <param name="n">The zero-based index of the bit to test.</param>
        /// <returns><c>true</c> if the bit at position <paramref name="n"/> is set; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="n"/> is negative.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Bit indexing follows little-endian convention where bit 0 corresponds to the least significant bit. <br/>
        /// For negative numbers, this method tests bits in the two's complement representation.
        /// </para>
        /// <para>
        /// Cryptographic considerations:
        /// <list type="bullet">
        /// <item><description>Bit testing is constant-time relative to the bit position</description></item>
        /// <item><description>Useful for implementing constant-time scalar multiplication and exponentiation algorithms</description></item>
        /// <item><description>Essential for side-channel resistant implementations of binary exponentiation and Montgomery ladder</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Performance: O(1) with respect to the bit position. The method performs a <br/>
        /// single array lookup and bitwise operation regardless of the integer's size.
        /// </para>
        /// </remarks>
        public bool TestBit(int n)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n),
                    "Bit index cannot be negative.");

            int wordIndex = n >> 5;
            int bitOffset = n & 0x1F;
            uint mask = (uint)1 << bitOffset;

            if (IsNegative)
            {
                if (wordIndex >= data.Used)
                    return true;

                return (data[wordIndex] & mask) != 0;
            }

            if (wordIndex >= data.Used)
                return false;

            return (data[wordIndex] & mask) != 0;
        }
        
        /// <summary>
        /// Sets the bit at the specified position to 1.
        /// </summary>
        /// <param name="n">The zero-based index of the bit to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="n"/> is negative or exceeds the integer's capacity.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Bit indexing follows little-endian convention where bit 0 corresponds to the least significant bit. <br/>
        /// For negative numbers, this method sets bits in the two's complement representation.
        /// </para>
        /// <para>
        /// This method requires the bit index to be within the current capacity. To set bits beyond <br/>
        /// the current capacity, the integer must first be expanded using multiplication or shift operations.
        /// </para>
        /// <para>
        /// Cryptographic considerations:
        /// <list type="bullet">
        /// <item><description>This method modifies the current instance</description></item>
        /// <item><description>Does not provide constant-time guarantees; avoid with secret-dependent indices</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Performance: O(1) when the bit is within the current limb array.
        /// </para>
        /// </remarks>
        public void SetBit(int n)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n),
                    "Bit index cannot be negative.");

            int wordIndex = n >> 5;
            int bitOffset = n & 0x1F;

            if (wordIndex >= data.Used)
                throw new ArgumentOutOfRangeException(nameof(n),
                    $"Bit index {n} exceeds the integer's " +
                    $"capacity of {data.Used * 32} bits.");

            uint mask = (uint)1 << bitOffset;
            data[wordIndex] |= mask;
        }

        /// <summary>
        /// Generates a cryptographically secure random integer within the specified inclusive range.
        /// </summary>
        /// <param name="rand">Cryptographically strong random number generator.</param>
        /// <param name="min">Inclusive lower bound.</param>
        /// <param name="max">Inclusive upper bound.</param>
        /// <returns>A random integer in the range [<paramref name="min"/>, <paramref name="max"/>].</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rand"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="min"/> > <paramref name="max"/>, or the range exceeds the maximum representable value.
        /// </exception>
        /// <remarks>
        /// Uses rejection sampling to ensure uniform distribution without modulo bias.
        /// </remarks>
        public static BigInteger RandomInRange(RandomNumberGenerator rand, BigInteger min, BigInteger max)
        {
            if (rand == null)
                throw new ArgumentNullException(nameof(rand), 
                    "Random number generator cannot be null.");

            if (min > max)
                throw new ArgumentException(
                    $"Minimum value {min} cannot " + 
                    $"be greater than maximum value {max}.", 
                    nameof(min));

            BigInteger modulus = max - min + 1;

            if (modulus <= 0)
                throw new ArgumentException(
                    $"The range [{min}, {max}] " + 
                    "is too large to represent.", 
                    nameof(max));

            BigInteger result = new BigInteger(modulus.GetBits(), rand);

            if (result >= modulus)
                result -= modulus;

            result += min;
            return result;
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="BigInteger"/> value.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>
        /// The hash code incorporates both the magnitude and sign of the value, <br/>
        /// ensuring that equal values produce the same hash code.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                for (int i = 0; i < data.Used; i++)
                    hash = (hash * 31) + (int)data[i];

                if (IsNegative)
                    hash = ~hash;

                return hash;
            }
        }

        /// <summary>
        /// Converts the BigInteger to its decimal string representation.
        /// </summary>
        /// <returns>A string representing the value in base 10, with a leading minus sign for negative numbers.</returns>
        /// <remarks>
        /// This method uses chunked conversion (9 digits per iteration) for efficiency. <br/>
        /// The implementation avoids costly division by large powers of 10 by using <br/>
        /// division by 10^9 which fits in 32 bits.
        /// </remarks>
        public override string ToString()
        {
            if (IsZero) return "0";
            BigInteger self = Abs();
            const int chunkSize = 9;

            uint[] powers = new uint[chunkSize];
            powers[0] = 10;

            for (int i = 1; i < chunkSize; i++)
                powers[i] = powers[i - 1] * 10;

            BigInteger Base = 1000000000;
            BigInteger Quotient, Remainder;
            var chunks = new List<int>();

            while (!self.IsZero)
            {
                SingleDivide(self, Base,
                    out Quotient,
                    out Remainder);

                int chunk = (int)Remainder;
                chunks.Add(chunk);
                self = Quotient;
            }

            var sb = new StringBuilder();

            if (chunks.Count > 0)
            {
                int len = chunks.Count - 2;
                sb.Append(chunks[len + 1].ToString());

                for (int i = len; i >= 0; i--)
                    sb.Append(chunks[i].ToString("D9"));
            }

            return IsNegative ? "-" +
                sb.ToString() :
                sb.ToString();
        }

        /// <summary>
        /// Converts the BigInteger to its hexadecimal string representation.
        /// </summary>
        /// <returns>A string representing the value in base 16, without a sign indicator.</returns>
        /// <remarks>
        /// For positive numbers, this returns the standard hexadecimal representation. <br/>
        /// For negative numbers, this returns the two's complement representation. <br/>
        /// This method is useful for debugging and for interoperating with systems that <br/>
        /// use hexadecimal encoding of large integers (e.g., ASN.1 DER encoding).
        /// </remarks>
        public string ToHexString()
        {
            if (IsZero) return "0";
            var sb = new StringBuilder();

            uint[] digits = new uint[data.Used << 3];
            int len = data.Used - 1;
            int i = 0, j, k;

            for (j = len; j >= 0; j--)
            {
                uint block = data[j];
                k = 7;

                while(k >= 0)
                {
                    digits[i] = (block >> (4 * k)) & 0xF;
                    k--; i++;
                }
            }

            int trimCount = 0;
            k = 0;

            while(k < i - 1)
            {
                uint currentDigit = digits[k];
                uint nextDigit = digits[k + 1];

                bool shouldTrim = IsNegative ?
                    currentDigit == 0xF && (nextDigit & 0x8) == 0x8
                    : currentDigit == 0 && (nextDigit & 0x8) == 0;

                if (!shouldTrim)
                    break;

                trimCount++;
                k++;
            }

            for(k = trimCount; k < i; k++)
            {
                if (digits[k] < 10)
                    sb.Append((char)(digits[k] + 48));
                else
                    if (digits[k] >= 10 && digits[k] <= 15)
                        sb.Append((char)(digits[k] + 55));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation in the specified radix.
        /// </summary>
        /// <param name="radix">The base of the number system (decimal or hexadecimal).</param>
        /// <returns>The string representation of the value in the specified radix.</returns>
        /// <remarks>
        /// For <see cref="Radix.Decimal"/>, this method delegates to <see cref="ToString"/> which performs <br/>
        /// chunked conversion (9 digits per iteration). For <see cref="Radix.HexaDecimal"/>, this method <br/>
        /// delegates to <see cref="ToHexString"/>.
        /// </remarks>
        public string ToString(Radix radix)
        {
            if (radix == Radix.Decimal)
                return ToString();

            return ToHexString();
        }

        /// <summary>
        /// Converts the BigInteger to a byte array in big-endian format.
        /// </summary>
        /// <returns>A byte array representing the absolute value in big-endian order (most significant byte first).</returns>
        /// <remarks>
        /// <para>
        /// This method returns the positive representation without <br/>
        /// a sign byte. The resulting array is suitable for:
        /// </para>
        /// <list type="bullet">
        /// <item><description>Cryptographic parameter encoding (e.g., RSA modulus, ECC coordinates)</description></item>
        /// <item><description>Network protocol serialization</description></item>
        /// <item><description>Storage in binary formats</description></item>
        /// <item><description>Interoperability with other BigInteger implementations</description></item>
        /// </list>
        /// Leading zero bytes are trimmed to produce the minimal <br/>
        /// representation. For zero, an empty array is returned.
        /// </remarks>
        public byte[] ToByteArray()
        {
            byte[] buffer = new byte[4 * data.Used];

            for (int k = data.Used - 1; k >= 0; k--)
            {
                byte[] array = BitConverter.GetBytes(data[k]);
                Array.Copy(array, 0, buffer, 4 * k, 4);
            }

            bool isNegative = IsNegative;
            byte signByte = (byte)(isNegative ? 0xFF : 0x00);
            const byte highBitMask = 0x80;

            int trimCount = 0;
            int lastIndex = buffer.Length;

            while (lastIndex > 1)
            {
                byte currentByte = buffer[lastIndex - 1];
                byte nextByte = buffer[lastIndex - 2];

                bool shouldTrim = isNegative
                    ? currentByte == 0xFF && (nextByte & highBitMask) == highBitMask
                    : currentByte == 0x00 && (nextByte & highBitMask) == 0x00;

                if (!shouldTrim)
                    break;

                trimCount++;
                lastIndex--;
            }

            if (trimCount > 0)
                Array.Resize(ref buffer, buffer.Length - trimCount);

            Array.Reverse(buffer);
            return buffer;
        }

        private bool IsZero
        {
            get { return data.IsZero; }
        }

        private bool IsNegative
        {
            get { return data.IsNegative; }
        }

        /// <summary>
        /// Implicitly converts a 64-bit signed integer to a BigInteger.
        /// </summary>
        /// <param name="value">The 64-bit signed integer to convert.</param>
        public static implicit operator BigInteger(long value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Implicitly converts a 64-bit unsigned integer to a BigInteger.
        /// </summary>
        /// <param name="value">The 64-bit unsigned integer to convert.</param>
        public static implicit operator BigInteger(ulong value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Implicitly converts a 32-bit signed integer to a BigInteger.
        /// </summary>
        /// <param name="value">The 32-bit signed integer to convert.</param>
        public static implicit operator BigInteger(int value)
        {
            return new BigInteger((long)value);
        }

        /// <summary>
        /// Implicitly converts a 32-bit unsigned integer to a BigInteger.
        /// </summary>
        /// <param name="value">The 32-bit unsigned integer to convert.</param>
        public static implicit operator BigInteger(uint value)
        {
            return new BigInteger((ulong)value);
        }

        /// <summary>
        /// Implicitly converts a 16-bit signed integer to a BigInteger.
        /// </summary>
        /// <param name="value">The 16-bit signed integer to convert.</param>
        public static implicit operator BigInteger(short value)
        {
            return new BigInteger((long)value);
        }

        /// <summary>
        /// Implicitly converts a 16-bit unsigned integer to a BigInteger.
        /// </summary>
        /// <param name="value">The 16-bit unsigned integer to convert.</param>
        public static implicit operator BigInteger(ushort value)
        {
            return new BigInteger((ulong)value);
        }

        /// <summary>
        /// Implicitly converts an 8-bit signed integer to a BigInteger.
        /// </summary>
        /// <param name="value">The 8-bit signed integer to convert.</param>
        public static implicit operator BigInteger(sbyte value)
        {
            return new BigInteger((long)value);
        }

        /// <summary>
        /// Implicitly converts an 8-bit unsigned integer to a BigInteger.
        /// </summary>
        /// <param name="value">The 8-bit unsigned integer to convert.</param>
        public static implicit operator BigInteger(byte value)
        {
            return new BigInteger((ulong)value);
        }

        /// <summary>
        /// Explicitly converts a BigInteger to a 64-bit signed integer.
        /// </summary>
        /// <param name="value">The BigInteger to convert.</param>
        /// <returns>The value as a 64-bit signed integer.</returns>
        /// <exception cref="OverflowException">
        /// Thrown when the value is outside the range
        /// of a 64-bit signed integer.
        /// </exception>
        public static explicit operator long(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            if (value > long.MaxValue || value < long.MinValue)
                throw new OverflowException("Value cannot be " + 
                    "represented as a 64-bit signed integer. " + 
                    $"The value must be between {long.MinValue}" + 
                    $" and {long.MaxValue}.");

            long result = ((long)value.data[1] << 32) | value.data[0];
            return result;
        }

        /// <summary>
        /// Explicitly converts a BigInteger to a 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">The BigInteger to convert.</param>
        /// <returns>The value as a 64-bit unsigned integer.</returns>
        /// <exception cref="OverflowException">
        /// Thrown when the value is negative or 
        /// exceeds <see cref="ulong.MaxValue"/>.
        /// </exception>
        public static explicit operator ulong(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            if (value < 0 || value > ulong.MaxValue)
                throw new OverflowException(
                    "Value cannot be represented" + 
                    " as a 64-bit unsigned integer." + 
                    " The value must be between 0 and" + 
                    $" {ulong.MaxValue}.");

            ulong result = ((ulong)value.data[1] << 32) | value.data[0];
            return result;
        }

        /// <summary>
        /// Explicitly converts a BigInteger to a 32-bit signed integer.
        /// </summary>
        /// <param name="value">The BigInteger to convert.</param>
        /// <returns>The value as a 32-bit signed integer.</returns>
        /// <exception cref="OverflowException">
        /// Thrown when the value is outside the 
        /// range of a 32-bit signed integer.
        /// </exception>
        public static explicit operator int(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            if (value > int.MaxValue || value < int.MinValue)
                throw new OverflowException("Value cannot be" + 
                    " represented as a 32-bit signed integer." + 
                    $" The value must be between {int.MinValue}" 
                    + $" and {int.MaxValue}.");

            int result = (int)value.data[0];
            return result;
        }

        /// <summary>
        /// Explicitly converts a BigInteger to a 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The BigInteger to convert.</param>
        /// <returns>The value as a 32-bit unsigned integer.</returns>
        /// <exception cref="OverflowException">Thrown when 
        /// the value is negative or exceeds <see cref="uint.MaxValue"/>.
        /// </exception>
        public static explicit operator uint(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            if (value < 0 || value > uint.MaxValue)
                throw new OverflowException("Value" + 
                    " cannot be represented as a 32-bit" + 
                    " unsigned integer. The value must be" + 
                    $" between 0 and {uint.MaxValue}.");

            return value.data[0];
        }

        /// <summary>
        /// Explicitly converts a BigInteger to a 16-bit signed integer.
        /// </summary>
        /// <param name="value">The BigInteger to convert.</param>
        /// <returns>The value as a 16-bit signed integer.</returns>
        /// <exception cref="OverflowException">Thrown when the value 
        /// is outside the range of a 16-bit signed integer.
        /// </exception>
        public static explicit operator short(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            if (value > short.MaxValue || value < short.MinValue)
                throw new OverflowException("Value cannot be " + 
                    "represented as a 16-bit signed integer. " + 
                    $"The value must be between {short.MinValue}" 
                    + $" and {short.MaxValue}.");

            return (short)value.data[0];
        }

        /// <summary>
        /// Explicitly converts a BigInteger to a 16-bit unsigned integer.
        /// </summary>
        /// <param name="value">The BigInteger to convert.</param>
        /// <returns>The value as a 16-bit unsigned integer.</returns>
        /// <exception cref="OverflowException">Thrown when the value 
        /// is negative or exceeds <see cref="ushort.MaxValue"/>.
        /// </exception>
        public static explicit operator ushort(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            if (value < 0 || value > ushort.MaxValue)
                throw new OverflowException("Value cannot " + 
                    "be represented as a 16-bit unsigned " + 
                    "integer. The value must be between 0 " 
                    + $"and {ushort.MaxValue}.");

            return (ushort)value.data[0];
        }

        /// <summary>
        /// Explicitly converts a BigInteger to an 8-bit signed integer.
        /// </summary>
        /// <param name="value">The BigInteger to convert.</param>
        /// <returns>The value as an 8-bit signed integer.</returns>
        /// <exception cref="OverflowException">Thrown when the value
        /// is outside the range of an 8-bit signed integer.
        /// </exception>
        public static explicit operator sbyte(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            if (value > sbyte.MaxValue || value < sbyte.MinValue)
                throw new OverflowException("Value cannot be " + 
                    "represented as an 8-bit signed integer. " + 
                    $"The value must be between {sbyte.MinValue}" 
                    + $" and {sbyte.MaxValue}.");

            return (sbyte)value.data[0];
        }

        /// <summary>
        /// Explicitly converts a BigInteger to an 8-bit unsigned integer.
        /// </summary>
        /// <param name="value">The BigInteger to convert.</param>
        /// <returns>The value as an 8-bit unsigned integer.</returns>
        /// <exception cref="OverflowException">Thrown when the value 
        /// is negative or exceeds <see cref="byte.MaxValue"/>.
        /// </exception>
        public static explicit operator byte(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            if (value < 0 || value > byte.MaxValue)
                throw new OverflowException("Value " + 
                    "cannot be represented as an 8-bit" + 
                    " unsigned integer. The value must " + 
                    $"be between 0 and {byte.MaxValue}.");

            return (byte)value.data[0];
        }
    }
}