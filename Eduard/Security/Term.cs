using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Eduard.Security
{
    /// <summary>
    /// Represents a single term in a bivariate polynomial.
    /// </summary>
    internal struct Term : IEquatable<Term>
    {
        /// <summary>
        /// The coefficient of this term (modular integer).
        /// </summary>
        public BigInteger coeff;

        /// <summary>
        /// The exponent (degree) in X.
        /// </summary>
        public int degx;

        /// <summary>
        /// The exponent (degree) in Y.
        /// </summary>
        public int degy;

        /// <summary>
        /// Initializes a new term with specified coefficient and degrees.
        /// </summary>
        /// <param name="coeff">The coefficient.</param>
        /// <param name="degx">The degree in X.</param>
        /// <param name="degy">The degree in Y.</param>
        public Term(BigInteger coeff, int degx, int degy)
        {
            this.coeff = coeff;
            this.degx = degx;
            this.degy = degy;
        }

        /// <summary>
        /// Initializes a new term by copying an existing term.
        /// </summary>
        /// <param name="other">The term to copy.</param>
        public Term(Term other)
        {
            coeff = other.coeff;
            degx = other.degx;
            degy = other.degy;
        }

        /// <summary>
        /// Determines whether two term instances are equal.
        /// </summary>
        /// <param name="left">The first term to compare.</param>
        /// <param name="right">The second term to compare.</param>
        /// <returns><c>true</c> if the terms have identical coefficient and degrees; otherwise <c>false</c>.</returns>
        public static bool operator ==(Term left, Term right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two term instances are not equal.
        /// </summary>
        /// <param name="left">The first term to compare.</param>
        /// <param name="right">The second term to compare.</param>
        /// <returns><c>true</c> if the terms differ in coefficient or degrees; otherwise <c>false</c>.</returns>
        public static bool operator !=(Term left, Term right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current term.
        /// </summary>
        /// <param name="obj">The object to compare with the current term.</param>
        /// <returns><c>true</c> if the specified object is a Term with identical values; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Term))
                return false;

            Term other = (Term)obj;
            return Equals(other);
        }

        /// <summary>
        /// Determines whether the current term is equal to another term.
        /// </summary>
        /// <param name="other">The term to compare with the current term.</param>
        /// <returns><c>true</c> if the terms have identical coefficient and degrees; otherwise <c>false</c>.</returns>
        public bool Equals(Term other)
        {
            return coeff == other.coeff
                && degx == other.degx
                && degy == other.degy;
        }

        /// <summary>
        /// Returns a hash code for this term instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                coeff, degx, degy);
        }
    }
}