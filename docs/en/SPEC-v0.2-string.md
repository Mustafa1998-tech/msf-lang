# MSF Lang v0.2 — String Core Addendum (Draft)

**Status:** Draft companion to [`SPEC-v0.2-practical.md`](SPEC-v0.2-practical.md). English normative.

## Builtins

| Name | Arity | Meaning |
|---|---|---|
| `upper(s)` | 1 | uppercase Unicode scalars (invariant culture) |
| `lower(s)` | 1 | lowercase |
| `trim(s)` | 1 | remove leading/trailing whitespace |
| `contains(s, sub)` | 2 | substring test (ordinal) |
| `startsWith(s, sub)` | 2 | prefix test (ordinal) |
| `endsWith(s, sub)` | 2 | suffix test (ordinal) |
| `split(s, delimiter)` | 2 | split into `list`; delimiter must not be empty |

All string arguments must be `string`; mismatch raises `E4009`. Empty delimiter raises `E4009`.

```msf
print(split("a,b,c", ",")); // ["a", "b", "c"]
print(contains("msf-lang", "lang")); // true
```
