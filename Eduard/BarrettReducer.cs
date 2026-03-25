using System;

namespace Eduard
{
    /// <summary>
    /// Provides optimized modular reduction using Barrett's algorithm with modulus caching.
    /// </summary>
    internal class BarrettReducer
    {
        /// <summary>
        /// Cached modulus from last operation.
        /// </summary>
        private static BigInteger field;

        /// <summary>
        /// Pre-computed Barrett constant μ = ⌊2^(2k)/mod⌋.
        /// </summary>
        private static BigInteger k;
        private static bool isEnabled;

        static BarrettReducer()
        {
            isEnabled = false;
            field = k = 0;
        }

        /// <summary>
        /// Initializes the reducer with a new modulus and pre-computes its Barrett constant.
        /// </summary>
        /// <param name="p">The modulus for subsequent reductions.</param>
        internal static void SetModulus(BigInteger p)
        {
            k = BigInteger.BarrettConstant(p);
            isEnabled = true; field = p;
        }

        /// <summary>
        /// Retrieves the currently cached modulus.
        /// </summary>
        /// <returns>The modulus currently used for Barrett reduction.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no modulus has been initialized via <see cref="SetModulus"/> or <see cref="Reduce"/>.
        /// </exception>
        /// <remarks>
        /// Call <see cref="SetModulus"/> or <see cref="Reduce"/> before invoking this method.
        /// </remarks>
        internal static BigInteger GetModulus()
        {
            if (!isEnabled)
                throw new InvalidOperationException(
                    "Barrett reducer modulus not initialized." 
                    + " Call SetModulus() or Reduce() first.");

            return field;
        }

        /// <summary>
        /// Adds two integers modulo the cached field.
        /// </summary>
        /// <param name="left">First operand.</param>
        /// <param name="right">Second operand.</param>
        /// <returns>(left + right) mod field.</returns>
        /// <exception cref="InvalidOperationException">Thrown when modulus not initialized.</exception>
        /// <remarks>
        /// Performs addition followed by conditional subtraction when result exceeds modulus.<br/>
        /// Assumes both inputs are already reduced in [0, field-1].
        /// </remarks>
        internal static BigInteger AddMod(BigInteger left, BigInteger right)
        {
            if (!isEnabled)
                throw new InvalidOperationException(
                    "Barrett reducer modulus not initialized."
                    + " Call SetModulus() or Reduce() first.");

            BigInteger result = left + right;

            if (result >= field)
                result -= field;

            return result;
        }

        /// <summary>
        /// Subtracts two integers modulo the cached field.
        /// </summary>
        /// <param name="left">First operand.</param>
        /// <param name="right">Second operand.</param>
        /// <returns>(left - right) mod field.</returns>
        /// <exception cref="InvalidOperationException">Thrown when modulus not initialized.</exception>
        /// <remarks>
        /// Performs subtraction followed by conditional addition when result is negative.<br/>
        /// Assumes both inputs are already reduced in [0, field-1].
        /// </remarks>
        internal static BigInteger SubMod(BigInteger left, BigInteger right)
        {
            if (!isEnabled)
                throw new InvalidOperationException(
                    "Barrett reducer modulus not initialized."
                    + " Call SetModulus() or Reduce() first.");

            BigInteger result = left - right;

            if (result < 0)
                result += field;

            return result;
        }

        /// <summary>
        /// Multiplies two integers modulo the cached field using Barrett reduction.
        /// </summary>
        /// <param name="left">First operand.</param>
        /// <param name="right">Second operand.</param>
        /// <returns>(left * right) mod field.</returns>
        /// <exception cref="InvalidOperationException">Thrown when modulus not initialized.</exception>
        /// <remarks>
        /// Performs multiplication followed by Barrett reduction. More efficient than<br/>
        /// using <see cref="Reduce"/> separately when both<br/> operands are already reduced.
        /// </remarks>
        internal static BigInteger MulMod(BigInteger left, BigInteger right)
        {
            if (!isEnabled)
                throw new InvalidOperationException(
                    "Barrett reducer modulus not initialized."
                    + " Call SetModulus() or Reduce() first.");

            BigInteger result = Reduce(left * right, field);
            return result;
        }

        /// <summary>
        /// Computes the modular inverse of a value modulo the cached field.
        /// </summary>
        /// <param name="val">The value to invert.</param>
        /// <returns>The modular inverse x such that (val * x) % field = 1.</returns>
        /// <exception cref="InvalidOperationException">Thrown when modulus not initialized.</exception>
        /// <exception cref="ArithmeticException">Thrown when val has no modular inverse (non-coprime).</exception>
        /// <remarks>
        /// Uses the extended Euclidean algorithm. For prime fields, every non-zero value <br/>
        /// has a unique inverse. Essential for projective to affine conversion, point <br/>
        /// addition slopes, and polynomial arithmetic.
        /// </remarks>
        internal static BigInteger InvMod(BigInteger val)
        {
            if (!isEnabled)
                throw new InvalidOperationException(
                    "Barrett reducer modulus not initialized."
                    + " Call SetModulus() or Reduce() first.");

            BigInteger result = val.Inverse(field);
            return result;
        }

        /// <summary>
        /// Reduces a value modulo the specified field using Barrett's algorithm. <br/>
        /// Automatically caches the modulus on first use for subsequent calls.
        /// </summary>
        /// <param name="val">The value to reduce.</param>
        /// <param name="p">The modulus. Cached after first invocation.</param>
        /// <returns>val mod p in range [0, p-1].</returns>
        internal static BigInteger Reduce(BigInteger val, BigInteger p)
        {
            if (!isEnabled || (isEnabled && field != p)) SetModulus(p);
            return BigInteger.BarrettReduction(val, p, k);
        }
    }
}
