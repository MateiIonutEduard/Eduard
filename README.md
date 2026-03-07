## Description

**Eduard** library is a production-ready cryptographic software artifact implemented in C#, designed to provide high-performance multi-precision integer arithmetic and robust elliptic curve cryptography (ECC). Its primary objective is to deliver a mathematically rigorous, secure, and optimized foundation for implementing elliptic curve–based protocols, encryption systems, and digital signature schemes. This work originates from an independent research initiative in Number Theory, Algebraic Geometry, and Elliptic Curve Cryptography, and has been developed into a production-grade library with comprehensive testing, benchmarking, and security validation.

## Scope and Motivation

The library provides:

- **High-performance multi-precision integer operations**, tailored for elliptic curve arithmetic with Barrett reduction, FFT multiplication, and Karatsuba algorithms.
- **Complete elliptic curve support** for Weierstrass, Twisted Edwards families
- **Limited elliptic curve support** for Montgomery curve family
- **Multiple coordinate systems** including affine, Jacobian (3w, 4w, 5w) for Weierstrass, and projective (3, 4) for Edwards curves.
- **Production-ready features**: constant-time operations, side-channel resistant implementations, comprehensive input validation, and exception safety.
- **Integrated unit tests and benchmark suite** ensuring mathematical correctness, performance, and memory efficiency.

It is designed for real-world deployment, guaranteeing safe encryption via elliptic curve cryptosystems <br/>while providing robust, secure, and high-speed operation with minimal memory footprint.

## Quality Assurance

- **Unit Tests**: Comprehensive coverage validating all mathematical operations against known results.
- **Benchmark Suite**: Performance metrics for scalar multiplication, point operations, and field arithmetic across all supported curves.
- **Memory Profiling**: Zero heap allocations in critical paths; struct-based primitives ensure memory efficiency.
- **Validation Tests**: Known Answer Tests (KATs) against NIST and RFC standards for all curve implementations.

## Performance

- **Scalar Multiplication**: Optimized for all supported curves
- **Point Operations**: Sub-microsecond addition and doubling in projective coordinates
- **Field Arithmetic**: Barrett reduction with cached constants
- **Memory Efficient**: No allocations in hot paths; stack-allocated temporary values

## Contribution and Issue Policy

Given its origin as an academic research artifact and its current production-ready status, contributions <br/>are subject to strict methodological and mathematical validation. The following rules apply:

1. **Notification of Issues**
   - If unexpected behavior is observed, please notify the maintainer via email: eduardmatei@outlook.com.
   - Reports must include reproducible experimental results under specified conditions.

2. **Issue Creation**
   - Upon positive initial response, create a GitHub issue documenting either the proposed extension or identified anomaly.

3. **Pull Requests**
   - Must conform strictly to the library's implementation conventions and include comprehensive tests.
   - Submissions are accepted only to the `develop` branch.
   - Pull requests targeting the `master` branch will be rejected without exception, regardless of contributor expertise.

Deviation from these rules will result in dismissal of the issue, pull request, or both.

## Research Context

This software artifact is embedded in a broader program of research in:
- Number Theory (arithmetic of large primes and modular structures),
- Algebraic Geometry (curves over finite fields), and
- Elliptic Curve Cryptography (protocol design, encryption schemes, and digital signatures).

The development adheres to principles of mathematical rigor, computational <br/>reproducibility, and production-grade software engineering.

## License

This project is distributed under the MIT License. It is production-ready and suitable for real-world deployment,<br/> with guarantees of correctness, security, and performance as validated by the integrated test suite.
