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

        #region Evaluate Tests
        [Fact]
        public void Evaluate_ForPointOnCurve_ReturnsSquare()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var xSquared = curve.Evaluate(G.GetAffineY());
            BigInteger Gx = G.GetAffineX();
            BigInteger field = curve.field;

            var expectedXSquared = (Gx * Gx) % field;
            Assert.Equal(expectedXSquared, xSquared);
        }

        [Fact]
        public void Evaluate_ForBoundaryValidY_ReturnsResult()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            BigInteger y = curve.field - 1;

            var exception = Record.Exception(() => curve.Evaluate(y));
            Assert.Null(exception);
        }

        [Fact]
        public void Evaluate_ForOutOfRangeY_ThrowsArgumentOutOfRangeException()
        {
            /* edge cases: y < 0 or y >= p */
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            BigInteger y = -1;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                curve.Evaluate(y));

            y = curve.field;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                curve.Evaluate(y));
        }

        [Fact]
        public void Evaluate_ForRandomY_ReturnsConsistentResult()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);

            for (int i = 0; i < 50; i++)
            {
                var Py = SecureRandom.Range(0, curve.field - 1);
                var result1 = curve.Evaluate(Py);
                var result2 = curve.Evaluate(Py);
                Assert.Equal(result1, result2);
            }
        }

        [Fact]
        public void Evaluate_MatchesTwistedEdwardsEquation()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            BigInteger p = curve.field;

            for (int i = 0; i < 50; i++)
            {
                var Py = SecureRandom.Range(0, curve.field - 1);
                var xSquared = curve.Evaluate(Py);

                /* compute it manually */
                BigInteger A1 = (Py * Py) % p;
                BigInteger A2 = (curve.d * A1) % p;
                BigInteger A3 = (p + 1 - A1) % p;
                BigInteger A4 = (p + curve.a - A2) % p;

                BigInteger A4i = A4.Inverse(p);
                BigInteger X2 = (A3 *  A4i) % p;
                Assert.Equal(X2, xSquared);
            }
        }
        #endregion
    }
}
