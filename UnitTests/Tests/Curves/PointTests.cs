using Eduard.Security.Primitives;
using System;
using System.Collections.Generic;

namespace Eduard.Tests.Curves
{
    public class PointTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidCoordinates_CreatesAffinePoint()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);

            Assert.True(!point.IsInfinity);
            Assert.Equal(x, point.GetAffineX());
            Assert.Equal(y, point.GetAffineY());
        }

        [Fact]
        public void Constructor_WithValidCoordinates_IsNotInfinity()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);

            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void Constructor_WithIsOnCurveFalse_CreatesInfinityPoint()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y, false);

            Assert.False(!point.IsInfinity);
            Assert.True(point.IsInfinity);
        }

        [Fact]
        public void Constructor_WithIsOnCurveTrue_CreatesFinitePoint()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y, true);

            Assert.True(!point.IsInfinity);
            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void Constructor_WithZeroCoordinates_CreatesValidPoint()
        {
            BigInteger x = 0;
            BigInteger y = 0;

            var point = new ECPoint(x, y);

            Assert.True(!point.IsInfinity);
            Assert.Equal(x, point.GetAffineX());
            Assert.Equal(y, point.GetAffineY());
        }

        [Fact]
        public void Constructor_WithNegativeCoordinates_CreatesValidPoint()
        {
            BigInteger x = -1;
            BigInteger y = -1;

            var point = new ECPoint(x, y);

            Assert.True(!point.IsInfinity);
            Assert.Equal(x, point.GetAffineX());
            Assert.Equal(y, point.GetAffineY());
        }

        [Fact]
        public void Constructor_WithNullX_ThrowsArgumentNullException()
        {
            BigInteger y = 10;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint(null, y));
        }

        [Fact]
        public void Constructor_WithNullY_ThrowsArgumentNullException()
        {
            BigInteger x = 5;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint(x, null));
        }

        [Fact]
        public void Constructor_WithNullXAndNullY_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint(null, null));
        }

        [Fact]
        public void Constructor_WithNullXAndIsOnCurveFalse_ThrowsArgumentNullException()
        {
            BigInteger y = 10;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint(null, y, false));
        }

        [Fact]
        public void Constructor_WithNullYAndIsOnCurveFalse_ThrowsArgumentNullException()
        {
            BigInteger x = 5;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint(x, null, false));
        }

        [Fact]
        public void Constructor_WithLargeCoordinates_CreatesValidPoint()
        {
            var x = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671663");
            var y = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");

            var point = new ECPoint(x, y);

            Assert.True(!point.IsInfinity);
            Assert.Equal(x, point.GetAffineX());
            Assert.Equal(y, point.GetAffineY());
        }

        #endregion

        #region Identity Element Tests

        [Fact]
        public void PointInfinity_ReturnsIdentityElement()
        {
            var infinity = ECPoint.POINT_INFINITY;
            Assert.True(infinity.IsInfinity);
        }

        [Fact]
        public void PointInfinity_NormalizedCoordinates_ReturnsZeroAndOne()
        {
            var infinity = ECPoint.POINT_INFINITY;
            Assert.Equal(0, infinity.GetAffineX());
            Assert.Equal(1, infinity.GetAffineY());
        }

        [Fact]
        public void PointInfinity_MultipleCalls_ReturnEqualInstances()
        {
            var infinity1 = ECPoint.POINT_INFINITY;
            var infinity2 = ECPoint.POINT_INFINITY;

            Assert.Equal(infinity1, infinity2);
            Assert.True(infinity1 == infinity2);
        }

        [Fact]
        public void PointInfinity_InternalCoordinatesNormalized_RegardlessOfInput()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            var notOnCurve = new ECPoint(x, y, false);
            Assert.Equal(0, notOnCurve.GetAffineX());
            Assert.Equal(1, notOnCurve.GetAffineY());
        }

        [Fact]
        public void DefaultStructValue_BehavesAsIdentityElement()
        {
            var defaultPoint = default(ECPoint);
            Assert.True(defaultPoint.IsInfinity);
            Assert.Equal(0, defaultPoint.GetAffineX());
            Assert.Equal(1, defaultPoint.GetAffineY());
        }

        #endregion

        #region Coordinate Accessor Tests

        [Fact]
        public void GetAffineX_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            Assert.Equal(x, point.GetAffineX());
        }

        [Fact]
        public void GetAffineX_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint.POINT_INFINITY;
            Assert.Equal(0, infinity.GetAffineX());
        }

        [Fact]
        public void GetAffineX_OffCurvePoint_ReturnsZero()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            var point = new ECPoint(x, y, false);
            Assert.Equal(0, point.GetAffineX());
        }

        [Fact]
        public void GetAffineX_WithZeroCoordinate_ReturnsZero()
        {
            BigInteger x = 0;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            Assert.Equal(0, point.GetAffineX());
        }

        [Fact]
        public void GetAffineX_WithNegativeCoordinate_ReturnsNegative()
        {
            BigInteger x = -1;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            Assert.Equal(x, point.GetAffineX());
        }

        [Fact]
        public void GetAffineX_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;

            var point = new ECPoint(x, y);
            var result1 = point.GetAffineX();

            var result2 = point.GetAffineX();
            var result3 = point.GetAffineX();

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void GetAffineY_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            Assert.Equal(y, point.GetAffineY());
        }

        [Fact]
        public void GetAffineY_IdentityElement_ReturnsOne()
        {
            var infinity = ECPoint.POINT_INFINITY;
            Assert.Equal(1, infinity.GetAffineY());
        }

        [Fact]
        public void GetAffineY_OffCurvePoint_ReturnsOne()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            var point = new ECPoint(x, y, false);
            Assert.Equal(1, point.GetAffineY());
        }

        [Fact]
        public void GetAffineY_WithZeroCoordinate_ReturnsZero()
        {
            BigInteger x = 5;
            BigInteger y = 0;

            var point = new ECPoint(x, y);
            Assert.Equal(0, point.GetAffineY());
        }

        [Fact]
        public void GetAffineY_WithNegativeCoordinate_ReturnsNegative()
        {
            BigInteger x = 5;
            BigInteger y = -1;

            var point = new ECPoint(x, y);
            Assert.Equal(y, point.GetAffineY());
        }

        [Fact]
        public void GetAffineY_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;

            var point = new ECPoint(x, y);
            var result1 = point.GetAffineY();

            var result2 = point.GetAffineY();
            var result3 = point.GetAffineY();

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        #endregion

        #region Point Validity Tests

        [Fact]
        public void IsInfinity_FinitePoint_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void IsInfinity_IdentityElement_ReturnsTrue()
        {
            var point = new ECPoint(5, 10, false);
            Assert.True(point.IsInfinity);
        }

        [Fact]
        public void IsInfinity_PointAtInfinityConstant_ReturnsTrue()
        {
            Assert.True(ECPoint.POINT_INFINITY.IsInfinity);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_SameCoordinates_ReturnsTrue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point1 = new ECPoint(x, y);
            var point2 = new ECPoint(x, y);

            Assert.True(point1.Equals(point2));
            Assert.True(point1 == point2);
        }

        [Fact]
        public void Equals_SameInstance_ReturnsTrue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            Assert.True(point.Equals(point));
            Assert.True(point == point);
        }

        [Fact]
        public void Equals_TwoIdentityElements_ReturnsTrue()
        {
            var inf1 = ECPoint.POINT_INFINITY;
            var inf2 = ECPoint.POINT_INFINITY;

            Assert.True(inf1.Equals(inf2));
            Assert.True(inf1 == inf2);
        }

        [Fact]
        public void Equals_TwoOffCurvePoints_ReturnsTrue()
        {
            BigInteger x1 = 5, y1 = 10;
            BigInteger x2 = 42, y2 = 100;

            var off1 = new ECPoint(x1, y1, false);
            var off2 = new ECPoint(x2, y2, false);

            Assert.True(off1.Equals(off2));
            Assert.True(off1 == off2);
        }

        [Fact]
        public void Equals_ZeroCoordinatePoints_ReturnsTrue()
        {
            BigInteger x = 0;
            BigInteger y = 0;

            var point1 = new ECPoint(x, y);
            var point2 = new ECPoint(x, y);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void Equals_NegativeCoordinatePoints_ReturnsTrue()
        {
            BigInteger x = -1;
            BigInteger y = -1;

            var point1 = new ECPoint(x, y);
            var point2 = new ECPoint(x, y);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void Equals_LargeCoordinatePoints_ReturnsTrue()
        {
            var x = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671663");
            var y = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");

            var point1 = new ECPoint(x, y);
            var point2 = new ECPoint(x, y);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void Equals_DifferentXCoordinate_ReturnsFalse()
        {
            BigInteger x1 = 5, x2 = 15;
            BigInteger y = 10;

            var point1 = new ECPoint(x1, y);
            var point2 = new ECPoint(x2, y);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void Equals_DifferentYCoordinate_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y1 = 10, y2 = 20;

            var point1 = new ECPoint(x, y1);
            var point2 = new ECPoint(x, y2);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void Equals_CompletelyDifferentCoordinates_ReturnsFalse()
        {
            BigInteger x1 = 5, y1 = 10;
            BigInteger x2 = 15, y2 = 20;

            var point1 = new ECPoint(x1, y1);
            var point2 = new ECPoint(x2, y2);

            Assert.False(point1.Equals(point2));
        }

        [Fact]
        public void Equals_FinitePointVsIdentityElement_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            var infinity = ECPoint.POINT_INFINITY;

            Assert.False(point.Equals(infinity));
            Assert.True(point != infinity);
        }

        [Fact]
        public void Equals_FinitePointVsOffCurvePoint_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var onCurve = new ECPoint(x, y);
            var offCurve = new ECPoint(x, y, false);

            Assert.False(onCurve.Equals(offCurve));
            Assert.True(onCurve != offCurve);
        }

        [Fact]
        public void Equals_NullObject_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            Assert.False(point.Equals(null));
        }

        [Fact]
        public void Equals_DifferentType_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            Assert.False(point.Equals("not a point"));

            Assert.False(point.Equals(42));
            Assert.False(point.Equals(new object()));
        }

        [Fact]
        public void Equals_SwappedCoordinates_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point1 = new ECPoint(x, y);
            var point2 = new ECPoint(y, x);

            Assert.False(point1.Equals(point2));
        }

        [Fact]
        public void Equals_PointWithInfinityCoordinates_NotEqualToInfinity()
        {
            BigInteger x = 0;
            BigInteger y = 1;

            var point = new ECPoint(x, y);
            var infinity = ECPoint.POINT_INFINITY;

            Assert.False(point.Equals(infinity));
            Assert.True(point != infinity);
        }

        [Fact]
        public void EqualityOperator_Transitivity()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var p1 = new ECPoint(x, y);
            var p2 = new ECPoint(x, y);
            var p3 = new ECPoint(x, y);

            Assert.True(p1 == p2);
            Assert.True(p2 == p3);
            Assert.True(p1 == p3);
        }

        [Fact]
        public void InequalityOperator_OppositeOfEqualityOperator()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var p1 = new ECPoint(x, y);
            var p2 = new ECPoint(x, y);
            var p3 = new ECPoint(x + 1, y);

            Assert.True((p1 == p2) == !(p1 != p2));
            Assert.True((p1 == p3) == !(p1 != p3));
        }

        #endregion

        #region GetHashCode Tests

        [Fact]
        public void GetHashCode_SamePoints_HaveSameHashCode()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point1 = new ECPoint(x, y);
            var point2 = new ECPoint(x, y);

            Assert.Equal(point1.GetHashCode(), 
                point2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint.POINT_INFINITY;
            Assert.Equal(0, infinity.GetHashCode());
        }

        [Fact]
        public void GetHashCode_OffCurvePoints_ReturnZero()
        {
            BigInteger x1 = 5, y1 = 10;
            BigInteger x2 = 42, y2 = 100;

            var off1 = new ECPoint(x1, y1, false);
            var off2 = new ECPoint(x2, y2, false);

            Assert.Equal(0, off1.GetHashCode());
            Assert.Equal(off1.GetHashCode(), 
                off2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_ConsistentAcrossMultipleCalls()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            int hash1 = point.GetHashCode();

            int hash2 = point.GetHashCode();
            int hash3 = point.GetHashCode();

            Assert.Equal(hash1, hash2);
            Assert.Equal(hash2, hash3);
        }

        [Fact]
        public void GetHashCode_DifferentPoints_MayDiffer()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point1 = new ECPoint(x, y);
            var point2 = new ECPoint(x + 1, y);

            int hash1 = point1.GetHashCode();
            int hash2 = point2.GetHashCode();
            Assert.True(hash1 != 0 || hash2 != 0);
        }

        [Fact]
        public void GetHashCode_VariousCoordinateValues_Consistent()
        {
            var values = new[] { 0, 1, -1, 42, 256 };

            foreach (var xVal in values)
            {
                foreach (var yVal in values)
                {
                    BigInteger x = xVal;
                    BigInteger y = yVal;

                    var point1 = new ECPoint(x, y);
                    var point2 = new ECPoint(x, y);

                    Assert.Equal(point1.GetHashCode(), 
                        point2.GetHashCode());
                }
            }
        }

        #endregion

        #region Collection Integration Tests

        [Fact]
        public void HashSet_HandlesFinitePointsCorrectly()
        {
            BigInteger x1 = 5, y1 = 10;
            BigInteger x2 = 15, y2 = 20;

            var set = new HashSet<ECPoint>();
            var p1 = new ECPoint(x1, y1);

            var p2 = new ECPoint(x1, y1);
            var p3 = new ECPoint(x2, y2);

            set.Add(p1);
            Assert.Single(set);

            Assert.True(set.Contains(p2));
            Assert.False(set.Contains(p3));
        }

        [Fact]
        public void HashSet_IdentityElementsHandledCorrectly()
        {
            var set = new HashSet<ECPoint>();
            var inf1 = ECPoint.POINT_INFINITY;
            BigInteger x = 42;
            BigInteger y = 100;

            var inf2 = new ECPoint(x, y, false);
            set.Add(inf1);

            Assert.Single(set);
            Assert.True(set.Contains(inf2));
        }

        [Fact]
        public void Dictionary_HandlesPointsAsKeys()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var dict = new Dictionary<ECPoint, string>();
            var p1 = new ECPoint(x, y);
            var p2 = new ECPoint(x, y);

            dict[p1] = "original";
            Assert.Equal("original", dict[p2]);
        }

        [Fact]
        public void List_ContainsUsesEquality()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var list = new List<ECPoint>();
            var p1 = new ECPoint(x, y);
            var p2 = new ECPoint(x, y);

            list.Add(p1);
            Assert.True(list.Contains(p2));
        }

        [Fact]
        public void List_IdentityElement_ContainsWorks()
        {
            var list = new List<ECPoint>();
            var inf1 = ECPoint.POINT_INFINITY;
            var inf2 = ECPoint.POINT_INFINITY;

            list.Add(inf1);
            Assert.True(list.Contains(inf2));
        }

        #endregion

        #region Value Type Semantics Tests

        [Fact]
        public void ECPoint_IsValueType()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var point = new ECPoint(x, y);
            Assert.True(point.GetType().IsValueType);
        }

        [Fact]
        public void ValueSemantics_AssignmentCreatesCopy()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var original = new ECPoint(x, y);
            var copy = original;

            Assert.Equal(original, copy);
            Assert.True(original == copy);
        }

        #endregion

        #region Real-World ECC Tests

        [Fact]
        public void Points_CanRepresentSecp256k1Generator()
        {
            /* secp256k1 generator point coordinates */
            var gx = BigInteger.Parse("55066263022277343669578718895168534326250603453777594175500187360389116729240");
            var gy = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");

            var generator = new ECPoint(gx, gy);
            Assert.False(generator.IsInfinity);

            Assert.Equal(gx, generator.GetAffineX());
            Assert.Equal(gy, generator.GetAffineY());
        }

        [Fact]
        public void Points_WithLargeCoordinates_EqualityWorks()
        {
            var x = BigInteger.Parse("1234567890123456789012345678901234567890");
            var y = BigInteger.Parse("9876543210987654321098765432109876543210");

            var point1 = new ECPoint(x, y);
            var point2 = new ECPoint(x, y);

            Assert.True(point1.Equals(point2));
            Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
        }

        #endregion

        #region Stress Tests

        [Fact]
        public void ManyPointsCreation_NoExceptions()
        {
            for (int i = 0; i < 1000; i++)
            {
                BigInteger x = i;
                BigInteger y = i * 2;

                var point = new ECPoint(x, y);
                Assert.Equal(x, point.GetAffineX());
                Assert.Equal(y, point.GetAffineY());
            }
        }

        [Fact]
        public void HashCodeDistribution_MultiplePoints()
        {
            var hashCodes = new HashSet<int>();

            for (int i = 0; i < 100; i++)
            {
                BigInteger x = i;
                BigInteger y = i * 2;

                var point = new ECPoint(x, y);
                hashCodes.Add(point.GetHashCode());
            }

            Assert.True(hashCodes.Count >= 95);
        }

        #endregion
    }
}
