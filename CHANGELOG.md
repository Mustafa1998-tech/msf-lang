# Changelog

All notable changes to MSF Lang will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) principles, and versions follow the project’s language/spec versioning where applicable.

## [Unreleased]

### Added
- v0.1 language specification drafts (EN/AR), grammar, architecture decisions, and implementation/test plan
- C#/.NET 10 solution scaffold (`src/`, `tests/`, examples, docs, benchmarks, branding concepts)
- Core engine pipeline: `SourceText`, diagnostics formatter, lexer, parser/AST, semantic resolver, and tree-walk interpreter
- CLI: `msf run`, `msf --help/-h`, `msf --version/-v`, and basic `msf repl` (interactive/piped, persistence, auto-display)
- Built-in `print` and v0.1 runtime/value/diagnostic behavior for implemented features
- Unit tests for source/diagnostics, lexer, parser, resolver, and basic REPL behavior
- Conformance test harness and initial `.msf`/`.expected.json` cases (positive/negative)
- Benchmarks, performance measurement policy, and user documentation (guide, syntax, CLI, diagnostics, examples)
- Project roadmap and problem-solving direction documents

### Notes
- MSF Lang v0.1 is under active development; full CLI/REPL spec compliance, complete conformance coverage (60+), and release hardening (M9) are in progress.
- Performance is measured for baselines/regression tracking; no superiority claims are made without fair, reproducible comparisons.
