namespace Eduard.Cryptography
{
    /// <summary>
    /// Provides different methods for scalar multiplication of points on an elliptic curve.
    /// </summary>
    public enum ECMode
    {
        /// <summary>
        /// Uses the standard affine representation for scalar multiplication of points.
        /// </summary>
        EC_STANDARD_AFFINE,
        /// <summary>
        /// Use the standard mixed projective representation for scalar multiplication of points.
        /// </summary>
        EC_STANDARD_PROJECTIVE,
        /// <summary>
        /// Uses the NAF sliding window algorithm and mixed projective coordinates for point representation.
        /// </summary>
        EC_FASTEST,
        /// <summary>
        /// Uses the Montgomery Ladder algorithm for scalar multiplication of mixed projective points to enhance security.
        /// </summary>
        EC_SECURE
    }
}
