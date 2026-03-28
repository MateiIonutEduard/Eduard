using Eduard.Security.Curves;
using Eduard.Security.Primitives;
using System.Diagnostics;

namespace Eduard.Security.Extensions
{
    /// <summary>
    /// This class is a utility for converting projective points to affine form and vice versa.
    /// </summary>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class ECPointExtensions
    {
        /// <summary>
        /// Converts a point from Jacobian projective coordinates to affine coordinates on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context providing field modulus.</param>
        /// <param name="point">The point in Jacobian coordinates (X, Y, Z).</param>
        /// <returns>The affine point (x, y), or <see cref="ECPoint.POINT_INFINITY"/> if Z = 0.</returns>
        /// <remarks>
        /// Performs the transformation: x = X/Z^2, y = Y/Z^3. The point at infinity (Z = 0) is preserved. <br/>
        /// This method is typically called after scalar multiplication to obtain the final result.
        /// </remarks>
        public static ECPoint ToAffine(this EllipticCurve curve, ECPoint3w point)
        {
            if (point == ECPoint3w.POINT_INFINITY || point.z == 0) 
                return ECPoint.POINT_INFINITY;

            BigInteger inv_Z = BarrettReducer.InvMod(point.z);
            BigInteger iZ2 = BarrettReducer.MultMod(inv_Z, inv_Z);
            BigInteger iZ3 = BarrettReducer.MultMod(iZ2, inv_Z);

            BigInteger X = BarrettReducer.MultMod(point.x, iZ2);
            BigInteger Y = BarrettReducer.MultMod(point.y, iZ3);
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Converts a point from extended projective coordinates to affine coordinates on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context providing field modulus.</param>
        /// <param name="point">The point in extended coordinates (X, Y, T, Z).</param>
        /// <returns>The affine point (x, y), or <see cref="ECPoint.POINT_INFINITY"/> if Z = 0 or (0, 1).</returns>
        /// <remarks>
        /// Performs: x = X/Z, y = Y/Z. The identity element (0, 1) maps to point at infinity. <br/>
        /// This representation is optimal for unified addition formulas (Hisil et al., ASIACRYPT 2008).
        /// </remarks>
        public static ECPoint ToAffine(this TwistedEdwardsCurve curve, ECPoint4 point)
        {
            if(point == ECPoint4.POINT_INFINITY || point.z == 0)
                return ECPoint.POINT_INFINITY;

            BigInteger inv_Z = BarrettReducer.InvMod(point.z);
            BigInteger X = BarrettReducer.MultMod(point.x, inv_Z);
            BigInteger Y = BarrettReducer.MultMod(point.y, inv_Z);

            if (X == 0 && Y == 1) return ECPoint.POINT_INFINITY;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Converts a point from homogeneous projective coordinates to affine coordinates on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context providing field modulus.</param>
        /// <param name="point">The point in homogeneous coordinates (X, Y, Z).</param>
        /// <returns>The affine point (x, y), or <see cref="ECPoint.POINT_INFINITY"/> if Z = 0 or (0, 1).</returns>
        /// <remarks>
        /// Performs: x = X/Z, y = Y/Z. Homogeneous coordinates are simpler but less efficient <br/>
        /// than extended coordinates for addition operations.
        /// </remarks>
        public static ECPoint ToAffine(this TwistedEdwardsCurve curve, ECPoint3 point)
        {
            if (point == ECPoint3.POINT_INFINITY || point.z == 0)
                return ECPoint.POINT_INFINITY;

            BigInteger inv_Z = BarrettReducer.InvMod(point.z);
            BigInteger X = BarrettReducer.MultMod(point.x, inv_Z);

            BigInteger Y = BarrettReducer.MultMod(point.y, inv_Z);
            if(X == 0 && Y == 1) return ECPoint.POINT_INFINITY;
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Converts an affine point to extended projective coordinates on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="point">The affine point (x, y).</param>
        /// <returns>Extended coordinates (X, Y, T, Z) with T = (X*Y)/Z, Z = 1.</returns>
        /// <remarks>
        /// For affine points, Z is set to 1, and T is precomputed as x*y. This representation <br/>
        /// enables the use of optimized unified addition formulas with 9M + 1S + 1D operations. <br/>
        /// The identity element (0, 1) maps to point at infinity.
        /// </remarks>
        public static ECPoint4 ToExtendedProjective(this TwistedEdwardsCurve curve, ECPoint point)
        {
            if(point == ECPoint.POINT_INFINITY || (point.x == 0 && point.y == 1)) 
                return ECPoint4.POINT_INFINITY;

            BigInteger t = BarrettReducer.MultMod(point.x, point.y);
            return new ECPoint4(point.x, point.y, t, 1);
        }

        /// <summary>
        /// Converts a point from homogeneous to extended projective coordinates on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="point">The point in homogeneous coordinates (X, Y, Z).</param>
        /// <returns>Extended coordinates (XZ, YZ, XY, Z^2) suitable for unified addition.</returns>
        /// <remarks>
        /// This transformation precomputes the products required for extended coordinate arithmetic, <br/>
        /// allowing seamless integration between coordinate systems during mixed operations.
        /// </remarks>
        public static ECPoint4 ToExtendedProjective(this TwistedEdwardsCurve curve, ECPoint3 point)
        {
            if (point == ECPoint3.POINT_INFINITY)
                return ECPoint4.POINT_INFINITY;

            BigInteger xz = BarrettReducer.MultMod(point.x, point.z);
            BigInteger yz = BarrettReducer.MultMod(point.y, point.z);

            BigInteger xy = BarrettReducer.MultMod(point.x, point.y);
            BigInteger z2 = BarrettReducer.MultMod(point.z, point.z);
            return new ECPoint4(xz, yz, xy, z2);
        }

        /// <summary>
        /// Creates an extended projective point from a homogeneous point, omitting T-coordinate computation.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="point">The source point in homogeneous coordinates.</param>
        /// <returns>Extended coordinates with T = 0, suitable for doubling operations where T is unused.</returns>
        /// <remarks>
        /// This lightweight conversion is used when T is not required for the subsequent operation, <br/>
        /// avoiding unnecessary multiplications during point doubling in scalar multiplication loops.
        /// </remarks>
        public static ECPoint4 GetPointCopy(this TwistedEdwardsCurve curve, ECPoint3 point)
        {
            /* point at infinity */
            if(point == ECPoint3.POINT_INFINITY)
                return ECPoint4.POINT_INFINITY;

            var res = new ECPoint4();
            res.x = point.x; res.y = point.y;

            /* T is not used in point doubling formula */
            res.z = point.z; res.t = 0;
            return res;
        }

        /// <summary>
        /// Converts an affine point to homogeneous projective coordinates on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="point">The affine point (x, y).</param>
        /// <returns>Homogeneous coordinates (X, Y, Z) with Z = 1.</returns>
        public static ECPoint3 ToProjective(this TwistedEdwardsCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY || (point.x == 0 && point.y == 1))
                return ECPoint3.POINT_INFINITY;

            return new ECPoint3(point.x, point.y, 1);
        }

        /// <summary>
        /// Converts a point from extended to homogeneous projective coordinates on a twisted Edwards curve.
        /// </summary>
        /// <param name="curve">The twisted Edwards curve context.</param>
        /// <param name="point">The point in extended coordinates (X, Y, T, Z).</param>
        /// <returns>Homogeneous coordinates (X, Y, Z).</returns>
        public static ECPoint3 ToProjective(this TwistedEdwardsCurve curve, ECPoint4 point)
        {
            if (point == ECPoint4.POINT_INFINITY)
                return ECPoint3.POINT_INFINITY;

            return new ECPoint3(point.x, point.y, point.z);
        }

        /// <summary>
        /// Converts an affine point to Jacobian projective coordinates on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="point">The affine point (x, y).</param>
        /// <returns>Jacobian coordinates (X, Y, Z) with Z = 1.</returns>
        /// <remarks>
        /// Jacobian coordinates eliminate the need for modular inverses in point addition, <br/>
        /// making them the standard choice for scalar multiplication algorithms.
        /// </remarks>
        public static ECPoint3w ToJacobian(this EllipticCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY) 
                return ECPoint3w.POINT_INFINITY;

            ECPoint3w jacobianPoint = new ECPoint3w(point.GetAffineX(),
                point.GetAffineY(), 1);

            return jacobianPoint;
        }

        /// <summary>
        /// Converts a point from modified Jacobian to standard Jacobian coordinates.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="point">The point in modified Jacobian coordinates (X, Y, Z, aZ^4).</param>
        /// <returns>Standard Jacobian coordinates (X, Y, Z).</returns>
        /// <remarks>
        /// Discards the cached aZ^4 value when switching to coordinate systems that don't support it.
        /// </remarks>
        public static ECPoint3w ToJacobian(this EllipticCurve curve, ECPoint4w point)
        {
            if (point == ECPoint4w.POINT_INFINITY) 
                return ECPoint3w.POINT_INFINITY;

            ECPoint3w jacobianPoint = new ECPoint3w(point.x,
                point.y, point.z);

            return jacobianPoint;
        }

        /// <summary>
        /// Converts a point from Jacobian-Chudnovsky to standard Jacobian coordinates.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="point">The point in Jacobian-Chudnovsky coordinates (X, Y, Z, Z^2, Z^3).</param>
        /// <returns>Standard Jacobian coordinates (X, Y, Z).</returns>
        /// <remarks>
        /// Discards the precomputed Z^2 and Z^3 powers when converting to simpler representations.
        /// </remarks>
        public static ECPoint3w ToJacobian(this EllipticCurve curve, ECPoint5w point)
        {
            if (point == ECPoint5w.POINT_INFINITY)
                return ECPoint3w.POINT_INFINITY;

            ECPoint3w jacobianPoint = new ECPoint3w(point.x,
                point.y, point.z);

            return jacobianPoint;
        }

        /// <summary>
        /// Converts a point from Jacobian-Chudnovsky to affine coordinates on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="point">The point in Jacobian-Chudnovsky coordinates (X, Y, Z, Z^2, Z^3).</param>
        /// <returns>The affine point (x, y), or <see cref="ECPoint.POINT_INFINITY"/> if Z = 0.</returns>
        /// <remarks>
        /// Uses the precomputed Z^2 and Z^3 values to avoid recomputing powers during conversion, <br/>
        /// improving efficiency when converting from window method accumulator points.
        /// </remarks>
        public static ECPoint ToAffine(this EllipticCurve curve, ECPoint5w point)
        {
            if (point == ECPoint5w.POINT_INFINITY || point.z == 0) 
                return ECPoint.POINT_INFINITY;

            BigInteger Z5 = BarrettReducer.MultMod(point.z2, point.z3);
            BigInteger iZ5 = BarrettReducer.InvMod(Z5);

            BigInteger iZ2 = BarrettReducer.MultMod(iZ5, point.z3);
            BigInteger iZ3 = BarrettReducer.MultMod(iZ5, point.z2);

            BigInteger X = BarrettReducer.MultMod(point.x, iZ2);
            BigInteger Y = BarrettReducer.MultMod(point.y, iZ3);
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Converts an affine point to Jacobian-Chudnovsky projective coordinates.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="point">The affine point (x, y).</param>
        /// <returns>Jacobian-Chudnovsky coordinates (X, Y, Z, Z^2, Z^3) with Z = Z^2 = Z^3 = 1.</returns>
        /// <remarks>
        /// This representation precomputes Z^2 and Z^3, which are used in the mixed addition <br/>
        /// formulas of window-based scalar multiplication (EC_FASTEST mode).
        /// </remarks>
        public static ECPoint5w ToJacobianChudnovsky(this EllipticCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY) 
                return ECPoint5w.POINT_INFINITY;

            ECPoint5w jacobianChudnovskyPoint = new ECPoint5w(point.GetAffineX(), 
                point.GetAffineY(), 1, 1, 1);

            return jacobianChudnovskyPoint;
        }

        /// <summary>
        /// Converts a point from modified Jacobian to affine coordinates on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="point">The point in modified Jacobian coordinates (X, Y, Z, aZ⁴).</param>
        /// <returns>The affine point (x, y), or <see cref="ECPoint.POINT_INFINITY"/> if Z = 0.</returns>
        /// <remarks>
        /// Recovers affine coordinates as x = X/Z^2, y = Y/Z^3 using a single modular <br/>
        /// inversion. The cached aZ^4 value is discarded during affine recovery.
        /// </remarks>
        public static ECPoint ToAffine(this EllipticCurve curve, ECPoint4w point)
        {
            if (point == ECPoint4w.POINT_INFINITY || point.z == 0) 
                return ECPoint.POINT_INFINITY;

            BigInteger inv_Z = BarrettReducer.InvMod(point.z);
            BigInteger iZ2 = BarrettReducer.MultMod(inv_Z, inv_Z);
            BigInteger iZ3 = BarrettReducer.MultMod(iZ2, inv_Z);

            BigInteger X = BarrettReducer.MultMod(point.x, iZ2);
            BigInteger Y = BarrettReducer.MultMod(point.y, iZ3);
            return new ECPoint(X, Y);
        }

        /// <summary>
        /// Converts an affine point to modified Jacobian coordinates on a Weierstrass curve.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="point">The affine point (x, y).</param>
        /// <returns>Modified Jacobian coordinates (X, Y, Z, aZ^4) with Z = 1, aZ^4 = a.</returns>
        /// <remarks>
        /// Modified Jacobian coordinates (X, Y, Z, aZ^4) cache the value aZ^4, eliminating two <br/>
        /// squaring operations (Z^2 and Z^4 recomputation) during point doubling. This reduces <br/>
        /// doubling cost from 4M + 6S to 4M + 4S, providing approximately 20-25% performance <br/>
        /// improvement in scalar multiplication loops where doubling dominates.
        /// </remarks>
        public static ECPoint4w ToModifiedJacobian(this EllipticCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY) 
                return ECPoint4w.POINT_INFINITY;

            ECPoint4w modifiedJacobianPoint = new ECPoint4w(
                point.GetAffineX(), point.GetAffineY(), 1, curve.a);
            return modifiedJacobianPoint;
        }

        /// <summary>
        /// Converts a point from Jacobian-Chudnovsky to modified Jacobian coordinates.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="point">The point in Jacobian-Chudnovsky coordinates (X, Y, Z, Z^2, Z^3).</param>
        /// <returns>Modified Jacobian coordinates (X, Y, Z, aZ^4) with aZ^4 = a*(Z^2)^2.</returns>
        /// <remarks>
        /// Computes aZ^4 from the precomputed Z^2 value, avoiding redundant squaring operations. <br/>
        /// Used when transitioning from window precomputation to modified Jacobian doubling.
        /// </remarks>
        public static ECPoint4w ToModifiedJacobian(this EllipticCurve curve, ECPoint5w point)
        {
            if (point == ECPoint5w.POINT_INFINITY) 
                return ECPoint4w.POINT_INFINITY;

            BigInteger Z4 = BarrettReducer.MultMod(point.z2, point.z2);
            BigInteger aZ4 = BarrettReducer.MultMod(curve.a, Z4);

            ECPoint4w modifiedJacobianPoint = new ECPoint4w(point.x, 
                point.y, point.z, aZ4);
            return modifiedJacobianPoint;
        }

        /// <summary>
        /// Converts a point from standard Jacobian to modified Jacobian coordinates.
        /// </summary>
        /// <param name="curve">The elliptic curve context.</param>
        /// <param name="point">The point in standard Jacobian coordinates (X, Y, Z).</param>
        /// <returns>Modified Jacobian coordinates (X, Y, Z, aZ^4) with aZ^4 = a*(Z^2)^2.</returns>
        /// <remarks>
        /// Computes the aZ^4 cache from Z. This conversion is used to switch from <br/>
        /// Jacobian addition to modified Jacobian doubling during mixed-coordinate <br/>
        /// scalar multiplication.
        /// </remarks>
        public static ECPoint4w ToModifiedJacobian(this EllipticCurve curve, ECPoint3w point)
        {
            if (point == ECPoint3w.POINT_INFINITY) 
                return ECPoint4w.POINT_INFINITY;

            BigInteger Z2 = BarrettReducer.MultMod(point.z, point.z);
            BigInteger Z4 = BarrettReducer.MultMod(Z2, Z2);
            BigInteger aZ4 = BarrettReducer.MultMod(curve.a, Z4);

            ECPoint4w modifiedJacobianPoint = new ECPoint4w(point.x, 
                point.y, point.z, aZ4);
            return modifiedJacobianPoint;
        }
    }
}
