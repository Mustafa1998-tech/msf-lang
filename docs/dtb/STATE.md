# MSF DTB State

- Product / repository: MSF Lang (`msf-lang`) — small, spec-driven programming language in C#/.NET
- Active branch: `feature/msf-lang-branding-update`
- Authorized scope: Repository work on MSF Lang v0.1/v0.2 practical draft, docs/tests/benchmarks; no production deployment, no registry/Windows integration changes, no paid services
- Last verified iteration: DTB bootstrap + verified help/output, v0.2 examples/guide, Arabic mirror, conformance E4008 cases, and ADR-0018
- Current item / status: DTB cycle 1 complete; awaiting next eligible backlog item (maps pending spec review)
- Relevant files: `src/MsfLang.Cli/CommandLine.cs`, `tests/MsfLang.Cli.Tests/CliSmokeTests.cs`, `examples/practical-lists.msf`, `docs/guide/examples.md`, `docs/ar/SPEC-v0.2-practical.md`, `tests/conformance/*/len-*`, `tests/conformance/*/append-*`, `docs/DECISIONS.md`, `docs/dtb/*`
- Baseline checks and results: `dotnet test MSFLang.slnx -c Release` → **181 passed**, 0 failed, 0 skipped (Core 88, Conformance 78, CLI 15)
- Most recent checks and results: `dotnet test MSFLang.slnx -c Release` → **183 passed**, 0 failed, 0 skipped (Core 88, Conformance 80, CLI 15); `msf run examples/practical-lists.msf` → output `doubled: [2, 4, 6, 8, 10]`, `total: 30`, exit 0; docs-only Arabic mirror not executed
- Unverified checks / limitations: No production release; no formal security audit; v0.2 remains draft and AR mirror is a condensed mirror, not full EN text; `MSF_DEBUG` stack trace and `map` not implemented
- Known risks / blockers: v0.2 names are reserved (`while`, `break`, `continue`, `len`, `append`) and are a v0.1 compatibility risk; maps require explicit ADR/spec review before approval
- Next safe action: Select DTB-006 (`MSF_DEBUG` internal-error stack trace) or hold for user-approved ADR review for DTB-008 (maps); otherwise no high-value unblocked work remains in this run
- Approval required (if any): Approval required for maps ADR/spec expansion, RC/release tag, production, package distribution, and breaking semantics
