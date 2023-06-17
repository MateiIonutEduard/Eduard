using System;

namespace Eduard
{
    internal class PolyMod
    {
        internal Polynomial poly;
        private static Polynomial mod;

        internal PolyMod()
        { poly = new Polynomial(); }

        internal PolyMod(Polynomial poly)
        { this.poly = poly % mod; }

        internal PolyMod(PolyMod poly)
        { this.poly = poly.poly; }

        internal BigInteger GetCoeff(int index)
        { return poly.GetCoeff(index); }

        public static PolyMod operator +(PolyMod left, PolyMod right)
        {
            Polynomial temp = (left.poly + right.poly) % mod;
            return new PolyMod(temp);
        }

        public static PolyMod operator -(PolyMod left, PolyMod right)
        {
            Polynomial temp = (left.poly - right.poly) % mod;
            return new PolyMod(temp);
        }

        public static PolyMod operator *(PolyMod left, PolyMod right)
        {
            Polynomial temp = Polynomial.Reduce(left.poly * right.poly, mod);
            return new PolyMod(temp);
        }

        public static PolyMod operator /(PolyMod left, BigInteger right)
        {
            Polynomial pol = left.poly / right;
            return new PolyMod(pol);
        }

        internal static Polynomial Gcd(Polynomial poly)
        { return Polynomial.Gcd(poly, mod); }

        internal static PolyMod Compose(PolyMod left, PolyMod right)
        {
            Polynomial poly = left.poly;
            Polynomial cpoly = 0;
            Polynomial temp = 1;

            for(int k = 0; k <= poly.Degree; k++)
            {
                Polynomial aux = poly.coeffs[k] * temp;
                temp = Polynomial.Reduce(temp * right.poly, mod);
                cpoly += aux;
            }

            PolyMod result = new PolyMod();
            result.poly = cpoly % mod;
            return result;
        }

        internal static PolyMod Pow(PolyMod poly, BigInteger k)
        {
            Polynomial val = Polynomial.Pow(poly.poly, k, mod);
            PolyMod result = new PolyMod();
            result.poly = val;
            return result;
        }

        internal BigInteger F(BigInteger x)
        { return poly.Horner(x); }

        internal static void SetModulus(Polynomial modulus)
        { 
            mod = modulus;
            Polynomial.SetPolyMod(modulus);
        }

        public static implicit operator PolyMod(int val)
        {
            Polynomial pol = val;
            return new PolyMod(pol);
        }

        public override bool Equals(object obj)
        { return poly.Equals(obj); }

        public override int GetHashCode()
        { return poly.GetHashCode(); }

        public static bool operator ==(PolyMod left, PolyMod right)
        { return (left.poly == right.poly); }

        public static bool operator !=(PolyMod left, PolyMod right)
        { return (left.poly != right.poly); }

        public static implicit operator PolyMod(BigInteger val)
        {
            Polynomial pol = val;
            return new PolyMod(pol);
        }

        public static implicit operator PolyMod(Polynomial poly)
        { return new PolyMod(poly); }
    }
}
