# MSF Lang v0.1 — Implementation and Test Plan

**Spec:** [`SPEC-v0.1.md`](SPEC-v0.1.md), version `0.1.0-draft.1`  
**Status:** Gate A approved; implementation in progress. Core/CLI/tests are implemented with recorded evidence (latest full suite: 161 passed / 0 failed / 0 skipped; 63 conformance cases). This document remains the milestone/test-plan reference and may lag implementation details in places; see [`../TEST-EVIDENCE.md`](../TEST-EVIDENCE.md) and [`../ARCHITECTURE.md`](../ARCHITECTURE.md) for current state.  
**Target runtime:** C# 14 / .NET 10 (`net10.0`), Windows first, portable design.

---

## 1. What this plan proves

The plan is written so that every claim can be checked by a command and an observed result:

- Each language feature maps to a unit test, an integration test, or a `.msf` conformance case.
- Each milestone has an exit criterion that is a command, not an opinion.
- Test success is recorded in [`docs/TEST-EVIDENCE.md`](../TEST-EVIDENCE.md) with the exact
  command, date, and output summary. Until a row is filled in, the feature is not complete.
- No feature outside the approved specification is added without a new decision record.

> **Current evidence:** none. There is no solution, no source code, and no test project yet.
> This document describes what will be built after Gate A.

---

## 2. Architecture

### 2.1 Pipeline

```text
 file bytes
    |
    v
[ strict UTF-8 reader ] -- E5005 / exit 4
    |
    v
[ SourceText + line map ]
    |
    v
[ Lexer ] -- E1xxx / exit 1
    |  tokens
    v
[ Parser ] -- E2xxx / exit 1
    |  AST with spans
    v
[ Resolver ] -- E3xxx / exit 1
    |  annotated AST + bindings
    v
[ Tree-walk Interpreter ] -- E4xxx / exit 2
    |
    v
 stdout / stderr / exit code
```

Every phase fails fast with one `Diagnostic`. No phase catches a lower phase's language error and
continues.

### 2.2 Design choices

| Decision | Choice | Reason |
|---|---|---|
| Lexer | Hand-written `Rune`-aware scanner | Exact locations, no generator dependency |
| Parser | Recursive descent with layered precedence | Grammar is LL(1); easy error locations |
| AST | Immutable records/classes carrying `TextSpan` | Simple and debuggable |
| Name resolution | Separate resolver, bindings by numeric ID | Static errors; no string lookup at runtime |
| Execution | Tree-walk interpreter | Required for v0.1; simplest correct runtime |
| Numbers | `double`, finite only | One numeric type; deterministic errors |
| Runtime values | Primitive value hierarchy | No user-defined types in v0.1 |
| Functions | Top-level only, no closures, exact arity | Small, statically checkable |
| CLI | `System.CommandLine`? **No.** Hand-written argument parser | One fewer dependency; exact contract |
| JSON in tests | `System.Text.Json` | BCL only |
| Test framework | xUnit | Mature .NET integration |

The Core project has **no third-party runtime dependencies**. Test projects may use xUnit and the
test SDK.

### 2.3 Planned solution layout

```text
MSF Lang/
|-- MSFLang.sln
|-- Directory.Build.props
|-- global.json
|-- src/
|   |-- MsfLang.Core/
|   |   |-- MsfLang.Core.csproj
|   |   |-- MsfEngine.cs                 # public facade
|   |   |-- Source/
|   |   |   |-- SourceText.cs
|   |   |   |-- TextSpan.cs
|   |   |   `-- SourcePosition.cs
|   |   |-- Diagnostics/
|   |   |   |-- Diagnostic.cs
|   |   |   |-- DiagnosticCode.cs
|   |   |   |-- DiagnosticPhase.cs
|   |   |   |-- DiagnosticFormatter.cs
|   |   |   `-- LanguageException.cs
|   |   |-- Lexing/
|   |   |   |-- TokenKind.cs
|   |   |   |-- Token.cs
|   |   |   `-- Lexer.cs
|   |   |-- Syntax/
|   |   |   |-- Ast.cs
|   |   |   |-- Parser.cs
|   |   |   `-- AstPrinter.cs           # debug-only, not a CLI feature
|   |   |-- Semantics/
|   |   |   |-- Binding.cs
|   |   |   |-- BindingKind.cs
|   |   |   |-- Resolver.cs
|   |   |   `-- SemanticModel.cs
|   |   |-- Runtime/
|   |   |   |-- MsfValue.cs
|   |   |   |-- Environment.cs
|   |   |   |-- Interpreter.cs
|   |   |   |-- Builtins.cs
|   |   |   `-- ValueFormatter.cs
|   |   `-- Repl/
|   |       `-- ReplSession.cs
|   `-- MsfLang.Cli/
|       |-- MsfLang.Cli.csproj
|       |-- Program.cs
|       |-- CommandLine.cs
|       |-- ExitCode.cs
|       |-- RunCommand.cs
|       `-- ReplCommand.cs
|-- tests/
|   |-- MsfLang.Core.Tests/
|   |-- MsfLang.Cli.Tests/
|   |-- MsfLang.Conformance.Tests/
|   |-- MsfLang.TestSupport/
|   `-- conformance/
|       |-- positive/
|       |-- negative/
|       `-- repl/
|-- examples/
|-- docs/
|-- grammar/
`-- branding/
```

### 2.4 Project settings

`Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest-recommended</AnalysisLevel>
    <InvariantGlobalization>true</InvariantGlobalization>
    <Deterministic>true</Deterministic>
  </PropertyGroup>
