using Eduard.Security;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
