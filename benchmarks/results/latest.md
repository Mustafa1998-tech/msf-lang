# Benchmark Run

- Date: 2026-11-16 09:57:38 +03:00
- Machine: HF-MOSTAFAWR
- OS: Microsoft Windows 10.0.26100 
- .NET: 10.0.302
- Configuration: Release
- Iterations per program: 5
- Command: dotnet run --project src/MsfLang.Cli -c Release --no-build -- run benchmarks/programs/<program>

| Program | Output | Avg (ms) | Min (ms) | Max (ms) | Samples (ms) |
|---|---:|---:|---:|---:|---|
| factorial20.msf | 2.43290200817664e18 | 672.247 | 653.543 | 692.356 | 692.356, 688.882, 653.543, 661.668, 664.787 |
| sum_recursive_1000.msf | 499500 | 662.543 | 646.531 | 686.414 | 686.414, 669.962, 652.5, 646.531, 657.309 |
| arith_loop_500.msf | 313125 | 640.318 | 634.906 | 645.848 | 637.674, 634.906, 640.565, 642.599, 645.848 |
| fib20.msf | 6765 | 729.966 | 714.605 | 744.685 | 736.109, 726.071, 744.685, 728.361, 714.605 |
