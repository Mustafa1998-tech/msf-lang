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

- `msf repl` is **not implemented yet** (planned)
- v0.1 has **no loops**, arrays/maps, modules/imports, closures, or file/network I/O
- Some edge-case/runtime behaviors and full conformance coverage are still being completed

For the full, normative behavior, see the [specification](../en/SPEC-v0.1.md).
