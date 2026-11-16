# MSF Lang Architecture

This document describes the current architecture and implementation structure of the MSF Lang reference implementation (v0.1, in active development).

> Status: implementation documentation for the current codebase. Normative language behavior is defined in [`docs/en/SPEC-v0.1.md`](en/SPEC-v0.1.md).

## 1. Goals and Design Principles

- **Spec-driven implementation:** behavior follows the v0.1 specification and ADRs.
- **Small, inspectable pipeline:** hand-written front-end stages and a tree-walk interpreter.
- **Precise diagnostics:** stable codes, phases, spans, and formatted source excerpts.
- **Deterministic core:** no clock/random/input/network/environment access in v0.1 runtime semantics.
- **No third-party runtime dependencies:** Core/CLI use .NET BCL only.

## 2. High-Level Pipeline

```text
.msf source (UTF-8)
   │
   ▼
SourceText (line map, BOM handling, Unicode-scalar positions)
   │
   ▼
Lexer (tokens, E1xxx)
   │
   ▼
Parser / AST (recursive descent, E2xxx)
   │
   ▼
Resolver / SemanticModel (scopes, bindings, static checks, E3xxx)
   │
   ▼
Interpreter (tree-walk execution, runtime checks, E4xxx)
   │
   ▼
stdout / stderr diagnostics / exit code
```

`MsfEngine.Run` orchestrates the pipeline and maps language exceptions to process-level results (`0` success, `1` compile-time, `2` runtime).

## 3. Projects and Responsibilities

| Area | Path | Responsibility |
|---|---|---|
| Core engine | `src/MsfLang.Core/` | Source model, diagnostics, lexing, parsing, semantic analysis, runtime, REPL session logic |
| CLI | `src/MsfLang.Cli/` | Command parsing, file/IO handling, process exit codes, REPL I/O loop and presentation |
| Conformance tests | `tests/conformance/` + `tests/MsfLang.Conformance.Tests/` | `.msf` + `.expected.json` behavioral fixtures and harness |
| Core tests | `tests/MsfLang.Core.Tests/` | Unit/integration-style tests for core components |
| CLI tests | `tests/MsfLang.Cli.Tests/` | Process-level CLI behavior tests |
| Examples/docs/benchmarks | `examples/`, `docs/`, `benchmarks/` | Usage samples, specifications/guides/evidence, baseline measurements |

## 4. Core Components

### 4.1 Source and Positions (`Source/`)
- `SourceText` stores normalized text (leading UTF-8 BOM removed), path, and precomputed line starts.
- Positions are 1-based line/column values; columns are counted in **Unicode scalar values**.
- Supports LF/CRLF/CR line breaks and line-text extraction for diagnostics.

### 4.2 Diagnostics (`Diagnostics/`)
- `Diagnostic` carries `Code`, `Phase`, `Message`, `Span`, optional `Hint`.
- Phase-specific exception types (`LexException`, `SyntaxException`, `SemanticException`, `RuntimeException`) are used to stop at the first failing phase.
- `DiagnosticFormatter` renders the spec-style excerpt with caret alignment (including tab preservation in the caret prefix).

### 4.3 Lexer (`Lexing/`)
- Hand-written scanner producing tokens with spans and literal values.
- Recognizes keywords, identifiers, numbers, strings/escapes, operators/punctuation, and comments.
- Emits lexical diagnostics (e.g. unexpected character, invalid escape, unterminated string/comment, non-finite numeric literal cases as implemented).

### 4.4 Parser and AST (`Syntax/`)
- Recursive-descent parser following v0.1 grammar/precedence with span-preserving immutable AST records.
- Produces declarations/statements/expressions and enforces syntax-level constraints.
- Enforces parser nesting safety limit (256) and reports syntax diagnostics (`E2xxx`).

### 4.5 Semantic Analysis (`Semantics/`)
- `Resolver` performs lexical scope resolution and static checks in textual order.
- Assigns numeric binding IDs and records mappings in `SemanticModel` (AST node → binding; parameter bindings for functions).
- Enforces v0.1 rules such as declaration/assignment validity, top-level function restrictions, direct-call arity checks, `return` placement, and value-position restrictions for callables.
- Builtin `print` is predeclared/reserved in the global scope.

### 4.6 Runtime (`Runtime/`)
- Tree-walk `Interpreter` evaluates the resolved AST using ID-keyed environments (`MsEnvironment`).
- Value model: `number` (finite `double`), `string`, `boolean`, `nil` (plus internal callable values).
- Implements operator semantics, strict boolean conditions, short-circuit logic, function calls/returns, recursion depth limit (1000 user calls), and builtin `print`.
- `ValueFormatter` provides display formatting for program output/REPL auto-display.

### 4.7 REPL (`Repl/`)
- `ReplSession` maintains persistent global runtime/semantic state across entries.
- Evaluates entries as program fragments, returns display values for top-level non-`nil` expression statements, and attempts transactional rollback on compile/runtime failures.
- CLI owns terminal/piped I/O behavior (prompts/banner, `:help/:quit`, continuation handling).

## 5. CLI Architecture (`src/MsfLang.Cli/`)

- Single entry point dispatches `run`, `repl`, `--help/-h`, `--version/-v`.
- Validates usage and reports CLI/I-O diagnostics with documented exit codes (`3` usage, `4` I/O).
- `run`: reads source as strict UTF-8, executes via `MsfEngine`, writes formatted diagnostics to stderr.
- `repl`: interactive vs redirected-stdin behavior, line accumulation/continuation prompts, command handling, auto-display output.
- Stream/encoding conventions: stdout UTF-8 without BOM, LF line endings, diagnostics to stderr.

## 6. Testing and Evidence

- **Core tests:** validate source/diagnostics, lexing, parsing, semantics, and REPL session basics.
- **Conformance tests:** fixture-driven `.msf`/`.expected.json` cases compare exit code, stdout, and diagnostic metadata (code/line/column/phase) and detect orphan fixtures.
- **CLI tests:** process-level checks for usage/help/version, run success/error/I-O cases, and piped REPL behavior.
- **Evidence:** executed commands and outcomes are recorded in [`docs/TEST-EVIDENCE.md`](TEST-EVIDENCE.md).

## 7. Known Implementation Notes (Current)

- **REPL internals:** current session integration uses reflection/internal access patterns to bridge resolver/runtime snapshots; this is functional but a maintainability risk and a candidate for refactoring behind explicit APIs.
- **Spec-contract hardening:** core behavior is implemented and substantially tested, but full M7/M9 contract edge cases (especially REPL rollback/incomplete-input details and complete C-*/R-* matrices) remain in progress.
- **Nesting/diagnostic precision:** parser nesting accounting and exact diagnostic text/placement for all catalog cases should continue to be validated against the normative spec as coverage expands.

## 8. Evolution Guidance

- Keep `SPEC-v0.1.md` and ADRs as the source of truth for behavior changes.
- Prefer adding conformance cases before/with behavior changes for externally visible semantics.
- Stabilize semantics and public contracts before larger runtime/backend optimizations.
- When expanding language features (v0.2+), introduce ADR + spec updates + tests/evidence together.
