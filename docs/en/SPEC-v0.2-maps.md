# MSF Lang v0.2 — Maps Addendum (Draft)

**Status:** Draft addendum to [`SPEC-v0.2-practical.md`](SPEC-v0.2-practical.md). English normative.

## Goals

Add a minimal deterministic string-keyed map for practical records and lookup tables without JSON/module/runtime evaluation.

## Syntax

```ebnf
map-literal = "map" , "{" , [ string , ":" , expression , { "," , string , ":" , expression } ] , "}" ;
```

`map` is a reserved keyword. `set`, `get`, `has`, and `remove` are reserved builtin names. `:` is valid only as map-literal separator in v0.2.

## Semantics

- Type name: `map`.
- Keys MUST be strings; string keys are compared ordinally.
- Values may be heterogeneous v0.1 values (`number`, `string`, `boolean`, `nil`) or `list`.
- Elements are evaluated left to right before the map value is created.
- `map == map` is identity equality only.
- `print`/REPL display quotes keys and nested strings (`{"a": 1}`) and is not stable ordering-sensitive; do not rely on print order.

## Builtins

### `get(map, key)`

- Returns the value for `key`, or `nil` if missing.

### `set(map, key, value)`

- Sets `value` at `key` in-place.
- Returns `nil`.

### `has(map, key)`

- Returns `true`/`false`.

### `remove(map, key)`

- Removes `key`; returns removed value or `nil`.

### `len(mapOrListOrString)`

- `len(map)` returns insertion count.

## Diagnostics

| Code | Meaning | Exit |
|---|---|---|
| `E4009` | map/key argument mismatch | `2` |

## Known limits

- Keys are strings only.
- No iteration/ordering semantics.
- No JSON parsing/generation.
- No nested map literal syntax inside a list is restricted; maps may contain lists/maps.

## Example

```msf
var scores = map { "ada": 90, "sam": 80 };
set(scores, "maya", 100);
print(get(scores, "ada"));
print(has(scores, "maya"));
print(len(scores)); // 3
```
