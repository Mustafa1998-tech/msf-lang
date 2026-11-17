# MSF Lang — Test Evidence Log

**Rule:** a feature or milestone is not complete until this file contains the command that was
actually run, the date, and the observed result. No test result may be inferred or copied from a
plan.

**Status:** Active implementation (post-Gate A). Evidence below records executed builds/tests/runs. Latest full solution run: `dotnet test MSFLang.slnx -c Release` -> **Passed 216, Failed 0, Skipped 0** (Core 110, Conformance 87, CLI 19) — after v0.3 local file modules implementation (2026-11-17).

---

## Evidence entries

| Date | Milestone | Command | Result | Notes |
|---|---|---|---|---|
| 2026-11-16 | M0 | — | Gate A approved (user) | Spec v0.1 accepted as starting point; implementation begun at M1 |
| 2026-11-16 | M1 | `dotnet restore MSFLang.slnx; dotnet build MSFLang.slnx -c Release -warnaserror` | Exit 0. Build succeeded, 0 Warning(s), 0 Error(s). | Solution scaffold, Core/CLI/Test projects, Directory.Build.props, global.json |
| 2026-11-16 | M1 | `dotnet run --project src/MsfLang.Cli -c Release --` | Exit 3. Printed usage. | Smoke: no args => usage error (E5002 path not yet wired, but exit 3 as required) |
| 2026-11-16 | M2 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Source\|FullyQualifiedName~Diagnostic"` | Exit 0. Passed: 10, Failed: 0, Skipped: 0. | SourceText (BOM, LF/CRLF/CR, Unicode scalar columns, tabs, EOF, line text) and DiagnosticFormatter (excerpt/caret/hint/tabs/Unicode/gutter) |
| 2026-11-16 | M3 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Lexer"` | Exit 0. Passed: 31, Failed: 0, Skipped: 0. | Lexer tokens/keywords/identifiers/numbers/strings/escapes/comments, EOF spans, E1001/E1003/E1004/E1005 cases, CRLF/Unicode string handling (subset covering key L-* cases) |
| 2026-11-16 | M4 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Parser"` | Exit 0. Passed: 23, Failed: 0, Skipped: 0. | AST/recursive-descent parser: decls/stmts/exprs/calls, precedence/associativity, if/else-if/dangling else, spans, E2002-E2013 and EOF->E2010, nesting limit 256/257 (key P-* cases) |
| 2026-11-16 | M5 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Resolver"` | Exit 0. Passed: 20, Failed: 0, Skipped: 0. | Bindings/IDs, lexical scoping, textual-order top-level visibility, direct recursion, E3001-E3011 checks (key S-* cases) |
| 2026-11-16 | M6 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release` | Exit 0. Passed: 84, Failed: 0, Skipped: 0. | All existing Core tests pass after interpreter/runtime integration (Source/Diagnostics/Lexer/Parser/Resolver suites) |
| 2026-11-16 | M6 | `dotnet run --project src/MsfLang.Cli -c Release --no-build -- run examples/hello.msf` | Exit 0. Output: `Hello, MSF!` | Basic run pipeline works |
| 2026-11-16 | M6 | `dotnet run --project src/MsfLang.Cli -c Release --no-build -- run examples/factorial.msf` | Exit 0. Output: `3628800`, `55` | Functions/recursion/arithmetic/control flow |
| 2026-11-16 | M6 | `dotnet run --project src/MsfLang.Cli -c Release --no-build -- run %TEMP%\msf-depth-ok.msf` / `...bad.msf` | Exit 0 for `f(999)`; Exit 2 with `E4005` for `f(1000)` | Call-depth limit enforced (1000 active user calls) |
| 2026-11-16 | M6/M7 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release` | Exit 0. Passed: 85, Failed: 0, Skipped: 0. | All Core tests pass including basic `ReplSession` sequence/persistence/auto-display behavior |
| 2026-11-16 | M7 | `Get-Content %TEMP%\msf-repl-test.txt \| dotnet run --project src/MsfLang.Cli -c Release --no-build -- repl` | Exit 0. Output: `10`, `25` (no prompts/banner in piped mode). | Basic REPL CLI (piped) with persistence and auto-display of non-nil expression results |
| 2026-11-16 | M8 | `dotnet test tests/MsfLang.Conformance.Tests/MsfLang.Conformance.Tests.csproj -c Release` | Exit 0. Passed: 21, Failed: 0, Skipped: 0. | Conformance harness + 20 `.msf/.expected.json` cases (positive/negative) + orphan-file check |
| 2026-11-16 | M8+ | `dotnet test MSFLang.slnx -c Release` | Exit 0. Passed: 108, Failed: 0, Skipped: 0 (Conformance 21, Core 85, CLI 2). | Full solution green after adding initial conformance suite and CLI smoke tests |
| 2026-11-16 | M8 (expanded) | `dotnet test tests/MsfLang.Conformance.Tests/MsfLang.Conformance.Tests.csproj -c Release` | Exit 0. Passed: 64, Failed: 0, Skipped: 0. | Conformance expanded to **63** `.msf/.expected.json` cases (positive/negative across lexical/syntax/semantic/runtime) + orphan-file check |
| 2026-11-16 | M7/M8 | `dotnet test tests/MsfLang.Cli.Tests/MsfLang.Cli.Tests.csproj -c Release` | Exit 0. Passed: 12, Failed: 0, Skipped: 0. | CLI process tests (help/version/usage/unknown, run success/error/io, repl piped) |
| 2026-11-16 | All (current) | `dotnet test MSFLang.slnx -c Release` | Exit 0. Passed: 161, Failed: 0, Skipped: 0 (Core 85, Conformance 64, CLI 12). | Full solution green with expanded conformance and CLI tests |
| 2026-11-16 | M6/M7 | `dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release` / `dotnet test tests/MsfLang.Cli.Tests/MsfLang.Cli.Tests.csproj -c Release` | Exit 0. Core Passed: 88, Failed: 0, Skipped: 0. CLI Passed: 15, Failed: 0, Skipped: 0. | REPL rollback/state handling fixes, incomplete-input/runtime rollback tests, and additional CLI REPL process tests (EOF/multiline/unknown command) |
| 2026-11-16 | M8 (expanded) | `dotnet test tests/MsfLang.Conformance.Tests/MsfLang.Conformance.Tests.csproj -c Release` | Exit 0. Passed: 72, Failed: 0, Skipped: 0. | Conformance expanded to **71** `.msf/.expected.json` cases + orphan-file check |
| 2026-11-16 | All (current) | `dotnet test MSFLang.slnx -c Release` | Exit 0. Passed: 175, Failed: 0, Skipped: 0 (Core 88, Conformance 72, CLI 15). | Full solution green after M6/M7/M8 hardening and test expansion |
| 2026-11-16 | M9 acceptance | Manual CLI/REPL scenarios (per implementation plan §6, scenarios 1-10) | Passed 10/10. | Verified hello/factorial, runtime/semantic diagnostics & exit codes, missing file I/O error, piped REPL auto-display/multiline/rollback, `--version`, path with spaces, UTF-8 Arabic+emoji output |
| 2026-11-16 | All (current) | `dotnet test MSFLang.slnx -c Release` / DTB cycle after v0.2 loop-control additions | Exit 0. Passed: 184, Failed: 0, Skipped: 0 (Core 88, Conformance 81, CLI 15). | Full suite green after repeatability/fixes: E3014 fixture, spec sync, README/ROADMAP doc updates |
| 2026-11-17 | All (v0.2 maps/MSF_DEBUG) | `dotnet test MSFLang.slnx -c Release` | Exit 0. Passed: 188, Failed: 0, Skipped: 0 (Core 88, Conformance 83, CLI 17). | v0.2 maps (`map {}`, `get/set/has/remove`, `len(map)`), ADR-0019, EN/AR map specs, `MSF_DEBUG=1` internal-error stack trace gating |
| 2026-11-17 | All (v0.2 math/string builtins) | `dotnet test MSFLang.slnx -c Release` | Exit 0. Passed: 190, Failed: 0, Skipped: 0 (Core 88, Conformance 85, CLI 17). | `abs/sqrt/floor/ceil/round/pow/min/max`, `upper/lower/trim/contains/startsWith/endsWith/split`, ADR-0020, EN/AR drafts |
| 2026-11-17 | v0.3 modules build | `dotnet build MSFLang.slnx -c Release -warnaserror` | Exit 0. 0 Error(s). | After v0.3 local file modules implementation (import/export, qualified access, module runtime isolation) |
| 2026-11-17 | v0.3 manual run | `dotnet run --project src/MsfLang.Cli -c Release --no-build -- run examples/modules/main.msf` | Exit 0. Output: `81`, `3.5`, `14`. | Multi-file program with qualified module access works |
| 2026-11-17 | v0.3 full suite | `dotnet test MSFLang.slnx -c Release` | Exit 0. Passed: 216, Failed: 0, Skipped: 0 (Core 110, Conformance 87, CLI 19). | Local file modules: `import`/`export`, qualified access (`math.square(9)`), module-executes-once across importers, import-cycle detection `E4014`, missing module `E4010` (exit 4), REPL module persistence across entries; ADR-0021 accepted |
| 2026-11-17 | v0.3 manual acceptance (10/10) | Manual scenarios on built CLI (`bin/Release/net10.0/msf.dll`) | All passed: hello/factorial/lists examples exit 0; runtime error exit 2 (`E4002`); semantic error exit 1 (`E3013`); missing file exit 4 (`E5003`); missing module exit 4 (`E4010`); piped REPL multiline function prints `5`; REPL rollback leaves `a=1` (runtime error rolls back `boom()` entry); `--version` prints `msf 0.3.0-rc.1`; path with spaces + UTF-8 Arabic output OK; module multi-file sample prints 81/3.5/14 |
| 2026-11-17 | v0.3.0-rc.1 | Release candidate published | `git tag -a v0.3.0-rc.1 a600cbd`; `git push origin v0.3.0-rc.1`; `gh release create v0.3.0-rc.1 --prerelease` | Annotated tag created on master @ a600cbd; GitHub pre-release published: https://github.com/Mustafa1998-tech/msf-lang/releases/tag/v0.3.0-rc.1 |
| — | M9 | — | RC published | v0.3.0-rc.1 tagged and pre-released (2026-11-17). NOT a production release. 10/10 manual acceptance scenarios re-verified. Production release/signing still approval-gated |

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

**Arabic — سجل أدلة الاختبار.** لا تُعد أي ميزة أو مرحلة مكتملة حتى يُسجَّل هنا الأمر المنفَّذ والتاريخ والنتيجة الملاحظة. السجل أدناه يحتوي نتائج فعلية منفذة (أحدثها: 216 اختبارًا ناجحًا بلا إخفاقات أو تخطٍ: Core 110، Conformance 87، CLI 19). لا تُعد الاختبارات المكتوبة أو المتخطاة أو المتوقعة دليلًا. قبل وسم أي مرشح إطلاق يجب نجاح كل الاختبارات بلا تخطٍ، وبناء بلا تحذيرات، وسيناريوهات القبول العشرة مُعادة التحقق من v0.3، وتحديث هذا السجل.
