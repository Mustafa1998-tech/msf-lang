# MSF Lang Security

This page describes the security posture of the reference implementation (`msf` CLI and tree-walk interpreter) as of v0.1. Normative language behavior is defined in the language specification; this page explains what the implementation does today, what it does not do, and what users/contributors should expect.

## What "applied" means here

- **Repository security policy exists:** [`SECURITY.md`](../../SECURITY.md) (responsible reporting, supported versions, safe harbor).
- **License is present and usable:** [`LICENSE`](../../LICENSE) (MIT) and linked from the README.
- **Runtime security controls are implemented in the engine/CLI where applicable** (see below).
- **The language itself is intentionally narrow:** v0.1 has no file/network/input/environment access, which removes large classes of attacks.

## 1. Language-level safety properties (v0.1, by design)

| Control | Status | Notes |
|---|---|---|
| No file I/O | ✅ | Programs cannot read/write files |
| No network I/O | ✅ | No sockets/HTTP/etc. in v0.1 |
| No input | ✅ | No stdin-based user input in programs |
| No environment/clock/randomness | ✅ | Deterministic execution; no `env`/time/RNG |
| No FFI / no dynamic code loading | ✅ | Programs cannot call native code or load assemblies |
| No `eval`/metaprogramming | ✅ | Source is fixed at parse time |
| Bounded recursion (user calls) | ✅ | Maximum **1000** active user-function calls; excess raises `E4005` |
| Bounded parser nesting | ✅ | Maximum **256** nesting depth; excess raises `E2013` |
| Fail-fast diagnostics | ✅ | First error stops the phase; no hidden recovery |
| Strict UTF-8 with BOM handling | ✅ | Invalid UTF-8 is `E5005` (exit 4), not a language error |
| Deterministic `print`-only output | ✅ | Only observable side effect is writing to stdout via `print` |

## 2. CLI security properties

| Control | Status | Notes |
|---|---|---|
| UTF-8 without BOM stdout | ✅ | Output encoding forced to UTF-8 without BOM |
| stderr for diagnostics | ✅ | Language/usage diagnostics go to stderr |
| Exit codes are stable | ✅ | 0/1/2/3/4/70 |
| No path/timing/banner noise on `run` | ✅ | Deterministic, script-friendly output |
| Internal errors not mixed with language diagnostics | ✅ | Unexpected exceptions report `internal error` (exit 70) |
| Dedicated 32 MiB interpreter stack | ✅ | `msf` runs engine logic on a dedicated thread to make the 1000-call limit deterministic |

## 3. Explicitly not a sandbox (threat model limits)

MSF Lang v0.1 is **not** a sandboxed execution environment for untrusted code:

- It does not enforce CPU time limits, wall-clock timeouts, memory limits, or per-file quotas.
- It does not perform sandboxing of the host process, filesystem, network, or OS.
- Malicious programs can still attempt CPU exhaustion within the 1000-call limit (e.g., exponential recursion) and can consume memory/formattable output like any program.
- Diagnostics include source paths/lines for the file being processed; when paths are sensitive, run `msf` with care.

If you need to run untrusted programs, run them in an OS-level sandbox/container/VM with appropriate resource and filesystem restrictions.

## 4. Supply-chain and build security (repository)

- Core/CLI target `.NET 10` and use **BCL only** (no third-party runtime dependencies).
- SDK is pinned via `global.json`.
- Tests use xUnit and the .NET test SDK (test-only).
- Repository build verification uses warnings-as-errors (`-warnaserror`).
- CI/automation is project work in progress; branch protection and reproducible releases are release-process goals.

## 5. Reporting and response

See [`SECURITY.md`](../../SECURITY.md). Do not open public issues for vulnerabilities. Include a minimal reproducer and environment details.

## 6. Known limitations and planned hardening

- **`MSF_DEBUG=1` stack traces:** the spec permits an internal-error stack trace under this environment variable; current CLI reports `internal error` without a stack trace. This is intentional to avoid leaking internals by default.
- **No time/memory quotas:** resource exhaustion is bounded only by recursion/nesting limits and host resources.
- **Windows file association/installer (Gate D):** not enabled by default; no registry writes during development.

## 7. Contributors and maintainers

- Review changes for unintended new capabilities (I/O, reflection, dynamic loading, serialization, file system access, environment access, or new builtins).
- Prefer adding tests and conformance fixtures for any externally visible security-relevant behavior.
- Keep `SECURITY.md`, this page, and the specification in sync when behavior changes.
