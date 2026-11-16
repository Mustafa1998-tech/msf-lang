# MSF Lang v0.1 — Language Specification

**Codename:** Seed
**Spec version:** `0.1.0-draft.1`
**Date:** 2026-11-16
**Status:** Draft for approval (Gate A approved); reference implementation in progress with recorded evidence (see [`../TEST-EVIDENCE.md`](../TEST-EVIDENCE.md)). This document remains the normative language contract for v0.1.
**Normative:** This document defines MSF Lang v0.1. The Arabic translation is kept in sync; if the
two disagree, this English document is normative until the discrepancy is corrected in both.

The key words **MUST**, **MUST NOT**, **SHOULD**, and **MAY** are used as described in RFC 2119.

---

## 1. Purpose and scope

MSF Lang is a small, general-purpose, dynamically typed programming language designed to be:

- **Implementable from scratch.** A hand-written lexer, recursive-descent parser, semantic resolver,
  and tree-walk interpreter are sufficient.
- **Predictable.** Evaluation order, scoping, numeric behavior, and error locations are specified.
- **Diagnosable.** Every language error carries a stable code, a 1-based line and column, and a
  source excerpt.
- **Testable.** Every feature in this document has a black-box test that can be written as a `.msf`
  program plus expected output or expected diagnostic.
- **Extensible without breaking v0.1 programs.** Deferred features are listed explicitly rather
  than added silently.

Version 0.1 is intentionally small. It is a complete language kernel, not a standard library or a
production runtime.

### 1.1 In scope for v0.1

| Area | Included |
|---|---|
| Values | finite `number`, `string`, `boolean`, `nil` |
| Bindings | `let` (immutable binding), `var` (mutable binding), block scoping |
| Arithmetic | `+ - * / %`, unary `-` |
| Strings | double-quoted literals, five escapes, `+` concatenation |
| Logic | `and`, `or`, `not`, short-circuit evaluation |
| Comparison | `== != < <= > >=` |
| Control flow | `if`, `else if`, `else` |
| Functions | top-level named functions, exact arity, recursion, `return` |
| Output | builtin `print` |
| Tooling | `msf run`, `msf repl`, `--help`, `--version` |
| Diagnostics | stable codes, line/column, source excerpt, documented exit codes |

### 1.2 Explicitly out of scope for v0.1

Loops, arrays, maps, user-defined types, closures, first-class functions, lambdas, modules, imports,
exceptions, input, file I/O, networking, a standard library beyond `print`, script arguments,
type annotations, Unicode escapes, exponent literals, multi-error recovery, stack traces, colored
diagnostics, an LSP, a formatter, a debugger, bytecode, a VM, and a JIT. See
[`docs/DECISIONS.md`](DECISIONS.md) for the full deferred-feature register.

---

## 2. Design principles

1. **One obvious way to write a statement.** Braces and semicolons are required; newlines are
   insignificant. There is no automatic semicolon insertion.
2. **No implicit conversions.** `"a" + 1` is an error, not `"a1"`. Only `print` converts values to
   text for display.
3. **No truthiness.** Conditions and logical operators require `boolean` values. `if (1)` is an
   error.
4. **Immutable by default at the binding level.** `let` is the normal declaration; `var` is the
   explicit opt-in to reassignment.
5. **Errors fail fast.** v0.1 reports the first error in the first failing phase. It does not
   attempt recovery or report cascades.
6. **Determinism.** v0.1 has no clock, randomness, input, environment access, or concurrency, so
   identical source and arguments always produce identical output.
7. **Locations are part of the contract.** Every token and AST node carries a span. A diagnostic's
   line and column are never guessed from a formatted message.

---

## 3. Source text and lexical structure

### 3.1 Encoding

- Source files MUST be UTF-8.
- A UTF-8 BOM (`EF BB BF`) at the start of the file MUST be accepted and ignored.
- A U+FEFF character anywhere else is an unexpected character (`E1001`).
- Invalid UTF-8 is an I/O error (`E5005`, exit code 4), not a language error.
- Source text is a sequence of Unicode scalar values. Implementations MUST NOT split a surrogate
  pair when reporting a column.

### 3.2 Line endings and positions

- `LF` (U+000A), `CRLF` (U+000D U+000A), and bare `CR` (U+000D) each count as one line break.
- Lines are numbered from **1**.
- Columns are numbered from **1** and count Unicode scalar values, not bytes or UTF-16 code units.
- A tab counts as **one** column. When rendering a caret under a tab, the renderer preserves tabs
  in the prefix so terminal tab stops stay aligned.
- A token's span is `[start, end)`. The start position is the first character of the token. The end
  position is one column after the last character.
- The EOF token is located immediately after the last source character.

### 3.3 Whitespace and comments

Whitespace is any sequence of:

- space `U+0020`
- horizontal tab `U+0009`
- line feed `U+000A`
- carriage return `U+000D`

No other Unicode whitespace is accepted; for example, U+00A0 is `E1001`.

Comments are whitespace:

- `//` starts a line comment that ends before the next line break or at EOF.
- `/*` starts a block comment that ends at the next `*/`.
- Block comments do **not** nest. `/* a /* b */ c` ends at the first `*/`; `c` is then source code.
- An unterminated block comment is `E1005` at the opening `/*`.

