namespace Eduard.Security.Curves
{
    /// <summary>
    /// Identifies Montgomery-form elliptic curves optimized for efficient <br/>
    /// scalar multiplication and constant-time implementation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// They enable the Montgomery ladder, a highly efficient and naturally <br/>
    /// constant-time scalar multiplication algorithm that uses
    /// only x-coordinate arithmetic.
    /// </para>
    /// <para>
    /// These curves are primarily used in Diffie-Hellman key exchange protocols <br/>
    /// (X25519, X448) due to their performance, simplicity, and resistance to <br/>
    /// side-channel attacks.
    /// </para>
    /// </remarks>
    public enum MontyCurveType
    {
        /// <summary>
        /// Curve25519 (also known as X25519's underlying curve)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Montgomery curve defined over the prime field 2^255 - 19. <br/>
        /// Designed by Daniel J. Bernstein for high-performance and constant-time ECDH.
        /// </para>
        /// <para>
        /// Provides approximately 128 bits of security. The curve's prime is a pseudo-Mersenne <br/>
        /// prime enabling extremely efficient modular reduction. Widely deployed in TLS 1.3, <br/>
        /// Signal Protocol, and modern cryptographic libraries.
        /// </para>
        /// </remarks>
        Curve25519 = 7,

        /// <summary>
        /// Curve448 (also known as X448's underlying curve, Goldilocks)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Montgomery curve defined over the prime field 2^448 - 2^224 - 1. <br/>
        /// Designed by Mike Hamburg as part of the "Goldilocks" family of curves.
        /// </para>
        /// <para>
        /// Provides approximately 224 bits of security. The modulus is a Solinas prime <br/>
        /// enabling efficient reduction. Offers a higher security alternative to Curve25519 <br/>
        /// while maintaining good performance. Standardized for use in X448 key exchange (RFC 7748).
        /// </para>
        /// </remarks>
        Curve448 = 8
    }
}
