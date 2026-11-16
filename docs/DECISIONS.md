# MSF Lang — Architecture Decisions & Deferred Features

**Status:** Gate A approved (implementation in progress). These records are treated as binding for v0.1 implementation unless superseded by a new ADR. Branding decisions become binding at Gate B; Windows integration at Gate D.
**Date:** 2026-11-16
**Spec:** [`docs/en/SPEC-v0.1.md`](en/SPEC-v0.1.md) / [`docs/ar/SPEC-v0.1.md`](ar/SPEC-v0.1.md)

This log is bilingual. Each decision records the context, the choice, and the consequences. It is
append-only: a decision is superseded by a new record, never silently edited.

---

## ADR index

| ID | Decision | Status |
|---|---|---|
| ADR-0001 | Hand-written lexer and recursive-descent parser | Proposed |
| ADR-0002 | Tree-walk interpreter; no bytecode in v0.1 | Proposed |
| ADR-0003 | Dynamic typing; four language value kinds; finite doubles | Proposed |
| ADR-0004 | Braces, mandatory semicolons, ASCII identifiers, UTF-8 | Proposed |
| ADR-0005 | `let` immutable, `var` mutable, block scoping | Proposed |
| ADR-0006 | Top-level functions only; no hoisting, closures, or first-class functions | Proposed |
| ADR-0007 | `print` is a reserved variadic builtin, not a keyword | Proposed |
| ADR-0008 | Strict boolean conditions; no truthiness | Proposed |
| ADR-0009 | Fail-fast diagnostics with stable codes and Unicode-scalar columns | Proposed |
| ADR-0010 | Exit-code scheme 0/1/2/3/4/70 | Proposed |
| ADR-0011 | Resolver assigns numeric binding IDs; no runtime name lookup | Proposed |
| ADR-0012 | Persistent REPL with transactional rollback | Proposed |
| ADR-0013 | 1,000-call limit on a 32 MiB dedicated stack | Proposed |
| ADR-0014 | Conformance fixtures are `.msf` + `.expected.json` | Proposed |
| ADR-0015 | C#/.NET 10 only; no third-party runtime dependencies | Proposed |
| ADR-0016 | Brand concept approval before final assets; no registry writes without Gate D | Proposed |
| ADR-0018 | v0.2 "Practical Programming" draft: loops, lists, len/append | Proposed (implemented as draft) |

---

## ADR-0001 — Hand-written lexer and recursive-descent parser

**Context.** The language is small, the grammar is LL(1), and exact source locations matter.
**Decision.** Implement the lexer and parser by hand. Do not use ANTLR, Yacc, parser combinators,
or generated tables in v0.1.
**Consequences.**

- Full control over spans, error messages, and recovery behavior.
- No generator dependency or generated code to review.
- The grammar and implementation must be kept in sync manually; parser tests and the EBNF file are
  the guard.
- Adding a substantially more complex grammar later may justify a parser generator, but that is
  not a v0.1 concern.

**Arabic.** يُنفَّذ المعجم والمحلل النحوي يدويًا لأن القواعد صغيرة و LL(1) ولأن دقة المواضع
ذات أهمية. لا تُستخدم مولّدات أو مكتبات تحليل في 0.1. النتيجة تحكم كامل في المدىات والأخطاء،
مقابل ضرورة مزامنة القواعد مع التنفيذ يدويًا.

---

## ADR-0002 — Tree-walk interpreter; no bytecode in v0.1

**Context.** The required v0.1 deliverable is a tree-walk interpreter. Performance is not a
release criterion.
**Decision.** Execute the resolved AST directly. Do not design a bytecode format, VM, JIT, or
caching layer in v0.1.
**Consequences.**

- The implementation stays inspectable and matches the specification.
- Recursion depth and performance are limited compared with a VM; the spec caps user calls at
  1,000.
- A future bytecode backend can reuse the lexer, parser, and resolver.

**Arabic.** يُنفَّذ الإصدار 0.1 بمفسّر شجري مباشر. لا بايت كود ولا آلة افتراضية ولا JIT. هذا
يبقي التنفيذ قابلًا للفحص ويطابق المواصفات، مع حدود في الأداء والعمق يمكن معالجتها لاحقًا.

---

