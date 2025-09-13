## Eduard Library

Eduard Library is a cryptographic software artifact implemented in C#, designed to optimize multi-precision 
integer arithmetic with direct applicability to elliptic curve cryptography (<b>ECC</b>). Its primary objective is to 
provide a performant and mathematically rigorous foundation for the implementation of elliptic curve–based 
protocols, encryption systems, and digital signature schemes.<br/>

This work originates from an independent research initiative in Number Theory, Algebraic Geometry, and Elliptic 
Curve Cryptography, and is intended to serve as a reliable reference for experimental and theoretical investigations 
into computational aspects of modern cryptography.<br/>

## Scope and Motivation
The library provides:

- Efficient algorithms for multi-precision integer operations, tailored for elliptic curve arithmetic.
- A foundation for implementing cryptographic primitives over elliptic curves.
- A research-grade software environment to support experimental work in algebraic <br/>and number-theoretic cryptography.

It is not conceived as a general-purpose or production-ready framework, but rather as a research artifact <br/>
emphasizing mathematical transparency, reproducibility, and fidelity to theoretical models.

## Contribution and Issue Policy
Given its origin as an academic research artifact, contributions are subject to strict methodological 
and <br/>mathematical validation. The following rules apply:

1. Notification of Exceptional Cases
   - If partial or incorrect behavior is observed, please notify the maintainer via email: eduardmatei@outlook.com.
   - Reports must include reproducible experimental results under the specified conditions.
   - Proposals for additional components are welcome if substantiated by clear mathematical necessity.

2. Issue Creation
   - If the initial response is positive, a corresponding GitHub issue should be created to document either the <br/>proposed extension or the identified anomaly.

3. Pull Requests
   - Pull requests must conform strictly to the library’s implementation conventions.
   - Submissions are accepted only to the <code>develop</code> branch.
   - Pull requests targeting the <code>master</code> branch will be rejected without exception, <br/>irrespective of the contributor’s expertise.
   
Deviation from rules (1)–(3) will result in the dismissal of the issue, the pull request, or both.

## Research Context
This software artifact is embedded in a broader program of research in:
- Number Theory (arithmetic of large primes and modular structures),
- Algebraic Geometry (curves over finite fields), and
- Elliptic Curve Cryptography (protocol design, encryption schemes, and digital signatures).

The development adheres to principles of mathematical rigor and computational reproducibility, 
<br/>with the overarching goal of bridging theory and implementation in cryptographic research.