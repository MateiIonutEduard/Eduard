using System;
using Eduard;
using System.Diagnostics;
using Eduard.Security.Extensions;
using Eduard.Security.Primitives;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Provides mathematical operations for points on the twisted Edwards curve.
    /// </summary>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public static class TwistedEdwardsMath
    {
        /// <summary>
        /// Multiply an affine point on the twisted Edwards curve by a given scalar.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="k"></param>
        /// <param name="point"></param>
        /// <param name="opMode"></param>
        /// <returns></returns>
        public static ECPoint Multiply(TwistedEdwardsCurve curve, BigInteger k, ECPoint point, ECMode opMode = ECMode.EC_STANDARD_AFFINE, bool securityCheck = false)
        {
            if (k < 0) throw new ArgumentException("Bad input.");

            if (k == 0 || point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            if (securityCheck && !curve.ValidatePoint(point))
                throw new ArgumentException("The generator point on the twisted Edwards curve produces a small-order subgroup.");

            ECPoint temp = point;
            ECPoint result = ECPoint.POINT_INFINITY;
            int t = k.GetBits();

            if (opMode == ECMode.EC_STANDARD_AFFINE)
            {
                for (int j = 0; j < t; j++)
                {
                    if (k.TestBit(j))
                        result = Add(curve, result, temp);

                    temp = Add(curve, temp, temp);
                }
            }
            else if (opMode == ECMode.EC_STANDARD_PROJECTIVE)
            {
                ECPoint3 auxPoint = ECPoint3.POINT_INFINITY;
                var basePoint = curve.ToProjective(temp);

                for (int j = 0; j < t; j++)
                {
                    if (k.TestBit(j))
                        auxPoint = Ed3Math.UnifiedAdd(curve, auxPoint, basePoint);

                    basePoint = Ed3Math.UnifiedDoubling(curve, basePoint);
                }

                result = curve.ToAffine(auxPoint);
            }
            else if (opMode == ECMode.EC_SECURE)
                throw new NotImplementedException("Requires transformation to Montgomery form for improved performance.");
            else
            {
                int i, j, win;
                int bc, ubits = 0;

                int windowSize = 8;
                int tbits = 0;

                var table = new ECPoint4[windowSize];
                table[0] = curve.ToExtendedProjective(point);

                var squarePoint = Ed4Math.DedicatedDoubling(curve, table[0]);
                ECPoint4 auxPoint = ECPoint4.POINT_INFINITY;

                BigInteger exp3 = 3 * k;
                bc = exp3.GetBits();

                /* compute the lookup table */
                for (i = 1; i < windowSize; i++)
                    table[i] = Ed4Math.Add(curve, table[i - 1], squarePoint);

                for (i = bc - 1; i >= 1;)
                {
                    win = WindowUtil.NAFWindow(k, exp3, i, ref ubits, ref tbits, windowSize);
                    var projectivePoint = curve.ToProjective(auxPoint);

                    for (j = 0; j < ubits - 1; j++)
                        projectivePoint = Ed3Math.UnifiedDoubling(curve, projectivePoint);

                    if (ubits >= 1)
                    {
                        var tempPoint = curve.GetPointCopy(projectivePoint);
                        auxPoint = Ed4Math.DedicatedDoubling(curve, tempPoint);
                    }

                    if (win > 0)
                    {
                        var table_point = table[win >> 1];
                        auxPoint = Ed4Math.Add(curve, table_point, auxPoint);
                    }
                    if (win < 0)
                    {
                        var table_point = table[(-win) >> 1];
                        var negative_point = Ed4Math.Negate(curve, table_point);
                        auxPoint = Ed4Math.Add(curve, negative_point, auxPoint);
                    }

                    i -= ubits;

                    if (tbits != 0)
                    {
                        var lastPoint = curve.ToProjective(auxPoint);
                        i -= tbits;

                        for (j = 0; j < tbits - 1; j++)
                            lastPoint = Ed3Math.UnifiedDoubling(curve, lastPoint);

                        if (tbits >= 1)
                        {
                            var tempPoint = curve.GetPointCopy(lastPoint);
                            auxPoint = Ed4Math.DedicatedDoubling(curve, tempPoint);
                        }
                    }
                }

                result = curve.ToAffine(auxPoint);
            }

            return result;
        }

        /// <summary>
        /// Add two affine points on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ECPoint Add(TwistedEdwardsCurve curve, ECPoint left, ECPoint right)
        {
            if (left == ECPoint.POINT_INFINITY && right == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            if (left == ECPoint.POINT_INFINITY)
                return right;

            if (right == ECPoint.POINT_INFINITY)
                return left;

            if (left == Negate(curve, right))
                return ECPoint.POINT_INFINITY;

            if(curve.isComplete) 
                return CompleteAdd(curve, left, right);

            if (left == right) return DedicatedDoubling(curve, left);
            return DedicatedAdd(curve, left, right);
        }

        /// <summary>
        /// Adds two affine points using the unified formula on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ECPoint CompleteAdd(TwistedEdwardsCurve curve, ECPoint left, ECPoint right)
        {
            BigInteger p = curve.field;
            BigInteger A1 = (left.x * right.y) % p;

            BigInteger A2 = (left.y * right.x) % p;
            BigInteger A3 = (curve.d * (A1 * A2)) % p;

            BigInteger tx = (A3 + 1) % p;
            if (tx == 0) return ECPoint.POINT_INFINITY;

            BigInteger ty = (p + 1 - A3) % p;
            if (ty < 0) ty += p;

            if (ty == 0) return ECPoint.POINT_INFINITY;
            BigInteger txy = ((tx * ty) % p).Inverse(p);

            BigInteger x = (A1 + A2) % p;
            x = (((x * txy) % p) * ty) % p;

            BigInteger A4 = (left.y * right.y) % p;
            BigInteger A5 = (curve.a * ((left.x * right.x) % p)) % p;

            BigInteger y = (p + A4 - A5) % p;
            y = (((y * txy) % p) * tx) % p;
            return new ECPoint(x, y);
        }

        /// <summary>
        /// Adds two affine points using the dedicated formula on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ECPoint DedicatedAdd(TwistedEdwardsCurve curve, ECPoint left, ECPoint right)
        {
            BigInteger p = curve.field;
            BigInteger A1 = (left.x * left.y) % p;

            BigInteger A2 = (right.x * right.y) % p;
            BigInteger A3 = (left.y * right.y) % p;

            BigInteger A4 = (curve.a * (left.x * right.x)) % p;
            BigInteger A5 = (A1 + A2) % p;

            BigInteger A6 = (A3 + A4) % p;
            if (A6 == 0) return ECPoint.POINT_INFINITY;

            BigInteger A7 = (p + A1 - A2) % p;
            BigInteger A8 = (left.x * right.y) % p;

            BigInteger A9 = (left.y * right.x) % p;
            BigInteger A10 = (p + A8 - A9) % p;

            if (A10 == 0) return ECPoint.POINT_INFINITY;
            BigInteger A11 = ((A6 * A10) % p).Inverse(p);

            BigInteger x = (((A5 * A10) % p) * A11) % p;

            BigInteger y = (((A6 * A7) % p) * A11) % p;
            return new ECPoint(x, y);
        }

        /// <summary>
        /// Doubles the given affine point on the twisted Edwards curve using the dedicated formula.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint DedicatedDoubling(TwistedEdwardsCurve curve, ECPoint point)
        {
            BigInteger p = curve.field;
            BigInteger A1 = (2 * (point.x * point.y)) % p;

            BigInteger A2 = (point.y * point.y) % p;
            BigInteger A3 = (curve.a * ((point.x * point.x) % p)) % p;

            BigInteger A4 = (A2 + A3) % p;
            if (A4 == 0) return ECPoint.POINT_INFINITY;

            BigInteger A5 = (p + 2 - A4) % p;
            if (A5 == 0) return ECPoint.POINT_INFINITY;
            BigInteger A6 = ((A4 * A5) % p).Inverse(p);

            BigInteger x = (((A1 * A5) % p) * A6) % p;
            BigInteger y = (p + A2 - A3) % p;

            y = (((y * A4) % p) * A6) % p;
            return new ECPoint(x, y);
        }

        /// <summary>
        /// Computes the additive inverse of a given affine point on the twisted Edwards curve.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static ECPoint Negate(TwistedEdwardsCurve curve, ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY)
                return ECPoint.POINT_INFINITY;

            return new ECPoint(curve.field - point.x, point.y);
        }
    }
}