## ADR-0003 — Dynamic typing; four language value kinds; finite doubles

**Context.** v0.1 needs variables, arithmetic, logic, and functions without a type system.
**Decision.** Values are `number`, `string`, `boolean`, and `nil`. Functions and builtins are
internal callables, not first-class values. Numbers are finite IEEE-754 binary64. There are no
implicit conversions.
**Consequences.**

- Simple interpreter and clear runtime errors.
- No static type checking; type errors surface at runtime with `E4001`/`E4004`.
- Numeric overflow is an error (`E4006`) rather than an infinity value.
- A static type system, integers, decimals, and user types are deferred.

**Arabic.** الأنواع ديناميكية وبأربع قيم لغوية فقط، والأعداد `double` منتهية. لا تحويلات
ضمنية. النتيجة بساطة التنفيذ وأخطاء أنواع وقت التشغيل، مقابل غياب الفحص الثابت للأنواع.

---

## ADR-0004 — Braces, mandatory semicolons, ASCII identifiers, UTF-8

**Context.** Newline-sensitive syntax complicates parsing, error messages, and REPL continuation.
**Decision.** Blocks use braces; statements end with semicolons; newlines are insignificant.
Identifiers are ASCII-only. Source files are UTF-8; strings and comments may contain any Unicode.
**Consequences.**

- No automatic semicolon insertion and no newline ambiguities.
- The REPL needs continuation detection for incomplete braces/parentheses, not for line endings.
- Arabic identifiers and localized keywords are deferred; Arabic text remains fully usable in
  strings, comments, output, and documentation.
- `msf run` accepts any path, but `.msf` is the convention.

**Arabic.** الأقواس والفواصل المنقوطة إلزامية وفواصل الأسطر بلا معنى، والمعرّفات ASCII فقط.
الملفات UTF-8 ويجوز استخدام العربية في النصوص والتعليقات والمخرجات. هذا يبسّط التحليل
والتشخيص ويؤجل المعرّفات العربية.

---

## ADR-0005 — `let` immutable, `var` mutable, block scoping

**Context.** The language needs variables without encouraging accidental mutation.
**Decision.** `let` creates a single-assignment binding; `var` permits reassignment. Both are
block-scoped. The initializer is resolved before the new binding is visible.
**Consequences.**

- Immutability is the default at the binding level.
- `let x = x + 1;` can read an outer `x`; without an outer binding it is `E3001`.
- Shadowing is allowed in child scopes.
- Assignment is a statement, not an expression.

**Arabic.** `let` ارتباط ثابت و`var` قابل للإسناد، وكلاهما داخل نطاق الكتلة. يُحلّ المُهيّئ
قبل ظهور الارتباط الجديد. الجمود هو الأصل، والحجب في النطاقات الفرعية مسموح.

---

## ADR-0006 — Top-level functions only; no hoisting, closures, or first-class functions

**Context.** Functions are required; closures and higher-order functions are not. Hoisting and
mutual recursion add resolver and runtime complexity.
**Decision.** Functions are declared only at top level. They are processed in textual order and
are not hoisted. A function body sees globals declared before it and itself. Direct recursion is
supported; mutual recursion is not. There are no closures, lambdas, function values, or aliases.
**Consequences.**

- Simple lexical scoping and a small runtime.
- Mutual recursion and forward references require a future language version or a redesign.
- A function name used as a value is `E3010`; calling a variable is `E3009`.
- A local binding may shadow a function, making it unreachable in that scope.

**Arabic.** الدوال على المستوى الأعلى فقط، وتُعالج بترتيب النص ولا تُرفع. يرى جسم الدالة
العوامات السابقة ونفسه، فيعمل الاستدعاء الذاتي المباشر. لا إغلاقات ولا دوال كقيم ولا استدعاء
متبادل في 0.1.

---

## ADR-0007 — `print` is a reserved variadic builtin, not a keyword

**Context.** Output is required for examples and tests. A keyword statement would complicate
expression composition; a normal global could be shadowed.
**Decision.** `print` is a predefined variadic builtin with zero or more arguments and a `nil`
return value. The name cannot be declared in any scope. It is an identifier at the lexer level and
is resolved specially.
**Consequences.**

