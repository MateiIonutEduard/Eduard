#if RELEASE && USE_BENCHMARKING
using System;
using System.Linq;
using System.Collections.Generic;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using Eduard.BenchTests.Curves;
using Eduard.BenchTests.BigInt;
using Eduard.BenchTests.Poly;
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
            if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
            {
                ShowHelp();
                return;
            }

            var config = DefaultConfig.Instance;
            var parser = new CommandLineParser(args);

            if (parser.HasCommand("--bigint", "-b"))
                RunBigIntBenchmarks(config, parser.GetTestIndices("--bigint", "-b"));

            if (parser.HasCommand("--polynomials", "-p"))
                RunPolynomialBenchmarks(config, parser.GetTestIndices("--polynomials", "-p"));

            if (parser.HasCommand("--curves", "-c"))
                RunCurveBenchmarks(config, parser.GetTestIndices("--curves", "-c"));

            if (parser.HasCommand("--run-all", "-a"))
                RunAllBenchmarks(config);
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
            Console.WriteLine("Usage: dotnet run -- [command] [test indices...]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  --bigint, -b [indices]  Execute BigInt arithmetic benchmarks");
            Console.WriteLine("  --polynomials, -p [indices] Execute polynomial operation benchmarks");
            Console.WriteLine("  --curves, -c [indices]  Execute elliptic curve cryptography benchmarks");
            Console.WriteLine("  --run-all, -a           Execute complete benchmark suite");
            Console.WriteLine("  --help, -h              Display this help information");
            Console.WriteLine();
            Console.WriteLine("Test Indices:");
            Console.WriteLine();
            Console.WriteLine("  BigInt Operations:");
            Console.WriteLine("    1 - Modular Exponentiation (Binary vs Sliding Window)");
            Console.WriteLine("    2 - Multiplication (Standard, Karatsuba, NTT)");
            Console.WriteLine("    3 - Prime Number Generation (Miller-Rabin)");
            Console.WriteLine("    4 - Modular Reduction (Barrett vs Standard)");
            Console.WriteLine();
            Console.WriteLine("  Polynomial Operations:");
            Console.WriteLine("    1 - Polynomial Power Modulo (Binary vs Optimized)");
            Console.WriteLine("    2 - Polynomial Reduction (Long Division vs FFT)");
            Console.WriteLine("    3 - Polynomial Multiplication (Plain vs FFT)");
            Console.WriteLine("    4 - Polynomial Power Modulo (General)");
            Console.WriteLine("    5 - Polynomial Composition Modulo (Standard vs Improved Horner)");
            Console.WriteLine();
            Console.WriteLine("  Elliptic Curves:");
            Console.WriteLine("    1 - Twisted Edwards Curves (Edwards25519, Edwards448)");
            Console.WriteLine("    2 - Montgomery Curves (Curve25519, Curve448)");
            Console.WriteLine("    3 - Weierstrass Curves (NIST P-192 to P-521, Wei25519, Wei448)");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run -- --bigint 1 3          # Run BigInt tests 1 and 3");
            Console.WriteLine("  dotnet run -- --curves 2 3          # Run curve tests 2 and 3");
            Console.WriteLine("  dotnet run -- -p 1 2 4              # Run polynomial tests 1, 2, and 4");
            Console.WriteLine("  dotnet run -- --bigint --curves 3   # Run all BigInt and curve test 3");
            Console.WriteLine("  dotnet run -- --run-all             # Run complete benchmark suite");
        }

        static void RunBigIntBenchmarks(IConfig config, HashSet<int> testIndices)
        {
            var tests = new List<(int index, Action<IConfig> run)>
            {
                (1, (cfg) => {
                    Console.WriteLine("\n[1/4] Modular Exponentiation Performance");
                    Console.WriteLine("       Comparing binary and sliding window algorithms");
                    BenchmarkRunner.Run<ExpoBenchmark>(cfg);
                }),
                (2, (cfg) => {
                    Console.WriteLine("\n[2/4] Multiplication Algorithm Analysis");
                    Console.WriteLine("       Standard, Karatsuba, and NTT implementations");
                    BenchmarkRunner.Run<MultBenchmark>(cfg);
                }),
                (3, (cfg) => {
                    Console.WriteLine("\n[3/4] Prime Number Generation");
                    Console.WriteLine("       Probabilistic prime generation with 50 rounds of Miller-Rabin");
                    BenchmarkRunner.Run<PrimeBenchmark>(cfg);
                }),
                (4, (cfg) => {
                    Console.WriteLine("\n[4/4] Modular Reduction Methods");
                    Console.WriteLine("       Barrett reduction vs standard modulo operation");
                    BenchmarkRunner.Run<ReduceBenchmark>(cfg);
                })
            };

            var selected = tests.Where(t => testIndices.Contains(t.index)).ToList();

            if (!selected.Any())
            {
                Console.WriteLine("No valid test indices specified for BigInt benchmarks.");
                Console.WriteLine("Available indices: 1, 2, 3, 4");
                return;
            }

            Console.WriteLine("\nBigInt Benchmarks");
            Console.WriteLine("─────────────────");

            foreach (var test in selected)
                test.run(config);

            Console.WriteLine("\n+ BigInt benchmark suite completed successfully.");
        }

        static void RunPolynomialBenchmarks(IConfig config, HashSet<int> testIndices)
        {
            var tests = new List<(int index, Action<IConfig> run)>
            {
                (1, (cfg) => {
                    Console.WriteLine("\n[1/5] Polynomial Power Modulo");
                    Console.WriteLine("       Binary exponentiation vs optimized algorithm");
                    BenchmarkRunner.Run<DegreePowBenchmark>(cfg);
                }),
                (2, (cfg) => {
                    Console.WriteLine("\n[2/5] Polynomial Reduction");
                    Console.WriteLine("       Standard long division vs FFT-based reduction");
                    BenchmarkRunner.Run<ModBenchmark>(cfg);
                }),
                (3, (cfg) => {
                    Console.WriteLine("\n[3/5] Polynomial Multiplication");
                    Console.WriteLine("       Plain convolution vs FFT-based multiplication");
                    BenchmarkRunner.Run<MultiBenchmark>(cfg);
                }),
                (4, (cfg) => {
                    Console.WriteLine("\n[4/5] Polynomial Power Modulo (General)");
                    Console.WriteLine("       Exponentiation of polynomials in quotient rings");
                    BenchmarkRunner.Run<PowModBenchmark>(cfg);
                }),
                (5, (cfg) => {
                    Console.WriteLine("\n[5/5] Polynomial Composition Modulo (Standard vs Improved Horner)");
                    Console.WriteLine("       Modular composition of polynomials in quotient rings");
                    BenchmarkRunner.Run<ComposeModBenchmark>(cfg);
                })
            };

            var selected = tests.Where(t => testIndices.Contains(t.index)).ToList();

            if (!selected.Any())
            {
                Console.WriteLine("No valid test indices specified for Polynomial benchmarks.");
                Console.WriteLine("Available indices: 1, 2, 3, 4, 5");
                return;
            }

            Console.WriteLine("\nPolynomial Benchmarks");
            Console.WriteLine("─────────────────────");

            foreach (var test in selected)
                test.run(config);

            Console.WriteLine("\n+ Polynomial benchmark suite completed successfully.");
        }

        static void RunCurveBenchmarks(IConfig config, HashSet<int> testIndices)
        {
            var tests = new List<(int index, Action<IConfig> run)>
            {
                (1, (cfg) => {
                    Console.WriteLine("\n[1/3] Twisted Edwards Curves");
                    Console.WriteLine("       Scalar multiplication on Edwards25519 and Edwards448");
                    BenchmarkRunner.Run<EdwardsCurveBenchmark>(cfg);
                }),
                (2, (cfg) => {
                    Console.WriteLine("\n[2/3] Montgomery Curves");
                    Console.WriteLine("       Scalar multiplication on Curve25519 and Curve448");
                    BenchmarkRunner.Run<MontyCurveBenchmark>(cfg);
                }),
                (3, (cfg) => {
                    Console.WriteLine("\n[3/3] Weierstrass Curves");
                    Console.WriteLine("       NIST curves and Wei25519/Wei448 with multiple algorithms");
                    BenchmarkRunner.Run<WeiCurveBenchmark>(cfg);
                })
            };

            var selected = tests.Where(t => testIndices.Contains(t.index)).ToList();

            if (!selected.Any())
            {
                Console.WriteLine("No valid test indices specified for Curve benchmarks.");
                Console.WriteLine("Available indices: 1, 2, 3");
                return;
            }

            Console.WriteLine("\nElliptic Curve Benchmarks");
            Console.WriteLine("──────────────────────────");

            foreach (var test in selected)
                test.run(config);

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
            RunBigIntBenchmarks(config, new HashSet<int> { 1, 2, 3, 4 });

            Console.WriteLine("\nPhase 2: Polynomial Operations");
            Console.WriteLine("───────────────────────────────");
            RunPolynomialBenchmarks(config, new HashSet<int> { 1, 2, 3, 4, 5 });

            Console.WriteLine("\nPhase 3: Elliptic Curve Cryptography");
            Console.WriteLine("─────────────────────────────────────");
            RunCurveBenchmarks(config, new HashSet<int> { 1, 2, 3 });

            Console.WriteLine("\n+ Complete benchmark suite finished successfully.");
            Console.WriteLine($"  Completion time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
#endif
    }
}