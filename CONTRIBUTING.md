# Contributing to MSF Lang

Thank you for your interest in contributing to MSF Lang. This project follows a specification-first, evidence-driven approach: changes must align with the v0.1 specification, architecture decisions, and implementation plan, and must be verified by tests with recorded evidence.

## Quick links

- [Language Specification (EN)](docs/en/SPEC-v0.1.md)
- [Implementation & Test Plan (EN)](docs/en/IMPLEMENTATION-PLAN.md)
- [Architecture Decisions](docs/DECISIONS.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Roadmap](docs/ROADMAP.md)
- [Problem-Solving Direction](docs/PROBLEM-SOLVING.md)
- [Test Evidence Log](docs/TEST-EVIDENCE.md)
- [Security](SECURITY.md) · [Security Guide](docs/guide/security.md)

## Ground rules

1. **Spec first.** Language behavior changes MUST be reflected in the specification and/or an ADR before or together with implementation.
2. **Tests required.** New behavior needs unit tests and, where applicable, `.msf` conformance cases.
3. **Evidence required.** Record executed commands and results in `docs/TEST-EVIDENCE.md` (no unverified claims).
4. **Small, reviewable changes.** Prefer focused pull requests over large unrelated edits.
5. **No third-party runtime dependencies** in Core/CLI. Tests may use xUnit.
6. **Do not silently break v0.1 behavior** for conforming programs. Breaking changes need explicit versioning/migration notes.

## How to contribute

### 1. Set up the environment

```powershell
git clone https://github.com/Mustafa1998-tech/msf-lang.git
cd msf-lang
dotnet restore MSFLang.slnx
dotnet build MSFLang.slnx -c Release -warnaserror
dotnet test MSFLang.slnx -c Release
```

See [`global.json`](global.json) for the pinned SDK.

### 2. Find something to work on

- Check open issues and discussions.
- Consult the [Roadmap](docs/ROADMAP.md) for priority work (v0.1 hardening, v0.2 practicality).
- Review known gaps in the [Test Evidence](docs/TEST-EVIDENCE.md) and implementation plan.

### 3. Make focused changes

- Keep the engine a clean layered pipeline (source → lexer → parser → resolver → interpreter).
- Preserve Unicode-scalar positions, spans, and diagnostic code stability.
- Follow the coding style of the surrounding files (nullable reference types, explicit types where helpful, no hidden global state).
- Do not add third-party runtime/scripting dependencies to Core or CLI.

### 4. Add tests

- **Core behavior:** add/update tests in `tests/MsfLang.Core.Tests`.
- **Language semantics end-to-end:** add `.msf` + `.expected.json` fixtures in `tests/conformance/` (positive/negative).
- **CLI behavior:** add process-level tests in `tests/MsfLang.Cli.Tests`.
- Ensure no new skipped tests.

### 5. Verify before opening a PR

```powershell
dotnet build MSFLang.slnx -c Release -warnaserror
dotnet test MSFLang.slnx -c Release
```

Update `docs/TEST-EVIDENCE.md` with any commands you actually ran and their observed outcomes (date, exit code, pass/fail/skip counts).

### 6. Submit a pull request

- Describe **what** changed and **why**.
- Link relevant issues/ADRs/spec sections.
- Include the test/evidence commands you ran.
- Keep the change scoped; avoid drive-by reformatting unrelated files.

## Language design proposals

If you propose a new language feature:

1. Open an issue describing the problem and proposed solution.
2. Propose specification changes (EN normative; keep AR in sync if possible).
3. Add/update ADR if the decision is architectural.
4. Add conformance cases and unit tests.
5. Record evidence and document compatibility/performance implications.

## Reporting bugs

Include:
- MSF Lang version/commit
- OS and .NET SDK version (`dotnet --version`)
- Minimal reproducer (`.msf` file or command)
- Expected vs actual behavior (exit code, stdout/stderr)

## Security

For security issues, follow [`SECURITY.md`](SECURITY.md). Do not open public issues for vulnerabilities.

## Code of Conduct

Participation in this project is governed by the [Code of Conduct](CODE_OF_CONDUCT.md).

## License

Contributions are provided under the project’s [MIT License](LICENSE).
