using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
    [Collection("Sequential")]
    public class TwistedEdwardsCurveTests
    {
        #region Constructor Tests
        [Fact]
        public void Constructor_WithValidParameters_CreatesCurve()
        {
            BigInteger a = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819948");
            BigInteger d = BigInteger.Parse("37095705934669439343138083508754565189542113879843219016388785533085940283555");

            BigInteger p = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564819949");
            BigInteger order = BigInteger.Parse("7237005577332262213973186563042994240857116359379907606001950938285454250989");
            BigInteger cofactor = 8;

            var curve = new TwistedEdwardsCurve(a, d, p, order, cofactor);
            Assert.Equal(a, curve.a);

            Assert.Equal(d, curve.d);
            Assert.Equal(p, curve.field);

            Assert.Equal(order, curve.order);
            Assert.Equal(cofactor, curve.cofactor);
        }

        [Fact]
        public void Constructor_WithTooManyParameters_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new TwistedEdwardsCurve(1, 2, 3, 4, 5, 6));
        }

        [Fact]
        public void Constructor_WithSingularCurve_ThrowsInvalidOperationException()
        {
            var p = BigInteger.Parse("23");
            BigInteger a = 0, d = 5;

            BigInteger order = 17;
            BigInteger cofactor = 4;

            /* edge case: a = 0 or d = 0 */
            Assert.Throws<InvalidOperationException>(() =>
                new TwistedEdwardsCurve(a, d, p, order, cofactor));

            a = 4;
            d = 0;

            Assert.Throws<InvalidOperationException>(() =>
                new TwistedEdwardsCurve(a, d, p, order, cofactor));

            /* edge case: a = d */
            a = 4;
            d = 4;

            Assert.Throws<InvalidOperationException>(() =>
                new TwistedEdwardsCurve(a, d, p, order, cofactor));

            /* cofactor not divisible by 4 */
            cofactor = 3;

            Assert.Throws<InvalidOperationException>(() =>
                new TwistedEdwardsCurve(a, d, p, order, cofactor));
        }
        #endregion

        #region GetNamedCurve Tests
        [Fact]
        public void GetNamedCurve_Edwards25519_ReturnsValidCurve()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            Assert.Equal(255, curve.field.GetBits());

            Assert.Equal(253, curve.order.GetBits());
            Assert.Equal(8, curve.cofactor);

            /* verify base point exists */
            var G = curve.GetBasePoint();
            Assert.NotEqual(ECPoint.POINT_INFINITY, G);
        }

        [Fact]
        public void GetNamedCurve_Edwards448_ReturnsValidCurve()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards448);
            Assert.Equal(448, curve.field.GetBits());

            Assert.Equal(446, curve.order.GetBits());
            Assert.Equal(4, curve.cofactor);

            /* verify base point exists */
            var G = curve.GetBasePoint();
            Assert.NotEqual(ECPoint.POINT_INFINITY, G);
        }
        #endregion
    }
}
