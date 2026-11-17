# MSF Lang Guide

Welcome to the MSF Lang user guide. This section explains how to build, run, and write programs in **MSF Lang v0.3**: the v0.1 core, the additive **v0.2 practical draft** (`while`, `break`/`continue`, lists, `len`, `append`, maps, math/string builtins), and **v0.3 local file modules** (`import`/`export`, qualified access).

> MSF Lang is under active development. This guide documents the currently implemented behavior and clearly notes features that are planned but not yet available.

## Contents

- [Getting Started](getting-started.md) — prerequisites, build, and your first program
- [Syntax & Language Basics](syntax.md) — values, variables, operators, control flow, functions
- [CLI Reference](cli.md) — `msf run`, help/version, exit codes, and current limitations
- [Diagnostics & Errors](diagnostics.md) — how to read errors and common error codes
- [Examples](examples.md) — runnable sample programs
- [Benchmarks](benchmarks.md) — performance benchmarks, methodology, and latest results
- [Security](security.md) — security properties, scope, and reporting

## Quick Example

```msf
fn greet(name) {
    print("Hello,", name);
}

let who = "MSF";
greet(who);
```

Run:

```powershell
dotnet run --project src/MsfLang.Cli -c Release -- run examples/hello.msf
```

## Related Documents

- [Language Specification (EN)](../en/SPEC-v0.1.md) · [v0.2 practical](../en/SPEC-v0.2-practical.md) · [v0.3 modules](../en/SPEC-v0.3-modules.md)
- [مواصفة اللغة (AR)](../ar/SPEC-v0.1.md) · [v0.2 العملي](../ar/SPEC-v0.2-practical.md) · [v0.3 الوحدات](../ar/SPEC-v0.3-modules.md)
- [Architecture](../ARCHITECTURE.md)
- [Implementation Plan](../en/IMPLEMENTATION-PLAN.md)
- [Test Evidence](../TEST-EVIDENCE.md)
