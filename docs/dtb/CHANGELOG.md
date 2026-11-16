# MSF DTB Changelog

| Date | ID | Change | Verification | Result |
|---|---|---|---|---|
| 2026-11-16 | DTB-001 | Created DTB ledger (STATE/BACKLOG/CHANGELOG) and recorded pre-change baseline | `dotnet test MSFLang.slnx -c Release` | 181 passed / 0 failed / 0 skipped (Core 88, Conformance 78, CLI 15) |
| 2026-11-16 | DTB-002 | Improved `msf --help` to include usage, commands, v0.1 scope summary, and spec URL | `dotnet test MSFLang.slnx -c Release`; CLI test updated | 183 passed / 0 failed / 0 skipped (Core 88, Conformance 80, CLI 15) |
| 2026-11-16 | DTB-003 | Added `examples/practical-lists.msf` and documented it in `docs/guide/examples.md` | `dotnet run --project src/MsfLang.Cli -c Release -- run examples/practical-lists.msf` | Exit 0; output `doubled: [2, 4, 6, 8, 10]` and `total: 30` |
| 2026-11-16 | DTB-004 | Added Arabic v0.2 practical spec mirror `docs/ar/SPEC-v0.2-practical.md` | File created; docs-only, no runtime verification | Created as condensed mirror of EN normative draft |
| 2026-11-16 | DTB-005 | Added conformance fixtures for E4008 (`len` non-list, `append` first arg non-list) | `dotnet test MSFLang.slnx -c Release` | 183 passed / 0 failed / 0 skipped; Conformance now 80 |
| 2026-11-16 | DTB-007 | Added ADR-0018 to `docs/DECISIONS.md` for v0.2 loops, lists, `len`/`append` | Doc change; no code/runtime behavior | ADR documented as proposed and implemented as draft |