</Project>
```

`global.json` pins SDK `10.0.302` with `rollForward: latestFeature`. The CLI assembly name is
`msf` so the built apphost is `msf.exe` on Windows.

---

## 3. Component contracts

### 3.1 `SourceText`

Responsibilities:

- Hold the display path and the original text.
- Accept an optional leading BOM and expose the text without it.
- Build a line-start index for LF, CRLF, and CR.
- Convert an absolute character offset to a 1-based `SourcePosition`.
- Count columns by Unicode scalar values (`Rune`), not UTF-16 code units.
- Return the source line for diagnostic excerpts.

The lexer never computes line/column itself; it records offsets and asks `SourceText`.

### 3.2 Diagnostics

`Diagnostic`:

```text
Code        string        // "E4002"
Phase       enum          // Lexical, Syntax, Semantic, Runtime, Cli, Io, Internal
Message     string
Span        TextSpan
Start       SourcePosition
Hint        string?
```

`DiagnosticFormatter` renders the format in SPEC section 12.3. It is the only code allowed to
decide layout. Tests assert both the structured fields and the rendered text for a small set of
golden diagnostics.

Exceptions:

- `LexException`, `SyntaxException`, `SemanticException` carry one `Diagnostic`.
- `RuntimeException` carries one `Diagnostic`.
- `InternalErrorException` is not a language diagnostic and maps to exit 70.

No exception message is parsed to recover a code. The code is a field.

### 3.3 Lexer

Public contract:

```csharp
IReadOnlyList<Token> Lex(SourceText source);
```

Rules:

- Skip whitespace, line comments, and non-nesting block comments.
- Accept BOM only at offset 0.
- Recognize keywords and identifiers case-sensitively.
- Recognize decimal literals with an optional fractional part; reject non-finite literals.
- Recognize strings and the five escapes; reject raw newlines and unknown escapes.
- Emit `EOF` with a zero-length span at the end.
- Throw `LexException` on the first lexical error.

`Token` contains `Kind`, `Lexeme`, optional literal value, and `TextSpan`. Token kinds are
explicit (`Let`, `Var`, `Fn`, `If`, `Else`, `Return`, `True`, `False`, `Nil`, `And`, `Or`, `Not`,
`Identifier`, `Number`, `String`, punctuation, `Eof`).

### 3.4 AST

Top-level nodes:

```text
Program        : Declaration[] 
Declaration    : FunctionDecl | VarDecl | Statement
Statement      : IfStmt | ReturnStmt | BlockStmt | AssignStmt | ExprStmt
Expression     : LiteralExpr | NameExpr | UnaryExpr | BinaryExpr | CallExpr | GroupingExpr
```

`FunctionDecl` holds the name token, parameter tokens, body block, and span. `VarDecl` holds
`IsMutable`, name token, initializer, and span. Every node has a `TextSpan`. Assignment is a
statement and is never an expression. `CallExpr` holds the callee name token and arguments; it
does not accept an arbitrary expression as callee.

### 3.5 Parser

Public contract:

```csharp
Program Parse(IReadOnlyList<Token> tokens);
```

Implementation:

- One method per grammar production, named after the production.
- Layered expression parsing exactly as in SPEC section 4.2.
- Assignment lookahead: `IDENT` followed by `=` at statement start.
- When a non-name expression is followed by `=`, throw `E2011` at the `=`.
- Track nesting depth for expressions, blocks, and if/else chains; enforce 256 (`E2013`).
- Always report the offending token. At EOF, use `E2010` for every expected-token failure.
- Build spans that start at the first token's start and end at the last consumed token's end.
- Throw `SyntaxException` on the first error.

### 3.6 Resolver

The resolver performs static checks and annotates name-bearing nodes with a `Binding`.

`Binding`:

```text
Id             int          // unique for the lifetime of a run/session
Name           string
Kind           Variable | Function | Builtin
IsMutable      bool         // variables only
Arity          int?         // user functions
Declaration    TextSpan
```

Scopes:

- A `Scope` maps name -> binding and has a parent.
- The global scope starts with `print` (Builtin, variadic).
- A function body's scope contains parameters and body-top-level locals. It is not an extra child
  of the parameter scope.
- Every other block creates a child scope.
- The initializer is resolved before the new binding is declared.
- Top-level functions are declared in the enclosing scope before their body is resolved, so direct
  recursion works. They are not hoisted, so later functions are not visible.
- The resolver does not perform type inference. Type errors are runtime errors.

Checks, in order of appearance:

| Code | Check | Reported at |
|---|---|---|
| `E3001` | Name not found | The name token |
| `E3002` | Duplicate declaration in one scope | The second name token |
| `E3003` | Duplicate parameter | The second parameter token |
| `E3004` | Declaration named `print` | The declared name |
| `E3005` | Assign to `let` | The assignment target |
| `E3006` | Assign to function/builtin | The assignment target |
| `E3007` | `fn` not at top level | The `fn` keyword |
| `E3008` | Direct call arity mismatch | The callee name |
| `E3009` | Call a variable | The callee name |
| `E3010` | Function/builtin used as a value | The name token |
| `E3011` | `return` outside a function | The `return` keyword |

The interpreter MUST use the resolver's binding IDs. It MUST NOT look up names by string at
runtime, except for debug tooling.

### 3.7 Interpreter

Public contract:

```csharp
RunResult Execute(Program program, SemanticModel model, TextWriter stdout);
```

- Values: `MsfNumber`, `MsfString`, `MsfBoolean`, `MsfNil`, plus internal callable values.
- Environment: `Dictionary<int, MsfValue>` with a parent pointer. Block scopes are children;
  function call scopes have the global environment as parent.
- `let` evaluates the initializer, then defines the binding once.
- `var` does the same and permits later assignment.
- Assignment evaluates the right side, then updates the binding.
- `if` evaluates the condition, requires a boolean, then executes one branch.
- `return` uses an internal `ReturnSignal` exception caught only at the function boundary.
- User function call checks the depth counter, binds arguments, executes the body, and returns
  `nil` if the body falls through.
- `print` formats each argument and writes one line. It flushes.
- E4001/E4002/E4003/E4004/E4005/E4006 are raised with the span specified in the spec.
- Numeric results are checked with `double.IsFinite`.
- No exceptions escape as language errors except `RuntimeException`; internal bugs become
  `InternalErrorException` with exit 70.

### 3.8 Stack-safety strategy

The interpreter enforces a maximum of 1000 active user calls, but a tree-walk interpreter uses
many host stack frames per language call. To make the limit safe:

- The CLI runs the engine on a dedicated `Thread` with a **32 MiB** stack.
- The test host uses the same thread helper for recursion-depth tests and conformance runs.
- The depth check occurs before binding arguments, so call 1001 fails with `E4005` rather than a
  host `StackOverflowException`.
- A test calls a recursive function 1001 times and asserts `E4005`; a separate test confirms that
  1000 nested calls complete.

This behavior is part of the plan, not the language surface; the language only promises the
1,000-call limit.

### 3.9 CLI

`Program.Main` returns an exit code. The command parser is hand-written:

```text
args[0] == "run"     -> require exactly one path
args[0] == "repl"    -> require no more args
args[0] == "--help"  -> print help, 0
args[0] == "-h"      -> print help, 0
args[0] == "--version" -> print "msf 0.1.0", 0
args[0] == "-v"      -> print "msf 0.1.0", 0
otherwise            -> E5001/E5002, 3
```

`RunCommand`:

- Strict UTF-8 read with `new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
  throwOnInvalidBytes: true)`.
- Distinguish `FileNotFoundException`/`DirectoryNotFoundException` (E5003) from other read
  failures (E5004) and `DecoderFallbackException` (E5005).
- Set `Console.Out.NewLine = "\n"` and use UTF-8 output.
- Format diagnostics to stderr; do not color.
- Run on the dedicated-stack thread.
- Map phases to exit codes 1 and 2.

### 3.10 REPL

`ReplSession` owns:

- a persistent global `Scope` in the resolver,
- a persistent global `Environment` in the interpreter,
- a monotonically increasing `Binding.Id` counter.

For each entry:

1. Read lines until complete or EOF.
2. Snapshot the global scope and environment (shallow copies of the dictionaries and the next ID).
3. Lex, parse, and resolve the accumulated source with the persistent global scope.
4. Execute.
5. On a compile error: print the diagnostic, restore nothing (resolution is transactional: the
   resolver mutates only a working copy or applies declarations after success), and continue.
6. On a runtime error: print the diagnostic, restore the snapshot, and continue.
7. On success: commit the new scope/environment and print any auto-display values.

The resolver MUST be transactional for REPL entries. A compile error must not leave a partial
binding. The simplest implementation is to resolve into a cloned global scope and swap on
success; this is cheap for v0.1.

Prompt/banner behavior follows SPEC section 14. Non-interactive mode is line-oriented and emits
no prompts, which makes it testable with redirected stdin.

### 3.11 Public facade

```csharp
public sealed record RunResult(
    int ExitCode,
    string StandardOutput,
    Diagnostic? Diagnostic);

