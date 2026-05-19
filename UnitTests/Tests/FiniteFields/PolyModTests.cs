using Eduard.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eduard.Tests.FiniteFields
{
    [Collection("Sequential")]
    public class PolyModTests
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

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ModulusNotInitialized_ThrowsInvalidOperationException()
        {
            var isInitializedField = typeof(PolyMod).GetField("isInitialized",
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.NonPublic);

            Assert.NotNull(isInitializedField);

            var previousState = isInitializedField.GetValue(null);

            try
            {
                isInitializedField.SetValue(null, false);
                Polynomial.SetField(P256);

                Assert.Throws<InvalidOperationException>(() =>
                {
                    var p = new PolyMod(5);
                });
            }
            finally
            {
                isInitializedField.SetValue(null, previousState);
            }
        }

        [Fact]
        public void Constructor_Polynomial_AutomaticallyReduced()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            Polynomial p = new Polynomial(1, 0, 5);
            PolyMod element = new PolyMod(p);

            Assert.True(element.poly.Degree == 0);
            Assert.True(element.GetCoeff(0) == 4);
        }

        [Fact]
        public void Constructor_PolynomialDegreeLessThanModulus_ReturnsUnchanged()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            Polynomial p = new Polynomial(3, 7);

            PolyMod element = new PolyMod(p);
            Assert.True(element.poly.Degree == 1);

            Assert.True(element.GetCoeff(1) == 3);
            Assert.True(element.GetCoeff(0) == 7);
        }

        [Fact]
        public void Constructor_Copy_IsIndependent()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod original = new PolyMod(new Polynomial(1, 2));
            PolyMod copy = new PolyMod(original);

            Assert.True(original == copy);

            copy.poly.coeffs[0] = 99;
            Assert.False(original == copy);
        }

        [Fact]
        public void Constructor_ImplicitFromInt_ConstantElement()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod element = 42;

            Assert.True(element.poly.Degree == 0);
            Assert.True(element.GetCoeff(0) == 42);
        }

        [Fact]
        public void Constructor_ImplicitFromBigInteger_ReducedConstant()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            BigInteger val = P256 + 10;
            PolyMod element = val;

            Assert.True(element.GetCoeff(0) == 10);
        }

        [Fact]
        public void Constructor_ImplicitFromPolynomial_Works()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            Polynomial p = new Polynomial(2, 3);

            PolyMod element = p;
            Assert.True(element.poly == p);
        }

        #endregion

        #region SetModulus Tests

        [Fact]
        public void SetModulus_ZeroPolynomial_ThrowsDivideByZeroException()
        {
            Polynomial.SetField(P256);
            Assert.Throws<DivideByZeroException>(() =>
                PolyMod.SetModulus(0));
        }

        [Fact]
        public void SetModulus_NonZeroPolynomial_InitializesRing()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod element = new PolyMod(new Polynomial(1, 0, 5));

            Assert.True(element.poly.Degree == 0);
            Assert.True(element.GetCoeff(0) == 4);
        }

        [Fact]
        public void SetModulus_ChangesModulus_NewModulusTakesEffect()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod e1 = new PolyMod(new Polynomial(1, 0, 0));
            BigInteger val1 = e1.GetCoeff(0);

            PolyMod.SetModulus(new Polynomial(1, 0, 2));
            PolyMod e2 = new PolyMod(new Polynomial(1, 0, 0));

            BigInteger val2 = e2.GetCoeff(0);
            Assert.False(val1 == val2);
        }

        #endregion

        #region GetCoeff Tests

        [Fact]
        public void GetCoeff_WithinDegree_ReturnsCoefficient()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod element = new PolyMod(new Polynomial(3, 7));

            Assert.True(element.GetCoeff(1) == 3);
            Assert.True(element.GetCoeff(0) == 7);
        }

        [Fact]
        public void GetCoeff_BeyondDegree_ReturnsZero()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod element = new PolyMod(new Polynomial(3, 7));
            Assert.True(element.GetCoeff(2) == 0);
        }

        [Fact]
        public void GetCoeff_NegativeIndex_ThrowsIndexOutOfRange()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod element = 5;

            Assert.Throws<IndexOutOfRangeException>(() =>
                element.GetCoeff(-1));
        }

        #endregion

        #region Arithmetic - Addition

        [Fact]
        public void Add_SameDegree_ReturnsSumReduced()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod a = new PolyMod(new Polynomial(3, 5));
            PolyMod b = new PolyMod(new Polynomial(2, 1));
            PolyMod sum = a + b;

            Assert.True(sum.GetCoeff(1) == 5);
            Assert.True(sum.GetCoeff(0) == 6);
        }

        [Fact]
        public void Add_ResultExceedsModulusDegree_ReducedAutomatically()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod a = new PolyMod(new Polynomial(1, 0));
            PolyMod b = new PolyMod(new Polynomial(1, 0));
            PolyMod sum = a + b;

            Assert.True(sum.GetCoeff(1) == 2);
            Assert.True(sum.GetCoeff(0) == 0);
        }

        [Fact]
        public void Add_WithZero_ReturnsOther()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod p = new PolyMod(new Polynomial(3, 2));
            PolyMod zero = 0;

            Assert.True(p + zero == p);
            Assert.True(zero + p == p);
        }

        [Fact]
        public void Add_Commutative()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod a = new PolyMod(new Polynomial(2, 3));

            PolyMod b = new PolyMod(new Polynomial(5, 1));
            Assert.True(a + b == b + a);
        }

        [Fact]
        public void Add_TermCancellation_YieldsZero()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod a = new PolyMod(new Polynomial(5, 10));

            PolyMod b = new PolyMod(new Polynomial(P256 - 5, P256 - 10));
            Assert.True((a + b) == 0);
        }

        #endregion

        #region Arithmetic - Subtraction

        [Fact]
        public void Sub_SameDegree_ReturnsDifference()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod a = new PolyMod(new Polynomial(5, 10));
            PolyMod b = new PolyMod(new Polynomial(2, 4));
            PolyMod diff = a - b;

            Assert.True(diff.GetCoeff(1) == 3);
            Assert.True(diff.GetCoeff(0) == 6);
        }

        [Fact]
        public void Sub_Self_YieldsZero()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod p = new PolyMod(new Polynomial(4, 3));
            Assert.True((p - p) == 0);
        }

        [Fact]
        public void Sub_FromZero_Negation()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod p = new PolyMod(new Polynomial(5, 1));

            PolyMod zero = 0;
            PolyMod neg = zero - p;

            Assert.True(neg.GetCoeff(1) == P256 - 5);
            Assert.True(neg.GetCoeff(0) == P256 - 1);
        }

        #endregion

        #region Arithmetic - Multiplication

        [Fact]
        public void Mul_ConstantElements_ReturnsProduct()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod a = 3, b = 4;
            PolyMod prod = a * b;

            Assert.True(prod.poly.Degree == 0);
            Assert.True(prod.GetCoeff(0) == 12);
        }

        [Fact]
        public void Mul_LinearElements_ProductReduced()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod a = new PolyMod(new Polynomial(1, 1));
            PolyMod b = new PolyMod(new Polynomial(1, 2));

            PolyMod prod = a * b;
            Assert.True(prod.poly.Degree == 1);

            Assert.True(prod.GetCoeff(1) == 3);
            Assert.True(prod.GetCoeff(0) == 1);
        }

        [Fact]
        public void Mul_Zero_ReturnsZero()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod p = new PolyMod(new Polynomial(5, 4));

            Assert.True(p * 0 == 0);
            Assert.True(0 * p == 0);
        }

        [Fact]
        public void Mul_One_ReturnsOther()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod p = new PolyMod(new Polynomial(3, 7));

            Assert.True(p * 1 == p);
            Assert.True(1 * p == p);
        }

        [Fact]
        public void Mul_Commutative()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod a = new PolyMod(new Polynomial(1, 2));

            PolyMod b = new PolyMod(new Polynomial(3, 4));
            Assert.True(a * b == b * a);
        }

        [Fact]
        public void Mul_Distributive()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod a = new PolyMod(new Polynomial(1, 0));

            PolyMod b = new PolyMod(new Polynomial(0, 2));
            PolyMod c = 3;
            Assert.True(a * (b + c) == a * b + a * c);
        }

        #endregion

        #region Arithmetic - Scalar Division

        [Fact]
        public void ScalarDiv_ByConstant_ScaledByInverse()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod a = new PolyMod(new Polynomial(2, 4));
            PolyMod q = a / 2;

            Assert.True(q.GetCoeff(1) == 1);
            Assert.True(q.GetCoeff(0) == 2);
        }

        [Fact]
        public void ScalarDiv_ByZero_ThrowsDivideByZeroException()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod a = new PolyMod(new Polynomial(1, 2));
            Assert.Throws<DivideByZeroException>(() => { var r = a / 0; });
        }

        #endregion

        #region GCD Computation

        [Fact]
        public void Gcd_OfPolynomialWithModulus_ReturnsCorrect()
        {
            Polynomial.SetField(P256);
            Polynomial modulus = new Polynomial(1, 0, 1);
            PolyMod.SetModulus(modulus);

            Polynomial g = PolyMod.Gcd(new Polynomial(1, 1));
            Assert.True(g.Degree >= 1 || g == 1);
        }

        [Fact]
        public void Gcd_CoprimePolynomial_ReturnsOne()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 2));
            Polynomial g = PolyMod.Gcd(new Polynomial(1, 0));
            Assert.True(g == 1);
        }

        #endregion

        #region Modular Exponentiation

        [Fact]
        public void Pow_ExponentZero_ReturnsOne()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod p = 5;

            PolyMod result = PolyMod.Pow(p, 0);
            Assert.True(result == 1);
        }

        [Fact]
        public void Pow_ExponentOne_ReturnsInput()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod p = new PolyMod(new Polynomial(3, 2));

            PolyMod result = PolyMod.Pow(p, 1);
            Assert.True(result == p);
        }

        [Fact]
        public void Pow_SmallExponent_SmallModulus_Works()
        {
            Polynomial.SetField(17);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod X = new PolyMod(new Polynomial(1, 0));
            PolyMod X2 = PolyMod.Pow(X, 2);

            Assert.True(X2.GetCoeff(1) == 0);
            Assert.True(X2.GetCoeff(0) == 16);

            PolyMod X3 = PolyMod.Pow(X, 3);
            Assert.True(X3.GetCoeff(1) == 16);

            Assert.True(X3.GetCoeff(0) == 0);
            Polynomial.SetField(P256);
        }

        [Fact]
        public void Pow_FermatLittleTheorem_ForQuotientRing()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod a = new PolyMod(new Polynomial(3, 7));
            PolyMod a2 = PolyMod.Pow(a, 2);

            PolyMod a4 = PolyMod.Pow(a2, 2);
            Assert.True(a4 == PolyMod.Pow(a, 4));
        }

        [Fact]
        public void Pow_HigherDegreeModulus_ExponentiationConsistent()
        {
            Polynomial.SetField(P256);
            int[] degrees = { 10, 12, 16, 20, 24 };
            int k, degreesCount = degrees.Length;

            for (k = 0; k < degreesCount; k++)
            {
                var modulus = GetRandomPoly(degrees[k], P256);
                PolyMod.SetModulus(modulus);

                var a = new PolyMod(GetRandomPoly(degrees[k] - 1, P256));
                BigInteger exp = SecureRandom.Range(2, 100);

                var result1 = PolyMod.Pow(a, exp);
                var expected = (PolyMod)1;
                var baseCopy = a;
                var e = exp;

                while (e > 0)
                {
                    if ((e & 1) == 1)
                        expected = expected * baseCopy;
                    baseCopy = baseCopy * baseCopy;
                    e >>= 1;
                }

                Assert.True(result1 == expected,
                    $"Degree {degrees[k]}: Pow result inconsistent with manual exponentiation");
            }
        }

        [Fact]
        public void Pow_ExponentiationProperty_PowAdditive()
        {
            Polynomial.SetField(P256);
            int[] degrees = { 10, 12, 16, 20, 24 };
            int k, degreesCount = degrees.Length;

            for (k = 0; k < degreesCount; k++)
            {
                var modulus = GetRandomPoly(degrees[k], P256);
                PolyMod.SetModulus(modulus);

                var a = new PolyMod(GetRandomPoly(degrees[k] - 1, P256));
                BigInteger e1 = SecureRandom.Range(1, 50);
                BigInteger e2 = SecureRandom.Range(1, 50);

                var p1 = PolyMod.Pow(a, e1);
                var p2 = PolyMod.Pow(a, e2);
                var pSum = PolyMod.Pow(a, e1 + e2);

                Assert.True(p1 * p2 == pSum,
                    $"Degree {degrees[k]}: a^e1 * a^e2 != a^(e1+e2)");
            }
        }

        #endregion

        #region Modular Composition (Brent-Kung)

        [Fact]
        public void Compose_IdentityPolynomial_IsNeutralElement()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod P = new PolyMod(new Polynomial(3, 7));
            PolyMod X = new PolyMod(new Polynomial(1, 0));

            var R = PolyMod.Compose(P, X);
            Assert.True(R == P);

            R = PolyMod.Compose(X, P);
            Assert.True(R == P);
        }

        [Fact]
        public void Compose_ConstantOuter_ReturnsConstant()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod c = 5;

            PolyMod Q = new PolyMod(new Polynomial(2, 3));
            PolyMod result = PolyMod.Compose(c, Q);
            Assert.True(result == c);
        }

        [Fact]
        public void Compose_LinearIntoLinear_ReturnsCorrect()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod P = new PolyMod(new Polynomial(3, 2));

            PolyMod Q = new PolyMod(new Polynomial(4, 1));
            PolyMod result = PolyMod.Compose(P, Q);

            Assert.True(result.GetCoeff(1) == 12);
            Assert.True(result.GetCoeff(0) == 5);
        }

        [Fact]
        public void Compose_QuadraticIntoLinear_ModularReductionApplied()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod P = new PolyMod(new Polynomial(1, 1, 1));

            PolyMod X = new PolyMod(new Polynomial(1, 0));
            PolyMod result = PolyMod.Compose(P, X);

            Assert.True(result.GetCoeff(1) == 1);
            Assert.True(result.GetCoeff(0) == 0);
        }

        [Fact]
        public void Compose_HigherDegreeModulus_BrentKungConsistent()
        {
            Polynomial.SetField(P256);
            int[] degrees = { 10, 12, 16, 20, 24 };
            int k, degreesCount = degrees.Length;

            for (k = 0; k < degreesCount; k++)
            {
                var modulus = GetRandomPoly(degrees[k], P256);
                PolyMod.SetModulus(modulus);

                var X = new PolyMod(new Polynomial(1, 0));
                var P = new PolyMod(GetRandomPoly(degrees[k] - 1, P256));

                var R = PolyMod.Compose(P, X);
                Assert.True(R == P,
                    $"Degree {degrees[k]}: Compose(P, X) != P");

                R = PolyMod.Compose(X, P);
                Assert.True(R == P,
                    $"Degree {degrees[k]}: Compose(X, P) != P");
            }
        }

        [Fact]
        public void Compose_FrobeniusEndomorphism_PropertyHolds()
        {
            Polynomial.SetField(P256);
            int[] degrees = { 10, 12, 16, 20, 24 };
            int k, degreesCount = degrees.Length;

            for (k = 0; k < degreesCount; k++)
            {
                var modulus = GetRandomPoly(degrees[k], P256);
                PolyMod.SetModulus(modulus);

                var X = new PolyMod(new Polynomial(1, 0));
                var XP = PolyMod.Pow(X, P256);

                var XP2 = PolyMod.Compose(XP, XP);
                var XPP = PolyMod.Pow(XP, P256);

                Assert.True(XP2 == XPP,
                    $"Degree {degrees[k]}: Frobenius composition property failed");
            }
        }

        [Fact]
        public void Compose_WithPowerMap_MatchesDirectExponentiation()
        {
            Polynomial.SetField(P256);
            int[] degrees = { 10, 12, 16, 20, 24 };
            int k, degreesCount = degrees.Length;

            for (k = 0; k < degreesCount; k++)
            {
                var modulus = GetRandomPoly(degrees[k], P256);
                PolyMod.SetModulus(modulus);

                var X = new PolyMod(new Polynomial(1, 0));
                var F = new PolyMod(GetRandomPoly(degrees[k] - 1, P256));

                BigInteger exp = SecureRandom.Range(2, 20);
                var Xe = PolyMod.Pow(X, exp);

                var composed = PolyMod.Compose(F, Xe);
                PolyMod expected = 0;

                for (int i = 0; i <= F.poly.Degree; i++)
                {
                    var coeff = F.poly.GetCoeff(i);
                    if (coeff != 0)
                    {
                        var term = PolyMod.Pow(Xe, i);
                        expected = expected + (PolyMod)(coeff * term.poly);
                    }
                }

                Assert.True(composed == expected,
                    $"Degree {degrees[k]}: Compose(F, X^e) != direct evaluation");
            }
        }

        #endregion

        #region Evaluation (F)

        [Fact]
        public void F_ConstantElement_ReturnsConstant()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod c = 7;

            BigInteger val = c.F(5);
            Assert.True(val == 7);
        }

        [Fact]
        public void F_LinearElement_EvaluatesCorrectly()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod element = new PolyMod(new Polynomial(3, 2));

            BigInteger val = element.F(5);
            Assert.True(val == 17);
        }

        [Fact]
        public void F_OutsideFieldRange_ThrowsArgumentOutOfRange()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod element = 1;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                element.F(P256));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                element.F(-1));
        }

        #endregion

        #region Equality and HashCode

        [Fact]
        public void Equality_SameElement_ReturnsTrue()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod a = new PolyMod(new Polynomial(1, 2));
            PolyMod b = new PolyMod(new Polynomial(1, 2));

            Assert.True(a == b);
            Assert.False(a != b);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equality_DifferentElements_ReturnsFalse()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));

            PolyMod a = new PolyMod(new Polynomial(1, 0));
            PolyMod b = 1;

            Assert.False(a == b);
            Assert.True(a != b);
        }

        [Fact]
        public void Equality_WithNullObject_ReturnsFalse()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod a = 1;

            object obj = null;
            Assert.False(a.Equals(obj));
        }

        [Fact]
        public void GetHashCode_SameElement_SameHash()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod a = new PolyMod(new Polynomial(3, 2, 1));

            PolyMod b = new PolyMod(new Polynomial(3, 2, 1));
            Assert.True(a.GetHashCode() == b.GetHashCode());
        }

        #endregion

        #region Cryptographic Property Tests

        [Fact]
        public void RingOperations_FormAbelianGroupUnderAddition()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod a = new PolyMod(new Polynomial(3, 7));

            PolyMod b = new PolyMod(new Polynomial(1, 4));
            PolyMod c = new PolyMod(new Polynomial(2, 5));
            PolyMod zero = 0;

            Assert.True((a + b) + c == a + (b + c));
            Assert.True(a + zero == a);

            Assert.True(a + (zero - a) == 0);
            Assert.True(a + b == b + a);
        }

        [Fact]
        public void RingOperations_FormMonoidUnderMultiplication()
        {
            Polynomial.SetField(P256);
            PolyMod.SetModulus(new Polynomial(1, 0, 1));
            PolyMod a = new PolyMod(new Polynomial(3, 7));

            PolyMod b = new PolyMod(new Polynomial(1, 4));
            PolyMod c = new PolyMod(new Polynomial(2, 5));
            PolyMod one = 1;

            Assert.True((a * b) * c == a * (b * c));
            Assert.True(a * one == a);
            Assert.True(a * b == b * a);
        }

        [Fact]
        public void FrobeniusEndomorphism_OnSplittingPolynomial_ReturnsIdentity()
        {
            Polynomial.SetField(P256);
            int[] degrees = { 6, 8, 10 };
            int count = degrees.Length;
            int k;

            for (k = 0; k < count; k++)
            {
                int chosenDegree = degrees[k];
                var roots = new List<BigInteger>();
                int counter = chosenDegree;

                while (counter > 0)
                {
                    var root = SecureRandom.Range(1, P256 - 1);
                    roots.Add(root);
                    counter--;
                }

                Polynomial modulus = 1;

                for (int i = 0; i < roots.Count; i++)
                {
                    BigInteger negRoot = P256 - roots[i];
                    modulus *= new Polynomial(1, negRoot);
                }

                PolyMod.SetModulus(modulus);
                PolyMod X = new PolyMod(new Polynomial(1, 0));

                PolyMod XP = PolyMod.Pow(X, P256);
                Assert.True(XP == X,
                    $"Degree {chosenDegree}: Frobenius not identity on splitting polynomial");
            }
        }

        #endregion
    }
}