### 3.4 Identifiers

```ebnf
identifier = identifier-start, { identifier-part } ;
identifier-start = "A".."Z" | "a".."z" | "_" ;
identifier-part  = identifier-start | "0".."9" ;
```

Identifiers are ASCII-only and case-sensitive. Unicode identifiers are deferred. `total`, `Total`,
and `_total2` are distinct identifiers.

### 3.5 Keywords

The following words are reserved and MUST NOT be used as identifiers:

```text
let  var  fn  if  else  return  true  false  nil  and  or  not
```

`print` is **not** a keyword. It is a reserved builtin name with special declaration rules
(section 10). All keywords are lowercase and case-sensitive.

### 3.6 Numeric literals

```ebnf
number = digit, { digit }, [ ".", digit, { digit } ] ;
digit  = "0".."9" ;
```

- `42`, `0`, and `3.14` are valid.
- `.5`, `5.`, `1e3`, `0x10`, `1_000`, and `+5` are not numeric literals. `.5` and `5.` fail
  lexically/syntactically; exponent, hexadecimal, separators, and leading signs are deferred.
- The value is parsed as an IEEE-754 binary64 (`double`) using round-to-nearest.
- A literal that does not produce a finite value is `E1002`.
- Negative numbers are formed with unary `-`, not part of the literal.

### 3.7 String literals

```ebnf
string  = '"', { string-char | escape }, '"' ;
escape  = "\\", ( "n" | "t" | "r" | "\\" | '"' ) ;
```

- Strings are double-quoted and cannot contain a raw line break. A raw line break before the
  closing quote is `E1003` at the opening quote.
- The supported escapes are `\n` (LF), `\t` (tab), `\r` (CR), `\\` (backslash), and `\"` (quote).
- Any other escape, including `\0`, `\x41`, and `\u{1F600}`, is `E1004` at the backslash.
- Strings are immutable values. Their internal representation is an implementation detail, but
  equality is defined in section 5.4.
- A string may contain any other Unicode scalar value, including Arabic text and emoji.

### 3.8 Punctuation and operators

```text
(  )  {  }  ,  ;
+  -  *  /  %
==  !=  <  <=  >  >=
=
```

Only the tokens above are valid in v0.1, plus the comment introducers `//` and `/*`. The dot `.`
is valid only inside a numeric literal. The characters `!` (alone), `?`, `&`, `|`, `[`, `]`, `:`,
`@`, `#`, `$`, `^`, `~`, and backslash outside a string are unexpected (`E1001`).

### 3.9 Lexical errors

| Code | Condition | Location |
|---|---|---|
| `E1001` | Character not valid at this point | The character |
| `E1002` | Numeric literal is not finite | The literal |
| `E1003` | Unterminated string literal | The opening `"` |
| `E1004` | Invalid escape sequence | The backslash |
| `E1005` | Unterminated block comment | The opening `/*` |

The lexer stops at the first lexical error and emits no token stream.

---

## 4. Grammar

The grammar is LL(1) with small lookahead for assignment detection. `EOF` is the end-of-file token.
Curly braces `{ ... }` in the EBNF mean repetition; square brackets `[ ... ]` mean optional.

```ebnf
program        = { declaration }, EOF ;

declaration    = function-decl
               | variable-decl
               | statement ;

function-decl  = "fn", identifier, "(", [ parameters ], ")", block ;

parameters     = identifier, { ",", identifier } ;

variable-decl  = ( "let" | "var" ), identifier, "=", expression, ";" ;

statement      = if-stmt
               | return-stmt
               | block
               | assignment-stmt
               | expression-stmt ;

if-stmt        = "if", "(", expression, ")", block,
                 [ "else", ( if-stmt | block ) ] ;

return-stmt    = "return", [ expression ], ";" ;

block          = "{", { declaration }, "}" ;

assignment-stmt = identifier, "=", expression, ";" ;

expression-stmt = expression, ";" ;

expression     = logic-or ;

logic-or       = logic-and, { "or", logic-and } ;

logic-and      = equality, { "and", equality } ;

equality       = comparison, { ( "==" | "!=" ), comparison } ;

comparison     = term, { ( "<" | "<=" | ">" | ">=" ), term } ;

term           = factor, { ( "+" | "-" ), factor } ;

factor         = unary, { ( "*" | "/" | "%" ), unary } ;

unary          = ( "-" | "not" ), unary
               | call-or-primary ;

call-or-primary = identifier, [ "(", [ arguments ], ")" ]
                | primary ;

arguments      = expression, { ",", expression } ;

primary        = number
               | string
               | "true"
               | "false"
               | "nil"
               | identifier
               | "(", expression, ")" ;
```

### 4.1 Grammar notes

- There are no empty statements. A lone `;` is a syntax error.
- There are no trailing commas in parameter lists or argument lists.
- Semicolons are required after every variable declaration, `return`, assignment, and expression
  statement. They are not used after blocks or function declarations.
- A block may contain declarations and statements, including a nested `fn`. Nested functions parse
  successfully but are rejected by the resolver (`E3007`).
