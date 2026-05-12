using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Eduard.Security
{
    /// <summary>
    /// Represents univariate polynomials with coefficients in a prime field.
    /// </summary>
    /// <remarks>
    /// All polynomial operations are performed modulo a prime field set via SetField().<br/>
    /// Supports addition, subtraction, multiplication, division, modulus, GCD, exponentiation,<br/>
    /// root finding, and FFT-based fast algorithms for large polynomials.<br/>
    /// Coefficients are stored in descending order (constant term last).
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public struct Polynomial : IEquatable<Polynomial>
    {
        /// <summary>
        /// The degree of the polynomial (highest exponent with non-zero coefficient).
        /// </summary>
        /// <remarks>
        /// Automatically updated when coefficients change. A zero polynomial has degree 0.
        /// </remarks>
        public int degree;

        /// <summary>
        /// The coefficients of the polynomial in ascending order (constant term at index 0).
        /// </summary>
        /// <remarks>
        /// Coefficients are always reduced modulo the current field. Automatically sized to degree + 1.
        /// </remarks>
        public BigInteger[] coeffs;

        /// <summary>
        /// Initializes a new polynomial instance with all coefficients set to zero.
        /// </summary>
        /// <param name="degree">The degree of the polynomial.</param>
        /// <remarks>
        /// Creates a zero polynomial of the specified degree.<br/>
        /// All coefficients are initialized to zero. Use this constructor<br/> 
        /// when building polynomials coefficient by coefficient.
        /// </remarks>
        public Polynomial(int degree)
        {
            this.degree = degree;
            coeffs = new BigInteger[degree + 1];

            for (int i = 0; i < coeffs.Length; i++)
                coeffs[i] = 0;
        }

        /// <summary>
        /// Initializes a new polynomial instance by copying an existing polynomial.
        /// </summary>
        /// <param name="poly">The polynomial to copy.</param>
        /// <remarks>
        /// Creates a deep copy where modifications to the new instance<br/>
        /// do not affect the original polynomial and vice versa.
        /// </remarks>
        public Polynomial(Polynomial poly)
        {
            degree = poly.degree;
            coeffs = new BigInteger[degree + 1];

            for (int i = 0; i < coeffs.Length; i++)
                coeffs[i] = poly.coeffs[i];
        }

        /// <summary>
        /// Initializes a new polynomial instance with specified coefficients.
        /// </summary>
        /// <param name="coeffs">Coefficients in descending order.</param>
        /// <remarks>
        /// Coefficients are automatically reduced modulo the current field.<br/>
        /// Example: new Polynomial(3, -5, 6) represents 3*X^2 - 5*X + 6.
        /// </remarks>
        public Polynomial(params BigInteger[] coeffs)
        {
            degree = coeffs.Length - 1;
            this.coeffs = new BigInteger[degree + 1];
            List<BigInteger> list = new List<BigInteger>();

            list.AddRange(coeffs);
            list.Reverse();

            for (int i = 0; i <= degree; i++)
                this.coeffs[i] = BarrettReducer.Reduce(list[i], true);
        }

        /// <summary>
        /// Sets the finite field modulus for all polynomial operations.
        /// </summary>
        /// <param name="field">The prime field modulus.</param>
        /// <remarks>
        /// Initializes Barrett reduction constants and optimizes square root computations<br/>
        /// based on field properties. Must be called before any polynomial operations.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when field modulus is less than 5 or not prime.
        /// </exception>
        public static void SetField(BigInteger field)
        {
            if (field < 5)
                throw new ArgumentException(
                    "Field modulus cannot " 
                    + "be less than 5.");

            bool isPrime = BigInteger.IsProbablePrime(field);

            if (!isPrime)
                throw new ArgumentException(
                    "Field modulus must be prime.");

            BarrettReducer.SetModulus(field);
            ModSqrtUtil.InitParams();
        }

        internal void Update()
        {
            List<BigInteger> list = new List<BigInteger>();
            list.AddRange(coeffs);

            while (list[degree] == 0 && list.Count > 1 && degree != 0)
                degree--;

            coeffs = new BigInteger[degree + 1];

            for (int i = 0; i <= degree; i++)
                coeffs[i] = list[i];
        }

        /// <summary>
        /// Adds two polynomials over the current finite field.
        /// </summary>
        /// <param name="left">The first polynomial.</param>
        /// <param name="right">The second polynomial.</param>
        /// <returns>The sum polynomial reduced modulo the field.</returns>
        /// <remarks>
        /// Performs coefficient-wise addition with modular reduction.<br/>
        /// The result degree is automatically trimmed to remove leading <br/>zero coefficients.
        /// </remarks>
        public static Polynomial operator +(Polynomial left, Polynomial right)
        {
            int max = (left.degree > right.degree) ? left.degree : right.degree;
            Polynomial result = new Polynomial(max);

            for (int i = 0; i <= max; i++)
                result.coeffs[i] = BarrettReducer.AddMod(left.GetCoeff(i), right.GetCoeff(i));

            result.Update();
            return result;
        }

        /// <summary>
        /// Subtracts one polynomial from another over the current finite field.
        /// </summary>
        /// <param name="left">The minuend polynomial.</param>
        /// <param name="right">The subtrahend polynomial.</param>
        /// <returns>The difference polynomial reduced modulo the field.</returns>
        public static Polynomial operator -(Polynomial left, Polynomial right)
        {
            int max = (left.degree > right.degree) ? left.degree : right.degree;
            Polynomial result = new Polynomial(max);

            for (int i = 0; i <= max; i++)
                result.coeffs[i] = BarrettReducer.SubMod(left.GetCoeff(i), right.GetCoeff(i));

            result.Update();
            return result;
        }

        /// <summary>
        /// Multiplies two polynomials over the current finite field.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>Product polynomial reduced modulo the field.</returns>
        /// <remarks>
        /// Automatically selects between schoolbook or FFT multiplication<br/>
        /// based on polynomial degrees for optimal performance.
        /// </remarks>
        public static Polynomial operator *(Polynomial left, Polynomial right)
        {
            if (left == right) return Square(left);
            else return Multiply(left, right);
        }

        private static Polynomial Multiply(Polynomial left, Polynomial right)
        {
            int min = Math.Min(left.degree, right.degree);

#if !USE_BENCHMARKING
            int FFT_POLY_MULT_THRESHOLD = (int)Threshold.POLY_FFT_MULT_THRESHOLD;
#else
            int FFT_POLY_MULT_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.POLY_FFT_MULT);
#endif
            if (min >= FFT_POLY_MULT_THRESHOLD)
            {
                int deg = left.degree + right.degree;
                BigInteger[] coeffs = FFT.FastPolyMult(left.coeffs, right.coeffs);

                while (deg > 0 && coeffs[deg] == 0)
                    deg--;

                Polynomial res = new Polynomial(deg);

                for (int i = 0; i <= deg; i++)
                    res.coeffs[i] = coeffs[i];

                return res;
            }
            else
                return plain_mult(left, right);
        }

        private static Polynomial Square(Polynomial val)
        {
#if !USE_BENCHMARKING
            int FFT_POLY_SQUARE_THRESHOLD = (int)Threshold.POLY_FFT_SQUARE_THRESHOLD;
#else
            int FFT_POLY_SQUARE_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.POLY_FFT_SQUARE);
#endif
            if (val.degree >= FFT_POLY_SQUARE_THRESHOLD)
            {
                BigInteger[] coeffs = FFT.FastPolySquare(val.coeffs);
                int deg = val.degree << 1;

                while (deg > 0 && coeffs[deg] == 0)
                    deg--;

                Polynomial res = new Polynomial(deg);

                for (int i = 0; i <= deg; i++)
                    res.coeffs[i] = coeffs[i];

                return res;
            }
            else 
                return plain_square(val);
        }

        /// <summary>
        /// Reduces a polynomial modulo another polynomial over the current finite field.
        /// </summary>
        /// <param name="x">The polynomial to reduce.</param>
        /// <param name="m">The modulus polynomial.</param>
        /// <returns>The remainder of x divided by m.</returns>
        /// <exception cref="DivideByZeroException">Thrown when modulus polynomial is zero.</exception>
        /// <remarks>
        /// Automatically selects between classical division and FFT-based reduction<br/>
        /// based on polynomial degrees. For large polynomials meeting the FFT threshold,<br/>
        /// uses optimized transform-based algorithm with precomputed reciprocals.
        /// </remarks>
        public static Polynomial Reduce(Polynomial x, Polynomial m)
        {
            if (m.degree == 0 && m.coeffs[0] == 0)
                throw new DivideByZeroException("Modulus polynomial cannot be zero.");

            int degm = m.degree;
            int n = x.degree;

#if USE_BENCHMARKING
            int FFT_POLY_MOD_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.POLY_FFT_MOD);
