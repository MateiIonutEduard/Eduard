# 🔐 Eduard

Cryptographic framework for .NET — rigorous, performant, production-ready.

[![License: LGPL v3](https://img.shields.io/badge/License-LGPL_v3-blue.svg)](https://www.gnu.org/licenses/lgpl-3.0)
[![Build](https://img.shields.io/github/actions/workflow/status/MateiIonutEduard/Eduard/build.yml?branch=master)](https://github.com/MateiIonutEduard/Eduard/actions)

## ✨ Overview

- 🔢 Multi-precision integer arithmetic with Barrett reduction, Karatsuba/FFT multiplication, and sliding window exponentiation
- 🧮 Prime field arithmetic over 𝔽ₚ, including univariate and bivariate polynomials, and quotient ring operations
- 📈 Elliptic curves in Weierstrass, Twisted Edwards, and Montgomery form with affine and projective coordinates
- ⚡ Scalar multiplication via fractional w-NAF sliding window method with optimal threshold selection
- 🛡️ Built-in curve and generator security validation

## 📦 Installation

```bash
dotnet add package Eduard
```

## 🎯 Targets
.NET 10, .NET 8, .NET Standard 2.0

## 📄 License
LGPL v3 — free for commercial use as a library.