- A call syntactically requires an identifier immediately before `(`. `(f)(1)` and `f()()` are
  syntax errors, because functions are not first-class values in v0.1.
- `else` binds to the nearest unmatched `if`. `else if` is parsed as an `if-stmt` in the `else`
  branch.
- `return` with no expression returns `nil`. A line break after `return` does not terminate it;
  newlines are insignificant.
- `not` is a keyword, not a function. `not not true` is valid.
- Unary operators are right-associative. All binary operators are left-associative.
- A non-name expression followed by `=` is reported as `E2011` (invalid assignment target) rather
  than a generic semicolon error.

### 4.2 Operator precedence

From highest to lowest:

| Level | Operators | Associativity | Meaning |
|---|---|---|---|
| 1 | `f(...)` | — | call (identifier only) |
| 2 | `-` (unary), `not` | right | numeric negation, logical negation |
| 3 | `*` `/` `%` | left | multiplication, division, remainder |
| 4 | `+` `-` | left | addition, subtraction, string concatenation |
| 5 | `<` `<=` `>` `>=` | left | numeric comparison |
| 6 | `==` `!=` | left | equality |
| 7 | `and` | left | short-circuit conjunction |
| 8 | `or` | left | short-circuit disjunction |

Parentheses override precedence. Comparisons do not chain: `1 < 2 < 3` parses as
`(1 < 2) < 3` and fails at runtime with `E4004`, because the left operand is a boolean.

### 4.3 Nesting limit

To prevent a malformed source file from crashing the host process, the parser MUST enforce a
maximum nesting depth of **256** for expressions, blocks, and `if`/`else` chains. The limit is
counted from the outermost construct. Exceeding it is `E2013` at the opening token that would
exceed the limit. A valid program with 256 nested parentheses or 256 nested blocks is accepted;
257 is rejected. This is a parser safety limit, not a language feature.

### 4.4 Grammar-level constraints

The following are syntax errors, not semantic errors:

- Missing `)`, `}`, `;`, `{`, `=` where required.
- A function declaration without `(` or a body.
- `if` without parentheses.
- `else` without a preceding `if` in the same statement.
- An expression statement with a trailing operator, e.g. `1 + ;`.
- A call with an empty argument between commas, e.g. `f(1, , 2)`.
- `return` used as an expression, e.g. `let x = return 1;`.
- Assignment used as an expression, e.g. `let y = (x = 1);`.
- A keyword used where an identifier is required.

---

## 5. Values and types

### 5.1 Value kinds

| Type name | Description | Examples |
|---|---|---|
| `number` | Finite IEEE-754 binary64 | `0`, `-12`, `3.5` |
| `string` | Immutable Unicode text | `"MSF"`, `"مرحبا"` |
| `boolean` | Logical truth value | `true`, `false` |
| `nil` | The single absent-value value | `nil` |
| `function` | Internal callable, not a language value | a declared `fn` |
| `builtin` | Internal predefined callable | `print` |

`function` and `builtin` are **not** first-class values in v0.1. A name bound to one of them may
only appear as the callee of a call. Using it in a value position is `E3010`.

### 5.2 Numbers

- All numbers are `double` values.
- NaN and infinities are not valid MSF Lang values. A numeric literal that is not finite is
  `E1002`; an arithmetic result that is not finite is `E4006`.
- `0` and `-0` compare equal and both display as `0`. The language does not otherwise distinguish
  signed zero.
- There are no integer and floating-point subtypes.

### 5.3 Strings

- Strings are immutable sequences of Unicode scalar values.
- Equality is ordinal and compares the exact sequence of scalar values.
- Concatenation with `+` allocates a new string. Both operands MUST be strings.
- There are no string indexing, length, slicing, interpolation, or methods in v0.1.

### 5.4 Equality

`==` and `!=` are defined for every pair of language values:

1. If the two operands have different type names, `==` is `false` and `!=` is `true`.
2. If both are `number`, compare IEEE-754 numeric equality.
3. If both are `string`, compare ordinal scalar sequences.
4. If both are `boolean`, compare truth values.
5. If both are `nil`, `==` is `true`.

Equality never raises a runtime error. Functions and builtins never appear as operands because they
are not values.

### 5.5 Display formatting

`print` and the REPL use the following value-to-text rules:

| Value | Text |
|---|---|
| number | Round-trip, invariant-culture representation; `-0` displays as `0`; integral values display without a trailing `.0` |
| string | The string contents, without quotes |
| boolean | `true` or `false` |
| nil | `nil` |

For numbers, implementations MUST produce the shortest string that round-trips to the same
`double`, using `.` as the decimal separator and lowercase `e` for scientific notation without a
`+` sign or leading zeros in the exponent (for example `1e20`, `1.5e-7`). The exact .NET format
used by the reference implementation is documented in the implementation plan.

---

## 6. Names, bindings, and scopes

### 6.1 Scopes

MSF Lang uses lexical, block-structured scoping:

- The **global scope** contains the builtin `print` and all top-level declarations.
- Every block `{ ... }` creates a child scope, including the body of an `if` branch, a standalone
  block, and a function body.
- A function body's scope contains its parameters and its local declarations. Parameters and
  locals share one scope.
