# Benchmarks

This page explains how benchmarks are run for MSF Lang and how to interpret the results.

## Goal

The benchmark suite provides a **repeatable baseline** for the current reference implementation (spec-driven v0.1, tree-walk interpreter) so future changes can be compared objectively.

**Policy:** we measure performance before making any claim of superiority. Benchmark results are evidence for understanding and improvement, not marketing claims.

## What Is Measured

Current benchmarks measure **end-to-end CLI execution time**:

```powershell
dotnet run --project src/MsfLang.Cli -c Release --no-build -- run benchmarks/programs/<program>
```

Each timing sample includes:
- `dotnet` host/process overhead
- CLI startup and argument handling
- File read + lex + parse + resolve + interpret + output

Because of that, these numbers are **not pure interpreter micro-benchmarks**. They are best used for:
- regression tracking across commits/versions on similar environments
- sanity-checking performance impact of implementation changes
- establishing baselines before optimization work

We do **not** claim superiority over other languages or implementations without rigorous, fair, like-for-like comparisons (same workloads, environment controls, methodology, and documented limitations).

## Benchmark Programs

Located in [`benchmarks/programs/`](../../benchmarks/programs/):

- `factorial20.msf` — recursion and numeric multiplication
- `sum_recursive_1000.msf` — deep recursion/addition pattern (`sumTo(999, 0)` within call-depth limit)
- `arith_loop_500.msf` — recursive iteration-style arithmetic workload
- `fib20.msf` — exponential recursion (`fib(20)`) stressing call/expression overhead

> MSF Lang v0.1 has no loops; recursion is used to model repeated work.

## How to Run

```powershell
powershell -ExecutionPolicy Bypass -File benchmarks/run-benchmarks.ps1 -Iterations 5 -Configuration Release
```

Results are printed to the console and written to:
- [`benchmarks/results/latest.md`](../../benchmarks/results/latest.md)

## Latest Recorded Run (example)

See the committed result file for exact environment and samples:
- [Latest results](../../benchmarks/results/latest.md)

A recorded run on the development machine produced average end-to-end times roughly in the **~640–730 ms** range for the included programs (5 iterations each). In this mode, a significant portion of that time is process/startup overhead rather than language execution alone.

## Reproducibility Notes

When sharing benchmark results, always include:
- Date/time
- OS and machine/CPU details
- .NET SDK version
- Build configuration (`Release` recommended)
- Command used and iteration count
- Commit hash

## Future Improvements

Potential next steps for more detailed performance analysis:
- In-process micro-benchmarks (engine-only timing, excluding `dotnet run` overhead)
- Warmup/steady-state measurements
- Larger workload matrix and memory/allocation tracking
- CI-based trend tracking across commits
