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
        POLY_FFT_MULT = 3,             // FFT multiplication threshold for polynomials
        POLY_FFT_SQUARE = 4,           // FFT squaring threshold for polynomials
        POLY_FFT_MOD = 5               // FFT remainder threshold for polynomials
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
                1792, 16, 32, 
                256, 256, 256 
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