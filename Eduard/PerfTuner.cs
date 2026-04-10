#if USE_BENCHMARKING
using System;

namespace Eduard
{
    /// <summary>
    /// Performance threshold identifiers for cryptographic algorithm selection.
    /// </summary>
    public enum PerfEntry : int
    {
        BIGINT_FFT = 0,                // FFT multiplication threshold for big integers
        BIGINT_KARATSUBA_MULTIPLY = 1, // Karatsuba multiplication threshold for big integers
        BIGINT_KARATSUBA_SQUARING = 2, // Karatsuba squaring threshold for big integers
        BIGINT_WORDS_THRESHOLD = 3,    // Big integer words count for sliding Window exponentiation
        POLY_FFT_MULT = 4,             // FFT multiplication threshold for polynomials
        POLY_FFT_SQUARE = 5,           // FFT squaring threshold for polynomials
        POLY_FFT_MOD = 6,              // FFT remainder threshold for polynomials
        POLY_DEGREE_POW_MOD = 7,       // Minimum polynomial degree for sliding Window exponentiation
        POLY_DEGREE_FAST_HORNER = 8    // Minimum polynomial degree for FFT-accelerated Horner modular composition
    }

    /// <summary>
    /// Runtime configuration manager for cryptographic performance thresholds.
    /// </summary>
    /// <remarks> 
    /// Provides centralized, low-overhead access to algorithm selection thresholds.
    /// </remarks>
    public static class PerfTuner
    {
        static int[] thresholds;
        const int MAX_COUNT = 6;

        static PerfTuner()
        {
            thresholds = new int[] { 
                1792, 16, 32, 10,
                128, 96, 64, 16, 
                88
            };
        }

        /// <summary>
        /// Updates a specific performance threshold at runtime.
        /// </summary>
        /// <param name="key">The threshold identifier to modify.</param>
        /// <param name="value">New threshold value (typically in words or coefficient count).</param>
        public static void SetThreshold(PerfEntry key, int value)
        {
            int index = (int)key;
            thresholds[index] = value;
        }

        /// <summary>
        /// Retrieves the current value of a specific performance threshold.
        /// </summary>
        /// <param name="key">The threshold identifier to query.</param>
        /// <returns></returns>
        public static int GetThreshold(PerfEntry key)
        {
            int index = (int)key;
            return thresholds[index];
        }
    }
}

#endif