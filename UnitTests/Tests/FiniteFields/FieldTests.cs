using System;
using Eduard.Security;

namespace Eduard.Tests.FiniteFields
{
    [Collection("Sequential")]
    public class FieldTests
    {
        #region Prime Fields

        private static readonly BigInteger P256 = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
        private static readonly BigInteger P384 = BigInteger.Parse("39402006196394479212279040100143613805079739270465446667948293404245721771496870329047266088258938001861606973112319");

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_PositiveValue_ReducesModField()
        {
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
            Field.SetField(P256);
            Field f = new Field(0);
            Assert.Equal(0, (BigInteger)f);
            Assert.True(f.fn == 0);
        }

        [Fact]
        public void Constructor_ValueAlreadyInRange_NoReduction()
        {
            Field.SetField(P256);
            BigInteger value = 123456789;
            Field f = new Field(value);
            Assert.Equal(value, (BigInteger)f);
        }

        [Fact]
        public void Constructor_MultipleOfModulus_ReducesToZero()
        {
            Field.SetField(P256);
            BigInteger value = P256 * 3;
            Field f = new Field(value);
            Assert.Equal(0, (BigInteger)f);
        }

        [Fact]
        public void Constructor_VeryLargeValue_ProperlyReduced()
        {
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
            Field.SetField(P384);
            Field a = 100;

            Field b = 200;
            Field prod = a * b;
            Assert.Equal(20000, (BigInteger)prod);
        }

        [Fact]
        public void SetField_SwitchingModulus_WorksCorrectly()
        {
            Field.SetField(P256);
            Field a = 100;

            Field b = 200;
            Field sum256 = a + b;

            Assert.Equal(300, (BigInteger)sum256);
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
            Field.SetField(P256);
            Field a = 100;

            Field b = 200;
            Field result = a + b;
            Assert.Equal(300, (BigInteger)result);
        }

        [Fact]
        public void Addition_ExceedsModulus_ReducesCorrectly()
        {
            Field.SetField(P256);
            Field a = P256 - 100;
            Field b = 200;

            Field result = a + b;
            Assert.Equal(100, (BigInteger)result);
        }

        [Fact]
        public void Addition_WithZero_PreservesValue()
        {
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
            Field.SetField(P256);
            Field a = 500;
            Field b = 200;

            Field result = a - b;
            Assert.Equal(300, (BigInteger)result);
        }

        [Fact]
        public void Subtraction_ZeroResult_ReducesToZero()
        {
            Field.SetField(P256);
            Field a = 100;
            Field b = 100;

            Field result = a - b;
            Assert.Equal(0, (BigInteger)result);
        }

        [Fact]
        public void Subtraction_NegativeResult_WrapsAround()
        {
            Field.SetField(P256);
            Field a = 100;
            Field b = 500;

            Field result = a - b;
            Assert.Equal(P256 - 400, (BigInteger)result);
        }

        [Fact]
        public void Subtraction_Anticommutative_NegationHolds()
        {
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
            Field.SetField(P256);
            Field a = 100;

            Field result = -a;
            Assert.Equal(P256 - 100, 
                (BigInteger)result);
        }

        [Fact]
        public void Negation_Zero_RemainsZero()
        {
            Field.SetField(P256);
            Field zero = 0;
            Field result = -zero;
            Assert.Equal(0, (BigInteger)result);
        }

        [Fact]
        public void Negation_DoubleNegation_ReturnsOriginal()
        {
            Field.SetField(P256);
            Field a = 12345;
            Field result = -(-a);
            Assert.Equal(a, result);
        }

        [Fact]
        public void Negation_AdditiveInverse_SumsToZero()
        {
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
            Field.SetField(P256);
            Field a = 12345;
            Field one = 1;

            Field result = a / one;
            Assert.Equal(a, result);
        }

        [Fact]
        public void Division_BySelf_ReturnsOne()
        {
            Field.SetField(P256);
            Field a = 12345;
            Field result = a / a;
            Assert.Equal(new Field(1), result);
        }

        [Fact]
        public void Division_MultiplicationInverse_CombinesToOne()
        {
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
            Field.SetField(P256);
            Field a = 100;
            Field zero = 0;
            Assert.Throws<DivideByZeroException>(() => a / zero);
        }

        [Fact]
        public void Division_ZeroByNonZero_ReturnsZero()
        {
            Field.SetField(P256);
            Field zero = 0;
            Field a = 100;

            Field result = zero / a;
            Assert.Equal(0, (BigInteger)result);
        }

        [Fact]
        public void Division_InverseMultiplication_RoundTrip()
        {
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
            Field.SetField(P256);
            Field b = 12345;
            Field result = Field.Pow(b, 0);
            Assert.Equal(new Field(1), result);
        }

        [Fact]
        public void Pow_OneExponent_ReturnsSameValue()
        {
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
            Field.SetField(P256);
            Field b = 0;

            Assert.Throws<ArithmeticException>(() =>
                Field.Pow(b, 0));
        }

        [Fact]
        public void Pow_BaseZero_PositiveExponent_ReturnsZero()
        {
            Field.SetField(P256);
            Field b = 0;

            Field result = Field.Pow(b, 100);
            Assert.Equal(new Field(0), result);
        }

