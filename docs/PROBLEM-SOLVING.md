# MSF Lang: Solving Real Problems

MSF Lang aims to be a clear, reliable language for solving real problems — not just a language specification demo. This document defines what the language can solve today, what it cannot yet solve well, and the highest-impact path to practical usefulness.

## 1. What MSF Lang Can Solve Today (v0.1 core)

With the currently implemented v0.1 core, MSF Lang is suitable for **small, deterministic, expression-oriented logic**:

- **Arithmetic and numeric computations** (finite numbers, recursion-based algorithms)
- **Conditional logic and decision rules** (if/else chains, boolean logic)
- **Functional decomposition** (named functions, parameters, return values)
- **Recursive algorithms** (factorial, Fibonacci, GCD-style logic, bounded recursion)
- **Text construction/output** via `print` and string concatenation
- **Learning and validation** of language semantics, diagnostics, and interpreter behavior

### Good current examples
- Math/logic exercises and algorithm demos expressed with recursion
- Rule evaluation (classification, scoring with simple formulas)
- Deterministic scripts that transform inputs encoded in code and print results

## 2. Current Limitations (Why Many Problems Are Hard Today)

v0.1 intentionally omits features that are essential for most real-world tasks:

| Missing capability | Impact |
|---|---|
| Loops (`while`/`for`) | Iterative problems are awkward; recursion depth limits apply |
| Collections (arrays/lists/maps) | Cannot model datasets, lists, tables, or key-value structures |
| Modules/imports | Cannot organize multi-file programs or reuse libraries |
| Standard library | No built-in math/string/collection/file utilities |
| Input/File/Network I/O | Cannot read external data or integrate with systems |
| Error handling constructs | No structured exception handling beyond fail-fast runtime errors |
| Tooling ecosystem | Limited editor/debug/package workflows (in progress) |

## 3. Problem Domains We Want MSF Lang to Solve

To become genuinely useful, MSF Lang should progressively target these domains:

### A. Scripting and automation (near-term priority)
- Read/process text and structured data
- Transform files and generate reports
- Run deterministic tasks with clear diagnostics

### B. Data processing and utilities
- Work with lists/maps/records
- Parse and format data (CSV/JSON-like workflows via libraries)
- Implement reusable utility modules

### C. Education and tooling
- Teach programming concepts with precise, understandable errors
- Provide stable semantics for exercises, graders, and language tooling
- Support analysis tools (formatter, LSP, debugger) on stable semantics

### D. Application logic / embedded rules engines (later)
- Deterministic rule evaluation
- Domain-specific calculations and validations
- Embeddable execution with well-defined host boundaries

## 4. Highest-Impact Roadmap (Problem-Solving First)

> Principle: **stabilize semantics before expansion**. New features must be added through spec/ADR + tests + evidence (and we measure performance before any superiority claims).

### Phase 1 — Finish and Stabilize v0.1 (foundation)
**Why:** a reliable core is required before solving bigger problems.
- Complete M6/M7/M8/M9 (runtime edge cases, CLI/REPL contract, conformance suite, hardening)
- Ensure deterministic behavior and diagnostic quality across supported cases

### Phase 2 — v0.2 "Practical Programming" (biggest usability jump)
**Why:** unlocks everyday problem solving.
1. **Loops and loop control** (`while`, and likely `break`/`continue`) to remove recursion-only constraints
2. **Collections** (at least arrays/lists; then maps) with clear iteration/access semantics
3. **Core string/number operations** (length, indexing/slicing strategy, parsing/formatting helpers)
4. **Functions/closures (scoped carefully)** if needed for expressive reusable abstractions

### Phase 3 — Standard Library (usable out of the box)
**Why:** users need tools, not just syntax.
- Math utilities
- String utilities
- Collection utilities
- Time/random (explicit, documented semantics)
- Structured error/result helpers (design-dependent)

### Phase 4 — I/O, Modules, and Packages (real projects)
**Why:** enables integration and multi-file software.
- File I/O and stdin/stdout/stderr APIs (specified and safe boundaries)
- Module/import system
- Package manifest, dependency workflow, and distribution model
- Tooling: formatter, LSP, debugger, project workflows

### Phase 5 — Performance/Compiler Backends (after semantics are stable)
**Why:** scale and deployment, without changing meaning.
- Bytecode/VM and/or compiler backends as semantics-preserving implementations
- Benchmarks/regression tracking for every optimization step

## 5. Immediate Next Steps (Recommended)

1. **Complete v0.1 launch readiness** (conformance + CLI/REPL contract + hardening)
2. **Define v0.2 problem-solving scope** with ADRs:
   - loops and control flow
   - collection types and operations
   - minimal stdlib API surface
3. **Prototype against real tasks** (scripts/utilities/educational exercises) to validate that new features actually solve target problems
4. **Document use-case-driven examples** alongside the spec (not just language-feature demos)

## 6. Design Rule for Future Features

A feature is accepted when it demonstrably improves problem-solving **and** is:
- specified precisely (EN/AR as applicable),
- implemented with diagnostics and tests,
- covered by conformance/evidence,
- evaluated for compatibility and performance impact.

---

MSF Lang will become a problem-solving language by growing deliberately: stable semantics first, then expressive features and libraries, then ecosystem and compiler performance — with evidence at every step.