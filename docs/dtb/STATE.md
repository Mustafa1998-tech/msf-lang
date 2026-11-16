# MSF DTB State

- Product / repository: MSF Lang (`msf-lang`) — small, spec-driven programming language in C#/.NET
- Active branch: `feature/msf-lang-branding-update`
- Authorized scope: Repository work on MSF Lang v0.1/v0.2 practical draft, docs/tests/benchmarks; no production deployment, no registry/Windows integration changes, no paid services
- Last verified iteration: Added negative `continue`-outside-loop fixture, synced EN/AR v0.2 diagnostics, updated README current-state note; all tests green
- Current item / status: DTB cycle 2 complete; small verified improvements delivered; no safe high-value work left without approval/major scope
- Relevant files: `src/`, `tests/`, `docs/en/SPEC-v0.2-practical.md`, `docs/ar/SPEC-v0.2-practical.md`, `README.md`, `docs/dtb/*`
- Baseline checks and results: `dotnet test MSFLang.slnx -c Release` → **181 passed** (before DTB edit), 0 failed
- Most recent checks and results: `dotnet test MSFLang.slnx -c Release` → **184 passed**, 0 failed, 0 skipped (Core 88, Conformance 81, CLI 15)
- Unverified checks / limitations: No production release; no formal security audit; v0.2 is draft not production-ready; MSF_DEBUG stack trace and maps unimplemented; Arabic mirror is condensed
- Known risks / blockers: v0.2 reserves new names; maps require ADR/spec approval; v0.2 runtime limits/spec still need broad acceptance scenarios before release RC
- Next safe action: Request ADR approval to implement DTB-008 (maps) or select DTB-006 (`MSF_DEBUG`) after env-gating design; otherwise hold at current verified state
- Approval required (if any): Maps ADR/spec; RC/release tag; production; package registry changes; breaking semantics
