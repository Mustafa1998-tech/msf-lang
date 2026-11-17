# MSF Lang v0.3.0-rc.1 — Release Notes (Release Candidate)

**Status:** Release Candidate — not a production release. MSF Lang should not yet be run with untrusted input or in production services. Production-general use still requires a stdlib, package ecosystem, runtime quotas/sandbox, and RC hardening.

## What's new

### v0.3 — Local file modules (accepted ADR-0021)

- `import "./math.msf";` loads a local module relative to the importing file. `.msf` extension only; absolute paths rejected (`E4012`).
- `export fn` / `export var` / `export let` at top level marks public names; everything else is module-private.
- **Qualified access only:** `math.square(9)`, `math.PI`. Unqualified use is `E3001`.
- **Run-once:** modules load and execute at most once per process/REPL session, in dependency order.
- Isolated module scopes; module functions close over their module globals.
- **Diagnostics:** import cycle `E4014`, missing module `E4010` (exit 4), invalid path/extension/file stem `E4012` (exit 4), unexported member `E4013`, module-as-value `E3017`, `import`/`export` outside top level `E3015`/`E3016`, syntax errors `E2014`/`E2015`.
- Multi-file diagnostics carry the importing/imported file path; the CLI renders the correct source.

### v0.2 — Practical core (additive)

- `while`, `break`, `continue`; `list` literals/indexing/in-place writes; `len`, `append`
- `map { "k": v }` literals with string keys; `get`/`set`/`has`/`remove`; `len(map)`
- Math builtins: `abs`, `sqrt`, `floor`, `ceil`, `round`, `pow`, `min`, `max`
- String builtins: `upper`, `lower`, `trim`, `contains`, `startsWith`, `endsWith`, `split`
- `MSF_DEBUG=1` prints an internal-error stack trace to stderr (all other paths never do)

## Compatibility

- `import` and `export` are now reserved keywords; v0.1 programs using them as identifiers will fail to parse and must be renamed.
- Otherwise v0.1/v0.2 single-file behavior is unchanged.

## Verification (executed 2026-11-17)

- `dotnet build MSFLang.slnx -c Release -warnaserror` → 0 errors
- `dotnet test MSFLang.slnx -c Release` → **216 passed / 0 failed / 0 skipped** (Core 110, Conformance 87, CLI 19)
- Manual acceptance scenarios (10) re-verified: hello, factorial, runtime/semantic diagnostics exits (2/1), missing file (4), piped REPL multiline + rollback, `--version`, path with spaces, UTF-8 Arabic output, modules end-to-end
- `examples/modules/main.msf` runs and prints `81`, `3.5`, `14`

## Known limitations (intentional)

- Local relative `.msf` modules only — no packages, manifests, registries, re-exports, or `as alias`
- No stdlib beyond the small builtin set; no file/stdin I/O APIs, no network, no closures/first-class functions
- No runtime quotas or sandbox; do not use for untrusted code
- Map iteration order is unspecified
- Math/string builtinset is a starter, not a full stdlib

## Feedback

File issues and discussion on GitHub: https://github.com/Mustafa1998-tech/msf-lang