- `print` works in expression statements and inside expressions.
- The name is unavailable to user programs, including locals.
- Additional builtins in future versions need a documented reservation policy.

**Arabic.** `print` دالة مدمجة محجوزة متغيرة الوسائط، وليست كلمة محجوزة. لا يجوز إعلانها في
أي نطاق، وتعيد `nil`. هذا يسهّل استخدامها داخل التعبيرات ويمنع حجبها.

---

## ADR-0008 — Strict boolean conditions; no truthiness

**Context.** Truthiness rules vary across languages and hide type mistakes.
**Decision.** `if` conditions, `and`, `or`, and `not` require `boolean` operands. Values are
never implicitly converted to booleans.
**Consequences.**

- `if (1)` is `E4001`; `1 and true` is `E4004`.
- Logical operators are short-circuit and always return a boolean.
- Programs must write explicit comparisons, which is intentional.

**Arabic.** الشروط والمعاملات المنطقية تتطلب `boolean` صراحةً، ولا يوجد مفهوم الصدق. `if (1)`
خطأ. هذا يكشف أخطاء الأنواع مبكرًا في وقت التشغيل ويجعل السلوك واضحًا.

---

## ADR-0009 — Fail-fast diagnostics with stable codes and Unicode-scalar columns

**Context.** v0.1 needs clear errors, not a full diagnostic engine. Columns must be correct for
Arabic and emoji.
**Decision.** Each phase reports the first error and stops. Codes are stable (`E1xxx`–`E5xxx`).
Locations are 1-based; columns count Unicode scalar values; a tab counts as one column; each of
LF, CRLF, and CR is one line break. Diagnostics include a source excerpt and caret, without ANSI
color in v0.1.
**Consequences.**

- One diagnostic per invocation; multi-error recovery is deferred.
- Tests assert code and location, not exact prose.
- The formatter must preserve tabs for caret alignment.
- The REPL reports one diagnostic per entry and continues.

**Arabic.** يبلّغ كل طور عن أول خطأ ويتوقف. الرموز ثابتة، والمواضع تبدأ من 1 وتُحسب بقيم
Unicode القياسية، والجدولة عمود واحد. التشخيص يحتوي مقتطفًا ومؤشرًا بلا ألوان في 0.1.

---

## ADR-0010 — Exit-code scheme 0/1/2/3/4/70

**Context.** Scripts and integration tests need to distinguish compile errors, runtime errors,
usage errors, and I/O failures.
**Decision.** `0` success; `1` lexical/syntax/semantic error; `2` runtime error; `3` CLI usage
error; `4` I/O error (including invalid UTF-8); `70` unexpected internal error.
**Consequences.**

- Tests can assert the failure class without parsing stderr.
- The scheme is part of the CLI contract and is stable for v0.x.
- Internal errors are bugs, not conformance failures.

**Arabic.** رموز الخروج تميّز النجاح (0) وخطأ الترجمة (1) وخطأ التشغيل (2) وخطأ الاستخدام (3)
وخطأ الإدخال/الإخراج (4) والخطأ الداخلي (70). هذا يجعل الاختبارات والسكربتات أدق.

---

## ADR-0011 — Resolver assigns numeric binding IDs; no runtime name lookup

**Context.** Static name resolution is needed for undefined-name, mutability, return, and arity
errors. Runtime string lookup would duplicate scope logic.
**Decision.** The resolver walks scopes in textual order, assigns each binding a unique numeric
ID, and annotates every name-bearing AST node with its binding. The interpreter stores values in
ID-keyed environments and never looks up names by string.
**Consequences.**

- One source of truth for scoping and shadowing.
- Fast runtime lookup and no typo resurfacing at runtime.
- The REPL must persist binding IDs and scope state across entries.
- Debug tooling may still map IDs back to names.

**Arabic.** المحلل الدلالي يمنح كل ارتباط معرّفًا رقميًا ويشرح العقد، ويستخدم المفسّر بيئات
مفاتيحها المعرّفات لا الأسماء. هذا يوحّد منطق النطاقات ويمنع أخطاء الأسماء وقت التشغيل.

---

## ADR-0012 — Persistent REPL with transactional rollback

