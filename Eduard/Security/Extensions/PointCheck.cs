namespace Eduard.Security.Extensions
{
    /// <summary>
    /// Represents the cryptographic validity classification of an elliptic curve point after <br/>
    /// validation against standard attack vectors.
    /// </summary>
    /// <remarks>
    /// This enumeration provides granular failure discrimination essential for secure protocol <br/>
    /// implementation. Distinguishing between twist points (<see cref="EC_TWIST"/>) and small <br/>
    /// subgroup elements (<see cref="EC_SMALL_SUBGROUP"/>) enables proper handling of <br/>
    /// contributory behavior in Diffie-Hellman key agreement and signature verification.
    /// </remarks>
    internal enum PointCheck
    {
        /// <summary>
        /// The point satisfies all security-critical validation criteria.
        /// </summary>
        /// <remarks>
        /// The point resides on the intended curve, is not on the quadratic twist, and lies outside <br/>
        /// all non-trivial small subgroups. This classification guarantees the point generates a <br/>
        /// subgroup of order at least the large prime factor of the curve order.
        /// </remarks>
        EC_VALID,

        /// <summary>
        /// The point fails the defining curve equation and does not reside on the quadratic twist.
        /// </summary>
        /// <remarks>
        /// This classification indicates either coordinate corruption, malformed encoding, or a point <br/>
        /// belonging to a completely unrelated curve. Acceptance of such points would violate the <br/>
        /// fundamental algebraic structure required for cryptographic operations.
        /// </remarks>
        EC_INVALID,

        /// <summary>
        /// The point resides on the quadratic twist of the intended curve.
        /// </summary>
        /// <remarks>
        /// Points on the twist satisfy a related but distinct curve equation with a different group <br/>
        /// order and security properties. This classification indicates a potential twist attack vector <br/>
        /// where an adversary supplies a point chosen to force discrete logarithm computation in a <br/>
        /// weaker group. Detection is performed via Jacobi symbol evaluation, ensuring constant-time <br/>
        /// rejection before point arithmetic proceeds.
        /// </remarks>
        EC_TWIST,

        /// <summary>
        /// The point lies within a small-order subgroup of the intended curve.
        /// </summary>
        /// <remarks>
        /// When multiplied by the curve's cofactor, the point reduces to the identity element at <br/>
        /// infinity. This indicates the point's order divides the cofactor rather than the large prime <br/>
        /// subgroup intended for cryptographic operations. Acceptance of such points enables <br/>
        /// small-subgroup confinement attacks, particularly dangerous in static-static Diffie-Hellman <br/>
        /// where the adversary may force the shared secret into a trivially enumerable set.
        /// </remarks>
        EC_SMALL_SUBGROUP
    }
}
