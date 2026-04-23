using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
    [Collection("Sequential")]
    public class TwistedEdwardsCurveScalarMultTests
    {
        #region Scalar Multiplication — Core Properties

        [Fact]
        public void Multiply_ZeroScalar_ReturnsInfinity()
        {
            TwistedEdwardsCurveType[] curveTypes = new TwistedEdwardsCurveType[]
            {
                TwistedEdwardsCurveType.Edwards25519,
                TwistedEdwardsCurveType.Edwards448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = TwistedEdwardsCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                for (int j = 0; j < modes.Length; j++)
                {
                    var result = TwistedEdwardsMath.Multiply(curve, 0, G, modes[j]);
                    Assert.Equal(ECPoint.POINT_INFINITY, result);
                }
            }
        }

        [Fact]
        public void Multiply_OneScalar_ReturnsBasePoint()
        {
            TwistedEdwardsCurveType[] curveTypes = new TwistedEdwardsCurveType[]
            {
                TwistedEdwardsCurveType.Edwards25519,
                TwistedEdwardsCurveType.Edwards448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = TwistedEdwardsCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                for (int j = 0; j < modes.Length; j++)
                {
                    var result = TwistedEdwardsMath.Multiply(curve, 1, G, modes[j]);
                    Assert.Equal(G, result);
                }
            }
        }

        [Fact]
        public void Multiply_OrderScalar_ReturnsInfinity()
        {
            TwistedEdwardsCurveType[] curveTypes = new TwistedEdwardsCurveType[]
            {
                TwistedEdwardsCurveType.Edwards25519,
                TwistedEdwardsCurveType.Edwards448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = TwistedEdwardsCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();
                var order = curve.order;

                for (int j = 0; j < modes.Length; j++)
                {
                    var result = TwistedEdwardsMath.Multiply(curve, order, G, modes[j]);
                    Assert.Equal(ECPoint.POINT_INFINITY, result);
                }
            }
        }

        [Fact]
        public void Multiply_InfinityPoint_ReturnsInfinity()
        {
            TwistedEdwardsCurveType[] curveTypes = new TwistedEdwardsCurveType[]
            {
                TwistedEdwardsCurveType.Edwards25519,
                TwistedEdwardsCurveType.Edwards448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = TwistedEdwardsCurve.GetNamedCurve(curveTypes[i]);
                var inf = ECPoint.POINT_INFINITY;
                var scalar = new BigInteger(12345);

                for (int j = 0; j < modes.Length; j++)
                {
                    var result = TwistedEdwardsMath.Multiply(curve, scalar, inf, modes[j]);
                    Assert.Equal(ECPoint.POINT_INFINITY, result);
                }
            }
        }

        #endregion

        #region Scalar Multiplication — Consistency Across Modes

        [Fact]
        public void Multiply_AllModesProduceSameResult_Edwards25519()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);

            int[] scalars = new int[] { 2, 3, 5, 7, 12345 };
            var G = curve.GetBasePoint();

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_FASTEST
            };

            for (int s = 0; s < scalars.Length; s++)
            {
                var k = new BigInteger(scalars[s]);
                ECPoint affineResult = ECPoint.POINT_INFINITY;

                for (int m = 0; m < modes.Length; m++)
                {
                    var result = TwistedEdwardsMath.Multiply(curve, k, G, modes[m]);

                    if (modes[m] == ECMode.EC_STANDARD_AFFINE)
                        affineResult = result;

                    Assert.Equal(affineResult, result);
                }
            }

            /* test order - 1 */
            var orderMinusOne = curve.order - 1;
            ECPoint affineOrderResult = ECPoint.POINT_INFINITY;

            for (int m = 0; m < modes.Length; m++)
            {
                var result = TwistedEdwardsMath.Multiply(curve, orderMinusOne, G, modes[m]);

                if (modes[m] == ECMode.EC_STANDARD_AFFINE)
                    affineOrderResult = result;

                Assert.Equal(affineOrderResult, result);
            }

            /* -G should equal order-1 * G */
            var negG = TwistedEdwardsMath.Negate(curve, G);
            Assert.Equal(negG, affineOrderResult);
        }

        [Fact]
        public void Multiply_AllModesProduceSameResult_Edwards448()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards448);

            int[] scalars = new int[] { 2, 3, 5, 7, 12345 };
            var G = curve.GetBasePoint();

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_FASTEST
            };

            for (int s = 0; s < scalars.Length; s++)
            {
                var k = new BigInteger(scalars[s]);
                ECPoint affineResult = ECPoint.POINT_INFINITY;

                for (int m = 0; m < modes.Length; m++)
                {
                    var result = TwistedEdwardsMath.Multiply(curve, k, G, modes[m]);

                    if (modes[m] == ECMode.EC_STANDARD_AFFINE)
                        affineResult = result;

                    Assert.Equal(affineResult, result);
                }
            }

            var orderMinusOne = curve.order - 1;
            ECPoint affineOrderResult = ECPoint.POINT_INFINITY;

            for (int m = 0; m < modes.Length; m++)
            {
                var result = TwistedEdwardsMath.Multiply(curve, 
                    orderMinusOne, G, modes[m]);

                if (modes[m] == ECMode.EC_STANDARD_AFFINE)
                    affineOrderResult = result;

                Assert.Equal(affineOrderResult, result);
            }

            var negG = TwistedEdwardsMath.Negate(curve, G);
            Assert.Equal(negG, affineOrderResult);
        }

        #endregion

        #region Scalar Multiplication — Algebraic Properties

        [Fact]
        public void Multiply_DistributiveProperty_Edwards25519()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);

            var G = curve.GetBasePoint();
            var a = new BigInteger(12345);
            var b = new BigInteger(67890);
            var sum = (a + b) % curve.order;

            var left = TwistedEdwardsMath.Multiply(curve, 
                sum, G, ECMode.EC_FASTEST);
            var rightA = TwistedEdwardsMath.Multiply(curve, 
                a, G, ECMode.EC_FASTEST);
            var rightB = TwistedEdwardsMath.Multiply(curve, 
                b, G, ECMode.EC_FASTEST);

            var right = TwistedEdwardsMath.Add(curve, rightA, rightB);
            Assert.Equal(left, right);
        }

        [Fact]
        public void Multiply_DistributiveProperty_Edwards448()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards448);

            var G = curve.GetBasePoint();
            var a = new BigInteger(12345);
            var b = new BigInteger(67890);
            var sum = (a + b) % curve.order;

            var left = TwistedEdwardsMath.Multiply(curve, 
                sum, G, ECMode.EC_FASTEST);
            var rightA = TwistedEdwardsMath.Multiply(curve, 
                a, G, ECMode.EC_FASTEST);
            var rightB = TwistedEdwardsMath.Multiply(curve, 
                b, G, ECMode.EC_FASTEST);

            var right = TwistedEdwardsMath.Add(curve, rightA, rightB);
            Assert.Equal(left, right);
        }

        [Fact]
        public void Multiply_CommutativeWithNegation_Edwards25519()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);
            var G = curve.GetBasePoint();

            var k = new BigInteger(1234567);
            var negK = curve.order - k;

            var kG = TwistedEdwardsMath.Multiply(curve, 
                k, G, ECMode.EC_FASTEST);
            var negKG = TwistedEdwardsMath.Multiply(curve, 
                negK, G, ECMode.EC_FASTEST);

            var negated = TwistedEdwardsMath.Negate(curve, kG);
            Assert.Equal(negated, negKG);
        }

        [Fact]
        public void Multiply_CommutativeWithNegation_Edwards448()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards448);
            var G = curve.GetBasePoint();

            var k = new BigInteger(1234567);
            var negK = curve.order - k;

            var kG = TwistedEdwardsMath.Multiply(curve, 
                k, G, ECMode.EC_FASTEST);
            var negKG = TwistedEdwardsMath.Multiply(curve, 
                negK, G, ECMode.EC_FASTEST);

            var negated = TwistedEdwardsMath.Negate(curve, kG);
            Assert.Equal(negated, negKG);
        }

        [Fact]
        public void Multiply_ScalarMultiplicationIsAssociative_Edwards25519()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards25519);

            var G = curve.GetBasePoint();
            var a = new BigInteger(123);

            var b = new BigInteger(456);
            var product = (a * b) % curve.order;

            var left = TwistedEdwardsMath.Multiply(curve, 
                product, G, ECMode.EC_FASTEST);

            var aG = TwistedEdwardsMath.Multiply(curve, 
                a, G, ECMode.EC_FASTEST);

            var right = TwistedEdwardsMath.Multiply(curve, 
                b, aG, ECMode.EC_FASTEST);

            Assert.Equal(left, right);
        }

        [Fact]
        public void Multiply_ScalarMultiplicationIsAssociative_Edwards448()
        {
            var curve = TwistedEdwardsCurve.GetNamedCurve(
                TwistedEdwardsCurveType.Edwards448);

            var G = curve.GetBasePoint();
            var a = new BigInteger(123);

            var b = new BigInteger(456);
            var product = (a * b) % curve.order;

            var left = TwistedEdwardsMath.Multiply(curve, 
                product, G, ECMode.EC_FASTEST);

            var aG = TwistedEdwardsMath.Multiply(curve, 
                a, G, ECMode.EC_FASTEST);

            var right = TwistedEdwardsMath.Multiply(curve, 
                b, aG, ECMode.EC_FASTEST);
            Assert.Equal(left, right);
        }

        #endregion

        #region Scalar Multiplication — Edge Cases

        [Fact]
        public void Multiply_ScalarTwo_ReturnsDoubleBasePoint()
        {
            TwistedEdwardsCurveType[] curveTypes = new TwistedEdwardsCurveType[]
            {
                TwistedEdwardsCurveType.Edwards25519,
                TwistedEdwardsCurveType.Edwards448
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = TwistedEdwardsCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                var result = TwistedEdwardsMath.Multiply(
                    curve, 2, G, ECMode.EC_FASTEST);
                var expected = TwistedEdwardsMath.Add(curve, G, G);

                Assert.Equal(expected, result);
            }
        }

        [Fact]
        public void Multiply_ScalarThree_ReturnsTripleBasePoint()
        {
            TwistedEdwardsCurveType[] curveTypes = new TwistedEdwardsCurveType[]
            {
                TwistedEdwardsCurveType.Edwards25519,
                TwistedEdwardsCurveType.Edwards448
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = TwistedEdwardsCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();

                var result = TwistedEdwardsMath.Multiply(
                    curve, 3, G, ECMode.EC_FASTEST);
                var doubleG = TwistedEdwardsMath.Add(curve, G, G);

                var expected = TwistedEdwardsMath.Add(curve, doubleG, G);
                Assert.Equal(expected, result);
            }
        }

        [Fact]
        public void Multiply_MaxScalar_HandlesOrderMinusOne()
        {
            TwistedEdwardsCurveType[] curveTypes = new TwistedEdwardsCurveType[]
            {
                TwistedEdwardsCurveType.Edwards25519,
                TwistedEdwardsCurveType.Edwards448
            };

            ECMode[] modes = new ECMode[]
            {
                ECMode.EC_STANDARD_AFFINE,
                ECMode.EC_STANDARD_PROJECTIVE,
                ECMode.EC_FASTEST
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = TwistedEdwardsCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();
                var orderMinusOne = curve.order - 1;

                for (int j = 0; j < modes.Length; j++)
                {
                    var result = TwistedEdwardsMath.Multiply(
                        curve, orderMinusOne, G, modes[j]);

                    var negG = TwistedEdwardsMath.Negate(curve, G);
                    Assert.Equal(negG, result);
                }
            }
        }

        [Fact]
        public void Multiply_ScalarCofactor_ReturnsPointInPrimeSubgroup()
        {
            TwistedEdwardsCurveType[] curveTypes = new TwistedEdwardsCurveType[]
            {
                TwistedEdwardsCurveType.Edwards25519,
                TwistedEdwardsCurveType.Edwards448
            };

            for (int i = 0; i < curveTypes.Length; i++)
            {
                var curve = TwistedEdwardsCurve.GetNamedCurve(curveTypes[i]);
                var G = curve.GetBasePoint();
                var cofactor = curve.cofactor;

                var result = TwistedEdwardsMath.Multiply(
                    curve, cofactor, G, ECMode.EC_FASTEST);
                Assert.NotEqual(ECPoint.POINT_INFINITY, result);
            }
        }

        #endregion
    }
}