- Name lookup walks from the current scope outward to the global scope. There is no dynamic
  scoping and no access to a caller's local variables.
- A declaration is visible only after its initializer has been evaluated. This means an inner
  declaration can shadow an outer binding in its own initializer:

  ```msf
  let x = 1;
  {
      let x = x + 1;  // the outer x is read; the new x is visible after this line
      print(x);      // 2
  }
  print(x);          // 1
  ```

  If no outer `x` exists, `let x = x;` is `E3001` on the initializer's `x`.

### 6.2 Declarations

- `let name = expr;` creates an immutable binding. The binding always refers to the same value;
  the value itself is immutable for all v0.1 types.
- `var name = expr;` creates a mutable binding. It may be reassigned to a value of any type.
- A declaration's initializer is mandatory.
- A name may be declared only once in a given scope (`E3002`).
- Shadowing a name from an outer scope is allowed.
- `print` is a reserved builtin name and cannot be declared in any scope (`E3004`).
- Top-level variables and functions share one namespace, so a variable and a function cannot have
  the same name in the global scope.
- A local variable may shadow a global function. Calls then resolve to the variable and are
  rejected with `E3009`.

### 6.3 Assignment

Assignment is a statement, not an expression:

```msf
var count = 0;
count = count + 1;
```

- The left side MUST be a single identifier.
- The name MUST resolve to an existing binding (`E3001`).
- Assigning to a `let` binding is `E3005`.
- Assigning to a function or the builtin `print` is `E3006`.
- The right side is evaluated, then the binding is updated.
- Assigning to a parameter is allowed; parameters are mutable local bindings.

### 6.4 Global visibility inside functions

Function declarations are **not hoisted** in v0.1:

- A function may be called only after its `fn` declaration has been processed in textual order.
- A function body may reference a global variable or function declared **before** the function
  declaration. It may not reference one declared later, even if the call happens later. Such a
  reference is `E3001`.
- A function body may call itself, enabling direct recursion.
- Mutual recursion is not possible in v0.1 because the second function cannot see the first unless
  the first is declared earlier, and the first cannot see the second. This is deliberate; see the
  deferred feature register.

Example:

```msf
let base = 10;

fn addBase(n) {
    return n + base;   // valid: base is declared before addBase
}

fn later() {
    return helper();   // E3001: helper is declared after later
}

fn helper() {
    return 1;
}

print(addBase(5));     // 15
```

### 6.5 Uninitialized and undefined names

- There are no uninitialized local variables: every declaration has an initializer.
- A name that has not been declared at the point of reference is `E3001`.
- Because functions are not hoisted, a call before its declaration is `E3001`, not a runtime error.
- There is no `undefined` value and no implicit `nil` initialization.

---

## 7. Expressions

### 7.1 Evaluation order

- Binary operands are evaluated left to right.
- Call arguments are evaluated left to right, then the call occurs.
- `and` and `or` evaluate the left operand first and short-circuit; the right operand is not
  evaluated when the result is already determined.
- Unary operands are evaluated before the operator.
- Assignment evaluates its right side, then stores.
- There is no unspecified evaluation order.

### 7.2 Arithmetic

Let `T` be the runtime type of an operand.

| Expression | Required types | Result | Error on mismatch |
|---|---|---|---|
| `a + b` | both `number` or both `string` | sum or concatenation | `E4004` |
| `a - b` | both `number` | difference | `E4004` |
| `a * b` | both `number` | product | `E4004` |
| `a / b` | both `number` | quotient | `E4004`; `E4002` if `b == 0` |
| `a % b` | both `number` | remainder | `E4004`; `E4003` if `b == 0` |
| `-a` | `number` | negation | `E4004` |

- `/` is floating-point division. It is not integer division.
- `%` is the truncated remainder: `a - trunc(a / b) * b`. The result has the sign of the
  dividend. Examples: `7 % 3 == 1`, `-7 % 3 == -1`, `7 % -3 == 1`.
- A zero divisor is checked before computing the result, including `0 / 0` and `0 % 0`.
- Every arithmetic result MUST be finite. If a result is NaN or infinite, the runtime raises
  `E4006` at the operator's location.
- `+` never coerces. `"1" + 2` is `E4004`, not `"12"` or `3`.

### 7.3 Comparison

- `<`, `<=`, `>`, `>=` accept two `number` operands and return `boolean`.
- Any other operand types are `E4004`.
- String ordering and boolean ordering are deferred.

### 7.4 Equality

`==` and `!=` are defined by section 5.4 and never raise an error.

### 7.5 Logical operators

- `and`, `or`, and `not` require `boolean` operands.
- `a and b`: evaluate `a`; if `a` is not boolean, `E4004`; if `false`, return `false` without
  evaluating `b`; otherwise evaluate `b`, require boolean, and return it.
- `a or b`: evaluate `a`; if `a` is not boolean, `E4004`; if `true`, return `true` without
  evaluating `b`; otherwise evaluate `b`, require boolean, and return it.
- `not a`: evaluate `a`, require boolean, return its negation.
- `and` and `or` are not value-returning selectors: they always return a boolean, and they never
  return either operand unchanged.

