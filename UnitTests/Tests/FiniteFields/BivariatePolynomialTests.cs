using System;
using Eduard.Security;

namespace Eduard.Tests.FiniteFields
{
    [Collection("Sequential")]
    public class BivariatePolynomialTests
    {
        #region Constructor Tests

        private static readonly BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");

        [Fact]
        public void Constructor_ConstantValue_CreatesConstantPolynomial()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(42);
            Assert.True(poly.GetCoeff(0, 0) == 42);
            Assert.False(poly.IsZero);
        }

        [Fact]
        public void Constructor_TermXY_CreatesMonomial()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(5, 2, 3);
            Assert.True(poly.GetCoeff(2, 3) == 5);

            Assert.True(BivariatePolynomial.GetDegreeX(poly) == 2);
            Assert.True(BivariatePolynomial.GetDegreeY(poly) == 3);
        }

        [Fact]
        public void Constructor_UnivariatePolynomial_CreatesBivariateWithYZero()
        {
            BivariatePolynomial.SetField(P256);
            var univariate = new Polynomial(new BigInteger[] { 1, 2, 3 });
            var poly = new BivariatePolynomial(univariate);

            Assert.True(BivariatePolynomial.GetDegreeX(poly) == 2);
            Assert.True(BivariatePolynomial.GetDegreeY(poly) == 0);
            Assert.True(poly.GetCoeff(2, 0) == 1);

            Assert.True(poly.GetCoeff(1, 0) == 2);
            Assert.True(poly.GetCoeff(0, 0) == 3);
        }

        [Fact]
        public void Constructor_CopyConstructor_CreatesDeepCopy()
        {
            BivariatePolynomial.SetField(P256);
            var original = new BivariatePolynomial(7, 1, 1);
            original.AddTerm(3, 0, 1);

            var copy = new BivariatePolynomial(original);
            Assert.True(original == copy);

            original.AddTerm(5, 2, 0);
            Assert.False(original == copy);
        }

        [Fact]
        public void Constructor_ZeroConstant_EqualsZero()
        {
            BivariatePolynomial.SetField(P256);
            var zeroPoly = new BivariatePolynomial(0);
            Assert.True(zeroPoly.IsZero);
            Assert.True(zeroPoly.GetCoeff(0, 0) == 0);
        }

        [Fact]
        public void Constructor_ValueOutsideField_ReducesModuloField()
        {
            BivariatePolynomial.SetField(P256);
            BigInteger tooBig = P256 + 100;
            var poly = new BivariatePolynomial(tooBig);
            Assert.True(poly.GetCoeff(0, 0) == 100);
        }

        #endregion

        #region AddTerm and Ordering

        [Fact]
        public void AddTerm_NewTerm_MaintainsLexicographicOrder()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial();
            poly.AddTerm(1, 0, 0);

            poly.AddTerm(2, 1, 0);
            poly.AddTerm(3, 0, 1);

            poly.AddTerm(4, 2, 0);
            poly.AddTerm(5, 1, 1);

            Assert.True(poly.GetCoeff(2, 0) == 4);
            Assert.True(poly.GetCoeff(1, 1) == 5);

            Assert.True(poly.GetCoeff(1, 0) == 2);
            Assert.True(poly.GetCoeff(0, 1) == 3);

            Assert.True(poly.GetCoeff(0, 0) == 1);
            Assert.True(BivariatePolynomial.GetDegreeX(poly) == 2);
        }

        [Fact]
        public void AddTerm_ExistingDegrees_CombinesCoefficients()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(3, 1, 0);
            poly.AddTerm(4, 1, 0);
            Assert.True(poly.GetCoeff(1, 0) == 7);
        }

        [Fact]
        public void AddTerm_SumToZero_RemovesTerm()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(5, 1, 1);
            poly.AddTerm(-5, 1, 1);

            Assert.True(poly.GetCoeff(1, 1) == 0);
            Assert.True(poly.IsZero);
        }

        [Fact]
        public void AddTerm_ReduceMod_AppliesFieldReduction()
        {
            BivariatePolynomial.SetField(17);
            var poly = new BivariatePolynomial(20, 0, 0);
            Assert.True(poly.GetCoeff(0, 0) == 3);

            poly.AddTerm(15, 0, 0);
            Assert.True(poly.GetCoeff(0, 0) == 1);
        }

        #endregion

        #region Arithmetic - Addition

        [Fact]
        public void Addition_TwoPolynomials_AddsCoefficients()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(2, 1, 0);
            a.AddTerm(3, 0, 1);

            var b = new BivariatePolynomial(4, 1, 0);
            b.AddTerm(5, 0, 0);
            var sum = a + b;

            Assert.True(sum.GetCoeff(1, 0) == 6);
            Assert.True(sum.GetCoeff(0, 1) == 3);
            Assert.True(sum.GetCoeff(0, 0) == 5);
        }

        [Fact]
        public void Addition_WithZero_OriginalUnchanged()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(42, 0, 0);
            var zero = BivariatePolynomial.Zero;

            var sum = poly + zero;
            Assert.True(sum == poly);
        }

        [Fact]
        public void Addition_Commutative()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(1, 2, 3);
            var b = new BivariatePolynomial(4, 5, 6);
            Assert.True(a + b == b + a);
        }

        [Fact]
        public void Addition_Associative()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(1, 0, 0);
            var b = new BivariatePolynomial(2, 1, 0);

            var c = new BivariatePolynomial(3, 0, 1);
            Assert.True((a + b) + c == a + (b + c));
        }

        [Fact]
        public void Addition_TermCancellation_YieldsCorrectResult()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(5, 1, 1);
            var b = new BivariatePolynomial(-5, 1, 1);

            var sum = a + b;
            Assert.True(sum.IsZero);
        }

        #endregion

        #region Arithmetic - Subtraction

        [Fact]
        public void Subtraction_Subtrahend_SubtractsCoefficients()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(10, 1, 0);
            a.AddTerm(7, 0, 1);

            var b = new BivariatePolynomial(3, 1, 0);
            b.AddTerm(2, 0, 1);
            var diff = a - b;

            Assert.True(diff.GetCoeff(1, 0) == 7);
            Assert.True(diff.GetCoeff(0, 1) == 5);
        }

        [Fact]
        public void Subtraction_FromZero_ProducesNegation()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(5, 1, 1);
            var zero = BivariatePolynomial.Zero;
            var neg = zero - poly;
            Assert.True(neg.GetCoeff(1, 1) == P256 - 5);
        }

        [Fact]
        public void Subtraction_Self_YieldsZero()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(3, 2, 1);
            poly.AddTerm(4, 0, 3);
            Assert.True((poly - poly).IsZero);
        }

        #endregion

        #region Arithmetic - Scalar Multiplication

        [Fact]
        public void ScalarMultiply_ByOne_ReturnsCopy()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(5, 1, 1);
            var result = (BigInteger)1 * poly;
            Assert.True(result == poly);
        }

        [Fact]
        public void ScalarMultiply_ByZero_ReturnsZero()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(100, 2, 2);
            var result = (BigInteger)0 * poly;
            Assert.True(result.IsZero);
        }

        [Fact]
        public void ScalarMultiply_ByFieldElement_AppliesModularMultiplication()
        {
            BivariatePolynomial.SetField(17);
            var poly = new BivariatePolynomial(3, 1, 0);

            poly.AddTerm(5, 0, 1);
            var scaled = (BigInteger)5 * poly;

            Assert.True(scaled.GetCoeff(1, 0) == (3 * 5) % 17);
            Assert.True(scaled.GetCoeff(0, 1) == (5 * 5) % 17);
        }

        #endregion

        #region Arithmetic - Polynomial Multiplication

        [Fact]
        public void Multiplication_ConstantTimesPolynomial_ScalesAllTerms()
        {
            BivariatePolynomial.SetField(P256);
            var constant = new BivariatePolynomial(3);
            var poly = new BivariatePolynomial(1, 2, 0);

            poly.AddTerm(2, 1, 1);
            var product = constant * poly;

            Assert.True(product.GetCoeff(2, 0) == 3);
            Assert.True(product.GetCoeff(1, 1) == 6);
        }

        [Fact]
        public void Multiplication_XYTermTimesXYTerm_IncreasesDegrees()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(2, 2, 1);

            var b = new BivariatePolynomial(3, 1, 2);
            var product = a * b;

            Assert.True(product.GetCoeff(3, 3) == 6);
            Assert.True(BivariatePolynomial.GetDegreeX(product) == 3);
            Assert.True(BivariatePolynomial.GetDegreeY(product) == 3);
        }

        [Fact]
        public void Multiplication_Commutative()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(1, 2, 3);
            a.AddTerm(4, 0, 1);

            var b = new BivariatePolynomial(5, 1, 0);
            b.AddTerm(6, 0, 0);
            Assert.True(a * b == b * a);
        }

        [Fact]
        public void Multiplication_Distributive_OverAddition()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(2, 1, 0);
            var b = new BivariatePolynomial(3, 0, 1);

            var c = new BivariatePolynomial(4, 0, 0);
            var left = a * (b + c);

            var right = a * b + a * c;
            Assert.True(left == right);
        }

        [Fact]
        public void Multiplication_ByZero_ReturnsZero()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(5, 3, 2);
            var zero = BivariatePolynomial.Zero;

            Assert.True((poly * zero).IsZero);
            Assert.True((zero * poly).IsZero);
        }

        [Fact]
        public void Multiplication_Associative()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(1, 1, 0);
            var b = new BivariatePolynomial(2, 0, 1);

            var c = new BivariatePolynomial(3, 1, 1);
            Assert.True((a * b) * c == a * (b * c));
        }

        [Fact]
        public void Multiplication_ModularReduction_AppliedToCoefficients()
        {
            BivariatePolynomial.SetField(17);
            var a = new BivariatePolynomial(10, 1, 0);
            var b = new BivariatePolynomial(10, 0, 1);

            var product = a * b;
            Assert.True(product.GetCoeff(1, 1) == 15);
        }

        #endregion

        #region Arithmetic - Division

        [Fact]
        public void Division_ExactDivision_ReturnsQuotient()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(2, 1, 1);

            var b = new BivariatePolynomial(3, 0, 0);
            var product = a * b;

            var quotient = product / a;
            Assert.True(quotient == b);
        }

        [Fact]
        public void Division_DividendLessDegree_ReturnsZero()
        {
            BivariatePolynomial.SetField(P256);
            var dividend = new BivariatePolynomial(5, 1, 0);
            var divisor = new BivariatePolynomial(1, 2, 2);
            Assert.True((dividend / divisor).IsZero);
        }

        [Fact]
        public void Division_BySelf_ReturnsOne()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(3, 2, 1);
            poly.AddTerm(4, 1, 3);

            var quotient = poly / poly;
            Assert.True(quotient == BivariatePolynomial.One);
        }

        [Fact]
        public void Division_ByZero_ThrowsDivideByZeroException()
        {
            BivariatePolynomial.SetField(P256);
            var dividend = new BivariatePolynomial(1, 0, 0);
            Assert.Throws<DivideByZeroException>(() => dividend / BivariatePolynomial.Zero);
        }

        [Fact]
        public void Division_WithRemainder_QuotientCorrect()
        {
            BivariatePolynomial.SetField(P256);
            var dividend = new BivariatePolynomial(2, 2, 0);
            dividend.AddTerm(1, 1, 0);

            var divisor = new BivariatePolynomial(1, 1, 0);
            var quotient = dividend / divisor;

            var expected = new BivariatePolynomial(2, 1, 0);
            expected.AddTerm(1, 0, 0);
            Assert.True(quotient == expected);
        }

        [Fact]
        public void Division_Remainder_LessThanDivisorDegrees()
        {
            BivariatePolynomial.SetField(P256);
            var dividend = new BivariatePolynomial(1, 1, 1);
            dividend.AddTerm(1, 0, 1);

            var divisor = new BivariatePolynomial(1, 1, 0);
            var quotient = dividend / divisor;

            var remainder = dividend % divisor;
            Assert.True(quotient.GetCoeff(0, 1) == 1);

            Assert.True(remainder.GetCoeff(0, 1) == 1);
            Assert.True(BivariatePolynomial.GetDegreeX(remainder) == 0);
        }

        #endregion

        #region Arithmetic - Modulus

        [Fact]
        public void Modulus_Remainder_DegreeLessThanDivisor()
        {
            BivariatePolynomial.SetField(17);
            var a = new BivariatePolynomial(1, 2, 0);

            a.AddTerm(1, 1, 0);
            a.AddTerm(1, 0, 0);

            var b = new BivariatePolynomial(1, 1, 0);
            b.AddTerm(1, 0, 0);
            var rem = a % b;

            Assert.True(rem.GetCoeff(0, 0) == 1);
            var remDegX = BivariatePolynomial.GetDegreeX(rem);

            var bDegX = BivariatePolynomial.GetDegreeX(b);
            Assert.True(remDegX < bDegX);
        }

        [Fact]
        public void Modulus_ExactDivision_ReturnsZero()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(2, 1, 0);
            var b = new BivariatePolynomial(3, 1, 0);
            Assert.True((a * b) % a == BivariatePolynomial.Zero);
        }

        [Fact]
        public void Modulus_DividendLessDegree_ReturnsDividend()
        {
            BivariatePolynomial.SetField(P256);
            var dividend = new BivariatePolynomial(5, 0, 1);
            var divisor = new BivariatePolynomial(1, 2, 0);

            var rem = dividend % divisor;
            Assert.True(rem == dividend);
        }

        #endregion

        #region Evaluation

        [Fact]
        public void EvaluateAtY_Univariate_X_Polynomial()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(1, 2, 0);

            poly.AddTerm(2, 1, 1);
            poly.AddTerm(3, 0, 2);

            Polynomial evalAt2 = poly.F(2);
            Assert.True(evalAt2.coeffs[0] == (3 * 4) % P256);
            Assert.True(evalAt2.coeffs[1] == (2 * 2) % P256);

            Assert.True(evalAt2.coeffs[2] == 1);
            Assert.True(evalAt2.Degree == 2);
        }

        [Fact]
        public void EvaluateAtX_Y_ConstantPolynomial_ReturnsConstant()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(42);
            var val = poly.F(100, 200);
            Assert.True(val == 42);
        }

        [Fact]
        public void EvaluateAtX_Y_Monomial_ComputesCorrectValue()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(5, 2, 3);
            BigInteger x = 3, y = 2;

            BigInteger expected = 5 * BigInteger.Pow(x, 2) * BigInteger.Pow(y, 3) % P256;
            Assert.True(poly.F(x, y) == expected);
        }

        [Fact]
        public void EvaluateAtY_ZeroPolynomial_ReturnsZeroPolynomial()
        {
            BivariatePolynomial.SetField(P256);
            var zero = BivariatePolynomial.Zero;
            var univariate = zero.F(123);

            Assert.True(univariate.Degree == 0);
            Assert.True(univariate.coeffs[0] == 0);
        }

        [Fact]
        public void EvaluateAtX_Y_ZeroPolynomial_ReturnsZero()
        {
            BivariatePolynomial.SetField(P256);
            var zero = BivariatePolynomial.Zero;
            Assert.True(zero.F(123, 456) == 0);
        }

        [Fact]
        public void EvaluateAtY_OutsideFieldRange_ThrowsArgumentOutOfRange()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(1, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => poly.F(P256));
            Assert.Throws<ArgumentOutOfRangeException>(() => poly.F(-1));
        }

        [Fact]
        public void EvaluateAtXY_OutsideFieldRange_ThrowsArgumentOutOfRange()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(1, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => poly.F(P256, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => poly.F(0, -1));
        }

        #endregion

        #region Differentiation

        [Fact]
        public void DiffX_XYMonomial_ReducesDegreeX()
        {
            BivariatePolynomial.SetField(17);
            var poly = new BivariatePolynomial(2, 3, 2);

            var diff = BivariatePolynomial.DiffX(poly);
            Assert.True(diff.GetCoeff(2, 2) == (2 * 3) % 17);

            Assert.True(BivariatePolynomial.GetDegreeX(diff) == 2);
            Assert.True(BivariatePolynomial.GetDegreeY(diff) == 2);
        }

        [Fact]
        public void DiffX_Constant_ReturnsZero()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(7);
            var diff = BivariatePolynomial.DiffX(poly);
            Assert.True(diff.IsZero);
        }

        [Fact]
        public void DiffX_PolynomialWithOnlyYTerms_ReturnsZero()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(3, 0, 4);
            var diff = BivariatePolynomial.DiffX(poly);
            Assert.True(diff.IsZero);
        }

        [Fact]
        public void DiffY_XYMonomial_ReducesDegreeY()
        {
            BivariatePolynomial.SetField(17);
            var poly = new BivariatePolynomial(3, 1, 4);
            var diff = BivariatePolynomial.DiffY(poly);

            Assert.True(diff.GetCoeff(1, 3) == (3 * 4) % 17);
            Assert.True(BivariatePolynomial.GetDegreeY(diff) == 3);
        }

        [Fact]
        public void DiffY_Constant_ReturnsZero()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(1);
            Assert.True(BivariatePolynomial.DiffY(poly).IsZero);
        }

        [Fact]
        public void DiffY_PolynomialWithOnlyXTerms_ReturnsZero()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(2, 5, 0);
            Assert.True(BivariatePolynomial.DiffY(poly).IsZero);
        }

        [Fact]
        public void DiffX_Summation_LinearBehavior()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(2, 2, 0);

            var b = new BivariatePolynomial(3, 0, 2);
            var diffSum = BivariatePolynomial.DiffX(a + b);

            var sumDiff = BivariatePolynomial.DiffX(a) + BivariatePolynomial.DiffX(b);
            Assert.True(diffSum == sumDiff);
        }

        [Fact]
        public void DiffY_Summation_LinearBehavior()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(2, 0, 2);

            var b = new BivariatePolynomial(3, 2, 0);
            var diffSum = BivariatePolynomial.DiffY(a + b);

            var sumDiff = BivariatePolynomial.DiffY(a) + BivariatePolynomial.DiffY(b);
            Assert.True(diffSum == sumDiff);
        }

        #endregion

        #region Degree and Coefficient

        [Fact]
        public void GetDegreeX_MixedPolynomial_ReturnsHighestX()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(1, 0, 5);
            poly.AddTerm(2, 7, 0);

            poly.AddTerm(3, 3, 2);
            Assert.True(BivariatePolynomial.GetDegreeX(poly) == 7);
        }

        [Fact]
        public void GetDegreeX_ZeroPolynomial_ReturnsZero()
        {
            Assert.True(BivariatePolynomial.GetDegreeX(BivariatePolynomial.Zero) == 0);
        }

        [Fact]
        public void GetDegreeY_MixedPolynomial_ReturnsHighestY()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(1, 10, 0);
            poly.AddTerm(2, 1, 9);

            poly.AddTerm(3, 0, 3);
            Assert.True(BivariatePolynomial.GetDegreeY(poly) == 9);
        }

        [Fact]
        public void GetDegreeY_ZeroPolynomial_ReturnsZero()
        {
            Assert.True(BivariatePolynomial.GetDegreeY(BivariatePolynomial.Zero) == 0);
        }

        [Fact]
        public void GetCoeff_ExistingTerm_ReturnsCoefficient()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(5, 2, 3);
            Assert.True(poly.GetCoeff(2, 3) == 5);
        }

        [Fact]
        public void GetCoeff_NonexistentTerm_ReturnsZero()
        {
            BivariatePolynomial.SetField(P256);
            var poly = new BivariatePolynomial(5, 2, 3);
            Assert.True(poly.GetCoeff(0, 0) == 0);
        }

        #endregion

        #region Equality

        [Fact]
        public void Equality_SamePolynomial_ReturnsTrue()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(1, 2, 3);
            a.AddTerm(4, 1, 1);

            var b = new BivariatePolynomial(1, 2, 3);
            b.AddTerm(4, 1, 1);

            Assert.True(a == b);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equality_DifferentPolynomial_ReturnsFalse()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(1, 2, 0);
            var b = new BivariatePolynomial(1, 2, 1);
            Assert.False(a == b);
        }

        [Fact]
        public void Equality_WithNull_ReturnsFalse()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(1);
            object someObject = null;
            Assert.False(a.Equals(someObject));
        }

        [Fact]
        public void Equality_WithDifferentType_ReturnsFalse()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(1);
            Assert.False(a.Equals("not a polynomial"));
        }

        [Fact]
        public void Inequality_Operator_OppositeOfEquality()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(5);

            var b = new BivariatePolynomial(5);
            var c = new BivariatePolynomial(6);

            Assert.False(a != b);
            Assert.True(a != c);
        }

        [Fact]
        public void GetHashCode_SamePolynomial_SameHash()
        {
            BivariatePolynomial.SetField(P256);
            var a = new BivariatePolynomial(1, 2, 3);

            a.AddTerm(4, 0, 1);
            var b = new BivariatePolynomial(1, 2, 3);

            b.AddTerm(4, 0, 1);
            Assert.True(a.GetHashCode() == b.GetHashCode());
        }

        #endregion

        #region Implicit Conversions

        [Fact]
        public void ImplicitFromBigInteger_ConstantPolynomial()
        {
            BivariatePolynomial.SetField(P256);
            BivariatePolynomial poly = (BigInteger)42;
            Assert.True(poly.GetCoeff(0, 0) == 42);
            Assert.True(poly == new BivariatePolynomial(42));
        }

        [Fact]
        public void ImplicitFromPolynomial_UnivariateToBivariate()
        {
            BivariatePolynomial.SetField(P256);
            var univariate = new Polynomial(new BigInteger[] { 2, 0, 1 });
            BivariatePolynomial poly = univariate;

            Assert.True(BivariatePolynomial.GetDegreeX(poly) == 2);
            Assert.True(BivariatePolynomial.GetDegreeY(poly) == 0);

            Assert.True(poly.GetCoeff(2, 0) == 2);
            Assert.True(poly.GetCoeff(0, 0) == 1);
        }

        #endregion
    }
}
