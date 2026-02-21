using System;

namespace Eduard
{
    public class Util
    {
        public static BigInteger mod;
        public static BigInteger k;

        public static void modulo(BigInteger field)
        {
            k = BigInteger.BarrettConstant(field);
            mod = field;
        }

        public static BigInteger BarrettReduction(BigInteger val, BigInteger field)
        {
            if (mod == null || field == mod) modulo(field);
            return BigInteger.BarrettReduction(val, mod, k);
        }
    }
}
