# MSF Lang Architecture

This document describes the current architecture and implementation structure of the MSF Lang reference implementation (v0.3, release-candidate stage).

> Status: implementation documentation for the current codebase. Normative language behavior is defined in [`docs/en/SPEC-v0.1.md`](en/SPEC-v0.1.md) (v0.1), the v0.2 practical specs, and [`docs/en/SPEC-v0.3-modules.md`](en/SPEC-v0.3-modules.md) (v0.3 modules).

## 1. Goals and Design Principles

- **Spec-driven implementation:** behavior follows the v0.1 specification, ADRs, and numbered addendum specs (v0.2/v0.3).
- **Small, inspectable pipeline:** hand-written front-end stages and a tree-walk interpreter.
- **Precise diagnostics:** stable codes, phases, spans, and formatted source excerpts.
- **Deterministic core:** no clock/random/input/network/environment access in v0.1 runtime semantics.
- **No third-party runtime dependencies:** Core/CLI use .NET BCL only.

## 2. High-Level Pipeline

```text
.msf source (UTF-8)
   |
   v
SourceText (line map, BOM handling, Unicode-scalar positions)
   |
   v
Lexer (tokens, E1xxx)
   |
   v
Parser / AST (recursive descent, E2xxx)
   |
   v
Resolver / SemanticModel (scopes, bindings, static checks, E3xxx)
   |
   v
Interpreter (tree-walk execution, runtime checks, E4xxx)
   |
   v
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
- `Diagnostic` carries `Code`, `Phase`, `Message`, `Span`, optional `Hint` and optional `SourcePath`.
- Phase-specific exception types (`LexException`, `SyntaxException`, `SemanticException`, `RuntimeException`, plus `ModuleLoadException`) are used to stop at the first failing phase.
- `DiagnosticFormatter` renders the spec-style excerpt with caret alignment (including tab preservation in the caret prefix). Multi-file diagnostics are formatted against the owning file via `DiagnosticSource.Resolve`.

### 4.3 Lexer (`Lexing/`)
- Hand-written scanner producing tokens with spans and literal values.
- Recognizes keywords, identifiers, numbers, strings/escapes, operators/punctuation, and comments.
- Emits lexical diagnostics (e.g. unexpected character, invalid escape, unterminated string/comment, non-finite numeric literal cases as implemented).

### 4.4 Parser and AST (`Syntax/`)
- Recursive-descent parser following the v0.1 grammar plus v0.2/v0.3 constructs with span-preserving immutable AST records.
- Produces declarations/statements/expressions (including `import`, `export`, qualified member/call expressions) and enforces syntax-level constraints.
- Enforces parser nesting safety limit (256) and reports syntax diagnostics (`E2xxx`).

### 4.5 Semantic Analysis (`Semantics/`)
- `Resolver` performs lexical scope resolution and static checks in textual order.
- Assigns numeric binding IDs and records mappings in `SemanticModel` (AST node -> binding; parameter bindings for functions).
- Enforces v0.1 rules such as declaration/assignment validity, top-level function restrictions, direct-call arity checks, `return` placement, and value-position restrictions for callables.
- Resolves module imports: loads and resolves the module in an isolated scope, caches resolution per path, detects cycles, validates exports, and binds the module as a `BindingKind.Module` entry in the importing scope.
- Builtin `print` is predeclared/reserved in the global scope.

### 4.6 Runtime (`Runtime/`)
- Tree-walk `Interpreter` evaluates the resolved AST using ID-keyed environments (`MsEnvironment`).
- Value model: `number` (finite `double`), `string`, `boolean`, `nil`, `list`, `map` (plus internal callable values).
- Implements operator semantics, strict boolean conditions, short-circuit logic, function calls/returns, recursion depth limit (1000 user calls), and builtin `print`.
- Module runtime environments (`ModuleRuntimeState`) guarantee execute-once per process/REPL session; function values capture their defining environment and model.
- `ValueFormatter` provides display formatting for program output/REPL auto-display.

### 4.7 REPL (`Repl/`)
- `ReplSession` maintains persistent global runtime/semantic state across entries.
- Evaluates entries as program fragments, returns display values for top-level non-`nil` expression statements, and attempts transactional rollback on compile/runtime failures.
- CLI owns terminal/piped I/O behavior (prompts/banner, `:help/:quit`, continuation handling).

### 4.8 Modules (`Modules/`)
- `ModuleLoadContext` holds the load cache (read/parse/resolve-once per path) and allocator for module IDs; its base directory is the importing file's directory (or the working directory for the REPL).
- `ModuleUnit` carries the module's `SourceText`, AST, isolated `SemanticModel`, and source path for multi-file diagnostics.
- `ModuleRuntimeState` keeps per-module runtime environments so a module executes before the importing file in dependency order and only once per session.

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

- **REPL internals:** session integration uses reflection/internal access patterns to bridge resolver/runtime snapshots; this is functional but a maintainability risk and a candidate for refactoring behind explicit APIs.
- **Module loading (v0.3):** modules are loaded relative to the importing file with `.msf` extension only; cycle detection and path validation are enforced; no packages, manifests, or sandboxes. REPL sessions re-resolve each import; re-importing the same path is a duplicate declaration (`E3002`).
- **Module cycle reporting:** cycle paths are reported textually (load order); no topological ordering guarantees beyond dependency-first execution.
- **Diagnostics precision:** line/column/excerpt is validated for catalog cases; module-aware formatting goes through `DiagnosticSource`.

## 8. Evolution Guidance

- Keep `SPEC-v0.1.md` and ADRs as the source of truth for behavior changes.
- Prefer adding conformance cases before/with behavior changes for externally visible semantics.
- Stabilize semantics and public contracts before larger runtime/backend optimizations.
- When expanding language features (v0.2+), introduce ADR + spec updates + tests/evidence together.
