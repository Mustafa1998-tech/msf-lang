# Syntax & Language Basics (v0.1)

This page is a practical syntax guide for MSF Lang v0.1. When in doubt, the [language specification](../en/SPEC-v0.1.md) is normative.

## Source Files

- Extension: `.msf` (recommended)
- Encoding: **UTF-8** (optional BOM at file start is accepted)
- Statements end with `;`
- Blocks use `{ ... }`
- Comments:
  - Line: `// comment`
  - Block: `/* comment */` (not nested)

```msf
// line comment
let x = 1; /* block comment */
```

## Values and Types

| Type | Examples | Notes |
|---|---|---|
| `number` | `0`, `42`, `3.14`, `-5` | Finite IEEE-754 double |
| `string` | `"MSF"`, `"مرحبا"` | Double quotes, UTF-8 text |
| `boolean` | `true`, `false` | Used in conditions and logic |
| `nil` | `nil` | Absence of a value |

No implicit type conversions. For example, `"1" + 2` is a runtime type error.

## Variables

```msf
let name = "Mustafa"; // immutable binding
var count = 1;        // mutable binding
count = count + 1;
```

- `let` creates an immutable binding (cannot be reassigned)
- `var` creates a mutable binding (can be reassigned)
- Variables are block-scoped
- Initializer is required

## Expressions and Operators

### Arithmetic
```msf
1 + 2 * 3      // 7
(1 + 2) * 3    // 9
7 / 2          // 3.5
7 % 3          // 1
-7 % 3         // -1
```

### Strings
```msf
"MSF" + " " + "Lang"
```
`+` concatenates strings only when **both** operands are strings.

### Comparison and Equality
```msf
3 < 5          // true
3 == 3         // true
3 == "3"       // false (different types)
nil == nil     // true
```

`==` / `!=` compare values by type and value (no implicit coercion).

### Logic (strict booleans)
```msf
true and false
true or false
not true
```
- Operands must be `boolean`
- `and` / `or` short-circuit
- No truthiness: `if (1)` is invalid

## Control Flow

### `if` / `else if` / `else`
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

print(classify(9));
```

- Condition must evaluate to `boolean`
- Each branch is a block with its own scope

## Functions

```msf
fn add(a, b) {
    return a + b;
}

print(add(2, 3));
```

- Declared with `fn` at **top level** (v0.1)
- Fixed arity (argument count must match)
- `return expr;` or `return;` (returns `nil`)
- Falling off the end returns `nil`
- Direct recursion is supported

```msf
fn factorial(n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}
```

## Built-in `print`

```msf
print();                 // blank line
print("Hello", "MSF"); // Hello MSF
print(1, true, nil);    // 1 true nil
```

- Accepts zero or more arguments
- Prints values separated by spaces, followed by a newline
- Returns `nil`
- `print` is a reserved built-in name

## Not Available in v0.1

The following are **not** part of v0.1:
- `while` / `for` loops
- Arrays, maps, objects/classes
- Closures, lambdas, first-class functions
- Modules/imports/packages
- Exceptions (`try/catch`)
- User input, file/network I/O
- Standard library beyond `print`

See [Architecture Decisions](../DECISIONS.md) for deferred features.