#else
            int FFT_POLY_MOD_THRESHOLD = (int)Threshold.POLY_FFT_MOD_THRESHOLD;
#endif

            if (degm < FFT_POLY_MOD_THRESHOLD || n - degm < FFT_POLY_MOD_THRESHOLD)
            {
                Polynomial r = x % m;
                return r;
            }

            BigInteger[] G = new BigInteger[n + 1];
            BigInteger[] R = new BigInteger[degm + 1];

            for (int i = 0; i <= n; i++)
                G[i] = x.coeffs[i];

            for (int j = 0; j <= degm; j++)
                R[j] = 0;
            

            if (!FFT.FastPolyMod(G, R))
            {
                SetPolyMod(m);
                FFT.FastPolyMod(G, R);
            }

            int deg = degm - 1;
            Polynomial res = new Polynomial(deg);

            for (int i = 0; i <= deg; i++)
                res.coeffs[i] = R[i];

            res.Update();
            return res;
        }

        /// <summary>
        /// Configures FFT-based modular reduction for a given modulus polynomial.
        /// </summary>
        /// <param name="poly">The modulus polynomial.</param>
        /// <exception cref="DivideByZeroException">Thrown when modulus polynomial is zero.</exception>
        /// <remarks>
        /// Precomputes reciprocal and other FFT parameters to accelerate subsequent<br/>
        /// modular reductions. Only takes effect when polynomial degree meets the<br/>
        /// FFT threshold. Called automatically by Reduce() when beneficial.
        /// </remarks>
        public static void SetPolyMod(Polynomial poly)
        {
            if (poly.degree == 0 && poly.coeffs[0] == 0)
                throw new DivideByZeroException("Modulus polynomial cannot be zero.");

            int m, n = poly.degree;

#if USE_BENCHMARKING
            int FFT_POLY_MOD_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.POLY_FFT_MOD);
