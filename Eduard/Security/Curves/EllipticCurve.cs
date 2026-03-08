using System;
using Eduard;
using System.Diagnostics;
using MSCrypto = System.Security.Cryptography;
using Eduard.Security.Primitives;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Represents an elliptic curve in short Weierstrass form over a prime field Fp.
    /// </summary>
    /// <remarks>
    /// Supports curve parameter management, point validation, Koblitz encoding for message <br/>
    /// embedding, and modular square root computation with algorithm auto-selection.
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public sealed class EllipticCurve
    {
        /// <summary>
        /// Curve coefficients a and b.
        /// </summary>
        public BigInteger a, b;

        /// <summary>
        /// Prime field modulus p and curve order.
        /// </summary>
        public BigInteger field, order;

        /// <summary>
        /// Cofactor h = #E(Fp)/order.
        /// </summary>
        public BigInteger cofactor;
        private ECPoint basePoint;

        private static MSCrypto.RandomNumberGenerator rand;
        private static bool enableSpeedup;

        /// <summary>
        /// Initializes a random Weierstrass curve with a prime field of specified bit length.
        /// </summary>
        /// <param name="bits">Bit length of the prime field.</param>
        public EllipticCurve(int bits)
        {
            rand = MSCrypto.RandomNumberGenerator.Create();
            field = BigInteger.GenProbablePrime(rand, bits, 50);

            a = BigInteger.Next(rand, 1, field - 1);
            enableSpeedup = ModSqrtUtil.CanSpeedup(field);
            ModSqrtUtil.InitParams(field);

            BigInteger temp = (a * a) % field;
            temp = (temp * a) % field;
            temp = (4 * temp) % field;

            b = BigInteger.Next(rand, 1, field - 1);
            BigInteger B2 = (b * b) % field;

            BigInteger val = (27 * B2) % field;
            BigInteger check = (temp + val) % field;

            order = 1; cofactor = 1;
            basePoint = ECPoint.POINT_INFINITY;

            while (check == 0)
            {
                b = BigInteger.Next(rand, 1, field - 1);
                B2 = (b * b) % field;

                val = (27 * B2) % field;
                check = (temp + val) % field;
            }
        }

        /// <summary>
        /// Initializes a Weierstrass curve with explicit parameters.
        /// </summary>
        /// <param name="args">[a, b, field, order, cofactor]</param>
        /// <exception cref="ArgumentException">Thrown when more than 5 parameters are provided.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the curve is singular.</exception>
        public EllipticCurve(params BigInteger[] args)
        {
            if (args.Length > 5)
                throw new ArgumentException("Too many arguments.");

            rand = MSCrypto.RandomNumberGenerator.Create();
            a = args[0];
            b = args[1];

            field = args[2]; order = args[3];
            basePoint = ECPoint.POINT_INFINITY;

            cofactor = args[4];
            BigInteger A2 = (a * a) % field;

            BigInteger B2 = (b * b) % field;
            BigInteger delta = (a * A2) % field;

            delta = (4 * delta) % field;
            BigInteger val = ((27 * B2) % field);
            delta = (delta + val) % field;

            if (delta == 0)
                throw new InvalidOperationException(
                    "Invalid curve: singular Weierstrass form.");

            enableSpeedup = ModSqrtUtil.CanSpeedup(field);
            ModSqrtUtil.InitParams(field);
        }

        /// <summary>
        /// Evaluates the right-hand side of the Weierstrass equation at x.
        /// </summary>
        /// <param name="x">The x-coordinate to evaluate.</param>
        /// <returns>y^2 = x^3 + ax + b (mod p).</returns>
        public BigInteger Evaluate(BigInteger x)
        {
            BigInteger result = (x * x) % field;
            result = (result * x) % field;
            BigInteger temp = (a * x + b) % field;
            result = (result + temp) % field;
            return result;
        }

        /// <summary>
        /// Encodes a message as a curve point using Koblitz's method.
        /// </summary>
        /// <param name="m">Represents a binary message as a large integer.</param>
        /// <param name="r">Iterations (default: 30).</param>
        /// <returns>Point encoding the message, or POINT_INFINITY if encoding fails.</returns>
        public ECPoint GetPoint(BigInteger m, int r=30)
        {
            BigInteger test = (r + 1) * m;
            BigInteger xs = (m * r) % field;

            /* if the product exceeds the value of the prime field, the algorithm fails */
            if (test >= field) return ECPoint.POINT_INFINITY;
            BigInteger ys = 1;

            int ks = 0;
            xs++;

            if (xs >= field)
                xs -= field;

            while(ks < r)
            {
                BigInteger t = Evaluate(xs);

                if (BigInteger.Jacobi(t, field) == 1)
                {
                    ys = Sqrt(t, true);
                    break;
                }

                xs++; 
                ks++;

                if(xs >= field) 
                    xs -= field;
            }

            return new ECPoint(xs, ys);
        }

        /// <summary>
        /// Decodes the original message from a Koblitz-encoded point.
        /// </summary>
        /// <param name="point">The encoded point.</param>
        /// <param name="r">Iterations used during encoding (default: 30).</param>
        /// <returns>The original message m, or -1 if invalid.</returns>
        public BigInteger GetMessage(ECPoint point, int r=30)
        {
            if (point == ECPoint.POINT_INFINITY) return -1;
            BigInteger steps = r;

            BigInteger m = point.GetAffineX() - 1;
            return m / steps;
        }

        /// <summary>
        /// Gets or generates the curve's base point (generator).
        /// </summary>
        /// <param name="isGenerated">If true, returns cached base point when available.</param>
        /// <returns>A point in the prime-order subgroup.</returns>
        public ECPoint GetBasePoint(bool isGenerated = false)
        {
            bool done = false;
            BigInteger x = 0;

            BigInteger y = 0;
            BigInteger temp = 0;

            if(isGenerated && basePoint != ECPoint.POINT_INFINITY)
                return basePoint;

            do
            {
                x = BigInteger.Next(rand, 0, field - 1);
                temp = Evaluate(x);

                if (temp < 2)
                    return new ECPoint(x, temp);

                if (BigInteger.Jacobi(temp, field) == 1)
                {
                    done = true;
                    y = Sqrt(temp);

                    BigInteger eval = (y * y) % field;
                    if (temp != eval) done = false;

                    if (done)
                    {
                        ECPoint tempPoint = new ECPoint(x, y);
                        basePoint = ECMath.Multiply(this, cofactor, tempPoint, ECMode.EC_STANDARD_PROJECTIVE);
                        done = (basePoint != ECPoint.POINT_INFINITY);
                    }
                }
            }
            while (!done);

            return basePoint;
        }

        /// <summary>
        /// Sets a specific point as the curve's base point with validation.
        /// </summary>
        /// <param name="point">The point to set as generator.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the point does not lie on the curve, the y-coordinate does not satisfy
        /// the curve equation, or when multiplied by the cofactor it yields the point at infinity
        /// (indicating it lies in a small-order subgroup).
        /// </exception>
        public void SetBasePoint(ECPoint point)
        {
            ECPoint tempPoint = point;
            var Y2 = Evaluate(tempPoint.GetAffineX());

            if (BigInteger.Jacobi(Y2, field) != 1 && Y2 > 0)
                throw new InvalidOperationException(
                    "The generator point is not on " 
                    + "the Weierstrass curve.");
            else
            {
                BigInteger y = tempPoint.GetAffineY();
                BigInteger eval = (y * y) % field;

                if (eval != Y2)
                    throw new InvalidOperationException(
                        "Invalid generator point for " 
                        + "Weierstrass curve.");
                else
                {
                    ECPoint testPoint = ECMath.Multiply(this, cofactor, tempPoint, ECMode.EC_STANDARD_PROJECTIVE);
                    if (testPoint != ECPoint.POINT_INFINITY) basePoint = tempPoint;
                    else
                        throw new InvalidOperationException(
                            "Chosen generator point yields small-order" 
                            + " subgroup on Weierstrass curve.");
                }
            }
        }

        /// <summary>
        /// Computes modular square root using optimal algorithm.
        /// </summary>
        /// <param name="val">Value to find root for.</param>
        /// <param name="forceOutput">If true, forces root computation.</param>
        /// <returns>Square root r with r^2 = val (mod p).</returns>
        public BigInteger Sqrt(BigInteger val, bool forceOutput = false)
        {
            /* compute the modular square root using the optimized Rotaru-Iftene method */
            if (enableSpeedup)
                return OptimizedRotaruIftene.Sqrt(val);
            
            /* if the correct output is required, the algorithm will solve random quadratic equations to find the real root */
            if (forceOutput) return ModSqrtUtil.Sqrt(val, field);

            /* uses the standard Tonelli-Shanks algorithm to obtain the modular square root */
            return ModSqrtUtil.TonelliShanks(val, field);
        }
    }
}
