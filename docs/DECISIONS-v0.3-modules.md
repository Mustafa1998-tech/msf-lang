# ADR-0021 — v0.3 local modules: qualified access and `export`

**Status:** Accepted (2026-11-17) by project owner. Implemented in `msf 0.3.0-rc.1`.
**Spec:** [`docs/en/SPEC-v0.3-modules.md`](en/SPEC-v0.3-modules.md) (EN normative), [`docs/ar/SPEC-v0.3-modules.md`](ar/SPEC-v0.3-modules.md) (AR mirror).

## Decision

MSF Lang v0.3 adds local file modules with **qualified access**:

```msf
import "./math.msf";
print(math.square(9));
```

- `import "path";` declares a module binding named after the imported file's stem (`math` for `math.msf`).
- Only names marked `export` in the module are visible on the module binding.
- Unqualified names are not imported; they remain module-local or caller-local.
- Modules are loaded (read/parse/resolve) at most once per process/REPL session and execute at most once, in dependency order, before the entry file.
- Circular imports are rejected with `E4014`; unreadable/missing modules are `E4010` (exit 4); invalid paths are `E4012` (exit 4); access to unexported members is `E4013`; module-as-value/call misuse is `E3017`.
- `import`/`export` are reserved keywords in v0.3 (breaking change for v0.1 code using them as identifiers).

## Why qualified access

- Prevents silent collisions overwriting caller globals.
- Makes API ownership visible at call site.
- Keeps the change incremental to the resolver/interpreter instead of forcing a global namespace merge.

## Why `export`

- Makes the module boundary explicit.
- Avoids accidental leakage of module-locals to importers.

## Consequences

- Modules execute in dependency order before the entry program; top-level module side effects (e.g. `print`) happen first.
- REPL modules persist for the session; re-importing the same path is a duplicate declaration (`E3002`).
- v0.1/v0.2 single-file behavior is unchanged apart from the reserved keywords.
- Package manifest/registry, `as alias`, re-exports, and stdlib collection APIs remain out of scope.

## Implementation notes

- `ModuleLoadContext` (per run, cached per REPL session) guarantees read/parse/resolve-once per path and stable module ids; `ModuleRuntimeState` guarantees execute-once per session.
- The module's interpreter environment is captured as `UserFunction.Home`, so module functions close over module globals; diagnostics produced inside a module carry the module's source path.

## Alternatives rejected for v0.3

- **Textual include semantics** — simple but silently merges global scopes; easy to shadow names accidentally.
- **Namespace object as first-class value** — requires full object/namespace value support; too large for v0.3.
- **No-namespace public globals** — destroys isolation and tooling behaviour.

## Open items

- `as alias`, re-export, package manifests, and runtime quotas/sandbox are explicitly not part of v0.3.
- Release candidate checklist and signing/distribution are separate approval items.
