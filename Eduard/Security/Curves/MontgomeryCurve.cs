using System;
using System.Diagnostics;
using Eduard.Security.Primitives;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Represents an elliptic curve in Montgomery form over a prime field Fp.
    /// </summary>
    /// <remarks>
    /// Montgomery curves are widely used in cryptographic protocols like X25519 (Curve25519) due to their <br/>
    /// efficient constant-time scalar multiplication using the Montgomery ladder. Pre-computed values <br/>
    /// A24 = (A + 2)/4 and 1/B are cached for optimized point operations.
    /// </remarks>
#if !USE_PROFILER
    [DebuggerStepThrough]
#endif
    public sealed class MontgomeryCurve
    {
        /// <summary>
        /// Prime field modulus p and curve order.
        /// </summary>
        public BigInteger field, order;

        /// <summary>
        /// Curve coefficients A and B from the Montgomery equation.
        /// </summary>
        public BigInteger A, B, BInv, A24;

        /// <summary>
        /// Cofactor h = #E(Fp)/order, for Montgomery curves.
        /// </summary>
        public BigInteger cofactor;

        private ECPoint basePoint;
        private static bool enableSpeedup;

        /// <summary>
        /// Initializes a Montgomery curve with explicit parameters.
        /// </summary>
        /// <param name="args">Parameter array: [A, B, field, order, cofactor]</param>
        /// <exception cref="ArgumentException">Thrown when more than 5 parameters are provided.</exception>
        /// <remarks>
        /// Pre-computes:
        /// <list type="bullet">
        /// <item><description>1/B = modular inverse of B for equation evaluation</description></item>
        /// <item><description>A24 = (A + 2) / 4 mod p for Montgomery ladder steps</description></item>
        /// </list>
        /// </remarks>
        public MontgomeryCurve(params BigInteger[] args)
        {
            if (args.Length > 5)
                throw new ArgumentException("Too many arguments.");

            A = args[0];
            B = args[1];

            field = args[2];
            order = args[3];

            basePoint = ECPoint.POINT_INFINITY;
            cofactor = args[4];

            BarrettReducer.SetModulus(field);
            bool isValid = ValidateDiscriminant();

            if ((cofactor & 0x3) != 0 || !isValid)
                throw new InvalidOperationException(
                    "The Montgomery curve is " +
                    "singular or invalid.");

            BInv = BarrettReducer.InvMod(B);
            BigInteger t = BarrettReducer.InvMod(4);
            BigInteger At = BarrettReducer.AddMod(A, 2);

            A24 = BarrettReducer.MultMod(At, t);
            ModSqrtUtil.InitParams();
        }

        /// <summary>
        /// Validates the curve discriminant to ensure non-singularity.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the discriminant is valid and the curve is non-singular;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// A valid discriminant guarantees that the Montgomery curve has no singular points, <br/>
        /// ensuring the Montgomery ladder computes correct scalar multiples for all inputs.
        /// </remarks>
        internal bool ValidateDiscriminant()
        {
            BigInteger t1 = BarrettReducer.MultMod(A, A);
            BigInteger t2 = BarrettReducer.SubMod(t1, 4);

            BigInteger discriminant = BarrettReducer.MultMod(t1, t2);
            return discriminant != 0;
        }

        /// <summary>
        /// Creates a Montgomery-form elliptic curve instance using standardized domain parameters from named curves.
        /// </summary>
        /// <param name="type">The curve type identifier from <see cref="MontyCurveType"/> enumeration.</param>
        /// <returns>An initialized <see cref="MontgomeryCurve"/> instance in Montgomery form with the specified curve parameters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the curve type corresponds to an invalid or unsupported curve index.
        /// </exception>
        /// <remarks>
        /// This factory method provides access to standardized elliptic curves in Montgomery form including:
        /// <list type="bullet">
        /// <item><description>Curve25519 (X25519): Montgomery form of Edwards25519</description></item>
        /// <item><description>Curve448 (X448): Montgomery form of Edwards448</description></item>
        /// </list>
        /// The returned curve pre-computes the essential Montgomery ladder constants. <br/>
        /// These curves are primarily designed for constant-time scalar multiplication using <br/>the
        /// Montgomery ladder, making them ideal for Diffie-Hellman key exchange protocols <br/>like
        /// X25519 and X448. The factory method ensures proper initialization of all curve <br/>
        /// parameters and cached values required for efficient point operations.
        /// </remarks>
        public static MontgomeryCurve GetNamedCurve(MontyCurveType type)
        {
            BigInteger[] array = NamedCurve.GetNamedCurve((int)type);
            return new MontgomeryCurve(array);
        }

        /// <summary>
        /// Evaluates the right-hand side of the Montgomery equation at x.
        /// </summary>
        /// <param name="x">The x-coordinate to evaluate.</param>
        /// <returns>The value y^2 = (x^3 + A * x^2 + x) / B (mod p).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when x is negative or exceeds or equals field modulus.
        /// </exception>
        public BigInteger Evaluate(BigInteger x)
        {
            if (x < 0 || x >= field)
                throw new ArgumentOutOfRangeException(nameof(x),
                    "Coordinate must be in the range [0, p-1].");

            BigInteger X2 = BarrettReducer.MultMod(x, x);
            BigInteger res = BarrettReducer.MultMod(x, X2);

            BigInteger tx = BarrettReducer.MultMod(A, X2);
            tx = BarrettReducer.AddMod(tx, x);

            res = BarrettReducer.AddMod(res, tx);
            return BarrettReducer.MultMod(res, BInv);
        }
    }
}
