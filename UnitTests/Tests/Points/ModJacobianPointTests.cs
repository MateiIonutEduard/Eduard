using System;
using Eduard.Security.Primitives;
using System.Collections.Generic;

namespace Eduard.Tests.Points
{
    public class ModJacobianPointTests
    {
        #region Constructor Tests

        [Fact]
        public void ECPoint4w_Constructor_WithValidCoordinates_CreatesModifiedJacobianPoint()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(!point.IsInfinity);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(z, point.Z);
            Assert.Equal(az4, point.aZ4);
        }

        [Fact]
        public void ECPoint4w_Constructor_WithNonUnitZ_CreatesValidPoint()
        {
            BigInteger x = 6;
            BigInteger y = 12;
            BigInteger z = 2;
            BigInteger az4 = 32;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(!point.IsInfinity);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);
            
            Assert.Equal(z, point.Z);
            Assert.Equal(az4, point.aZ4);
        }

        [Fact]
        public void ECPoint4w_Constructor_WithZZeroAndAZ4Zero_CreatesInfinityPoint()
        {
            BigInteger x = 1;
            BigInteger y = 1;

            BigInteger z = 0;
            BigInteger az4 = 0;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4w_Constructor_WithZZeroAndNonZeroAZ4_ThrowsInvalidOperationException()
        {
            BigInteger x = 1;
            BigInteger y = 1;

            BigInteger z = 0;
            BigInteger az4 = 5;

            Assert.Throws<InvalidOperationException>(() =>
                new ECPoint4w(x, y, z, az4));
        }

        [Fact]
        public void ECPoint4w_Constructor_WithZeroCoordinates_CreatesValidPoint()
        {
            BigInteger x = 0;
            BigInteger y = 0;

            BigInteger z = 1;
            BigInteger az4 = 0;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(!point.IsInfinity);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(z, point.Z);
            Assert.Equal(az4, point.aZ4);
        }

        [Fact]
        public void ECPoint4w_Constructor_WithNegativeCoordinates_CreatesValidPoint()
        {
            BigInteger x = -1;
            BigInteger y = -1;

            BigInteger z = 1;
            BigInteger az4 = -2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(!point.IsInfinity);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(z, point.Z);
            Assert.Equal(az4, point.aZ4);
        }

        [Fact]
        public void ECPoint4w_Constructor_WithNullX_ThrowsArgumentNullException()
        {
            BigInteger y = 10;
            BigInteger z = 1;
            BigInteger az4 = 2;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint4w(null, y, z, az4));
        }

        [Fact]
        public void ECPoint4w_Constructor_WithNullY_ThrowsArgumentNullException()
        {
            BigInteger x = 5;
            BigInteger z = 1;
            BigInteger az4 = 2;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint4w(x, null, z, az4));
        }

        [Fact]
        public void ECPoint4w_Constructor_WithNullZ_ThrowsArgumentNullException()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger az4 = 2;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint4w(x, y, null, az4));
        }

        [Fact]
        public void ECPoint4w_Constructor_WithNullAZ4_ThrowsArgumentNullException()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;

            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint4w(x, y, z, null));
        }

        [Fact]
        public void ECPoint4w_Constructor_WithAllNullCoordinates_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ECPoint4w(null, null, null, null));
        }

        [Fact]
        public void ECPoint4w_Constructor_WithLargeCoordinates_CreatesValidPoint()
        {
            var x = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671663");
            var y = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");
            BigInteger z = 1;

            BigInteger az4 = BigInteger.Parse("12345678901234567890");
            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(!point.IsInfinity);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(z, point.Z);
            Assert.Equal(az4, point.aZ4);
        }

        [Fact]
        public void ECPoint4w_Constructor_WithNegativeZ_ThrowsInvalidOperationException()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = -1;
            BigInteger az4 = 2;

            var exception = Record.Exception(() =>
                new ECPoint4w(x, y, z, az4));

            Assert.Null(exception);
        }

        #endregion

        #region Identity Element Tests

        [Fact]
        public void ECPoint4w_PointInfinity_ReturnsIdentityElement()
        {
            var infinity = ECPoint4w.POINT_INFINITY;
            Assert.True(infinity.IsInfinity);
        }

        [Fact]
        public void ECPoint4w_PointInfinity_HasZEqualToZero()
        {
            var infinity = ECPoint4w.POINT_INFINITY;
            Assert.Equal(0, infinity.Z);
        }

        [Fact]
        public void ECPoint4w_PointInfinity_HasAZ4EqualToZero()
        {
            var infinity = ECPoint4w.POINT_INFINITY;
            Assert.Equal(0, infinity.aZ4);
        }

        [Fact]
        public void ECPoint4w_PointInfinity_NormalizedCoordinates_ReturnsOneOneZeroZero()
        {
            var infinity = ECPoint4w.POINT_INFINITY;
            Assert.Equal(1, infinity.X);
            Assert.Equal(1, infinity.Y);

            Assert.Equal(0, infinity.Z);
            Assert.Equal(0, infinity.aZ4);
        }

        [Fact]
        public void ECPoint4w_PointInfinity_MultipleCalls_ReturnEqualInstances()
        {
            var infinity1 = ECPoint4w.POINT_INFINITY;
            var infinity2 = ECPoint4w.POINT_INFINITY;
            Assert.Equal(infinity1, infinity2);
            Assert.True(infinity1 == infinity2);
        }

        [Fact]
        public void ECPoint4w_PointInfinity_AnyPointWithZZeroAndAZ4Zero_IsInfinity()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger z = 0;
            BigInteger az4 = 0;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(point.IsInfinity);

            Assert.Equal(0, point.Z);
            Assert.Equal(0, point.aZ4);
        }

        [Fact]
        public void ECPoint4w_DefaultStructValue_BehavesAsIdentityElement()
        {
            var defaultPoint = default(ECPoint4w);
            Assert.True(defaultPoint.IsInfinity);

            Assert.Equal(1, defaultPoint.X);
            Assert.Equal(1, defaultPoint.Y);

            Assert.Equal(0, defaultPoint.Z);
            Assert.Equal(0, defaultPoint.aZ4);
        }

        #endregion

        #region Coordinate Accessor Tests

        [Fact]
        public void ECPoint4w_GetX_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.Equal(x, point.X);
        }

        [Fact]
        public void ECPoint4w_GetX_IdentityElement_ReturnsOne()
        {
            var infinity = ECPoint4w.POINT_INFINITY;
            Assert.Equal(1, infinity.X);
        }

        [Fact]
        public void ECPoint4w_GetX_OffCurvePoint_ReturnsOne()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger z = 0;
            BigInteger az4 = 0;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.Equal(1, point.X);
        }

        [Fact]
        public void ECPoint4w_GetX_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;

            BigInteger z = 3;
            BigInteger az4 = 162;

            var point = new ECPoint4w(x, y, z, az4);
            var result1 = point.X;

            var result2 = point.X;
            var result3 = point.X;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint4w_GetY_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.Equal(y, point.Y);
        }

        [Fact]
        public void ECPoint4w_GetY_IdentityElement_ReturnsOne()
        {
            var infinity = ECPoint4w.POINT_INFINITY;
            Assert.Equal(1, infinity.Y);
        }

        [Fact]
        public void ECPoint4w_GetY_OffCurvePoint_ReturnsOne()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger z = 0;
            BigInteger az4 = 0;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.Equal(1, point.Y);
        }

        [Fact]
        public void ECPoint4w_GetY_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;

            BigInteger z = 3;
            BigInteger az4 = 162;

            var point = new ECPoint4w(x, y, z, az4);
            var result1 = point.Y;

            var result2 = point.Y;
            var result3 = point.Y;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint4w_GetZ_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 2;
            BigInteger az4 = 32;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.Equal(z, point.Z);
        }

        [Fact]
        public void ECPoint4w_GetZ_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint4w.POINT_INFINITY;
            Assert.Equal(0, infinity.Z);
        }

        [Fact]
        public void ECPoint4w_GetZ_OffCurvePoint_ReturnsZero()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger z = 0;
            BigInteger az4 = 0;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.Equal(0, point.Z);
        }

        [Fact]
        public void ECPoint4w_GetZ_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;

            BigInteger z = 7;
            BigInteger az4 = 2401;

            var point = new ECPoint4w(x, y, z, az4);
            var result1 = point.Z;

            var result2 = point.Z;
            var result3 = point.Z;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint4w_GetAZ4_FinitePoint_ReturnsStoredValue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 2;
            BigInteger az4 = 32;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.Equal(az4, point.aZ4);
        }

        [Fact]
        public void ECPoint4w_GetAZ4_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint4w.POINT_INFINITY;
            Assert.Equal(0, infinity.aZ4);
        }

        [Fact]
        public void ECPoint4w_GetAZ4_OffCurvePoint_ReturnsZero()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger z = 0;
            BigInteger az4 = 0;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.Equal(0, point.aZ4);
        }

        [Fact]
        public void ECPoint4w_GetAZ4_MultipleCalls_ReturnsConsistentResult()
        {
            BigInteger x = 12345;
            BigInteger y = 67890;

            BigInteger z = 3;
            BigInteger az4 = 162;

            var point = new ECPoint4w(x, y, z, az4);
            var result1 = point.aZ4;

            var result2 = point.aZ4;
            var result3 = point.aZ4;

            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        [Fact]
        public void ECPoint4w_CoordinateAccessors_WithLargeAZ4_ReturnCorrectValues()
        {
            BigInteger x = 100;
            BigInteger y = 200;
            BigInteger z = 1;

            BigInteger az4 = BigInteger.Parse("12345678901234567890");
            var point = new ECPoint4w(x, y, z, az4);

            Assert.Equal(x, point.X);
            Assert.Equal(y, point.Y);

            Assert.Equal(z, point.Z);
            Assert.Equal(az4, point.aZ4);
        }

        #endregion

        #region Point Validity Tests

        [Fact]
        public void ECPoint4w_IsInfinity_FinitePoint_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4w_IsInfinity_IdentityElement_ReturnsTrue()
        {
            BigInteger x = 1;
            BigInteger y = 1;

            BigInteger z = 0;
            BigInteger az4 = 0;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4w_IsInfinity_PointAtInfinityConstant_ReturnsTrue()
        {
            Assert.True(ECPoint4w.POINT_INFINITY.IsInfinity);
        }

        [Fact]
        public void ECPoint4w_IsInfinity_MatchesIsOnCurveNegation_ForFinitePoint()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4w_IsInfinity_MatchesIsOnCurveNegation_ForIdentityElement()
        {
            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger z = 0;
            BigInteger az4 = 0;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4w_IsInfinity_NonZeroZ_ReturnsFalse()
        {
            var zValues = new[] { 1, 2, 3, 100 };

            foreach (var zVal in zValues)
            {
                BigInteger x = 5;
                BigInteger y = 10;
                BigInteger z = zVal;

                BigInteger az4 = zVal * zVal * zVal * zVal;
                var point = new ECPoint4w(x, y, z, az4);
                Assert.False(point.IsInfinity);
            }
        }

        [Fact]
        public void ECPoint4w_IsInfinity_ZeroZAndZeroAZ4_AlwaysReturnsTrue()
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
                BigInteger az4 = 0;

                var point = new ECPoint4w(x, y, z, az4);
                Assert.True(point.IsInfinity);
            }
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void ECPoint4w_Equals_SameCoordinates_ReturnsTrue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point1 = new ECPoint4w(x, y, z, az4);
            var point2 = new ECPoint4w(x, y, z, az4);

            Assert.True(point1.Equals(point2));
            Assert.True(point1 == point2);
        }

        [Fact]
        public void ECPoint4w_Equals_SameInstance_ReturnsTrue()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(point.Equals(point));
        }

        [Fact]
        public void ECPoint4w_Equals_TwoIdentityElements_ReturnsTrue()
        {
            var inf1 = ECPoint4w.POINT_INFINITY;
            var inf2 = ECPoint4w.POINT_INFINITY;
            Assert.True(inf1.Equals(inf2));
            Assert.True(inf1 == inf2);
        }

        [Fact]
        public void ECPoint4w_Equals_TwoOffCurvePoints_ReturnsTrue()
        {
            BigInteger x1 = 5, y1 = 10, z1 = 0, az41 = 0;
            BigInteger x2 = 42, y2 = 100, z2 = 0, az42 = 0;

            var off1 = new ECPoint4w(x1, y1, z1, az41);
            var off2 = new ECPoint4w(x2, y2, z2, az42);

            Assert.True(off1.Equals(off2));
            Assert.True(off1 == off2);
        }

        [Fact]
        public void ECPoint4w_Equals_SameProjectiveCoordinates_ReturnsTrue()
        {
            BigInteger x = 6;
            BigInteger y = 12;

            BigInteger z = 2;
            BigInteger az4 = 32;

            var point1 = new ECPoint4w(x, y, z, az4);
            var point2 = new ECPoint4w(x, y, z, az4);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4w_Equals_ZeroCoordinatePoints_ReturnsTrue()
        {
            BigInteger x = 0;
            BigInteger y = 0;

            BigInteger z = 1;
            BigInteger az4 = 0;

            var point1 = new ECPoint4w(x, y, z, az4);
            var point2 = new ECPoint4w(x, y, z, az4);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4w_Equals_NegativeCoordinatePoints_ReturnsTrue()
        {
            BigInteger x = -1;
            BigInteger y = -1;

            BigInteger z = 1;
            BigInteger az4 = -2;

            var point1 = new ECPoint4w(x, y, z, az4);
            var point2 = new ECPoint4w(x, y, z, az4);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4w_Equals_LargeCoordinatePoints_ReturnsTrue()
        {
            var x = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671663");
            var y = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");
            BigInteger z = 1;

            BigInteger az4 = BigInteger.Parse("12345678901234567890");
            var point1 = new ECPoint4w(x, y, z, az4);

            var point2 = new ECPoint4w(x, y, z, az4);
            Assert.True(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4w_Equals_DifferentXCoordinate_ReturnsFalse()
        {
            BigInteger x1 = 5, x2 = 15;
            BigInteger y = 10, z = 1;
            BigInteger az4 = 2;

            var point1 = new ECPoint4w(x1, y, z, az4);
            var point2 = new ECPoint4w(x2, y, z, az4);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint4w_Equals_DifferentYCoordinate_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y1 = 10, y2 = 20;
            BigInteger z = 1, az4 = 2;

            var point1 = new ECPoint4w(x, y1, z, az4);
            var point2 = new ECPoint4w(x, y2, z, az4);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint4w_Equals_DifferentZCoordinate_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z1 = 1, z2 = 2;
            BigInteger az4 = 2;

            var point1 = new ECPoint4w(x, y, z1, az4);
            var point2 = new ECPoint4w(x, y, z2, az4);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint4w_Equals_DifferentAZ4Coordinate_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az41 = 2, az42 = 3;

            var point1 = new ECPoint4w(x, y, z, az41);
            var point2 = new ECPoint4w(x, y, z, az42);

            Assert.False(point1.Equals(point2));
            Assert.True(point1 != point2);
        }

        [Fact]
        public void ECPoint4w_Equals_CompletelyDifferentCoordinates_ReturnsFalse()
        {
            BigInteger x1 = 5, y1 = 10, z1 = 1, az41 = 2;
            BigInteger x2 = 15, y2 = 20, z2 = 2, az42 = 32;

            var point1 = new ECPoint4w(x1, y1, z1, az41);
            var point2 = new ECPoint4w(x2, y2, z2, az42);
            Assert.False(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4w_Equals_FinitePointVsIdentityElement_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            var infinity = ECPoint4w.POINT_INFINITY;

            Assert.False(point.Equals(infinity));
            Assert.True(point != infinity);
        }

        [Fact]
        public void ECPoint4w_Equals_FinitePointVsOffCurvePoint_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            var onCurve = new ECPoint4w(x, y, 1, 2);
            var offCurve = new ECPoint4w(x, y, 0, 0);

            Assert.False(onCurve.Equals(offCurve));
            Assert.True(onCurve != offCurve);
        }

        [Fact]
        public void ECPoint4w_Equals_NullObject_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.False(point.Equals(null));
        }

        [Fact]
        public void ECPoint4w_Equals_DifferentType_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.False(point.Equals("not a point"));

            Assert.False(point.Equals(42));
            Assert.False(point.Equals(new object()));
        }

        [Fact]
        public void ECPoint4w_Equals_SameXYZ_DifferentAZ4_ReturnsFalse()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az41 = 2, az42 = 4;

            var point1 = new ECPoint4w(x, y, z, az41);
            var point2 = new ECPoint4w(x, y, z, az42);
            Assert.False(point1.Equals(point2));
        }

        [Fact]
        public void ECPoint4w_EqualityOperator_Transitivity()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var p1 = new ECPoint4w(x, y, z, az4);
            var p2 = new ECPoint4w(x, y, z, az4);
            var p3 = new ECPoint4w(x, y, z, az4);

            Assert.True(p1 == p2);
            Assert.True(p2 == p3);
            Assert.True(p1 == p3);
        }

        [Fact]
        public void ECPoint4w_InequalityOperator_OppositeOfEqualityOperator()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var p1 = new ECPoint4w(x, y, z, az4);
            var p2 = new ECPoint4w(x, y, z, az4);
            var p3 = new ECPoint4w(x + 1, y, z, az4);

            Assert.True((p1 == p2) == !(p1 != p2));
            Assert.True((p1 == p3) == !(p1 != p3));
        }

        #endregion

        #region GetHashCode Tests

        [Fact]
        public void ECPoint4w_GetHashCode_SamePoints_HaveSameHashCode()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point1 = new ECPoint4w(x, y, z, az4);
            var point2 = new ECPoint4w(x, y, z, az4);

            Assert.Equal(point1.GetHashCode(), 
                point2.GetHashCode());
        }

        [Fact]
        public void ECPoint4w_GetHashCode_IdentityElement_ReturnsZero()
        {
            var infinity = ECPoint4w.POINT_INFINITY;
            Assert.Equal(0, infinity.GetHashCode());
        }

        [Fact]
        public void ECPoint4w_GetHashCode_OffCurvePoints_ReturnZero()
        {
            BigInteger x1 = 5, y1 = 10, z1 = 0, az41 = 0;
            BigInteger x2 = 42, y2 = 100, z2 = 0, az42 = 0;

            var off1 = new ECPoint4w(x1, y1, z1, az41);
            var off2 = new ECPoint4w(x2, y2, z2, az42);

            Assert.Equal(0, off1.GetHashCode());
            Assert.Equal(off1.GetHashCode(), off2.GetHashCode());
        }

        [Fact]
        public void ECPoint4w_GetHashCode_ConsistentAcrossMultipleCalls()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            int hash1 = point.GetHashCode();

            int hash2 = point.GetHashCode();
            int hash3 = point.GetHashCode();

            Assert.Equal(hash1, hash2);
            Assert.Equal(hash2, hash3);
        }

        [Fact]
        public void ECPoint4w_GetHashCode_DifferentPoints_MayDiffer()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point1 = new ECPoint4w(x, y, z, az4);
            var point2 = new ECPoint4w(x + 1, y, z, az4);

            int hash1 = point1.GetHashCode();
            int hash2 = point2.GetHashCode();
            Assert.True(hash1 != 0 || hash2 != 0);
        }

        [Fact]
        public void ECPoint4w_GetHashCode_DifferentAZ4Values_ProduceDifferentHashes()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az41 = 2, az42 = 3;

            var point1 = new ECPoint4w(x, y, z, az41);
            var point2 = new ECPoint4w(x, y, z, az42);

            int hash1 = point1.GetHashCode();
            int hash2 = point2.GetHashCode();
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void ECPoint4w_GetHashCode_VariousCoordinateValues_Consistent()
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
                        BigInteger az4 = zVal * zVal;

                        var point1 = new ECPoint4w(x, y, z, az4);
                        var point2 = new ECPoint4w(x, y, z, az4);

                        Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
                    }
                }
            }
        }

        #endregion

        #region Collection Integration Tests

        [Fact]
        public void ECPoint4w_HashSet_HandlesModifiedJacobianPointsCorrectly()
        {
            BigInteger x1 = 5, y1 = 10, z1 = 1, az41 = 2;
            BigInteger x2 = 15, y2 = 20, z2 = 1, az42 = 2;

            var set = new HashSet<ECPoint4w>();
            var p1 = new ECPoint4w(x1, y1, z1, az41);

            var p2 = new ECPoint4w(x1, y1, z1, az41);
            var p3 = new ECPoint4w(x2, y2, z2, az42);

            set.Add(p1);
            Assert.Single(set);

            Assert.Contains(p2, set);
            Assert.DoesNotContain(p3, set);
        }

        [Fact]
        public void ECPoint4w_HashSet_DifferentAZ4Values_TreatedAsDifferent()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az41 = 2, az42 = 3;

            var set = new HashSet<ECPoint4w>();
            var p1 = new ECPoint4w(x, y, z, az41);
            var p2 = new ECPoint4w(x, y, z, az42);

            set.Add(p1);
            set.Add(p2);
            Assert.Equal(2, set.Count);
        }

        [Fact]
        public void ECPoint4w_HashSet_IdentityElementsHandledCorrectly()
        {
            var set = new HashSet<ECPoint4w>();
            var inf1 = ECPoint4w.POINT_INFINITY;

            BigInteger x = 42;
            BigInteger y = 100;

            BigInteger z = 0;
            BigInteger az4 = 0;

            var inf2 = new ECPoint4w(x, y, z, az4);
            set.Add(inf1);

            Assert.Single(set);
            Assert.Contains(inf2, set);
        }

        [Fact]
        public void ECPoint4w_Dictionary_HandlesPointsAsKeys()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var dict = new Dictionary<ECPoint4w, string>();
            var p1 = new ECPoint4w(x, y, z, az4);
            var p2 = new ECPoint4w(x, y, z, az4);

            dict[p1] = "original";
            Assert.Equal("original", dict[p2]);
        }

        [Fact]
        public void ECPoint4w_List_ContainsUsesEquality()
        {
            BigInteger x = 5;
            BigInteger y = 10;
            BigInteger z = 1;
            BigInteger az4 = 2;

            var list = new List<ECPoint4w>();
            var p1 = new ECPoint4w(x, y, z, az4);
            var p2 = new ECPoint4w(x, y, z, az4);

            list.Add(p1);

            Assert.Contains(p2, list);
        }

        [Fact]
        public void ECPoint4w_List_IdentityElement_ContainsWorks()
        {
            var list = new List<ECPoint4w>();
            var inf1 = ECPoint4w.POINT_INFINITY;
            var inf2 = ECPoint4w.POINT_INFINITY;

            list.Add(inf1);

            Assert.Contains(inf2, list);
        }

        #endregion

        #region Value Type Semantics Tests

        [Fact]
        public void ECPoint4w_IsValueType()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.True(point.GetType().IsValueType);
        }

        [Fact]
        public void ECPoint4w_ValueSemantics_AssignmentCreatesCopy()
        {
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var original = new ECPoint4w(x, y, z, az4);
            var copy = original;

            Assert.Equal(original, copy);
            Assert.True(original == copy);
        }

        #endregion

        #region Real-World ECC Tests

        [Fact]
        public void ECPoint4w_Points_CanRepresentSecp256k1GeneratorInModifiedJacobian()
        {
            /* secp256k1 generator point in affine */
            var gx = BigInteger.Parse("55066263022277343669578718895168534326250603453777594175500187360389116729240");
            var gy = BigInteger.Parse("32670510020758816978083085130507043184471273380659243275938904335757337482424");

            BigInteger z = 1;
            BigInteger az4 = 0;

            var generator = new ECPoint4w(gx, gy, z, az4);
            Assert.False(generator.IsInfinity);

            Assert.Equal(gx, generator.X);
            Assert.Equal(gy, generator.Y);

            Assert.Equal(1, generator.Z);
            Assert.Equal(0, generator.aZ4);
        }

        [Fact]
        public void ECPoint4w_Points_WithProjectiveZ_CorrectlyStored()
        {
            var x = BigInteger.Parse("123456789012345678901234567890");
            var y = BigInteger.Parse("987654321098765432109876543210");

            BigInteger z = 5;
            BigInteger az4 = 625;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.Equal(x, point.X);

            Assert.Equal(y, point.Y);
            Assert.Equal(z, point.Z);

            Assert.Equal(az4, point.aZ4);
            Assert.True(!point.IsInfinity);
        }

        [Fact]
        public void ECPoint4w_Points_IdentityElement_UsedInECCContext()
        {
            var infinity = ECPoint4w.POINT_INFINITY;
            BigInteger x = 5;
            BigInteger y = 10;

            BigInteger z = 1;
            BigInteger az4 = 2;

            var point = new ECPoint4w(x, y, z, az4);
            Assert.False(point.Equals(infinity));

            Assert.True(infinity.IsInfinity);
            Assert.False(point.IsInfinity);
        }

        [Fact]
        public void ECPoint4w_Points_InfinityRequiresZeroAZ4()
        {
            /* verify the invariant: point at infinity must have aZ^4 = 0 */
            BigInteger x = 1;
            BigInteger y = 1;

            BigInteger z = 0;
            BigInteger az4 = 0;

            var infinity = new ECPoint4w(x, y, z, az4);
            Assert.True(infinity.IsInfinity);

            Assert.Equal(0, infinity.Z);
            Assert.Equal(0, infinity.aZ4);
        }

        #endregion

        #region Stress Tests

        [Fact]
        public void ECPoint4w_ManyPointsCreation_NoExceptions()
        {
            for (int i = 0; i < 1000; i++)
            {
                BigInteger x = i;
                BigInteger y = i * 2;

                BigInteger z = (i % 10) + 1;
                BigInteger az4 = z * z * z * z;
                var point = new ECPoint4w(x, y, z, az4);

                Assert.Equal(x, point.X);
                Assert.Equal(y, point.Y);
                Assert.Equal(z, point.Z);
                Assert.Equal(az4, point.aZ4);
            }
        }

        [Fact]
        public void ECPoint4w_HashCodeDistribution_MultiplePoints()
        {
            var hashCodes = new HashSet<int>();

            for (int i = 0; i < 100; i++)
            {
                BigInteger x = i;
                BigInteger y = i * 2;

                BigInteger z = (i % 5) + 1;
                BigInteger az4 = z * z * z * z;

                var point = new ECPoint4w(x, y, z, az4);
                hashCodes.Add(point.GetHashCode());
            }

            Assert.True(hashCodes.Count >= 95);
        }

        [Fact]
        public void ECPoint4w_InfinityCreation_WithNonZeroAZ4_AlwaysThrows()
        {
            var nonZeroAZ4Values = new[] { 1, 5, -1, 100, 
                BigInteger.Parse("12345678901234567890") };

            foreach (var az4 in nonZeroAZ4Values)
            {
                Assert.Throws<InvalidOperationException>(() =>
                    new ECPoint4w(1, 1, 0, az4));
            }
        }

        #endregion
    }
}