public static class MsfEngine
{
    public static RunResult Run(SourceText source);
}
```

The CLI can use overloads that write directly to `TextWriter`; the in-process test harness uses a
`StringWriter`. `RunResult` never contains a stack trace.

---

## 4. Testing strategy

### 4.1 Test layers

| Layer | Project | What it proves |
|---|---|---|
| Unit — source/diagnostics | `MsfLang.Core.Tests` | Offsets, lines, columns, BOM, CRLF/CR, Unicode, formatting |
| Unit — lexer | `MsfLang.Core.Tests` | Every token and lexical error |
| Unit — parser | `MsfLang.Core.Tests` | Grammar, precedence, spans, syntax errors, nesting limit |
| Unit — resolver | `MsfLang.Core.Tests` | Scopes, mutability, function rules, all E3xxx |
| Unit — interpreter | `MsfLang.Core.Tests` | Values, operators, control flow, functions, print, depth, all E4xxx |
| Integration — CLI | `MsfLang.Cli.Tests` | Process arguments, streams, exit codes, REPL |
| Conformance | `MsfLang.Conformance.Tests` | Real `.msf` programs against golden expectations |

Unit tests call the Core facade directly. CLI tests launch the built executable as a child process
and assert raw stdout/stderr/exit code, so they also catch encoding and newline mistakes.

### 4.2 Conformance fixture format

Every `.msf` file under `tests/conformance/` has a sidecar with the same base name:

```text
tests/conformance/positive/arithmetic-precedence.msf
tests/conformance/positive/arithmetic-precedence.expected.json
```

```json
{
  "exitCode": 0,
  "stdout": "7\n9\n-1\n3.75\n",
  "diagnostic": null
}
```

For an error case:

```json
{
  "exitCode": 2,
  "stdout": "",
  "diagnostic": {
    "code": "E4002",
    "line": 7,
    "column": 14,
    "phase": "runtime"
  }
}
```

Rules:

- `stdout` is exact, with `\n` line endings, and is compared as text.
- `exitCode` is required.
- `diagnostic` is `null` for a successful run.
- The harness fails if a `.msf` file has no sidecar, or a sidecar has no `.msf` file.
- The sidecar is UTF-8 JSON with no comments.
- The conformance harness runs in-process and does not assert the full diagnostic text; CLI tests
  assert the diagnostic format separately.
- Every conformance program is a valid MSF Lang program and is included in the repository.
- No test may be skipped or marked "pending" in a release gate.

### 4.3 Minimum coverage of the specification

These are minimum test counts, not claims of completed work:

| Area | Minimum tests | Required coverage |
|---|---:|---|
| Source, positions, diagnostics | 20 | BOM, LF/CRLF/CR, tabs, Arabic, emoji, EOF, caret format |
| Lexer | 35 | Every token kind, comments, escapes, malformed inputs |
| Parser | 35 | Every production, precedence, associativity, errors, 256 limit |
| Resolver | 30 | Every E3xxx, shadowing, initializer order, recursion visibility |
| Interpreter | 55 | Every operator/type combination, equality, short-circuit, scopes, calls, print, finite checks, depth |
| CLI integration | 18 | run/repl/help/version, all exit codes, IO, UTF-8, newlines, prompt suppression |
| Conformance programs | 60 | Positive and negative programs across every feature |
| REPL integration | 8 | Persistence, auto-display, multiline, rollback, EOF, quit |

### 4.4 Mandatory test matrix

#### Lexer (`L-*`)

- `L-01` empty file -> only EOF
- `L-02` BOM at start is ignored
- `L-03` BOM not at start -> E1001
- `L-04` spaces, tabs, LF, CRLF, CR
- `L-05` line comment at start, middle, EOF
- `L-06` block comment, including newlines
- `L-07` nested block comment ends at first `*/`
- `L-08` unterminated block comment -> E1005 at opening
- `L-09` every keyword token
- `L-10` keyword prefixes (`lets`, `iffy`) are identifiers
- `L-11` identifier with underscore/digits
- `L-12` integer literals, zero, multi-digit
- `L-13` fractional literals
- `L-14` `1.` / `.5` do not form a number
- `L-15` non-finite literal -> E1002
- `L-16` string with Arabic and emoji
- `L-17` each escape: `\n \t \r \\ \"`
- `L-18` unknown escape -> E1004
- `L-19` raw newline in string -> E1003
- `L-20` unterminated string at EOF -> E1003
- `L-21` two-character operators
- `L-22` lone `!`, `?`, `&`, `|`, `[` -> E1001
- `L-23` columns count scalar values for Arabic
- `L-24` columns count scalar values for emoji
- `L-25` tab counts as one column
- `L-26` CRLF counts as one line
- `L-27` EOF span is zero-length after last character
- `L-28` `print` lexes as identifier, not keyword
- `L-29` comments do not produce tokens
- `L-30` string containing `//` and `/*` is one token
- `L-31` empty string
- `L-32` string with escaped quote
- `L-33` `0.0` and `-0` handling (the latter via parser/unary)
- `L-34` very long identifier
- `L-35` unexpected backslash outside a string

