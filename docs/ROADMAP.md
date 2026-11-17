# MSF Lang Roadmap

This roadmap describes the planned evolution of MSF Lang from a stable v0.1 language core to a broader ecosystem (standard library, packages, and compiler), while preserving specification clarity and backward compatibility.

> Status: planning document. Priorities and timelines may change as implementation evidence and design trade-offs are validated.

## Guiding Principles

- **Semantics first:** language behavior must be specified, implemented, and tested before ecosystem expansion
- **Compatibility:** avoid silent breaking changes to conforming v0.1 programs
- **Evidence-driven delivery:** milestones are complete only with recorded, reproducible test/benchmark evidence
- **Incremental growth:** add features in coherent layers (language, tooling, stdlib, packages, compiler)

---

## Phase 1 — Complete the v0.1 Language Implementation (Current)

**Goal:** deliver a conforming, well-tested reference implementation of MSF Lang v0.1.

### Current progress (evidence-backed)
- **M6 (Runtime hardening):** ✅ core interpreter/runtime implemented and tested (while, break/continue, lists, maps)
- **M7 (CLI & REPL):** ✅ `run/help/version/repl` implemented and tested; REPL rollback/incomplete-input handling improved and covered by tests (including piped/multiline/EOF cases)
- **M8 (Conformance):** ✅ harness and **87** `.msf` + `.expected.json` conformance cases passing (60+ target reached)
- **M9 (v0.2 practical + v0.3 local modules):** 🚧 full suite green (**216/216**): v0.2 loops/lists/maps/math/string builtins and v0.3 local file modules (`import`/`export`, qualified access, module-executes-once, cycle detection) implemented with docs and tests; remaining: RC/tag/release, package ecosystem, stdlib, sandbox

### Exit criteria
- Normative v0.1 behaviors implemented and verified
- Diagnostics/exit codes/CLI behavior match the specification
- Conformance and evidence artifacts are complete and reproducible

---

## Phase 2 — Stabilize Semantics (v0.1.x / v0.2 planning)

**Goal:** lock down semantics and resolve ambiguities before major surface expansion.

### Focus areas
- Close spec/implementation gaps discovered during conformance and real usage
- Clarify edge cases (numeric formatting, diagnostics details, REPL behavior boundaries)
- Publish migration/compatibility policy for post-v0.1 changes
- Define a conservative feature intake process (ADR + spec + tests before implementation)

### Candidate language additions (post-stabilization, subject to ADRs)
- Control flow: `while`, `break`, `continue` (and possibly `for`)
- Collections: arrays/lists and/or maps with defined semantics
- Function capabilities: closures / first-class functions (requires careful scoping/runtime design)
- Improved diagnostics (multi-error reporting, optional stack traces) as tooling features

---

## Phase 3 — Standard Library (after core semantics stabilize)

**Goal:** provide a small, consistent, and safe standard library without compromising language simplicity.

### Principles
- Explicit APIs, deterministic behavior where possible
- Clear error model and type expectations
- Versioned and documented modules/namespaces (design TBD)

### Likely initial areas
- **Core utilities:** type/value helpers, assertions/debug helpers (as appropriate)
- **Math:** common numeric functions/constants
- **Strings:** length, slicing/search/transform primitives
- **Collections:** operations aligned with chosen collection types
- **I/O boundaries (carefully scoped):** file/stdin APIs introduced with explicit design and security considerations

> Any standard library that introduces side effects (I/O, time, randomness, environment access) must be specified with determinism and portability implications.

---

## Phase 4 — Packages & Tooling Ecosystem

**Goal:** enable reuse and distribution once language/module semantics are ready.

### Planned directions
- **Module/import system** (language feature prerequisite)
- **Package manifest format** and dependency resolution strategy
- **Package registry/distribution workflow** (hosted or decentralized model TBD)
- **Tooling integration:** formatter/linter/LSP/debugger foundations, editor support improvements
- **Build/test workflows** for multi-file/package projects

### Notes
- Package management will not be finalized before module semantics and versioning rules are stable.
- Security, reproducibility, and supply-chain concerns will be part of the design.

---

## Phase 5 — Compiler & Performance Backends (when semantics are stable)

**Goal:** improve performance and deployment options without changing language meaning.

### Rationale
A compiler/bytecode/VM/JIT effort is most valuable after syntax and semantics are stable, so optimization work does not chase a moving target.

### Possible tracks (incremental, ADR-driven)
- **Bytecode + VM backend** (performance-focused interpreter alternative)
- **AOT/JIT compilation experiments** for selected targets
- **Optimizations:** constant folding, specialization, call/path optimizations (only where semantics-preserving)
- **Runtime profiling and observability** to guide optimization priorities

### Compatibility commitment
- Backends must preserve observable language semantics defined by the specification and conformance suite
- Any backend-specific behavior differences (performance, limits, diagnostics formatting details beyond spec) must be documented

---

## Cross-Cutting Workstreams (ongoing)

- **Documentation:** user guides, tutorials, language reference, migration notes (EN/AR where appropriate)
- **Branding & distribution:** final identity assets, VS Code integration, Windows integration (gated approvals)
- **Quality & performance:** fuzzing/property tests, regression benchmarks, CI automation. **Performance policy:** measure before making any claim of superiority; comparisons must be fair, reproducible, and documented.
- **Community & governance:** contribution workflow, RFC/ADR process, release/versioning policy

---

## Near-Term Next Steps

1. RC hardening and tag: final spec/implementation cross-check, release notes, RC tag/signing (approval-gated), 10/10 manual acceptance scenarios re-verified
2. Expand conformance/contract coverage for v0.3 modules (more positive fixtures, multi-file examples, traceability)
3. Begin stdlib v0.1 core (deterministic math/string/map/list utilities, `args`), then packaging manifests and runtime quotas/sandbox per ADRs
4. Semantics-stabilization review for closures/first-class functions and collections (with ADRs and compatibility analysis)

See also: [Problem-Solving Direction](PROBLEM-SOLVING.md) for use-case-driven priorities.

---

*MSF Lang is developed with a specification-first, evidence-driven approach to support long-term reliability and ecosystem growth.*