using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography
{
    public class Field
    {
        public BigInteger fn;
        static BigInteger field;
        static BigInteger brc;

        public Field(BigInteger val)
        {
            fn = val % field;
            if (fn < 0) fn += field;
        }

        public static Field Pow(Field b, BigInteger k)
        {
            return BigInteger.Pow(b.fn, k, field);
        }

        public static void modulo(BigInteger mod)
        {
            field = mod;
            brc = BigInteger.BarrettConstant(mod);
        }

        public static Field operator +(Field left, Field right)
        {
            BigInteger sum = left.fn + right.fn;
            if (sum >= field) sum -= field;
            return sum;
        }

        public static Field operator -(Field left, Field right)
        {
            BigInteger sum = left.fn - right.fn;
            if (sum < 0) sum += field;
            return sum;
        }

        public static Field operator -(Field val)
        {
            BigInteger fn = field - val.fn;
            return new Field(fn);
        }

        public static Field operator *(Field left, Field right)
        {
            BigInteger val = left.fn * right.fn;
            val = BigInteger.BarrettReduction(val, field, brc);
            return val;
        }

        public static Field operator /(Field left, Field right)
        {
            BigInteger inv = right.fn.Inverse(field);
            BigInteger val = BigInteger.BarrettReduction(inv * left.fn, field, brc);
            return val;
        }

        public static Field Sqrt(Field val)
        {
            BigInteger root = Polynomial.Sqrt((BigInteger)val, true);
            return root;
        }

        public static bool operator ==(Field left, Field right)
        {
            return (left.fn == right.fn);
        }

        public static bool operator !=(Field left, Field right)
        {
            return (left.fn != right.fn);
        }

        public static implicit operator Field(int val)
        {
            return new Field(val);
        }

        public static implicit operator Field(uint val)
        {
            return new Field(val);
        }

        public static implicit operator Field(long val)
        {
            return new Field(val);
        }

        public static implicit operator Field(ulong val)
        {
            return new Field(val);
        }

        public static implicit operator Field(BigInteger val)
        {
            return new Field(val);
        }

        public static explicit operator BigInteger(Field field)
        {
            return field.fn;
        }
    }
}
