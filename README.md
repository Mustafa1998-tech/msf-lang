<div align="center">

<img src="branding/logo/msf-lang-logo-approved.png" alt="MSF Lang" width="360" />

[Official brand guidelines](branding/BRAND-GUIDELINES.md)

# MSF Lang

**A small, precise, and testable programming language — designed and engineered by Mustafa Software Factory.**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Language](https://img.shields.io/badge/language-C%23-239120?logo=csharp&logoColor=white)](src/)
[![Spec](https://img.shields.io/badge/spec-v0.1%20draft-8B35F5)](docs/en/SPEC-v0.1.md)
[![Status](https://img.shields.io/badge/status-in%20development-00D4FF)](#project-status)
[![License: MIT](https://img.shields.io/badge/license-MIT-171334)](LICENSE)

*Deterministic semantics · clear diagnostics · implementation-first specification*

[User Guide](docs/guide/README.md) · [Specification](docs/en/SPEC-v0.1.md) · [Architecture](docs/ARCHITECTURE.md) · [Contributing](CONTRIBUTING.md) · [العربية](#العربية)

</div>

---

## Overview

**MSF Lang** (`.msf`) is a small, general-purpose, dynamically typed programming language built with a focus on:

- **Clarity** — explicit syntax, no implicit conversions, no truthiness
- **Precision** — stable diagnostic codes, 1-based Unicode-scalar locations, source excerpts
- **Testability** — specification-driven development with traceable milestones and recorded evidence
- **Craft** — a hand-written lexer, recursive-descent parser, semantic resolver, and tree-walk interpreter in C#/.NET

This repository contains the language specification, architecture decisions, grammar, implementation, tests, examples, benchmarks, and documentation.

> **Current state:** v0.1 implementation is feature-complete for the current scope and under release hardening. The core pipeline, CLI (`run`/`help`/`version`/`repl`), and expanded test suites are green (**175 tests passing**, including **71 conformance cases** and **15 CLI process tests**). M9 release-candidate preparation (final spec cross-check/evidence and RC tag) is in progress.

---

## Quick Start

### Prerequisites
- [.NET SDK 10.0+](https://dotnet.microsoft.com/download) (see [`global.json`](global.json))

### Build
```powershell
dotnet restore MSFLang.slnx
dotnet build MSFLang.slnx -c Release -warnaserror
```

### Run a program
```powershell
dotnet run --project src/MsfLang.Cli -c Release -- run examples/hello.msf
```

**Output**
```text
Hello, MSF!
```

### Run tests
```powershell
dotnet test MSFLang.slnx -c Release
```

### CLI
```text
msf run <path>       Execute a .msf program
msf repl             Interactive/piped REPL
msf --help | -h      Show usage
msf --version | -v   Print version
```

---

## Example

```msf
fn factorial(n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}

fn fibonacci(n) {
    if (n < 2) {
        return n;
    }
    return fibonacci(n - 1) + fibonacci(n - 2);
}

print(factorial(10));
print(fibonacci(10));
```

```text
3628800
55
```

More examples: [`examples/`](examples/)

---

## Language at a Glance (v0.1)

| Area | v0.1 Design |
|---|---|
| Values | `number` (finite IEEE-754 double), `string`, `boolean`, `nil` |
| Bindings | `let` (immutable), `var` (mutable), block scope |
| Control flow | `if` / `else if` / `else` |
| Functions | Top-level named functions, exact arity, direct recursion |
| Operators | Arithmetic, comparison, equality, short-circuit `and`/`or`, `not` |
| Output | Built-in `print(...)` |
| Diagnostics | Stable codes (`E1xxx`–`E5xxx`), line/column, excerpt, hint |
| Out of scope (v0.1) | Loops, collections, closures, modules/imports, I/O beyond `print`, stdlib |

See the normative specification: [English](docs/en/SPEC-v0.1.md) · [العربية](docs/ar/SPEC-v0.1.md)

---

## Architecture

```text
Source (.msf)
   |
   v
SourceText / line map
   |
   v
Lexer        -> tokens            (E1xxx)
   |
   v
Parser       -> AST               (E2xxx)
   |
   v
Resolver     -> bindings/IDs      (E3xxx)
   |
   v
Interpreter  -> execution/output  (E4xxx)
```

- **Core engine:** [`src/MsfLang.Core`](src/MsfLang.Core)
- **CLI:** [`src/MsfLang.Cli`](src/MsfLang.Cli)
- **Grammar:** [`grammar/msf-v0.1.ebnf`](grammar/msf-v0.1.ebnf)
- **Tests:** [`tests/`](tests/)
- **Architecture details:** [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)

---

## Project Status

| Milestone | Scope | Status |
|---|---|---|
| M0 | Specification, plan, decisions, grammar, brand concepts | ✅ Gate A approved |
| M1 | Solution scaffold & build | ✅ Complete |
| M2 | Source text & diagnostics | ✅ Complete |
| M3 | Lexer | ✅ Complete |
| M4 | Parser & AST | ✅ Complete |
| M5 | Resolver & semantic checks | ✅ Complete |
| M6 | Interpreter & runtime | ✅ Core implemented and tested (edge-case coverage expanded; 175 total tests passing) |
| M7 | CLI & REPL contract | ✅ `run/help/version/repl` implemented and tested (process-level CLI tests, piped/multiline REPL, rollback/incomplete-input fixes) |
| M8 | Conformance suite & examples | ✅ Harness + **71** conformance cases passing (60+ target reached) |
| M9 | Hardening & release candidate | 🚧 Full suite green (175/175), 10/10 manual acceptance scenarios verified, spec cross-check in progress, RC/tag pending |

Execution evidence is recorded in [`docs/TEST-EVIDENCE.md`](docs/TEST-EVIDENCE.md).

---

## Repository Structure

```text
.
|-- src/
|   |-- MsfLang.Core/        # Language engine (source, diagnostics, lex/parse/resolve/runtime/repl)
|   `-- MsfLang.Cli/         # msf command-line interface
|-- tests/
|   |-- conformance/         # .msf programs + .expected.json fixtures
|   |-- MsfLang.*.Tests/     # Unit, CLI, and conformance test projects
|   `-- MsfLang.TestSupport/
|-- examples/                # Runnable .msf programs
|-- benchmarks/              # Benchmark programs, runner, and results
|-- docs/                    # Specs, plans, guides, architecture, roadmap, decisions, evidence (EN/AR)
|-- grammar/                 # Machine-readable EBNF
|-- branding/                # Brand concepts and review materials
|-- MSFLang.slnx
|-- Directory.Build.props
`-- global.json
```

---

## Documentation

### User Guide
- [Guide Index](docs/guide/README.md)
- [Getting Started](docs/guide/getting-started.md)
- [Syntax & Language Basics](docs/guide/syntax.md)
- [CLI Reference](docs/guide/cli.md)
- [Diagnostics & Errors](docs/guide/diagnostics.md)
- [Examples](docs/guide/examples.md)
- [Benchmarks](docs/guide/benchmarks.md)
- [Security](docs/guide/security.md)

### Architecture, Vision & Direction
- [Architecture](docs/ARCHITECTURE.md)
- [Problem-Solving Direction](docs/PROBLEM-SOLVING.md)
- [Roadmap](docs/ROADMAP.md)

### Reference & Project Docs
- [Language Specification (EN)](docs/en/SPEC-v0.1.md)
- [مواصفة اللغة (AR)](docs/ar/SPEC-v0.1.md)
- [Implementation & Test Plan (EN)](docs/en/IMPLEMENTATION-PLAN.md)
- [خطة التنفيذ والاختبارات (AR)](docs/ar/IMPLEMENTATION-PLAN.md)
- [Architecture Decisions](docs/DECISIONS.md)
- [Test Evidence Log](docs/TEST-EVIDENCE.md)
- [Brand Concepts](branding/CONCEPTS.md)
- [Changelog](CHANGELOG.md)

### Community & Project
- [Contributing](CONTRIBUTING.md)
- [Code of Conduct](CODE_OF_CONDUCT.md)
- [Security Policy](SECURITY.md)
- [License](LICENSE)

---

## العربية

**MSF Lang** لغة برمجة صغيرة ودقيقة بامتداد `.msf`، طوّرتها **Mustafa Software Factory** مع تركيز على الوضوح، التشخيص الدقيق، والاختبارية.

- المواصفة والخطة والقرارات متاحة بالعربية والإنجليزية داخل [`docs/`](docs/)
- الحالة الحالية: اكتمال تنفيذ نطاق v0.1 الحالي مع تقسية للإطلاق؛ المحرك وواجهة الأوامر (`msf run`/`msf repl`) والاختبارات ناجحة (**175 اختبارًا** منها **71 حالة مطابقة** و**15 اختبار CLI**)، مع استمرار تجهيز مرشح الإطلاق (مراجعة تقاطع نهائية مع المواصفة/وسم RC)
- الكلمات المحجوزة إنجليزية في v0.1، مع دعم كامل للنصوص والتعليقات والمخرجات العربية (UTF-8)

للبدء سريعًا: راجع [دليل البدء](docs/guide/getting-started.md) و[المعمارية](docs/ARCHITECTURE.md) و[خارطة الطريق](docs/ROADMAP.md).

---

## Contributing

Contributions, issues, and design discussions are welcome. Please read [`CONTRIBUTING.md`](CONTRIBUTING.md) for guidelines. If you propose a language change, align it with the [specification](docs/en/SPEC-v0.1.md), [decisions log](docs/DECISIONS.md), and implementation plan, and include tests/evidence.

## Code of Conduct

Participation is governed by the [Code of Conduct](CODE_OF_CONDUCT.md).

## Security

To report a vulnerability, see the [Security Policy](SECURITY.md). Do not open public issues for security bugs.

## License

Distributed under the [MIT License](LICENSE).

---

<div align="center">

**MSF Lang** · Built with precision by Mustafa Software Factory

</div>

