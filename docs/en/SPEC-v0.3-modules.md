# MSF Lang v0.3 — Modules (Accepted)

**Spec version:** `0.3.0`
**Status:** Accepted per ADR-0021 (2026-11-17). Implemented by `msf 0.3.0-rc.1`. EN normative.

## Goals

- Unit organization: named, reusable, dependency-graph file modules for real scripts and libraries.
- Multi-file projects without global namespace pollution.
- Deterministic execution (same run + same files = same behavior).
- Simple rules first; packages/registries/network imports remain deferred.

## Non-goals (v0.3)

- No packages/registries/semver resolution; only local relative `.msf` file imports.
- No network/HTTP imports, manifest or dependency injection, or environment-based module switching.
- No namespace pollution without explicit qualification: imports enter the importing file's scope as module bindings; names are accessed qualified.
- No partial evaluation, macros, conditional compilation, or dynamic import.
- Qualified assignment (`math.x = 1`) is NOT supported in v0.3; module exports cannot be assigned from outside (module-exported `var` is read-only for importers).

## Syntax

```ebnf
module-decl   = "import" , string , ";" ;
export-decl   = "export" , ( function-decl | var-decl ) ;
program       = { program-decl } , EOF ;
program-decl  = import-decl | export-decl | declaration ;
member-expr   = identifier , "." , identifier ;
qualified-call-expr = identifier , "." , identifier , "(" , [ arguments ] , ")" ;
```

Examples:

```msf
// math.msf
export fn square(n) { return n * n; }
export var PI = 3.5;
var secret = 42;

// main.msf
import "./math.msf";
print(math.square(9));   // 81
print(math.PI);          // 3.5
print(square(9));        // E3001 (unqualified name not imported)
```

## Semantics

- `import "path";` loads a local module. `path` is a relative path (relative to the importing file) with extension `.msf`. Absolute paths and other extensions are rejected with `E4012`.
- The module name is the file stem; it must be a valid identifier (`[A-Za-z_][A-Za-z0-9_]*`), otherwise `E4012`. The stem must be `math` for `math.msf`.
- Imported bindings are **module bindings** visible in the importing scope. A duplicate module binding (declared variable or import with the same name) is `E3002`.
- Only names marked `export` in the module are visible on the module binding. Unexported names are module-private (`E4013`).
- **Qualified access only**: `math.square`, `math.PI`. Unqualified names are not imported.
  - `math.PI` used as a function is `E3009`; `math.square` used as a value is `E3010`.
  - `math` itself used as a value is `E3017`.
  - Wrong argument count is `E3008`.
- A module is loaded (read, lexed, parsed, resolved) at most once per process/REPL session. It executes at most once, before the importing file, in dependency order (dependencies first). Top-level module statements execute for their side effects.
- Import cycles are detected during loading and rejected with `E4014`; the diagnostic message includes the cycle path. Imports inside functions/blocks are rejected with `E3015`; `export` outside top level is rejected with `E3016`.
- Modules have isolated global scopes. They see builtins but not the importing file's bindings. Transitive imports work; a module may import other modules.
- Execution order: dependency modules execute before dependent modules, which execute before the entry program.
- REPL: `import` is allowed; imported modules stay loaded for the session; their exports remain usable afterwards. Re-importing the same path in the same scope is a duplicate declaration (`E3002`).
- Runtime errors inside a module carry the module's source path in the diagnostic.

## Diagnostics

| Code | Meaning | Exit |
|---|---|---|
| `E2014` | `import` path token is not a string | `1` |
| `E2015` | `export` not followed by `fn`/`var`/`let` | `1` |
| `E3002` | Duplicate declaration / duplicate import in same scope | `1` |
| `E3008` | Argument count mismatch | `1` |
| `E3009` | Not callable (variable/module used as function) | `1` |
| `E3010` | Function used as value | `1` |
| `E3015` | `import` outside top level | `1` |
| `E3016` | `export` outside top level | `1` |
| `E3017` | Module used as value/called/unqualified | `1` |
| `E4010` | Module cannot be read/resolved (missing, not UTF-8) | `4` |
| `E4012` | Invalid import path (absolute, wrong extension, bad file stem) | `4` |
| `E4013` | Access to non-exported module member | `1` |
| `E4014` | Import cycle | `1` |

## Test plan (implemented)

Positive: qualified call, exported variable, transitive dependency, module-local helper functions, module-executes-once across importers, module import from REPL across entries.
Negative: unqualified access `E3001`, unexported member `E4013`, module-as-value `E3017`, missing file `E4010` (CLI exit 4), cycle `E4014`, duplicate import `E3002`, absolute path `E4012`, bad extension `E4012`, bad file stem `E4012`, import in block `E3015`, export in block `E3016`, arity mismatch `E3008`.

## Compatibility

Local relative `.msf` imports only. No observable change to v0.1/v0.2 single-file behavior. `import` and `export` are now reserved keywords; v0.1 programs using those identifiers must be renamed.

## Known limits

- No `as alias` syntax yet.
- No re-export.
- No packages/manifests/registries.
- REPL entries re-resolve imports; re-importing the same path is `E3002`.
- Module iteration/lookup order for cycles is reported textually (load order).
