# MSF DTB State

- Product / repository: MSF Lang (`msf-lang`) — small, spec-driven programming language in C#/.NET
- Active branch: `feature/msf-lang-branding-update`
- Authorized scope: MSF Lang language/CLI/docs/tests work; no production release, package signing, deployment, Windows registry changes, or breaking contracts without approval
- Last verified iteration: v0.2 maps implemented in code/docs/tests; `MSF_DEBUG=1` internal-error stack-trace path implemented and CLI tests added
- Current item / status: Maps (`DTB-008`) and MSF_DEBUG (`DTB-013`) implemented and pushed as draft work; still not production-ready
- Relevant files: `src/`, `tests/`, `docs/en/SPEC-v0.2-maps.md`, `docs/ar/SPEC-v0.2-maps.md`, `docs/DECISIONS.md`, `CHANGELOG.md`, `docs/dtb/*`
- Baseline checks and results: Before this iteration: `dotnet test MSFLang.slnx -c Release` → 184/184 green
- Most recent checks and results: `dotnet test MSFLang.slnx -c Release` → **188 passed**, 0 failed, 0 skipped (Core 88, Conformance 83, CLI 17)
- Unverified checks / limitations: v0.2 remains draft; no formal security audit; map iteration order unspecified; stdlib/modules/file IO/packages/toolset missing; `MSF_DEBUG` internal branch implemented but not triggered in process tests
- Known risks / blockers: Maps change API surface and reserve new names; needs acceptance/spec review and possibly RC tagging; MSF_DEBUG is env-gated but internal-error branch not directly exercised
- Next safe action: User reviews maps draft/ADR and RC scope; do not release/tag without approval
- Approval required (if any): Release candidate/tag/production/package distribution/maps stabilization
