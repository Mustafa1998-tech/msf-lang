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
- Unit tests for source/diagnostics, lexer, parser, resolver, and REPL session behavior
- Conformance test harness and **63** `.msf`/`.expected.json` cases (positive/negative, lexical/syntax/semantic/runtime)
- CLI process-level smoke/contract tests (usage, help/version, run success/errors/I-O, repl piped)
- Benchmarks, performance measurement policy, and user documentation (guide, syntax, CLI, diagnostics, examples)
- Project roadmap, problem-solving direction, and architecture documentation (`docs/ARCHITECTURE.md`)

### Notes
- MSF Lang v0.1 is under active development toward launch; latest recorded full-suite result is **161 passed / 0 failed / 0 skipped** with **63 conformance cases** (60+ target reached).
- Full CLI/REPL spec-contract compliance details (edge cases/rollback/incomplete-input and complete C-*/R-* coverage) and M9 release hardening (manual acceptance, spec cross-check, RC/tag) remain in progress.
- Performance is measured for baselines/regression tracking; no superiority claims are made without fair, reproducible comparisons.
