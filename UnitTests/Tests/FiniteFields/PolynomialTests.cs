using System;
using Eduard.Security;
using System.Collections.Generic;

namespace Eduard.Tests.FiniteFields
{
    [Collection("Sequential")]
    public class PolynomialTests
    {
        #region Primes Setup and Utilities

        static readonly BigInteger P256 = BigInteger.Pow(2, 256) - BigInteger.Pow(2, 224) +
                BigInteger.Pow(2, 192) + BigInteger.Pow(2, 96) - 1;
        static readonly BigInteger Ed25519 = BigInteger.Pow(2, 255) - 19;

        static Polynomial GetRandomPoly(int degree, BigInteger field)
        {
            Polynomial poly = new Polynomial(degree);

            for (int k = 0; k <= degree; k++)
                poly.coeffs[k] = SecureRandom.Range(0, field - 1);

            return poly;
        }

        static Polynomial GetPolyFromRoots(List<BigInteger> roots, BigInteger field)
        {
            int k, rootsCount = roots.Count;
            Polynomial result = 1;

            for (k = 1; k < rootsCount; k++)
            {
                BigInteger a = field - roots[k];
                Polynomial Pa = new Polynomial(1, a);
                result *= Pa;
            }

            return result;
        }

        static List<BigInteger> GenRoots(int rootsCount, BigInteger field)
        {
            var result = new List<BigInteger>();
            int counter = rootsCount;

            while (counter > 0)
            {
                var root = SecureRandom.Range(1, field - 1);
                result.Add(root);
                counter--;
            }

            return result;
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_Degree_CreatesZeroPolynomial()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(3);
            Assert.True(p.Degree == 3);
            Assert.True(p.coeffs.Length == 4);

            for (int i = 0; i <= 3; i++)
                Assert.True(p.GetCoeff(i) == 0);
        }

        [Fact]
        public void Constructor_Degree_Negative_ThrowsArgumentOutOfRange()
        {
            Polynomial.SetField(P256);
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                new Polynomial(-1));
        }

        [Fact]
        public void Constructor_Params_DescendingCoefficients_StoredAscending()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(3, 5, 7);

            Assert.True(p.Degree == 2);
            Assert.True(p.GetCoeff(2) == 3);

