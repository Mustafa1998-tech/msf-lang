<div align="center">

<img src="branding/logo/msf-lang-logo.png" alt="MSF Lang" width="360" />

[Official brand guidelines](branding/BRAND-GUIDELINES.md)

# MSF Lang

**A small, precise, and testable programming language â€” designed and engineered by Mustafa Software Factory.**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Language](https://img.shields.io/badge/language-C%23-239120?logo=csharp&logoColor=white)](src/)
[![Spec](https://img.shields.io/badge/spec-v0.1%20draft-8B35F5)](docs/en/SPEC-v0.1.md)
[![Status](https://img.shields.io/badge/status-in%20development-00D4FF)](#project-status)
[![License: MIT](https://img.shields.io/badge/license-MIT-171334)](LICENSE)

*Deterministic semantics Â· clear diagnostics Â· implementation-first specification*

[User Guide](docs/guide/README.md) Â· [Specification](docs/en/SPEC-v0.1.md) Â· [Implementation Plan](docs/en/IMPLEMENTATION-PLAN.md) Â· [Decisions](docs/DECISIONS.md) Â· [Ø§Ù„Ø¹Ø±Ø¨ÙŠØ©](#Ø§Ù„Ø¹Ø±Ø¨ÙŠØ©)

</div>

---

## Overview

**MSF Lang** (`.msf`) is a small, general-purpose, dynamically typed programming language built with a focus on:

- **Clarity** â€” explicit syntax, no implicit conversions, no truthiness
- **Precision** â€” stable diagnostic codes, 1-based Unicode-scalar locations, source excerpts
- **Testability** â€” specification-driven development with traceable milestones and recorded evidence
- **Craft** â€” a hand-written lexer, recursive-descent parser, semantic resolver, and tree-walk interpreter in C#/.NET

This repository contains the language specification, architecture decisions, grammar, branding concepts, and the evolving reference implementation.

> **Current state:** active development. Core pipeline (source/diagnostics â†’ lexer â†’ parser â†’ resolver â†’ interpreter) is implemented for key v0.1 features, and `msf run` executes real `.msf` programs. The CLI/REPL contract, full conformance suite, and release hardening are in progress.

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
| Diagnostics | Stable codes (`E1xxx`â€“`E5xxx`), line/column, excerpt, hint |
| Out of scope (v0.1) | Loops, collections, closures, modules/imports, I/O beyond `print`, stdlib |

See the normative specification: [English](docs/en/SPEC-v0.1.md) Â· [Ø§Ù„Ø¹Ø±Ø¨ÙŠØ©](docs/ar/SPEC-v0.1.md)

---

## Architecture

```text
Source (.msf)
   â”‚
   â–¼
SourceText / line map
   â”‚
   â–¼
Lexer        â†’ tokens            (E1xxx)
   â”‚
   â–¼
Parser       â†’ AST               (E2xxx)
   â”‚
   â–¼
Resolver     â†’ bindings/IDs      (E3xxx)
   â”‚
   â–¼
Interpreter  â†’ execution/output  (E4xxx)
```

- **Core engine:** [`src/MsfLang.Core`](src/MsfLang.Core)
- **CLI:** [`src/MsfLang.Cli`](src/MsfLang.Cli)
- **Grammar:** [`grammar/msf-v0.1.ebnf`](grammar/msf-v0.1.ebnf)
- **Tests:** [`tests/`](tests/)

---

## Project Status

| Milestone | Scope | Status |
|---|---|---|
| M0 | Specification, plan, decisions, grammar, brand concepts | âœ… Gate A approved |
| M1 | Solution scaffold & build | âœ… Complete |
| M2 | Source text & diagnostics | âœ… Complete |
| M3 | Lexer | âœ… Complete |
| M4 | Parser & AST | âœ… Complete |
| M5 | Resolver & semantic checks | âœ… Complete |
| M6 | Interpreter & runtime | 🚧 Core implemented (values, operators, control flow, functions/recursion, `print`, depth limit) + examples/tests green; edge-case hardening ongoing |
| M7 | CLI & REPL contract | 🚧 `run/help/version/repl` implemented; process-level tests added; full spec-contract edge cases ongoing |
| M8 | Conformance suite & examples | 🚧 Harness + **63** conformance cases passing (60+ target reached; matrix/hardening ongoing) |
| M9 | Hardening & release candidate | â³ Planned |

Execution evidence is recorded in [`docs/TEST-EVIDENCE.md`](docs/TEST-EVIDENCE.md).

---

## Repository Structure

```text
.
â”œâ”€â”€ src/
â”‚   â”œâ”€â”€ MsfLang.Core/        # Language engine (source, diagnostics, lex/parse/resolve/runtime/repl)
â”‚   â””â”€â”€ MsfLang.Cli/         # msf command-line interface
â”œâ”€â”€ tests/
â”‚   â”œâ”€â”€ conformance/         # .msf programs + .expected.json fixtures
â”‚   â”œâ”€â”€ MsfLang.*.Tests/     # Unit, CLI, and conformance test projects
â”‚   â””â”€â”€ MsfLang.TestSupport/
â”œâ”€â”€ examples/                # Runnable .msf programs
â”œâ”€â”€ benchmarks/              # Benchmark programs, runner, and results
â”œâ”€â”€ docs/                    # Specs, plans, guides, roadmap, decisions, evidence (EN/AR)
â”œâ”€â”€ grammar/                 # Machine-readable EBNF
â”œâ”€â”€ branding/                # Brand concepts and review materials
â”œâ”€â”€ MSFLang.slnx
â”œâ”€â”€ Directory.Build.props
â””â”€â”€ global.json
```

---

## Documentation

### User Guide (How to use MSF Lang)
- [Guide Index](docs/guide/README.md)
- [Getting Started](docs/guide/getting-started.md)
- [Syntax & Language Basics](docs/guide/syntax.md)
- [CLI Reference](docs/guide/cli.md)
- [Diagnostics & Errors](docs/guide/diagnostics.md)
- [Examples](docs/guide/examples.md)
- [Benchmarks](docs/guide/benchmarks.md)

### Vision & Direction
- [Problem-Solving Direction](docs/PROBLEM-SOLVING.md)
- [Roadmap](docs/ROADMAP.md)

### Reference & Project Docs
- [Language Specification (EN)](docs/en/SPEC-v0.1.md)
- [Ù…ÙˆØ§ØµÙØ© Ø§Ù„Ù„ØºØ© (AR)](docs/ar/SPEC-v0.1.md)
- [Implementation & Test Plan (EN)](docs/en/IMPLEMENTATION-PLAN.md)
- [Ø®Ø·Ø© Ø§Ù„ØªÙ†ÙÙŠØ° ÙˆØ§Ù„Ø§Ø®ØªØ¨Ø§Ø±Ø§Øª (AR)](docs/ar/IMPLEMENTATION-PLAN.md)
- [Architecture Decisions](docs/DECISIONS.md)
- [Test Evidence Log](docs/TEST-EVIDENCE.md)
- [Brand Concepts](branding/CONCEPTS.md)

---

## Ø§Ù„Ø¹Ø±Ø¨ÙŠØ©

**MSF Lang** Ù„ØºØ© Ø¨Ø±Ù…Ø¬Ø© ØµØºÙŠØ±Ø© ÙˆØ¯Ù‚ÙŠÙ‚Ø© Ø¨Ø§Ù…ØªØ¯Ø§Ø¯ `.msf`ØŒ Ø·ÙˆØ±ØªÙ‡Ø§ **Mustafa Software Factory** Ù…Ø¹ ØªØ±ÙƒÙŠØ² Ø¹Ù„Ù‰ Ø§Ù„ÙˆØ¶ÙˆØ­ØŒ Ø§Ù„ØªØ´Ø®ÙŠØµ Ø§Ù„Ø¯Ù‚ÙŠÙ‚ØŒ ÙˆØ§Ù„Ø§Ø®ØªØ¨Ø§Ø±ÙŠØ©.

- Ø§Ù„Ù…ÙˆØ§ØµÙØ© ÙˆØ§Ù„Ø®Ø·Ø© ÙˆØ§Ù„Ù‚Ø±Ø§Ø±Ø§Øª Ù…ØªØ§Ø­Ø© Ø¨Ø§Ù„Ø¹Ø±Ø¨ÙŠØ© ÙˆØ§Ù„Ø¥Ù†Ø¬Ù„ÙŠØ²ÙŠØ© Ø¯Ø§Ø®Ù„ [`docs/`](docs/)
- Ø§Ù„Ø­Ø§Ù„Ø© Ø§Ù„Ø­Ø§Ù„ÙŠØ©: ØªÙ†ÙÙŠØ° Ù†ÙˆØ§Ø© Ø§Ù„Ù„ØºØ© (Ø§Ù„Ù…Ø¹Ø¬Ù…ØŒ Ø§Ù„Ù…Ø­Ù„Ù„ØŒ Ø§Ù„Ø¯Ù„Ø§Ù„Ø§ØªØŒ Ø§Ù„Ù…ÙØ³Ø±) ÙˆØªØ´ØºÙŠÙ„ Ø§Ù„Ø¨Ø±Ø§Ù…Ø¬ Ø¹Ø¨Ø± `msf run` Ùˆ`msf repl` (Ø£Ø³Ø§Ø³ÙŠ)ØŒ Ù…Ø¹ Ø§Ø³ØªÙ…Ø±Ø§Ø± Ø§Ù„Ø¹Ù…Ù„ Ø¹Ù„Ù‰ Ø§ÙƒØªÙ…Ø§Ù„ Ø§Ù„Ø¹Ù‚Ø¯ØŒ ÙˆØ­Ø²Ù…Ø© Ø§Ù„Ù…Ø·Ø§Ø¨Ù‚Ø©ØŒ ÙˆØ§Ù„ØªÙ‚Ø³ÙŠØ©
- Ø§Ù„ÙƒÙ„Ù…Ø§Øª Ø§Ù„Ù…Ø­Ø¬ÙˆØ²Ø© Ø¥Ù†Ø¬Ù„ÙŠØ²ÙŠØ© ÙÙŠ v0.1ØŒ Ù…Ø¹ Ø¯Ø¹Ù… ÙƒØ§Ù…Ù„ Ù„Ù„Ù†ØµÙˆØµ ÙˆØ§Ù„ØªØ¹Ù„ÙŠÙ‚Ø§Øª ÙˆØ§Ù„Ù…Ø®Ø±Ø¬Ø§Øª Ø§Ù„Ø¹Ø±Ø¨ÙŠØ©

---

## Contributing

Contributions, issues, and design discussions are welcome. If you propose a language change, please align it with the [specification](docs/en/SPEC-v0.1.md), [decisions log](docs/DECISIONS.md), and implementation plan.

## License

Distributed under the [MIT License](LICENSE).

---

<div align="center">

**MSF Lang** Â· Built with precision by Mustafa Software Factory

</div>
