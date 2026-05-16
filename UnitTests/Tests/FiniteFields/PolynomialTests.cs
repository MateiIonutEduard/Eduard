using Eduard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eduard.Tests.FiniteFields
{
    [Collection("Sequential")]
    public class PolynomialTests
    {
        #region Primes Setup

        private static readonly BigInteger P256 = BigInteger.Pow(2, 256) - BigInteger.Pow(2, 224) +
                BigInteger.Pow(2, 192) + BigInteger.Pow(2, 96) - 1;
        private static readonly BigInteger Ed25519 = BigInteger.Pow(2, 255) - 19;

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_Degree_CreatesZeroPolynomial()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(3);
            Assert.True(p.degree == 3);
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

            Assert.True(p.degree == 2);
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
            Assert.True(p.degree == 0);
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

            Assert.True(sum.degree == 2);
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

            Assert.True(prod.degree == 2);
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

            Assert.True(prod.degree == 16);
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
            Assert.True(q.degree == 1);

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
            Assert.True(rem.degree < den.degree);

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
            Assert.True(p3.degree == 3);

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
            Assert.True(g.GetCoeff(g.degree) == 1);

            Assert.True(g.GetCoeff(2) == 1);
            Assert.True(g.GetCoeff(0) == 4);
        }

        #endregion

        #region Horner Evaluation

        [Fact]
        public void Horner_ZeroPolynomial_ReturnsZero()
        {
            Polynomial.SetField(P256);
            Assert.True(Polynomial.Horner(0, 5) == 0);
        }

        [Fact]
        public void Horner_Constant_ReturnsConstant()
        {
            Polynomial.SetField(P256);
            Polynomial p = 7;
            Assert.True(Polynomial.Horner(p, 10) == 7);
        }

        [Fact]
        public void Horner_Linear_EvaluatesCorrectly()
        {
            Polynomial.SetField(P256);
            Polynomial p = new Polynomial(3, 2);
            BigInteger val = Polynomial.Horner(p, 5);
            Assert.True(val == 17 % P256);
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
    }
}
