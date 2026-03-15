using System;
using System.Collections.Generic;
using System.Text;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Identifies standardized Weierstrass-form elliptic curves over prime fields. <br/>
    /// Includes NIST P-curves (FIPS 186-4) and Weierstrass representations of <br/>
    /// Montgomery curves for benchmarking and testing purposes.
    /// </summary>
    public enum WeiCurveType
    {
        /// <summary>
        /// NIST P-192 (also known as secp192r1, ansix9p192r1)
        /// </summary>
        /// <remarks>
        /// 192-bit prime field curve. Provides approximately 96 bits of security. <br/>
        /// Considered legacy; not recommended for new applications. Included for <br/>
        /// backward compatibility and performance benchmarking of smaller field sizes.
        /// </remarks>
        NistP192 = 0,

        /// <summary>
        /// NIST P-224 (also known as secp224r1)
        /// </summary>
        /// <remarks>
        /// 224-bit prime field curve. Provides approximately 112 bits of security. <br/>
        /// Suitable for legacy systems but generally superseded by P-256 in <br/>
        /// modern applications.
        /// </remarks>
        NistP224 = 1,

        /// <summary>
        /// NIST P-256 (also known as secp256r1, prime256v1)
        /// </summary>
        /// <remarks>
        /// 256-bit prime field curve. Provides approximately 128 bits of security. <br/>
        /// The most widely deployed NIST curve, recommended for most applications <br/>
        /// requiring strong security with good performance. Used extensively in TLS, <br/>
        /// blockchain, and government applications.
        /// </remarks>
        NistP256 = 2,

        /// <summary>
        /// NIST P-384 (also known as secp384r1)
        /// </summary>
        /// <remarks>
        /// 384-bit prime field curve. Provides approximately 192 bits of security. <br/>
        /// Recommended for top-secret government applications and scenarios <br/>
        /// requiring long-term security against quantum computing advances. <br/>
        /// Performance overhead is significant compared to P-256.
        /// </remarks>
        NistP384 = 3,

        /// <summary>
        /// NIST P-521 (also known as secp521r1)
        /// </summary>
        /// <remarks>
        /// 521-bit prime field curve. Provides approximately 260 bits of security. <br/>
        /// The strongest standardized NIST curve. Note the modulus is a Mersenne <br/>
        /// prime (2^521 - 1), enabling efficient reduction. Suitable for
        /// highest-security<br/> requirements despite substantial performance cost.
        /// </remarks>
        NistP521 = 4,

        /// <summary>
        /// Weierstrass form of Curve25519 (W-25519)
        /// </summary>
        /// <remarks>
        /// Transforms the Montgomery curve Curve25519 into Weierstrass form
        /// via <br/>birational equivalence. Enables compatibility with Weierstrass-based <br/>
        /// implementations while maintaining the security properties of the original <br/>
        /// curve.
        /// Useful for benchmarking the performance impact of curve representation <br/>
        /// and for systems requiring unified Weierstrass interfaces.
        /// </remarks>
        Wei25519 = 5,

        /// <summary>
        /// Weierstrass form of Curve448 (W-448)
        /// </summary>
        /// <remarks>
        /// Transforms the Montgomery curve Curve448 into Weierstrass form. <br/>
        /// Based on the 448-bit Edwards curve that underlies Ed448 and X448, <br/>
        /// offering approximately 224 bits of security. Provides a Weierstrass <br/>
        /// interface to the Goldilocks curve for compatibility purposes.
        /// </remarks>
        Wei448 = 6
    }
}
