# MSF Lang v0.2 — Math Core Addendum (Draft)

**Status:** Draft companion to [`SPEC-v0.2-practical.md`](SPEC-v0.2-practical.md). English normative.

## Builtins

MSF Lang v0.2 adds deterministic numeric builtins:

| Name | Arity | Meaning |
|---|---|---|
| `abs(x)` | 1 | absolute value |
| `sqrt(x)` | 1 | square root; `x >= 0` required |
| `floor(x)` | 1 | floor |
| `ceil(x)` | 1 | ceiling |
| `round(x)` | 1 | round, ties away from zero |
| `pow(x, y)` | 2 | `x^y`; non-finite results raise `E4006` |
| `min(a, b)` | 2 | minimum |
| `max(a, b)` | 2 | maximum |

All arguments must be `number`. Non-number arguments raise `E4009`. `pow` non-finite results raise `E4006`.

```msf
print(pow(2, 8)); // 256
print(round(2.5)); // 3
```