#### Parser (`P-*`)

- `P-01` empty program
- `P-02` variable declarations, both keywords
- `P-03` assignment statement
- `P-04` expression statement
- `P-05` function declaration with zero/one/many params
- `P-06` call with zero/one/many args
- `P-07` nested calls as arguments
- `P-08` grouping
- `P-09` if/else and else-if
- `P-10` dangling-else binds to nearest `if`
- `P-11` standalone block
- `P-12` return with and without expression
- `P-13` unary chains `--1`, `not not true`
- `P-14` arithmetic precedence
- `P-15` comparison/equality/logical precedence
- `P-16` left associativity of `-`, `/`, `and`
- `P-17` missing `;` -> E2002
- `P-18` missing `;` at EOF -> E2010
- `P-19` missing `)` -> E2003
- `P-20` missing `{` -> E2004
- `P-21` missing `}` at EOF -> E2010
- `P-22` missing identifier -> E2006
- `P-23` missing `(` after `if`/`fn` -> E2007
- `P-24` missing `=` in declaration -> E2008
- `P-25` lone `;` -> E2009
- `P-26` `(f)(1)` rejected
- `P-27` `f()()` rejected
- `P-28` `x + 1 = 2;` -> E2011 at `=`
- `P-29` trailing comma rejected
- `P-30` `else` without `if` rejected
- `P-31` `return` in a value position rejected
- `P-32` 256 nested parentheses accepted
- `P-33` 257 nested parentheses -> E2013
- `P-34` spans start/end on the correct tokens
- `P-35` keywords cannot be parameter names

