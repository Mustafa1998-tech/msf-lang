# MSF DTB State

- Product / repository: MSF Lang (`msf-lang`) — small, spec-driven programming language in C#/.NET
- Active branch: `feature/msf-lang-branding-update`
- Authorized scope: Repository work on MSF Lang v0.1/v0.2 practical draft, docs/tests/benchmarks; no production deployment, no registry/Windows integration changes, no paid services
- Last verified iteration: Synced v0.2 grammar/EN/AR docs and operator-facing docs (help/CLI/getting-started/guide index/roadmap/test evidence); reran full suite green
- Current item / status: DTB cycle 3 complete; v0.2 practical draft documented across user-facing docs; larger items blocked on ADR/approval
- Relevant files: `README.md`, `docs/guide/cli.md`, `docs/guide/getting-started.md`, `docs/guide/README.md`, `docs/ROADMAP.md`, `docs/TEST-EVIDENCE.md`, `docs/en/SPEC-v0.2-practical.md`, `docs/dtb/*`
- Baseline checks and results: `dotnet test MSFLang.slnx -c Release` → **181 passed** (before DTB edit), 0 failed
- Most recent checks and results: `dotnet test MSFLang.slnx -c Release` → **184 passed**, 0 failed, 0 skipped (Core 88, Conformance 81, CLI 15)
- Unverified checks / limitations: No production release; no formal security audit; v0.2 is draft not production-ready; MSF_DEBUG stack trace and maps unimplemented; Arabic mirror is condensed
- Known risks / blockers: v0.2 reserves new names; maps require ADR/spec approval; v0.2 runtime limits/spec still need broad acceptance scenarios before release RC
- Next safe action: Request ADR approval to implement DTB-008 (maps) or select DTB-006 (`MSF_DEBUG`) after env-gating design; otherwise hold at current verified state
- Approval required (if any): Maps ADR/spec; RC/release tag; production; package registry changes; breaking semantics
