# MSF DTB Changelog

| Date | ID | Change | Verification | Result |
|---|---|---|---|---|
| 2026-11-16 | DTB-001 | Created DTB ledger and baseline | `dotnet test MSFLang.slnx -c Release` | 181 passed |
| 2026-11-16 | DTB-002 | Improved `msf --help` | `dotnet test MSFLang.slnx -c Release` | help/v0.1/tests green |
| 2026-11-16 | DTB-003 | Added practical lists example and guide docs | `dotnet run -- ... examples/practical-lists.msf` | output `doubled: [2, 4, 6, 8, 10]`, `total: 30` |
| 2026-11-16 | DTB-004 | Added AR v0.2 practical mirror | docs-only | created |
| 2026-11-16 | DTB-005 | Added E4008 conformance fixtures | `dotnet test ...` | 183 passed |
| 2026-11-16 | DTB-007 | ADR-0018 for v0.2 loops/lists | docs-only | created |
| 2026-11-16 | DTB-009 | continue-outside-loop conformance fixture | `dotnet test ...` | 184 passed |
| 2026-11-16 | DTB-010 | Sync v0.2 spec diagnostics EN/AR | docs-only | synced |
| 2026-11-16 | DTB-011 | README current-state update | docs-only | updated |
| 2026-11-16 | DTB-012 | Sync docs: CLI/getting-started/guide/roadmap/evidence | `dotnet test ...` | 184 passed |
| 2026-11-16 | DTB-008 | Implemented maps draft: `map {}`, map type, `get/set/has/remove`, `len(map)`, ADR-0019, EN/AR map specs, conformance fixtures | `dotnet test MSFLang.slnx -c Release` | 188 passed (Core 88, Conformance 83, CLI 17) |
| 2026-11-16 | DTB-013 | Implemented env-gated `MSF_DEBUG=1` raw stack trace path for internal errors; CLI tests assert normal language errors do not print stack traces | `dotnet test MSFLang.slnx -c Release` | 188 passed |