#### Resolver (`S-*`)

- `S-01` valid forward use of a global declared before a function
- `S-02` later global inside a function -> E3001
- `S-03` call before function declaration -> E3001
- `S-04` direct recursion resolves
- `S-05` mutual recursion fails at the forward call
- `S-06` undefined variable/function -> E3001
- `S-07` duplicate variable in same scope -> E3002
- `S-08` shadowing in a child block is allowed
- `S-09` duplicate parameter -> E3003
- `S-10` parameter and body local same name -> E3002
- `S-11` `let print = 1;` -> E3004 in any scope
- `S-12` assignment to `let` -> E3005
- `S-13` assignment to function -> E3006
- `S-14` assignment to `print` -> E3006
- `S-15` nested function -> E3007
- `S-16` wrong arity (too few/too many) -> E3008
- `S-17` calling a variable -> E3009
- `S-18` function used as value -> E3010
- `S-19` builtin used as value -> E3010
- `S-20` `return` at top level -> E3011
- `S-21` `return` in a nested block inside a function is allowed
- `S-22` initializer sees outer binding, not the new one
- `S-23` initializer without outer binding -> E3001
- `S-24` local variable shadows a global function
- `S-25` top-level variable/function name collision -> E3002
- `S-26` parameter assignment is allowed
- `S-27` `var` can change value type
- `S-28` function body cannot see caller locals (only globals) — scoping test
- `S-29` zero-argument call accepted
- `S-30` print accepts zero and many args

#### Interpreter (`R-*`)

