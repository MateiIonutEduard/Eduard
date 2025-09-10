using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eduard.Cryptography
{
    /// <summary>
    /// Provides methods to store affine points on the elliptic curve.
    /// </summary>
    public enum ECPointCompressionMode
    {
        /// <summary>
        /// Use the compressed form to represent affine points compactly.
        /// </summary>
        EC_POINT_COMPRESSED,
        /// <summary>
        /// Use the uncompressed form for a standardized representation of affine points.
        /// </summary>
        EC_POINT_UNCOMPRESSED
    }
}