        [Fact]
        public void Pow_MultiplicationOfPowers_EquivalentToAdditionOfExponents()
        {
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

        #region Square Root Tests

        [Fact]
        public void Sqrt_PerfectSquare_ReturnsCorrectRoot()
        {
            Field.SetField(P256);
            Field a = 9;
            Field root = Field.Sqrt(a);
            Assert.Equal(a, root * root);
        }

        [Fact]
        public void Sqrt_Zero_ReturnsZero()
        {
            Field.SetField(P256);
            Field zero = 0;
            Field root = Field.Sqrt(zero);
            Assert.Equal(0, (BigInteger)root);
        }

        [Fact]
        public void Sqrt_One_ReturnsPlusMinusOne()
        {
            Field.SetField(P256);
            Field one = 1;
            Field root = Field.Sqrt(one);
            Assert.True(root == 1 || root == P256 - 1);
        }

        [Fact]
        public void Sqrt_LargeField_QuadraticResidue()
        {
            Field.SetField(P256);
            int k;

            /* test large modular quadratic residues mod p */
            for (k = 1; k <= 10; k++)
            {
                Field a = SecureRandom.Range(1, P256 - 1);
                Field qr = a * a;
                Field aroot = Field.Sqrt(qr);
                Field squareTest = aroot * aroot;
                Assert.True(qr == squareTest);
            }
        }

        #endregion

        #region Implicit Conversion Tests

        [Fact]
        public void ImplicitConversion_FromInt_CreatesField()
        {
            Field.SetField(P256);
            Field f = 42;
            Assert.Equal(42, (BigInteger)f);
            Assert.IsType<Field>(f);
        }

        [Fact]
        public void ImplicitConversion_FromLong_CreatesField()
        {
            Field.SetField(P256);
            Field f = 123456789012345L;
            Assert.Equal(123456789012345L % P256, (BigInteger)f);
        }

        [Fact]
        public void ImplicitConversion_FromBigInteger_CreatesField()
        {
            Field.SetField(P256);
            BigInteger val = BigInteger.Parse("12345678901234567890");
            Field f = val;
            Assert.Equal(val % P256, (BigInteger)f);
        }

        [Fact]
        public void ImplicitConversion_FromUInt_HandlesCorrectly()
        {
            Field.SetField(P256);
            uint val = 12345;
            Field f = val;
            Assert.Equal(val % P256, (BigInteger)f);
        }

        [Fact]
        public void ExplicitConversion_ToBigInteger_ReturnsValue()
        {
            Field.SetField(P256);
            Field f = 12345;
            BigInteger val = (BigInteger)f;
            Assert.Equal(12345, val);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equality_SameValues_ReturnsTrue()
        {
            Field.SetField(P256);
            Field a = 100;
            Field b = 100;

            Assert.True(a == b);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Equality_DifferentValues_ReturnsFalse()
        {
            Field.SetField(P256);
            Field a = 100;
            Field b = 200;

            Assert.False(a == b);
            Assert.False(a.Equals(b));
        }

        [Fact]
        public void Equality_WithNull_ReturnsFalse()
        {
            Field.SetField(P256);
            Field a = 100;
            object obj = null;
            Assert.False(a.Equals(obj));
        }

        [Fact]
        public void Equality_WithDifferentType_ReturnsFalse()
        {
            Field.SetField(P256);
            Field a = 100;
            Assert.False(a.Equals("100"));
        }

        [Fact]
        public void Equality_GetHashCode_IdenticalForEqualValues()
        {
            Field.SetField(P256);
            Field a = 12345;
            Field b = 12345;

            Assert.Equal(a.GetHashCode(),
                b.GetHashCode());
        }

        [Fact]
        public void Inequality_DifferentValues_ReturnsTrue()
        {
            Field.SetField(P256);
            Field a = 100;
            Field b = 200;
            Assert.True(a != b);
        }

        #endregion

        #region String Representation Tests

        [Fact]
        public void ToString_ReturnsCorrectRepresentation()
        {
            Field.SetField(P256);
            Field f = 12345;
            string result = f.ToString();
            Assert.Equal("12345", result);
        }

        [Fact]
        public void ToString_LargeField_ReturnsFullNumber()
        {
            Field.SetField(P256);
            Field f = P256 - 1;
            string result = f.ToString();
            Assert.Equal((P256 - 1).ToString(), result);
        }

        #endregion

        #region Cryptographic Field Properties Tests

        [Fact]
        public void Field_AllZeroOperations_Consistent()
        {
            Field.SetField(P256);
            Field zero = 0;

            Assert.Equal(0, (BigInteger)(zero + zero));
            Assert.Equal(0, (BigInteger)(zero - zero));

            Assert.Equal(0, (BigInteger)(zero * zero));
            Assert.Equal(0, (BigInteger)(-zero));
        }

        [Fact]
        public void Field_MultiplicativeGroup_NonZeroHasInverse()
        {
            Field.SetField(P256);
            Field a = 12345;

            Field inverse = new Field(1) / a;
            Field product = a * inverse;
            Assert.Equal(new Field(1), product);
        }

        [Fact]
        public void Field_Pow_PowerOfP_MinusOne_EqualsOneForNonZero()
        {
            Field.SetField(17);
            Field a = 5;

            Field result = Field.Pow(a, 16);
            Assert.Equal(new Field(1), result);
        }

        [Fact]
        public void Field_ConsistentArithmetic_UnderMultipleModuli()
        {
            Field.SetField(17);

            Field a = 5;
            Field b = 7;

            Field sum = a + b;
            Field prod = a * b;

            Assert.Equal(12, (BigInteger)sum);
            Assert.Equal(1, (BigInteger)prod);
            Field.SetField(23);

            a = 5;
            b = 7;

            sum = a + b;
            prod = a * b;

            Assert.Equal(12, (BigInteger)sum);
            Assert.Equal(12, (BigInteger)prod);
        }

        #endregion
    }
}
