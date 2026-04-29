using System;
using System.Diagnostics;
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

        /// <summary>
        /// Initializes a random Weierstrass curve with a prime field of specified bit length.
        /// </summary>
        /// <param name="bits">Bit length of the prime field.</param>
        public EllipticCurve(int bits)
        {
            field = SecureRandom.GenProbablePrime(bits, 50);
            BarrettReducer.SetModulus(field);

            a = SecureRandom.Range(1, field - 1);
            ModSqrtUtil.InitParams();

            BigInteger temp = BarrettReducer.MultMod(a, a);
            temp = BarrettReducer.MultMod(temp, a);
            temp = BarrettReducer.MultMod(4, temp);

            b = SecureRandom.Range(1, field - 1);
            BigInteger B2 = BarrettReducer.MultMod(b, b);

            BigInteger val = BarrettReducer.MultMod(27, B2);
            BigInteger check = BarrettReducer.AddMod(temp, val);

            order = 1; cofactor = 1;
            basePoint = ECPoint.POINT_INFINITY;

            while (check == 0)
            {
                b = SecureRandom.Range(1, field - 1);
                B2 = BarrettReducer.MultMod(b, b);

                val = BarrettReducer.MultMod(27, B2);
                check = BarrettReducer.AddMod(temp, val);
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

            a = args[0];
            b = args[1];

            field = args[2]; order = args[3];
            basePoint = ECPoint.POINT_INFINITY;
            cofactor = args[4];

            BarrettReducer.SetModulus(field);
            BigInteger A2 = BarrettReducer.MultMod(a, a);

            BigInteger B2 = BarrettReducer.MultMod(b, b);
            BigInteger delta = BarrettReducer.MultMod(a, A2);

            delta = BarrettReducer.MultMod(4, delta);
            BigInteger val = BarrettReducer.MultMod(27, B2);
            delta = BarrettReducer.AddMod(delta, val);

            if (delta == 0)
                throw new InvalidOperationException(
                    "Invalid curve: singular Weierstrass form.");

            ModSqrtUtil.InitParams();
        }

        /// <summary>
        /// Creates a Weierstrass-form elliptic curve instance using standardized domain parameters from named curves.
        /// </summary>
        /// <param name="type">The curve type identifier from <see cref="WeiCurveType"/> enumeration.</param>
        /// <returns>An initialized <see cref="EllipticCurve"/> instance in short Weierstrass form with the specified curve parameters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the curve type corresponds to an invalid or unsupported curve index.
        /// </exception>
        /// <remarks>
        /// This factory method provides access to standardized elliptic curves in short Weierstrass form including:
        /// <list type="bullet">
        /// <item><description>NIST curves: P-192, P-224, P-256, P-384, P-521</description></item>
        /// <item><description>Weierstrass form of Montgomery curves: W-25519, W-448</description></item>
        /// </list>
        /// For Montgomery or twisted Edwards native forms, use the respective curve classes. <br/>
        /// The returned curve includes proper validation of non-singularity and initializes <br/>
        /// optimized modular arithmetic components for the specific prime field.
        /// </remarks>
        public static EllipticCurve GetNamedCurve(WeiCurveType type)
        {
            BigInteger[] array = NamedCurve.GetNamedCurve((int)type);
            return new EllipticCurve(array);
        }

        /// <summary>
        /// Evaluates the right-hand side of the Weierstrass equation at x.
        /// </summary>
        /// <param name="x">The x-coordinate to evaluate.</param>
        /// <returns>y^2 = x^3 + ax + b (mod p).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when x is negative or exceeds or equals field modulus.
        /// </exception>
        public BigInteger Evaluate(BigInteger x)
        {
            if (x < 0 || x >= field)
                throw new ArgumentOutOfRangeException(nameof(x),
                    "Coordinate must be in the range [0, p-1].");

            BigInteger result = BarrettReducer.MultMod(x, x);
            result = BarrettReducer.MultMod(result, x);

            BigInteger ax = BarrettReducer.MultMod(a, x);
            BigInteger temp = BarrettReducer.AddMod(ax, b);

            result = BarrettReducer.AddMod(result, temp);
            return result;
        }

        /// <summary>
        /// Encodes a message as a curve point using Koblitz's method.
        /// </summary>
        /// <param name="m">Represents a binary message as a large integer.</param>
        /// <param name="r">Iterations (default: 30).</param>
        /// <returns>Point encoding the message.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the message is negative or exceeds the maximum encodable value for this field.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the algorithm fails to find a suitable curve point within the specified iterations.
        /// </exception>
        public ECPoint GetPoint(BigInteger m, int r = 30)
        {
            if (m < 0)
                throw new ArgumentException(
                    "The message to encode "
                    + "cannot be negative.",
                    nameof(m));

            /* if the product exceeds the value of the prime field, the algorithm fails */
            if (m * r >= field - r + 1)
                throw new ArgumentException(
                    "The message is too large to encode within"
                    + " the specified number of iterations.",
                    nameof(m));

            BigInteger xs = BarrettReducer.MultMod(m, r);
            BigInteger ys = 1;

            int ks = 0;
            xs++;

            if (xs >= field)
                xs -= field;

            while (ks < r)
            {
                BigInteger t = Evaluate(xs);

                if (BigInteger.Jacobi(t, field) == 1)
                {
                    ys = ModSqrtUtil.Sqrt(t, true);
                    return new ECPoint(xs, ys);
                }

                xs++;
                ks++;

                if (xs >= field)
                    xs -= field;
            }

            throw new InvalidOperationException(
                "Failed to encode the message: no suitable "
                + "curve point found within the iteration limit.");
        }

        /// <summary>
        /// Decodes the original message from a Koblitz-encoded point.
        /// </summary>
        /// <param name="point">The encoded point.</param>
        /// <param name="r">Iterations used during encoding (default: 30).</param>
        /// <returns>The original message m.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the point is at infinity or does not lie on the Weierstrass curve.
        /// </exception>
        public BigInteger GetMessage(ECPoint point, int r=30)
        {
            if (point == ECPoint.POINT_INFINITY)
                throw new ArgumentException(
                    "The encoded point cannot "
                    + "be the point at infinity.",
                    nameof(point));

            BigInteger x = point.GetAffineX();
            BigInteger y = point.GetAffineY();

            if (x < 0 || x >= field || y < 0 || y >= field)
                throw new ArgumentException(
                    "The specified point contains coordinates "
                    + "outside the field range [0, p-1].",
                    nameof(point));

            BigInteger Y2 = BarrettReducer.MultMod(y, y);
            BigInteger eval = Evaluate(x);

            if (eval != Y2)
                throw new ArgumentException(
                    "The specified point does "
                    + "not satisfy the Weierstrass "
                    + "curve equation.",
                    nameof(point));

            BigInteger steps = r;
            BigInteger m = x - 1;

            return m / steps;
        }

        /// <summary>
        /// Gets the curve's base point (generator), either from cache or by finding a new valid point.
        /// </summary>
        /// <param name="useCached">
        /// If <c>true</c>, returns the cached base point when available.
        /// If <c>false</c>, always finds a new base point via random search.
        /// </param>
        /// <param name="skipValidation">
        /// If <c>true</c>, bypasses subgroup validation. Use only for number theory applications.
        /// Default is <c>false</c>.
        /// </param>
        /// <returns>
        /// A point suitable as a generator. When <paramref name="skipValidation"/> is <c>false</c>,
        /// the point is guaranteed to lie in the prime-order subgroup.
        /// </returns>
        public ECPoint GetBasePoint(bool useCached = false, bool skipValidation = false)
        {
            bool done = false;
            BigInteger x = 0;

            BigInteger y = 0;
            BigInteger temp = 0;

            if (useCached && !skipValidation && basePoint != ECPoint.POINT_INFINITY)
                return basePoint;

            do
            {
                x = SecureRandom.Range(0, field - 1);
                temp = Evaluate(x);

                if (temp < 2)
                    return new ECPoint(x, temp);

                if (BigInteger.Jacobi(temp, field) == 1)
                {
                    done = true;
                    y = ModSqrtUtil.Sqrt(temp);

                    BigInteger eval = BarrettReducer.MultMod(y, y);
                    if (temp != eval) done = false;

                    if (done)
                    {
                        ECPoint tempPoint = new ECPoint(x, y);

                        if (skipValidation)
                            return tempPoint;
                        else
                        {
                            basePoint = ECMath.Multiply(this, cofactor,
                                tempPoint, ECMode.EC_STANDARD_PROJECTIVE);
                            done = (basePoint != ECPoint.POINT_INFINITY);
                        }
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
        /// Thrown when the point is at infinity, does not lie on the Weierstrass curve, maps to the
        /// quadratic twist, or when multiplied by the cofactor yields the point at infinity
        /// (indicating it lies in a small-order subgroup).
        /// </exception>
        public void SetBasePoint(ECPoint point)
        {
            if (point == ECPoint.POINT_INFINITY)
                throw new ArgumentException(
                    "The generator point cannot " 
                    + "be the point at infinity.",
                    nameof(point));

            ECPoint tempPoint = point;
            var Y2 = Evaluate(tempPoint.GetAffineX());

            if (BigInteger.Jacobi(Y2, field) != 1 && Y2 > 0)
                throw new ArgumentException(
                    "The specified point does not" 
                    + " lie on the Weierstrass curve.",
                    nameof(point));
            else
            {
                BigInteger y = tempPoint.GetAffineY();
                BigInteger eval = BarrettReducer.MultMod(y, y);

                if (eval != Y2)
                    throw new ArgumentException(
                        "The specified point does " + 
                        "not satisfy the Weierstrass" 
                        + " curve equation.",
                        nameof(point));
                else
                {
                    ECPoint testPoint = ECMath.Multiply(this, cofactor, 
                        tempPoint, ECMode.EC_STANDARD_PROJECTIVE);

                    if (testPoint != ECPoint.POINT_INFINITY) 
                        basePoint = tempPoint;
                    else
                        throw new ArgumentException(
                            "The specified point lies in a " + 
                            "small-order subgroup and cannot be " 
                            + "used as a cryptographic generator.",
                            nameof(point));
                }
            }
        }
    }
}
