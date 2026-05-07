using System;
using System.Collections.Generic;
using System.Text;

namespace Eduard
{
    public class Garner
    {
        static uint[][] c;
        static uint[] m;
        static int n;

        public static void Init(uint[] moduli)
        {
            n = moduli.Length;
            c = new uint[n][];
            int i;

            m = new uint[n];
            uint product;

            for(i = 0; i < n; i++)
            {
                c[i] = new uint[n];
                m[i] = moduli[i];
                product = 1;

                for (int j = 0; j < i; j++)
                    product = CoreMath.MultMod(product, 
                        moduli[j], moduli[i]);
            }
        }

        public static BigInteger GetInteger(uint[] residues)
        {
            uint[] v = new uint[n];
            uint product, sum;

            v[0] = residues[0] % m[0];
            if (v[0] < 0) v[0] += m[0];

            for (int i = 1; i < n; i++)
            {
                sum = v[0];
                product = 1;

                for (int j = 1; j < i; j++)
                {
                    product = CoreMath.MultMod(product, m[j - 1], m[i]);
                    CoreMath.MultAdd(v[j], product, sum, m[i], ref sum);
                }

                uint diff = CoreMath.DiffMod(residues[i], sum, m[i]);
                product = CoreMath.MultMod(product, m[i - 1], m[i]);

                uint inverse = CoreMath.Inverse(product, m[i]);
                v[i] = CoreMath.MultMod(diff, inverse, m[i]);
            }

            BigInteger res = v[0];
            BigInteger weight = 1;

            for (int i = 1; i < n; i++)
            {
                weight *= m[i - 1];
                res += v[i] * weight;
            }

            return res;
        }
    }
}
