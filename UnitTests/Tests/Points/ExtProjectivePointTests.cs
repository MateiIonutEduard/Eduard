using System;
using Eduard.Security.Primitives;
using System.Collections.Generic;

namespace Eduard.Tests.Points
{
    public class ExtProjectivePointTests
    {
        #region Constructor Tests

        [Fact]
        public void ECPoint4_Constructor_WithValidCoordinates_CreatesExtendedProjectivePoint()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.False(point.IsInfinity);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(t, point.T);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint4_Constructor_WithNonUnitZ_CreatesValidPoint()
        {
            BigInteger x = 6;
            BigInteger y = 12;

            BigInteger t = 36;
            BigInteger z = 2;

            var point = new ECPoint4(x, y, t, z);
            Assert.False(point.IsInfinity);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(t, point.T);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint4_Constructor_WithZZeroAndTZero_CreatesInfinityPoint()
        {
            BigInteger x = 0;
            BigInteger y = 1;

            BigInteger t = 0;
            BigInteger z = 0;

            var point = new ECPoint4(x, y, t, z);
            Assert.True(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4_Constructor_WithZZeroAndNonZeroT_ThrowsInvalidOperationException()
        {
            BigInteger x = 0;
            BigInteger y = 1;

            BigInteger t = 5;
            BigInteger z = 0;

            Assert.Throws<InvalidOperationException>(() =>
                new ECPoint4(x, y, t, z));
        }

        [Fact]
        public void ECPoint4_Constructor_WithZeroCoordinates_CreatesValidPoint()
        {
            BigInteger x = 0;
            BigInteger y = 0;

            BigInteger t = 0;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.False(point.IsInfinity);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(t, point.T);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint4_Constructor_WithNegativeCoordinates_CreatesValidPoint()
        {
            BigInteger x = -1;
            BigInteger y = -1;

            BigInteger t = 1;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.False(point.IsInfinity);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(t, point.T);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint4_Constructor_WithNullX_ThrowsArgumentNullException()
        {
            BigInteger y = 10;
            BigInteger t = 50;
            BigInteger z = 1;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint4(null, y, t, z));
        }

        [Fact]
        public void ECPoint4_Constructor_WithNullY_ThrowsArgumentNullException()
        {
            BigInteger x = 5;
            BigInteger t = 50;
            BigInteger z = 1;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint4(x, null, t, z));
        }

        [Fact]
        public void ECPoint4_Constructor_WithNullT_ThrowsArgumentNullException()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint4(x, y, null, z));
        }

        [Fact]
        public void ECPoint4_Constructor_WithNullZ_ThrowsArgumentNullException()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger t = 50;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint4(x, y, t, null));
        }

