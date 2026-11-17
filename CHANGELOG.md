# Changelog

All notable changes to MSF Lang will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) principles, and versions follow the project’s language/spec versioning where applicable.

## [Unreleased]

### Added
- v0.1 language specification drafts (EN/AR), grammar, architecture decisions, and implementation/test plan
- C#/.NET 10 solution scaffold (`src/`, `tests/`, examples, docs, benchmarks, branding concepts)
- Core engine pipeline: `SourceText`, diagnostics formatter, lexer, parser/AST, semantic resolver, and tree-walk interpreter
- CLI: `msf run`, `msf --help/-h`, `msf --version/-v`, and `msf repl` (interactive/piped, persistence, auto-display)
- Built-in `print` and v0.1 runtime/value/diagnostic behavior for implemented features
- Unit tests for source/diagnostics, lexer, parser, resolver, and REPL session behavior (including rollback/incomplete-input cases)
- Conformance test harness and **71** `.msf`/`.expected.json` cases (positive/negative, lexical/syntax/semantic/runtime)
- CLI process-level smoke/contract tests (usage, help/version, run success/errors/I-O, repl piped/EOF/multiline/command handling)
- Runtime limits enforced by implementation: 1,000 user-call depth (`E4005`), 256 parser nesting limit (`E2013`), strict UTF-8 (`E5005`)
- CLI runs engine logic on a dedicated 32 MiB interpreter stack (ADR-0013) for deterministic recursion-limit behavior
- Security guide/page documenting v0.1 security properties, threat model, and repository policy (`docs/guide/security.md`, `SECURITY.md`)
- License and community policy files: `LICENSE` (MIT), `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`
- Benchmarks, performance measurement policy, and user documentation (guide, syntax, CLI, diagnostics, examples, security)
- Project roadmap, problem-solving direction, and architecture documentation (`docs/ARCHITECTURE.md`)
- v0.2 "Practical Programming" draft:
  - `while (condition) { block }` (strict boolean condition)
  - `break;` / `continue;` loop control
  - `list` value: dynamic heterogeneous arrays, list literals, zero-based indexing, in-place element assignment
  - Builtins: `len(list|string)`, `append(list, value)`
  - New diagnostics: `E4007` (list index/type failures), `E4008` (`len`/`append` argument mismatch), `E3013` (`break` outside loop), `E3014` (`continue` outside loop), `E4009` (map builtin/key mismatch)
  - Maps: string-keyed `map { "k": v }` literal, map runtime value, `get/set/has/remove` builtins, `len(map)`
  - Math core builtins: `abs`, `sqrt`, `floor`, `ceil`, `round`, `pow`, `min`, `max`
  - String core builtins: `upper`, `lower`, `trim`, `contains`, `startsWith`, `endsWith`, `split`
  - Specs: `docs/en/SPEC-v0.2-math.md`, `docs/en/SPEC-v0.2-string.md`
- v0.2 practical draft specifications are available in EN and AR (`SPEC-v0.2-practical`, `-maps`, `-math`, `-string`)
  - Specs: `docs/en/SPEC-v0.2-practical.md`, `docs/en/SPEC-v0.2-maps.md`, `docs/en/SPEC-v0.2-math.md` (AR mirrors: `docs/ar/SPEC-v0.2-practical.md`, `docs/ar/SPEC-v0.2-maps.md`, `docs/ar/SPEC-v0.2-math.md`)
- v0.3 "Local Modules":
  - `import "./file.msf";` local file imports (relative paths, `.msf` extension only)
  - `export fn` / `export var` / `export let` top-level exports; everything else module-private
  - Qualified access only: `math.square(9)`, `math.PI`; unqualified names are not imported (`E3001`)
  - Modules are loaded and executed at most once per process/REPL session; dependency modules execute first
  - Import cycle detection (`E4014`), missing module (`E4010`, exit 4), invalid path/extension/stem (`E4012`), unexported access (`E4013`), module used as value (`E3017`), `import`/`export` outside top level (`E3015`/`E3016`), new syntax diagnostics (`E2014`, `E2015`)
  - Multi-file diagnostics carry the module's source path (CLI renders the correct file)
  - Specs: `docs/en/SPEC-v0.3-modules.md` (AR mirror), grammar `grammar/msf-v0.3-modules.ebnf`, ADR-0021
  - 216 tests green (Core 110, Conformance 87, CLI 19)

### Notes
- Latest recorded full-suite result: **216 passed / 0 failed / 0 skipped** (Core 110, Conformance 87, CLI 19).
- Manual acceptance scenarios (10/10) from the implementation plan were verified for v0.1; v0.3 module behavior covered by module unit tests, conformance fixtures, and CLI tests.
- M9 release-candidate preparation is in progress (RC tag/signing pending approval). Performance is measured for baselines/regression tracking; no superiority claims are made without fair, reproducible comparisons.
