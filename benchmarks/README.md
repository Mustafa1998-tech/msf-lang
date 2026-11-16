# MSF Lang Benchmarks

This directory contains simple, reproducible benchmark programs and a runner for measuring **end-to-end CLI execution time** of the current MSF Lang implementation.

## Contents

- `programs/` — `.msf` benchmark programs
- `run-benchmarks.ps1` — benchmark runner (PowerShell)
- `results/latest.md` — latest recorded run (generated)

## Programs

| Program | What it measures | Notes |
|---|---|---|
| `factorial20.msf` | Recursive function calls + multiplication | Returns `2432902008176640000` (displayed in numeric form by runtime formatting) |
| `sum_recursive_1000.msf` | Deep recursion + addition (tail-shaped recursion in source, not TCO) | Uses `sumTo(999, 0)` to stay within the v0.1 call-depth limit |
| `arith_loop_500.msf` | Recursive loop pattern with arithmetic operations | 500 iterations via recursion |
| `fib20.msf` | Exponential recursive workload (`fib(20)`) | Stresses repeated function calls and expression evaluation |

> v0.1 has no `while/for` loops, so iteration-style benchmarks are expressed with recursion and must respect the language call-depth limit.

## Run

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File benchmarks/run-benchmarks.ps1 -Iterations 5 -Configuration Release
```

The script:
1. Builds `src/MsfLang.Cli` in `Release`
2. Runs each benchmark multiple times with:
   `dotnet run --project src/MsfLang.Cli -c Release --no-build -- run benchmarks/programs/<program>`
3. Reports average/min/max/sample timings and program output
4. Writes results to `benchmarks/results/latest.md`

## Methodology and Interpretation

- **Measurement type:** end-to-end wall-clock time per CLI invocation (includes `dotnet` host/startup overhead and process execution around `msf run`)
- **Not a pure interpreter micro-benchmark:** results are useful for tracking overall CLI/runtime trends, but they do **not** isolate lexer/parser/resolver/interpreter time alone
- **Environment-dependent:** CPU, OS, .NET SDK/runtime, thermal state, and background load affect numbers
- **Correctness-first implementation:** MSF Lang is in active development (v0.1 draft, tree-walk interpreter); benchmark results should be compared against future versions of the same project, not treated as cross-language leaderboard numbers
- **No superiority claims without evidence:** we measure performance before making any claim of superiority. Any comparison with other languages/engines must be fair, reproducible, and clearly documented.

See also: [`docs/guide/benchmarks.md`](../docs/guide/benchmarks.md) and [`benchmarks/results/latest.md`](results/latest.md).
