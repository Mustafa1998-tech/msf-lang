# MSF Lang — Test Evidence Log

**Rule:** a feature or milestone is not complete until this file contains the command that was
actually run, the date, and the observed result. No test result may be inferred or copied from a
plan.

**Status:** Phase 1. No engine, test project, or runnable CLI exists. **Zero tests have been
executed.** This is intentional: implementation starts only after Gate A approves the
specification.

---

## Evidence entries

| Date | Milestone | Command | Result | Notes |
|---|---|---|---|---|
| 2026-11-16 | M0 | — | Gate A approved (user) | Spec v0.1 accepted as starting point; implementation begun at M1 |
| 2026-11-16 | M1 | `dotnet restore MSFLang.slnx; dotnet build MSFLang.slnx -c Release -warnaserror` | Exit 0. Build succeeded, 0 Warning(s), 0 Error(s). | Solution scaffold, Core/CLI/Test projects, Directory.Build.props, global.json |
| 2026-11-16 | M1 | `dotnet run --project src/MsfLang.Cli -c Release --` | Exit 3. Printed usage. | Smoke: no args => usage error (E5002 path not yet wired, but exit 3 as required) |
| 2026-11-16 | M2 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Source\|FullyQualifiedName~Diagnostic"` | Exit 0. Passed: 10, Failed: 0, Skipped: 0. | SourceText (BOM, LF/CRLF/CR, Unicode scalar columns, tabs, EOF, line text) and DiagnosticFormatter (excerpt/caret/hint/tabs/Unicode/gutter) |
| 2026-11-16 | M3 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Lexer"` | Exit 0. Passed: 31, Failed: 0, Skipped: 0. | Lexer tokens/keywords/identifiers/numbers/strings/escapes/comments, EOF spans, E1001/E1003/E1004/E1005 cases, CRLF/Unicode string handling (subset covering key L-* cases) |
| 2026-11-16 | M4 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Parser"` | Exit 0. Passed: 23, Failed: 0, Skipped: 0. | AST/recursive-descent parser: decls/stmts/exprs/calls, precedence/associativity, if/else-if/dangling else, spans, E2002-E2013 and EOF→E2010, nesting limit 256/257 (key P-* cases) |
| 2026-11-16 | M5 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Resolver"` | Exit 0. Passed: 20, Failed: 0, Skipped: 0. | Bindings/IDs, lexical scoping, textual-order top-level visibility, direct recursion, E3001-E3011 checks (key S-* cases) |
| 2026-11-16 | M6 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release` | Exit 0. Passed: 84, Failed: 0, Skipped: 0. | All existing Core tests pass after interpreter/runtime integration (Source/Diagnostics/Lexer/Parser/Resolver suites) |
| 2026-11-16 | M6 | `dotnet run --project src/MsfLang.Cli -c Release --no-build -- run examples/hello.msf` | Exit 0. Output: `Hello, MSF!` | Basic run pipeline works |
| 2026-11-16 | M6 | `dotnet run --project src/MsfLang.Cli -c Release --no-build -- run examples/factorial.msf` | Exit 0. Output: `3628800`, `55` | Functions/recursion/arithmetic/control flow |
| 2026-11-16 | M6 | `dotnet run --project src/MsfLang.Cli -c Release --no-build -- run %TEMP%\msf-depth-ok.msf` / `...bad.msf` | Exit 0 for `f(999)`; Exit 2 with `E4005` for `f(1000)` | Call-depth limit enforced (1000 active user calls) |
| 2026-11-16 | M6/M7 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release` | Exit 0. Passed: 85, Failed: 0, Skipped: 0. | All Core tests pass including basic `ReplSession` sequence/persistence/auto-display behavior |
| 2026-11-16 | M7 | `Get-Content %TEMP%\msf-repl-test.txt \| dotnet run --project src/MsfLang.Cli -c Release --no-build -- repl` | Exit 0. Output: `10`, `25` (no prompts/banner in piped mode). | Basic REPL CLI (piped) with persistence and auto-display of non-nil expression results |
| — | M8 | — | Not started | — |
| — | M3 | — | Not started | — |
| — | M4 | — | Not started | — |
| — | M5 | — | Not started | — |
| — | M6 | — | Not started | — |
| — | M7 | — | Not started | — |
| — | M8 | — | Not started | — |
| — | M9 | — | Not started | — |

---

## What must be recorded

For every run:

- the exact command, including project and configuration,
- the date (UTC or local, stated),
- the exit code,
- the pass/fail/skip counts printed by the test runner,
- any warnings,
- for a manual acceptance scenario, the exact input and observed output.

Example of an acceptable future entry (illustrative only, **not a result**):

```text
2026-12-01  M3  dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Lexer"
Exit code: 0. Passed: 35, Failed: 0, Skipped: 0. Warnings: 0.
```

## What is not evidence

- "It should work."
- A screenshot of source code.
- A test that was written but not executed.
- A test run with failures that are described as "expected".
- A skipped test counted as a pass.
- Output from a different commit or build than the one being claimed.

## Release gate

Before tagging v0.1.0:

- [ ] All unit, CLI, and conformance tests pass.
- [ ] Zero skipped tests.
- [ ] `dotnet build -warnaserror` succeeds.
- [ ] All ten manual acceptance scenarios from the implementation plan pass.
- [ ] The specification and implementation have been cross-checked.
- [ ] This log contains the commands and results.

**Arabic — سجل أدلة الاختبار.** لا تُعد أي ميزة أو مرحلة مكتملة حتى يُسجَّل هنا الأمر
المنفَّذ والتاريخ والنتيجة الملاحظة. لا توجد اختبارات منفذة في المرحلة الأولى لأن المحرك لم
يُبنَ بعد. لا تُعد الاختبارات المكتوبة أو المتخطاة أو المتوقعة دليلًا. قبل وسم الإصدار يجب
نجاح كل الاختبارات بلا تخطٍ، وبناء بلا تحذيرات، وسيناريوهات القبول العشرة، وتحديث هذا السجل.
