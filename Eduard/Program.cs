using System.Security.Cryptography;
using Eduard.Security;
using ECPoint = Eduard.Security.ECPoint;

namespace Eduard
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RandomNumberGenerator rand = RandomNumberGenerator.Create();
            EllipticCurve ellipticCurve = new EllipticCurve(256);
            ECPoint basePoint = ellipticCurve.BasePoint;
            BigInteger kk = BigInteger.Next(rand, 1, ellipticCurve.field - 1);
            ECPoint point = ellipticCurve.Multiply(kk, basePoint);
            Console.WriteLine();
        }
    }
}
