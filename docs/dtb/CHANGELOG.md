# MSF DTB Changelog

| Date | ID | Change | Verification | Result |
|---|---|---|---|---|
| 2026-11-16 | DTB-001 | Created DTB ledger and recorded pre-change baseline | `dotnet test MSFLang.slnx -c Release` | 181 passed / 0 failed / 0 skipped |
| 2026-11-16 | DTB-002 | Improved `msf --help` output with usage/commands/v0.1 summary/spec URL and CLI assertion | `dotnet test MSFLang.slnx -c Release` | 183 passed / 0 failed |
| 2026-11-16 | DTB-003 | Added `examples/practical-lists.msf` and guide docs | `dotnet run --project src/MsfLang.Cli -c Release -- run examples/practical-lists.msf` | Exit 0; expected output |
| 2026-11-16 | DTB-004 | Added Arabic v0.2 practical spec mirror | docs-only | File created |
| 2026-11-16 | DTB-005 | Added E4008 conformance cases | `dotnet test MSFLang.slnx -c Release` | 183 passed / 0 failed |
| 2026-11-16 | DTB-007 | Added ADR-0018 to `docs/DECISIONS.md` | docs-only | ADR documented as v0.2 draft |
| 2026-11-16 | DTB-009 | Added negative `continue` outside-loop fixture (`E3014`) | `dotnet test MSFLang.slnx -c Release` | 184 passed / 0 failed |
| 2026-11-16 | DTB-010 | Synced EN/AR v0.2 spec diagnostic tables/lists | docs-only | EN documents E3013/E3014; AR mirrors them |
| 2026-11-16 | DTB-011 | Updated README current-state note for v0.2 draft | docs-only | README now says v0.1 complete, v0.2 draft tested |
| 2026-11-16 | DTB-012 | Synced user-facing/v0.2 docs (help/CLI/getting-started/guide index/roadmap/test evidence) | `dotnet test MSFLang.slnx -c Release` | 184 passed / 0 failed / 0 skipped (Core 88, Conformance 81, CLI 15) |
