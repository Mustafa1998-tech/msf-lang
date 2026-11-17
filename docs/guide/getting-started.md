# Getting Started

## Prerequisites

- **.NET SDK 10.0+** (the repository pins SDK `10.0.302` via [`global.json`](../../global.json))
- Windows, macOS, or Linux (development and current verification are Windows-first)

Check your SDK:

```powershell
dotnet --version
```

## Clone and Build

```powershell
git clone https://github.com/Mustafa1998-tech/msf-lang.git
cd msf-lang
dotnet restore MSFLang.slnx
dotnet build MSFLang.slnx -c Release -warnaserror
```

## Run Your First Program

Create a file named `hello.msf`:

```msf
print("Hello, MSF!");
```

Run it with the CLI project:

```powershell
dotnet run --project src/MsfLang.Cli -c Release -- run hello.msf
```

Output:

```text
Hello, MSF!
```

You can also run the included examples:

```powershell
dotnet run --project src/MsfLang.Cli -c Release -- run examples/hello.msf
dotnet run --project src/MsfLang.Cli -c Release -- run examples/factorial.msf
```

## Project Layout (for users)

- `examples/` — runnable `.msf` programs
- `src/MsfLang.Cli/` — command-line app (`msf`)
- `src/MsfLang.Core/` — language engine
- `docs/en/SPEC-v0.1.md` — normative language specification

## Current Limitations

- `msf repl` is implemented (interactive and piped modes), but some **spec-contract edge cases** (e.g. rollback/incomplete-input behaviors and full contract coverage) are still being hardened
- **v0.2** draft adds `while`, `break`/`continue`, lists, `len`/`append`, maps (`map { "k": v }`, `get`/`set`/`has`/`remove`), math and string builtins
- **v0.3** adds local file modules: `import "./math.msf";` + `export`, qualified access (`math.square(9)`), module-executes-once, cycle diagnostics
- Closures/inline functions as values, packages/registries, file/network I/O, and a full stdlib are not available yet
- MSF Lang is not production-ready: there is no sandbox, no runtime quotas, and no package manager

See also [v0.3 modules spec](../en/SPEC-v0.3-modules.md).

For the full, normative behavior, see the [v0.1 specification](../en/SPEC-v0.1.md).
