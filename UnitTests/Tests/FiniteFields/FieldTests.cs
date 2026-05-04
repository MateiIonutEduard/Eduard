using System;
using Eduard.Security;

namespace Eduard.Tests.FiniteFields
{
    [Collection("Sequential")]
    public class FieldTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_PositiveValue_ReducesModField()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            BigInteger value = P256 + 100;

            Field f = new Field(value);

            Assert.Equal(100, (BigInteger)f);
            Assert.True(f.fn >= 0);
            Assert.True(f.fn < P256);
        }

        [Fact]
        public void Constructor_NegativeValue_ReducesModField()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            BigInteger value = -50;

            Field f = new Field(value);

            Assert.Equal(P256 - 50, (BigInteger)f);
            Assert.True(f.fn >= 0);
            Assert.True(f.fn < P256);
        }

        [Fact]
        public void Constructor_Zero_ReducesCorrectly()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field f = new Field(0);

            Assert.Equal(0, (BigInteger)f);
            Assert.True(f.fn == 0);
        }

        [Fact]
        public void Constructor_ValueAlreadyInRange_NoReduction()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            BigInteger value = 123456789;

            Field f = new Field(value);

            Assert.Equal(value, (BigInteger)f);
        }

        [Fact]
        public void Constructor_MultipleOfModulus_ReducesToZero()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            BigInteger value = P256 * 3;

            Field f = new Field(value);

            Assert.Equal(0, (BigInteger)f);
        }

        [Fact]
        public void Constructor_VeryLargeValue_ProperlyReduced()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            BigInteger value = BigInteger.Pow(2, 500);

            Field f = new Field(value);

            Assert.True(f.fn >= 0);
            Assert.True(f.fn < P256);
            Assert.Equal(value % P256, (BigInteger)f);
        }

        #endregion

        #region SetField Validation Tests

        [Fact]
        public void SetField_ValidPrime_SetsModulus()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 100;
            Field b = 200;

            Field sum = a + b;
            Assert.Equal(300, (BigInteger)sum);
        }

        [Fact]
        public void SetField_PrimeLessThanFive_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => Field.SetField(3));
            Assert.Contains("less than 5", ex.Message);
        }

        [Fact]
        public void SetField_CompositeNumber_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                Field.SetField(81719));
            Assert.Contains("prime", ex.Message);
        }

        [Fact]
        public void SetField_EvenNumber_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                Field.SetField(90288));
            Assert.Contains("prime", ex.Message);
        }

        [Fact]
        public void SetField_SmallestValidPrime_SetsSuccessfully()
        {
            Field.SetField(5);
            Field a = 3;
            Field b = 4;

            Field sum = a + b;
            Assert.Equal(2, (BigInteger)sum);
        }

        [Fact]
        public void SetField_LargeCryptographicPrime_SetsSuccessfully()
        {
            BigInteger P384 = BigInteger.Parse("39402006196394479212279040100143613805079739270465446667948293404245721771496870329047266088258938001861606973112319");
            Field.SetField(P384);

            Field a = 100;
            Field b = 200;

            Field prod = a * b;
            Assert.Equal(20000, (BigInteger)prod);
        }

        [Fact]
        public void SetField_SwitchingModulus_WorksCorrectly()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 100;
            Field b = 200;

            Field sum256 = a + b;
            Assert.Equal(300, (BigInteger)sum256);

            BigInteger P384 = BigInteger.Parse("39402006196394479212279040100143613805079739270465446667948293404245721771496870329047266088258938001861606973112319");
            Field.SetField(P384);

            Field c = 100;
            Field d = 200;

            Field sum384 = c + d;
            Assert.Equal(300, (BigInteger)sum384);
        }

        #endregion

        #region Addition Tests

        [Fact]
        public void Addition_WithinField_NoOverflow()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 100;
            Field b = 200;

            Field result = a + b;
            Assert.Equal(300, (BigInteger)result);
        }

        [Fact]
        public void Addition_ExceedsModulus_ReducesCorrectly()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = P256 - 100;
            Field b = 200;

            Field result = a + b;
            Assert.Equal(100, (BigInteger)result);
        }

        [Fact]
        public void Addition_WithZero_PreservesValue()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 12345;
            Field zero = 0;

            Field result1 = a + zero;
            Field result2 = zero + a;

            Assert.Equal(a, result1);
            Assert.Equal(a, result2);
        }

        [Fact]
        public void Addition_Commutative_ProducesSameResult()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 12345;
            Field b = 67890;

            Field result1 = a + b;
            Field result2 = b + a;
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void Addition_Associative_ProducesSameResult()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 100;
            Field b = 200;
            Field c = 300;

            Field result1 = (a + b) + c;
            Field result2 = a + (b + c);
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void Addition_NegativeNumbers_ReducesCorrectly()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = new Field(-100);
            Field b = new Field(-200);

            Field result = a + b;
            Assert.Equal(P256 - 300, (BigInteger)result);
        }

        #endregion

        #region Subtraction Tests

        [Fact]
        public void Subtraction_PositiveResult_NoReduction()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 500;
            Field b = 200;

            Field result = a - b;
            Assert.Equal(300, (BigInteger)result);
        }

        [Fact]
        public void Subtraction_ZeroResult_ReducesToZero()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 100;
            Field b = 100;

            Field result = a - b;
            Assert.Equal(0, (BigInteger)result);
        }

        [Fact]
        public void Subtraction_NegativeResult_WrapsAround()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 100;
            Field b = 500;

            Field result = a - b;
            Assert.Equal(P256 - 400, (BigInteger)result);
        }

        [Fact]
        public void Subtraction_Anticommutative_NegationHolds()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 100;
            Field b = 200;

            Field result1 = a - b;
            Field result2 = -(b - a);
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void Subtraction_FromZero_ProducesNegation()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 100;
            Field zero = 0;

            Field result = zero - a;
            Assert.Equal(-a, result);
        }

        #endregion

        #region Negation Tests

        [Fact]
        public void Negation_PositiveValue_ProducesFieldComplement()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            Field a = 100;

            Field result = -a;
            Assert.Equal(P256 - 100, (BigInteger)result);
        }

        [Fact]
        public void Negation_Zero_RemainsZero()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            Field zero = 0;

            Field result = -zero;
            Assert.Equal(0, (BigInteger)result);
        }

        [Fact]
        public void Negation_DoubleNegation_ReturnsOriginal()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            Field a = 12345;

            Field result = -(-a);
            Assert.Equal(a, result);
        }

        [Fact]
        public void Negation_AdditiveInverse_SumsToZero()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 54321;
            Field negA = -a;

            Field sum = a + negA;
            Assert.Equal(0, (BigInteger)sum);
        }

        #endregion

        #region Multiplication Tests

        [Fact]
        public void Multiplication_WithinField_NoReductionNeeded()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 100;
            Field b = 200;

            Field result = a * b;
            Assert.Equal(20000, (BigInteger)result);
        }

        [Fact]
        public void Multiplication_ExceedsModulus_ReducesCorrectly()
        {
            Field.SetField(17);
            Field a = 5;
            Field b = 4;

            Field result = a * b;
            Assert.Equal(3, (BigInteger)result);
        }

        [Fact]
        public void Multiplication_ByZero_ProducesZero()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 12345;
            Field zero = 0;

            Field result1 = a * zero;
            Field result2 = zero * a;

            Assert.Equal(0, (BigInteger)result1);
            Assert.Equal(0, (BigInteger)result2);
        }

        [Fact]
        public void Multiplication_ByOne_PreservesValue()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 12345;
            Field one = 1;

            Field result1 = a * one;
            Field result2 = one * a;

            Assert.Equal(a, result1);
            Assert.Equal(a, result2);
        }

        [Fact]
        public void Multiplication_Commutative_ProducesSameResult()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 12345;
            Field b = 67890;

            Field result1 = a * b;
            Field result2 = b * a;
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void Multiplication_Associative_ProducesSameResult()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 100;
            Field b = 200;
            Field c = 300;

            Field result1 = (a * b) * c;
            Field result2 = a * (b * c);
            Assert.Equal(result1, result2);
        }

        [Fact]
        public void Multiplication_Distributive_OverAddition()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 10;
            Field b = 20;
            Field c = 30;

            Field left = a * (b + c);
            Field right = (a * b) + (a * c);
            Assert.Equal(left, right);
        }

        [Fact]
        public void Multiplication_LargeValues_HandlesCorrectly()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = P256 - 1;
            Field b = P256 - 1;

            Field result = a * b;
            Assert.Equal(1, (BigInteger)result);
        }

        #endregion

        #region Division Tests

        [Fact]
        public void Division_ByOne_ReturnsSameValue()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 12345;
            Field one = 1;

            Field result = a / one;
            Assert.Equal(a, result);
        }

        [Fact]
        public void Division_BySelf_ReturnsOne()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            Field a = 12345;

            Field result = a / a;
            Assert.Equal(new Field(1), result);
        }

        [Fact]
        public void Division_MultiplicationInverse_CombinesToOne()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 54321;
            Field b = 12345;

            Field quotient = a / b;
            Field product = quotient * b;
            Assert.Equal(a, product);
        }

        [Fact]
        public void Division_ByZero_ThrowsDivideByZeroException()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            Field a = 100;

            Field zero = 0;
            Assert.Throws<DivideByZeroException>(() => a / zero);
        }

        [Fact]
        public void Division_ZeroByNonZero_ReturnsZero()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field zero = 0;
            Field a = 100;

            Field result = zero / a;
            Assert.Equal(0, (BigInteger)result);
        }

        [Fact]
        public void Division_InverseMultiplication_RoundTrip()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);

            Field a = 999888777;
            Field b = 555444333;

            Field result = (a * b) / b;
            Assert.Equal(a, result);
        }

        #endregion

        #region Exponentiation Tests

        [Fact]
        public void Pow_ZeroExponent_ReturnsOne()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            Field b = 12345;

            Field result = Field.Pow(b, 0);
            Assert.Equal(new Field(1), result);
        }

        [Fact]
        public void Pow_OneExponent_ReturnsSameValue()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            Field b = 12345;

            Field result = Field.Pow(b, 1);
            Assert.Equal(b, result);
        }

        [Fact]
        public void Pow_LargeExponent_ComputesCorrectly()
        {
            Field.SetField(17);
            Field b = 3;
            Field result = Field.Pow(b, 5);
            Assert.Equal(5, (BigInteger)result);
        }

        [Fact]
        public void Pow_FermatLittleTheorem_HoldsForPrimeField()
        {
            Field.SetField(17);
            Field a = 3;

            Field result = Field.Pow(a, 16);
            Assert.Equal(new Field(1), result);
        }

        [Fact]
        public void Pow_BaseZero_ExponentZero_ThrowsArithmeticException()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            Field b = 0;

            Assert.Throws<ArithmeticException>(() =>
                Field.Pow(b, 0));
        }

        [Fact]
        public void Pow_BaseZero_PositiveExponent_ReturnsZero()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            Field b = 0;

            Field result = Field.Pow(b, 100);
            Assert.Equal(new Field(0), result);
        }

        [Fact]
        public void Pow_MultiplicationOfPowers_EquivalentToAdditionOfExponents()
        {
            BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            Field.SetField(P256);
            Field b = 12345;

            BigInteger k1 = 10;
            BigInteger k2 = 20;

            Field pow1 = Field.Pow(b, k1);
            Field pow2 = Field.Pow(b, k2);

            Field powCombined = Field.Pow(b, k1 + k2);
            Assert.Equal(pow1 * pow2, powCombined);
        }

        #endregion
    }
}