### 7.6 Conditions

An `if` condition MUST evaluate to a `boolean`. Any other type raises `E4001` at the condition's
location. There is no truthiness.

### 7.7 Calls

- A call target is an identifier. It MUST resolve to a user function or the builtin `print`.
- A call to a variable is `E3009`.
- A call to a user function MUST supply exactly the declared number of arguments (`E3008`).
- `print` accepts zero or more arguments.
- Arguments are evaluated left to right before the callee runs.
- A user function returns the value supplied to `return`; falling off the end returns `nil`.
- Recursion is allowed. The maximum number of active user-function calls is **1000**. Attempting
  call 1001 raises `E4005` at the call site.
- Calls to `print` do not count toward the user-function depth limit.

---

## 8. Statements and control flow

### 8.1 Expression statements

`expr;` evaluates the expression and discards its value in `msf run`. In the REPL, a non-`nil`
value is displayed (section 14).

### 8.2 Variable declarations

See section 6.2. A declaration is a statement; it does not produce a value.

### 8.3 Assignment statements

See section 6.3.

### 8.4 Blocks

`{ ... }` executes its declarations in order in a new scope. A standalone block is allowed and is
useful for limiting a binding's lifetime:

```msf
{
    let temporary = 99;
    print(temporary);
}
// temporary is not visible here
```

### 8.5 `if` statements

```msf
if (condition) {
    // then branch
} else if (otherCondition) {
    // else-if branch
} else {
    // else branch
}
```

- The condition MUST be boolean.
- Exactly one branch executes.
- Each branch block has its own scope.
- An `if` statement without `else` is valid.
- `else if` is syntactic sugar for an `if` statement in the `else` branch; it is not a separate
  construct.

### 8.6 `return` statements

- `return expr;` returns `expr` from the innermost user function.
- `return;` returns `nil`.
- A `return` outside any function is `E3011`.
- A `return` in a nested block inside a function returns from the function, not just the block.

### 8.7 No other statements

v0.1 has no loops, `break`, `continue`, `throw`, `try`, `switch`, `match`, or `defer`. Using those
words as identifiers is allowed unless they become keywords in a future version; see section 15.

---

## 9. Functions

### 9.1 Declaration

```msf
fn name(p1, p2) {
    // body
}
```

- `fn` declarations are allowed only at the top level of a program (`E3007` otherwise). They may
  appear anywhere among top-level declarations, but they take effect at their textual position.
- Parameter names MUST be unique within one function (`E3003`).
- A function and a variable cannot share a name in the same scope (`E3002`).
- A function cannot be named `print` (`E3004`).
- A function declaration has no trailing semicolon.
- A function may have zero parameters.

### 9.2 Parameters and locals

- Parameters are mutable local bindings.
- Parameters and the function body's top-level locals share one function scope. A local declaration
  with the same name as a parameter is `E3002`.
- Inner blocks in the body may shadow parameters and locals normally.
- A function body cannot access the caller's locals (no dynamic scoping) and captures no
  environment (no closures). Because only top-level functions exist, its only outer scope is the
  global scope.

### 9.3 Calls and arity

- User functions have fixed arity. The resolver checks every direct call against the declaration
  (`E3008`).
- There are no default parameters, variadic user functions, overloads, named arguments, or
  optional arguments.
- `print` is the only variadic callable.

### 9.4 Recursion

Direct recursion is supported:

```msf
fn factorial(n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}
```

The function's own name is visible inside its body. If a local binding shadows the function name,
calls through that name become `E3009`. v0.1 has no aliases or first-class functions, so inside
that scope the shadowed function is unreachable. This is a consequence of lexical scoping, not a
special rule.

### 9.5 Return values

- Falling off the end of a function returns `nil`.
- `return;` returns `nil`.
- A function call used as an expression statement is allowed; its return value is discarded in
  `run` mode.

---

## 10. Builtin `print`

`print` is a predefined global builtin:

- Name: `print`
- Arity: zero or more arguments
- Return value: `nil`
- Side effect: writes the formatted arguments to standard output, separated by one space, followed
  by one `\n`, and flushes.

Formatting follows section 5.5. `print()` writes a single newline. `print("a", 1, true)` writes
`a 1 true\n`. Strings are not quoted. `print` is not a keyword and cannot be shadowed or
redeclared. Using `print` as a value rather than a callee is `E3010`.

`print` is the only builtin in v0.1. No other names are predefined.

---

## 11. Program execution

### 11.1 Pipeline

A run consists of these phases, in order:

1. Read the file as strict UTF-8.
2. Lex the entire file. On the first lexical error, report and stop.
3. Parse the token stream into an AST. On the first syntax error, report and stop.
4. Resolve names, scopes, mutability, `return` placement, function placement, and direct-call
   arity. On the first semantic error, report and stop.
5. Execute the program's declarations sequentially in the global scope.
6. Flush standard output and exit.

A lexical, syntax, or semantic error prevents all execution. A runtime error stops execution at
the failing statement; output already written remains written.

### 11.2 Entry point

- There is no required `main` function.
- Top-level statements execute in textual order.
- An empty program is valid and produces no output.
- Top-level `fn` declarations create their bindings when reached; they do not execute their bodies.
- A program may end with or without a trailing newline.

