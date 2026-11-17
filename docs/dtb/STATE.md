# MSF DTB State

- Product / repository: MSF Lang (`msf-lang`) — small, spec-driven programming language in C#/.NET
- Active branch: `feature/msf-lang-branding-update`
- Authorized scope: MSF Lang language/CLI/docs/tests work; no production release, package signing, deployment, Windows registry changes, or breaking contracts without approval
- Last verified iteration: v0.3 local file modules implemented (`import`/`export`, qualified access, module-executes-once, cycle detection, REPL module persistence); ADR-0021 accepted
- Current item / status: v0.3 modules done and green; v0.3 spec approved (EN normative, AR mirror); examples/docs/evidence updated; remaining: RC hardening/tag
- Relevant files: `src/`, `tests/`, `docs/en/SPEC-v0.3-modules.md`, `docs/ar/SPEC-v0.3-modules.md`, `grammar/msf-v0.3-modules.ebnf`, `docs/DECISIONS-v0.3-modules.md`, `CHANGELOG.md`, `docs/dtb/*`, `examples/modules/`
- Baseline checks and results: Before this iteration: `dotnet test MSFLang.slnx -c Release` → 190/190 green
- Most recent checks and results: `dotnet build MSFLang.slnx -c Release -warnaserror` → 0 errors; `dotnet test MSFLang.slnx -c Release` → **216 passed**, 0 failed, 0 skipped (Core 110, Conformance 87, CLI 19)
- Unverified checks / limitations: v0.3 is a release candidate, not a production-general language: stdlib/packages/sandbox/file IO still missing; module cycles reported textually; REPL re-import is `E3002`; manual acceptance scenarios re-verification for v0.3 pending before any RC tag
- Known risks / blockers: v0.3 reserves `import`/`export` keywords (documented breaking change); RC signing/tag still approval-gated
- Next safe action: Manual acceptance scenario re-verification for v0.3, then RC tag/signing only with explicit approval
- Approval required (if any): RC tag/signing, product release, package distribution, runtime sandbox/quotas, stdlib rollout