#else
            int FFT_POLY_MOD_THRESHOLD = (int)Threshold.POLY_FFT_MOD_THRESHOLD;
#endif

            if (n < FFT_POLY_MOD_THRESHOLD) return;
            Polynomial h = new Polynomial(poly);
            h.Reverse();

            h = Invmodxn(h, n);
            h.Reverse();
            m = h.degree;

            if (m < n - 1) 
                h = Mulxn(h, n - 1 - m);

            BigInteger[] f = new BigInteger[n + 1];
            BigInteger[] rf = new BigInteger[n + 1];

            for (int i = 0; i <= n; i++)
            {
                f[i] = poly.coeffs[i];
                rf[i] = (i <= m) ? h.coeffs[i] : 0;
            }

            FFT.SetPolyMod(n, rf, f);
        }

        private void Reverse()
        {
            List<BigInteger> list = new List<BigInteger>();
            list.AddRange(coeffs);

            list.Reverse();
            this.coeffs = list.ToArray();
        }

        private static Polynomial plain_mult(Polynomial left, Polynomial right)
        {
            int degree = left.degree + right.degree;
            Polynomial result = new Polynomial(degree);

            for (int j = 0; j <= left.degree; j++)
            {
                if (left.GetCoeff(j) == 0) continue;

                for (int k = 0; k <= right.degree; k++)
                {
                    if (right.GetCoeff(k) == 0) continue;
                    result.coeffs[j + k] = BarrettReducer.AddMod(result.coeffs[j + k], BarrettReducer.MultMod(left.GetCoeff(j), right.GetCoeff(k)));
                }
            }

            result.Update();
            return result;
        }

        private static Polynomial plain_square(Polynomial poly)
        {
            int degree = 2 * poly.degree;
            Polynomial result = new Polynomial(degree);

            for (int i = 0; i <= poly.degree; i++)
                result.coeffs[2 * i] = BarrettReducer.MultMod(poly.GetCoeff(i), poly.GetCoeff(i));

            for(int j = 0; j < poly.degree; j++)
            {
                if (poly.GetCoeff(j) == 0) continue;

                for(int k = j + 1; k <= poly.degree; k++)
                {
                    BigInteger t = BarrettReducer.MultMod(poly.GetCoeff(j), poly.GetCoeff(k));
                    result.coeffs[j + k] = BarrettReducer.AddMod(result.coeffs[j + k], BarrettReducer.MultMod(2, t));
                }
            }

            result.Update();
            return result;
        }

        /// <summary>
        /// Multiplies a polynomial by X^n, shifting coefficients upward.
        /// </summary>
        /// <param name="poly">The input polynomial.</param>
        /// <param name="words">The exponent n (number of shifts).</param>
        /// <returns>Polynomial result of multiplying by X^n (coefficients shifted up by n).</returns>
        /// <remarks>
        /// Used internally for polynomial arithmetic and FFT-based algorithms.
        /// </remarks>
        public static Polynomial Mulxn(Polynomial poly, int words)
        {
            BigInteger[] coeffs = new BigInteger[poly.coeffs.Length + words];

            for(int k = 0; k < coeffs.Length; k++)
            {
                if (k < words)
                    coeffs[k] = 0;
                else
                    coeffs[k] = poly.coeffs[k - words];
            }

            Polynomial result = new Polynomial();
            result.coeffs = coeffs;

            result.degree = coeffs.Length - 1;
            return result;
        }

        /// <summary>
        /// Divides two polynomials and returns the quotient.
        /// </summary>
        /// <param name="left">Dividend polynomial.</param>
        /// <param name="right">Divisor polynomial.</param>
        /// <returns>Quotient of polynomial division.</returns>
        /// <exception cref="DivideByZeroException">Thrown when divisor is zero.</exception>
        /// <remarks>Returns only the quotient; use % operator for remainder.</remarks>
        public static Polynomial operator /(Polynomial left, Polynomial right)
        {
            if (right.degree == 0 && right.coeffs[0] == 0)
                throw new DivideByZeroException("Polynomial divisor cannot be zero.");

            if (left.degree < right.degree)
                return new Polynomial(0);

            if(right.degree == 0)
            {
                BigInteger vn = BarrettReducer.InvMod(right.coeffs[right.degree]);
                List <BigInteger> words = new List<BigInteger>();

                for (int i = 0; i <= left.degree; i++)
                    words.Add(BarrettReducer.MultMod(left.coeffs[i], vn));

                words.Reverse();
                return new Polynomial(words.ToArray());
            }

            Polynomial quo, rem;
            Divide(left, right, out quo, out rem);
            return quo;
        }

        private static void Divide(Polynomial left, Polynomial right, out Polynomial quo, out Polynomial rem)
        {
            int m = left.degree;
            int n = right.degree;

            BigInteger inv = BarrettReducer.InvMod(right.coeffs[right.degree]);
            quo = new Polynomial(m - n);
            rem = new Polynomial(left);
            
            for (int k = m - n; k >= 0; k--)
            {
                quo.coeffs[k] = BarrettReducer.MultMod(rem.coeffs[n + k], inv);

                for(int j = n + k; j >= k; j--)
                    rem.coeffs[j] = BarrettReducer.SubMod(rem.coeffs[j], BarrettReducer.MultMod(quo.coeffs[k], right.coeffs[j - k]));
            }

            quo.Update();
            rem.Update();
        }

        /// <summary>
        /// Returns the remainder after division of two polynomials.
        /// </summary>
        /// <param name="left">Dividend polynomial.</param>
        /// <param name="right">Divisor polynomial.</param>
        /// <returns>The remainder polynomial (degree less than divisor).</returns>
        /// <exception cref="DivideByZeroException">Thrown when divisor is zero.</exception>
        public static Polynomial operator %(Polynomial left, Polynomial right)
        {
            if (right.degree == 0 && right.coeffs[0] == 0)
                throw new DivideByZeroException("Polynomial divisor cannot be zero.");

            if (left.degree < right.degree)
                return left;

            if (right.degree == 0)
                return new Polynomial(0);

            Polynomial quo, rem;
            Divide(left, right, out quo, out rem);
            return rem;
        }

        /// <summary>
        /// Multiplies two polynomials and reduces the result modulo a third polynomial over the current finite field.
        /// </summary>
        /// <param name="left">The first polynomial operand.</param>
        /// <param name="right">The second polynomial operand.</param>
        /// <param name="modulus">The modulus polynomial.</param>
        /// <returns>The product polynomial reduced modulo the modulus.</returns>
        /// <remarks>
        /// Combines multiplication and reduction in a single operation.<br/>
        /// Equivalent to (left * right) % modulus. Automatically selects between<br/>
        /// schoolbook or FFT-based algorithms based on polynomial degrees.
        /// </remarks>
        public static Polynomial MultMod(Polynomial left, Polynomial right, Polynomial modulus)
        {
            Polynomial res = left * right;
            res = Reduce(res, modulus);
            return res;
        }

        /// <summary>
        /// Computes exponentiation of a polynomial raised to an integer exponent over the current finite field.
        /// </summary>
        /// <param name="val">The base polynomial.</param>
        /// <param name="exponent">The exponent value.</param>
        /// <returns>The base polynomial raised to the exponent.</returns>
        /// <exception cref="ArithmeticException">Thrown when both base and exponent are zero (0^0 is undefined).</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when exponent is negative.</exception>
        /// <remarks>
        /// Implements binary exponentiation (square-and-multiply).<br/>
        /// For modular exponentiation, use <see cref="Pow(Polynomial, BigInteger, Polynomial)"/>.
        /// </remarks>
        public static Polynomial Pow(Polynomial val, int exponent)
        {
            if (val == 0 && exponent == 0)
                throw new ArithmeticException(
                    "Cannot compute 0^0: zero " + 
                    "polynomial raised to power" + 
                    " zero is undefined.");

            if (exponent < 0)
                throw new ArgumentOutOfRangeException(
                    "Exponent cannot be negative for " 
                    + "polynomial exponentiation.");

            if (exponent == 0) return 1;
            if (exponent == 1) return val;

            Polynomial res = 1;
            int e = exponent;
            
            while(e != 0)
            {
                if ((e & 1) == 1)
                    res *= val;

                val = val * val;
                e >>= 1;
            }

            return res;
        }

        /// <summary>
        /// Computes modular exponentiation of a polynomial raised to a big integer exponent.
        /// </summary>
        /// <param name="val">The base polynomial.</param>
        /// <param name="exponent">The exponent value.</param>
        /// <param name="modulus">The modulus polynomial.</param>
        /// <returns>The base polynomial raised to the exponent modulo the modulus.</returns>
        /// <exception cref="DivideByZeroException">Thrown when modulus polynomial is zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when exponent is negative.</exception>
        /// <exception cref="ArithmeticException">Thrown when both base and exponent are zero (0^0 is undefined).</exception>
        public static Polynomial Pow(Polynomial val, BigInteger exponent, Polynomial modulus)
        {
            Polynomial nb = new Polynomial(val);
            Polynomial rb = nb % modulus;
            Polynomial result = 1;

            if(modulus.degree == 0 && modulus.coeffs[0] == 0)
                throw new DivideByZeroException(
                    "Modulus polynomial cannot be zero.");

            if (rb == 0 && exponent == 0)
                throw new ArithmeticException(
                    "Cannot compute 0^0: zero " + 
                    "polynomial raised to power " 
                    + "zero is undefined.");

            if (exponent < 0)
                throw new ArgumentOutOfRangeException(
                    "Exponent cannot be negative for " + 
                    "polynomial modular exponentiation.");

            if (exponent == 0) return 1;
            if (exponent == 1) return rb;

#if !USE_BENCHMARKING
            int DEGREE_THRESHOLD = (int)Threshold.POLY_DEGREE_THRESHOLD;
#else
            int DEGREE_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.POLY_DEGREE_POW_MOD);