### 11.3 Determinism and side effects

The only observable side effect in v0.1 is writing to standard output through `print`. There is no
input, clock, randomness, file access, network access, environment access, or concurrency.

---

## 12. Errors and diagnostics

### 12.1 Fail-fast rule

v0.1 reports exactly one diagnostic per invocation, from the first failing phase. It does not
attempt to recover or report multiple errors. The REPL reports one diagnostic per input entry and
continues.

### 12.2 Diagnostic fields

Every language diagnostic has:

- a stable code (`E1001`–`E4006`),
- a phase-derived severity (`error`),
- a human-readable message,
- a source path (`<repl>` in the REPL),
- a 1-based line and column,
- an optional hint.

### 12.3 Text format

Diagnostics are written to standard error, without ANSI color, in this shape:

```text
error[E4002]: division by zero
 --> examples/div-zero.msf:7:14
  |
7 |     print(10 / 0);
  |              ^
  = hint: check that the divisor is not zero
```

Rules:

- The first line is exactly `error[<code>]: <message>`.
- The location line starts with one space, then `--> `, then the display path, `:`, line, `:`,
  column.
- The excerpt gutter width grows with the line number.
- The caret line contains `^` repeated for the span length, with a minimum of one caret.
- Tabs in the source line are preserved in the caret prefix so caret alignment is correct in a
  terminal.
- A hint line is optional and begins with `  = hint: `.
- The display path is the path as supplied on the command line, without conversion to an absolute
  path. The REPL uses `<repl>`.
- The source excerpt is the single line containing the diagnostic start. Multi-line spans are not
  underlined in v0.1.
- Implementations MAY truncate very long source lines to 120 display columns around the span; the
  line and column in the header remain authoritative. Truncation is not required by conformance
  tests.
- An implementation MUST NOT include a stack trace in a normal diagnostic. Stack traces are
  deferred.

### 12.4 Diagnostic catalog

#### Lexical

| Code | Message pattern |
|---|---|
| `E1001` | `unexpected character '<char>'` |
| `E1002` | `numeric literal is not finite: '<text>'` |
| `E1003` | `unterminated string literal` |
| `E1004` | `invalid escape sequence '\<char>'` |
| `E1005` | `unterminated block comment` |

#### Syntax

| Code | Message pattern |
|---|---|
| `E2001` | `expected expression, found '<token>'` |
| `E2002` | `expected ';', found '<token>'` |
| `E2003` | `expected ')', found '<token>'` |
| `E2004` | `expected '{', found '<token>'` |
| `E2005` | `expected '}', found '<token>'` |
| `E2006` | `expected identifier, found '<token>'` |
| `E2007` | `expected '(', found '<token>'` |
| `E2008` | `expected '=', found '<token>'` |
| `E2009` | `unexpected token '<token>'` |
| `E2010` | `unexpected end of input` |
| `E2011` | `invalid assignment target` |
| `E2012` | `expected function body` |
| `E2013` | `maximum nesting depth (256) exceeded` |

When a parser expects a specific token and finds EOF, it MUST use `E2010` (not the specific
expected-token code). This is also how the REPL detects that an entry is incomplete.

#### Semantic

| Code | Message pattern |
|---|---|
| `E3001` | `undefined name '<name>'` |
| `E3002` | `duplicate declaration '<name>' in this scope` |
| `E3003` | `duplicate parameter '<name>'` |
| `E3004` | `builtin 'print' cannot be redeclared` |
| `E3005` | `cannot assign to immutable '<name>'; declare it with 'var'` |
| `E3006` | `cannot assign to '<name>'; functions and builtins are not assignable` |
| `E3007` | `function declarations are only allowed at top level` |
| `E3008` | `function '<name>' expects <n> argument(s), got <m>` |
| `E3009` | `cannot call '<name>'; it is a variable, not a function` |
| `E3010` | `'<name>' can only be used in a call` |
| `E3011` | `'return' is only allowed inside a function` |

#### Runtime

| Code | Message pattern |
|---|---|
| `E4001` | `condition must be a boolean, got <type>` |
| `E4002` | `division by zero` |
| `E4003` | `modulo by zero` |
| `E4004` | `invalid operand types for '<op>': <type> and <type>` (binary); `invalid operand type for '<op>': <type>` (unary) |
| `E4005` | `maximum call depth (1000) exceeded in '<name>'` |
| `E4006` | `numeric result is not finite` |

#### CLI and I/O

| Code | Message pattern | Exit |
|---|---|---|
| `E5001` | `unknown command '<name>'` | 3 |
| `E5002` | `invalid usage: <detail>` | 3 |
| `E5003` | `file not found: <path>` | 4 |
| `E5004` | `cannot read file: <path>` | 4 |
| `E5005` | `file is not valid UTF-8: <path>` | 4 |

An unexpected internal exception is not a language diagnostic. The CLI MUST print
`internal error: <message>` to stderr and exit 70. If the environment variable `MSF_DEBUG` is set
to `1`, it MAY also print a stack trace. Internal errors are bugs and are covered by tests as
process failures, not as language conformance.

### 12.5 Exit codes

