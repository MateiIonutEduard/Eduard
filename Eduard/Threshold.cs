#if !USE_BENCHMARKING
using System;

namespace Eduard
{
    public enum Threshold : int
    {
        BIGINT_KARATSUBA_THRESHOLD = 16,
        BIGINT_FFT_THRESHOLD = 2048,
        POLY_FFT_MULT_THRESHOLD = 256,
        POLY_FFT_SQUARE_THRESHOLD = 256,
        POLY_FFT_MOD_THRESHOLD = 256
    }
}
#endif