**Context.** A REPL must keep declarations across entries, but a runtime error must not leave the
semantic scope and runtime environment inconsistent.
**Decision.** The REPL persists one global scope and environment. Each entry is resolved and
executed transactionally; on a runtime error, all declarations and assignments from that entry
are rolled back. Printed output remains. Compile errors never commit changes. Expression
statements auto-display non-`nil` values. Redirected input suppresses banner and prompts.
**Consequences.**

- State stays coherent after errors.
- Rollback requires snapshots or a staged scope/environment.
- Auto-display is REPL-only and not a language feature.
- Piped REPL tests are deterministic.

**Arabic.** يحتفظ REPL بنطاق وبيئة عامين، ويعامل كل مدخلة كمعاملة: عند خطأ تشغيل يتراجع عن
إعلاناتها وإسناداتها مع بقاء المخرجات المطبوعة. هذا يمنع انحراف الحالة بين جدول الرموز وبيئة
التشغيل. العرض التلقائي في REPL فقط.

---

## ADR-0013 — 1,000-call limit on a 32 MiB dedicated stack

**Context.** A tree-walk interpreter can exhaust the host stack before reaching a language-level
limit.
**Decision.** The language limit is 1,000 active user calls (`E4005`). The CLI and test host run
the engine on a dedicated thread with a 32 MiB stack and check depth before binding arguments.
**Consequences.**

- Deep recursion fails deterministically instead of crashing the process.
- The limit is observable and testable at 1,000/1,001 calls.
- Host memory use increases slightly.
- Builtin `print` calls do not count toward the limit.

**Arabic.** حد الاستدعاء 1000، ويُشغّل المحرك على خيط بمكدس 32 MiB مع فحص العمق قبل ربط
الوسائط. النتيجة فشل حتمي بـ E4005 بدل انهيار العملية.

---

## ADR-0014 — Conformance fixtures are `.msf` + `.expected.json`

**Context.** Language tests should be programs written in the language, not only C# unit tests.
**Decision.** Each conformance program is a `.msf` file with a same-named `.expected.json` sidecar
containing exit code, exact stdout, and optional diagnostic code/line/column/phase. The harness
fails on orphans in either direction.
**Consequences.**

- Tests are readable and double as examples.
- Exact output and error locations are checked.
- Full diagnostic prose is tested separately in CLI tests.
- At least 60 conformance programs are required for v0.1.

**Arabic.** كل اختبار مطابقة برنامج `.msf` حقيقي مع ملف `.expected.json` يحوي رمز الخروج
والمخرجات الدقيقة والتشخيص المتوقع. هذا يجعل الاختبارات أمثلة قابلة للقراءة ويمنع ادعاء
النجاح بلا مخرجات.

---

## ADR-0015 — C#/.NET 10 only; no third-party runtime dependencies

**Context.** The engine must be built in C# and .NET 10 without using another language to
interpret MSF Lang.
**Decision.** Core and CLI target `net10.0` and use only the BCL. Tests may use xUnit and the
.NET test SDK. No scripting engine, dynamic language runtime, parser generator, or interpreter
library is used.
**Consequences.**

- Deployment is a normal .NET application.
- VS Code integration is declarative for highlighting and launches the real `msf` executable; it
  never interprets MSF Lang in JavaScript/TypeScript.
- Third-party test tooling is allowed but not part of the runtime.

**Arabic.** المحرك وCLI على .NET 10 وبمكتبات BCL فقط. لا تُستخدم لغة أخرى لتفسير MSF Lang.
إضافة VS Code تعريفية للإبراز وتشغّل `msf` الحقيقي، ولا تفسّر اللغة في JavaScript.

---

## ADR-0016 — Brand concept approval before final assets; no registry writes without Gate D

**Context.** The brand must be original and usable, and Windows file associations are
security-sensitive.
**Decision.** Present three logo concepts (Gate B) before producing final assets. Do not create a
final icon set or VS Code icon theme until a concept is selected. Do not write to the Windows
registry, change file associations, or run an installer during development. Windows integration
is implemented in an installer and requires explicit Gate D approval.
**Consequences.**

- No premature final assets or "almost final" files.
- The VS Code icon theme must be explicitly selected by the user; contributing it does not make
  it active automatically.
- The installer must register a ProgID, make association optional, respect the user's default
  app, and remove only its own keys on uninstall.
