using System;
using Eduard.Security.Primitives;
using System.Collections.Generic;
using Eduard;

namespace Eduard.Tests.Points
{
    public class JacobianPointTests
    {
        #region Constructor Tests

        [Fact]
        public void ECPoint3w_Constructor_WithValidCoordinates_CreatesJacobianPoint()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.True(!point.IsInfinity);
            Assert.Equal(x, point.X);

            Assert.Equal(y, point.Y);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint3w_Constructor_WithNonUnitZ_CreatesValidPoint()
        {
            BigInteger x = 6;
            BigInteger y = 12;
            BigInteger z = 2;

            var point = new ECPoint3w(x, y, z);
            Assert.True(!point.IsInfinity);
            Assert.Equal(x, point.X);

            Assert.Equal(y, point.Y);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint3w_Constructor_WithZZero_CreatesInfinityPoint()
        {
            BigInteger x = 1;
            BigInteger y = 1;
            BigInteger z = 0;

            var point = new ECPoint3w(x, y, z);
            Assert.True(point.IsInfinity);
        }

        [Fact]
        public void ECPoint3w_Constructor_WithZeroCoordinates_CreatesValidPoint()
        {
            BigInteger x = 0;
            BigInteger y = 0;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.True(!point.IsInfinity);
            Assert.Equal(x, point.X);

            Assert.Equal(y, point.Y);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint3w_Constructor_WithNegativeCoordinates_CreatesValidPoint()
        {
            BigInteger x = -1;
            BigInteger y = -1;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.True(!point.IsInfinity);
            Assert.Equal(x, point.X);

            Assert.Equal(y, point.Y);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint3w_Constructor_WithNullX_ThrowsArgumentNullException()
        {
            BigInteger y = 10;
            BigInteger z = 1;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint3w(null, y, z));
        }

        [Fact]
        public void ECPoint3w_Constructor_WithNullY_ThrowsArgumentNullException()
        {
            BigInteger x = 5;
            BigInteger z = 1;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint3w(x, null, z));
        }

        [Fact]
        public void ECPoint3w_Constructor_WithNullZ_ThrowsArgumentNullException()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint3w(x, y, null));
        }