| Exit | Meaning |
|---|---|
| `0` | Success |
| `1` | Lexical, syntax, or semantic (compile-time) error |
| `2` | Runtime error |
| `3` | CLI usage error |
| `4` | I/O error, including missing files, unreadable files, and invalid UTF-8 |
| `70` | Unexpected internal error |

A REPL session exits `0` on `:quit` or EOF, even if earlier entries had diagnostics. A REPL startup
I/O failure exits `4`.

---

## 13. CLI contract

The executable is `msf` (on Windows, `msf.exe`). The v0.1 command surface is:

```text
msf run <path>
msf repl
msf --help | -h
msf --version | -v
```

### 13.1 `msf run <path>`

- Reads `<path>` as strict UTF-8, accepts an optional BOM, and executes it.
- Accepts any file name; the `.msf` extension is recommended but not enforced.
- The path may be relative or absolute and may contain spaces. Quoting is the shell's job.
- Extra arguments after the path are a usage error (`E5002`, exit 3). Script arguments are
  deferred.
- On success, exits 0 after flushing stdout.
- On a language error, prints one diagnostic to stderr and exits 1 or 2.
- On an I/O error, prints an `E500x` message to stderr and exits 4.
- Output written before a runtime error remains on stdout.
- The command does not print the source path, timing, or a success banner.

Example:

```text
> msf run examples/hello.msf
Hello, MSF!
> echo $LASTEXITCODE
0
```

### 13.2 `msf repl`

See section 14.

### 13.3 `msf --help`

Prints usage, the command list, the v0.1 scope summary, and a link to the documentation to stdout;
exits 0. Help output is not a conformance surface; the commands and exit codes are.

### 13.4 `msf --version`

Prints exactly `msf 0.1.0` followed by `\n`; exits 0. This string is a conformance surface.

### 13.5 Argument errors

- `msf` with no arguments: print usage to stderr and exit 3.
- Unknown command or option: `E5001`/`E5002` to stderr, exit 3.
- `msf run` with zero or more than one path: `E5002`, exit 3.
- `msf repl` with extra arguments: `E5002`, exit 3.
- Commands and options are case-sensitive.
- `--` is not a separator in v0.1.

### 13.6 Streams and encoding

- Normal program output goes to stdout.
- Diagnostics and usage errors go to stderr.
- Output uses `\n` line endings on every platform, including Windows.
- When stdout is redirected, it is UTF-8 without a BOM.
- Number and value formatting is culture-invariant and independent of the operating system locale.
- No ANSI color is emitted in v0.1.

---

## 14. REPL contract

### 14.1 Session model

- The REPL keeps one persistent global scope and one persistent runtime environment across
  entries.
- Declarations persist: a `let`, `var`, or `fn` entered in one entry is visible in later entries.
- Each entry is lexed, parsed, resolved, and executed as a complete program fragment.
- A compile-time error prints a diagnostic and leaves all state unchanged.
- A runtime error prints a diagnostic and **rolls back all declarations and assignments made by
  that entry** to the state before the entry. Output already printed remains printed. This keeps
  the semantic symbol table and runtime environment consistent.
- `:quit`, `:help`, and EOF are handled by the REPL, not the language.

### 14.2 Input and prompts

- Interactive mode is used when standard input is a terminal (`Console.IsInputRedirected` is
  false in the reference implementation).
- Interactive mode prints a short banner and the prompt `msf> ` to stdout.
- If the parser or lexer reports an incomplete entry (an error whose offending position is EOF,
  including an unterminated string or block comment), the REPL prints the continuation prompt
  `...> ` and reads more lines.
- A blank line while a continuation is pending terminates the entry and reports the pending
  incomplete-input diagnostic. The session then resets to `msf> `.
- A complete entry is executed as soon as its final line is entered.
- Commands `:quit` and `:help` are recognized only at the start of a fresh entry, after trimming
  surrounding whitespace. They are not language identifiers.
- `:quit` exits 0. EOF exits 0.
- An unknown command beginning with `:` prints a CLI-style message and returns to the prompt; it
  does not exit.
- Ctrl+C behavior is terminal-dependent and is not part of the v0.1 conformance contract.

### 14.3 Non-interactive mode

When stdin is redirected:

- No banner and no prompts are printed.
- Input is read line by line. Each line starts a new entry; if the entry is incomplete, subsequent
  lines are appended until it is complete or EOF is reached.
- Expression-statement results are still auto-displayed.
- The process exits 0 unless startup I/O fails.

This mode exists so the REPL can be tested by piping a script to `msf repl`.

### 14.4 Auto-display

In the REPL only, each top-level expression statement whose value is not `nil` is formatted using
section 5.5 and printed to stdout, followed by `\n`. This is an output convenience, not a language
feature. Declarations, assignments, `if` statements, and function declarations never auto-display.
`print(...)` returns `nil`, so it does not auto-display a second value.

Example (interactive or piped input):

```text
let x = 5;
x * 2;
print("done");
```

Output:

```text
10
done
```

The first entry produces no output; the second produces `10\n`; the third produces `done\n`.

---

## 15. Versioning and compatibility

