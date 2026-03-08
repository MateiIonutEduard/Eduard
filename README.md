## 🔬 Description

**Eduard** is a production-grade cryptographic library in C# implementing high-performance multi-precision integer arithmetic and elliptic curve cryptography (ECC). Developed from independent research in Number Theory, Algebraic Geometry, and ECC, the library provides a mathematically rigorous foundation for elliptic curve–based protocols, encryption systems, and digital signature schemes. The implementation emphasizes correctness, security, and performance, validated through comprehensive testing and benchmarking.

## 🎯 Scope and Motivation

The library delivers:

- **Multi-precision integer arithmetic** optimized for elliptic curve operations
  - Barrett reduction, FFT multiplication, Karatsuba algorithms

- **Complete elliptic curve support** for Weierstrass and Twisted Edwards families
  - Limited Montgomery family support

- **Multiple coordinate systems**
  - Affine and Jacobian (3w, 4w, 5w) for Weierstrass curves
  - Projective (3, 4) for Edwards curves

- **Production-ready implementation**
  - Constant-time operations, side-channel resistance
  - Comprehensive validation and exception safety

- **Integrated test and benchmark infrastructure**
  - Mathematical correctness and performance characterization

The architecture targets real-world deployment, providing secure, high-speed <br/>elliptic curve cryptosystems with minimal memory footprint.

## ✅ Quality Assurance

- **Unit Tests**: Complete coverage against known mathematical results
- **Benchmark Suite**: Performance characterization across all supported curves
- **Memory Profiling**: Zero heap allocations in critical paths
- **Validation Tests**: Known Answer Tests (KATs) against NIST and RFC standards

## ⚡ Performance

- **Scalar multiplication**: Optimized across all supported curves
- **Point operations**: Sub-microsecond addition and doubling
- **Field arithmetic**: Barrett reduction with cached constants
- **Memory footprint**: Zero allocations in hot paths; stack-allocated temporaries

## 🤝 Contribution and Issue Policy

This library originated as an academic research artifact and maintains its production-ready status through rigorous validation. Contributions must adhere to strict methodological standards:

1. **Issue Notification**: Report unexpected behavior to eduardmatei@outlook.com with reproducible experimental results.

2. **Issue Creation**: Following positive initial response, create a GitHub issue documenting proposed extensions or anomalies.

3. **Pull Requests**: Must conform to conventions, include tests, and target `develop` only. PRs to `master` rejected unconditionally.

Non-compliance results in immediate dismissal of issues or pull requests.

## 📚 Research Context

This software constitutes part of an ongoing research program in:
- **Number Theory**: arithmetic of large primes and modular structures
- **Algebraic Geometry**: curves over finite fields
- **Elliptic Curve Cryptography**: protocol design, encryption schemes, and digital signatures

The development maintains mathematical rigor and ensures computational reproducibility. <br/>
It upholds production-grade software engineering standards throughout.

## 📄 License

BSD 3-Clause License. Production-ready with correctness, security, and performance guarantees validated by the integrated test suite.
