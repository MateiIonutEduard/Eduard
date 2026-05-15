using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eduard.Security
{
    /// <summary>
    /// Represents bivariate polynomials with coefficients in a prime field.
    /// </summary>
    /// <remarks>
    /// All polynomial operations are performed modulo a prime field set via SetField(). <br/>
    /// Supports addition, subtraction, multiplication, division, modulus, differentiation, <br/>
    /// and evaluation. Terms are stored in lexicographic order (descending by X, then Y).
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public struct BivariatePolynomial : IEquatable<BivariatePolynomial>
    {
        private List<Term> terms;

        /// <summary>
        /// Initializes a new bivariate polynomial with a constant term.
        /// </summary>
        /// <param name="value">The constant value.</param>
        public BivariatePolynomial(BigInteger value)
        {
            terms = new List<Term>();
            AddTerm(value, 0, 0);
        }

        /// <summary>
        /// Initializes a new bivariate polynomial with a single XY term.
        /// </summary>
        /// <param name="coeff">The coefficient.</param>
        /// <param name="degx">The degree in X.</param>
        /// <param name="degy">The degree in Y.</param>
        public BivariatePolynomial(BigInteger coeff, int degx, int degy)
        {
            terms = new List<Term>();
            AddTerm(coeff, degx, degy);
        }

        /// <summary>
        /// Initializes a new bivariate polynomial from a univariate polynomial in X.
        /// </summary>
        /// <param name="poly">The univariate polynomial (terms are treated as X^i).</param>
        public BivariatePolynomial(Polynomial poly)
        {
            terms = new List<Term>();

            for (int i = poly.degree; i >= 0; i--)
                AddTerm(poly.coeffs[i], i, 0, false);
        }

        /// <summary>
        /// Initializes a new bivariate polynomial by copying an existing one.
        /// </summary>
        /// <param name="other">The bivariate polynomial to copy.</param>
        /// <remarks>
        /// Creates a deep copy where modifications to the new instance <br/>
        /// do not affect the original polynomial and vice versa.
        /// </remarks>
        public BivariatePolynomial(BivariatePolynomial other)
        {
            int len = other.terms.Count;
            terms = new List<Term>();
            int k;

            for(k = 0; k < len; k++)
            {
                Term newTerm = new Term(other.terms[k]);
                AddTerm(newTerm.coeff, newTerm.degx, 
                    newTerm.degy, false);
            }
        }

        /// <summary>
        /// Gets the zero polynomial.
        /// </summary>
        public static BivariatePolynomial Zero
        {
            get
            {
                var poly = new BivariatePolynomial(0);
                return poly;
            }
        }

        /// <summary>
        /// Gets the constant one polynomial.
        /// </summary>
        public static BivariatePolynomial One
        {
            get
            {
                var poly = new BivariatePolynomial(1);
                return poly;
            }
        }

        /// <summary>
        /// Adds two bivariate polynomials over the current finite field.
        /// </summary>
        /// <param name="left">The first polynomial.</param>
        /// <param name="right">The second polynomial.</param>
        /// <returns>The sum polynomial reduced modulo the field.</returns>
        public static BivariatePolynomial operator +(BivariatePolynomial left, BivariatePolynomial right)
        {
            if (left.IsZero && right.IsZero)
                return Zero;

            var res = new BivariatePolynomial(left);
            int termsCount = right.terms.Count;
            int k;

            for (k = 0; k < termsCount; k++)
            {
                Term term = right.terms[k];
                BigInteger coeff = term.coeff;
                int degx = term.degx;

                int degy = term.degy;
                res.AddTerm(coeff, degx, 
                    degy, false);
            }

            return res;
        }

        /// <summary>
        /// Subtracts one bivariate polynomial from another over the current finite field.
        /// </summary>
        /// <param name="left">The minuend polynomial.</param>
        /// <param name="right">The subtrahend polynomial.</param>
        /// <returns>The difference polynomial reduced modulo the field.</returns>
        public static BivariatePolynomial operator -(BivariatePolynomial left, BivariatePolynomial right)
        {
            if (left.IsZero && right.IsZero)
                return Zero;

            var field = BarrettReducer.GetModulus();
            var res = new BivariatePolynomial(left);
            int termsCount = right.terms.Count;
            int k;

            for(k = 0; k < termsCount; k++)
            {
                Term term = right.terms[k];
                BigInteger coeff = term.coeff;

                int degx = term.degx;
                int degy = term.degy;

                res.AddTerm(field - coeff, 
                    degx, degy, false);
            }

            return res;
        }

        /// <summary>
        /// Multiplies a bivariate polynomial by a scalar over the current finite field.
        /// </summary>
        /// <param name="left">The scalar.</param>
        /// <param name="right">The bivariate polynomial.</param>
        /// <returns>The scaled polynomial reduced modulo the field.</returns>
        public static BivariatePolynomial operator *(BigInteger left, BivariatePolynomial right)
        {
            BivariatePolynomial res;
            if (left == 0) return Zero;

            if (left == 1)
            {
                res = new BivariatePolynomial(right);
                return res;
            }

            res = new BivariatePolynomial();
            res.terms = new List<Term>();

            var val = BarrettReducer.Reduce(left, true);
            int termsCount = right.terms.Count;
            int k;

            for (k = 0; k < termsCount; k++)
            {
                Term term = right.terms[k];
                BigInteger coeff = term.coeff;

                int degx = term.degx;
                int degy = term.degy;

                BigInteger product = BarrettReducer.MultMod(val, coeff);
                res.AddTerm(product, degx, degy, false);
            }

            return res;
        }

        /// <summary>
        /// Multiplies two bivariate polynomials over the current finite field.
        /// </summary>
        /// <param name="left">The first polynomial.</param>
        /// <param name="right">The second polynomial.</param>
        /// <returns>The product polynomial reduced modulo the field.</returns>
        public static BivariatePolynomial operator *(BivariatePolynomial left, BivariatePolynomial right)
        {
            if (left.IsZero || right.IsZero)
                return Zero;

            var res = new BivariatePolynomial();
            res.terms = new List<Term>();

            int lTermsCount = left.terms.Count;
            int rTermsCount = right.terms.Count;
            int j, k;

            for(j = 0; j < lTermsCount; j++)
            {
                Term leftTerm = left.terms[j];

                for (k = 0; k < rTermsCount; k++)
                {
                    Term rightTerm = right.terms[k];
                    BigInteger product = BarrettReducer.MultMod(
                        leftTerm.coeff, rightTerm.coeff);

                    int newDegx = leftTerm.degx + rightTerm.degx;
                    int newDegy = leftTerm.degy + rightTerm.degy;
                    res.AddTerm(product, newDegx, newDegy, false);
                }
            }

            return res;
        }

        /// <summary>
        /// Divides two bivariate polynomials and returns the quotient.
        /// </summary>
        /// <param name="left">Dividend polynomial.</param>
        /// <param name="right">Divisor polynomial.</param>
        /// <returns>Quotient of polynomial division.</returns>
        /// <exception cref="DivideByZeroException">Thrown when divisor is zero.</exception>
        /// <remarks>Returns only the quotient; use % operator for remainder.</remarks>
        public static BivariatePolynomial operator /(BivariatePolynomial left, BivariatePolynomial right)
        {
            if (right.IsZero)
                throw new DivideByZeroException(
                    "Polynomial divisor cannot be zero.");

            bool lessDegX = GetDegreeX(left) < GetDegreeX(right);
            bool lessDegY = GetDegreeY(left) < GetDegreeY(right);

            if (lessDegX && lessDegY) return Zero;
            if (left == right) return One;

            BivariatePolynomial quo, rem;
            Divide(left, right, out quo, out rem);
            return quo;
        }

        /// <summary>
        /// Returns the remainder after division of two bivariate polynomials.
        /// </summary>
        /// <param name="left">Dividend polynomial.</param>
        /// <param name="right">Divisor polynomial.</param>
        /// <returns>The remainder polynomial (degree less than divisor).</returns>
        /// <exception cref="DivideByZeroException">Thrown when divisor is zero.</exception>
        public static BivariatePolynomial operator %(BivariatePolynomial left, BivariatePolynomial right)
        {
            if (right.IsZero)
                throw new DivideByZeroException(
                    "Polynomial divisor cannot be zero.");

            bool lessDegX = GetDegreeX(left) < GetDegreeX(right);
            bool lessDegY = GetDegreeY(left) < GetDegreeY(right);
            BivariatePolynomial quo, rem;

            if (lessDegX && lessDegY)
            {
                rem = new BivariatePolynomial(left);
                return rem;
            }

            if (left == right) return Zero;
            Divide(left, right, out quo, out rem);
            return rem;
        }

        private static void Divide(BivariatePolynomial left,  BivariatePolynomial right, out BivariatePolynomial quo,  out BivariatePolynomial rem)
        {
            BigInteger firstTermCoeff = right.terms[0].coeff;
            BigInteger inv = BarrettReducer.InvMod(firstTermCoeff);

            rem = new BivariatePolynomial(left);
            quo = new BivariatePolynomial();
            quo.terms = new List<Term>();

            int rDegX = right.terms[0].degx;
            int rDegY = right.terms[0].degy;

            while (rem.terms.Count > 0 &&
                   rem.terms[0].degx >= rDegX &&
                   rem.terms[0].degy >= rDegY)
            {
                Term leading = rem.terms[0];
                BigInteger q = BarrettReducer.MultMod(
                    leading.coeff, inv);

                int degx = leading.degx - rDegX;
                int degy = leading.degy - rDegY;
                quo.AddTerm(q, degx, degy);

                var termPoly = new BivariatePolynomial(q, degx, degy);
                var product = termPoly * right;
                rem -= product;
            }
        }

        /// <summary>
        /// Determines whether two bivariate polynomial instances are equal.
        /// </summary>
        /// <param name="left">The first polynomial to compare.</param>
        /// <param name="right">The second polynomial to compare.</param>
        /// <returns><c>true</c> if the polynomials have identical terms; otherwise <c>false</c>.</returns>
        public static bool operator ==(BivariatePolynomial left, BivariatePolynomial right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two bivariate polynomial instances are not equal.
        /// </summary>
        /// <param name="left">The first polynomial to compare.</param>
        /// <param name="right">The second polynomial to compare.</param>
        /// <returns><c>true</c> if the polynomials differ in any term; otherwise <c>false</c>.</returns>
        public static bool operator !=(BivariatePolynomial left, BivariatePolynomial right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Evaluates the bivariate polynomial at a given Y value, returning a univariate polynomial in X.
        /// </summary>
        /// <param name="Y">The Y value to evaluate at.</param>
        /// <returns>A univariate polynomial in X with all Y terms evaluated.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when evaluation point is outside the valid field range [0, field-1].
        /// </exception>
        public Polynomial F(BigInteger Y)
        {
            if (IsZero)
            {
                var res = new Polynomial(0);
                return res;
            }

            BigInteger field = BarrettReducer.GetModulus();
            
            if (Y < 0 || Y >= field)
                throw new ArgumentOutOfRangeException(
                    nameof(Y), "Evaluation point must" +
                    " be in range [0, field-1].");

            int termsCount = terms.Count;
            int maxy = 0, i, j;

            for (i = 0; i < termsCount; i++)
            {
                if (maxy < terms[i].degy)
                    maxy = terms[i].degy;
            }

            BigInteger[] py = new BigInteger[maxy + 1];
            py[0] = 1;

            for (i = 1; i <= maxy; i++)
                py[i] = BarrettReducer.MultMod(py[i - 1], Y);

            int coeffsCount = terms[0].degx + 1;
            BigInteger[] coeffs = new BigInteger[coeffsCount];

            for (j = 0; j < coeffs.Length; j++)
                coeffs[j] = 0;

            for (j = 0; j < termsCount; j++)
            {
                int degx = terms[j].degx;
                int degy = terms[j].degy;
                BigInteger coeff = terms[j].coeff;

                BigInteger val = BarrettReducer.MultMod(coeff, py[degy]);
                coeffs[degx] = BarrettReducer.AddMod(coeffs[degx], val);
            }

            List<BigInteger> list = new List<BigInteger>();
            list.AddRange(coeffs);

            list.Reverse();
            return new Polynomial(list.ToArray());
        }

        /// <summary>
        /// Evaluates the bivariate polynomial at given X and Y values.
        /// </summary>
        /// <param name="X">The X value to evaluate at.</param>
        /// <param name="Y">The Y value to evaluate at.</param>
        /// <returns>The value of the polynomial at (X, Y) modulo the field.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when evaluation point is outside the valid field range [0, field-1].
        /// </exception>
        public BigInteger F(BigInteger X, BigInteger Y)
        {
            Polynomial poly = F(Y);
            return Polynomial.Horner(poly, X);
        }

        /// <summary>
        /// Gets the coefficient at the specified degrees.
        /// </summary>
        /// <param name="degx">The degree in X.</param>
        /// <param name="degy">The degree in Y.</param>
        /// <returns>The coefficient at the specified degrees, or 0 if no such term exists.</returns>
        public BigInteger GetCoeff(int degx, int degy)
        {
            if (ReferenceEquals(terms, null))
                return 0;

            if (terms.Count == 0)
                return 0;

            int termsCount = terms.Count;
            int k = 0;

            for (k = 0; k < termsCount; k++)
            {
                int degreeX = terms[k].degx;
                int degreeY = terms[k].degy;

                if (degreeX == degx && degreeY == degy)
                    return terms[k].coeff;
            }

            return 0;
        }

        /// <summary>
        /// Computes the formal derivative with respect to X.
        /// </summary>
        /// <param name="poly">Input polynomial.</param>
        /// <returns>The derivative polynomial dP/dX.</returns>
        public static BivariatePolynomial DiffX(BivariatePolynomial poly)
        {
            int termsCount = poly.terms.Count;
            var res = new BivariatePolynomial();
            res.terms = new List<Term>();
            int k;

            for(k = 0; k < termsCount; k++)
            {
                BigInteger value = poly.terms[k].coeff;
                int degx = poly.terms[k].degx;

                BigInteger newCoeff = BarrettReducer.MultMod(value, degx);
                int degy = poly.terms[k].degy;

                if (newCoeff != 0)
                    res.AddTerm(newCoeff, degx - 1, 
                        degy, false);
            }

            return res;
        }

        /// <summary>
        /// Computes the formal derivative with respect to Y.
        /// </summary>
        /// <param name="poly">Input polynomial.</param>
        /// <returns>The derivative polynomial dP/dY.</returns>
        public static BivariatePolynomial DiffY(BivariatePolynomial poly)
        {
            int termsCount = poly.terms.Count;
            var res = new BivariatePolynomial();
            res.terms = new List<Term>();
            int k;

            for (k = 0; k < termsCount; k++)
            {
                BigInteger value = poly.terms[k].coeff;
                int degy = poly.terms[k].degy;

                BigInteger newCoeff = BarrettReducer.MultMod(value, degy);
                int degx = poly.terms[k].degx;

                if (newCoeff != 0)
                    res.AddTerm(newCoeff, degx, 
                        degy - 1, false);
            }

            return res;
        }

        /// <summary>
        /// Returns the degree of the bivariate polynomial in X.
        /// </summary>
        /// <param name="poly">The input polynomial.</param>
        /// <returns>The highest exponent of X among all terms.</returns>
        public static int GetDegreeX(BivariatePolynomial poly)
        {
            if (ReferenceEquals(poly.terms, null)) 
                return 0;

            if (poly.terms.Count == 0) 
                return 0;

            return poly.terms[0].degx;
        }

        /// <summary>
        /// Returns the degree of the bivariate polynomial in Y.
        /// </summary>
        /// <param name="poly">The input polynomial.</param>
        /// <returns>The highest exponent of Y among all terms.</returns>
        public static int GetDegreeY(BivariatePolynomial poly)
        {
            var terms = poly.terms;

            if (ReferenceEquals(terms, null)) 
                return 0;
            
            if (terms.Count == 0) 
                return 0;

            int termsCount = terms.Count;
            int yDegree = 0, k;

            for (k = 0; k < termsCount; k++)
            {
                if (yDegree < terms[k].degy)
                    yDegree = terms[k].degy;
            }

            return yDegree;
        }

        /// <summary>
        /// Adds a new XY term to the bivariate polynomial.
        /// </summary>
        /// <param name="coeff">The coefficient.</param>
        /// <param name="degx">The degree in X.</param>
        /// <param name="degy">The degree in Y.</param>
        /// <param name="reduce">When <c>true</c>, reduces the coefficient modulo the field.</param>
        /// <remarks>
        /// Terms with the same degrees are combined via addition. Terms with zero <br/>
        /// coefficient after combination are removed. Insertion maintains lexicographic <br/>
        /// order (descending by X, then descending by Y).
        /// </remarks>
        public void AddTerm(BigInteger coeff, int degx, int degy, bool reduce = true)
        {
            BigInteger value = reduce ? 
                BarrettReducer.Reduce(coeff, true) 
                : coeff;

            if (ReferenceEquals(terms, null))
                terms = new List<Term>();

            int termsCount = terms.Count;
            Term newTerm;
            int k;

            if(termsCount == 0)
            {
                newTerm = new Term(value, degx, degy);
                terms.Add(newTerm);
                return;
            }

            if(!FindTerm(value, degx, degy))
            {
                for (k = 0; k < termsCount; k++)
                {
                    bool higherX = degx > terms[k].degx;
                    bool sameX = degx == terms[k].degx;

                    bool sameXAndHigherY = sameX &&
                        (degy > terms[k].degy);

                    if(higherX || sameXAndHigherY)
                    {
                        newTerm = new Term(value, degx, degy);
                        terms.Insert(k, newTerm);
                        return;
                    }
                }

                newTerm = new Term(value, degx, degy);
                terms.Add(newTerm);
            }
        }

        private bool FindTerm(BigInteger coeff, int degx, int degy)
        {
            int termsCount = terms.Count;
            int k;

            for (k = 0; k < termsCount; k++)
            {
                int degxt = terms[k].degx;
                int degyt = terms[k].degy;

                if (degxt == degx && degyt == degy)
                {
                    BigInteger value = terms[k].coeff;
                    BigInteger sum = BarrettReducer.AddMod(value, coeff);

                    if (sum == 0)
                    {
                        terms.RemoveAt(k);
                        return true;
                    }
                    else
                    {
                        Term term = terms[k];
                        term.coeff = sum;
                        terms[k] = term;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the finite field modulus for all bivariate polynomial operations.
        /// </summary>
        /// <param name="field">The prime field modulus.</param>
        public static void SetField(BigInteger field)
        { 
            Polynomial.SetField(field); 
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current bivariate polynomial.
        /// </summary>
        /// <param name="obj">The object to compare with the current polynomial.</param>
        /// <returns><c>true</c> if the specified object is a BivariatePolynomial with identical terms; otherwise <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BivariatePolynomial))
                return false;

            var other = (BivariatePolynomial)obj;
            return Equals(other);
        }

        /// <summary>
        /// Determines whether the current bivariate polynomial is equal to another.
        /// </summary>
        /// <param name="other">The polynomial to compare with the current polynomial.</param>
        /// <returns><c>true</c> if the polynomials have identical terms; otherwise <c>false</c>.</returns>
        public bool Equals(BivariatePolynomial other)
        {
            if (IsZero && other.IsZero)
                return true;

            if (other.terms.Count != terms.Count)
                return false;

            for(int k = 0; k < terms.Count; k++)
            {
                if (terms[k] != other.terms[k])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this bivariate polynomial instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>
        /// The hash code is derived from the internal term list.<br/>
        /// Two equal polynomials will produce the same hash code.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                int termsCount = terms.Count;
                int hash = 17, k;

                for (k = 0; k < termsCount; k++)
                    hash = hash * 31 + terms[k].GetHashCode();

                return hash;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this polynomial is zero.
        /// </summary>
        public bool IsZero
        {
            get
            {
                bool isZeroDegree = GetDegreeX(this) == 0 && GetDegreeY(this) == 0;
                return (isZeroDegree && this.GetCoeff(0, 0) == 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this polynomial is the constant one.
        /// </summary>
        public bool IsOne
        {
            get
            {
                bool isZeroDegree = GetDegreeX(this) == 0 && GetDegreeY(this) == 0;
                return (isZeroDegree && this.GetCoeff(0, 0) == 1);
            }
        }

        /// <summary>
        /// Implicitly converts a big integer to a constant bivariate polynomial.
        /// </summary>
        /// <param name="val">The big integer value.</param>
        /// <returns>A constant bivariate polynomial equal to the specified big integer.</returns>
        public static implicit operator BivariatePolynomial(BigInteger val)
        {
            return new BivariatePolynomial(val);
        }

        /// <summary>
        /// Implicitly converts a univariate polynomial to a bivariate polynomial in X.
        /// </summary>
        /// <param name="poly">The univariate polynomial.</param>
        /// <returns>A bivariate polynomial with Y degrees all zero.</returns>
        public static implicit operator BivariatePolynomial(Polynomial poly)
        {
            return new BivariatePolynomial(poly);
        }
    }
}
