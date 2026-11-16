# MSF Lang v0.2 — "Practical Programming" Practical Addendum (Draft)

**Spec version:** `0.2.0-draft.1`  
**Date:** 2026-11-16  
**Status:** Draft. This document defines additive v0.2 extensions to [`SPEC-v0.1.md`](SPEC-v0.1.md). English is normative.

> v0.2 extends v0.1 with a small set of constructs that unlock practical problem-solving: **iteration (`while`)**, **lists (dynamic arrays)**, and **two universal list/string builtins (`len`, `append`)**. Everything else from v0.1 remains unchanged unless explicitly stated here.

---

## 1. Why this exists

v0.1 is intentionally small and deterministic. v0.2 adds the minimum required to write everyday scripts and data-processing logic without abandoning determinism, simple semantics, or spec-driven testability.

### 1.1 In scope for v0.2

| Area | Included |
|---|---|
| Iteration | `while (condition) { block }` with boolean condition, plus `break;` / `continue;` loop control |
| Collections | `list` value: dynamic heterogeneous array |
| Indexing | list reads and writes (`xs[i]`, `xs[i] = value;`) |
| Builtins | `len(listOrString)`, `append(list, value)` |
| Display | `list` display in `print`/REPL auto-display |

### 1.2 Examples of problems v0.2 unlocks

- Sum/filter/map-like logic with loops (no recursion-depth required for common iteration)
- Accumulate results in a list while scanning
- String length checks for input validation patterns
- Simple report generation from finite data encoded in the program

---

## 2. `while` statement

### 2.1 Syntax

```ebnf
stmt = while-stmt
     | if-stmt
     | return-stmt
     | block
     | assignment-stmt
     | expression-stmt ;

while-stmt = "while" , "(" , expression , ")" , block ;
```

- `while` is a keyword in v0.2 (reserved).
- Condition MUST evaluate to `boolean`. Any other type is `E4001` at the condition’s location.
- The body is a block; each iteration creates a new block scope for that iteration.
- `while` does not introduce iteration variables; use explicit indices/accumulators.
- Newline after `while` does not terminate it; semicolons are not used after the block.

### 2.2 Semantics and termination

```msf
var i = 0;
var total = 0;
while (i < 10) {
    total = total + i;
    i = i + 1;
}
print(total); // 45
```

- Evaluation order: left-to-right as usual; condition is evaluated before each iteration.
- v0.2 includes loop control: `break;` exits the innermost enclosing `while`, and `continue;` skips to the next iteration.
- There is no labeled `break`/`continue` and no `else` on loops in v0.2.
- A `while` loop may not terminate; the implementation may apply host resource limits outside the language semantics. For tests, use bounded loops.

### 2.3 Diagnostics

- Condition not boolean → `E4001` at condition location.
- Parser expects `(` after `while` → `E2007`.
- Parser expects `)` after condition → `E2003`.
- Parser expects `{` for body → `E2004`.
- `break;` outside any `while` → `E3013` at the `break` keyword.
- `continue;` outside any `while` → `E3014` at the `continue` keyword.

---

## 3. Lists (`list` value)

### 3.1 Literal and indexing syntax

```ebnf
list-literal = "[" , [ expression , { "," , expression } ] , "]" ;

primary = ...
        | list-literal ;

postfix-expr = call-or-primary , { "[" , expression , "]" } ;

assignment-stmt = identifier , "[" , expression , "]" , "=" , expression , ";"
                | identifier , "=" , expression , ";" ;
```

- `[` and `]` are valid punctuation in v0.2 for list literal and indexing.
- List literal is an `expression`, allowed anywhere an expression is allowed.
- `xs[i]` yields the element at index `i`.
- `xs[i] = value;` assigns the element at index `i` (mutation in place).

### 3.2 Type and semantics

- **Type name:** `list`
- **Representation:** reference to a mutable dynamic array (heap allocated).
- **Element type:** heterogeneous; each element has its own v0.1 type.
- **Equality:** `list == list` and `list != list` compare by identity (same list object), not by element-wise equality. Comparisons with non-list values are `false`/`true` by type as usual.
- **Display/print format:** `list` prints as `[e0, e1, ...]` with elements formatted using the same scalar formatting rules used for `print` (strings **are quoted** in list display to remain unambiguous). Example:

```msf
print([1, "a", true, nil]);
// [1, "a", true, nil]
```

