using Eduard;
using System.Diagnostics;

namespace Eduard.Security
{
    /// <summary>
    /// Represents an element of a polynomial quotient ring Fp[X]/(m(X)) with automatic reduction.<br/>
    /// Provides arithmetic operations in the ring using the specified modulus polynomial.
    /// </summary>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public struct PolyMod : IEquatable<PolyMod>
    {
        /// <summary>
        /// The underlying polynomial value reduced modulo the ring modulus.
        /// </summary>
        public Polynomial poly;
        private static Polynomial mod;
        private static bool isInitialized;

        static PolyMod()
        {
            isInitialized = false;
            mod = 0;
        }

        /// <summary>
        /// Creates a ring element from a polynomial, automatically reduced modulo the ring modulus.
        /// </summary>
        /// <param name="poly">The polynomial to convert to a ring element.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized via <see cref="SetModulus"/>.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when the ring modulus is zero.
        /// </exception>
        public PolyMod(Polynomial poly)
        {
            if (!isInitialized)
                throw new InvalidOperationException(
                    "Polynomial ring modulus not initialized." 
                    + " Call SetModulus() first.");

            this.poly = poly % mod; 
        }

        /// <summary>
        /// Creates a copy of an existing ring element.
        /// </summary>
        /// <param name="t">The ring element to copy.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized via <see cref="SetModulus"/>.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when the ring modulus is zero.
        /// </exception>
        public PolyMod(PolyMod t) : this(t.poly)
        { }

        /// <summary>
        /// Gets the coefficient at the specified degree from the underlying polynomial.
        /// </summary>
        /// <param name="index">The degree of the term (0 = constant term).</param>
        /// <returns>The coefficient at the specified degree.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when index is negative.</exception>
        public BigInteger GetCoeff(int index)
        { return poly.GetCoeff(index); }

        /// <summary>
        /// Adds two ring elements.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>Sum of the ring elements reduced modulo the modulus.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when modulus polynomial is zero.
        /// </exception>
        public static PolyMod operator +(PolyMod left, PolyMod right)
        {
            if (!isInitialized)
                throw new InvalidOperationException(
                    "Polynomial ring modulus not initialized."
                    + " Call SetModulus() first.");

            Polynomial temp = (left.poly + right.poly) % mod;
            return new PolyMod(temp);
        }

        /// <summary>
        /// Subtracts two ring elements.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>Difference of the ring elements reduced modulo the modulus.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when modulus polynomial is zero.
        /// </exception>
        public static PolyMod operator -(PolyMod left, PolyMod right)
        {
            if (!isInitialized)
                throw new InvalidOperationException(
                    "Polynomial ring modulus not initialized."
                    + " Call SetModulus() first.");

            Polynomial temp = (left.poly - right.poly) % mod;
            return new PolyMod(temp);
        }

        /// <summary>
        /// Multiplies two ring elements using optimized reduction.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>Product of the ring elements reduced modulo the modulus.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when modulus polynomial is zero.
        /// </exception>
        public static PolyMod operator *(PolyMod left, PolyMod right)
        {
            if (!isInitialized)
                throw new InvalidOperationException(
                    "Polynomial ring modulus not initialized."
                    + " Call SetModulus() first.");

            Polynomial temp = Polynomial.Reduce(left.poly * right.poly, mod);
            return new PolyMod(temp);
        }

        /// <summary>
        /// Divides a ring element by a scalar.
        /// </summary>
        /// <param name="left">The ring element.</param>
        /// <param name="right">The scalar divisor.</param>
        /// <returns>The ring element divided by the scalar.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when the scalar divisor is zero or not invertible modulo the field.
        /// </exception>
        public static PolyMod operator /(PolyMod left, BigInteger right)
        {
            if (!isInitialized)
                throw new InvalidOperationException(
                    "Polynomial ring modulus not initialized."
                    + " Call SetModulus() first.");

            Polynomial pol = left.poly / right;
            return new PolyMod(pol);
        }

        /// <summary>
        /// Computes the greatest common divisor of a polynomial with the ring modulus.
        /// </summary>
        /// <param name="poly">The input polynomial.</param>
        /// <returns>Monic GCD(poly, mod).</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized.
        /// </exception>
        public static Polynomial Gcd(Polynomial poly)
        {
            if (!isInitialized)
                throw new InvalidOperationException(
                    "Polynomial ring modulus not initialized."
                    + " Call SetModulus() first.");

            return Polynomial.Gcd(poly, mod); 
        }

        /// <summary>
        /// Composes two ring elements, computing f(g(X)) modulo the ring modulus.
        /// </summary>
        /// <param name="left">The outer polynomial f(X).</param>
        /// <param name="right">The inner polynomial g(X).</param>
        /// <returns>The composition f(g(X)) reduced modulo the ring modulus m(X).</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized via <see cref="SetModulus"/>.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when the ring modulus is zero.
        /// </exception>
        /// <remarks>
        /// Delegates to <see cref="Polynomial.Compose(Polynomial, Polynomial, Polynomial, bool)"/> <br/>
        /// with <c>prepareModulus = false</c> since FFT parameters are precomputed by <br/>
        /// <see cref="SetModulus"/>.
        /// </remarks>
        public static PolyMod Compose(PolyMod left, PolyMod right)
        {
            if (!isInitialized)
                throw new InvalidOperationException(
                    "Polynomial ring modulus not initialized."
                    + " Call SetModulus() first.");

            Polynomial res = Polynomial.Compose(left.poly, 
                right.poly, mod, false);
            return res;
        }

        /// <summary>
        /// Computes modular exponentiation of a ring element.
        /// </summary>
        /// <param name="poly">The base ring element.</param>
        /// <param name="k">The exponent.</param>
        /// <returns>poly^k mod m(X).</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when modulus polynomial is zero.
        /// </exception>
        public static PolyMod Pow(PolyMod poly, BigInteger k)
        {
            if (!isInitialized)
                throw new InvalidOperationException(
                    "Polynomial ring modulus not initialized."
                    + " Call SetModulus() first.");

            Polynomial val = Polynomial.Pow(poly.poly, k, mod);
            PolyMod result = new PolyMod(val);
            return result;
        }

        /// <summary>
        /// Evaluates the ring element as a polynomial at a point x.
        /// </summary>
        /// <param name="x">The point to evaluate at.</param>
        /// <returns>poly(x) mod field.</returns>
        public BigInteger F(BigInteger x)
        { return poly.Horner(x); }

        /// <summary>
        /// Sets the modulus polynomial for the ring Fp[X]/(m(X)).
        /// Must be called before any ring operations.
        /// </summary>
        /// <param name="modulus">The modulus polynomial m(X) (must be non-zero).</param>
        /// <exception cref="DivideByZeroException">
        /// Thrown when modulus polynomial is zero.
        /// </exception>
        /// <remarks>
        /// Precomputes FFT parameters for efficient reduction when applicable.<br/>
        /// The modulus should typically be irreducible for field applications,<br/>
        /// but any non-zero polynomial is accepted for general quotient rings.
        /// </remarks>
        public static void SetModulus(Polynomial modulus)
        {
            Polynomial.SetPolyMod(modulus);
            isInitialized = true;
            mod = modulus;
        }

        /// <summary>
        /// Implicitly converts an integer to a constant ring element.
        /// </summary>
        /// <param name="val">The integer value.</param>
        /// <returns>A constant ring element representing the integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when modulus polynomial is zero.
        /// </exception>
        public static implicit operator PolyMod(int val)
        {
            Polynomial pol = val;
            return new PolyMod(pol);
        }

        /// <summary>
        /// Checks equality with another ring element.
        /// </summary>
        /// <param name="other">The ring element to compare.</param>
        /// <returns>true if the underlying polynomials are equal; otherwise false.</returns>
        public bool Equals(PolyMod other)
        { return poly.Equals(other.poly); }

        /// <summary>
        /// Determines whether the specified object is equal to the current ring element.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>true if the object is a PolyMod with equal polynomial; otherwise false.</returns>
        public override bool Equals(object obj)
        { return poly.Equals(obj); }

        /// <summary>
        /// Returns the hash code for this ring element.
        /// </summary>
        /// <returns>A hash code based on the underlying polynomial.</returns>
        public override int GetHashCode()
        { return poly.GetHashCode(); }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>true if the ring elements are equal; otherwise false.</returns>
        public static bool operator ==(PolyMod left, PolyMod right)
        { return (left.poly == right.poly); }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>true if the ring elements are not equal; otherwise false.</returns>
        public static bool operator !=(PolyMod left, PolyMod right)
        { return (left.poly != right.poly); }

        /// <summary>
        /// Implicitly converts a big integer to a constant ring element.
        /// </summary>
        /// <param name="val">The big integer value.</param>
        /// <returns>A constant ring element representing the big integer.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when modulus polynomial is zero.
        /// </exception>
        public static implicit operator PolyMod(BigInteger val)
        {
            Polynomial pol = val;
            return new PolyMod(pol);
        }

        /// <summary>
        /// Implicitly converts a polynomial to a ring element (automatically reduced).
        /// </summary>
        /// <param name="poly">The polynomial to convert.</param>
        /// <returns>A ring element representing the polynomial reduced modulo the modulus.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the ring modulus has not been initialized.
        /// </exception>
        /// <exception cref="DivideByZeroException">
        /// Thrown when modulus polynomial is zero.
        /// </exception>
        public static implicit operator PolyMod(Polynomial poly)
        { return new PolyMod(poly); }
    }
}
