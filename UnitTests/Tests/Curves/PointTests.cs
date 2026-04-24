using Eduard;
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
    }
}
