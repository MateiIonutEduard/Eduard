## 🔬 Overview

**Eduard** is a production-grade cryptographic library written in C#, implementing multi-precision integer arithmetic, finite field operations (restricted to prime fields), univariate polynomials over large prime fields, modular operations using quotient rings, and algebraic operations with bivariate polynomials over 𝔽ₚ. It provides the foundation for specialized algorithms such as Schoof's algorithm, Schoof–Elkies–Atkin (SEA), isogeny-based cryptography, and the cryptographic primitives for point and curve representation, along with the extensions required for efficient implementation of elliptic curve cryptosystems over prime fields. This project was developed independently for scientific research in Number Theory, Algebraic Geometry, and Elliptic Curve Cryptography, and offers a mathematically rigorous foundation for the optimal implementation of protocols, encryption systems, and digital signature schemes based on elliptic curves. The implementation emphasizes mathematical correctness, supported by a comprehensive correctness test suite, and maximum performance, guided by benchmarking and threshold tuning for optimal algorithm selection.

[![License](https://img.shields.io/badge/License-Proprietary-red.svg)](LICENSE.txt)
[![Build Status](https://img.shields.io/github/actions/workflow/status/MateiIonutEduard/Eduard/build.yml?branch=master)](https://github.com/MateiIonutEduard/Eduard/actions)
[![Changelog](https://img.shields.io/badge/changelog-view-lightgreen)](CHANGELOG.md)

## 🎯 Scope and Motivation

The cryptographic framework delivers the following functionality:

- **Multi-precision integer arithmetic** with dynamically optimized routines for:
  * Modular reduction (Barrett reduction and remainder-based reduction)
  * Multiplication and squaring (classical, Karatsuba, FFT multiplication)
  * Modular exponentiation combining the binary method and a dynamic sliding window method

- **Complete finite field arithmetic** for elements over prime fields:
  * Algebraic operations on elements over 𝔽ₚ, elegantly extending the multi-precision integer arithmetic functionality
  * Algebraic operations on polynomials over 𝔽ₚ, complete for cryptographic use
  * `PolyMod` class, built on polynomials in 𝔽ₚ[X], providing advanced functionality essential for Computer Algebra, Number Theory, and isogeny-based cryptography
  * Partial support for algebraic operations and management of bivariate polynomials over 𝔽ₚ[X][Y], used in Schoof's and Schoof–Elkies–Atkin algorithms as well as isogeny-based protocols

- **Full support for elliptic curve families** in Weierstrass and Twisted Edwards form, including:
  * Algebraic point operations in affine form and all state-of-the-art projective representations
  * Scalar multiplication on elliptic curves using the fractional w-NAF sliding window method
  * Efficient generation of generator points on elliptic curves, with the ability to generate points from small-order subgroups — essential for specialized Number Theory applications
  * Built-in setting of the elliptic curve generator point, critical for implementing encryption systems and digital signature schemes using elliptic curves
  * Security validation for elliptic curves and, optionally, for elliptic curve generators

- **Limited support for Montgomery-form elliptic curves** for scalar multiplication using affine points (full support will be added in future versions)
- **A comprehensive unit test suite** covering all edge cases, including mathematical correctness verification and in-depth security validation of parameters used in elliptic curve–based cryptosystems
- **Benchmarks** for establishing optimal thresholds used in the intelligent selection of appropriate Number Theory algorithms for elliptic curve–based protocols

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

## 🤝 Issue Policy

This library was developed as an independent research project and is maintained with the same rigor as an academic artifact. Bug reports and issues are the only accepted form of external contribution.

- **Pull requests are not accepted.** Any pull request will be rejected unconditionally. This project is not open to code contributions from external parties.
- **Issue Reporting:** Before opening an issue, contact eduardmatei@outlook.com with a clear description of the observed behavior, including reproducible test cases or mathematical counterexamples where applicable.
- **Issue Creation:** After initial acknowledgment, a GitHub issue may be opened. The issue must include a detailed description, expected vs. actual behavior, and steps to reproduce, referencing the relevant mathematical or cryptographic context.

Issues that do not meet these standards may be deferred or declined to preserve the library's correctness and performance guarantees.

## 📚 Research Context

This software constitutes part of an ongoing research program in:
- **Number Theory**: arithmetic of large primes and modular structures
- **Algebraic Geometry**: curves over finite fields
- **Elliptic Curve Cryptography**: protocol design, encryption schemes, and digital signatures

The development maintains mathematical rigor and ensures computational reproducibility.  
It upholds production-grade software engineering standards throughout.

## 📄 License

**Copyright (c) 2020-Present, Matei Ionut-Eduard. All rights reserved.**

This software is proprietary and confidential. Unauthorized copying, distribution, modification, public display, or public performance of this software, via any medium, is strictly prohibited without prior written permission from the copyright holder.
**Permitted Use:** Access to the source code is granted solely for evaluation, academic and scientific research, and personal study. Researchers in Number Theory, Algebraic Geometry, Cryptography, and related disciplines are expressly permitted to use the Software for non-commercial study, experimentation, benchmarking, and publication.
**Strictly Prohibited:** Selling, sublicensing, or incorporating this Software into any commercial product or service without a separate commercial license agreement.
The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose, and non-infringement.
For licensing inquiries, contact eduardmatei@outlook.com.

See [LICENSE.txt](LICENSE.txt) for the full license terms.
