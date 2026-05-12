using System;
using System.Collections.Generic;
using System.Text;

namespace Eduard.Tests.Core
{
    public class GarnerTests
    {
        #region Constructor Validation

        [Fact]
        public void Constructor_NullModuli_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Garner(null));
            Assert.Contains("null", ex.Message.ToLower());
        }

        [Fact]
        public void Constructor_EmptyModuli_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Garner(Array.Empty<uint>()));
            Assert.Contains("empty", ex.Message.ToLower());
        }

        [Fact]
        public void Constructor_SingleModulus_DoesNotThrow()
        {
            var g = new Garner(new uint[] { 17 });
            Assert.NotNull(g);
        }

        [Fact]
        public void Constructor_InversesArray_SizedCorrectly()
        {
            uint[] moduli = { 2, 3, 5, 7 };
            var g = new Garner(moduli);
            uint[] residues = { 1, 2, 3, 4 };
            var result = g.GetInteger(residues);
        }

        #endregion

        #region Reconstruction - Unsigned Mode

        [Fact]
        public void GetInteger_StandardExample_ReturnsCorrectValue()
        {
            uint[] moduli = { 3, 5, 7 };
            uint[] residues = { 2, 3, 2 };
            var g = new Garner(moduli);

            var result = g.GetInteger(residues);
            Assert.Equal(new BigInteger(23), result);
        }

        [Fact]
        public void GetInteger_ZeroResidues_ReturnsZero()
        {
            uint[] moduli = { 3, 5, 7, 11 };
            uint[] residues = { 0, 0, 0, 0 };
            var g = new Garner(moduli);

            var result = g.GetInteger(residues);
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetInteger_AllOnes_ReturnsOne()
        {
            uint[] moduli = { 3, 5, 7 };
            uint[] residues = { 1, 1, 1 };
            var g = new Garner(moduli);
            var result = g.GetInteger(residues);
            Assert.Equal(1, result);
        }

        [Fact]
        public void GetInteger_LargeModuli_ReconstructsCorrectly()
        {
            uint[] moduli = { 65521, 65537, 65539 };
            uint[] residues = { 12345, 54321, 11111 };

            var g = new Garner(moduli);
            var result = g.GetInteger(residues);

            foreach (int i in new[] { 0, 1, 2 })
                Assert.Equal(residues[i], (uint)(result % moduli[i]));
        }

        [Fact]
        public void GetInteger_MaxUIntResidues_WithinModulus()
        {
            uint[] moduli = { 65521, 65537 };
            uint[] residues = { 65520, 65536 };

            var g = new Garner(moduli);
            var result = g.GetInteger(residues);

            Assert.True(result % moduli[0] == residues[0]);
            Assert.True(result % moduli[1] == residues[1]);
        }

        [Fact]
        public void GetInteger_FourModuli_ConsistentWithCRT()
        {
            uint[] moduli = { 2, 3, 5, 7 };
            uint[] residues = { 1, 1, 1, 1 };

            var g = new Garner(moduli);
            var result = g.GetInteger(residues);

            Assert.Equal(1, result);
            Assert.True(result < 2 * 3 * 5 * 7);
        }

        [Fact]
        public void GetInteger_EightModuli_ReconstructsCorrectly()
        {
            uint[] moduli = { 2, 3, 5, 7, 11, 13, 17, 19 };
            uint[] residues = { 1, 2, 3, 4, 5, 6, 7, 8 };

            var g = new Garner(moduli);
            var result = g.GetInteger(residues);

            foreach (int i in new[] { 0, 1, 2, 3, 4, 5, 6, 7 })
                Assert.Equal(residues[i], (uint)(result % moduli[i]));
        }

        [Fact]
        public void GetInteger_TypicalSEAModuli_ReturnsInRange()
        {
            uint[] moduli = { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };
            uint[] residues = { 2, 4, 1, 10, 12, 3, 18, 5, 28, 30 };

            var g = new Garner(moduli);
            var result = g.GetInteger(residues);
            BigInteger product = 1;

            foreach (uint m in moduli) 
                product *= m;

            Assert.True(result >= 0);
            Assert.True(result < product);

            foreach (int i in new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })
                Assert.Equal(residues[i], (uint)(result % moduli[i]));
        }

        #endregion

        #region Reconstruction - Signed Mode

        [Fact]
        public void GetInteger_SignedMode_SmallPositive_ReturnsSameAsUnsigned()
        {
            uint[] moduli = { 3, 5, 7 };
            uint[] residues = { 2, 3, 2 };
            var g = new Garner(moduli, negative: true);
            var result = g.GetInteger(residues);
            Assert.Equal(new BigInteger(23), result);
        }

        [Fact]
        public void GetInteger_SignedMode_LargeValue_WrapsToNegative()
        {
            uint[] moduli = { 3, 5, 7 };
            uint[] residues = { 0, 0, 6 };
            var g = new Garner(moduli, negative: true);
            var result = g.GetInteger(residues);
            Assert.Equal(new BigInteger(-15), result);
        }

        [Fact]
        public void GetInteger_SignedMode_ExactHalf_StaysPositive()
        {
            uint[] moduli = { 3, 5, 7 };
            uint[] residues = { 1, 2, 3 };

            var g = new Garner(moduli, negative: true);
            var result = g.GetInteger(residues);
            Assert.Equal(new BigInteger(52), result);
        }

        [Fact]
        public void GetInteger_SignedMode_Zero_ReturnsZero()
        {
            uint[] moduli = { 3, 5, 7, 11 };
            uint[] residues = { 0, 0, 0, 0 };

            var g = new Garner(moduli, negative: true);
            var result = g.GetInteger(residues);
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetInteger_SignedMode_ReturnsInCenteredRange()
        {
            uint[] moduli = { 3, 5, 7, 11 };
            uint[] residues = { 1, 2, 3, 4 };

            var g = new Garner(moduli, negative: true);
            var result = g.GetInteger(residues);
            BigInteger N = 3 * 5 * 7 * 11;

            Assert.True(result > -N / 2);
            Assert.True(result <= N / 2);
        }

        [Fact]
        public void GetInteger_SignedMode_LargeModuli_ReconstructsCentered()
        {
            uint[] moduli = { 65521, 65537, 65539 };
            uint[] residues = { 40000, 50000, 60000 };

            var g = new Garner(moduli, negative: true);
            var result = g.GetInteger(residues);

            BigInteger N = (BigInteger)65521 * 65537 * 65539;
            Assert.True(result > -N / 2);
            Assert.True(result <= N / 2);

            foreach (int i in new[] { 0, 1, 2 })
                Assert.Equal(residues[i], (uint)(((result % moduli[i]) + moduli[i]) % moduli[i]));
        }

        #endregion

        #region GetInteger Validation

        [Fact]
        public void GetInteger_NullResidues_ThrowsArgumentNullException()
        {
            uint[] moduli = { 3, 5, 7 };
            var g = new Garner(moduli);

            var ex = Assert.Throws<ArgumentNullException>(() => 
                g.GetInteger(null));

            Assert.Contains("null", 
                ex.Message.ToLower());
        }

        [Fact]
        public void GetInteger_WrongLengthResidues_ThrowsArgumentException()
        {
            uint[] moduli = { 3, 5, 7 };
            uint[] residues = { 1, 2 };
            var g = new Garner(moduli);

            var ex = Assert.Throws<ArgumentException>(() => 
                    g.GetInteger(residues));

            Assert.Contains("length", 
                ex.Message.ToLower());
        }

        [Fact]
        public void GetInteger_TooManyResidues_ThrowsArgumentException()
        {
            uint[] moduli = { 3, 5, 7 };
            uint[] residues = { 1, 2, 3, 4 };
            var g = new Garner(moduli);

            var ex = Assert.Throws<ArgumentException>(() => 
                g.GetInteger(residues));
            Assert.Contains("length", 
                ex.Message.ToLower());
        }

        [Fact]
        public void GetInteger_DefaultStruct_ThrowsInvalidOperationException()
        {
            Garner g = default;
            uint[] residues = { 1, 2, 3 };

            var ex = Assert.Throws<InvalidOperationException>(() => 
                g.GetInteger(residues));

            Assert.Contains("not initialized", 
                ex.Message.ToLower());
        }

        [Fact]
        public void GetInteger_SignedMode_MinusOne_AllMaxResidues_WrapsToNegative()
        {
            uint[] moduli = { 3, 5, 7 };
            uint[] residues = { 2, 4, 6 };

            var g = new Garner(moduli, negative: true);
            var result = g.GetInteger(residues);
            Assert.Equal(new BigInteger(-1), result);
        }

        #endregion

        #region Round-Trip Fidelity

        [Fact]
        public void RoundTrip_Unsigned_SmallPrimes_RecoversOriginal()
        {
            uint[] moduli = { 2, 3, 5, 7, 11 };
            var g = new Garner(moduli);

            BigInteger max = 2 * 3 * 5 * 7 * 11;
            BigInteger[] testValues = { 0, 1, 100, 
                500, 1000, 2000, (max - 1) };

            foreach (var val in testValues)
            {
                uint[] residues = new uint[moduli.Length];

                for (int i = 0; i < moduli.Length; i++)
                    residues[i] = (uint)(val % moduli[i]);

                var reconstructed = g.GetInteger(residues);
                Assert.Equal(val, reconstructed);
            }
        }

        [Fact]
        public void RoundTrip_Signed_SmallPrimes_RecoversInCenteredRange()
        {
            uint[] moduli = { 2, 3, 5, 7, 11 };
            var g = new Garner(moduli, negative: true);

            BigInteger N = 2 * 3 * 5 * 7 * 11;
            BigInteger half = N / 2;

            BigInteger[] testValues = { 0, 1, 50, half, -1, -50, -(half - 1) };
            foreach (var val in testValues)
            {
                BigInteger normalized = ((val % N) + N) % N;
                uint[] residues = new uint[moduli.Length];

                for (int i = 0; i < moduli.Length; i++)
                    residues[i] = (uint)(normalized % moduli[i]);

                var reconstructed = g.GetInteger(residues);
                Assert.Equal(val, reconstructed);
            }
        }

        [Fact]
        public void RoundTrip_Unsigned_UniformRandom_MatchesDirect()
        {
            uint[] moduli = { 3, 5, 7, 11, 13 };
            var g = new Garner(moduli);

            BigInteger N = 3 * 5 * 7 * 11 * 13;
            var rng = new Random(0x53656564);

            for (int t = 0; t < 20; t++)
            {
                byte[] bytes = new byte[N.ToByteArray().Length];
                rng.NextBytes(bytes);
                bytes[bytes.Length - 1] = 0;
                BigInteger val = new BigInteger(bytes) % N;
                if (val < 0) val += N;

                uint[] residues = new uint[moduli.Length];
                for (int i = 0; i < moduli.Length; i++)
                    residues[i] = (uint)(val % moduli[i]);
                Assert.Equal(val, g.GetInteger(residues));
            }
        }

        #endregion

        #region Idempotence and Consistency

        [Fact]
        public void GetInteger_RepeatedCalls_SameResidues_ReturnsSameResult()
        {
            uint[] moduli = { 3, 5, 7, 11 };
            uint[] residues = { 1, 2, 3, 4 };
            var g = new Garner(moduli);
            var r1 = g.GetInteger(residues);
            var r2 = g.GetInteger(residues);
            var r3 = g.GetInteger(residues);
            Assert.Equal(r1, r2);
            Assert.Equal(r2, r3);
        }

        [Fact]
        public void GetInteger_DifferentResidues_DifferentResults()
        {
            uint[] moduli = { 3, 5, 7 };
            var g = new Garner(moduli);
            var r1 = g.GetInteger(new uint[] { 0, 0, 0 });
            var r2 = g.GetInteger(new uint[] { 1, 1, 1 });
            Assert.NotEqual(r1, r2);
        }

        [Fact]
        public void GetInteger_SameResidues_DifferentInstances_SameResult()
        {
            uint[] moduli = { 3, 5, 7 };
            uint[] residues = { 2, 3, 2 };

            var g1 = new Garner(moduli);
            var g2 = new Garner(moduli);

            Assert.Equal(g1.GetInteger(residues), 
                g2.GetInteger(residues));
        }

        #endregion
    }
}