- Registry testing requires a disposable VM or export/restore plan.

**Arabic.** تُعرض ثلاثة مفاهيم للشعار قبل الأصول النهائية. لا تُكتب أي مفاتيح في سجل Windows
ولا تُغيَّر ارتباطات الملفات أثناء التطوير. يتم التكامل عبر مثبّت وبعد موافقة صريحة (Gate D)،
مع احترام التطبيق الافتراضي للمستخدم وتنظيف مفاتيح المثبّت فقط عند الإزالة.

---

## ADR-0017 — Arabic + English documentation; English normative on conflict

**Context.** The project owner works in Arabic; the language aims at an international audience.
**Decision.** Maintain full Arabic and English specifications, plans, examples, and brand
guidelines. English is normative if the two texts disagree; discrepancies must be fixed in both.
**Consequences.**

- More maintenance, but no second-class documentation.
- Code and keywords remain ASCII/English in v0.1; Arabic is fully supported in strings, comments,
  output, and docs.
- Localized keywords are deferred.

**Arabic.** تُصان وثائق عربية وإنجليزية كاملة، والإنجليزية مرجعية عند التعارض. الكود والكلمات
المحجوزة إنجليزية في 0.1، والعربية مدعومة كاملة في النصوص والتعليقات والمخرجات والتوثيق.

---

## ADR-0018 — v0.2 "Practical Programming" draft: loops, lists, `len`, `append`

**Context.** v0.1 is intentionally minimal; common scripts need iteration and dynamic collections. A full v0.2 feature set (maps, modules, stdlib) is too large for one stable increment.
**Decision.** MSF Lang introduces an explicitly labeled **v0.2 draft** with `while`, `break`/`continue`, dynamic heterogeneous `list` literals/indexing/in-place element assignment, and builtins `len` (list or string) and `append` (mutating a list). List equality is identity reference equality. List display quotes nested strings for unambiguous `print`/REPL output.
**Consequences.**

- Everyday iteration no longer depends solely on recursion.
- List operations are deterministic and avoid hidden conversions.
- v0.2 reserves names (`while`, `break`, `continue`, `len`, `append`) that were legal identifiers in v0.1; this is a documented compatibility risk.
- Indexing failures use new runtime diagnostics `E4007`; builtin argument mismatch uses `E4008`; `break`/`continue` outside loops uses `E3013`/`E3014`.
- v0.2 maps, modules, stdlib APIs, and file I/O remain deferred; they require separate ADRs and conformance coverage.

**Arabic.** تُقدّم لغة 0.2 مسودة عملية تتضمن `while` و`break/continue` وقوائم ديناميكية وفهرسة وإسناد العناصر و`len`/`append`. المساواة بين القوائم بمرجعها فقط. لا تُضاف خريطة/وحدات/مكتبة قياسية في هذه المسودة.

---

## ADR-0019 — v0.2 practical draft: maps and map builtins

**Context.** Practical scripts need string-keyed records; dynamic JSON-like data remains deferred.
**Decision.** Add a reserved `map` literal `map { "k": expr, ... }` with string keys, a `map` runtime value with identity equality, and builtins `get/set/has/remove`. `len` accepts maps. Map value display quotes string keys/values for unambiguous output.
**Consequences.**

- Useful records/simple lookup tables become possible in MSF Lang programs.
- Keys are strings only; arbitrary key types and JSON interop are deferred.
- Map iteration order is intentionally unspecified; tests must not depend on print order.
- `get` returns `nil` for missing keys; no exceptions for absence.
- New diagnostic `E4009` for map builtin misuse and invalid keys.
- JSON, user-defined records, file I/O, and stdlib collection APIs remain deferred.

---

## ADR-0020 — v0.2 string core builtins

**Context.** Practical text processing requires deterministic minimal string helpers before a full stdlib.
**Decision.** Add `upper`, `lower`, `trim`, `contains`, `startsWith`, `endsWith`, and `split` as builtins (ordinal string comparisons). Empty `split` delimiter is rejected.
**Consequences.**

- Common text normalization and splitting problems become expressible.
- Full regex, slicing/indexing conventions, and Unicode case-folding nuance remain deferred.
- Display format for strings inside lists quotes them for unambiguity.

---

## Deferred features register

