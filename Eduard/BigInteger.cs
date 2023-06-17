using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Eduard
{
    /// <summary>
    /// Represents an arbitrarily large signed integer.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class BigInteger
    {
        private Data data;

        /// <summary>
        /// Create a <seealso cref="BigInteger"/> with an integer value of 0.
        /// </summary>
        public BigInteger()
        {
            data = new Data(1, 1);
        }

        /// <summary>
        /// Creates a <seealso cref="BigInteger"/> using a 64-bit signed integer value.
        /// </summary>
        /// <param name="number"></param>
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
        /// Creates a <seealso cref="BigInteger"/> with an unsigned 64-bit integer value.
        /// </summary>
        /// <param name="number"></param>
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
        /// Creates a <seealso cref="BigInteger"/> in base-10 from the parameter.
        /// </summary>
        /// <param name="digits"></param>
        /// <exception cref="FormatException"></exception>
        public BigInteger(string digits)
        {
            if (!Check(digits, Radix.Decimal))
                throw new FormatException("The format of string is invalid.");

            BuildDecimal(digits);
        }

        /// <summary>
        /// Creates a <seealso cref="BigInteger"/> in base and value from the parameters.
        /// </summary>
        /// <param name="digits"></param>
        /// <param name="radix"></param>
        /// <exception cref="FormatException"></exception>
        public BigInteger(string digits, Radix radix)
        {
            if (!Check(digits, radix))
                throw new FormatException("The format of string is invalid.");

            if (radix == Radix.Decimal)
                BuildDecimal(digits);
            else
                BuildHexaDecimal(digits);
        }

        /// <summary>
        /// Creates a positive <seealso cref="BigInteger"/> initialized from the byte array.
        /// </summary>
        /// <param name="array"></param>
        public BigInteger(byte[] array)
        {
            int length = array.Length >> 2;
            int rem = array.Length & 3;

            if (rem != 0)
                length++;

            data = new Data(length);
            uint digit;

            int h = 0;

            for(int i = array.Length - 1; i >= rem; i -= 4)
            {
                digit = 0;

                for(int j = 0; j < 4; j++)
                {
                    uint val = (uint)array[i - j] << (8 * j);
                    digit |= val;
                }

                data[h++] = digit;
            }

            digit = 0;
            int shift = 0;

            for(int k = rem - 1; k >= 0; k--)
            {
                uint val = (uint)array[k] << shift;
                digit |= val;
                shift += 8;
            }

            if (rem > 0)
                data[h] = digit;

            data.Update();
        }

        /// <summary>
        /// Creates a random positive <seealso cref="BigInteger"/> on a specified number of bits.
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="rand"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public BigInteger(int bits, RandomNumberGenerator rand)
        {
            if (bits <= 0)
                throw new ArgumentException("Number of bits must be greater than 0.");

            if (rand == null)
                throw new NullReferenceException("The generator cannot be null.");

            int bufLen = bits >> 5;
            int remLen = bits & 0x1F;
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

        private BigInteger(Data buffer)
        {
            buffer.Update();
            data = buffer;
        }

        private bool Check(string digits, Radix radix)
        {
            int StartPos = 0;

            if (radix == Radix.Decimal && digits[0] == '-')
                StartPos = 1;

            for(int k = StartPos; k < digits.Length; k++)
            {
                if (radix == Radix.Decimal && (digits[k] < '0' || digits[k] > '9'))
                    return false;

                if (radix == Radix.HexaDecimal && (digits[k] < '0' || digits[k] > 'F' || (digits[k] > '9' && digits[k] < 'A')))
                    return false;
            }

            return true;
        }

        private void BuildDecimal(string digits)
        {
            int Sign = (digits[0] == '-' ? 1 : 0);
            BigInteger result = new BigInteger();
            BigInteger multiply = new BigInteger(1);

            for(int k = digits.Length - 1; k >= Sign; k--)
            {
                int digit = digits[k] - '0';
                result += (multiply * digit);
                multiply *= 10;
            }

            if (Sign == 1)
                result = result.Negate();

            data = result.data;
        }

        private void BuildHexaDecimal(string digits)
        {
            int limit = digits.Length & 7;
            int bufLen = digits.Length >> 3;
            int length = bufLen;

            if (limit != 0)
                length++;

            data = new Data(length);
            int i = 0;
            uint digit;

            for (int j = digits.Length - 1; j >= limit; j -= 8)
            {
                digit = 0;

                for(int k = 0; k < 8; k++)
                {
                    uint val = digits[j - k];

                    if (val >= '0' && val <= '9')
                        val -= 48;
                    else
                        if (val >= 'A' && val <= 'F')
                           val = (val - 'A') + 10;
                    else
                        throw new ArgumentOutOfRangeException();

                    digit |= (val << (4 * k));
                }

                data[i++] = digit;
            }

            digit = 0;
            int shift = 0;

            for(int k = limit - 1; k >= 0; k--)
            {
                uint val = digits[k];

                if (val >= '0' && val <= '9')
                    val -= '0';
                else
                    if (val >= 'A' && val <= 'F')
                    val = (val - 'A') + 10;
                else
                    throw new ArgumentOutOfRangeException();

                digit |= (val << shift);
                shift += 4;
            }

            if (limit != 0)
                data[i] = digit;

            data.Update();
        }

        /// <summary>
        /// Adds the values of two specified <seealso cref="BigInteger"/> objects.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
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
        /// Increments a <seealso cref="BigInteger"/> value by 1.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BigInteger operator ++(BigInteger value)
        {
            return (value + 1);
        }

        /// <summary>
        /// Subtracts a <seealso cref="BigInteger"/> value from another <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
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
        /// Decrements a <seealso cref="BigInteger"/> value by 1.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BigInteger operator --(BigInteger value)
        {
            return (value - 1);
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
            int order = left.data.Used + right.data.Used;
            if(order <= 16) return PlainMultiply(left, right);
            else
            {
                bool Sign = (left.IsNegative != right.IsNegative);
                left = left.Abs();
                right = right.Abs();

                BigInteger result = KMultiply(left, right);
                return Sign ? -result : result;
            }
        }

        private static BigInteger Square(BigInteger val)
        {
            if (2 * val.data.Used <= 16) return PlainSquare(val);
            else
                return KSquare(val);
        }

        /// <summary>
        /// Multiplies two specified <seealso cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigInteger operator *(BigInteger left, BigInteger right)
        {
            if (left == right) return Square(left);
            return Multiply(left, right);
        }

        private static void SingleDivide(BigInteger left, BigInteger right, out BigInteger Quotient, out BigInteger Remainder)
        {
            if (right.IsZero)
                throw new DivideByZeroException();

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

            Remainder = new BigInteger(RemainderData);

            Data QuotientData = new Data(resultPos + 1, resultPos);
            int j = 0;

            for (int i = QuotientData.Used - 1; i >= 0; i--, j++)
                QuotientData[j] = result[i];

            Quotient = new BigInteger(QuotientData);
        }

        private static void MultiDivide(BigInteger left, BigInteger right, out BigInteger Quotient, out BigInteger Remainder)
        {
            if (right.IsZero)
                throw new DivideByZeroException();

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
            Quotient = new BigInteger(new Data(result));

            int n = Data.ShiftRight(remainderDat, d);
            Data buf = new Data(n, n);
            buf.CopyFrom(remainderDat, 0, 0, buf.Used);
            Remainder = new BigInteger(buf);
        }

        private static void Divide(BigInteger left, BigInteger right, out BigInteger Quotient, out BigInteger Remainder)
        {
            if (left.IsZero)
            {
                Quotient = new BigInteger();
                Remainder = new BigInteger();
                return;
            }

            if (right.data.Used == 1)
                SingleDivide(left, right, out Quotient, out Remainder);
            else
                MultiDivide(left, right, out Quotient, out Remainder);
        }

        /// <summary>
        /// Divides a specified <seealso cref="BigInteger"/> value by another specified <seealso cref="BigInteger"/> value by using integer division.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <exception cref="DivideByZeroException"></exception>
        /// <returns></returns>
        public static BigInteger operator /(BigInteger left, BigInteger right)
        {
            if (right.IsZero)
                throw new DivideByZeroException();

            bool Sign = (left.IsNegative != right.IsNegative);

            left = left.Abs();
            right = right.Abs();

            if (left < right)
                return new BigInteger();

            if (left == right)
                return (Sign ? -1 : 1);

            BigInteger Quotient, Remainder;
            Divide(left, right, out Quotient, out Remainder);

            return (Sign ? -Quotient : Quotient);
        }

        /// <summary>
        /// Returns the remainder that results from division with two specified <seealso cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <exception cref="DivideByZeroException"></exception>
        /// <returns></returns>
        public static BigInteger operator %(BigInteger left, BigInteger right)
        {
            if (right.IsZero)
                throw new DivideByZeroException();

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
        /// Negates a specified <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        /// Negates this <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <returns></returns>
        public BigInteger Negate()
        {
            return -this;
        }

        /// <summary>
        /// Returns the absolute value of this.
        /// </summary>
        /// <returns></returns>
        public BigInteger Abs()
        {
            if (IsNegative)
                return -this;

            return this;
        }

        /// <summary>
        /// Performs a bitwise And operation on two <seealso cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigInteger operator &(BigInteger left, BigInteger right)
        {
            int length = Math.Max(left.data.Used, right.data.Used);
            Data buffer = new Data(length, length);

            for (int k = 0; k < length; k++)
                buffer[k] = left.data[k] & right.data[k];

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Performs a bitwise Or operation on two <seealso cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigInteger operator |(BigInteger left, BigInteger right)
        {
            int length = Math.Max(left.data.Used, right.data.Used);
            Data buffer = new Data(length, length);

            for (int k = 0; k < length; k++)
                buffer[k] = left.data[k] | right.data[k];

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Performs a bitwise Xor operation on two <seealso cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigInteger operator ^(BigInteger left, BigInteger right)
        {
            int length = Math.Max(left.data.Used, right.data.Used);
            Data buffer = new Data(length, length);

            for (int k = 0; k < length; k++)
                buffer[k] = left.data[k] ^ right.data[k];

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Returns the bitwise one's complement of a <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static BigInteger operator ~(BigInteger value)
        {
            Data buffer = new Data(value.data.Length);

            for (int k = 0; k < buffer.Length; k++)
                buffer[k] = value.data[k] ^ 0xFFFFFFFF;

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Shifts a <seealso cref="BigInteger"/> value a specified number of bits to the left.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigInteger operator <<(BigInteger left, int right)
        {
            Data buffer = new Data(left.data);
            buffer.Used = buffer.ShiftLeftWithoutOverflow(right);

            return new BigInteger(buffer);
        }

        /// <summary>
        /// Shifts a <seealso cref="BigInteger"/> value a specified number of bits to the right.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static BigInteger operator >>(BigInteger left, int right)
        {
            Data buffer = new Data(left.data);
            buffer.Used = buffer.ShiftRight(right);

            if(left.IsNegative)
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
        /// Compares this instance to a second <seealso cref="BigInteger"/> and returns an integer that indicates whether the value of this instance is less than, equal to, or greater than the value of the specified object.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns></returns>
        public int CompareTo(BigInteger other)
        {
            return Compare(this, other);
        }

        /// <summary>
        /// Compares two <seealso cref="BigInteger"/> values and returns an integer that indicates whether the first value is less than, equal to, or greater than the second value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns></returns>
        public static int Compare(BigInteger left, BigInteger right)
        {
            if (left > right)
                return 1;

            if (left == right)
                return 0;

            return -1;
        }

        /// <summary>
        /// Determines whether a number is almost certainly prime.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
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
        /// Determines whether a number is probable prime.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="val"></param>
        /// <param name="trials"></param>
        /// <returns></returns>
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
                BigInteger witness = Next(rand, 2, val - 2);
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
        /// Generates a random positive <seealso cref="BigInteger"/> with the specified number of bits and is possibly prime.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="bits"></param>
        /// <param name="trials"></param>
        /// <returns></returns>
        public static BigInteger GenProbablePrime(RandomNumberGenerator rand, int bits, int trials)
        {
            BigInteger result = new BigInteger(bits, rand);
            result |= 1;
            
            while(!IsProbablePrime(rand, result, trials))
            {
                result = new BigInteger(bits, rand);
                result |= 1;
            }
            
            return result;
        }

        /// <summary>
        /// Returns the value of Jacobi Symbol for two specified <seealso cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public static int Jacobi(BigInteger left, BigInteger right)
        {
            if ((right & 1) == 0)
                throw new ArgumentException("Jacobi defined only for odd integers.");

            if (left >= right)
                left %= right;

            if (left == 0)
                return 0;

            if (left == 1)
                return 1;

            if(left < 0)
            {
                if (((right - 1) & 2) == 0)
                    return Jacobi(-left, right);
                else
                    return -Jacobi(-left, right);
            }

            int bits = 0;

            for (int j = 0; j < left.data.Used; j++)
            {
                uint mask = 1;

                for (int k = 1; k <= 32; k++)
                {
                    if ((left.data[j] & mask) != 0)
                    {
                        j = left.data.Used;
                        break;
                    }

                    mask <<= 1;
                    bits++;
                }
            }

            int sign = 1;
            BigInteger temp = left >> bits;

            if ((bits & 1) != 0 && ((right & 7) == 3 || (right & 7) == 5))
                sign = -1;

            if ((right & 3) == 3 && (temp & 3) == 3)
                sign = -sign;

            if (temp == 1)
                return sign;
            else
                return sign * Jacobi(right % temp, temp);
        }

        /// <summary>
        /// Returns the Barrett's constant required for fast modular reduction.
        /// </summary>
        /// <param name="Modulus"></param>
        /// <returns></returns>
        public static BigInteger BarrettConstant(BigInteger Modulus)
        {
            int k = Modulus.data.Used << 1;
            Data buffer = new Data(k + 1, k + 1);
            buffer[k] = 0x0001;

            BigInteger Constant = new BigInteger(buffer);
            Constant /= Modulus;
            return Constant;
        }

        /// <summary>
        /// Fast calculation of modular reduction using Barrett's reduction.
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Modulus"></param>
        /// <param name="Constant"></param>
        /// <returns></returns>
        public static BigInteger BarrettReduction(BigInteger Value, BigInteger Modulus, BigInteger Constant)
        {
            int k = Modulus.data.Used,
                    kPlusOne = k + 1,
                    kMinusOne = k - 1;

            int length = Value.data.Used - kMinusOne;

            if (length <= 0)
                length = 1;

            Data data = new Data(length);

            for (int i = kMinusOne, j = 0; i < Value.data.Used; i++, j++)
                data[j] = Value.data[i];

            BigInteger q1 = new BigInteger(data);
            BigInteger q2 = q1 * Constant;
            length = q2.data.Used - kPlusOne;

            if (length <= 0)
                length = 1;

            data = new Data(length);

            for (int i = kPlusOne, j = 0; i < q2.data.Used; i++, j++)
                data[j] = q2.data[i];

            BigInteger q3 = new BigInteger(data);
            int lengthToCopy = (Value.data.Used > kPlusOne) ? kPlusOne : Value.data.Used;
            data = new Data(lengthToCopy);

            for (int i = 0; i < lengthToCopy; i++)
                data[i] = Value.data[i];


            BigInteger r1 = new BigInteger(data);
            data = new Data(kPlusOne);

            for (int i = 0; i < q3.data.Used; i++)
            {
                if (q3.data[i] == 0) continue;

                ulong mcarry = 0;
                int t = i;
                for (int j = 0; j < Modulus.data.Used && t < kPlusOne; j++, t++)
                {
                    ulong val = ((ulong)q3.data[i] * (ulong)Modulus.data[j]) +
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

            while (r1 >= Modulus)
                r1 -= Modulus;

            return r1;
        }

        /// <summary>
        /// Raises a <seealso cref="BigInteger"/> value to the power of a specified value.
        /// </summary>
        /// <param name="Base">The number to raise to the exponent power.</param>
        /// <param name="Exponent">The exponent to raise value by.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArithmeticException"></exception>
        /// <returns></returns>
        public static BigInteger Pow(BigInteger Base, int Exponent)
        {
            if (Exponent < 0)
                throw new ArgumentOutOfRangeException("The exponent must be positive.");

            if (Base == 0 && Exponent == 0)
                throw new ArithmeticException("Arithmetic operation unsupported.");

            BigInteger result = 1;

            while(Exponent != 0)
            {
                if ((Exponent & 1) == 1)
                    result = result * Base;

                Base = Base * Base;
                Exponent >>= 1;
            }

            return result;
        }

        private static int Window(BigInteger val, int i, ref int nbs, ref int nzs, int size)
        {
            int j, r, w;
            w = size;

            nbs = 1;
            nzs = 0;

            if (!val.TestBit(i)) return 0;
            if (i - w + 1 < 0) w = i + 1;

            r = 1;
            for (j = i - 1; j > i - w; j--)
            {
                nbs++;
                r *= 2;
                if (val.TestBit(j)) r += 1;

                if ((r & 0x3) == 0)
                {
                    r >>= 2;
                    nbs -= 2;
                    nzs = 2;
                    break;
                }
            }

            if ((r & 0x1) == 0)
            {
                r >>= 1;
                nzs = 1;
                nbs--;
            }

            return r;
        }

        /// <summary>
        /// Performs modulus division on a number raised to the power of another number.
        /// </summary>
        /// <param name="Base">The number to raise to the exponent power.</param>
        /// <param name="Exponent">The exponent to raise value by.</param>
        /// <param name="Modulus">The number by which to divide value raised to the exponent power.</param>
        /// <exception cref="DivideByZeroException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArithmeticException"></exception>
        /// <returns></returns>
        public static BigInteger Pow(BigInteger Base, BigInteger Exponent, BigInteger Modulus)
        {
            if (Modulus.IsZero)
                throw new DivideByZeroException("Attempted to divide by zero.");

            if (Base.IsZero && Exponent.IsZero)
                throw new ArithmeticException("Arithmetic operation unsupported.");

            if (Exponent.IsNegative)
                throw new ArgumentOutOfRangeException("The exponent must be positive.");

            if (Modulus.IsNegative)
                Modulus = -Modulus;

            int k = Modulus.data.Used << 1;
            Data buffer = new Data(k + 1, k + 1);
            buffer[k] = 0x0001;

            BigInteger Constant = new BigInteger(buffer);
            Constant /= Modulus;

            BigInteger result = 1;
            bool negative = false;

            if (Base.IsNegative)
            {
                Base = -Base;
                negative = true;
            }

            if (Modulus.data.Used >= 16)
            {
                BigInteger[] table = new BigInteger[16];
                table[0] = Base % Modulus;
                BigInteger b2 = BarrettReduction(table[0] * table[0], Modulus, Constant);

                // Creates table of odd powers.
                for (int i = 1; i < 16; i++)
                    table[i] = BarrettReduction(table[i - 1] * b2, Modulus, Constant);

                int bits = Exponent.GetBits();
                int nbw = 0, nzs = 0;

                for (int i = bits - 1; i > -1;)
                {
                    int n = Window(Exponent, i, ref nbw, ref nzs, 5);

                    for (int j = 0; j < nbw; j++)
                        result = BarrettReduction(result * result, Modulus, Constant);

                    if (n != 0)
                        result = BarrettReduction(result * table[n >> 1], Modulus, Constant);
                    i -= nbw;
                    if (nzs != 0)
                    {
                        for (int j = 0; j < nzs; j++)
                            result = BarrettReduction(result * result, Modulus, Constant);
                        i -= nzs;
                    }
                }
            }
            else
            {
                Data data = Exponent.data;
                uint temp = 0;

                for (int i = 0; i < data.Used - 1; i++)
                {
                    temp = data[i];

                    for (int j = 0; j < 32; j++)
                    {
                        if ((temp & 1) == 1)
                            result = BarrettReduction(result * Base, Modulus, Constant);

                        Base = BarrettReduction(Base * Base, Modulus, Constant);
                        temp >>= 1;
                    }
                }

                temp = data[data.Used - 1];

                while (temp != 0)
                {
                    if ((temp & 1) == 1)
                        result = BarrettReduction(result * Base, Modulus, Constant);

                    Base = BarrettReduction(Base * Base, Modulus, Constant);
                    temp >>= 1;
                }
            }

            if (negative && (Exponent.data[0] & 0x1) != 0)
                return -result;

            return result;
        }

        /// <summary>
        /// Finds the greatest common divisor of two <seealso cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns></returns>
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
        ///  Returns the modulo inverse of this.
        /// </summary>
        /// <param name="modulus"></param>
        /// <exception cref="ArithmeticException"></exception>
        /// <returns></returns>
        public BigInteger Inverse(BigInteger modulus)
        {
            if (Gcd(this, modulus) != 1)
                throw new ArithmeticException("The numbers are not coprime.");

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
        /// Returns the square root of this <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <exception cref="ArithmeticException"></exception>
        /// <returns></returns>
        public BigInteger Sqrt()
        {
            if (IsNegative)
                throw new ArithmeticException("Cannot extract square root from negative values.");

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
        /// Returns the root of a specified order from this <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <param name="order"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArithmeticException"></exception>
        /// <returns></returns>
        public BigInteger Root(int order)
        {
            if (order < 2)
                throw new ArgumentException("The order must be greater than or equal with 2.");

            if ((order & 1) == 0 && IsNegative)
                throw new ArithmeticException("Cannot extract root.");

            BigInteger self = Abs();
            uint numBits = (uint)self.GetBits();

            numBits = (numBits / (uint)order) + (numBits % (uint)order);
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

                    if (Pow(result,order) > self)
                        result.data[i] ^= mask;

                    mask >>= 1;
                }

                mask = 0x80000000;
            }

            return (IsNegative ? -result : result);
        }

        /// <summary>
        /// Returns a value that indicates whether a <seealso cref="BigInteger"/> value is less than another <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <(BigInteger left, BigInteger right)
        {
            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
                throw new ArgumentNullException();

            if (left.IsNegative != right.IsNegative)
                return left.IsNegative;

            if (left.data.Used != right.data.Used)
                return (left.data.Used < right.data.Used);

            for (int k = left.data.Used - 1; k >= 0; k--)
            {
                if (left.data[k] != right.data[k])
                    return (left.data[k] < right.data[k]);
            }

            return false;
        }

        /// <summary>
        /// Returns a value that indicates whether a <seealso cref="BigInteger"/> value is greater than another <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >(BigInteger left, BigInteger right)
        {
            if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
                throw new ArgumentNullException();

            if (left.IsNegative != right.IsNegative)
                return right.IsNegative;

            if (left.data.Used != right.data.Used)
                return (left.data.Used > right.data.Used);

            for (int k = left.data.Used - 1; k >= 0; k--)
            {
                if (left.data[k] != right.data[k])
                    return (left.data[k] > right.data[k]);
            }
            return false;
        }

        /// <summary>
        /// Returns a value that indicates whether a <seealso cref="BigInteger"/> value is greater than or equal to another <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator >=(BigInteger left, BigInteger right)
        {
            return (Compare(left, right) >= 0);
        }

        /// <summary>
        /// Returns a value that indicates whether a <seealso cref="BigInteger"/> value is less than or equal to another <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator <=(BigInteger left, BigInteger right)
        {
            return (Compare(left, right) <= 0);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null))
                return false;

            if (object.ReferenceEquals(this, obj))
                return true;

            BigInteger Obj = (BigInteger)obj;

            if (data.Used != Obj.data.Used)
                return false;

            for(int k = 0; k < data.Used; k++)
            {
                if (data[k] != Obj.data[k])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <seealso cref="BigInteger"/> objects are equal.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(BigInteger left, BigInteger right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <seealso cref="BigInteger"/> objects have different values.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(BigInteger left, BigInteger right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the number of bits for this <seealso cref="BigInteger"/> value.
        /// </summary>
        /// <returns></returns>
        public int GetBits()
        {
            int bits = 0;
            BigInteger self = Abs();

            bits = 32 * self.data.Used;
            uint mask = 0x80000000;
            int remBits = 0;

            while((self.data[self.data.Used - 1] & mask) == 0 && mask != 0)
            {
                ++remBits;
                mask >>= 1;
            }

            return (bits - remBits);
        }

        /// <summary>
        /// Returns a boolean value that is the value specified by the selected bit.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool TestBit(int n)
        {
            if (n < 0)
                throw new ArgumentException("The argument is not a valid value.");

            int j = n >> 5;
            int k = n & 0x1F;

            uint mask = (uint)1 << k;
            return (data[j] & mask) != 0;
        }

        /// <summary>
        /// Generates a random <seealso cref="BigInteger"/> in a specified range.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="Min"></param>
        /// <param name="Max"></param>
        /// <returns></returns>
        public static BigInteger Next(RandomNumberGenerator rand, BigInteger Min, BigInteger Max)
        {
            BigInteger modulus = Max - Min + 1;
            BigInteger result = new BigInteger(modulus.GetBits(), rand);

            if (result >= modulus)
                result -= modulus;

            result += Min;

            return result;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer has code.</returns>
        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        /// <summary>
        /// Returns a string representing the <seealso cref="BigInteger"/> in base 10.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IsZero)
                return "0";

            BigInteger self = Abs();
            BigInteger Quotient, Remainder;
            BigInteger Base = new BigInteger(10);
            string result = "";

            while(self.data.Used > 1 || (self.data.Used == 1 && self.data[0] != 0))
            {
                SingleDivide(self, Base, out Quotient, out Remainder);
                char digit = (char)(Remainder.data[0] + 48);
                result = digit + result;
                self = Quotient;
            }

            if (IsNegative)
                return "-" + result;

            return result;
        }

        /// <summary>
        /// Returns a hex string showing the contains of the <seealso cref="BigInteger"/>.
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            if (IsZero)
                return "0";

            string result = "";

            for (int j = 0; j < data.Used - 1; j++)
                result = data[j].ToString("X8") + result;

            result = string.Format("{0:X}", data[data.Used - 1]) + result;

            return result;
        }

        /// <summary>
        /// Converts a <seealso cref="BigInteger"/> value to a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            byte[] buffer = new byte[4 * data.Used];

            for(int k = 0; k < data.Used; k++)
            {
                byte[] array = BitConverter.GetBytes(data[k]);
                Array.Copy(array, 0, buffer, 4 * k, 4);
            }

            int j = buffer.Length;
            int remove = 0;

            while(buffer[j - 1] == 0 && j > 1)
            {
                ++remove;
                j--;
            }

            Array.Resize<byte>(ref buffer, buffer.Length - remove);
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
        /// Creates a <seealso cref="BigInteger"/> from a 64-bit signed integer.
        /// </summary>
        /// <param name="value">A 64-bit signed integer.</param>
        public static implicit operator BigInteger(long value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Creates a <seealso cref="BigInteger"/> from a 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">A 64-bit unsigned integer.</param>
        public static implicit operator BigInteger(ulong value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Creates a <seealso cref="BigInteger"/> from a 32-bit signed integer.
        /// </summary>
        /// <param name="value">A 32-bit signed integer.</param>
        public static implicit operator BigInteger(int value)
        {
            return new BigInteger((long)value);
        }

        /// <summary>
        /// Creates a <seealso cref="BigInteger"/> from a 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">A 32-bit unsigned integer.</param>
        public static implicit operator BigInteger(uint value)
        {
            return new BigInteger((ulong)value);
        }

        /// <summary>
        /// Creates a <seealso cref="BigInteger"/> from a 16-bit signed integer.
        /// </summary>
        /// <param name="value">A 16-bit signed integer.</param>
        public static implicit operator BigInteger(short value)
        {
            return new BigInteger((long)value);
        }

        /// <summary>
        /// Creates a <seealso cref="BigInteger"/> from a 16-bit unsigned integer.
        /// </summary>
        /// <param name="value">A 16-bit unsigned integer.</param>
        public static implicit operator BigInteger(ushort value)
        {
            return new BigInteger((ulong)value);
        }

        /// <summary>
        /// Creates a <seealso cref="BigInteger"/> from a 8-bit signed integer.
        /// </summary>
        /// <param name="value">A 8-bit signed integer.</param>
        public static implicit operator BigInteger(sbyte value)
        {
            return new BigInteger((long)value);
        }

        /// <summary>
        /// Creates a <seealso cref="BigInteger"/> from a 8-bit unsigned integer.
        /// </summary>
        /// <param name="value">A 8-bit unsigned integer.</param>
        public static implicit operator BigInteger(byte value)
        {
            return new BigInteger((ulong)value);
        }

        /// <summary>
        /// Converts a specified <seealso cref="BigInteger"/> value to 64-bit signed integer.
        /// </summary>
        /// <param name="value">A BigInteger.</param>
        public static explicit operator long(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            long result = ((long)value.data[1] << 32) | value.data[0];

            return result;
        }

        /// <summary>
        /// Converts a specified <seealso cref="BigInteger"/> value to 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">A BigInteger.</param>
        public static explicit operator ulong(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            ulong result = ((ulong)value.data[1] << 32) | value.data[0];

            return result;
        }

        /// <summary>
        /// Converts a specified <seealso cref="BigInteger"/> value to 32-bit signed integer.
        /// </summary>
        /// <param name="value">A BigInteger.</param>
        public static explicit operator int(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            int result = (int)value.data[0];

            return result;
        }

        /// <summary>
        /// Converts a specified <seealso cref="BigInteger"/> value to 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">A BigInteger.</param>
        public static explicit operator uint(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            return value.data[0];
        }

        /// <summary>
        /// Converts a specified <seealso cref="BigInteger"/> value to 16-bit signed integer.
        /// </summary>
        /// <param name="value">A BigInteger.</param>
        public static explicit operator short(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            return (short)value.data[0];
        }

        /// <summary>
        /// Converts a specified <seealso cref="BigInteger"/> value to 16-bit unsigned integer.
        /// </summary>
        /// <param name="value">A BigInteger.</param>
        public static explicit operator ushort(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            return (ushort)value.data[0];
        }

        /// <summary>
        /// Converts a specified <seealso cref="BigInteger"/> value to 8-bit signed integer.
        /// </summary>
        /// <param name="value">A BigInteger.</param>
        public static explicit operator sbyte(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            return (sbyte)value.data[0];
        }

        /// <summary>
        /// Converts a specified <seealso cref="BigInteger"/> value to 8-bit unsigned integer.
        /// </summary>
        /// <param name="value">A BigInteger.</param>
        public static explicit operator byte(BigInteger value)
        {
            if (value.IsZero)
                return 0;

            return (byte)value.data[0];
        }
    }
}