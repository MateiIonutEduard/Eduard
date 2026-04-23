using Eduard;
using Eduard.Security;
using Eduard.Security.Curves;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Tests.Curves
{
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
    }
}
