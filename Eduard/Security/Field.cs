using System;

namespace Eduard.Security
{
    /// <summary>
    /// Represents an element of a prime field Fp with automatic Barrett reduction caching.<br/>
    /// Provides efficient arithmetic operations using pre-computed Barrett constants.
    /// </summary>
    /// <remarks>
    /// This struct implements field arithmetic for cryptographic operations.<br/> The modulus
    /// must be set via <see cref="BarrettReducer.SetModulus"/><br/>
    /// or <see cref="Polynomial.SetField"/> before any field operations.<br/> All arithmetic
    /// is performed in constant-time where possible to <br/>prevent side-channel attacks.
    /// </remarks>
    public struct Field : IEquatable<Field>
    {
        /// <summary>
        /// The field element value normalized to [0, p-1].
        /// </summary>
        public BigInteger fn;

        /// <summary>
        /// Creates a field element from a value, automatically reduced modulo p.
        /// </summary>
        /// <param name="val">Integer value to convert to field element.</param>
        public Field(BigInteger val)
        {
            BigInteger field = BarrettReducer.GetModulus();
            fn = val % field;

            if (fn < 0) 
                fn += field;
        }

        /// <summary>
        /// Computes b^k in the field using modular exponentiation.
        /// </summary>
        /// <param name="b">Base element.</param>
        /// <param name="k">Exponent.</param>
        /// <returns>b^k mod p.</returns>
        public static Field Pow(Field b, BigInteger k)
        {
            BigInteger field = BarrettReducer.GetModulus();
            return BigInteger.Pow(b.fn, k, field);
        }

        /// <summary>
        /// Adds two field elements.
        /// </summary>
        public static Field operator +(Field left, Field right)
        {
            BigInteger sum = BarrettReducer.AddMod(left.fn, right.fn);
            return sum;
        }

        /// <summary>
        /// Subtracts two field elements.
        /// </summary>
        public static Field operator -(Field left, Field right)
        {
            BigInteger diff = BarrettReducer.SubMod(left.fn, right.fn);
            return diff;
        }

        /// <summary>
        /// Negates a field element.
        /// </summary>
        public static Field operator -(Field val)
        {
            BigInteger field = BarrettReducer.GetModulus();
            BigInteger fn = field - val.fn;
            return new Field(fn);
        }

        /// <summary>
        /// Multiplies two field elements using Barrett reduction.
        /// </summary>
        public static Field operator *(Field left, Field right)
        {
            BigInteger val = BarrettReducer.MultMod(left.fn, right.fn);
            return val;
        }

        /// <summary>
        /// Divides two field elements (left * (1 / right)).
        /// </summary>
        public static Field operator /(Field left, Field right)
        {
            BigInteger field = BarrettReducer.GetModulus();
            BigInteger inv = BarrettReducer.InvMod(right.fn);
            BigInteger val = BarrettReducer.MultMod(inv, left.fn);
            return val;
        }

        /// <summary>
        /// Computes square root of a field element.
        /// </summary>
        public static Field Sqrt(Field val)
        {
            BigInteger root = ModSqrtUtil.Sqrt((BigInteger)val, true);
            return root;
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        public static bool operator ==(Field left, Field right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        public static bool operator !=(Field left, Field right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Checks equality with another field element.
        /// </summary>
        public bool Equals(Field other)
        {
            return (fn == other.fn);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current field element.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is Field))
                return false;

            Field other = (Field)obj;
            return Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this field element.
        /// </summary>
        public override int GetHashCode()
        {
            return fn.GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of this field element.
        /// </summary>
        public override string ToString()
        {
            return fn.ToString();
        }

        /// <summary>
        /// Implicit conversion from int.
        /// </summary>
        public static implicit operator Field(int val)
        {
            return new Field(val);
        }

        /// <summary>
        /// Implicit conversion from uint.
        /// </summary>
        public static implicit operator Field(uint val)
        {
            return new Field(val);
        }

        /// <summary>
        /// Implicit conversion from long.
        /// </summary>
        public static implicit operator Field(long val)
        {
            return new Field(val);
        }

        /// <summary>
        /// Implicit conversion from ulong.
        /// </summary>
        public static implicit operator Field(ulong val)
        {
            return new Field(val);
        }

        /// <summary>
        /// Implicit conversion from BigInteger.
        /// </summary>
        public static implicit operator Field(BigInteger val)
        {
            return new Field(val);
        }

        /// <summary>
        /// Explicit conversion to BigInteger.
        /// </summary>
        public static explicit operator BigInteger(Field field)
        {
            return field.fn;
        }
    }
}
