﻿using System;
using System.Security.Cryptography;

namespace Eduard
{
    /// <summary>
    /// Represents an elliptic curve.
    /// </summary>
    public sealed class EllipticCurve
    {
        public BigInteger a, b;
        public BigInteger field, order;
        private static RandomNumberGenerator rand;

        /// <summary>
        /// Creates an elliptic curve with randomly coefficients.
        /// </summary>
        /// <param name="bits"></param>
        public EllipticCurve(int bits)
        {
            rand = RandomNumberGenerator.Create();
            field = BigInteger.GenProbablePrime(rand, bits, 50);
            a = BigInteger.Next(rand, 1, field - 1);

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
        /// Creates an elliptic curve with specified coefficients.
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
        }

        /// <summary>
        /// Evaluates the Weierstrass equation of the elliptic curve in the specified value.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public BigInteger Eval(BigInteger x)
        {
            BigInteger result = (x * x) % field;
            result = (result * x) % field;
            BigInteger temp = (a * x + b) % field;
            result = (result + temp) % field;
            return result;
        }

        /// <summary>
        /// Represents a base point on the elliptic curve.
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
                    temp = Eval(x);

                    if (temp < 2)
                        return new ECPoint(x, temp);

                    if (BigInteger.Jacobi(temp, field) == 1)
                    {
                        done = true;
                        y = TonelliShanks(temp, field);
                        BigInteger eval = (y * y) % field;

                        if (temp != eval)
                            done = false;
                    }
                }
                while (!done);

                return new ECPoint(x, y);
            }
        }

        private static BigInteger TonelliShanks(BigInteger val, BigInteger field)
        {
            long e = 0, r, s;
            BigInteger b = 0, bp = 0, q = field - 1, n = 0;
            BigInteger t = 0, x = 0, y = 0, z = 0;

            while ((q & 1) == 0)
            {
                e++;
                q >>= 1;
            }

            // Find a generator.

            int JSymbol = 0;

            do
            {
                n = BigInteger.Next(rand, 2, field - 1);
                JSymbol = BigInteger.Jacobi(n, field);
            } while (JSymbol == -1);

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

                if (s == r)
                    return 0; // Has failed !

                t = BigInteger.Pow(y, (long)Math.Pow(2, r - s - 1), field);
                y = (t * t) % field;
                x = (x * t) % field;
                b = (b * y) % field;
                r = s;
            }
        }

        /// <summary>
        /// Returns modular square root of the specified value.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static BigInteger Sqrt(BigInteger val, BigInteger field)
        {
            if ((field & 3) == 3)
                return BigInteger.Pow(val, (field + 1) >> 2, field);

            BigInteger root = 0;
            BigInteger delta = ((field - 4) * (field - val)) % field;
            BigInteger temp = 1;
            BigInteger qnr = 0;
            BigInteger buf = 0;
            int ID = 1;

            switch (ID)
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
