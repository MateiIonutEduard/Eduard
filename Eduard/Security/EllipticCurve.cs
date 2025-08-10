using System.Security.Cryptography;
#pragma warning disable

namespace Eduard.Security
{
    /// <summary>
    /// Represents an elliptic curve given in Weierstrass form.
    /// </summary>
    public sealed class EllipticCurve
    {
        public BigInteger a, b;
        public BigInteger field, order;

        private static RandomNumberGenerator rand;
        private static bool enableSpeedup;

        /// <summary>
        /// Creates a Weierstrass elliptic curve with random coefficients.
        /// </summary>
        /// <param name="bits"></param>
        public EllipticCurve(int bits)
        {
            rand = RandomNumberGenerator.Create();
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

            while (check == 0)
            {
                b = BigInteger.Next(rand, 1, field - 1);
                B2 = (b * b) % field;

                val = (27 * B2) % field;
                check = (temp + val) % field;
            }
        }

        /// <summary>
        /// Creates a Weierstrass elliptic curve with specific coefficients.
        /// </summary>
        /// <param name="args"></param>
        public EllipticCurve(params BigInteger[] args)
        {
            if (args.Length > 4)
                throw new ArgumentException("Too many arguments.");

            rand = RandomNumberGenerator.Create();
            a = args[0];
            b = args[1];

            field = args[2];
            order = args[3];

            enableSpeedup = ModSqrtUtil.CanSpeedup(field);
            ModSqrtUtil.InitParams(field);
        }

        /// <summary>
        /// Evaluate the Weierstrass elliptic curve equation at the x-coordinate.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public BigInteger Evaluate(BigInteger x)
        {
            BigInteger result = (x * x) % field;
            result = (result * x) % field;
            BigInteger temp = (a * x + b) % field;
            result = (result + temp) % field;
            return result;
        }

        /// <summary>
        /// Encodes the binary message as a point on the elliptic curve using Koblitz's point compression algorithm.
        /// </summary>
        /// <param name="m">Represents the given binary message as a big integer.</param>
        /// <param name="r">Represents the number of iterations for Koblitz's algorithm.</param>
        /// <returns></returns>
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
        /// Decodes the binary message using the corresponding point on the elliptic curve.
        /// </summary>
        /// <param name="point">Represents the point on the elliptic curve that corresponds to the binary message.</param>
        /// <param name="r">Represents the number of iterations for Koblitz's algorithm.</param>
        /// <returns></returns>
        public BigInteger GetMessage(ECPoint point, int r=30)
        {
            if (point == ECPoint.POINT_INFINITY) return -1;
            BigInteger steps = r;

            BigInteger m = point.GetAffineX() - 1;
            return m / steps;
        }

        /// <summary>
        /// Represents a randomly chosen base point on the elliptic curve.
        /// </summary>
        public ECPoint BasePoint
        {
            get
            {
                bool done = false;
                BigInteger x = 0;

                BigInteger y = 0;
                BigInteger temp = 0;

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
                    }
                }
                while (!done);

                return new ECPoint(x, y);
            }
        }

        private BigInteger Sqrt(BigInteger val, bool forceOutput = false)
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