        [Fact]
        public void ECPoint4_Constructor_WithAllNullCoordinates_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint4(null, null, null, null));
        }

        [Fact]
        public void ECPoint4_Constructor_WithLargeCoordinates_CreatesValidPoint()
        {
            var x = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671663");
            var y = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");

            BigInteger t = BigInteger.Parse("12345678901234567890");
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.False(point.IsInfinity);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(t, point.T);
            Assert.Equal(z, point.Z);
        }

        #endregion

        #region Identity Element Tests

        [Fact]
        public void ECPoint4_PointInfinity_ReturnsIdentityElement()
        {
            var infinity = ECPoint4.POINT_INFINITY;
            Assert.True(infinity.IsInfinity);
        }

        [Fact]
        public void ECPoint4_PointInfinity_HasZEqualToZero()
        {
            var infinity = ECPoint4.POINT_INFINITY;
            Assert.Equal(0, infinity.Z);
        }

        [Fact]
        public void ECPoint4_PointInfinity_HasTEqualToZero()
        {
            var infinity = ECPoint4.POINT_INFINITY;
            Assert.Equal(0, infinity.T);
        }

        [Fact]
        public void ECPoint4_PointInfinity_NormalizedCoordinates_ReturnsZeroOneZeroZero()
        {
            var infinity = ECPoint4.POINT_INFINITY;
            Assert.Equal(0, infinity.X);
            Assert.Equal(1, infinity.Y);

            Assert.Equal(0, infinity.T);
            Assert.Equal(0, infinity.Z);
        }

        [Fact]
        public void ECPoint4_PointInfinity_MultipleCalls_ReturnEqualInstances()
        {
            var infinity1 = ECPoint4.POINT_INFINITY;
            var infinity2 = ECPoint4.POINT_INFINITY;
            Assert.Equal(infinity1, infinity2);
            Assert.True(infinity1 == infinity2);
        }

        [Fact]
        public void ECPoint4_PointInfinity_AnyPointWithZZeroAndTZero_IsInfinity()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger t = 0;
            BigInteger z = 0;

            var point = new ECPoint4(x, y, t, z);
            Assert.True(point.IsInfinity);

            Assert.Equal(0, point.Z);
            Assert.Equal(0, point.T);
        }

        [Fact]
        public void ECPoint4_DefaultStructValue_BehavesAsIdentityElement()
        {
            var defaultPoint = default(ECPoint4);
            Assert.True(defaultPoint.IsInfinity);

            Assert.Equal(0, defaultPoint.X);
            Assert.Equal(1, defaultPoint.Y);

            Assert.Equal(0, defaultPoint.T);
            Assert.Equal(0, defaultPoint.Z);
        }

        #endregion

        #region Coordinate Accessor Tests

        [Fact]
        public void ECPoint4_GetX_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(x, point.X);
        }

        [Fact]
        public void ECPoint4_GetX_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint4.POINT_INFINITY;
            Assert.Equal(0, infinity.X);
        }

        [Fact]
        public void ECPoint4_GetX_OffCurvePoint_ReturnsZero()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger t = 0;
            BigInteger z = 0;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(0, point.X);
        }

        [Fact]
        public void ECPoint4_GetX_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;
            BigInteger t = 838102050;
            BigInteger z = 3;

            var point = new ECPoint4(x, y, t, z);
            var result1 = point.X;

            var result2 = point.X;
            var result3 = point.X;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint4_GetY_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(y, point.Y);
        }

        [Fact]
        public void ECPoint4_GetY_IdentityElement_ReturnsOne()
        {
            var infinity = ECPoint4.POINT_INFINITY;
            Assert.Equal(1, infinity.Y);
        }

        [Fact]
        public void ECPoint4_GetY_OffCurvePoint_ReturnsOne()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger t = 0;
            BigInteger z = 0;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(1, point.Y);
        }

        [Fact]
        public void ECPoint4_GetY_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;

            BigInteger t = 838102050;
            BigInteger z = 3;

            var point = new ECPoint4(x, y, t, z);
            var result1 = point.Y;

            var result2 = point.Y;
            var result3 = point.Y;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint4_GetT_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(t, point.T);
        }

        [Fact]
        public void ECPoint4_GetT_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint4.POINT_INFINITY;
            Assert.Equal(0, infinity.T);
        }

        [Fact]
        public void ECPoint4_GetT_OffCurvePoint_ReturnsZero()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger t = 0;
            BigInteger z = 0;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(0, point.T);
        }

        [Fact]
        public void ECPoint4_GetT_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;

            BigInteger t = 838102050;
            BigInteger z = 3;

            var point = new ECPoint4(x, y, t, z);
            var result1 = point.T;

            var result2 = point.T;
            var result3 = point.T;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint4_GetZ_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 2;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint4_GetZ_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint4.POINT_INFINITY;
            Assert.Equal(0, infinity.Z);
        }

        [Fact]
        public void ECPoint4_GetZ_OffCurvePoint_ReturnsZero()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger t = 0;
            BigInteger z = 0;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(0, point.Z);
        }

        [Fact]
        public void ECPoint4_GetZ_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;

            BigInteger t = 838102050;
            BigInteger z = 7;

            var point = new ECPoint4(x, y, t, z);
            var result1 = point.Z;

            var result2 = point.Z;
            var result3 = point.Z;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint4_CoordinateAccessors_WithLargeT_ReturnCorrectValues()
        {
            BigInteger x = 100;
            BigInteger y = 200;

            BigInteger t = BigInteger.Parse("12345678901234567890");
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(t, point.T);
            Assert.Equal(z, point.Z);
        }

        #endregion

        #region Point Validity Tests

        [Fact]
        public void ECPoint4_IsInfinity_FinitePoint_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4_IsInfinity_IdentityElement_ReturnsTrue()
        {
            BigInteger x = 0;
            BigInteger y = 1;

            BigInteger t = 0;
            BigInteger z = 0;

            var point = new ECPoint4(x, y, t, z);
            Assert.True(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4_IsInfinity_PointAtInfinityConstant_ReturnsTrue()
        {
            Assert.True(ECPoint4.POINT_INFINITY.IsInfinity);
        }

        [Fact]
        public void ECPoint4_IsInfinity_NonZeroZ_ReturnsFalse()
        {
            var zValues = new[] { 1, 2, 3, 100 };

            foreach (var zVal in zValues)
            {
                BigInteger x = 5;
                BigInteger y = 10;

                BigInteger t = 50 / zVal;
                BigInteger z = zVal;

                var point = new ECPoint4(x, y, t, z);
                Assert.False(point.IsInfinity);
            }
        }

        [Fact]
        public void ECPoint4_IsInfinity_ZeroZAndZeroT_AlwaysReturnsTrue()
        {
            var coordinates = new[]
            {
                (0, 1), (1, 1), (-1, -1), 
                (42, 100), (999, 888)
            };

            foreach (var (xVal, yVal) in coordinates)
            {
                BigInteger x = xVal;
                BigInteger y = yVal;

                BigInteger t = 0;
                BigInteger z = 0;

                var point = new ECPoint4(x, y, t, z);
                Assert.True(point.IsInfinity);
            }
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void ECPoint4_Equals_SameCoordinates_ReturnsTrue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point1 = new ECPoint4(x, y, t, z);
            var point2 = new ECPoint4(x, y, t, z);

            Assert.True(point1.Equals(point2));
            Assert.True(point1 == point2);
        }

        [Fact]
        public void ECPoint4_Equals_SameInstance_ReturnsTrue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.True(point.Equals(point));
        }

        [Fact]
        public void ECPoint4_Equals_TwoIdentityElements_ReturnsTrue()
        {
            var inf1 = ECPoint4.POINT_INFINITY;
            var inf2 = ECPoint4.POINT_INFINITY;

            Assert.True(inf1.Equals(inf2));
            Assert.True(inf1 == inf2);
        }

        [Fact]
        public void ECPoint4_Equals_TwoOffCurvePoints_ReturnsTrue()
        {
            BigInteger x1 = 5, y1 = 10, t1 = 0, z1 = 0;
            BigInteger x2 = 42, y2 = 100, t2 = 0, z2 = 0;

            var off1 = new ECPoint4(x1, y1, t1, z1);
            var off2 = new ECPoint4(x2, y2, t2, z2);

            Assert.True(off1.Equals(off2));
            Assert.True(off1 == off2);
        }

        [Fact]
        public void ECPoint4_Equals_SameExtendedCoordinates_ReturnsTrue()
        {
            BigInteger x = 6;
            BigInteger y = 12;

            BigInteger t = 36;
            BigInteger z = 2;

            var point1 = new ECPoint4(x, y, t, z);
            var point2 = new ECPoint4(x, y, t, z);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4_Equals_ZeroCoordinatePoints_ReturnsTrue()
        {
            BigInteger x = 0;
            BigInteger y = 0;

            BigInteger t = 0;
            BigInteger z = 1;

            var point1 = new ECPoint4(x, y, t, z);
            var point2 = new ECPoint4(x, y, t, z);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4_Equals_NegativeCoordinatePoints_ReturnsTrue()
        {
            BigInteger x = -1;
            BigInteger y = -1;

            BigInteger t = 1;
            BigInteger z = 1;

            var point1 = new ECPoint4(x, y, t, z);
            var point2 = new ECPoint4(x, y, t, z);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4_Equals_LargeCoordinatePoints_ReturnsTrue()
        {
            var x = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671663");
            var y = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");

            BigInteger t = BigInteger.Parse("12345678901234567890");
            BigInteger z = 1;

            var point1 = new ECPoint4(x, y, t, z);
            var point2 = new ECPoint4(x, y, t, z);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4_Equals_DifferentXCoordinate_ReturnsFalse()
        {
            BigInteger x1 = 5, x2 = 15;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point1 = new ECPoint4(x1, y, t, z);
            var point2 = new ECPoint4(x2, y, t, z);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint4_Equals_DifferentYCoordinate_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y1 = 10, y2 = 20;

            BigInteger t = 50;
            BigInteger z = 1;

            var point1 = new ECPoint4(x, y1, t, z);
            var point2 = new ECPoint4(x, y2, t, z);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint4_Equals_DifferentTCoordinate_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t1 = 50, t2 = 100;
            BigInteger z = 1;

            var point1 = new ECPoint4(x, y, t1, z);
            var point2 = new ECPoint4(x, y, t2, z);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint4_Equals_DifferentZCoordinate_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z1 = 1, z2 = 2;

            var point1 = new ECPoint4(x, y, t, z1);
            var point2 = new ECPoint4(x, y, t, z2);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint4_Equals_CompletelyDifferentCoordinates_ReturnsFalse()
        {
            BigInteger x1 = 5, y1 = 10, t1 = 50, z1 = 1;
            BigInteger x2 = 15, y2 = 20, t2 = 150, z2 = 2;
            var point1 = new ECPoint4(x1, y1, t1, z1);

            var point2 = new ECPoint4(x2, y2, t2, z2);
            Assert.False(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4_Equals_FinitePointVsIdentityElement_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            var infinity = ECPoint4.POINT_INFINITY;

            Assert.False(point.Equals(infinity));
            Assert.True(point != infinity);
        }

        [Fact]
        public void ECPoint4_Equals_FinitePointVsOffCurvePoint_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var onCurve = new ECPoint4(x, y, 50, 1);
            var offCurve = new ECPoint4(x, y, 0, 0);

            Assert.False(onCurve.Equals(offCurve));
            Assert.True(onCurve != offCurve);
        }

        [Fact]
        public void ECPoint4_Equals_NullObject_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.False(point.Equals(null));
        }

        [Fact]
        public void ECPoint4_Equals_DifferentType_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.False(point.Equals("not a point"));

            Assert.False(point.Equals(42));
            Assert.False(point.Equals(new object()));
        }

        [Fact]
        public void ECPoint4_EqualityOperator_Transitivity()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var p1 = new ECPoint4(x, y, t, z);
            var p2 = new ECPoint4(x, y, t, z);

            var p3 = new ECPoint4(x, y, t, z);
            Assert.True(p1 == p2);

            Assert.True(p2 == p3);
            Assert.True(p1 == p3);
        }

        [Fact]
        public void ECPoint4_InequalityOperator_OppositeOfEqualityOperator()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var p1 = new ECPoint4(x, y, t, z);
            var p2 = new ECPoint4(x, y, t, z);
            var p3 = new ECPoint4(x + 1, y, t, z);

            Assert.True((p1 == p2) == !(p1 != p2));
            Assert.True((p1 == p3) == !(p1 != p3));
        }

        #endregion

        #region GetHashCode Tests

        [Fact]
        public void ECPoint4_GetHashCode_SamePoints_HaveSameHashCode()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point1 = new ECPoint4(x, y, t, z);
            var point2 = new ECPoint4(x, y, t, z);

            Assert.Equal(point1.GetHashCode(), 
                point2.GetHashCode());
        }

        [Fact]
        public void ECPoint4_GetHashCode_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint4.POINT_INFINITY;
            Assert.Equal(0, infinity.GetHashCode());
        }

        [Fact]
        public void ECPoint4_GetHashCode_OffCurvePoints_ReturnZero()
        {
            BigInteger x1 = 5, y1 = 10, t1 = 0, z1 = 0;
            BigInteger x2 = 42, y2 = 100, t2 = 0, z2 = 0;

            var off1 = new ECPoint4(x1, y1, t1, z1);
            var off2 = new ECPoint4(x2, y2, t2, z2);

            Assert.Equal(0, off1.GetHashCode());
            Assert.Equal(off1.GetHashCode(), off2.GetHashCode());
        }

        [Fact]
        public void ECPoint4_GetHashCode_ConsistentAcrossMultipleCalls()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            int hash1 = point.GetHashCode();

            int hash2 = point.GetHashCode();
            int hash3 = point.GetHashCode();

            Assert.Equal(hash1, hash2);
            Assert.Equal(hash2, hash3);
        }

        [Fact]
        public void ECPoint4_GetHashCode_DifferentPoints_MayDiffer()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point1 = new ECPoint4(x, y, t, z);
            var point2 = new ECPoint4(x + 1, y, t, z);
            int hash1 = point1.GetHashCode();

            int hash2 = point2.GetHashCode();
            Assert.True(hash1 != 0 || hash2 != 0);
        }

        [Fact]
        public void ECPoint4_GetHashCode_DifferentTValues_ProduceDifferentHashes()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t1 = 50, t2 = 100;
            BigInteger z = 1;

            var point1 = new ECPoint4(x, y, t1, z);
            var point2 = new ECPoint4(x, y, t2, z);
            int hash1 = point1.GetHashCode();

            int hash2 = point2.GetHashCode();
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void ECPoint4_GetHashCode_VariousCoordinateValues_Consistent()
        {
            var values = new[] { 0, 1, -1, 42, 256 };

            foreach (var xVal in values)
            {
                foreach (var yVal in values)
                {
                    foreach (var zVal in new[] { 1, 2, 3 })
                    {
                        BigInteger x = xVal;
                        BigInteger y = yVal;

                        BigInteger t = xVal * yVal / zVal;
                        BigInteger z = zVal;

                        var point1 = new ECPoint4(x, y, t, z);
                        var point2 = new ECPoint4(x, y, t, z);

                        Assert.Equal(point1.GetHashCode(), 
                            point2.GetHashCode());
                    }
                }
            }
        }

        #endregion

        #region Collection Integration Tests

        [Fact]
        public void ECPoint4_HashSet_HandlesExtendedProjectivePointsCorrectly()
        {
            BigInteger x1 = 5, y1 = 10, t1 = 50, z1 = 1;
            BigInteger x2 = 15, y2 = 20, t2 = 300, z2 = 1;

            var set = new HashSet<ECPoint4>();
            var p1 = new ECPoint4(x1, y1, t1, z1);

            var p2 = new ECPoint4(x1, y1, t1, z1);
            var p3 = new ECPoint4(x2, y2, t2, z2);

            set.Add(p1);
            Assert.Single(set);

            Assert.Contains(p2, set);
            Assert.DoesNotContain(p3, set);
        }

        [Fact]
        public void ECPoint4_HashSet_DifferentTValues_TreatedAsDifferent()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t1 = 50, t2 = 100;
            BigInteger z = 1;

            var set = new HashSet<ECPoint4>();
            var p1 = new ECPoint4(x, y, t1, z);
            var p2 = new ECPoint4(x, y, t2, z);

            set.Add(p1); set.Add(p2);
            Assert.Equal(2, set.Count);
        }

        [Fact]
        public void ECPoint4_HashSet_IdentityElementsHandledCorrectly()
        {
            var set = new HashSet<ECPoint4>();
            var inf1 = ECPoint4.POINT_INFINITY;

            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger t = 0;
            BigInteger z = 0;

            var inf2 = new ECPoint4(x, y, t, z);
            set.Add(inf1);

            Assert.Single(set);
            Assert.Contains(inf2, set);
        }

        [Fact]
        public void ECPoint4_Dictionary_HandlesPointsAsKeys()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var dict = new Dictionary<ECPoint4, string>();
            var p1 = new ECPoint4(x, y, t, z);
            var p2 = new ECPoint4(x, y, t, z);

            dict[p1] = "original";
            Assert.Equal("original", dict[p2]);
        }

        [Fact]
        public void ECPoint4_List_ContainsUsesEquality()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var list = new List<ECPoint4>();
            var p1 = new ECPoint4(x, y, t, z);
            var p2 = new ECPoint4(x, y, t, z);

            list.Add(p1);
            Assert.Contains(p2, list);
        }

        [Fact]
        public void ECPoint4_List_IdentityElement_ContainsWorks()
        {
            var list = new List<ECPoint4>();
            var inf1 = ECPoint4.POINT_INFINITY;
            var inf2 = ECPoint4.POINT_INFINITY;

            list.Add(inf1);
            Assert.Contains(inf2, list);
        }

        #endregion

        #region Value Type Semantics Tests

        [Fact]
        public void ECPoint4_IsValueType()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.True(point.GetType().IsValueType);
        }

        [Fact]
        public void ECPoint4_ValueSemantics_AssignmentCreatesCopy()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var original = new ECPoint4(x, y, t, z);
            var copy = original;

            Assert.Equal(original, copy);
            Assert.True(original == copy);
        }

        #endregion

        #region Real-World ECC Tests

        [Fact]
        public void ECPoint4_Points_CanRepresentEd25519BasePointInExtended()
        {
            var x = BigInteger.Parse("15112221349535800782501122409520352403146587523749316481546487528623150257971");
            var y = BigInteger.Parse("46316835694926478169428394003475163141307993866256225615783033603165251855960");

            BigInteger t = BigInteger.Parse("46827403850823138040409258857748751836130598381944710651147842685073140789320");
            BigInteger z = 1;

            var basePoint = new ECPoint4(x, y, t, z);
            Assert.False(basePoint.IsInfinity);

            Assert.Equal(x, basePoint.X);
            Assert.Equal(y, basePoint.Y);

            Assert.Equal(t, basePoint.T);
            Assert.Equal(1, basePoint.Z);
        }

        [Fact]
        public void ECPoint4_Points_WithExtendedZ_CorrectlyStored()
        {
            var x = BigInteger.Parse("123456789012345678901234567890");
            var y = BigInteger.Parse("987654321098765432109876543210");

            BigInteger t = BigInteger.Parse("12193263113712995431619503321700856618036222682");
            BigInteger z = 5;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(x, point.X);

            Assert.Equal(y, point.Y);
            Assert.Equal(t, point.T);

            Assert.Equal(z, point.Z);
            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4_Points_IdentityElement_UsedInECCContext()
        {
            var infinity = ECPoint4.POINT_INFINITY;
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger t = 50;
            BigInteger z = 1;

            var point = new ECPoint4(x, y, t, z);
            Assert.False(point.Equals(infinity));

            Assert.True(infinity.IsInfinity);
            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4_Points_TRelationship_TEqualsXYDivZ()
        {
            BigInteger x = 6;
            BigInteger y = 12;

            BigInteger t = 36;
            BigInteger z = 2;

            var point = new ECPoint4(x, y, t, z);
            Assert.Equal(x, point.X);

            Assert.Equal(y, point.Y);
            Assert.Equal(t, point.T);

            Assert.Equal(z, point.Z);
            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4_Points_InfinityRequiresZeroT()
        {
            BigInteger x = 0;
            BigInteger y = 1;

            BigInteger t = 0;
            BigInteger z = 0;

            var infinity = new ECPoint4(x, y, t, z);
            Assert.True(infinity.IsInfinity);

            Assert.Equal(0, infinity.Z);
            Assert.Equal(0, infinity.T);
        }

        #endregion

        #region Stress Tests

        [Fact]
        public void ECPoint4_ManyPointsCreation_NoExceptions()
        {
            for (int i = 0; i < 1000; i++)
            {
                BigInteger x = i;
                BigInteger y = i * 2;

                BigInteger z = (i % 10) + 1;
                BigInteger t = (x * y) / z;

                var point = new ECPoint4(x, y, t, z);
                Assert.Equal(x, point.X);
                Assert.Equal(y, point.Y);

                Assert.Equal(t, point.T);
                Assert.Equal(z, point.Z);
            }
        }

        [Fact]
        public void ECPoint4_HashCodeDistribution_MultiplePoints()
        {
            var hashCodes = new HashSet<int>();

            for (int i = 0; i < 100; i++)
            {
                BigInteger x = i;
                BigInteger y = i * 2;

                BigInteger z = (i % 5) + 1;
                BigInteger t = (x * y) / z;

                var point = new ECPoint4(x, y, t, z);
                hashCodes.Add(point.GetHashCode());
            }

            Assert.True(hashCodes.Count >= 95);
        }

        [Fact]
        public void ECPoint4_InfinityCreation_WithNonZeroT_AlwaysThrows()
        {
            var nonZeroTValues = new[] { 1, 5, -1, 100, 
                BigInteger.Parse("12345678901234567890") };

            foreach (var t in nonZeroTValues)
            {
                Assert.Throws<InvalidOperationException>(() =>
                    new ECPoint4(0, 1, t, 0));
            }
        }

        #endregion
    }
}
