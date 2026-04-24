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
    }
}
