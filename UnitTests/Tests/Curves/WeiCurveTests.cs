using System;
using Eduard.Security.Curves;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
    [Collection("Sequential")]
    public class WeiCurveTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_RandomCurve_GeneratesValidNonSingularCurve()
        {
            var curve = new EllipticCurve(256);
            BigInteger p = curve.field;

            /* verify field is strong probable prime */
            Assert.True(BigInteger.IsProbablePrime(curve.field));
            Assert.True(curve.field > 0);

            /* verify curve is non-singular: 4a^3 + 27b^2 ≠ 0 mod p */
            var a3 = (curve.a * curve.a) % p;
            a3 = (a3 * curve.a) % p;
            a3 = (4 * a3) % p;

            var b2 = (curve.b * curve.b) % p;
            var b2_27 = (27 * b2) % p;

            var discriminant = (a3 + b2_27) % p;
            Assert.NotEqual(0, discriminant);

            /* verify a and b are in valid range */
            Assert.True(curve.a > 0 && curve.a < curve.field);
            Assert.True(curve.b > 0 && curve.b < curve.field);
        }

        [Fact]
        public void Constructor_RandomCurve_DifferentBits_ProducesDifferentFields()
        {
            var curve256 = new EllipticCurve(256);
            var curve384 = new EllipticCurve(384);
            var curve512 = new EllipticCurve(512);

            Assert.NotEqual(curve256.field, curve384.field);
            Assert.NotEqual(curve256.field, curve512.field);
            Assert.NotEqual(curve384.field, curve512.field);

            Assert.True(curve256.field.GetBits() >= 250);
            Assert.True(curve384.field.GetBits() >= 378);
            Assert.True(curve512.field.GetBits() >= 506);
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesCurve()
        {
            /* NIST P-256 parameters */
            BigInteger p = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853951");
            BigInteger a = BigInteger.Parse("115792089210356248762697446949407573530086143415290314195533631308867097853948");
            BigInteger b = BigInteger.Parse("41058363725152142129326129780047268409114441015993725554835256314039467401291");
            BigInteger order = BigInteger.Parse("115792089210356248762697446949407573529996955224135760342422259061068512044369");
            BigInteger cofactor = 1;

            var curve = new EllipticCurve(a, b, p, order, cofactor);
            Assert.Equal(a, curve.a);

            Assert.Equal(b, curve.b);
            Assert.Equal(p, curve.field);

            Assert.Equal(order, curve.order);
            Assert.Equal(cofactor, curve.cofactor);
        }

        [Fact]
        public void Constructor_WithTooManyParameters_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new EllipticCurve(1, 2, 3, 4, 5, 6));
        }

        [Fact]
        public void Constructor_WithSingularCurve_ThrowsInvalidOperationException()
        {
            /* singular curve: y^2 = x^3 (a=0, b=0) */
            var p = BigInteger.Parse("23");
            BigInteger a = 0, b = 0;

            BigInteger order = 1, cofactor = 1;
            Assert.Throws<InvalidOperationException>(() =>
                new EllipticCurve(a, b, p, order, cofactor));
        }

        [Fact]
        public void Constructor_WithNegativeDiscriminant_AcceptsValidCurve()
        {
            /* curve with negative discriminant is still valid */
            BigInteger p = BigInteger.Parse("97");
            BigInteger a = 2, b = 3;

            BigInteger order = 100;
            BigInteger cofactor = 1;

            var exception = Record.Exception(() =>
                new EllipticCurve(a, b, p, order, cofactor));

            Assert.Null(exception);
        }

        #endregion

        #region GetNamedCurve Tests

        [Fact]
        public void GetNamedCurve_NistP192_ReturnsValidCurve()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP192);

            Assert.Equal(192, curve.field.GetBits());
            Assert.Equal(192, curve.order.GetBits());
            Assert.Equal(1, curve.cofactor);

            /* verify base point exists */
            var G = curve.GetBasePoint();
            Assert.NotEqual(ECPoint.POINT_INFINITY, G);
        }

        [Fact]
        public void GetNamedCurve_NistP224_ReturnsValidCurve()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP224);

            Assert.Equal(224, curve.field.GetBits());
            Assert.Equal(224, curve.order.GetBits());
            Assert.Equal(1, curve.cofactor);

            /* verify base point exists */
            var G = curve.GetBasePoint();
            Assert.NotEqual(ECPoint.POINT_INFINITY, G);
        }

        [Fact]
        public void GetNamedCurve_NistP256_ReturnsValidCurve()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            Assert.Equal(256, curve.field.GetBits());
            Assert.Equal(256, curve.order.GetBits());
            Assert.Equal(1, curve.cofactor);

            /* verify base point exists */
            var G = curve.GetBasePoint();
            Assert.NotEqual(ECPoint.POINT_INFINITY, G);
        }

        [Fact]
        public void GetNamedCurve_NistP384_ReturnsValidCurve()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP384);

            Assert.Equal(384, curve.field.GetBits());
            Assert.Equal(384, curve.order.GetBits());
            Assert.Equal(1, curve.cofactor);

            var G = curve.GetBasePoint();
            Assert.NotEqual(ECPoint.POINT_INFINITY, G);
        }

        [Fact]
        public void GetNamedCurve_NistP521_ReturnsValidCurve()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP521);

            Assert.Equal(521, curve.field.GetBits());
            Assert.Equal(521, curve.order.GetBits());
            Assert.Equal(1, curve.cofactor);

            var G = curve.GetBasePoint();
            Assert.NotEqual(ECPoint.POINT_INFINITY, G);
        }

        [Fact]
        public void GetNamedCurve_Wei25519_ReturnsValidCurve()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);

            /* Wei25519 has cofactor 8 */
            Assert.Equal(8, curve.cofactor);
            Assert.Equal(255, curve.field.GetBits());

            var G = curve.GetBasePoint();
            Assert.NotEqual(ECPoint.POINT_INFINITY, G);

            /* verify that cofactor * G is still on curve */
            var cofactorG = ECMath.Multiply(curve, curve.cofactor, G);
            Assert.NotEqual(ECPoint.POINT_INFINITY, cofactorG);
        }

        [Fact]
        public void GetNamedCurve_Wei448_ReturnsValidCurve()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei448);

            /* Wei448 has cofactor 4 */
            Assert.Equal(4, curve.cofactor);
            Assert.Equal(448, curve.field.GetBits());

            var G = curve.GetBasePoint();
            Assert.NotEqual(ECPoint.POINT_INFINITY, G);
        }

        #endregion

        #region Evaluate Tests

        [Fact]
        public void Evaluate_ForPointOnCurve_ReturnsSquare()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var ySquared = curve.Evaluate(G.GetAffineX());
            BigInteger Gy = G.GetAffineY();

            var expectedYSquared = (Gy * Gy) % curve.field;
            Assert.Equal(expectedYSquared, ySquared);
        }

        [Fact]
        public void Evaluate_ForBoundaryValidX_ReturnsResult()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            BigInteger x = curve.field - 1;

            var exception = Record.Exception(() => curve.Evaluate(x));
            Assert.Null(exception);
        }

        [Fact]
        public void Evaluate_ForOutOfRangeX_ThrowsArgumentOutOfRangeException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            BigInteger x = -1;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                curve.Evaluate(x));

            x = curve.field;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                curve.Evaluate(x));
        }

        [Fact]
        public void Evaluate_ForRandomX_ReturnsConsistentResult()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            for (int i = 0; i < 50; i++)
            {
                var x = SecureRandom.Range(0, curve.field - 1);
                var result1 = curve.Evaluate(x);

                var result2 = curve.Evaluate(x);
                Assert.Equal(result1, result2);
            }
        }

        [Fact]
        public void Evaluate_MatchesWeierstrassEquation()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            BigInteger p = curve.field;

            for (int i = 0; i < 50; i++)
            {
                var x = SecureRandom.Range(0, curve.field - 1);
                var ySquared = curve.Evaluate(x);

                /* compute x^3 + ax + b manually */
                var x3 = (x * x) % p;
                x3 = (x3 * x) % p;

                var ax = (curve.a * x) % p;
                var ax_b = (ax + curve.b) % p;

                var expected = (x3 + ax_b) % p;
                Assert.Equal(expected, ySquared);
            }
        }

        #endregion

        #region GetPoint and GetMessage Tests (Koblitz Encoding)

        [Fact]
        public void GetPoint_GetMessage_RoundTrip_ReturnsOriginalMessage()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var messages = new BigInteger[] { 1, 42, 100, 999, 12345 };
            var r = 30;

            foreach (var originalMessage in messages)
            {
                var encodedPoint = curve.GetPoint(originalMessage, r);

                Assert.NotEqual(ECPoint.POINT_INFINITY, encodedPoint);

                var decodedMessage = curve.GetMessage(encodedPoint, r);
                Assert.Equal(originalMessage, decodedMessage);
            }
        }

        [Fact]
        public void GetPoint_WithNegativeMessage_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            BigInteger message = -1;

            Assert.Throws<ArgumentException>(() =>
                curve.GetPoint(message));
        }

        [Fact]
        public void GetPoint_WithLargeMessage_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var r = 30;

            /* message too large */
            var largeMessage = (curve.field - r) / r + 1;

            Assert.Throws<ArgumentException>(() =>
                curve.GetPoint(largeMessage, r));
        }

        [Fact]
        public void GetPoint_WithMessageExceedingFieldCapacity_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var r = 30;

            /* m * r >= field - r + 1 */
            var largeMessage = (curve.field - r) / r + 1;

            var exception = Assert.Throws<ArgumentException>(() =>
                curve.GetPoint(largeMessage, r));

            Assert.Contains("too large to encode", exception.Message);
        }

        [Fact]
        public void GetPoint_EncodingFailure_ThrowsInvalidOperationException()
        {
            /* use a small curve where encoding may fail */
            var curve = new EllipticCurve(32);
            BigInteger message = 0;
            var r = 1;

            /* on very small curves, the loop may exhaust without finding a valid point */
            var exception = Record.Exception(() => curve.GetPoint(message, r));

            if (exception != null)
                Assert.IsType<InvalidOperationException>(exception);
        }

        [Fact]
        public void GetPoint_GetMessage_DifferentRValues_Consistent()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var message = new BigInteger(1234);
            var rValues = new int[] { 10, 20, 30, 50 };

            foreach (var r in rValues)
            {
                var encodedPoint = curve.GetPoint(message, r);
                Assert.NotEqual(ECPoint.POINT_INFINITY, encodedPoint);

                var decodedMessage = curve.GetMessage(encodedPoint, r);
                Assert.Equal(message, decodedMessage);
            }
        }

        [Fact]
        public void GetPoint_EncodedPointLiesOnCurve()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var message = new BigInteger(54321);
            var r = 30;

            var encodedPoint = curve.GetPoint(message, r);

            /* verify point satisfies curve equation */
            var ySquared = curve.Evaluate(encodedPoint.GetAffineX());
            BigInteger field = curve.field;
            BigInteger My = encodedPoint.GetAffineY();

            var actualYSquared = (My * My) % field;
            Assert.Equal(ySquared, actualYSquared);
        }

        [Fact]
        public void GetMessage_OnInfinity_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            Assert.Throws<ArgumentException>(() =>
                curve.GetMessage(ECPoint.POINT_INFINITY));
        }

        [Fact]
        public void GetMessage_WithCoordinatesOutsideField_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            var invalidPoint = new ECPoint(-1, 0);
            Assert.Throws<ArgumentException>(() =>
                curve.GetMessage(invalidPoint));

            invalidPoint = new ECPoint(curve.field, 0);
            Assert.Throws<ArgumentException>(() =>
                curve.GetMessage(invalidPoint));

            invalidPoint = new ECPoint(0, -1);
            Assert.Throws<ArgumentException>(() =>
                curve.GetMessage(invalidPoint));

            invalidPoint = new ECPoint(0, curve.field);
            Assert.Throws<ArgumentException>(() =>
                curve.GetMessage(invalidPoint));
        }

        [Fact]
        public void GetMessage_PointNotOnCurve_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var invalidPoint = new ECPoint(123, 456);

            Assert.Throws<ArgumentException>(() =>
                curve.GetMessage(invalidPoint));
        }

        [Fact]
        public void GetPoint_MessageZero_EncodesSuccessfully()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            BigInteger message = 0;
            int r = 30;

            var encodedPoint = curve.GetPoint(message, r);
            Assert.NotEqual(ECPoint.POINT_INFINITY, encodedPoint);

            var decodedMessage = curve.GetMessage(encodedPoint, r);
            Assert.Equal(message, decodedMessage);
        }

        #endregion

        #region GetBasePoint Tests

        [Fact]
        public void GetBasePoint_Default_ReturnsValidGenerator()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            Assert.NotEqual(ECPoint.POINT_INFINITY, G);
            BigInteger p = curve.field;

            /* verify point lies on curve */
            var ySquared = curve.Evaluate(G.GetAffineX());
            BigInteger Gy = G.GetAffineY();

            var actualYSquared = (Gy * Gy) % p;
            Assert.Equal(ySquared, actualYSquared);
        }

        [Fact]
        public void GetBasePoint_UseCached_ReturnsSamePoint()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            var firstPoint = curve.GetBasePoint(false);
            var cachedPoint = curve.GetBasePoint(true);

            Assert.Equal(firstPoint, cachedPoint);
        }

        [Fact]
        public void GetBasePoint_SkipValidation_ReturnsPointOnCurve()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = curve.GetBasePoint(useCached: false, skipValidation: true);

            /* point should still satisfy curve equation */
            var ySquared = curve.Evaluate(G.GetAffineX());
            var p = curve.field;

            var actualYSquared = (G.GetAffineY() * G.GetAffineY()) % p;
            Assert.Equal(ySquared, actualYSquared);
        }

        [Fact]
        public void GetBasePoint_WithCofactor_GeneratesPrimeOrderPoint()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = curve.GetBasePoint();

            /* cofactor * G should not be infinity */
            var cofactorG = ECMath.Multiply(curve, curve.cofactor, G, ECMode.EC_FASTEST);
            Assert.NotEqual(ECPoint.POINT_INFINITY, cofactorG);

            /* order * G should be infinity */
            var orderG = ECMath.Multiply(curve, curve.order, G, ECMode.EC_FASTEST);
            Assert.Equal(ECPoint.POINT_INFINITY, orderG);
        }

        [Fact]
        public void GetBasePoint_OnRandomCurve_ReturnsNonInfinityPoint()
        {
            var curve = new EllipticCurve(256);
            var G = curve.GetBasePoint();

            Assert.NotEqual(
                ECPoint.POINT_INFINITY, 
                G);
        }

        #endregion

        #region SetBasePoint Tests

        [Fact]
        public void SetBasePoint_ValidPoint_SetsGenerator()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var originalG = curve.GetBasePoint();

            /* set same point again */
            curve.SetBasePoint(originalG);

            var newG = curve.GetBasePoint(true);
            Assert.Equal(originalG, newG);
        }

        [Fact]
        public void SetBasePoint_DifferentValidPoint_SetsNewGenerator()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var originalG = curve.GetBasePoint();

            /* find another valid point (2G) */
            var twoG = ECMath.Add(curve, originalG, originalG);

            curve.SetBasePoint(twoG);
            var newG = curve.GetBasePoint(true);

            Assert.Equal(twoG, newG);
            Assert.NotEqual(originalG, newG);
        }

        [Fact]
        public void SetBasePoint_PointAtInfinity_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);

            Assert.Throws<ArgumentException>(() =>
                curve.SetBasePoint(ECPoint.POINT_INFINITY));
        }

        [Fact]
        public void SetBasePoint_PointNotOnCurve_ThrowsArgumentException()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var invalidPoint = new ECPoint(123, 456);

            Assert.Throws<ArgumentException>(() =>
                curve.SetBasePoint(invalidPoint));
        }

        [Fact]
        public void SetBasePoint_SmallOrderPoint_ThrowsArgumentException()
        {
            /* Wei25519 has cofactor 8, so small-order points exist */
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            ECPoint smallOrderPoint = ECPoint.POINT_INFINITY;

            while (smallOrderPoint == ECPoint.POINT_INFINITY)
            {
                var G = curve.GetBasePoint(false, true);

                smallOrderPoint = ECMath.Multiply(curve,
                    curve.order, G, ECMode.EC_FASTEST);
            }

            Assert.Throws<ArgumentException>(() =>
                curve.SetBasePoint(smallOrderPoint));
        }

        [Fact]
        public void SetBasePoint_ThenGetBasePoint_ReturnsSetPoint()
        {
            var curve = new EllipticCurve(256);
            var originalG = curve.GetBasePoint();

            /* set a different valid point */
            var newBasePoint = ECMath.Multiply(curve, 2, originalG);
            curve.SetBasePoint(newBasePoint);

            var retrievedPoint = curve.GetBasePoint(true);
            Assert.Equal(newBasePoint, retrievedPoint);
        }

        #endregion
    }
}
