﻿using System.Security.Cryptography;
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
            InitParams();

            BigInteger temp = (a * a) % field;
            temp = (temp * a) % field;
            temp <<= 2;

            b = BigInteger.Next(rand, 1, field - 1);
            BigInteger val = (27 * (b * b)) % field;
            BigInteger check = (temp + val) % field;

            while (check == 0)
            {
                b = BigInteger.Next(rand, 1, field - 1);
                val = (27 * (b * b)) % field;
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
            InitParams();
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

        private void InitParams(int windowSize = 4)
        {
            /* check whether optimizations can be used */
            enableSpeedup = CanSpeedup(field);

            if (enableSpeedup)
                OptimizedRotaruIftene.Precompute(rand, field, windowSize);
        }

        private bool CanSpeedup(BigInteger field)
        {
            int m = field.GetBits();
            BigInteger order = field - 1;
            int s = 0;

            while ((order & 1) == 0)
            {
                order >>= 1;
                s++;
            }

            long left = s * (long)(s - 1);
            long right = 8L * m + 20L;
            return left > right;
        }

        private BigInteger Sqrt(BigInteger val, bool forceOutput = false)
        {
            /* compute the modular square root using the optimized Rotaru-Iftene method */
            if (enableSpeedup)
                return OptimizedRotaruIftene.Sqrt(val);

            /* if the correct output is required, the algorithm will solve random quadratic equations to find the real root */
            if (forceOutput) return Sqrt(val, field);

            /* uses the standard Tonelli-Shanks algorithm to obtain the modular square root */
            return TonelliShanks(val, field);
        }

        private BigInteger TonelliShanks(BigInteger val, BigInteger field)
        {
            long e = 0, r, s;
            BigInteger b = 0, bp = 0, q = field - 1, n = 0;
            BigInteger t = 0, x = 0, y = 0, z = 0;

            while ((q & 1) == 0)
            {
                e++;
                q >>= 1;
            }

            /* find a generator */
            int JSymbol = 0;

            do
            {
                n = BigInteger.Next(rand, 2, field - 1);
                JSymbol = BigInteger.Jacobi(n, field);
            } while (JSymbol != -1);

            z = BigInteger.Pow(n, q, field);
            y = z;
            r = e;

            x = BigInteger.Pow(val, (q - 1) / 2, field);
            b = (((val * x) % field) * x) % field;
            x = (val * x) % field;

            while (true)
            {
                if (b == 1 || b == field - 1)
                    return x;

                s = 1;

                do
                {
                    bp = BigInteger.Pow(b, (long)Math.Pow(2, s), field);
                    s++;
                } while (bp != 1 && bp != field - 1 && s < r);

                /* has failed */
                if (s == r) return 0;
                t = BigInteger.Pow(y, (long)Math.Pow(2, r - s - 1), field);
                y = (t * t) % field;

                x = (x * t) % field;
                b = (b * y) % field;
                r = s;
            }
        }

        private BigInteger Sqrt(BigInteger val, BigInteger field)
        {
            if ((field & 3) == 3)
                return BigInteger.Pow(val, (field + 1) >> 2, field);

            BigInteger root = 0;
            BigInteger delta = ((field - 4) * (field - val)) % field;

            BigInteger temp = 1;
            BigInteger qnr = 0;

            BigInteger buf = 0;
            int uid = 1;

            switch (uid)
            {
                case 1:

                    root = TonelliShanks(val, field);

                    if (val == (root * root) % field)
                        return root;

                    goto case 2;

                case 2:

                    qnr = BigInteger.Next(rand, 2, field - 1);

                    if (BigInteger.Jacobi(qnr, field) != -1)
                        goto case 2;

                    BigInteger square = (qnr * qnr) % field;
                    delta = (delta * square) % field;

                    temp = (temp * qnr) % field;
                    buf = TonelliShanks(delta, field);

                    if (delta != (buf * buf) % field)
                        goto case 2;
                    goto case 3;

                case 3:

                    BigInteger vtemp = (2 * temp) % field;
                    BigInteger inv = vtemp.Inverse(field);

                    root = (buf * inv) % field;
                    break;
            }

            return root;
        }
    }
}