            Assert.True(p.GetCoeff(1) == 5);
            Assert.True(p.GetCoeff(0) == 7);
        }

        [Fact]
        public void Constructor_Params_ReducesModuloField()
        {
            Polynomial.SetField(P256);
            Polynomial p = 20;
            Assert.True(p.GetCoeff(0) == 20 % P256);
        }

        [Fact]
        public void Constructor_Params_Null_ThrowsArgumentNull()
        {
            Polynomial.SetField(P256);
            BigInteger[] arr = null;

            Assert.Throws<ArgumentNullException>(() => 
                new Polynomial(arr));
        }

        [Fact]
        public void Constructor_Params_Empty_ThrowsArgumentException()
        {
            Polynomial.SetField(P256);
            Assert.Throws<ArgumentException>(() => 
                new Polynomial(new BigInteger[0]));
        }

        [Fact]
        public void Constructor_Copy_IsIndependent()
        {
            Polynomial.SetField(P256);
            Polynomial original = new Polynomial(1, 2, 3);

            Polynomial copy = new Polynomial(original);
            Assert.True(original == copy);

            copy.coeffs[0] = 99;
            Assert.False(original == copy);
        }

        [Fact]
        public void Constructor_ImplicitFromInt_ConstantPolynomial()
        {
            Polynomial.SetField(P256);
            Polynomial p = 42;
            Assert.True(p.Degree == 0);
            Assert.True(p.GetCoeff(0) == 42);
        }

        [Fact]
        public void Constructor_ImplicitFromBigInteger_ConstantReduced()
        {
            Polynomial.SetField(P256);
            BigInteger val = P256 + 10;

            Polynomial p = val;
            Assert.True(p.GetCoeff(0) == 10);
        }

        #endregion

        #region SetField Tests

        [Fact]
        public void SetField_InvalidLessThan5_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 
                Polynomial.SetField(3));
        }

        [Fact]
        public void SetField_Composite_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 
                Polynomial.SetField(15));
        }

        [Fact]
        public void SetField_ChangesModulusCorrectly()
        {
            Polynomial.SetField(P256);
            Polynomial p1 = new Polynomial(P256 + 7);
            Assert.True(p1.GetCoeff(0) == 7);

            Polynomial.SetField(Ed25519);
            Polynomial p2 = new Polynomial(Ed25519 + 7);

            Assert.True(p2.GetCoeff(0) == 7);
            Polynomial.SetField(P256);
        }

        #endregion

        #region Arithmetic - Addition

        [Fact]
        public void Add_SameDegree_ReturnsCorrectSum()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(3, 5, 7);
            Polynomial b = new Polynomial(2, 1, 4);

            Polynomial sum = a + b;
            Assert.True(sum.GetCoeff(2) == 5);

            Assert.True(sum.GetCoeff(1) == 6);
            Assert.True(sum.GetCoeff(0) == 11);
        }

        [Fact]
        public void Add_DifferentDegrees_ExtendsCorrectly()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(5, 1);

            Polynomial b = new Polynomial(2, 0, 3);
            Polynomial sum = a + b;

            Assert.True(sum.Degree == 2);
            Assert.True(sum.GetCoeff(2) == 2);

            Assert.True(sum.GetCoeff(1) == 5);
            Assert.True(sum.GetCoeff(0) == 4);
        }

        [Fact]
        public void Add_WithZero_ReturnsOther()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(3, 2, 1);
            Polynomial zero = new Polynomial();

            Assert.True(p + zero == p);
            Assert.True(zero + p == p);
        }

        [Fact]
        public void Add_Commutative()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(2, 3);
            Polynomial b = new Polynomial(5, 1);
            Assert.True(a + b == b + a);
        }

        [Fact]
        public void Add_TermCancellation_YieldsZero()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(5, 10);
            Polynomial b = new Polynomial(P256 - 5, P256 - 10);
            Assert.True((a + b) == 0);
        }

        #endregion

        #region Arithmetic - Subtraction

        [Fact]
        public void Sub_SameDegree_ReturnsDifference()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(5, 10, 3);
            Polynomial b = new Polynomial(2, 4, 1);

            Polynomial diff = a - b;
            Assert.True(diff.GetCoeff(2) == 3);

            Assert.True(diff.GetCoeff(1) == 6);
            Assert.True(diff.GetCoeff(0) == 2);
        }

        [Fact]
        public void Sub_Self_YieldsZero()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(4, 3, 2);
            Assert.True((p - p) == 0);
        }

        [Fact]
        public void Sub_FromZero_Negation()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(5, 1);

            Polynomial zero = new Polynomial(0);
            Polynomial neg = zero - p;

            Assert.True(neg.GetCoeff(1) == P256 - 5);
            Assert.True(neg.GetCoeff(0) == P256 - 1);
        }

        #endregion

        #region Arithmetic - Multiplication

        [Fact]
        public void Mul_ConstantFactor_ScalesCoefficients()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(3, 4);
            Polynomial result = p * 2;

            Assert.True(result.GetCoeff(1) == 6);
            Assert.True(result.GetCoeff(0) == 8);
        }

        [Fact]
        public void Mul_TwoLinears_ReturnsQuadratic()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(1, 1);

            Polynomial b = new Polynomial(1, 1);
            Polynomial prod = a * b;

            Assert.True(prod.Degree == 2);
            Assert.True(prod.GetCoeff(2) == 1);

            Assert.True(prod.GetCoeff(1) == 2);
            Assert.True(prod.GetCoeff(0) == 1);
        }

        [Fact]
        public void Mul_Zero_ReturnsZero()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(5, 4);
            Assert.True(p * 0 == 0);
            Assert.True(0 * p == 0);
        }

        [Fact]
        public void Mul_Commutative()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(1, 2);
            Polynomial b = new Polynomial(3, 4);
            Assert.True(a * b == b * a);
        }

        [Fact]
        public void Mul_Distributive()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(1, 0);
            Polynomial b = new Polynomial(0, 2);
            Polynomial c = new Polynomial(3);
            Assert.True(a * (b + c) == a * b + a * c);
        }

        [Fact]
        public void Mul_ModularReduction_Applied()
        {
            Polynomial.SetField(17);
            Polynomial a = new Polynomial(10, 1);
            Polynomial b = new Polynomial(10, 0);

            Polynomial prod = a * b;
            Assert.True(prod.GetCoeff(2) == 15);

            Assert.True(prod.GetCoeff(1) == 10);
            Polynomial.SetField(P256);
        }

        [Fact]
        public void Mul_DegreeUpTo8_WorksWithoutFFT()
        {
            Polynomial a = new Polynomial(8);
            a.coeffs[0] = 1; a.coeffs[8] = 1;

            Polynomial b = new Polynomial(8);
            b.coeffs[0] = 1; b.coeffs[8] = 1;
            Polynomial prod = a * b;

            Assert.True(prod.Degree == 16);
            Assert.True(prod.GetCoeff(16) == 1);

            Assert.True(prod.GetCoeff(8) == 2);
            Assert.True(prod.GetCoeff(0) == 1);
        }

        #endregion

        #region Arithmetic - Division and Modulus

        [Fact]
        public void Div_ByZero_ThrowsDivideByZeroException()
        {
            Assert.Throws<DivideByZeroException>(() =>
            {
                Polynomial.SetField(P256);
                var res = new Polynomial(1, 2) / 0;
            });
        }

        [Fact]
        public void Div_DegreeNumeratorLessThanDenominator_ReturnsZero()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(1, 2); 
            Polynomial b = new Polynomial(1, 0, 3);
            Assert.True((a / b) == 0);
        }

        [Fact]
        public void Div_ByConstant_ScaledByInverse()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(2, 4);
            Polynomial q = a / 2;

            Assert.True(q.GetCoeff(1) == 1);
            Assert.True(q.GetCoeff(0) == 2);
        }

        [Fact]
        public void Div_ExactDivision_ReturnsCorrectQuotient()
        {
            Polynomial.SetField(P256);
            Polynomial num = new Polynomial(1, 3, 2);
            Polynomial den = new Polynomial(1, 1);

            Polynomial q = num / den;
            Assert.True(q.Degree == 1);

            Assert.True(q.GetCoeff(1) == 1);
            Assert.True(q.GetCoeff(0) == 2);
        }

        [Fact]
        public void Div_WithRemainder_QuotientCorrect()
        {
            Polynomial.SetField(17);
            Polynomial num = new Polynomial(1, 2, 3);
            Polynomial den = new Polynomial(1, 4);

            Polynomial q = num / den;
            Assert.True(q.GetCoeff(1) == 1);

            Assert.True(q.GetCoeff(0) == 15);
            Polynomial.SetField(P256);
        }

        [Fact]
        public void Mod_RemainderDegreeLessThanDivisor()
        {
            Polynomial.SetField(17);
            Polynomial num = new Polynomial(1, 2, 3);
            Polynomial den = new Polynomial(1, 4);

            Polynomial rem = num % den;
            Assert.True(rem.Degree < den.Degree);

            Assert.True(rem.GetCoeff(0) == 11);
            Polynomial.SetField(P256);
        }

        [Fact]
        public void Mod_ExactDivision_ReturnsZero()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(1, 1);
            Polynomial b = new Polynomial(1, 1);
            Assert.True(((a * b) % a) == 0);
        }

        [Fact]
        public void Mod_DegreeNumeratorLessThanDenominator_ReturnsNumerator()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(1, 2);
            Polynomial b = new Polynomial(1, 0, 3);
            Assert.True(a % b == a);
        }

        [Fact]
        public void MultMod_ReturnsProductReduced()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(1, 1);
            Polynomial b = new Polynomial(1, 1);

            Polynomial m = new Polynomial(1, 0, 1);
            Polynomial res = Polynomial.MultMod(a, b, m);

            Assert.True(res.GetCoeff(1) == 2);
            Assert.True(res.GetCoeff(0) == 0);
        }

        #endregion

        #region Exponentiation (int exponent)

        [Fact]
        public void Pow_ZeroBaseZeroExponent_ThrowsArithmeticException()
        {
            Assert.Throws<ArithmeticException>(() => {
                Polynomial.SetField(P256);
                Polynomial.Pow(0, 0);
            });
        }

        [Fact]
        public void Pow_NegativeExponent_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                Polynomial.SetField(P256);
                Polynomial.Pow(1, -1);
            });
        }

        [Fact]
        public void Pow_ExponentZero_ReturnsOne()
        {
            Polynomial.SetField(P256);
            Polynomial p = 5;
            Assert.True(Polynomial.Pow(p, 0) == 1);
        }

        [Fact]
        public void Pow_ExponentOne_ReturnsInput()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(2, 3);
            Assert.True(Polynomial.Pow(p, 1) == p);
        }

        [Fact]
        public void Pow_SmallExponent_Works()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(1, 1);

            Polynomial p3 = Polynomial.Pow(p, 3);
            Assert.True(p3.Degree == 3);

            Assert.True(p3.GetCoeff(3) == 1);
            Assert.True(p3.GetCoeff(2) == 3);

            Assert.True(p3.GetCoeff(1) == 3);
            Assert.True(p3.GetCoeff(0) == 1);
        }

        #endregion

        #region Modular Exponentiation (BigInteger exponent)

        [Fact]
        public void PowMod_ModulusZero_ThrowsDivideByZeroException()
        {
            Assert.Throws<DivideByZeroException>(() => { 
                Polynomial.SetField(P256); 
                Polynomial.Pow(1, 5, 0); }
            );
        }

        [Fact]
        public void PowMod_NegativeExponent_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { 
                Polynomial.SetField(P256); 
                Polynomial.Pow(1, -1, 1); 
            });
        }

        [Fact]
        public void PowMod_ZeroBaseZeroExponent_ThrowsArithmeticException()
        {
            Assert.Throws<ArithmeticException>(() => { 
                Polynomial.SetField(P256); 
                Polynomial.Pow(0, 0, 1); 
            });
        }

        [Fact]
        public void PowMod_ExponentZero_ReturnsOne()
        {
            Polynomial.SetField(P256);
            Polynomial p = 5;

            Polynomial m = new Polynomial(1, 0, 1);
            Assert.True(Polynomial.Pow(p, 0, m) == 1);
        }

        [Fact]
        public void PowMod_ExponentOne_ReturnsBaseReduced()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(1, 0, 2);
            Polynomial m = new Polynomial(1, 0, 1);

            Polynomial result = Polynomial.Pow(p, 1, m);
            Assert.True(result == p % m);
        }

        [Fact]
        public void PowMod_SmallModulus_UsesBinaryMethod()
        {
            Polynomial.SetField(17);
            Polynomial basePoly = new Polynomial(1, 1);

            Polynomial mod = new Polynomial(1, 0, 1);
            BigInteger exp = 3;

            Polynomial res = Polynomial.Pow(basePoly, exp, mod);
            Assert.True(res.GetCoeff(1) == 2);

            Assert.True(res.GetCoeff(0) == 15);
            Polynomial.SetField(P256);
        }

        #endregion

        #region GCD Computation

        [Fact]
        public void Gcd_One_And_Polynomial_ReturnsOne()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(5, 3);
            Assert.True(Polynomial.Gcd(p, 1) == 1);
            Assert.True(Polynomial.Gcd(1, p) == 1);
        }

        [Fact]
        public void Gcd_Zero_And_Polynomial_ReturnsPolynomial()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(2, 1);
            Assert.True(Polynomial.Gcd(p, 0) == p);
            Assert.True(Polynomial.Gcd(0, p) == p);
        }

        [Fact]
        public void Gcd_MonicResult()
        {
            Polynomial.SetField(P256);
            Polynomial a = new Polynomial(2, 0, 8);
            Polynomial b = new Polynomial(4, 0, 16);

            Polynomial g = Polynomial.Gcd(a, b);
            Assert.True(g.GetCoeff(g.Degree) == 1);

            Assert.True(g.GetCoeff(2) == 1);
            Assert.True(g.GetCoeff(0) == 4);
        }

        #endregion

        #region Horner Evaluation

        [Fact]
        public void Horner_ZeroPolynomial_ReturnsZero()
        {
            Polynomial.SetField(P256);
            Polynomial G = 0;
            BigInteger eval = Polynomial.Horner(G, 5);
            Assert.True(eval == 0);
        }

        [Fact]
        public void Horner_OnePolynomial_ReturnOne()
        {
            Polynomial.SetField(P256);
            Polynomial G = 1;

            BigInteger eval = Polynomial.Horner(G, 3);
            Assert.True(eval == 1);
        }

        [Fact]
        public void Horner_Constant_ReturnsConstant()
        {
            Polynomial.SetField(P256);
            Polynomial G = 7;
            BigInteger eval = Polynomial.Horner(G, 10);
            Assert.True(eval == 7);
        }

        [Fact]
        public void Horner_Linear_EvaluatesCorrectly()
        {
            Polynomial.SetField(P256);
            Polynomial G = new Polynomial(3, 2);
            BigInteger val = Polynomial.Horner(G, 5);
            Assert.True(val == (17 % P256));
        }

        [Fact]
        public void Horner_Quadratic_EvaluatesCorrectly()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(1, -11, 24);

            /* check if roots cancel the polynomial equation */
            BigInteger val = Polynomial.Horner(p, 3);
            Assert.True(val == 0);

            val = Polynomial.Horner(p, 8);
            Assert.True(val == 0);
        }

        [Fact]
        public void Horner_OutsideFieldRange_ThrowsArgumentOutOfRange()
        {
            Polynomial.SetField(P256);
            Polynomial p = 1;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Polynomial.Horner(p, P256));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Polynomial.Horner(p, -1));
        }

        #endregion

        #region Composition

        [Fact]
        public void Compose_LinearIntoLinear_ReturnsCorrect()
        {
            Polynomial.SetField(P256);
            Polynomial P = new Polynomial(2, 1);

            Polynomial Q = new Polynomial(3, 0);
            Polynomial result = Polynomial.Compose(P, Q);

            Assert.True(result.GetCoeff(1) == 6);
            Assert.True(result.GetCoeff(0) == 1);
        }

        [Fact]
        public void Compose_ZeroOuter_ReturnsZero()
        {
            Polynomial.SetField(P256);
            Polynomial Q = new Polynomial(1, 2);
            Assert.True(Polynomial.Compose(0, Q) == 0);
        }

        [Fact]
        public void Compose_Modular_Basic()
        {
            Polynomial.SetField(P256);
            Polynomial P = new Polynomial(1, 1);
            Polynomial Q = new Polynomial(1, 1);

            Polynomial mod = new Polynomial(1, 0, 1);
            Polynomial res = Polynomial.Compose(P, Q, mod, false);

            Assert.True(res.GetCoeff(1) == 1);
            Assert.True(res.GetCoeff(0) == 2);
        }

        #endregion

        #region Solve (Degree 1 and 2)

        [Fact]
        public void Solve_Degree1_ReturnsRoot()
        {
            Polynomial.SetField(17);
            Polynomial p = new Polynomial(3, 6);

            List<BigInteger> roots = new List<BigInteger>();
            int ret = Polynomial.Solve(p, ref roots);

            Assert.True(ret == 1);
            Assert.True(roots.Count == 1);
            Assert.True(roots[0] == 15);
        }

        [Fact]
        public void Solve_Degree2_NoRoots_ReturnsMinusOne()
        {
            Polynomial.SetField(17);
            Polynomial p = new Polynomial(1, 0, 3);

            List<BigInteger> roots = new List<BigInteger>();
            int ret = Polynomial.Solve(p, ref roots);

            Assert.True(ret == -1);
            Assert.True(roots.Count == 0);
        }

        [Fact]
        public void Solve_Degree2_HasRoots_ReturnsTwo()
        {
            Polynomial.SetField(17);
            Polynomial p = new Polynomial(1, 0, 15);

            List<BigInteger> roots = new List<BigInteger>();
            int ret = Polynomial.Solve(p, ref roots);

            Assert.True(ret == 1);
            Assert.True(roots.Count == 2);

            Assert.Contains(6, roots);
            Assert.Contains(11, roots);
        }

        #endregion

        #region FindRoots (Higher Degree)

        [Fact]
        public void FindRoots_PolynomialWithRoots_FindsAll()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(1, 14, 3, 14, 8);

            List<BigInteger> roots = new List<BigInteger>();
            p.FindRoots(ref roots);

            var expectedRoots = new BigInteger[]
            {
                BigInteger.Parse("97760724979562125688104076753543978168852176537384239078018327437708905822447"),
                BigInteger.Parse("14708384506749648925872721302311855803994325867860222177754982456116863041220")
            };

            Assert.True(Polynomial.Horner(p, roots[0]) == 0);
            Assert.True(Polynomial.Horner(p, roots[1]) == 0);
            Assert.Contains(expectedRoots[0], roots);

            Assert.Contains(expectedRoots[1], roots);
            Assert.True(roots.Count == 2);
        }

        [Fact]
        public void FindRoots_NoRoots_ReturnsEmpty()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(1, -2, 14, 0, 3, 7);
            List<BigInteger> roots = new List<BigInteger>();

            p.FindRoots(ref roots);
            Assert.True(roots.Count == 0);
        }

        #endregion

        #region Shift and Truncation Operations

        [Fact]
        public void Mulxn_ShiftsUp()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(1, 2, 3);

            Polynomial shifted = Polynomial.Mulxn(p, 2);
            Assert.True(shifted.Degree == 4);

            Assert.True(shifted.GetCoeff(4) == 1);
            Assert.True(shifted.GetCoeff(3) == 2);

            Assert.True(shifted.GetCoeff(2) == 3);
            Assert.True(shifted.GetCoeff(1) == 0);
        }

        [Fact]
        public void Mulxn_NegativeShift_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                Polynomial.SetField(P256);
                Polynomial.Mulxn(1, -1);
            });
        }

        [Fact]
        public void Divxn_ShiftsDown()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(10, 20, 30, 40);

            Polynomial result = Polynomial.Divxn(p, 2);
            Assert.True(result.Degree == 1);

            Assert.True(result.GetCoeff(1) == 10);
            Assert.True(result.GetCoeff(0) == 20);
        }

        [Fact]
        public void Divxn_BeyondDegree_ReturnsZero()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(1, 2);
            Assert.True(Polynomial.Divxn(p, 5) == 0);
        }

        [Fact]
        public void Modxn_TruncatesToDegreeNMinus1()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(5, 4, 3, 2, 1);

            Polynomial trunc = Polynomial.Modxn(p, 2);
            Assert.True(trunc.Degree == 1);

            Assert.True(trunc.GetCoeff(1) == 2);
            Assert.True(trunc.GetCoeff(0) == 1);
        }

        [Fact]
        public void Modxn_Negative_ThrowsArgumentOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Polynomial.SetField(P256);
                Polynomial.Modxn(1, 0);
            });
        }

        [Fact]
        public void Modxn_l_CyclicReduction()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(1, 2, 3, 4);
            Polynomial red = Polynomial.Modxn_l(p, 2);

            Assert.True(red.GetCoeff(1) == 4);
            Assert.True(red.GetCoeff(0) == 6);
        }

        [Fact]
        public void Invmodxn_InverseModX2()
        {
            Polynomial.SetField(17);
            Polynomial p = new Polynomial(1, 1);

            Polynomial inv = Polynomial.Invmodxn(p, 2);
            Assert.True(inv.GetCoeff(0) == 1);

            Assert.True(inv.GetCoeff(1) == 16);
            Polynomial.SetField(P256);
        }

        [Fact]
        public void Invmodxn_ZeroPolynomial_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Polynomial.SetField(P256);
                Polynomial.Invmodxn(0, 3);
            });
        }

        [Fact]
        public void Invmodxn_ConstantTermZero_ThrowsArgumentException()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(1, 0);

            Assert.Throws<ArgumentException>(() => 
                Polynomial.Invmodxn(p, 3));
        }

        #endregion

        #region Differentiation

        [Fact]
        public void Differentiate_Constant_ReturnsZero()
        {
            Polynomial.SetField(P256);
            Assert.True(Polynomial.Differentiate(5) == 0);
        }

        [Fact]
        public void Differentiate_Linear_ReturnsConstantCoefficient()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(3, 2);
            Polynomial d = Polynomial.Differentiate(p);

            Assert.True(d.Degree == 0);
            Assert.True(d.GetCoeff(0) == 3);
        }

        [Fact]
        public void Differentiate_Quadratic_Works()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(2, 3, 4);
            Polynomial d = Polynomial.Differentiate(p);

            Assert.True(d.GetCoeff(1) == 4);
            Assert.True(d.GetCoeff(0) == 3);
        }

        #endregion

        #region Equality and HashCode

        [Fact]
        public void Equality_SameCoefficients_ReturnsTrue()
        {
            Polynomial.SetField(Ed25519);
            Polynomial a = new Polynomial(1, 2, 3);

            Polynomial b = new Polynomial(1, 2, 3);
            Assert.True(a == b);

            Assert.False(a != b);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equality_DifferentDegree_ReturnsFalse()
        {
            Polynomial.SetField(Ed25519);
            Polynomial a = new Polynomial(1, 0);
            Polynomial b = 1;
            Assert.False(a == b);
        }

        [Fact]
        public void Equality_WithNullObject_ReturnsFalse()
        {
            Polynomial.SetField(Ed25519);
            Polynomial a = 1;
            object obj = null;
            Assert.False(a.Equals(obj));
        }

        [Fact]
        public void Equality_WithNonPolynomial_ReturnsFalse()
        {
            Polynomial.SetField(Ed25519);
            Polynomial a = 1;
            Assert.False(a.Equals("not a polynomial"));
        }

        [Fact]
        public void GetHashCode_SamePolynomial_SameHash()
        {
            Polynomial.SetField(Ed25519);
            Polynomial a = new Polynomial(3, 2, 1);
            Polynomial b = new Polynomial(3, 2, 1);
            Assert.True(a.GetHashCode() == b.GetHashCode());
        }

        #endregion

        #region Ed25519 Cross‑Check (Critical Operations)

        [Fact]
        public void Ed25519_BasicArithmetic_Works()
        {
            Polynomial.SetField(Ed25519);
            Polynomial a = new Polynomial(1, 2);

            Polynomial b = new Polynomial(3, 4);
            Polynomial sum = a + b;

            Assert.True(sum.GetCoeff(1) == 4);
            Assert.True(sum.GetCoeff(0) == 6);
            Polynomial prod = a * b;

            Assert.True(prod.Degree == 2);
            Assert.True(prod.GetCoeff(2) == 3);

            Assert.True(prod.GetCoeff(1) == 10);
            Assert.True(prod.GetCoeff(0) == 8);
        }

        [Fact]
        public void Ed25519_DivisionAndModulus_Works()
        {
            Polynomial.SetField(Ed25519);
            Polynomial num = new Polynomial(1, 3, 2);

            Polynomial den = new Polynomial(1, 1);
            Polynomial q = num / den;

            Assert.True(q.Degree == 1);
            Assert.True(q.GetCoeff(1) == 1);

            Assert.True(q.GetCoeff(0) == 2);
            Assert.True((num % den) == 0);

            Polynomial.SetField(P256);
        }

        #endregion

        #region Cryptographic Property Tests

        [Fact]
        public void FrobeniusEndomorphism_OnSplittingPolynomial_ReturnsIdentity()
        {
            Polynomial.SetField(P256);
            int[] degrees = { 12, 16, 18, 20 };
            int count = degrees.Length;
            int k;

            for(k = 0; k < count; k++)
            {
                int choosenDegree = degrees[k];
                var roots = GenRoots(choosenDegree, P256);

                var modulus = GetPolyFromRoots(roots, P256);
                Polynomial X = new Polynomial(1, 0);

                Polynomial XP = Polynomial.Pow(X, P256, modulus);
                Assert.Equal(XP, X);
            }
        }

        [Fact]
        public void DistinctDegreeFactorization_OnRandomPolynomials_YieldsValidFactors()
        {
            Polynomial.SetField(P256);
            int[] degrees = { 12, 16, 18, 20 };
            int count = degrees.Length;
            int j, k;

            for (j = 0; j < count; j++)
            {
                int choosenDegree = degrees[j];
                var modulus = GetRandomPoly(choosenDegree, P256);

                Polynomial X = new Polynomial(1, 0);
                Polynomial current = modulus;

                Polynomial product = 1;
                Polynomial XP = 0;

                bool needReset = true;
                Polynomial h = 0;
                int maxK = choosenDegree >> 1;

                for (k = 1; k <= maxK; k++)
                {
                    if (needReset)
                    {
                        XP = Polynomial.Pow(X, P256, current);
                        h = X;

                        for (int i = 1; i <= k; i++)
                            h = Polynomial.Compose(h, XP, current);

                        needReset = false;
                    }
                    else if (k == 1)
                    {
                        XP = Polynomial.Pow(X, P256, current);
                        h = XP;
                    }
                    else
                        h = Polynomial.Compose(h, XP, current);

                    Polynomial factor = Polynomial.Gcd(h - X, current);
                    Assert.True(current % factor == 0, $"Degree {choosenDegree}, " 
                        + $"k={k}: factor does not divide current modulus");

                    if (factor != 1)
                    {
                        Polynomial quotient = current / factor;
                        Assert.Equal(current, quotient * factor);

                        product *= factor;
                        current = quotient;
                        needReset = true;

                        if (current.Degree == 0)
                            break;
                    }
                }

                product *= current;
                Assert.Equal(modulus, product);
            }
        }

        #endregion
    }
}
