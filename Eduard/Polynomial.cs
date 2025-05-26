using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Eduard.Security
{
    /// <summary>
    /// Represents the modular univariate polynomials.
    /// </summary>
    //[DebuggerStepThrough]
    public sealed class Polynomial
    {
        /// <summary>
        /// Represents the degree of modular univariate polynomial.
        /// </summary>
        public int Degree;
        public BigInteger[] coeffs;
        private static RandomNumberGenerator rand = RandomNumberGenerator.Create();
        private static BigInteger constant;
        internal static BigInteger field;

        /// <summary>
        /// Creates an <seealso cref="Polynomial"/> value using integer value of 0.
        /// </summary>
        public Polynomial()
        {
            Degree = 0;
            coeffs = new BigInteger[1];
            coeffs[0] = 0;
        }

        /// <summary>
        /// Creates an <seealso cref="Polynomial"/> value with a specified degree.
        /// </summary>
        /// <param name="Degree"></param>
        public Polynomial(int Degree)
        {
            this.Degree = Degree;
            coeffs = new BigInteger[Degree + 1];

            for (int i = 0; i < coeffs.Length; i++)
                coeffs[i] = 0;
        }

        /// <summary>
        /// Creates an <seealso cref="Polynomial"/> from another polynomial value.
        /// </summary>
        /// <param name="poly"></param>
        public Polynomial(Polynomial poly)
        {
            Degree = poly.Degree;
            coeffs = new BigInteger[Degree + 1];

            for (int i = 0; i < coeffs.Length; i++)
                coeffs[i] = poly.coeffs[i];
        }

        /// <summary>
        /// Creates an <seealso cref="Polynomial"/> using specified coefficients.
        /// </summary>
        /// <param name="coeffs"></param>
        public Polynomial(params BigInteger[] coeffs)
        {
            Degree = coeffs.Length - 1;
            this.coeffs = new BigInteger[Degree + 1];
            List<BigInteger> list = new List<BigInteger>();

            list.AddRange(coeffs);
            list.Reverse();

            for (int i = 0; i <= Degree; i++)
                this.coeffs[i] = Reduce(list[i]);
        }

        public static void SetField(BigInteger field)
        {
            Polynomial.field = field;
            constant = BigInteger.BarrettConstant(field);
        }

        internal static BigInteger Reduce(BigInteger val)
        {
            BigInteger temp = val % field;

            if (temp < 0)
                temp += field;

            return temp;
        }

        internal void Update()
        {
            List<BigInteger> list = new List<BigInteger>();
            list.AddRange(coeffs);

            while (list[Degree] == 0 && list.Count > 1 && Degree != 0)
                Degree--;

            coeffs = new BigInteger[Degree + 1];

            for (int i = 0; i <= Degree; i++)
                coeffs[i] = list[i];
        }

        /// <summary>
        /// Adds two specified polynomials over an specified field.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Polynomial operator +(Polynomial left, Polynomial right)
        {
            int max = (left.Degree > right.Degree) ? left.Degree : right.Degree;
            Polynomial result = new Polynomial(max);

            for (int i = 0; i <= max; i++)
                result.coeffs[i] = AddMod(left.GetCoeff(i), right.GetCoeff(i));

            result.Update();
            return result;
        }

        /// <summary>
        /// Subtracts a specified <seealso cref="Polynomial"/> value from another specified <seealso cref="Polynomial"/> value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Polynomial operator -(Polynomial left, Polynomial right)
        {
            int max = (left.Degree > right.Degree) ? left.Degree : right.Degree;
            Polynomial result = new Polynomial(max);

            for (int i = 0; i <= max; i++)
                result.coeffs[i] = SubMod(left.GetCoeff(i), right.GetCoeff(i));

            result.Update();
            return result;
        }

        /// <summary>
        /// Multiplies two specified polynomials values over an specified field.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Polynomial operator *(Polynomial left, Polynomial right)
        {
            if (left == right) return Square(left);
            else return Multiply(left, right);
        }

        private static Polynomial Multiply(Polynomial left, Polynomial right)
        {
            int min = (left.Degree < right.Degree) ? left.Degree : right.Degree;

            if (min >= 256)
            {
                int deg = left.Degree + right.Degree;

                BigInteger[] coeffs = Core.fast_poly_mul(left.coeffs, right.coeffs, field);

                while (deg > 0 && coeffs[deg] == 0)
                    deg--;

                Polynomial res = new Polynomial(deg);

                for (int i = 0; i <= deg; i++)
                    res.coeffs[i] = coeffs[i];

                return res;
            }
            else
                return plain_mult(left, right);
        }

        private static Polynomial Square(Polynomial val)
        {
            if (val.Degree >= 256)
            {
                BigInteger[] coeffs = Core.fast_poly_sqr(val.coeffs, field);
                int deg = val.Degree << 1;

                while (deg > 0 && coeffs[deg] == 0)
                    deg--;

                Polynomial res = new Polynomial(deg);

                for (int i = 0; i <= deg; i++)
                    res.coeffs[i] = coeffs[i];

                return res;
            }
            else 
                return plain_square(val);
        }

        public static Polynomial Reduce(Polynomial x, Polynomial m)
        {
            int degm = m.Degree;
            int n = x.Degree;

            if(degm < 256 || n - degm < 256)
            {
                Polynomial r = x % m;
                return r;
            }

            BigInteger[] G = new BigInteger[n + 1];
            BigInteger[] R = new BigInteger[degm + 1];

            for (int i = 0; i <= n; i++)
                G[i] = x.coeffs[i];

            for (int j = 0; j <= degm; j++)
                R[j] = 0;
            

            if (!Core.fast_poly_rem(G, R, field))
            {
                SetPolyMod(m);
                Core.fast_poly_rem(G, R, field);
            }

            int deg = degm - 1;
            Polynomial res = new Polynomial(deg);

            for (int i = 0; i <= deg; i++)
                res.coeffs[i] = R[i];

            res.Update();
            return res;
        }

        public static void SetPolyMod(Polynomial poly)
        {
            int m, n = poly.Degree;
            if (n < 256) return;

            Polynomial h = new Polynomial(poly);
            h.Reverse();

            h = Invmodxn(h, n);
            h.Reverse();
            m = h.Degree;

            if (m < n - 1) 
                h = Mulxn(h, n - 1 - m);

            BigInteger[] f = new BigInteger[n + 1];
            BigInteger[] rf = new BigInteger[n + 1];

            for (int i = 0; i <= n; i++)
            {
                f[i] = poly.coeffs[i];
                rf[i] = (i <= m) ? h.coeffs[i] : 0;
            }

            Core.polymod_set(n, rf, f, field);
        }

        private void Reverse()
        {
            List<BigInteger> list = new List<BigInteger>();
            list.AddRange(coeffs);

            list.Reverse();
            this.coeffs = list.ToArray();
        }

        private static Polynomial plain_mult(Polynomial left, Polynomial right)
        {
            int degree = left.Degree + right.Degree;
            Polynomial result = new Polynomial(degree);

            for (int j = 0; j <= left.Degree; j++)
            {
                if (left.GetCoeff(j) == 0) continue;

                for (int k = 0; k <= right.Degree; k++)
                {
                    if (right.GetCoeff(k) == 0) continue;
                    result.coeffs[j + k] = AddMod(result.coeffs[j + k], MulMod(left.GetCoeff(j), right.GetCoeff(k)));
                }
            }

            result.Update();
            return result;
        }

        private static Polynomial plain_square(Polynomial poly)
        {
            int degree = 2 * poly.Degree;
            Polynomial result = new Polynomial(degree);

            for (int i = 0; i <= poly.Degree; i++)
                result.coeffs[2 * i] = MulMod(poly.GetCoeff(i), poly.GetCoeff(i));

            for(int j = 0; j < poly.Degree; j++)
            {
                if (poly.GetCoeff(j) == 0) continue;

                for(int k = j + 1; k <= poly.Degree; k++)
                {
                    BigInteger t = MulMod(poly.GetCoeff(j), poly.GetCoeff(k));
                    result.coeffs[j + k] = AddMod(result.coeffs[j + k], MulMod(2, t));
                }
            }

            result.Update();
            return result;
        }

        internal static Polynomial Mulxn(Polynomial poly, int words)
        {
            BigInteger[] coeffs = new BigInteger[poly.coeffs.Length + words];

            for(int k = 0; k < coeffs.Length; k++)
            {
                if (k < words)
                    coeffs[k] = 0;
                else
                    coeffs[k] = poly.coeffs[k - words];
            }

            Polynomial result = new Polynomial();
            result.coeffs = coeffs;

            result.Degree = coeffs.Length - 1;
            return result;
        }

        /// <summary>
        /// Returns the quotient value that results from division of two polynomials.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Polynomial operator /(Polynomial left, Polynomial right)
        {
            if (left.Degree < right.Degree)
                return new Polynomial(0);

            if(right.Degree == 0)
            {
                BigInteger vn = right.coeffs[right.Degree].Inverse(field);
                List<BigInteger> words = new List<BigInteger>();

                for (int i = 0; i <= left.Degree; i++)
                    words.Add(MulMod(left.coeffs[i], vn));

                words.Reverse();
                return new Polynomial(words.ToArray());
            }

            if (right.Degree == 1 && right.coeffs[0] == 0)
                throw new DivideByZeroException("The polynomial must be not equal with 0.");

            Polynomial quo, rem;
            Divide(left, right, out quo, out rem);
            return quo;
        }

        private static void Divide(Polynomial left, Polynomial right, out Polynomial quo, out Polynomial rem)
        {
            int m = left.Degree;
            int n = right.Degree;
            BigInteger inv = right.coeffs[right.Degree].Inverse(field);
            quo = new Polynomial(m - n);
            rem = new Polynomial(left);
            
            for (int k = m - n; k >= 0; k--)
            {
                quo.coeffs[k] = MulMod(rem.coeffs[n + k], inv);

                for(int j = n + k; j >= k; j--)
                    rem.coeffs[j] = SubMod(rem.coeffs[j], MulMod(quo.coeffs[k], right.coeffs[j - k]));
            }

            quo.Update();
            rem.Update();
        }

        /// <summary>
        /// Returns the remainder value that results from division of two polynomials.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Polynomial operator %(Polynomial left, Polynomial right)
        {
            if (left.Degree < right.Degree)
                return left;

            if (right.Degree == 0)
                return new Polynomial(0);

            if (right.Degree == 0 && right.coeffs[0] == 0)
                throw new DivideByZeroException("The polynomial must be not equal with 0.");

            Polynomial quo, rem;
            Divide(left, right, out quo, out rem);
            return rem;
        }

        private static int Window(BigInteger x, int i, ref int nbs, ref int nzs, int size)
        {
            int j, r, w;
            w = size;

            nbs = 1;
            nzs = 0;

            if (!x.TestBit(i)) return 0;
            if (i - w + 1 < 0) w = i + 1;

            r = 1;
            for (j = i - 1; j > i - w; j--)
            {
                nbs++;
                r *= 2;
                if (x.TestBit(j)) r += 1;

                if ((r & 0x3) == 0)
                {
                    r >>= 2;
                    nbs -= 2;
                    nzs = 2;
                    break;
                }
            }

            if ((r & 0x1) == 0)
            {
                r >>= 1;
                nzs = 1;
                nbs--;
            }

            return r;
        }

        public static Polynomial Pow(Polynomial val, BigInteger exponent, Polynomial mod)
        {
            Polynomial nb = new Polynomial(val);
            Polynomial result = 1;

            if (mod.Degree >= 16)
            {
                Polynomial[] table = new Polynomial[16];
                table[0] = nb % mod;

                Polynomial b2 = Reduce(table[0] * table[0], mod);
                // Creates table of odd powers.
                for (int i = 1; i < 16; i++)
                    table[i] = Reduce(table[i - 1] * b2, mod);

                int bits = exponent.GetBits();
                int nbw = 0, nzs = 0;

                for (int i = bits - 1; i > -1;)
                {
                    int n = Window(exponent, i, ref nbw, ref nzs, 5);

                    for (int j = 0; j < nbw; j++)
                        result = Reduce(result * result, mod);

                    if (n != 0)
                        result = Reduce(result * table[n >> 1], mod);
                    i -= nbw;
                    if (nzs != 0)
                    {
                        for (int j = 0; j < nzs; j++)
                            result = Reduce(result * result, mod);
                        i -= nzs;
                    }
                }
            }
            else
            {
                int size = exponent.GetBits();

                for(int k = 0; k < size; k++)
                {
                    if (exponent.TestBit(k))
                    {
                        Polynomial temp = nb * result;
                        result = temp % mod;
                    }

                    nb = nb * nb;
                    nb %= mod;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the greatest common divisor of two polynomials over specified field.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Polynomial Gcd(Polynomial left, Polynomial right)
        {
            Polynomial a, b;

            if (left == 0 && right != 0)
                return right;

            if (left != 0 && right == 0)
                return left;

            if (left == 1 || right == 1)
                return 1;

            if(left.Degree < right.Degree)
            {
                a = new Polynomial(right);
                b = new Polynomial(left);
            }
            else
            {
                a = new Polynomial(left);
                b = new Polynomial(right);
            }

            while(b != 0)
            {
                Polynomial r = a % b;
                a = new Polynomial(b);
                b = new Polynomial(r);
            }

            BigInteger inv = a.coeffs[a.Degree].Inverse(field);
            a *= inv;
            return a;
        }

        /// <summary>
        /// Evaluates the polynomial in the specified value over specified field.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public BigInteger Horner(BigInteger x)
        {
            BigInteger sum = coeffs[0];
            BigInteger val = 1;

            for(int k = 1; k <= Degree; k++)
            {
                val = MulMod(val, x);
                BigInteger test = MulMod(val, coeffs[k]);
                sum = AddMod(sum, test);
            }

            return sum;
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

                if (s == r)
                    return 0;

                t = BigInteger.Pow(y, (long)Math.Pow(2, r - s - 1), field);
                y = (t * t) % field;
                x = (x * t) % field;
                b = (b * y) % field;
                r = s;
            }
        }

        internal static BigInteger Sqrt(BigInteger val, BigInteger field)
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

        private Polynomial FindFactor()
        {
            while(true)
            {
                BigInteger a = BigInteger.Next(rand, 1, field - 1);
                Polynomial g = new Polynomial(1, a);
                Polynomial h = Pow(g, (field - 1) / 2, this);
                Polynomial poly = h + 1;

                if (poly.Degree == 0) continue;

                Polynomial factor = Gcd(poly, this);

                if (factor.Degree != 0)
                    return factor;
            }
        }

        /// <summary>
        /// Returns the roots of small degree polynomial.
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="roots"></param>
        /// <returns></returns>
        public static int Solve(Polynomial poly, ref List<BigInteger> roots)
        {
            if (poly.Degree > 2)
                return 0;

            if(poly.Degree == 1)
            {
                BigInteger inv = poly.coeffs[1].Inverse(field);
                BigInteger b = field - poly.coeffs[0];
                BigInteger root = MulMod(b, inv);

                roots.Add(root);
                return 1;
            }
            
            if(poly.Degree == 2)
            {
                BigInteger sb = MulMod(poly.coeffs[1], poly.coeffs[1]);
                BigInteger ac4 = MulMod(field - 4, poly.coeffs[2]);

                ac4 = MulMod(ac4, poly.coeffs[0]);
                BigInteger delta = AddMod(sb, ac4);

                BigInteger root = Sqrt(delta, field);
                BigInteger val = MulMod(2, poly.coeffs[2]);
                BigInteger inv = val.Inverse(field);
                BigInteger t = AddMod(field - poly.coeffs[1], root);
                t = MulMod(inv, t);

                roots.Add(t);

                t = AddMod(field - poly.coeffs[1], field - root);
                t = MulMod(inv, t);

                roots.Add(t);
                return 1;
            }

            return -1;
        }

        /// <summary>
        /// Finds the <seealso cref="Polynomial"/> roots in a specified field.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="roots"></param>
        public void FindRoots(ref List<BigInteger> roots)
        {
            Polynomial self = new Polynomial(this);
            Polynomial aux = new Polynomial(1, 0);
            Polynomial vtemp = Pow(aux, field, self);
            vtemp -= aux;

            Polynomial poly = Gcd(vtemp, self);

            if (poly == 1)
                return;

            if (poly.Degree >= 1 && poly != 0 && poly != this)
                self = new Polynomial(poly);

            while (roots.Count == 0)
            {
                Polynomial temp = self.FindFactor();
                int ID = Solve(temp, ref roots);

                if (ID == 0)
                {
                    self = new Polynomial(temp);
                    continue;
                }
            }
        }

        public static Polynomial Divxn(Polynomial poly, int degn)
        {
            if (poly.Degree < degn) return 0;
            Polynomial result = new Polynomial();
            result.coeffs = new BigInteger[poly.Degree - degn + 1];

            for (int k = degn; k <= poly.Degree; k++)
                result.coeffs[k - degn] = poly.GetCoeff(k);

            result.Degree = poly.Degree - degn;
            result.Update();
            return result;
        }

        public static Polynomial Modxn(Polynomial poly, int degn)
        {
            if (poly.Degree < degn) return poly;
            Polynomial result = new Polynomial();
            result.coeffs = new BigInteger[degn];

            for (int k = 0; k < degn; k++)
                result.coeffs[k] = poly.GetCoeff(k);

            result.Degree = degn - 1;
            result.Update();
            return result;
        }

        public static Polynomial Modxn_l(Polynomial poly, int degn)
        {
            if (poly.Degree < degn) return poly;
            Polynomial result = new Polynomial(degn - 1);

            for (int k = 0; k + degn <= poly.Degree; k++)
                result.coeffs[k] = AddMod(poly.GetCoeff(k), poly.GetCoeff(k + degn));

            result.Update();
            return result;
        }

        internal static Polynomial Invmodxn(Polynomial poly, int degn)
        {
            int k = 0;
            Polynomial result = new Polynomial(poly.GetCoeff(0).Inverse(field));
            while ((1 << k) < degn) k++;

            for (int i = 1; i <= k; i++)
                result = Modxn(2 * result - poly * result * result, 1 << i);

            result = Modxn(result, degn);
            return result;
        }

        internal BigInteger Min()
        {
            for(int k = coeffs.Length - 1; k >= 0; k--)
            {
                if (coeffs[k] != 0)
                    return coeffs[k];
            }

            return 0;
        }

        /// <summary>
        /// Differentiates a specified <seealso cref="Polynomial"/> over specified field.
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static Polynomial Differentiate(Polynomial poly, BigInteger field)
        {
            Polynomial diff = new Polynomial(poly.Degree - 1);

            for (int k = 1; k <= poly.Degree; k++)
                diff.coeffs[k - 1] = MulMod(k, poly.coeffs[k]);

            return diff;
        }

        /// <summary>
        /// Determines whether two polynomials have the same value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Polynomial left, Polynomial right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two polynomials have not the same value.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Polynomial left, Polynomial right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Determines whether two polynomials have the same value.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, (Polynomial)obj))
                return true;

            Polynomial right = (Polynomial)obj;
            int degree = (this.Degree > right.Degree) ? this.Degree : right.Degree;

            for (int i = 0; i <= degree; i++)
            {
                if (this.GetCoeff(i) != right.GetCoeff(i))
                    return false;
            }

            return true;
        }

        internal static BigInteger AddMod(BigInteger left, BigInteger right)
        {
            BigInteger result = left + right;

            if (result >= field)
                result -= field;

            return result;
        }

        internal static BigInteger SubMod(BigInteger left, BigInteger right)
        {
            BigInteger result = left - right;

            if (result < 0)
                result += field;

            return result;
        }

        internal static BigInteger MulMod(BigInteger left, BigInteger right)
        {
            BigInteger result = BigInteger.BarrettReduction(left * right, field, constant);
            return result;
        }

        public BigInteger GetCoeff(int index)
        {
            if (index > Degree)
                return 0;
            else
                if (index >= 0 && index <= Degree)
                return coeffs[index];
            else
                throw new IndexOutOfRangeException("Index out of range.");
        }

        public static implicit operator Polynomial(int val)
        {
            return new Polynomial((BigInteger)val);
        }

        public static implicit operator Polynomial(BigInteger val)
        {
            return new Polynomial(val);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return coeffs.GetHashCode();
        }
    }
}