- `R-01` number literal and display
- `R-02` string literal and display
- `R-03` boolean and nil display
- `R-04` `+ - * / %` numeric
- `R-05` division by zero -> E4002
- `R-06` modulo by zero -> E4003
- `R-07` `%` sign behavior
- `R-08` string concatenation
- `R-09` mixed `+` -> E4004
- `R-10` numeric operator on string/bool -> E4004
- `R-11` comparison numbers
- `R-12` comparison wrong types -> E4004
- `R-13` equality same type
- `R-14` equality different types -> false/true, no error
- `R-15` nil equality
- `R-16` `and` true/false
- `R-17` `or` true/false
- `R-18` `not`
- `R-19` short-circuit avoids division by zero
- `R-20` logical non-boolean -> E4004
- `R-21` condition non-boolean -> E4001
- `R-22` if/else-if/else branch selection
- `R-23` block scoping and shadowing
- `R-24` let/var assignment behavior
- `R-25` assignment evaluates right side then updates
- `R-26` function call and return value
- `R-27` fall-through returns nil
- `R-28` bare return returns nil
- `R-29` parameter assignment
- `R-30` recursion factorial
- `R-31` recursion fibonacci
- `R-32` 1000 active calls succeed
- `R-33` 1001 active calls -> E4005
- `R-34` `print()` newline
- `R-35` print separators and multiple values
- `R-36` print strings unquoted
- `R-37` number formatting: integers, fractions, `-0`, scientific
- `R-38` print returns nil
- `R-39` output before runtime error remains
- `R-40` empty program
- `R-41` expression values discarded in run mode
- `R-42` finite overflow -> E4006
- `R-43` very large but finite literal accepted
- `R-44` unary minus on non-number -> E4004
- `R-45` nested function calls and argument order
- `R-46` if condition evaluated once
- `R-47` function values not reachable at runtime (resolver gate)
- `R-48` `0` vs `-0` equality and display
- `R-49` string equality is exact
- `R-50` Arabic/emoji output
- `R-51` multiple statements execute in order
- `R-52` return exits nested blocks
- `R-53` global read at call time reflects reassignment
- `R-54` no dynamic scoping (resolver prevents caller locals)
- `R-55` print flush is observable through redirected stdout

#### CLI (`C-*`)

- `C-01` `run` success, exit 0
- `C-02` runtime error, exit 2, stderr diagnostic
- `C-03` semantic error, exit 1, no execution
- `C-04` syntax error, exit 1
- `C-05` missing file, exit 4, E5003
- `C-06` invalid UTF-8, exit 4, E5005
- `C-07` no arguments, exit 3
- `C-08` unknown command, exit 3
- `C-09` `run` with zero/two paths, exit 3
- `C-10` `repl` with extra args, exit 3
- `C-11` `--version` exact output, exit 0
- `C-12` `--help` exit 0
- `C-13` output uses LF when redirected
- `C-14` UTF-8 output when redirected
- `C-15` diagnostic line/column/caret format
- `C-16` run on path with spaces
- `C-17` REPL piped: persistence and auto-display
- `C-18` REPL piped: no banner/prompts, exit 0
- `C-19` REPL piped: compile error, state unchanged
- `C-20` REPL piped: runtime error rollback, state restored
- `C-21` REPL piped: multiline function definition
- `C-22` REPL piped: EOF while incomplete -> one E2010
- `C-23` `:quit` and `:help`
- `C-24` runtime output before error preserved in stdout

#### Conformance (`CFP-*`)

At least 60 `.msf` programs, including:

- hello world
- variable declaration/assignment
- arithmetic precedence and associativity
- negative numbers
- remainder signs
- division by zero
- string concatenation
- string escapes
- Arabic and emoji output
- equality across types
- all comparison operators
- short-circuit `and`/`or`
- log short-circuit avoids division by zero
- non-boolean condition
- if / else-if / else
- nested if and dangling else
- block shadowing
- initializer shadowing
- function with zero/one/many params
- recursion factorial
- recursion fibonacci
- gcd
- wrong arity
- undefined name
- duplicate declaration
- immutable assignment
- function as value
- call a variable
- return outside function
- nested function
- print zero/one/many args
- number formatting
- runtime type mismatch
- compile error before output
- runtime error after output
- recursion depth error
- 256/257 nesting
- REPL-oriented programs under `tests/conformance/repl/`

### 4.5 Traceability

| Specification | Tests | Milestone |
|---|---|---|
| §3 source/lexing | L-01…L-35 | M2, M3 |
| §4 grammar | P-01…P-35 | M4 |
| §5 values/equality | R-01…R-15, R-48 | M6 |
| §6 scopes/declarations | S-01…S-30, R-22…R-25 | M5, M6 |
| §7 expressions | R-01…R-21, R-42…R-44 | M6 |
| §8 statements | R-22…R-25, R-52 | M6 |
| §9 functions | S-03…S-19, R-26…R-33 | M5, M6 |
| §10 print | R-34…R-38, R-50 | M6 |
| §11 execution | R-39…R-55 | M6 |
| §12 diagnostics | D-tests, C-15 | M2, M7 |
| §13 CLI | C-01…C-16 | M7 |
| §14 REPL | C-17…C-24 | M7 |
| §16 conformance | CFP-* | M8 |

