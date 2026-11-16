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
- The `.msf` extension is recommended but not required by the CLI

### `msf --help` / `msf -h`
Prints usage information and exits `0`.

### `msf --version` / `msf -v`
Prints the version string (e.g. `msf 0.1.0`) and exits `0`.

### `msf repl`
Starts an interactive REPL session (**not implemented yet** in the current build).

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

- Script arguments, `--` separator, and additional subcommands are not available in v0.1
- REPL behavior described in the spec (persistence, rollback, multiline input) is planned but not yet implemented
