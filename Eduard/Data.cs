using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Eduard
{
    [DebuggerStepThrough]
    internal struct Data
    {
        private uint[] data;
        private int used;

        internal Data(int length)
        {
            data = new uint[length + 1];
            used = 0;
        }

        internal Data(int length, int used)
        {
            data = new uint[length + 1];
            this.used = used;
        }

        internal Data(uint[] array)
        {
            data = new uint[array.Length + 1];
            used = 0;
            CopyFrom(array, 0, 0, array.Length);
            Update();
        }

        internal Data(Data Source)
        {
            data = new uint[Source.Length + 1];
            used = Source.Used;
            Array.Copy(Source.data, 0, data, 0, Source.Length);
        }

        internal void CopyFrom(uint[] source, int sourceOffset, int offset, int length)
        {
            Array.Copy(source, sourceOffset, data, 0, length);
        }

        internal void CopyTo(uint[] array, int offset, int length)
        {
            Array.Copy(data, 0, array, offset, length);
        }

        internal void Update()
        {
            used = data.Length;
            uint mask = (IsNegative ? 0xFFFFFFFF : 0);

            while (used > 1 && data[used - 1] == mask)
                --used;

            if (IsNegative)
                used++;

            if (used == 0)
                used = 1;
        }

        internal int ShiftRight(int bits)
        {
            return ShiftRight(data, bits);
        }

        internal static int ShiftRight(uint[] buffer, int bits)
        {
            int shiftAmount = 32;
            int invShift = 0;
            int bufLen = buffer.Length;

            while (bufLen > 1 && buffer[bufLen - 1] == 0)
                bufLen--;

            for (int count = bits; count > 0; count -= shiftAmount)
            {
                if (count < shiftAmount)
                {
                    shiftAmount = count;
                    invShift = 32 - shiftAmount;
                }

                ulong carry = 0;
                for (int i = bufLen - 1; i >= 0; i--)
                {
                    ulong val = ((ulong)buffer[i]) >> shiftAmount;
                    val |= carry;

                    carry = ((ulong)buffer[i]) << invShift;
                    buffer[i] = (uint)val;
                }
            }

            while (bufLen > 1 && buffer[bufLen - 1] == 0)
                bufLen--;

            return bufLen;
        }

        internal int ShiftLeft(int bits)
        {
            return ShiftLeft(data, bits);
        }

        internal static int ShiftLeft(uint[] buffer, int bits)
        {
            int shiftAmount = 32;
            int bufLen = buffer.Length;

            while (bufLen > 1 && buffer[bufLen - 1] == 0)
                bufLen--;

            for (int count = bits; count > 0; count -= shiftAmount)
            {
                if (count < shiftAmount)
                    shiftAmount = count;

                ulong carry = 0;

                for (int i = 0; i < bufLen; i++)
                {
                    ulong val = ((ulong)buffer[i]) << shiftAmount;
                    val |= carry;

                    buffer[i] = (uint)(val & 0xFFFFFFFF);
                    carry = val >> 32;
                }

                if (carry != 0)
                {
                    if (bufLen + 1 <= buffer.Length)
                    {
                        buffer[bufLen] = (uint)carry;
                        bufLen++;
                        carry = 0;
                    }
                    else throw new OverflowException();
                }
            }

            return bufLen;
        }

        internal int ShiftLeftWithoutOverflow(int bits)
        {
            List<uint> temporary = new List<uint>(data);
            int shiftAmount = 32;

            for (int count = bits; count > 0; count -= shiftAmount)
            {
                if (count < shiftAmount)
                    shiftAmount = count;

                ulong carry = 0;

                for (int i = 0; i < temporary.Count; i++)
                {
                    ulong val = ((ulong)temporary[i]) << shiftAmount;
                    val |= carry;

                    temporary[i] = (uint)(val & 0xFFFFFFFF);
                    carry = val >> 32;
                }

                if (carry != 0)
                {
                    temporary.Add(0);
                    temporary[temporary.Count - 1] = (uint)carry;
                }
            }

            data = new uint[temporary.Count];
            temporary.CopyTo(data);

            return data.Length;
        }

        internal uint this[int index]
        {
            get
            {
                if (index >= 0 && index < data.Length)
                    return data[index];
                else
                    if (index < 0)
                    throw new IndexOutOfRangeException("Index cannot be negative.");
                else
                    return (IsNegative ? 0xFFFFFFFF : 0);
            }
            set
            {
                if (index >= 0 && index < data.Length)
                    data[index] = value;
                else
                    throw new IndexOutOfRangeException("Bad index.");
            }
        }

        internal int Length
        {
            get { return data.Length; }
        }

        internal int Used
        {
            get { return used; }
            set { used = value; }
        }

        internal bool IsNegative
        {
            get { return (data[data.Length - 1] & 0x80000000) == 0x80000000; }
        }

        internal bool IsZero
        {
            get { return (used == 0 || (used == 1 && data[0] == 0)); }
        }
    }
}