---

## 5. Milestones, commands, and exit criteria

Every milestone ends with a recorded command and observed result in `docs/TEST-EVIDENCE.md`.
A milestone is not complete because code exists; it is complete when its exit criteria pass.

### M0 — Specification approval (current)

Deliverables:

- `docs/en/SPEC-v0.1.md`, `docs/ar/SPEC-v0.1.md`
- `grammar/msf-v0.1.ebnf`
- this plan and its Arabic counterpart
- `docs/DECISIONS.md`
- three brand concepts and preview

Exit criterion: the owner approves Gate A and the v0.1 scope. **No code is written before this.**

### M1 — Solution scaffold

Deliverables:

- solution, Core, CLI, test projects, `Directory.Build.props`, `global.json`
- empty facade and test project references
- no language behavior

Commands:

```powershell
dotnet restore MSFLang.sln
dotnet build MSFLang.sln -c Release -warnaserror
```

Exit criterion: restore and build succeed with zero warnings. A smoke test confirms the CLI
executable starts and exits 3 with no arguments.

### M2 — Source text and diagnostics

Deliverables:

- `SourceText`, `SourcePosition`, `TextSpan`
- `Diagnostic`, `DiagnosticCode`, `DiagnosticFormatter`
- tests for BOM, LF/CRLF/CR, tabs, Arabic, emoji, EOF, caret output

Command:

```powershell
dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Source|FullyQualifiedName~Diagnostic"
```

Exit criterion: source/diagnostic tests pass; no test is skipped.

### M3 — Lexer

Deliverables: all token kinds, comments, strings, numbers, lexical errors.

```powershell
dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Lexer"
```

Exit criterion: L-01…L-35 pass; token spans verified; no lexer crash on fuzz-like malformed input.

### M4 — AST and parser

Deliverables: AST records, parser, syntax diagnostics, nesting limit.

```powershell
dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Parser"
```

Exit criterion: P-01…P-35 pass; precedence and spans verified; no parser stack overflow on 257
nested parentheses (it returns E2013).

### M5 — Resolver

Deliverables: bindings, scopes, all E3xxx checks, recursive function visibility.

```powershell
dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Resolver"
```

Exit criterion: S-01…S-30 pass; every semantic code has at least one test; no runtime name lookup
is required by the interpreter's binding IDs.

### M6 — Interpreter

Deliverables: values, environments, all expressions/statements/functions, print, depth limit,
finite-number checks.

```powershell
dotnet test tests/MsfLang.Core.Tests/MsfLang.Core.Tests.csproj -c Release --filter "FullyQualifiedName~Interpreter"
dotnet run --project src/MsfLang.Cli -c Release -- run examples/hello.msf
```

Exit criterion: R-01…R-55 pass; hello and factorial produce exact output; 1001 nested calls give
E4005, not a host crash.

### M7 — CLI and REPL

Deliverables: `run`, `repl`, help, version, exit codes, streams, REPL persistence/rollback.

```powershell
dotnet test tests/MsfLang.Cli.Tests/MsfLang.Cli.Tests.csproj -c Release
```

Exit criterion: C-01…C-24 pass as real child processes; redirected output uses LF; REPL rollback
test passes.

### M8 — Conformance suite and examples

Deliverables:

- 60+ `.msf` programs and `.expected.json` sidecars
- conformance harness
- `examples/hello.msf`, `examples/factorial.msf`, `examples/classify.msf`,
  `examples/errors/div-zero.msf`, `examples/errors/immutable.msf`
- README quick start

```powershell
dotnet test tests/MsfLang.Conformance.Tests/MsfLang.Conformance.Tests.csproj -c Release
```

Exit criterion: every conformance case passes; no missing sidecars; no skipped tests; examples
produce the documented output under `msf run`.

### M9 — Hardening and release candidate

Deliverables:

- full test suite green on Windows
- coverage report
- manual smoke script
- updated test evidence log
- spec/implementation cross-check
- `CHANGELOG.md` with 0.1.0-rc.1
- no known severity-1 bugs

Commands:

```powershell
dotnet build MSFLang.sln -c Release -warnaserror
dotnet test MSFLang.sln -c Release --no-build
dotnet run --project src/MsfLang.Cli -c Release -- --version
dotnet run --project src/MsfLang.Cli -c Release -- run examples/hello.msf
```

Exit criterion: all commands pass, all tests green, zero skipped, and the evidence log is filled.
Only then is v0.1.0 tagged.

---

## 6. Verification scenarios (black-box acceptance)

