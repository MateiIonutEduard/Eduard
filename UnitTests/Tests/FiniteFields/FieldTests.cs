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
    }
}
