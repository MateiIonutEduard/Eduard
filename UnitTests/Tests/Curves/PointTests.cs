using System;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
    [Collection("Sequential")]
    public class PointTests
    {
        #region Affine Point Tests (ECPoint)

        [Fact]
        public void ECPoint_ConstructorAndEquality_HandlesAllCases()
        {
            /* normal point construction */
            var point1 = new ECPoint(5, 7);
            Assert.Equal(5, point1.GetAffineX());
            Assert.Equal(7, point1.GetAffineY());
            Assert.False(point1 == ECPoint.POINT_INFINITY);

            /* point at infinity via static property */
            var infinity = ECPoint.POINT_INFINITY;
            Assert.Equal(0, infinity.GetAffineX());
            Assert.Equal(1, infinity.GetAffineY());

            /* point at infinity via constructor with flag */
            var infinity2 = new ECPoint(0, 1, true);
            Assert.Equal(infinity, infinity2);

            /* equality: same coordinates */
            var point2 = new ECPoint(5, 7);
            Assert.Equal(point1, point2);
            Assert.True(point1 == point2);

            /* inequality: different coordinates */
            var point3 = new ECPoint(5, 8);
            Assert.NotEqual(point1, point3);
            Assert.True(point1 != point3);

            /* inequality: infinity vs finite */
            Assert.NotEqual(infinity, point1);
            Assert.True(infinity != point1);

            /* hash code consistency */
            Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
            Assert.NotEqual(point1.GetHashCode(), point3.GetHashCode());
            Assert.Equal(infinity.GetHashCode(), infinity2.GetHashCode());

            /* null comparison */
            Assert.False(point1.Equals(null));
            Assert.False(point1.Equals("not a point"));
        }

        #endregion

        #region Weierstrass Jacobian Tests (ECPoint3w)

        [Fact]
        public void ECPoint3w_ConstructorAndEquality_HandlesAllCases()
        {
            /* normal point construction */
            var point1 = new ECPoint3w(10, 20, 1);
            Assert.Equal(10, point1.x);
            Assert.Equal(20, point1.y);
            Assert.Equal(1, point1.z);

            /* point at infinity via static property */
            var infinity = ECPoint3w.POINT_INFINITY;
            Assert.Equal(1, infinity.x);
            Assert.Equal(1, infinity.y);
            Assert.Equal(0, infinity.z);

            /* point at infinity with arbitrary coordinates (Z=0) */
            var infinityAlt = new ECPoint3w(999, 888, 0);
            Assert.Equal(infinity, infinityAlt);

            /* equality: same coordinates */
            var point2 = new ECPoint3w(10, 20, 1);
            Assert.Equal(point1, point2);
            Assert.True(point1 == point2);

            /* equality: both infinity (different X,Y but Z=0) */
            Assert.Equal(infinity, infinityAlt);
            Assert.True(infinity == infinityAlt);

            /* inequality: different coordinates */
            var point3 = new ECPoint3w(10, 21, 1);
            Assert.NotEqual(point1, point3);
            Assert.True(point1 != point3);

            /* inequality: finite vs infinity */
            Assert.NotEqual(point1, infinity);
            Assert.True(point1 != infinity);

            /* hash code: finite points hash based on coordinates */
            Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
            Assert.NotEqual(point1.GetHashCode(), point3.GetHashCode());

            /* hash code: all infinity points hash to same value */
            Assert.Equal(infinity.GetHashCode(), infinityAlt.GetHashCode());
            Assert.Equal(0, infinity.GetHashCode());

            /* null comparison */
            Assert.False(point1.Equals(null));
            Assert.False(point1.Equals("not a point"));
        }

        #endregion

        #region Weierstrass Modified Jacobian Tests (ECPoint4w)

        [Fact]
        public void ECPoint4w_ConstructorAndEquality_HandlesAllCases()
        {
            /* normal point construction */
            var point1 = new ECPoint4w(10, 20, 5, 100);
            Assert.Equal(10, point1.x);
            Assert.Equal(20, point1.y);
            Assert.Equal(5, point1.z);
            Assert.Equal(100, point1.aZ4);

            /* point at infinity via static property */
            var infinity = ECPoint4w.POINT_INFINITY;
            Assert.Equal(1, infinity.x);
            Assert.Equal(1, infinity.y);
            Assert.Equal(0, infinity.z);
            Assert.Equal(0, infinity.aZ4);

            /* point at infinity with Z=0 and aZ4=0 (valid) */
            var infinityValid = new ECPoint4w(999, 888, 0, 0);
            Assert.Equal(infinity, infinityValid);

            /* invalid: point at infinity with non-zero aZ4 */
            Assert.Throws<InvalidOperationException>(() =>
                new ECPoint4w(1, 1, 0, 5));

            /* equality: same coordinates */
            var point2 = new ECPoint4w(10, 20, 5, 100);
            Assert.Equal(point1, point2);
            Assert.True(point1 == point2);

            /* equality: both infinity (valid representations) */
            Assert.Equal(infinity, infinityValid);
            Assert.True(infinity == infinityValid);

            /* inequality: different coordinates */
            var point3 = new ECPoint4w(10, 21, 5, 100);
            Assert.NotEqual(point1, point3);
            Assert.True(point1 != point3);

            /* inequality: finite vs infinity */
            Assert.NotEqual(point1, infinity);
            Assert.True(point1 != infinity);

            /* hash code: finite points hash based on coordinates */
            Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
            Assert.NotEqual(point1.GetHashCode(), point3.GetHashCode());

            /* hash code: all infinity points hash to same value */
            Assert.Equal(infinity.GetHashCode(), infinityValid.GetHashCode());
            Assert.Equal(0, infinity.GetHashCode());

            /* null comparison */
            Assert.False(point1.Equals(null));
            Assert.False(point1.Equals("not a point"));

            /* invariant violation detection in Equals */
            var invalidInfinity = new ECPoint4w(0, 0, 0, 0);
            invalidInfinity.aZ4 = 5;
            Assert.Throws<InvalidOperationException>(() =>
                invalidInfinity.Equals(infinity));
        }

        #endregion

        #region Weierstrass Jacobian-Chudnovsky Tests (ECPoint5w)

        [Fact]
        public void ECPoint5w_ConstructorAndEquality_HandlesAllCases()
        {
            /* normal point construction */
            var point1 = new ECPoint5w(10, 20, 3, 9, 27);
            Assert.Equal(10, point1.x);
            Assert.Equal(20, point1.y);
            Assert.Equal(3, point1.z);
            Assert.Equal(9, point1.z2);
            Assert.Equal(27, point1.z3);

            /* point at infinity via static property */
            var infinity = ECPoint5w.POINT_INFINITY;
            Assert.Equal(1, infinity.x);
            Assert.Equal(1, infinity.y);
            Assert.Equal(0, infinity.z);
            Assert.Equal(0, infinity.z2);
            Assert.Equal(0, infinity.z3);

            /* point at infinity with Z=0, Z2=0, Z3=0 (valid) */
            var infinityValid = new ECPoint5w(999, 888, 0, 0, 0);
            Assert.Equal(infinity, infinityValid);

            /* invalid: point at infinity with non-zero Z2 */
            Assert.Throws<InvalidOperationException>(() =>
                new ECPoint5w(1, 1, 0, 1, 0));

            /* invalid: point at infinity with non-zero Z3 */
            Assert.Throws<InvalidOperationException>(() =>
                new ECPoint5w(1, 1, 0, 0, 1));

            /* equality: same coordinates */
            var point2 = new ECPoint5w(10, 20, 3, 9, 27);
            Assert.Equal(point1, point2);
            Assert.True(point1 == point2);

            /* equality: both infinity (valid representations) */
            Assert.Equal(infinity, infinityValid);
            Assert.True(infinity == infinityValid);

            /* inequality: different coordinates */
            var point3 = new ECPoint5w(10, 21, 3, 9, 27);
            Assert.NotEqual(point1, point3);
            Assert.True(point1 != point3);

            /* inequality: finite vs infinity */
            Assert.NotEqual(point1, infinity);
            Assert.True(point1 != infinity);

            /* hash code: finite points hash based on coordinates */
            Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
            Assert.NotEqual(point1.GetHashCode(), point3.GetHashCode());

            /* hash code: all infinity points hash to same value */
            Assert.Equal(infinity.GetHashCode(), infinityValid.GetHashCode());
            Assert.Equal(0, infinity.GetHashCode());

            /* null comparison */
            Assert.False(point1.Equals(null));
            Assert.False(point1.Equals("not a point"));

            /* invariant violation detection in Equals */
            var invalidInfinity = new ECPoint5w(0, 0, 0, 0, 0);
            invalidInfinity.z2 = 5;
            Assert.Throws<InvalidOperationException>(() =>
                invalidInfinity.Equals(infinity));
        }

        #endregion

        #region Twisted Edwards Projective Tests (ECPoint3)

        [Fact]
        public void ECPoint3_ConstructorAndEquality_HandlesAllCases()
        {
            /* normal point construction */
            var point1 = new ECPoint3(10, 20, 1);
            Assert.Equal(10, point1.x);
            Assert.Equal(20, point1.y);
            Assert.Equal(1, point1.z);

            /* point at infinity via static property */
            var infinity = ECPoint3.POINT_INFINITY;
            Assert.Equal(0, infinity.x);
            Assert.Equal(1, infinity.y);
            Assert.Equal(0, infinity.z);

            /* point at infinity with arbitrary coordinates (Z=0) */
            var infinityAlt = new ECPoint3(999, 888, 0);
            Assert.Equal(infinity, infinityAlt);

            /* equality: same coordinates */
            var point2 = new ECPoint3(10, 20, 1);
            Assert.Equal(point1, point2);
            Assert.True(point1 == point2);

            /* equality: both infinity (different X,Y but Z=0) */
            Assert.Equal(infinity, infinityAlt);
            Assert.True(infinity == infinityAlt);

            /* inequality: different coordinates */
            var point3 = new ECPoint3(10, 21, 1);
            Assert.NotEqual(point1, point3);
            Assert.True(point1 != point3);

            /* inequality: finite vs infinity */
            Assert.NotEqual(point1, infinity);
            Assert.True(point1 != infinity);

            /* hash code: finite points hash based on coordinates */
            Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
            Assert.NotEqual(point1.GetHashCode(), point3.GetHashCode());

            /* hash code: all infinity points hash to same value */
            Assert.Equal(infinity.GetHashCode(), infinityAlt.GetHashCode());
            Assert.Equal(0, infinity.GetHashCode());

            /* null comparison */
            Assert.False(point1.Equals(null));
            Assert.False(point1.Equals("not a point"));
        }

        #endregion

        #region Twisted Edwards Extended Projective Tests (ECPoint4)

        [Fact]
        public void ECPoint4_ConstructorAndEquality_HandlesAllCases()
        {
            /* normal point construction */
            var point1 = new ECPoint4(10, 20, 200, 1);
            Assert.Equal(10, point1.x);
            Assert.Equal(20, point1.y);
            Assert.Equal(200, point1.t);
            Assert.Equal(1, point1.z);

            /* point at infinity via static property */
            var infinity = ECPoint4.POINT_INFINITY;
            Assert.Equal(0, infinity.x);
            Assert.Equal(1, infinity.y);
            Assert.Equal(0, infinity.t);
            Assert.Equal(0, infinity.z);

            /* point at infinity with Z=0 and T=0 (valid) */
            var infinityValid = new ECPoint4(999, 888, 0, 0);
            Assert.Equal(infinity, infinityValid);

            /* invalid: point at infinity with non-zero T */
            Assert.Throws<InvalidOperationException>(() =>
                new ECPoint4(1, 1, 5, 0));

            /* equality: same coordinates (X,Y,Z only, T is derived) */
            var point2 = new ECPoint4(10, 20, 200, 1);
            Assert.Equal(point1, point2);
            Assert.True(point1 == point2);

            /* points with different T but same X,Y,Z are not equal */
            var pointDifferentT = new ECPoint4(10, 20, 999, 1);
            Assert.NotEqual(point1, pointDifferentT);
            Assert.False(point1 == pointDifferentT);

            /* equality: both infinity (valid representations) */
            Assert.Equal(infinity, infinityValid);
            Assert.True(infinity == infinityValid);

            /* inequality: different coordinates */
            var point3 = new ECPoint4(10, 21, 210, 1);
            Assert.NotEqual(point1, point3);
            Assert.True(point1 != point3);

            /* inequality: finite vs infinity */
            Assert.NotEqual(point1, infinity);
            Assert.True(point1 != infinity);

            /* hash code: finite points hash based on coordinates */
            Assert.Equal(point1.GetHashCode(), point2.GetHashCode());
            Assert.NotEqual(point1.GetHashCode(), point3.GetHashCode());

            /* points with different T have different hash code */
            Assert.NotEqual(point1.GetHashCode(), pointDifferentT.GetHashCode());

            /* hash code: all infinity points hash to same value */
            Assert.Equal(infinity.GetHashCode(), infinityValid.GetHashCode());
            Assert.Equal(0, infinity.GetHashCode());

            /* null comparison */
            Assert.False(point1.Equals(null));
            Assert.False(point1.Equals("not a point"));

            /* invariant violation detection in Equals */
            var invalidInfinity = new ECPoint4(0, 0, 0, 0);
            invalidInfinity.t = 5;
            Assert.Throws<InvalidOperationException>(() =>
                invalidInfinity.Equals(infinity));
        }

        #endregion
    }
}
