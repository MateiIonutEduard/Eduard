using System;
using System.Collections.Generic;
using System.Text;

namespace Eduard.Security.Curves
{
    /// <summary>
    /// Identifies twisted Edwards-form elliptic curves optimized for <br/>
    /// signature schemes and complete, unified group operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// They offer several advantages for cryptographic implementation:
    /// <list type="bullet">
    /// <item><description>Complete addition formulas with no exceptional points</description></item>
    /// <item><description>Unified addition and doubling operations</description></item>
    /// <item><description>Exceptional performance for signature verification</description></item>
    /// <item><description>Natural resistance to side-channel attacks</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// These curves are primarily used in the EdDSA signature algorithm <br/>
    /// family (Ed25519, Ed448) as specified in RFC 8032.
    /// </para>
    /// </remarks>
    public enum TwistedEdwardsCurveType
    {
        /// <summary>
        /// Edwards25519 (edwards25519) - Twisted Edwards form of Curve25519
        /// </summary>
        /// <remarks>
        /// <para>
        /// Twisted Edwards curve birationally equivalent to Curve25519.
        /// </para>
        /// <para>
        /// This is the curve underlying the Ed25519 signature scheme, offering <br/>
        /// approximately 128 bits of security. Features include:
        /// <list type="bullet">
        /// <item><description>High-performance verification with batch processing</description></item>
        /// <item><description>Small signatures (64 bytes) and keys (32 bytes)</description></item>
        /// <item><description>Deterministic, nonce-free signing</description></item>
        /// <item><description>Collision resilience and side-channel resistance</description></item>
        /// </list>
        /// Widely adopted in SSH, TLS 1.3, and numerous <br/>
        /// cryptocurrency and messaging protocols.
        /// </para>
        /// </remarks>
        Edwards25519 = 9,

        /// <summary>
        /// Edwards448 (edwards448) - Twisted Edwards form of Curve448
        /// </summary>
        /// <remarks>
        /// <para>
        /// Twisted Edwards curve birationally equivalent to Curve448, defined <br/>
        /// over the Goldilocks prime field 2^448 - 2^224 - 1. This curve <br/>
        /// underlies the Ed448 signature scheme (EdDSA with Curve448).
        /// </para>
        /// <para>
        /// Provides approximately 224 bits of security with signature sizes of
        /// 114 bytes.<br/> Features similar properties to Ed25519 but with higher
        /// security margin.<br/> Standardized in RFC 8032 for applications requiring
        /// stronger security or <br/>meeting specific government requirements.
        /// </para>
        /// </remarks>
        Edwards448 = 10
    }
}
