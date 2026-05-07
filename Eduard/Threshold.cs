#if !USE_BENCHMARKING
using System;

namespace Eduard
{
    /// <summary>
    /// Algorithm selection thresholds for cryptographic arithmetic operations.
    /// </summary>
    /// <remarks>
    /// Values determine when to switch from classical to advanced algorithms<br/>
    /// (Karatsuba, FFT/NTT) based on operand size.
    /// </remarks>
    public enum Threshold : int
    {
        /// <summary>
        /// Minimum words for Karatsuba big integer multiplication.
        /// </summary>
        BIGINT_KARATSUBA_MULT_THRESHOLD = 16,

        /// <summary>
        /// Minimum words for Karatsuba big integer squaring.
        /// </summary>
        BIGINT_KARATSUBA_SQUARE_THRESHOLD = 32,

        /// <summary>
        /// Minimum words for FFT-based big integer multiplication.
        /// </summary>
        BIGINT_FFT_THRESHOLD = 1792,

        /// <summary>
        /// Minimum degree for FFT-based polynomial multiplication.
        /// </summary>
        POLY_FFT_MULT_THRESHOLD = 64,

        /// <summary>
        /// Minimum degree for optimized FFT-based polynomial squaring.
        /// </summary>
        POLY_FFT_SQUARE_THRESHOLD = 64,

        /// <summary>
        /// Minimum degree for FFT-based polynomial remainder operations.
        /// </summary>
        POLY_FFT_MOD_THRESHOLD = 64,

        /// <summary>
        /// Minimum polynomial degree for sliding window exponentiation to outperform binary method.
        /// </summary>
        POLY_DEGREE_THRESHOLD = 16,

        /// <summary>
        /// The minimum polynomial degree for FFT-accelerated Horner modular composition.
        /// </summary>
        POLY_MOD_COMPOSE_THRESHOLD = 88,

        /// <summary>
        /// Minimum words for sliding window exponentiation to outperform binary method.
        /// </summary>
        BIGINT_WORDS_THRESHOLD = 10
    }
}
#endif