using System;
using Eduard;
using System.Diagnostics;
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

            a = args[0];
            d = args[1];

            field = args[2]; order = args[3];
            basePoint = ECPoint.POINT_INFINITY;
            cofactor = args[4];

            BarrettReducer.SetModulus(field);
            BigInteger t1 = BarrettReducer.SubMod(a, d);

            BigInteger t2 = BarrettReducer.MultMod(a, d);
            BigInteger t = BarrettReducer.MultMod(t1, t2);

            if((cofactor & 0x3) != 0 || t == 0)
                throw new InvalidOperationException(
                    "The twisted Edwards curve is " 
                    + "invalid or singular.");

            isComplete = (BigInteger.Jacobi(a, field) == 1
                && BigInteger.Jacobi(d, field) == -1);

            ModSqrtUtil.InitParams();
            computeOnTwist = false;
            kt = aroot = 0;

            /* see Hisil et al. (2008) "Twisted Edwards curves revisited." pp. 326-343 */
            if (a == field - 1 && BigInteger.Jacobi(field - a, field) == 1 && isComplete)
            {
                aroot = ModSqrtUtil.Sqrt(field - a, true);
                BigInteger ta = BarrettReducer.MultMod(aroot, aroot);

                BigInteger ma = BarrettReducer.InvMod(ta);
                BigInteger kt1 = BarrettReducer.MultMod(d, ma);

                kt = BarrettReducer.AddMod(kt1, kt1);
                computeOnTwist = true;
            }
        }

        /// <summary>
        /// Retrieves a standardized twisted Edwards curve by its enumeration type.
        /// </summary>
        /// <param name="type">The curve identifier from <see cref="TwistedEdwardsCurveType"/>.</param>
        /// <returns>A fully initialized twisted Edwards curve instance with domain parameters
        /// loaded from the internal named curve repository.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the specified curve type is not supported or maps to an invalid index.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This factory method provides access to standardized twisted Edwards curves
        /// used in modern cryptographic protocols:
        /// <list type="bullet">
        /// <item><description><see cref="TwistedEdwardsCurveType.Edwards25519"/>: The curve underlying Ed25519 signatures (RFC 8032).</description></item>
        /// <item><description><see cref="TwistedEdwardsCurveType.Edwards448"/>: The curve underlying Ed448 signatures (RFC 8032).</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The returned curve instances are pre-validated for:
        /// <list type="bullet">
        /// <item><description>Non-singularity condition: a*d*(a - d) != 0 (mod p)</description></item>
        /// <item><description>Cofactor multiple of 4 requirement for twisted Edwards curves</description></item>
        /// <item><description>Optimization pre-computation for a = -1 case (Hisil et al. formulas)</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// These curves implement complete addition laws and support the optimization <br/>
        /// techniques described in Hisil et al. (2008) "Twisted Edwards Curves Revisited", <br/>
        /// including quadratic twist operations when applicable.
        /// </para>
        /// </remarks>
        public static TwistedEdwardsCurve GetNamedCurve(TwistedEdwardsCurveType type)
        {
            BigInteger[] array = NamedCurve.GetNamedCurve((int)type);
            return new TwistedEdwardsCurve(array);
        }

        /// <summary>
        /// Evaluates the right-hand side of the twisted Edwards equation at a given y-coordinate.
        /// </summary>
        /// <param name="y">The y-coordinate to evaluate.</param>
        /// <returns>The value x^2 = (1 - y^2) / (a - d*(y^2)) mod p.</returns>
        public BigInteger Evaluate(BigInteger y)
        {
            BigInteger A1 = BarrettReducer.MultMod(y, y);
            BigInteger A2 = BarrettReducer.MultMod(d, A1);

            BigInteger A3 = BarrettReducer.SubMod(1, A1);
            BigInteger A4 = BarrettReducer.SubMod(a, A2);

            BigInteger A4i = BarrettReducer.InvMod(A4);
            BigInteger X2 = BarrettReducer.MultMod(A3, A4i);
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
                y = SecureRandom.Range(0, field - 1);
                temp = Evaluate(y);

                if (temp < 2)
                    return new ECPoint(temp, y);

                if (BigInteger.Jacobi(temp, field) == 1)
                {
                    done = true;
                    x = ModSqrtUtil.Sqrt(temp);

                    BigInteger eval = BarrettReducer.MultMod(x, x);
                    if (temp != eval) done = false;

                    if (done)
                    {
                        ECPoint tempPoint = new ECPoint(x, y);
                        basePoint = TwistedEdwardsMath.Multiply(this, cofactor, 
                            tempPoint, ECMode.EC_STANDARD_PROJECTIVE);
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
        /// <exception cref="ArgumentException">
        /// Thrown when the point does not satisfy the curve equation, or when multiplied
        /// by the cofactor it yields the point at infinity (indicating small-order subgroup).
        /// </exception>
        public void SetBasePoint(ECPoint point)
        {
            ECPoint tempPoint = point;
            var temp = Evaluate(tempPoint.GetAffineY());

            if (BigInteger.Jacobi(temp, field) != 1 && temp > 0)
                throw new ArgumentException(
                    "The generator point is not on the" 
                    + " twisted Edwards curve.");
            else
            {
                BigInteger x = tempPoint.GetAffineX();
                BigInteger eval = BarrettReducer.MultMod(x, x);

                if (eval != temp)
                    throw new ArgumentException(
                        "Invalid generator point for the" 
                        + " twisted Edwards curve.");
                else
                {
                    ECPoint testPoint = TwistedEdwardsMath.Multiply(this, cofactor, 
                        tempPoint, ECMode.EC_STANDARD_PROJECTIVE);

                    if (testPoint != ECPoint.POINT_INFINITY) 
                        basePoint = tempPoint;
                    else
                        throw new ArgumentException(
                            "Chosen generator point yields a"
                            + " small-order subgroup on the " 
                            + "twisted Edwards curve.");
                }
            }
        }
    }
}
