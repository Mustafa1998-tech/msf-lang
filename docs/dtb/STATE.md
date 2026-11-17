# MSF DTB State

- Product / repository: MSF Lang (`msf-lang`) — small, spec-driven programming language in C#/.NET
- Active branch: `feature/msf-lang-branding-update`
- Authorized scope: MSF Lang language/CLI/docs/tests work; no production release, package signing, deployment, Windows registry changes, or breaking contracts without approval
- Last verified iteration: v0.3 local file modules (`import`/`export`, qualified access, run-once, cycle detection), ADR-0021 accepted; manual acceptance scenarios re-verified (10/10); `msf 0.3.0-rc.1` tagged and pre-released on GitHub (2026-11-17)
- Current item / status: v0.3 release candidate published (GitHub pre-release); still NOT production-general (no stdlib, packages, sandbox, quotas)
- Relevant files: `src/`, `tests/`, `docs/en/SPEC-v0.3-modules.md`, `docs/ar/SPEC-v0.3-modules.md`, `grammar/msf-v0.3-modules.ebnf`, `docs/DECISIONS-v0.3-modules.md`, `docs/RELEASE-NOTES-v0.3.0-rc.1.md`, `CHANGELOG.md`, `docs/dtb/*`, `examples/modules/`
- Baseline checks and results: Before this iteration: `dotnet test MSFLang.slnx -c Release` → 190/190 green
- Most recent checks and results: `dotnet build MSFLang.slnx -c Release -warnaserror` → 0 errors; `dotnet test MSFLang.slnx -c Release` → **216 passed**, 0 failed, 0 skipped (Core 110, Conformance 87, CLI 19); manual acceptance scenarios 10/10 on built CLI
- Unverified checks / limitations: v0.3 RC is not production-general: stdlib/packages/sandbox/file IO/packaging missing; module cycle reporting textual; REPL re-import is `E3002`; packaging/distribution/signing of installers NOT done
- Known risks / blockers: v0.3 reserves `import`/`export` keywords (documented breaking change); production use before quotas/sandbox is discouraged
- Next safe action: v0.3 RC review; collect issues; do NOT tag production release without approval
- Approval required (if any): Production release/tag/signing, package distribution, sandbox/quotas design, stdlib v0.1 rollout
