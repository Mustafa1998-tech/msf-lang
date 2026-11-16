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

### Notes
- Latest recorded full-suite result: **175 passed / 0 failed / 0 skipped** (Core 88, Conformance 72, CLI 15) with **71 conformance cases**.
- Manual acceptance scenarios (10/10) from the implementation plan were verified (run/repl/diagnostics/exit codes/UTF-8/paths).
- M9 release-candidate preparation is in progress (final spec/implementation cross-check and RC tag). Performance is measured for baselines/regression tracking; no superiority claims are made without fair, reproducible comparisons.
