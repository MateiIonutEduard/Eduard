using System;
using Eduard;
using System.Diagnostics;
using Eduard.Security.Primitives;
using MSCrypto = System.Security.Cryptography;

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

        private static MSCrypto.RandomNumberGenerator rand;
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

            rand = MSCrypto.RandomNumberGenerator.Create();
            A = args[0];
            B = args[1];

            field = args[2];
            order = args[3];

            basePoint = ECPoint.POINT_INFINITY;
            cofactor = args[4];

            BInv = B.Inverse(field);
            BarrettReducer.SetModulus(field);

            BigInteger t = new BigInteger(4).Inverse(field);
            BigInteger At = BarrettReducer.AddMod(A, 2);
            A24 = BarrettReducer.MulMod(At, t);

            enableSpeedup = ModSqrtUtil.CanSpeedup(field);
            ModSqrtUtil.InitParams(field);
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
        /// Evaluates the right-hand side of the Montgomery equation at a given x-coordinate.
        /// </summary>
        /// <param name="x">The x-coordinate to evaluate.</param>
        /// <returns>The value y^2 = (x^3 + A * x^2 + x) / B (mod p).</returns>
        public BigInteger Evaluate(BigInteger x)
        {
            BigInteger X2 = BarrettReducer.MulMod(x, x);
            BigInteger res = BarrettReducer.MulMod(x, X2);

            BigInteger tx = BarrettReducer.MulMod(A, X2);
            tx = BarrettReducer.AddMod(tx, x);

            res = BarrettReducer.AddMod(res, tx);
            return BarrettReducer.MulMod(res, BInv);
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
