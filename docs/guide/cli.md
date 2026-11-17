# CLI Reference

The MSF Lang command-line executable is **`msf`** (`msf.exe` on Windows).

> In this repository (before packaging), run it via:
> ```powershell
> dotnet run --project src/MsfLang.Cli -c Release -- <args>
> ```

## Commands

### `msf run <path>`
Reads a UTF-8 source file and executes it.

```powershell
dotnet run --project src/MsfLang.Cli -c Release -- run examples/hello.msf
```

- Program output is written to **stdout**
- Diagnostics/errors are written to **stderr**
- On success, exits with code `0`
- The `.msf` extension is recommended but not required by the CLI; **imported modules must use `.msf`** (v0.3)
- Module diagnostics point into the imported file (its path is shown in `error[Exxxx]:` headers)

### `msf --help` / `msf -h`
Prints usage, the command list, a short v0.3 scope summary, and a link to the specification, then exits `0`.

### `msf --version` / `msf -v`
Prints the version string (e.g. `msf 0.3.0-rc.1`) and exits `0`.

### `msf repl`
Starts a REPL session.

- **Interactive mode:** shows a banner and prompts (`msf>`, `...>`), supports multiline input and commands `:help`, `:quit`
- **Piped/non-interactive mode:** no banner/prompts; reads input from stdin and prints auto-displayed expression results
- Persists declarations/state across entries; imported modules stay loaded for the session

## Modules (v0.3)

```msf
import "./math.msf";
print(math.square(9));
```

Rules in brief: relative `.msf` paths only, `export` marks public names, access is qualified (`math.name`), a module runs once, cycles are `E4014`. Full rules: [v0.3 modules spec](../en/SPEC-v0.3-modules.md) and [example](../guide/examples.md#8-local-modules-v03).

## Exit Codes

| Code | Meaning |
|---|---|
| `0` | Success |
| `1` | Compile-time error (lexical, syntax, or semantic) |
| `2` | Runtime error |
| `3` | CLI usage/argument error |
| `4` | I/O error (missing/unreadable file, invalid UTF-8) |
| `70` | Unexpected internal error |

## Common Usage Errors

- No arguments: prints usage to stderr and exits `3`
- Unknown command: reports usage error and exits `3`
- `msf run` without exactly one path: exits `3`

## Current Limitations

- Script arguments, `--` separator, and additional subcommands are not available
- Full spec-contract compliance (all rollback/incomplete-input edge cases) is still in progress
- No packages, package manifests, or sandbox yet; MSF Lang should not be treated as production-ready
