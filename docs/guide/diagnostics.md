# Diagnostics & Errors

MSF Lang is designed to produce clear, actionable diagnostics with stable error codes.

## Diagnostic Format

Errors are printed to **stderr** in a structured format:

```text
error[E4002]: division by zero
 --> examples/div-zero.msf:3:11
  |
3 | print(10 / 0);
  |           ^
  = hint: check that the divisor is not zero
```

A diagnostic typically includes:
- **Code** (stable identifier, e.g. `E3005`, `E4004`)
- **Message** (human-readable description)
- **Location** (`path:line:column`, 1-based)
- **Source excerpt** with a caret (`^`)
- **Hint** (optional)

## Error Categories

| Range | Phase | Typical Exit Code |
|---|---|---|
| `E1xxx` | Lexical (tokenization) | `1` |
| `E2xxx` | Syntax (parsing/grammar) | `1` |
| `E3xxx` | Semantic (names, scope, declarations, static checks) | `1` |
| `E4xxx` | Runtime (execution/type/operation failures) | `2` |
| `E5xxx` | CLI / I/O / usage | `3` or `4` |

## Common Examples

### Undefined name (`E3001`)
```msf
print(total);
```
Use a declared variable/function name, or declare it before use.

### Cannot assign to immutable binding (`E3005`)
```msf
let x = 1;
x = 2;
```
Use `var` if reassignment is intended: `var x = 1;`

### Type mismatch (`E4004`)
```msf
print(1 + "2");
```
MSF Lang does not perform implicit conversions. Convert logic or use matching types.

### Non-boolean condition (`E4001`)
```msf
if (1) { print("x"); }
```
Conditions must be `boolean`, e.g. `if (1 == 1) { ... }`

### Division/modulo by zero (`E4002`, `E4003`)
Check the divisor before performing `/` or `%`.

## Tips

- Fix the **first reported error** first (the language currently reports errors fail-fast)
- Use the caret position and excerpt to locate the exact token/expression
- Refer to the [specification](../en/SPEC-v0.1.md) for complete, normative rules and error definitions
