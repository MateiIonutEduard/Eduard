using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
    [Collection("Sequential")]
    public class WeiCurveScalarMultTests
    {
        #region Scalar Multiplication — Core Properties

        [Fact]
        public void ScalarMult_ZeroScalar_ReturnsInfinity()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384,
                WeiCurveType.Wei25519,
                WeiCurveType.Wei448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_SECURE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                for (int j = 0; j < modes.Length; j++)
                {
                    var result = ECMath.Multiply(curve, 0, G, modes[j]);
                    Assert.Equal(ECPoint.POINT_INFINITY, result);
                }
            }
        }

        [Fact]
        public void ScalarMult_NegativeScalar_ThrowsException()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384,
                WeiCurveType.Wei25519,
                WeiCurveType.Wei448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_SECURE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                for (int j = 0; j < modes.Length; j++)
                {
                    Assert.Throws<ArgumentException>(() =>
                        ECMath.Multiply(curve, -1, G, modes[j]));

                    Assert.Throws<ArgumentException>(() =>
                        ECMath.Multiply(curve, -100, G, modes[j]));
                }
            }
        }

        [Fact]
        public void ScalarMult_OrderScalar_ReturnsInfinity()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384,
                WeiCurveType.Wei25519,
                WeiCurveType.Wei448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_SECURE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();
                var order = curve.order;

                for (int j = 0; j < modes.Length; j++)
                {
                    var result = ECMath.Multiply(curve, order, G, modes[j]);
                    Assert.Equal(ECPoint.POINT_INFINITY, result);
                }
            }
        }

        [Fact]
        public void ScalarMult_OneScalar_ReturnsBasePoint()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384,
                WeiCurveType.Wei25519,
                WeiCurveType.Wei448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_SECURE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                for (int j = 0; j < modes.Length; j++)
                {
                    var result = ECMath.Multiply(curve, 1, G, modes[j]);
                    Assert.Equal(G, result);
                }
            }
        }

        [Fact]
        public void ScalarMult_InfinityPoint_ReturnsInfinity()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384,
                WeiCurveType.Wei25519,
                WeiCurveType.Wei448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_SECURE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var inf = ECPoint.POINT_INFINITY;
                var scalar = new BigInteger(12345);

                for (int j = 0; j < modes.Length; j++)
                {
                    var result = ECMath.Multiply(curve, scalar, inf, modes[j]);
                    Assert.Equal(ECPoint.POINT_INFINITY, result);
                }
            }
        }

        #endregion

        #region Scalar Multiplication — Consistency Across Modes

        [Fact]
        public void ScalarMult_AllModesProduceSameResult()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384,
                WeiCurveType.Wei25519,
                WeiCurveType.Wei448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_SECURE,
                ECMode.EC_FASTEST
            };

            int[] scalars = new int[] { 2, 3, 5, 7, 12345 };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                for (int s = 0; s < scalars.Length; s++)
                {
                    var k = new BigInteger(scalars[s]);
                    ECPoint? affineResult = null;

                    for (int m = 0; m < modes.Length; m++)
                    {
                        var result = ECMath.Multiply(curve, k, G, modes[m]);

                        if (modes[m] == ECMode.EC_STANDARD_AFFINE)
                            affineResult = result;

                        Assert.Equal(affineResult, result);
                    }
                }

                /* also test order - 1 */
                var orderMinusOne = curve.order - 1;
                ECPoint? affineOrderResult = null;

                for (int m = 0; m < modes.Length; m++)
                {
                    var result = ECMath.Multiply(curve, orderMinusOne, G, modes[m]);

                    if (modes[m] == ECMode.EC_STANDARD_AFFINE)
                        affineOrderResult = result;

                    Assert.Equal(affineOrderResult, result);
                }

                /* -G should equal order-1 * G */
                var negG = ECMath.Negate(curve, G);
                Assert.Equal(negG, affineOrderResult);
            }
        }

        #endregion

        #region Scalar Multiplication — Algebraic Properties

        [Fact]
        public void ScalarMult_DistributiveProperty()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384,
                WeiCurveType.Wei25519,
                WeiCurveType.Wei448
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                var a = new BigInteger(12345);
                var b = new BigInteger(67890);
                var sum = (a + b) % curve.order;

                var left = ECMath.Multiply(curve, sum, G, ECMode.EC_FASTEST);
                var rightA = ECMath.Multiply(curve, a, G, ECMode.EC_FASTEST);
                var rightB = ECMath.Multiply(curve, b, G, ECMode.EC_FASTEST);
                var right = ECMath.Add(curve, rightA, rightB);

                Assert.Equal(left, right);
            }
        }

        [Fact]
        public void ScalarMult_CommutativeWithNegation()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384,
                WeiCurveType.Wei25519,
                WeiCurveType.Wei448
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                var k = new BigInteger(1234567);
                var negK = curve.order - k;

                var kG = ECMath.Multiply(curve, k, G, ECMode.EC_FASTEST);
                var negKG = ECMath.Multiply(curve, negK, G, ECMode.EC_FASTEST);
                var negated = ECMath.Negate(curve, kG);

                Assert.Equal(negated, negKG);
            }
        }

        [Fact]
        public void ScalarMult_ScalarMultiplicationIsAssociative()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384,
                WeiCurveType.Wei25519,
                WeiCurveType.Wei448
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                var a = new BigInteger(123);
                var b = new BigInteger(456);
                var product = (a * b) % curve.order;

                var left = ECMath.Multiply(curve, product, G, ECMode.EC_FASTEST);
                var aG = ECMath.Multiply(curve, a, G, ECMode.EC_FASTEST);
                var right = ECMath.Multiply(curve, b, aG, ECMode.EC_FASTEST);

                Assert.Equal(left, right);
            }
        }

        #endregion

        #region Security and Edge Cases

        [Fact]
        public void ScalarMult_SecurityCheck_RejectsInvalidPoints()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);

                /* point not on curve */
                var invalidPoint = new ECPoint(123, 456);
                var scalar = new BigInteger(2);

                Assert.Throws<ArgumentException>(() =>
                    ECMath.Multiply(curve, scalar, invalidPoint, ECMode.EC_FASTEST, true));
            }
        }

        [Fact]
        public void ScalarMult_SecurityCheck_AcceptsValidPoints()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();
                var scalar = new BigInteger(2);

                var exception = Record.Exception(() =>
                    ECMath.Multiply(curve, scalar, G, ECMode.EC_FASTEST, true));

                Assert.Null(exception);
            }
        }

        [Fact]
        public void ScalarMult_MaxScalar_HandlesOrderMinusOne()
        {
            WeiCurveType[] curveTypes = new WeiCurveType[]
            {
                WeiCurveType.NistP256,
                WeiCurveType.NistP384,
                WeiCurveType.Wei25519,
                WeiCurveType.Wei448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_SECURE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = EllipticCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();
                var orderMinusOne = curve.order - 1;

                for (int j = 0; j < modes.Length; j++)
                {
                    var result = ECMath.Multiply(curve, orderMinusOne, G, modes[j]);
                    var negG = ECMath.Negate(curve, G);
                    Assert.Equal(negG, result);
                }
            }
        }

        #endregion

        #region Vector Validation (Known Answer Tests)

        [Fact]
        public void ScalarMult_NistP256_KnownVectors()
        {
            /* NIST P-256 known answer test vectors */
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            /* test vector 1: k = 2 */
            var k1 = new BigInteger(2);
            var expected1 = ECMath.Add(curve, G, G);
            var result1 = ECMath.Multiply(curve, k1, G, ECMode.EC_FASTEST);
            Assert.Equal(expected1, result1);

            /* test vector 2: k = 3 */
            var k2 = new BigInteger(3);
            var expected2 = ECMath.Add(curve, expected1, G);
            var result2 = ECMath.Multiply(curve, k2, G, ECMode.EC_FASTEST);
            Assert.Equal(expected2, result2);

            /* test vector 3: k = order - 1 */
            var k3 = curve.order - 1;
            var expected3 = ECMath.Negate(curve, G);
            var result3 = ECMath.Multiply(curve, k3, G, ECMode.EC_FASTEST);
            Assert.Equal(expected3, result3);
        }

        #endregion

        #region Curve-Specific Tests

        [Fact]
        public void ScalarMult_Wei25519_OrderValidation()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei25519);
            var G = curve.GetBasePoint();

            /* Wei25519 is the Weierstrass form of Curve25519 */
            var order = curve.order;
            var doubleOrder = ECMath.Multiply(curve, order, G);
            Assert.Equal(ECPoint.POINT_INFINITY, doubleOrder);

            /* 2G should not be infinity */
            var twoG = ECMath.Multiply(curve, 2, G);
            Assert.NotEqual(ECPoint.POINT_INFINITY, twoG);
        }

        [Fact]
        public void ScalarMult_Wei448_OrderValidation()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.Wei448);
            var G = curve.GetBasePoint();

            /* Wei448 is the Weierstrass form of Curve448 */
            var order = curve.order;
            var doubleOrder = ECMath.Multiply(curve, order, G);
            Assert.Equal(ECPoint.POINT_INFINITY, doubleOrder);

            /* 2G should not be infinity */
            var twoG = ECMath.Multiply(curve, 2, G);
            Assert.NotEqual(ECPoint.POINT_INFINITY, twoG);
        }

        [Fact]
        public void ScalarMult_NistP256_OrderValidation()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP256);
            var G = curve.GetBasePoint();

            var order = curve.order;
            var result = ECMath.Multiply(curve, order, G);
            Assert.Equal(ECPoint.POINT_INFINITY, result);
        }

        [Fact]
        public void ScalarMult_NistP384_OrderValidation()
        {
            var curve = EllipticCurve.GetNamedCurve(WeiCurveType.NistP384);
            var G = curve.GetBasePoint();

            var order = curve.order;
            var result = ECMath.Multiply(curve, order, G);
            Assert.Equal(ECPoint.POINT_INFINITY, result);
        }

        #endregion
    }
}
