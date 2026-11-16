# MSF DTB Backlog

| ID | Item | Evidence | Priority | Authorization | Status | Acceptance criteria |
|---|---|---|---|---|---|---|
| DTB-001 | Create DTB ledger (`docs/dtb/*`) and record baseline | User activated msf-dtb; no ledger existed | high | authorized | VERIFIED | STATE/BACKLOG/CHANGELOG exist; baseline 181/181 recorded |
| DTB-002 | Improve `msf --help` to match spec intent (usage, commands, v0.1 scope summary, docs pointer) | SPEC §13.3; current help printed only usage; CLI test updated | high | authorized | VERIFIED | `msf --help` exits 0; includes usage/commands/v0.1 summary/docs URL; CLI tests pass |
| DTB-003 | Add runnable example for v0.2 practical slice | `examples/practical-lists.msf` created and run manually; guide examples updated | high | authorized | VERIFIED | Example runs (`EC=0`, expected output); guide links it |
| DTB-004 | Add Arabic v0.2 practical spec mirror | `docs/ar/SPEC-v0.2-practical.md` created | medium | authorized | VERIFIED | Arabic mirror exists and points to EN normative spec |
| DTB-005 | Expand conformance with v0.2 `len`/`append` mismatch negative cases | Added `len-non-list` and `append-non-list` fixtures; full suite green | medium | authorized | VERIFIED | Conformance passes with new fixtures (80 total) |
| DTB-006 | Add `MSF_DEBUG=1` internal-error stack trace support | SPEC §12.4 internal errors | low | authorized | PROPOSED | Only gate behind env var; CLI tests set/unset; internal diagnostics still not mixed with language errors |
| DTB-007 | ADR for v0.2 practical slice | Added ADR-0018 to DECISIONS.md | high | authorized | VERIFIED | DECISIONS.md gains ADR-0018 documenting loops/lists/len/append, risks, and compatibility |
| DTB-008 | v0.2 maps (`map` type, literals, get/set/has/remove/len) | Practical data-processing gap for user-facing scripts | high | authorized after spec review | PROPOSED | Needs approved ADR + spec text + tests; large scope expansion; do after smaller verified items |