- **Index rules:**
  - Index MUST be a `number`. Non-number is `E4004`.
  - Index MUST be an integer (no fractional part). Non-integer is `E4004` (see §3.3).
  - Index MUST be within `[0, len(list))`. Out-of-bounds read is `E4007`; out-of-bounds write is `E4007` (see §3.4).

### 3.3 Index integer requirement

List indices are zero-based and integral.

- `xs[0]` valid; `xs[len(xs) - 1]` valid for non-empty list.
- `xs[1.5]` → `E4004` (`invalid operand type for '[]': number and number` is too vague; specify `list index must be an integer`). v0.2 introduces `E4007` for list-specific failures.

### 3.4 New runtime error codes

| Code | Meaning | Exit |
|---|---|---|
| `E4007` | list failure: index out of bounds / non-integer index / index into non-list | `2` |

Sub-cases communicated via message text:
- `list index must be an integer, got <type>`
- `list index out of bounds: <i> (len=<n>)`
- `cannot index type <type>`

### 3.5 `len` builtin

- Name: `len`
- Arity: exactly 1 argument
- Accepts: `list` or `string`
- Returns: `number` equal to element count (`list`) or Unicode scalar count (`string`)
- Other types → `E4008` (`len` argument must be list or string)

Examples:

```msf
print(len([1, 2, 3]));
print(len("abc"));
print(len("مرحبا"));
```

### 3.6 `append` builtin

- Name: `append`
- Arity: exactly 2 arguments
- First argument MUST be a `list`; second is any value
- Effect: appends second argument to the list **in place**
- Returns: `nil`
- Passing a non-list as first argument → `E4008`

Example:

```msf
var xs = [];
append(xs, 1);
append(xs, 2);
print(xs); // [1, 2]
```

### 3.7 List literal evaluation

Elements are evaluated left to right before the list value is created.

---

## 4. Grammar deltas (summary)

```ebnf
stmt = while-stmt | if-stmt | return-stmt | block | assignment-stmt | expression-stmt ;

while-stmt = "while" , "(" , expression , ")" , block ;

factor = ... (unchanged) ;
call-or-primary = identifier , [ "(" , [ arguments ] , ")" ]
                | primary , { "[" , expression , "]" } ;
```

`assignment-stmt` is extended to support list element assignment as defined in §3.1.

---

## 5. Diagnostic/CLI deltas

- New `E4007` (list failure) and `E4008` (`len`/`append` argument mismatch) are runtime diagnostics (`2`).
- No changes to existing exit codes.
- New builtins `len` and `append` behave like `print`: variadic/builtin dispatch, not first-class values, `E3010` if used as value.
- New identifiers reserved: `while` (keyword), `len`, `append` (reserved builtin names).

---

## 6. Determinism and limits

- `list` operations are deterministic.
- `len`/`append` add no randomness or IO.
- There is no guarantee of termination for `while`; bounded tests are required for conformance.
- Memory use is implementation-defined; `append` to an unbounded list may exhaust memory and terminate the process (outside language semantics). For tests, use bounded workloads.

---

## 7. Compatibility and rollout

- v0.1 programs remain valid v0.2 programs (except: v0.1 use of the names `while`, `len`, `append` as identifiers is now invalid; those names are reserved).
- This is an additive minor version; no v0.1 behavior is redefined.
- v0.2 does **not** add closures, maps, exceptions, modules, or file IO. Those remain deferred.

---

## 8. Acceptance criteria (minimum; see implementation plan for full matrix)

- All v0.1 conformance/unit tests remain passing.
- v0.2 additions covered by:
  - Core tests: `while`, list literal, indexing read/write, `len` (list/string), `append`, and error cases
  - Conformance: new positive/negative `.msf`/`.expected.json` fixtures
- CLI/REPL behavior: `while`/lists work under `msf run` and in REPL
- No regressions in diagnostics/exit codes

---

## 9. Examples

### Sum 1..10

```msf
var i = 1;
var total = 0;
while (i <= 10) {
    total = total + i;
    i = i + 1;
}
print(total); // 55
```

### Build and print a list

```msf
var xs = [];
append(xs, 10);
append(xs, 20);
append(xs, 30);
print(xs);              // [10, 20, 30]
print(len(xs));         // 3
print(xs[1]);           // 20
```

### Mutable list elements

```msf
var xs = [1, 2, 3];
xs[1] = 99;
print(xs);              // [1, 99, 3]
```