- MSF Lang source files use the `.msf` extension by convention; the CLI accepts any path.
- The language version is `0.1.0`. The CLI version string is exactly `msf 0.1.0`.
- v0.1 is a pre-1.0 language. Later minor versions MAY add features, but MUST NOT silently change
  the meaning of a v0.1 program that conforms to this document. Breaking changes require a minor
  version bump and a migration note in the changelog.
- New keywords are a compatibility risk. v0.1 does not reserve words for deferred features such
  as `while`, `for`, `break`, `continue`, `class`, `import`, or `match`. A future version that
  introduces them as keywords will document the break and offer migration guidance.
- The spec version `0.1.0-draft.1` is not the same as an approved release. This document must be
  approved before implementation begins.

---

## 16. Conformance

An implementation conforms to MSF Lang v0.1 if it satisfies every normative statement in this
document and passes the conformance suite defined in
[`docs/en/IMPLEMENTATION-PLAN.md`](IMPLEMENTATION-PLAN.md).

Conformance is defined by observable behavior:

- stdout bytes (after normalizing platform line endings to `\n`),
- stderr diagnostics (code, line, column; full formatting where specified),
- exit code,
- REPL state across entries.

Implementation details such as AST shape, memory layout, and error message wording beyond the
documented patterns are not conformance surfaces. Diagnostic **codes** and **locations** are.

No conformance run has been performed yet. The test suite will be created in Phase 2 and every
claim of passing tests will be backed by an actual command and output.

---

## 17. Complete examples

### 17.1 Hello, MSF

```msf
print("Hello, MSF!");
```

Output:

```text
Hello, MSF!
```

### 17.2 Variables and types

```msf
let name = "Mustafa";
var visits = 1;
visits = visits + 1;

print("Hello,", name);
print("Visits:", visits);
print("Same?", 1 == 1, 1 == "1", nil == nil);
```

Output:

```text
Hello, Mustafa
Visits: 2
Same? true false true
```

### 17.3 Arithmetic and precedence

```msf
print(1 + 2 * 3);
print((1 + 2) * 3);
print(-7 % 3);
print(7.5 / 2);
```

Output:

```text
7
9
-1
3.75
```

### 17.4 Conditionals

```msf
fn classify(n) {
    if (n < 0) {
        return "negative";
    } else if (n == 0) {
        return "zero";
    } else {
        return "positive";
    }
}

print(classify(-4), classify(0), classify(9));
```

Output:

```text
negative zero positive
```

### 17.5 Recursion

```msf
fn factorial(n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}

fn fibonacci(n) {
    if (n < 2) {
        return n;
    }
    return fibonacci(n - 1) + fibonacci(n - 2);
}

print(factorial(10));
print(fibonacci(10));
```

Output:

```text
3628800
55
```

### 17.6 Short-circuit logic

```msf
print(false and (1 / 0 == 0));
print(true or (1 / 0 == 0));
print(not false);
```

Output:

```text
false
true
true
```

The division is never evaluated, so no `E4002` occurs.

### 17.7 Error: immutable assignment

```msf
let x = 10;
x = 20;
```

Expected diagnostic (shape):

```text
error[E3005]: cannot assign to immutable 'x'; declare it with 'var'
 --> immutable.msf:2:1
  |
2 | x = 20;
  | ^
  = hint: change the declaration to 'var x = 10;' if reassignment is intended
```

Exit code: `1`. Nothing is executed because resolution fails before execution.

### 17.8 Error: runtime type mismatch

```msf
print(1 + "2");
```

Expected diagnostic (shape):

```text
error[E4004]: invalid operand types for '+': number and string
 --> type-error.msf:1:7
  |
1 | print(1 + "2");
  |       ^
```

Exit code: `2`.

### 17.9 Error: undefined name

```msf
print(total);
```

Expected diagnostic (shape):

```text
error[E3001]: undefined name 'total'
 --> undefined.msf:1:7
  |
1 | print(total);
  |       ^^^^^
```

Exit code: `1`.

### 17.10 REPL transcript

```text
msf> let x = 5;
msf> x * 2;
10
msf> fn square(n) {
...>     return n * n;
...> }
msf> square(x);
25
msf> :quit
```

The continuation prompt is shown after the first line of the function because the parser reaches
EOF while expecting `}` (`E2010`).

---

## 18. Specification checklist for approval

The specification is ready to implement when the project owner confirms:

- [ ] The v0.1 feature set and the deferred-feature register are correct.
- [ ] `let` / `var` semantics and block scoping are approved.
- [ ] Top-level-only functions, no hoisting, direct recursion, and no mutual recursion are accepted.
- [ ] Strict boolean conditions and no truthiness are accepted.
- [ ] `print` as a reserved variadic builtin is accepted.
- [ ] The grammar, especially semicolons, braces, and identifier-only calls, is approved.
- [ ] Diagnostic codes, format, and exit codes are approved.
- [ ] The CLI and REPL contracts, including REPL rollback and auto-display, are approved.
- [ ] The implementation plan, milestones, and test matrix are approved.
- [ ] Branding concept selection is handled as a separate Gate B.

Once these are approved, implementation begins at Milestone 1 in the implementation plan. No engine
code will be written before this checklist is accepted.