#endif

            if (modulus.degree >= DEGREE_THRESHOLD)
            {
                int windowSize = 5;
                int store = 1 << (windowSize - 1);
                SetPolyMod(modulus);

                Polynomial[] table = new Polynomial[store];
                table[0] = rb;
                Polynomial b2 = MultMod(rb, rb, modulus);

                // Creates table of odd powers.
                for (int i = 1; i < store; i++)
                    table[i] = MultMod(table[i - 1], b2, modulus);

                int bits = exponent.GetBits();
                int ubits = 0, tbits = 0;

                for (int i = bits - 1; i > -1;)
                {
                    int win = WindowUtil.Window(exponent, i, ref ubits, ref tbits);

                    for (int j = 0; j < ubits; j++)
                        result = MultMod(result, result, modulus);

                    if (win != 0)
                        result = MultMod(result, table[win >> 1], modulus);
                    i -= ubits;
                    if (tbits != 0)
                    {
                        for (int j = 0; j < tbits; j++)
                            result = MultMod(result, result, modulus);
                        i -= tbits;
                    }
                }
            }
            else
            {
                int size = exponent.GetBits();

                for(int k = size - 1; k >= 0; k--)
                {
                    result = (result * result) % modulus;

                    if (exponent.TestBit(k))
                        result = (result * rb) % modulus;
                }
            }

            return result;
        }

        private static Polynomial ScaleMod(BigInteger k, Polynomial poly)
        {
            Polynomial res = new Polynomial(poly.degree);
            int j;

            for (j = 0; j <= res.degree; j++)
                res.coeffs[j] = BarrettReducer.MultMod(k, poly.coeffs[j]);

            return res;
        }

        /// <summary>
        /// Composes two polynomials, computing P(Q(X)) over the current finite field.
        /// </summary>
        /// <param name="left">The outer polynomial P(X).</param>
        /// <param name="right">The inner polynomial Q(X).</param>
        /// <returns>The composition polynomial P(Q(X)) reduced modulo the field.</returns>
        /// <remarks>
        /// Implements Horner's method for polynomial composition. The result degree is deg(P) * deg(Q). For <br/>
        /// composition with modular reduction, use <see cref="Compose(Polynomial, Polynomial, Polynomial, bool)"/>.
        /// </remarks>
        public static Polynomial Compose(Polynomial left, Polynomial right)
        {
            Polynomial poly = left;
            Polynomial res = 0;
            Polynomial temp = 1;

            for (int k = 0; k <= poly.degree; k++)
            {
                BigInteger kt = poly.coeffs[k];
                Polynomial aux = ScaleMod(kt, temp);

                temp *= right;
                res += aux;
            }

            return res;
        }

        /// <summary>
        /// Composes two polynomials modulo a third polynomial, computing P(Q(X)) mod M(X) over the current finite field.
        /// </summary>
        /// <param name="left">The outer polynomial P(X).</param>
        /// <param name="right">The inner polynomial Q(X).</param>
        /// <param name="modulus">The modulus polynomial M(X).</param>
        /// <param name="prepareModulus">
        /// If <c>true</c>, precomputes FFT parameters for the modulus using <see cref="SetPolyMod"/>.
        /// Set to <c>false</c> when calling repeatedly with the same modulus.
        /// </param>
        /// <returns>The composition polynomial P(Q(X)) reduced modulo M(X) over the current field.</returns>
        /// <exception cref="DivideByZeroException">Thrown when modulus polynomial is zero.</exception>
        /// <remarks>
        /// <para>
        /// Implements polynomial modular composition using a hybrid algorithm strategy:
        /// </para>
        /// <list type="bullet">
        /// <item><description>FFT-accelerated Horner's method for large-degree polynomials</description></item>
        /// <item><description>Classical Horner's method for small-degree polynomials</description></item>
        /// </list>
        /// <para>
        /// Critical for GLV/GLS endomorphisms, isogeny evaluation in CSIDH/SIKE, and Frobenius <br/>
        /// endomorphisms in pairing-based cryptography. The result degree is bounded by deg(M) - 1.
        /// </para>
        /// </remarks>
        public static Polynomial Compose(Polynomial left, Polynomial right, Polynomial modulus, bool prepareModulus = true)
        {
            if (modulus.degree == 0 && modulus.coeffs[0] == 0)
                throw new DivideByZeroException(
                    "Modulus polynomial cannot be zero.");

#if !USE_BENCHMARKING
            int POLY_MOD_COMPOSE_THRESHOLD = (int)Threshold.POLY_MOD_COMPOSE_THRESHOLD;
#else
            int POLY_MOD_COMPOSE_THRESHOLD = PerfTuner.GetThreshold(PerfEntry.POLY_DEGREE_FAST_HORNER);
#endif

            Polynomial poly = left;
            Polynomial res = 0;
            Polynomial temp = 1;

            if (modulus.degree >= POLY_MOD_COMPOSE_THRESHOLD)
            {
                if (prepareModulus)
                    SetPolyMod(modulus);

                for (int k = 0; k <= poly.degree; k++)
                {
                    BigInteger kt = poly.coeffs[k];
                    Polynomial aux = ScaleMod(kt, temp);

                    temp = MultMod(temp, right, modulus);
                    res += aux;
                }
            }
            else
            {
                for (int k = 0; k <= poly.degree; k++)
                {
                    BigInteger kt = poly.coeffs[k];
                    Polynomial aux = ScaleMod(kt, temp);

                    temp = (temp * right) % modulus;
                    res += aux;
                }
            }

            return res;
        }

        /// <summary>
        /// Computes the greatest common divisor of two polynomials over the current finite field.
        /// </summary>
        /// <param name="left">First polynomial.</param>
        /// <param name="right">Second polynomial.</param>
        /// <returns>A monic polynomial representing the GCD of the inputs.</returns>
        /// <remarks>
        /// Uses Euclidean algorithm for polynomial GCD.<br/>
        /// The result is normalized to be monic (leading coefficient = 1).
        /// </remarks>
        public static Polynomial Gcd(Polynomial left, Polynomial right)
        {
            Polynomial a, b;

            if (left == 0 && right != 0)
                return right;

            if (left != 0 && right == 0)
                return left;

            if (left == 1 || right == 1)
                return 1;

            if(left.degree < right.degree)
            {
                a = new Polynomial(right);
                b = new Polynomial(left);
            }
            else
            {
                a = new Polynomial(left);
                b = new Polynomial(right);
            }

            while(b != 0)
            {
                Polynomial r = a % b;
                a = new Polynomial(b);
                b = new Polynomial(r);
            }

            BigInteger coeff = a.coeffs[a.degree];
            BigInteger inv = BarrettReducer.InvMod(coeff);

            a *= inv;
            return a;
        }

        /// <summary>
        /// Evaluates the polynomial at a given point using Horner's method.
        /// </summary>
        /// <param name="X">The point to evaluate at.</param>
        /// <returns>The value of the polynomial at X modulo the field.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when evaluation point is outside the valid field range [0, field-1].
        /// </exception>
        public BigInteger Horner(BigInteger X)
        {
            BigInteger field = BarrettReducer.GetModulus();
            BigInteger sum = coeffs[0];
            BigInteger val = 1;

            if (X < 0 || X >= field)
                throw new ArgumentOutOfRangeException(
                    nameof(X), "Evaluation point must" +
                    " be in range [0, field-1].");

            for (int k = 1; k <= degree; k++)
            {
                val = BarrettReducer.MultMod(val, X);
                BigInteger test = BarrettReducer.MultMod(val, coeffs[k]);
                sum = BarrettReducer.AddMod(sum, test);
            }

            return sum;
        }

        private Polynomial FindFactor()
        {
            while(true)
            {
                BigInteger field = BarrettReducer.GetModulus();
                BigInteger a = SecureRandom.Range(1, field - 1);
                Polynomial g = new Polynomial(1, a);
                
                Polynomial h = Pow(g, (field - 1) / 2, this);
                Polynomial poly = h + 1;

                if (poly == 1) continue;
                Polynomial factor = Gcd(poly, this);

                if (factor.degree != 0)
                    return factor;
            }
        }

        /// <summary>
        /// Finds roots of polynomials with small degree over the current finite field.
        /// </summary>
        /// <param name="poly">The polynomial to solve (degree 1 or 2).</param>
        /// <param name="roots">List that will be populated with the roots found.</param>
        /// <returns>1 if roots were found, 0 if polynomial degree > 2, -1 if no roots exist.</returns>
        /// <exception cref="InvalidOperationException">Thrown when field not initialized.</exception>
        public static int Solve(Polynomial poly, ref List<BigInteger> roots)
        {
            BigInteger field = BarrettReducer.GetModulus();
            if (poly.degree > 2) return 0;

            if(poly.degree == 1)
            {
                BigInteger inv = BarrettReducer.InvMod(poly.coeffs[1]);
                BigInteger b = field - poly.coeffs[0];
                BigInteger root = BarrettReducer.MultMod(b, inv);

                roots.Add(root);
                return 1;
            }
            
            if(poly.degree == 2)
            {
                BigInteger sb = BarrettReducer.MultMod(poly.coeffs[1], poly.coeffs[1]);
                BigInteger ac4 = BarrettReducer.MultMod(field - 4, poly.coeffs[2]);

                ac4 = BarrettReducer.MultMod(ac4, poly.coeffs[0]);
                BigInteger delta = BarrettReducer.AddMod(sb, ac4);

                int jSymbol = BigInteger.Jacobi(delta, field);
                if (jSymbol == -1) return -1;

                BigInteger root = ModSqrtUtil.Sqrt(delta, true);
                BigInteger val = BarrettReducer.MultMod(2, poly.coeffs[2]);

                BigInteger inv = val.Inverse(field);
                BigInteger t = BarrettReducer.AddMod(field - poly.coeffs[1], root);

                t = BarrettReducer.MultMod(inv, t);
                roots.Add(t);

                t = BarrettReducer.AddMod(field - poly.coeffs[1], field - root);
                t = BarrettReducer.MultMod(inv, t);

                roots.Add(t);
                return 1;
            }

            return -1;
        }

        /// <summary>
        /// Finds one or two roots of the polynomial in the current finite field.
        /// </summary>
        /// <param name="roots">List that will be populated with found roots.</param>
        public void FindRoots(ref List<BigInteger> roots)
        {
            Polynomial self = new Polynomial(this);
            Polynomial aux = new Polynomial(1, 0);

            BigInteger field = BarrettReducer.GetModulus();
            Polynomial vtemp = Pow(aux, field, self);
            vtemp -= aux;

            Polynomial poly = Gcd(vtemp, self);
            if (poly == 1) return;

            /* compute the roots of a polynomial early if the degree is 1 or 2 */
            if (poly.degree == 1 || poly.degree == 2)
            {
                Solve(poly, ref roots);
                return;
            }

            if (poly.degree >= 1 && poly != 0 && poly != this)
                self = new Polynomial(poly);

            while (roots.Count == 0)
            {
                Polynomial temp = self.FindFactor();
                int ID = Solve(temp, ref roots);

                if (ID == 0)
                {
                    self = new Polynomial(temp);
                    continue;
                }
            }
        }

        /// <summary>
        /// Divides a polynomial by X^n, effectively shifting coefficients down.
        /// </summary>
        /// <param name="poly">The input polynomial.</param>
        /// <param name="degn">The exponent n (number of shifts).</param>
        /// <returns>Polynomial result of dividing by X^n (coefficients shifted down by n).</returns>
        public static Polynomial Divxn(Polynomial poly, int degn)
        {
            if (poly.degree < degn) return 0;
            Polynomial result = new Polynomial();
            result.coeffs = new BigInteger[poly.degree - degn + 1];

            for (int k = degn; k <= poly.degree; k++)
                result.coeffs[k - degn] = poly.GetCoeff(k);

            result.degree = poly.degree - degn;
            result.Update();
            return result;
        }

        /// <summary>
        /// Computes polynomial modulo X^n, keeping only the lowest n coefficients.
        /// </summary>
        /// <param name="poly">The input polynomial.</param>
        /// <param name="degn">The exponent n (number of coefficients to keep).</param>
        /// <returns>Polynomial truncated to degree n-1.</returns>
        public static Polynomial Modxn(Polynomial poly, int degn)
        {
            if (poly.degree < degn) return poly;
            Polynomial result = new Polynomial();
            result.coeffs = new BigInteger[degn];

            for (int k = 0; k < degn; k++)
                result.coeffs[k] = poly.GetCoeff(k);

            result.degree = degn - 1;
            result.Update();
            return result;
        }

        /// <summary>
        /// Computes polynomial modulo X^n - 1, wrapping coefficients cyclically.
        /// </summary>
        /// <param name="poly">The input polynomial.</param>
        /// <param name="degn">The exponent n (modulus is X^n - 1).</param>
        /// <returns>Polynomial reduced modulo X^n - 1 with degree less than n.</returns>
        /// <remarks>
        /// Reduces poly modulo X^n - 1 by adding coefficient k to coefficient (k mod n).<br/>
        /// This implements cyclic convolution and is used in NTT-based algorithms.
        /// </remarks>
        public static Polynomial Modxn_l(Polynomial poly, int degn)
        {
            if (poly.degree < degn) return poly;
            Polynomial result = new Polynomial(degn - 1);

            for (int k = 0; k + degn <= poly.degree; k++)
                result.coeffs[k] = BarrettReducer.AddMod(poly.GetCoeff(k), poly.GetCoeff(k + degn));

            result.Update();
            return result;
        }

        /// <summary>
        /// Computes the modular inverse of a polynomial modulo X^n using Newton iteration.
        /// </summary>
        /// <param name="poly">The polynomial to invert (constant term must be invertible).</param>
        /// <param name="degn">The exponent n (inverse is computed modulo X^n).</param>
        /// <returns>The polynomial A(X) such that poly * A = 1 (mod X^n).</returns>
        /// <remarks>
        /// Implements Newton's method for polynomial inversion: given an initial approximation <br/>
        /// modulo X^1, each iteration doubles the precision. Complexity is O(n log n).
        /// Used internally<br/> for FFT-based division and modular reduction algorithms.
        /// </remarks>
        public static Polynomial Invmodxn(Polynomial poly, int degn)
        {
            int k = 0;
            BigInteger field = BarrettReducer.GetModulus();
            BigInteger lastCoeff = BarrettReducer.InvMod(poly.GetCoeff(0));

            Polynomial result = new Polynomial(lastCoeff);
            while ((1 << k) < degn) k++;

            for (int i = 1; i <= k; i++)
                result = Modxn(2 * result - poly * result * result, 1 << i);

            result = Modxn(result, degn);
            return result;
        }

        /// <summary>
        /// Returns the leading coefficient (the highest-degree non-zero coefficient).
        /// </summary>
        /// <returns>The leading coefficient, or 0 if the polynomial is zero.</returns>
        /// <remarks>
        /// For a non-zero polynomial, returns the coefficient of the highest-degree term. <br/>
        /// For the zero polynomial, returns 0. Used internally for normalization
        /// and monic <br/> polynomial operations.
        /// </remarks>
        internal BigInteger Min()
        {
            for(int k = coeffs.Length - 1; k >= 0; k--)
            {
                if (coeffs[k] != 0)
                    return coeffs[k];
            }

            return 0;
        }

        /// <summary>
        /// Computes the formal derivative of a polynomial.
        /// </summary>
        /// <param name="poly">Input polynomial.</param>
        /// <param name="field">Field modulus.</param>
        /// <returns>The derivative polynomial.</returns>
        public static Polynomial Differentiate(Polynomial poly, BigInteger field)
        {
            if (poly.degree == 0) return 0;
            Polynomial diff = new Polynomial(poly.degree - 1);

            for (int k = 1; k <= poly.degree; k++)
                diff.coeffs[k - 1] = BarrettReducer.MultMod(k, poly.coeffs[k]);

            return diff;
        }

        /// <summary>
        /// Determines whether two polynomial instances are equal.
        /// </summary>
        /// <param name="left">The first polynomial to compare.</param>
        /// <param name="right">The second polynomial to compare.</param>
        /// <returns>true if the polynomials have identical coefficients; otherwise false.</returns>
        /// <remarks>
        /// Two polynomials are considered equal if they have the same <br/>
        /// coefficients for all terms, ignoring any trailing zero coefficients. <br/>
        /// The comparison is performed coefficient-wise modulo the current field.
        /// </remarks>
        public static bool operator ==(Polynomial left, Polynomial right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two polynomial instances are not equal.
        /// </summary>
        /// <param name="left">The first polynomial to compare.</param>
        /// <param name="right">The second polynomial to compare.</param>
        /// <returns>true if the polynomials differ in any coefficient; otherwise false.</returns>
        /// <remarks>
        /// Returns the logical negation of the equality operator.
        /// </remarks>
        public static bool operator !=(Polynomial left, Polynomial right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Determines whether the current polynomial is equal to another polynomial.
        /// </summary>
        /// <param name="other">The polynomial to compare with the current polynomial.</param>
        /// <returns>true if the polynomials have identical coefficients; otherwise false.</returns>
        /// <remarks>
        /// Performs coefficient-wise comparison modulo the current field.<br/>
        /// Two polynomials are considered equal if they have the same coefficients<br/>
        /// for all terms up to the maximum degree, treating missing terms as zero.
        /// </remarks>
        public bool Equals(Polynomial other)
        {
            if (this.degree != other.degree)
                return false;

            int degree = other.degree;

            for (int i = 0; i <= degree; i++)
            {
                if (this.coeffs[i] != other.coeffs[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current polynomial.
        /// </summary>
        /// <param name="obj">The object to compare with the current polynomial.</param>
        /// <returns>true if the specified object is a polynomial with identical coefficients; otherwise false.</returns>
        /// <remarks>
        /// Performs coefficient-wise comparison modulo the current field. <br/>
        /// Null objects or non-polynomial types return false.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (!(obj is Polynomial))
                return false;

            Polynomial other = (Polynomial)obj;
            return Equals(other);
        }

        /// <summary>
        /// Gets the coefficient at the specified degree.
        /// </summary>
        /// <param name="index">The degree of the term (0 = constant term).</param>
        /// <returns>The coefficient at the specified degree, or 0 if index exceeds polynomial degree.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when index is negative.</exception>
        /// <remarks>
        /// Returns 0 for indices greater than the polynomial degree, <br/>treating missing higher-degree
        /// terms as zero coefficients. <br/>This simplifies polynomial arithmetic by allowing
        /// access <br/>beyond the current degree without bounds checking.
        /// </remarks>
        public BigInteger GetCoeff(int index)
        {
            if (index > degree)
                return 0;
            else
                if (index >= 0 && index <= degree)
                return coeffs[index];
            else
                throw new IndexOutOfRangeException("Index out of range.");
        }

        /// <summary>
        /// Implicitly converts an integer to a constant polynomial.
        /// </summary>
        /// <param name="val">The integer value.</param>
        /// <returns>A constant polynomial equal to the specified integer.</returns>
        /// <remarks>
        /// Allows natural usage of integers in polynomial contexts: <br/>
        /// Polynomial p = 5;  // Creates constant polynomial 5
        /// </remarks>
        public static implicit operator Polynomial(int val)
        {
            return new Polynomial((BigInteger)val);
        }

        /// <summary>
        /// Implicitly converts a big integer to a constant polynomial.
        /// </summary>
        /// <param name="val">The big integer value.</param>
        /// <returns>A constant polynomial equal to the specified big integer.</returns>
        /// <remarks>
        /// Allows natural usage of big integers in polynomial contexts: <br/>
        /// Polynomial p = new BigInteger(123);  // Creates constant polynomial 123
        /// </remarks>
        public static implicit operator Polynomial(BigInteger val)
        {
            return new Polynomial(val);
        }

        /// <summary>
        /// Returns a hash code for this polynomial instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        /// <remarks>
        /// The hash code is derived from the internal coefficient array.<br/>
        /// Two equal polynomials will produce the same hash code.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + degree.GetHashCode();

                for (int i = 0; i <= degree; i++)
                    hash = hash * 31 + coeffs[i].GetHashCode();

                return hash;
            }
        }
    }
}
