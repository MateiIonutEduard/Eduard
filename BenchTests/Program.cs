#if RELEASE && USE_BENCHMARKING
using System;
using System.Linq;
using System.Collections.Generic;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchTests.Core.Curves;
using BenchmarkTests.Core.BigInt;
using BenchmarkTests.Core.Poly;
#else
using System;
#endif

namespace BenchTests
{
    public class Program
    {
        static void Main(string[] args)
        {
#if RELEASE && USE_BENCHMARKING
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var config = DefaultConfig.Instance;

            if (args.Contains("--help") || args.Contains("-h"))
            {
                ShowHelp();
                return;
            }

            /* process arguments */
            for(int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--bigint":
                    case "-b":
                        Console.WriteLine("\nBigInt Benchmarks");
                        Console.WriteLine("─────────────────");
                        RunBigIntBenchmarks(config);
                        break;

                    case "--polynomials":
                    case "-p":
                        Console.WriteLine("\nPolynomial Benchmarks");
                        Console.WriteLine("─────────────────────");
                        RunPolynomialBenchmarks(config);
                        break;

                    case "--curves":
                    case "-c":
                        Console.WriteLine("\nElliptic Curve Benchmarks");
                        Console.WriteLine("──────────────────────────");
                        RunCurveBenchmarks(config);
                        break;

                    case "--run-all":
                    case "-a":
                        Console.WriteLine("\nEduard Crypto Library - Complete Benchmark Suite");
                        Console.WriteLine("────────────────────────────────────────────────");
                        RunAllBenchmarks(config);
                        break;
                }
            }
#else
            Console.WriteLine("Benchmarking is only available in RELEASE mode with USE_BENCHMARKING defined.");
#endif
        }

#if RELEASE && USE_BENCHMARKING
        static void ShowHelp()
        {
            Console.WriteLine("Eduard Crypto Library - Benchmark Runner");
            Console.WriteLine("Version 1.0.0");
            Console.WriteLine();
            Console.WriteLine("Usage: dotnet run -- [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --bigint, -b     Execute BigInt arithmetic benchmarks");
            Console.WriteLine("  --polynomials, -p Execute polynomial operation benchmarks");
            Console.WriteLine("  --curves, -c     Execute elliptic curve cryptography benchmarks");
            Console.WriteLine("  --run-all, -a    Execute complete benchmark suite");
            Console.WriteLine("  --help, -h       Display this help information");
            Console.WriteLine();
            Console.WriteLine("Benchmark Categories:");
            Console.WriteLine();
            Console.WriteLine("  BigInt Operations:");
            Console.WriteLine("    - Modular Exponentiation: Binary vs Sliding Window");
            Console.WriteLine("    - Multiplication: Standard, Karatsuba, and NTT-based");
            Console.WriteLine("    - Prime Number Generation: Probabilistic primality testing");
            Console.WriteLine("    - Modular Reduction: Barrett reduction vs standard modulo");
            Console.WriteLine();
            Console.WriteLine("  Polynomial Operations:");
            Console.WriteLine("    - Power Modulo: Binary exponentiation vs optimized algorithm");
            Console.WriteLine("    - Polynomial Reduction: Standard vs FFT-based");
            Console.WriteLine("    - Multiplication: Plain vs FFT-based convolution");
            Console.WriteLine("    - Squaring: Plain vs FFT-based optimization");
            Console.WriteLine();
            Console.WriteLine("  Elliptic Curves:");
            Console.WriteLine("    - Twisted Edwards Curves: Edwards25519, Edwards448");
            Console.WriteLine("    - Montgomery Curves: Curve25519, Curve448");
            Console.WriteLine("    - Weierstrass Curves: NIST P-192 to P-521, Wei25519, Wei448");
            Console.WriteLine("    - Scalar Multiplication: Binary, Mixed, Montgomery ladder, Fastest");
        }

        static void RunBigIntBenchmarks(IConfig config)
        {
            Console.WriteLine("\n[1/4] Modular Exponentiation Performance");
            Console.WriteLine("       Comparing binary and sliding window algorithms");
            BenchmarkRunner.Run<ExpoBenchmark>(config);

            Console.WriteLine("\n[2/4] Multiplication Algorithm Analysis");
            Console.WriteLine("       Standard, Karatsuba, and NTT implementations");
            BenchmarkRunner.Run<MultBenchmark>(config);

            Console.WriteLine("\n[3/4] Prime Number Generation");
            Console.WriteLine("       Probabilistic prime generation with 50 rounds of Miller-Rabin");
            BenchmarkRunner.Run<PrimeBenchmark>(config);

            Console.WriteLine("\n[4/4] Modular Reduction Methods");
            Console.WriteLine("       Barrett reduction vs standard modulo operation");
            BenchmarkRunner.Run<ReduceBenchmark>(config);

            Console.WriteLine("\n+ BigInt benchmark suite completed successfully.");
        }

        static void RunPolynomialBenchmarks(IConfig config)
        {
            Console.WriteLine("\n[1/4] Polynomial Power Modulo");
            Console.WriteLine("       Binary exponentiation vs optimized algorithm");
            BenchmarkRunner.Run<DegreePowBenchmark>(config);

            Console.WriteLine("\n[2/4] Polynomial Reduction");
            Console.WriteLine("       Standard long division vs FFT-based reduction");
            BenchmarkRunner.Run<ModBenchmark>(config);

            Console.WriteLine("\n[3/4] Polynomial Multiplication");
            Console.WriteLine("       Plain convolution vs FFT-based multiplication");
            BenchmarkRunner.Run<MultiBenchmark>(config);

            Console.WriteLine("\n[4/4] Polynomial Power Modulo (General)");
            Console.WriteLine("       Exponentiation of polynomials in quotient rings");
            BenchmarkRunner.Run<PowModBenchmark>(config);

            Console.WriteLine("\n+ Polynomial benchmark suite completed successfully.");
        }

        static void RunCurveBenchmarks(IConfig config)
        {
            Console.WriteLine("\n[1/3] Twisted Edwards Curves");
            Console.WriteLine("       Scalar multiplication on Edwards25519 and Edwards448");
            BenchmarkRunner.Run<EdwardsCurveBenchmark>(config);

            Console.WriteLine("\n[2/3] Montgomery Curves");
            Console.WriteLine("       Scalar multiplication on Curve25519 and Curve448");
            BenchmarkRunner.Run<MontyCurveBenchmark>(config);

            Console.WriteLine("\n[3/3] Weierstrass Curves");
            Console.WriteLine("       NIST curves and Wei25519/Wei448 with multiple algorithms");
            BenchmarkRunner.Run<WeiCurveBenchmark>(config);

            Console.WriteLine("\n+ Elliptic curve benchmark suite completed successfully.");
        }

        static void RunAllBenchmarks(IConfig config)
        {
            Console.WriteLine("\nEduard Crypto Library - Complete Benchmark Suite");
            Console.WriteLine("Starting comprehensive performance analysis...");
            Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();

            Console.WriteLine("Phase 1: BigInt Arithmetic");
            Console.WriteLine("──────────────────────────");
            RunBigIntBenchmarks(config);

            Console.WriteLine("\nPhase 2: Polynomial Operations");
            Console.WriteLine("───────────────────────────────");
            RunPolynomialBenchmarks(config);

            Console.WriteLine("\nPhase 3: Elliptic Curve Cryptography");
            Console.WriteLine("─────────────────────────────────────");
            RunCurveBenchmarks(config);

            Console.WriteLine("\n+ Complete benchmark suite finished successfully.");
            Console.WriteLine($"  Completion time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
#endif
    }
}