These are the scenarios a reviewer can run manually after M8:

1. `msf run examples/hello.msf` -> `Hello, MSF!` and exit 0.
2. `msf run examples/factorial.msf` -> `3628800` then `55` and exit 0.
3. A runtime type error -> exit 2, diagnostic with E4004, correct line/column, no stack trace.
4. An immutable assignment -> exit 1, E3005, and no output from the program.
5. A missing file -> exit 4, E5003, no crash.
6. Piped REPL:
   ```text
   let x = 5;
   x * 2;
   fn square(n) { return n * n; }
   square(x);
   ```
   -> `10` and `25`, exit 0, no prompts.
7. REPL rollback: declare `var x = 1;`, then an entry that assigns `x = 2;` and divides by zero;
   after the error, `x;` still displays `1`.
8. `msf --version` -> exactly `msf 0.1.0`.
9. Path with spaces works.
10. Arabic string output is UTF-8 and not mojibake.

If any scenario fails, the release candidate is not accepted, regardless of unit test counts.

---

## 7. Risks and mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Host stack overflow before E4005 | Process crash | Dedicated 32 MiB stack thread; depth check; 1001-call test |
| REPL state divergence after runtime error | Wrong later results | Snapshot/rollback of global scope and environment; test C-20 |
| Unicode column off-by-one | Wrong diagnostics | `Rune`-based `SourceText`; Arabic/emoji location tests |
| Windows newline translation | Golden test failures | Explicit `\n`; raw-byte CLI tests |
| UTF-8 BOM or encoding inconsistency | Mojibake, wrong E5005 | Strict decoder; BOM test; redirected UTF-8 test |
| Diagnostic path mismatch | Brittle tests | Display path as given; tests pass a controlled relative path |
| Parser ambiguity around `else` or assignment | Wrong AST | Recursive descent; dangling-else test; E2011 test |
| Number formatting drift | Golden output differences | Single `ValueFormatter`; exact tests for `-0`, scientific, fractions |
| Scope creep | Delay, inconsistency | Frozen v0.1; new features require an approved decision record |
| Test claims without execution | False confidence | `TEST-EVIDENCE.md`; no "done" without command output |
| CLI color/encoding surprises | Non-deterministic snapshots | No ANSI in v0.1; UTF-8 and LF tests |

---

## 8. Definition of done for v0.1

A feature is done only when all of the following are true:

1. Its behavior is specified in both language documents.
2. It has at least one unit test and, where observable, one `.msf` conformance case.
3. The test has actually been run and its command/output recorded.
4. It introduces no warning under `-warnaserror`.
5. It does not change behavior outside the approved spec.
6. Its diagnostic code and location are tested if it can fail.
7. Documentation examples use the implemented behavior and are executed during M8.

The release is done when all M0–M9 exit criteria are met, the full suite is green with zero
skipped tests, and the manual acceptance scenarios pass.

---

## 9. Branding and integration track (separate from the engine)

This track does not block the engine, and the engine does not block it. It has its own gates.

| Phase | Work | Gate |
|---|---|---|
| B0 | Three logo concepts + preview + rationale | **Gate B: owner selects a concept** |
| B1 | Final logo (color/dark/light/mono), file icon detailed + simplified, PNG sizes, ICO, preview page | Visual and technical review |
| B2 | VS Code extension: `.msf` language id, TextMate grammar, language configuration, icon theme, light/dark | Install and test in VS Code |
| B3 | Windows installer: per-user install, optional `.msf` association, ProgID, uninstall cleanup, VS Code open | **Gate D: explicit approval before any registry write** |
| B4 | `BRAND_GUIDELINES.md`, asset manifest, integration test notes | Final review |

VS Code icon-theme activation is explicit: a contributed icon theme is not automatically shown.
The extension will document selecting **File Icon Theme -> MSF Lang**, and may offer a command that
updates `workbench.iconTheme` only after the user invokes it. The extension is declarative for
highlighting and launches the real `msf` executable for execution; it never interprets MSF Lang
in JavaScript or TypeScript.

Windows integration will be implemented in the installer, not by ad-hoc registry scripts during
development. No registry changes are made in this phase.

---

## 10. Approval requested

Approve:

- [ ] The architecture and project layout.
- [ ] The resolver-by-binding-ID design.
- [ ] The 32 MiB dedicated-stack strategy for the 1,000-call limit.
- [ ] The test layers, fixture format, and minimum counts.
- [ ] The M0–M9 milestones and exit criteria.
- [ ] The rule that no feature is "done" without recorded test evidence.
- [ ] The separation of the engine and branding tracks.

After approval, implementation starts at M1. No engine code is written before Gate A.
