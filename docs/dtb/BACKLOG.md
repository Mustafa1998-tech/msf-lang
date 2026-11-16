# MSF DTB Backlog

| ID | Item | Evidence | Priority | Authorization | Status | Acceptance criteria |
|---|---|---|---|---|---|---|
| DTB-001 | Create DTB ledger and record baseline | User activated msf-dtb; Bible documents require `docs/dtb` | high | authorized | VERIFIED | STATE/BACKLOG/CHANGELOG exist; baseline recorded |
| DTB-002 | Improve `msf --help` to match spec intent | SPEC §13.3 help content; CLI test updated | high | authorized | VERIFIED | Help prints usage/commands/v0.1 scope/docs URL and exits 0; CLI suite green |
| DTB-003 | Add runnable example for v0.2 practical slice | `examples/practical-lists.msf` documented and run | high | authorized | VERIFIED | Example runs with exit 0 and deterministic output |
| DTB-004 | Add Arabic v0.2 practical spec mirror | `docs/ar/SPEC-v0.2-practical.md` | high | authorized | VERIFIED | Arabic mirror exists and defers to EN normative |
| DTB-005 | Expand conformance with v0.2 E4008 cases | Added len/append mismatch fixtures | medium | authorized | VERIFIED | Conformance includes E4008 cases; suite green |
| DTB-006 | Add `MSF_DEBUG=1` internal-error stack trace support | SPEC §12.4 internal-error handling | low | authorized | PROPOSED | Env-gated only; internal diagnostics never bypass language diagnostics; tests set/unset MSF_DEBUG |
| DTB-007 | ADR for v0.2 practical slice | ADR-0018 in DECISIONS.md | high | authorized | VERIFIED | ADR documents loops/lists/len/append, compatibility, deferrals |
| DTB-008 | v0.2 maps (`map`) | Practical data-processing gap; largest remaining gap for scripts | high | requires ADR/spec approval | PROPOSED | Needs approved ADR + spec + conformance before implementation |
| DTB-009 | Complete negative `continue` outside-loop fixture | Added `continue-outside-loop.expected.json` (E3014) | medium | authorized | VERIFIED | `dotnet test` conformance shows E3014 case passing (81 total) |
| DTB-010 | Sync v0.2 spec diagnostics across EN/AR mirrors | EN now documents E3013/E3014 and points to list diagnostics; AR lists them | medium | authorized | VERIFIED | Docs consistent for E3013/E3014/E4007/E4008 |
| DTB-011 | Update README current-state note for v0.2 draft | README current-state paragraph now says implementation is v0.1 complete with v0.2 draft tests | medium | authorized | VERIFIED | README reflects v0.2 draft and does not claim production readiness |
| DTB-012 | Sync operator-facing docs for v0.2 practical slice | `msf --help` docs, getting-started limitations, guide index, roadmap/test evidence updated to 184/81 current state | medium | authorized | VERIFIED | Docs no longer claim v0.1-only limitations/no loops in current release; test log latest total 184 green |