Nothing in this table is part of v0.1. Adding any item requires a new ADR, a specification
update, and tests. Target versions are indicative, not promises.

| Feature | Reason deferred | Revisit |
|---|---|---|
| `while`, `for`, `break`, `continue` | Not required for v0.1; recursion covers iteration | v0.2 candidate |
| Arrays/lists, maps, tuples | Larger runtime and type design | v0.2 candidate |
| User-defined types, classes, enums, records | Not in v0.1 scope | post-v0.2 |
| Closures, lambdas, first-class functions | Requires environment capture and callable values | v0.2 candidate |
| Function hoisting, mutual recursion, forward references | Resolver/runtime complexity; explicit v0.1 limitation | v0.2 candidate |
| Modules, imports, packages | Needs file resolution and a module system | post-v0.2 |
| Exceptions, `try`/`catch` | Error model is fail-fast only | post-v0.2 |
| Standard library beyond `print` (math, strings, collections) | Scope control; needs API design | v0.2+ |
| User input, files, network, environment, time, randomness | Determinism and safety | post-v0.2 |
| Type annotations / static types | Would change the language model | post-v0.2 |
| Unicode identifiers, Arabic keywords, localized syntax | Lexer and compatibility decisions | future |
| Numeric exponents, hex/binary/underscore literals, integers/decimals | Grammar and numeric model | v0.2 candidate |
| Unicode escapes, string interpolation, raw/multiline strings | Lexer scope | v0.2 candidate |
| String ordering (`<` on strings) | Semantics not needed in v0.1 | v0.2 candidate |
| Multiple diagnostics / parser recovery | Fail-fast is sufficient for v0.1 | v0.2 candidate |
| Stack traces in runtime errors | Diagnostic scope | v0.2 candidate |
| ANSI-colored diagnostics | Deterministic output and scope | v0.2 candidate |
| Script arguments, shebang, `--` separator | CLI scope | v0.2 candidate |
| REPL history, completion, line editing, Ctrl+C semantics | Terminal-specific UX | v0.2+ |
| `msf check`, `msf ast`, `msf fmt`, LSP, debugger | Tooling beyond v0.1 | post-v0.1 |
| Bytecode, VM, JIT, incremental execution | Performance not a v0.1 goal | future |
| Concurrency, async | Major runtime design | future |
| Windows installer, `.msf` association, ProgID | Branding track Gate D | after Gate B |
| Final logo assets, file icons, PNG/ICO, VS Code extension | Branding track Gate B | after concept selection |
| Package manager / registry | Ecosystem scope | future |

---

## Open decisions for Gate A approval

The owner should explicitly confirm or change these points. Each is already reflected in the
specification and plan.

| # | Question | Current proposal |
|---|---|---|
| 1 | Immutable `let` by default and mutable `var`? | Yes, block-scoped |
| 2 | Top-level functions only, no hoisting, no mutual recursion? | Yes for v0.1 |
| 3 | `print` reserved and variadic, not a keyword? | Yes |
| 4 | Strict boolean conditions and no truthiness? | Yes |
| 5 | Identifier-only calls, no first-class functions? | Yes |
| 6 | Fail-fast with one diagnostic per run? | Yes |
| 7 | Exit codes 0/1/2/3/4/70? | Yes |
| 8 | REPL persistent state with runtime-error rollback? | Yes |
| 9 | 1,000-call limit, 256 nesting limit? | Yes |
| 10 | 60+ `.msf` conformance programs as a release gate? | Yes |
| 11 | Brand: three concepts, then one selected? | Awaiting Gate B |
| 12 | No Windows registry changes before Gate D? | Binding policy |

**Arabic — القرارات المفتوحة.** على مالك المشروع تأكيد أو تعديل النقاط التالية: `let` ثابت
و`var` قابل للإسناد، الدوال على المستوى الأعلى فقط بلا رفع أو استدعاء متبادل، `print` مدمجة
محجوزة، شروط منطقية صارمة، استدعاء بالمعرّفات فقط، تشخيص واحد بالفشل السريع، رموز الخروج
المقترحة، تراجع REPL، حدود العمق والتداخل، اختبارات المطابقة، اختيار الشعار، ومنع تغييرات
السجل قبل Gate D.
