using System;
using Eduard;
using System.Diagnostics;
using MSCrypto = System.Security.Cryptography;
using Eduard.Security.Primitives;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Represents an elliptic curve in twisted Edwards form over a prime field Fp.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Twisted Edwards curves provide complete addition laws and are widely used <br/>
    /// in cryptographic protocols like Ed25519. The curve parameters a and d must <br/>
    /// satisfy a != 0, d != 0, and a != d to ensure non-singularity.
    /// </para>
    /// <para>
    /// For complete curves (d non-square), unified addition formulas work for all points. <br/>
    /// This implementation also supports optimization via quadratic twist when a = -1, <br/>
    /// as described in Hisil et al. (2008) "Twisted Edwards Curves Revisited".
    /// </para>
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public sealed class TwistedEdwardsCurve
    {
        /// <summary>
        /// Curve coefficients a and d from the twisted Edwards equation.
        /// </summary>
        public BigInteger a, d;

        /// <summary>
        /// Prime field modulus p and curve order.
        /// </summary>
        public BigInteger field, order;

        /// <summary>
        /// Cofactor h = #E(Fp)/order, for twisted Edwards curves.
        /// </summary>
        public BigInteger cofactor;
        private ECPoint basePoint;

        private static MSCrypto.RandomNumberGenerator rand;
        private static bool enableSpeedup;

        /// <summary>
        /// If true, operations are performed on the quadratic twist for optimization.
        /// </summary>
        internal bool computeOnTwist;

        /// <summary>
        /// Twist parameter kt for optimized formulas when a = -1.
        /// </summary>
        internal BigInteger kt, aroot;

        /// <summary>
        /// Indicates whether the curve is complete (a square, d non-square).
        /// </summary>
        internal bool isComplete;

        /// <summary>
        /// Initializes a twisted Edwards curve with explicit parameters.
        /// </summary>
        /// <param name="args">Parameter array: [a, d, field, order, cofactor]</param>
        /// <exception cref="ArgumentException">Thrown when more than 5 parameters are provided.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the curve is singular (invalid parameters) or when the cofactor
        /// modulo 4 condition is not satisfied.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Validates:
        /// <list type="bullet">
        /// <item><description>Cofactor must be multiple of 4 for twisted Edwards curve requirements</description></item>
        /// <item><description>Non-singularity condition: a*d*(a - d) != 0 mod p</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// For curves with a = -1, pre-computes optimization parameters for Hisil et al. formulas:
        /// <list type="bullet">
        /// <item><description>aroot = modular square root of (-a) mod p</description></item>
        /// <item><description>kt = -2d / a</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public TwistedEdwardsCurve(params BigInteger[] args)
        {
            if (args.Length > 5)
                throw new ArgumentException("Too many arguments.");

            rand = MSCrypto.RandomNumberGenerator.Create();
            a = args[0];
            d = args[1];

            field = args[2]; order = args[3];
            basePoint = ECPoint.POINT_INFINITY;
            cofactor = args[4];

            BigInteger t = (field + a - d) % field;
            t = (t * ((a * d) % field)) % field;

            if((cofactor & 0x3) != 0 || t == 0)
                throw new InvalidOperationException(
                    "The twisted Edwards curve is " 
                    + "invalid or singular.");

            isComplete = (BigInteger.Jacobi(a, field) == 1
                && BigInteger.Jacobi(d, field) == -1);

            enableSpeedup = ModSqrtUtil.CanSpeedup(field);
            ModSqrtUtil.InitParams(field);

            computeOnTwist = false;
            kt = aroot = 0;

            /* see Hisil et al. (2008) "Twisted Edwards curves revisited." pp. 326-343 */
            if (a == field - 1 && BigInteger.Jacobi(field - a, field) == 1 && isComplete)
            {
                aroot = Sqrt(field - a, true);
                BigInteger ma = ((aroot * aroot) % field).Inverse(field);

                kt = (2 * d * ma) % field;
                computeOnTwist = true;
            }
        }

        /// <summary>
        /// Evaluates the right-hand side of the twisted Edwards equation at a given y-coordinate.
        /// </summary>
        /// <param name="y">The y-coordinate to evaluate.</param>
        /// <returns>The value x^2 = (1 - y^2) / (a - d·y^2) mod p.</returns>
        public BigInteger Evaluate(BigInteger y)
        {
            BigInteger A1 = (y * y) % field;
            BigInteger A2 = (d * A1) % field;

            BigInteger A3 = (field + 1 - A1) % field;
            BigInteger A4 = (field + a - A2) % field;

            BigInteger X2 = (A3 * A4.Inverse(field)) % field;
            return X2;
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

            if (isGenerated && basePoint != ECPoint.POINT_INFINITY)
                return basePoint;

            do
            {
                y = BigInteger.Next(rand, 0, field - 1);
                temp = Evaluate(y);

                if (temp < 2)
                    return new ECPoint(temp, y);

                if (BigInteger.Jacobi(temp, field) == 1)
                {
                    done = true;
                    x = Sqrt(temp);

                    BigInteger eval = (x * x) % field;
                    if (temp != eval) done = false;

                    if (done)
                    {
                        ECPoint tempPoint = new ECPoint(x, y);
                        basePoint = TwistedEdwardsMath.Multiply(this, cofactor, tempPoint, ECMode.EC_STANDARD_PROJECTIVE);
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
        /// Thrown when the point does not satisfy the curve equation, or when multiplied
        /// by the cofactor it yields the point at infinity (indicating small-order subgroup).
        /// </exception>
        public void SetBasePoint(ECPoint point)
        {
            ECPoint tempPoint = point;
            var temp = Evaluate(tempPoint.GetAffineY());

            if (BigInteger.Jacobi(temp, field) != 1 && temp > 0)
                throw new InvalidOperationException(
                    "The generator point is not on the" 
                    + " twisted Edwards curve.");
            else
            {
                BigInteger x = tempPoint.GetAffineX();
                BigInteger eval = (x * x) % field;

                if (eval != temp)
                    throw new InvalidOperationException(
                        "Invalid generator point for the" 
                        + " twisted Edwards curve.");
                else
                {
                    ECPoint testPoint = TwistedEdwardsMath.Multiply(this, cofactor, tempPoint, ECMode.EC_STANDARD_PROJECTIVE);
                    if (testPoint != ECPoint.POINT_INFINITY) basePoint = tempPoint;
                    else
                        throw new InvalidOperationException(
                            "Chosen generator point yields a"
                            + " small-order subgroup on the " 
                            + "twisted Edwards curve.");
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