        [Fact]
        public void ECPoint3w_Constructor_WithAllNullCoordinates_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint3w(null, null, null));
        }

        [Fact]
        public void ECPoint3w_Constructor_WithLargeCoordinates_CreatesValidPoint()
        {
            var x = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671663");
            var y = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.True(!point.IsInfinity);
            Assert.Equal(x, point.X);

            Assert.Equal(y, point.Y);
            Assert.Equal(z, point.Z);
        }

        #endregion

        #region Identity Element Tests

        [Fact]
        public void ECPoint3w_PointInfinity_ReturnsIdentityElement()
        {
            var infinity = ECPoint3w.POINT_INFINITY;
            Assert.True(infinity.IsInfinity);
        }

        [Fact]
        public void ECPoint3w_PointInfinity_HasZEqualToZero()
        {
            var infinity = ECPoint3w.POINT_INFINITY;
            Assert.Equal(0, infinity.Z);
        }

        [Fact]
        public void ECPoint3w_PointInfinity_NormalizedCoordinates_ReturnsOneOneZero()
        {
            var infinity = ECPoint3w.POINT_INFINITY;
            Assert.Equal(1, infinity.X);
            Assert.Equal(1, infinity.Y);
            Assert.Equal(0, infinity.Z);
        }

        [Fact]
        public void ECPoint3w_PointInfinity_MultipleCalls_ReturnEqualInstances()
        {
            var infinity1 = ECPoint3w.POINT_INFINITY;
            var infinity2 = ECPoint3w.POINT_INFINITY;

            Assert.Equal(infinity1, infinity2);
            Assert.True(infinity1 == infinity2);
        }

        [Fact]
        public void ECPoint3w_PointInfinity_AnyPointWithZZero_IsInfinity()
        {
            BigInteger x = 42;
            BigInteger y = 100;
            BigInteger z = 0;

            var point = new ECPoint3w(x, y, z);
            Assert.True(point.IsInfinity);
            Assert.Equal(0, point.Z);
        }

        [Fact]
        public void ECPoint3w_DefaultStructValue_BehavesAsIdentityElement()
        {
            var defaultPoint = default(ECPoint3w);
            Assert.True(defaultPoint.IsInfinity);
            Assert.Equal(1, defaultPoint.X);

            Assert.Equal(1, defaultPoint.Y);
            Assert.Equal(0, defaultPoint.Z);
        }

        #endregion

        #region Coordinate Accessor Tests

        [Fact]
        public void ECPoint3w_GetX_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.Equal(x, point.X);
        }

        [Fact]
        public void ECPoint3w_GetX_IdentityElement_ReturnsOne()
        {
            var infinity = ECPoint3w.POINT_INFINITY;
            Assert.Equal(1, infinity.X);
        }

        [Fact]
        public void ECPoint3w_GetX_OffCurvePoint_ReturnsOne()
        {
            BigInteger x = 42;
            BigInteger y = 100;
            BigInteger z = 0;

            var point = new ECPoint3w(x, y, z);
            Assert.Equal(1, point.X);
        }

        [Fact]
        public void ECPoint3w_GetX_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;
            BigInteger z = 3;

            var point = new ECPoint3w(x, y, z);
            var result1 = point.X;

            var result2 = point.X;
            var result3 = point.X;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint3w_GetY_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.Equal(y, point.Y);
        }

        [Fact]
        public void ECPoint3w_GetY_IdentityElement_ReturnsOne()
        {
            var infinity = ECPoint3w.POINT_INFINITY;
            Assert.Equal(1, infinity.Y);
        }

        [Fact]
        public void ECPoint3w_GetY_OffCurvePoint_ReturnsOne()
        {
            BigInteger x = 42;
            BigInteger y = 100;
            BigInteger z = 0;

            var point = new ECPoint3w(x, y, z);
            Assert.Equal(1, point.Y);
        }

        [Fact]
        public void ECPoint3w_GetY_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;
            BigInteger z = 3;

            var point = new ECPoint3w(x, y, z);
            var result1 = point.Y;

            var result2 = point.Y;
            var result3 = point.Y;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint3w_GetZ_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 2;

            var point = new ECPoint3w(x, y, z);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint3w_GetZ_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint3w.POINT_INFINITY;
            Assert.Equal(0, infinity.Z);
        }

        [Fact]
        public void ECPoint3w_GetZ_OffCurvePoint_ReturnsZero()
        {
            BigInteger x = 42;
            BigInteger y = 100;
            BigInteger z = 0;

            var point = new ECPoint3w(x, y, z);
            Assert.Equal(0, point.Z);
        }

        [Fact]
        public void ECPoint3w_GetZ_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;
            BigInteger z = 7;

            var point = new ECPoint3w(x, y, z);
            var result1 = point.Z;

            var result2 = point.Z;
            var result3 = point.Z;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint3w_CoordinateAccessors_WithLargeZ_ReturnCorrectValues()
        {
            BigInteger x = 100;
            BigInteger y = 200;

            BigInteger z = BigInteger.Parse("12345678901234567890");
            var point = new ECPoint3w(x, y, z);
            Assert.Equal(x, point.X);

            Assert.Equal(y, point.Y);
            Assert.Equal(z, point.Z);
        }

        #endregion

        #region Point Validity Tests

        [Fact]
        public void ECPoint3w_IsInfinity_FinitePoint_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void ECPoint3w_IsInfinity_IdentityElement_ReturnsTrue()
        {
            BigInteger x = 1;
            BigInteger y = 1;
            BigInteger z = 0;

            var point = new ECPoint3w(x, y, z);
            Assert.True(point.IsInfinity);
        }

        [Fact]
        public void ECPoint3w_IsInfinity_PointAtInfinityConstant_ReturnsTrue()
        {
            Assert.True(ECPoint3w.POINT_INFINITY.IsInfinity);
        }

        [Fact]
        public void ECPoint3w_IsInfinity_NonZeroZ_ReturnsFalse()
        {
            var zValues = new[] { 1, 2, 3,
                100, -1, -5 };

            foreach (var zVal in zValues)
            {
                BigInteger x = 5;
                BigInteger y = 10;
                BigInteger z = zVal;

                var point = new ECPoint3w(x, y, z);
                Assert.False(point.IsInfinity);
            }
        }

        [Fact]
        public void ECPoint3w_IsInfinity_ZeroZ_AlwaysReturnsTrue()
        {
            var coordinates = new[]
            {
                (1, 1), (0, 0), (-1, -1),
                (42, 100), (999, 888)
            };

            foreach (var (xVal, yVal) in coordinates)
            {
                BigInteger x = xVal;
                BigInteger y = yVal;
                BigInteger z = 0;

                var point = new ECPoint3w(x, y, z);
                Assert.True(point.IsInfinity);
            }
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void ECPoint3w_Equals_SameCoordinates_ReturnsTrue()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point1 = new ECPoint3w(x, y, z);
            var point2 = new ECPoint3w(x, y, z);

            Assert.True(point1.Equals(point2));
            Assert.True(point1 == point2);
        }

        [Fact]
        public void ECPoint3w_Equals_SameInstance_ReturnsTrue()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.True(point.Equals(point));
        }

        [Fact]
        public void ECPoint3w_Equals_TwoIdentityElements_ReturnsTrue()
        {
            var inf1 = ECPoint3w.POINT_INFINITY;
            var inf2 = ECPoint3w.POINT_INFINITY;

            Assert.True(inf1.Equals(inf2));
            Assert.True(inf1 == inf2);
        }

        [Fact]
        public void ECPoint3w_Equals_TwoOffCurvePoints_ReturnsTrue()
        {
            BigInteger x1 = 5, y1 = 10, z1 = 0;
            BigInteger x2 = 42, y2 = 100, z2 = 0;

            var off1 = new ECPoint3w(x1, y1, z1);
            var off2 = new ECPoint3w(x2, y2, z2);

            Assert.True(off1.Equals(off2));
            Assert.True(off1 == off2);
        }

        [Fact]
        public void ECPoint3w_Equals_SameProjectiveCoordinates_ReturnsTrue()
        {
            BigInteger x = 6;
            BigInteger y = 12;
            BigInteger z = 2;

            var point1 = new ECPoint3w(x, y, z);
            var point2 = new ECPoint3w(x, y, z);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint3w_Equals_ZeroCoordinatePoints_ReturnsTrue()
        {
            BigInteger x = 0;
            BigInteger y = 0;
            BigInteger z = 1;

            var point1 = new ECPoint3w(x, y, z);
            var point2 = new ECPoint3w(x, y, z);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint3w_Equals_NegativeCoordinatePoints_ReturnsTrue()
        {
            BigInteger x = -1;
            BigInteger y = -1;
            BigInteger z = 1;

            var point1 = new ECPoint3w(x, y, z);
            var point2 = new ECPoint3w(x, y, z);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint3w_Equals_LargeCoordinatePoints_ReturnsTrue()
        {
            var x = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671663");
            var y = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");
            BigInteger z = 1;

            var point1 = new ECPoint3w(x, y, z);
            var point2 = new ECPoint3w(x, y, z);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint3w_Equals_DifferentXCoordinate_ReturnsFalse()
        {
            BigInteger x1 = 5, x2 = 15;
            BigInteger y = 10;
            BigInteger z = 1;

            var point1 = new ECPoint3w(x1, y, z);
            var point2 = new ECPoint3w(x2, y, z);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint3w_Equals_DifferentYCoordinate_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y1 = 10, y2 = 20;
            BigInteger z = 1;

            var point1 = new ECPoint3w(x, y1, z);
            var point2 = new ECPoint3w(x, y2, z);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint3w_Equals_DifferentZCoordinate_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z1 = 1, z2 = 2;
            var point1 = new ECPoint3w(x, y, z1);
            var point2 = new ECPoint3w(x, y, z2);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint3w_Equals_CompletelyDifferentCoordinates_ReturnsFalse()
        {
            BigInteger x1 = 5, y1 = 10, z1 = 1;
            BigInteger x2 = 15, y2 = 20, z2 = 2;

            var point1 = new ECPoint3w(x1, y1, z1);
            var point2 = new ECPoint3w(x2, y2, z2);
            Assert.False(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint3w_Equals_FinitePointVsIdentityElement_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            var infinity = ECPoint3w.POINT_INFINITY;

            Assert.False(point.Equals(infinity));
            Assert.True(point != infinity);
        }

        [Fact]
        public void ECPoint3w_Equals_FinitePointVsOffCurvePoint_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var onCurve = new ECPoint3w(x, y, 1);
            var offCurve = new ECPoint3w(x, y, 0);

            Assert.False(onCurve.Equals(offCurve));
            Assert.True(onCurve != offCurve);
        }

        [Fact]
        public void ECPoint3w_Equals_NullObject_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.False(point.Equals(null));
        }

        [Fact]
        public void ECPoint3w_Equals_DifferentType_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.False(point.Equals("not a point"));

            Assert.False(point.Equals(42));
            Assert.False(point.Equals(new object()));
        }

        [Fact]
        public void ECPoint3w_Equals_SwappedCoordinates_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point1 = new ECPoint3w(x, y, z);
            var point2 = new ECPoint3w(y, x, z);
            Assert.False(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint3w_EqualityOperator_Transitivity()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var p1 = new ECPoint3w(x, y, z);
            var p2 = new ECPoint3w(x, y, z);
            var p3 = new ECPoint3w(x, y, z);

            Assert.True(p1 == p2);
            Assert.True(p2 == p3);
            Assert.True(p1 == p3);
        }

        [Fact]
        public void ECPoint3w_InequalityOperator_OppositeOfEqualityOperator()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var p1 = new ECPoint3w(x, y, z);
            var p2 = new ECPoint3w(x, y, z);
            var p3 = new ECPoint3w(x + 1, y, z);

            Assert.True((p1 == p2) == !(p1 != p2));
            Assert.True((p1 == p3) == !(p1 != p3));
        }

        #endregion

        #region GetHashCode Tests

        [Fact]
        public void ECPoint3w_GetHashCode_SamePoints_HaveSameHashCode()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point1 = new ECPoint3w(x, y, z);
            var point2 = new ECPoint3w(x, y, z);

            Assert.Equal(point1.GetHashCode(),
                point2.GetHashCode());
        }

        [Fact]
        public void ECPoint3w_GetHashCode_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint3w.POINT_INFINITY;
            Assert.Equal(0, infinity.GetHashCode());
        }

        [Fact]
        public void ECPoint3w_GetHashCode_OffCurvePoints_ReturnZero()
        {
            BigInteger x1 = 5, y1 = 10, z1 = 0;
            BigInteger x2 = 42, y2 = 100, z2 = 0;

            var off1 = new ECPoint3w(x1, y1, z1);
            var off2 = new ECPoint3w(x2, y2, z2);

            Assert.Equal(0, off1.GetHashCode());
            Assert.Equal(off1.GetHashCode(), off2.GetHashCode());
        }

        [Fact]
        public void ECPoint3w_GetHashCode_ConsistentAcrossMultipleCalls()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            int hash1 = point.GetHashCode();

            int hash2 = point.GetHashCode();
            int hash3 = point.GetHashCode();

            Assert.Equal(hash1, hash2);
            Assert.Equal(hash2, hash3);
        }

        [Fact]
        public void ECPoint3w_GetHashCode_DifferentPoints_MayDiffer()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point1 = new ECPoint3w(x, y, z);
            var point2 = new ECPoint3w(x + 1, y, z);

            int hash1 = point1.GetHashCode();
            int hash2 = point2.GetHashCode();
            Assert.True(hash1 != 0 || hash2 != 0);
        }

        [Fact]
        public void ECPoint3w_GetHashCode_DifferentZValues_ProduceDifferentHashes()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z1 = 1, z2 = 2;

            var point1 = new ECPoint3w(x, y, z1);
            var point2 = new ECPoint3w(x, y, z2);

            int hash1 = point1.GetHashCode();
            int hash2 = point2.GetHashCode();
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void ECPoint3w_GetHashCode_VariousCoordinateValues_Consistent()
        {
            var values = new[] { 0, 1,
                -1, 42, 256 };

            foreach (var xVal in values)
            {
                foreach (var yVal in values)
                {
                    foreach (var zVal in new[] { 1, 2, 3 })
                    {
                        BigInteger x = xVal;
                        BigInteger y = yVal;
                        BigInteger z = zVal;

                        var point1 = new ECPoint3w(x, y, z);
                        var point2 = new ECPoint3w(x, y, z);

                        Assert.Equal(point1.GetHashCode(),
                            point2.GetHashCode());
                    }
                }
            }
        }

        #endregion

        #region Collection Integration Tests

        [Fact]
        public void ECPoint3w_HashSet_HandlesJacobianPointsCorrectly()
        {
            BigInteger x1 = 5, y1 = 10, z1 = 1;
            BigInteger x2 = 15, y2 = 20, z2 = 1;

            var set = new HashSet<ECPoint3w>();
            var p1 = new ECPoint3w(x1, y1, z1);

            var p2 = new ECPoint3w(x1, y1, z1);
            var p3 = new ECPoint3w(x2, y2, z2);

            set.Add(p1);
            Assert.Single(set);

            Assert.Contains(p2, set);
            Assert.DoesNotContain(p3, set);
        }

        [Fact]
        public void ECPoint3w_HashSet_DifferentZValues_TreatedAsDifferent()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z1 = 1, z2 = 2;
            var set = new HashSet<ECPoint3w>();

            var p1 = new ECPoint3w(x, y, z1);
            var p2 = new ECPoint3w(x, y, z2);

            set.Add(p1); set.Add(p2);
            Assert.Equal(2, set.Count);
        }

        [Fact]
        public void ECPoint3w_HashSet_IdentityElementsHandledCorrectly()
        {
            var set = new HashSet<ECPoint3w>();
            var inf1 = ECPoint3w.POINT_INFINITY;
            BigInteger x = 42;
            BigInteger y = 100;
            BigInteger z = 0;

            var inf2 = new ECPoint3w(x, y, z);
            set.Add(inf1);

            Assert.Single(set);
            Assert.Contains(inf2, set);
        }

        [Fact]
        public void ECPoint3w_Dictionary_HandlesPointsAsKeys()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var dict = new Dictionary<ECPoint3w, string>();
            var p1 = new ECPoint3w(x, y, z);
            var p2 = new ECPoint3w(x, y, z);

            dict[p1] = "original";
            Assert.Equal("original", dict[p2]);
        }

        [Fact]
        public void ECPoint3w_List_ContainsUsesEquality()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var list = new List<ECPoint3w>();
            var p1 = new ECPoint3w(x, y, z);
            var p2 = new ECPoint3w(x, y, z);

            list.Add(p1);

            Assert.Contains(p2, list);
        }

        [Fact]
        public void ECPoint3w_List_IdentityElement_ContainsWorks()
        {
            var list = new List<ECPoint3w>();
            var inf1 = ECPoint3w.POINT_INFINITY;
            var inf2 = ECPoint3w.POINT_INFINITY;

            list.Add(inf1);
            Assert.Contains(inf2, list);
        }

        #endregion

        #region Value Type Semantics Tests

        [Fact]
        public void ECPoint3w_IsValueType()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.True(point.GetType().IsValueType);
        }

        [Fact]
        public void ECPoint3w_ValueSemantics_AssignmentCreatesCopy()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            var original = new ECPoint3w(x, y, z);
            var copy = original;

            Assert.Equal(original, copy);
            Assert.True(original == copy);
        }

        #endregion

        #region Real-World ECC Tests

        [Fact]
        public void ECPoint3w_Points_CanRepresentSecp256k1GeneratorInJacobian()
        {
            /* secp256k1 generator point in affine */
            var gx = BigInteger.Parse("55066263022277343669578718895168534326250603453777594175500187360389116729240");
            var gy = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");
            BigInteger z = 1;

            var generator = new ECPoint3w(gx, gy, z);
            Assert.False(generator.IsInfinity);
            Assert.Equal(gx, generator.X);

            Assert.Equal(gy, generator.Y);
            Assert.Equal(1, generator.Z);
        }

        [Fact]
        public void ECPoint3w_Points_WithProjectiveZ_CorrectlyStored()
        {
            var x = BigInteger.Parse("123456789012345678901234567890");
            var y = BigInteger.Parse("987654321098765432109876543210");
            BigInteger z = 5;

            var point = new ECPoint3w(x, y, z);
            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(z, point.Z);
            Assert.True(!point.IsInfinity);
        }

        [Fact]
        public void ECPoint3w_Points_IdentityElement_UsedInECCContext()
        {
            var infinity = ECPoint3w.POINT_INFINITY;
            BigInteger x = 5, y = 10, z = 1;

            var point = new ECPoint3w(x, y, z);
            Assert.False(point.Equals(infinity));

            Assert.True(infinity.IsInfinity);
            Assert.False(point.IsInfinity);
        }

        #endregion

        #region Stress Tests

        [Fact]
        public void ECPoint3w_ManyPointsCreation_NoExceptions()
        {
            for (int i = 0; i < 1000; i++)
            {
                BigInteger x = i;
                BigInteger y = i * 2;

                BigInteger z = (i % 10) + 1;
                var point = new ECPoint3w(x, y, z);
                Assert.Equal(x, point.X);

                Assert.Equal(y, point.Y);
                Assert.Equal(z, point.Z);
            }
        }

        [Fact]
        public void ECPoint3w_HashCodeDistribution_MultiplePoints()
        {
            var hashCodes = new HashSet<int>();

            for (int i = 0; i < 100; i++)
            {
                BigInteger x = i;
                BigInteger y = i * 2;
                BigInteger z = (i % 5) + 1;

                var point = new ECPoint3w(x, y, z);
                hashCodes.Add(point.GetHashCode());
            }

            Assert.True(hashCodes.Count >= 20);
        }

        #endregion
    }
}
