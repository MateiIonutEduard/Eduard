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

    }
}
