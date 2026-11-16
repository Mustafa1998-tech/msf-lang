# Security Policy

## Supported versions

| Version | Supported |
|---|---|
| `master` / latest development | Yes |
| `v0.1.0-rc*` (when published) | Yes |
| `v0.1.0` (when published) | Yes |
| Older tags/branches | Best effort only |

## Reporting a vulnerability

If you discover a security issue in MSF Lang or this repository, **please do not open a public issue**.

Preferred reporting route:

1. Use GitHub’s **private vulnerability reporting** for this repository (Security tab → Report a vulnerability), or
2. Contact the maintainers through the repository’s private contact channel or issue tracker by marking the report as confidential.

Please include:
- A clear description of the vulnerability
- Steps to reproduce (commands, minimal `.msf` source if relevant)
- Affected version/commit and environment (OS, .NET SDK)
- Potential impact and any known mitigations
- Your assessment of severity (if any)

We aim to acknowledge reports promptly and coordinate fixes responsibly. Please allow reasonable time before public disclosure.

## Scope

In scope:
- CLI and engine behavior that can lead to code execution, data loss, privilege escalation, or crashes triggered by untrusted input
- Diagnostic/output handling that leaks sensitive local paths in unexpected contexts
- Supply-chain/packaging issues in this repository’s build and distribution process
- Documentation/branding assets that misrepresent security guarantees

Out of scope (generally):
- General language design disagreements without a security impact
- Bugs that require the user to intentionally run untrusted malicious `.msf` code in their own environment (though we still want to know)
- Denial of service via deliberately extreme resource exhaustion without a practical abuse path beyond normal limits (still report; we will triage)

## Safe harbor

We will not pursue legal action against researchers who:
- report vulnerabilities responsibly through the private channels above
- avoid privacy violations, data destruction, or service disruption
- do not disclose publicly before coordinated remediation (or 90 days, whichever is reasonable and agreed)

## Security practices for contributors

- Do not add third-party runtime dependencies to Core/CLI without explicit approval and review.
- Do not commit secrets, certificates, or private keys.
- Review registry/file-association changes (Windows integration) require explicit Gate D approval and a disposable test environment.
- Keep dependencies and tooling pinned where practical (see `global.json`).
- Runtime security controls (see [`docs/guide/security.md`](../docs/guide/security.md)): bounded user-call depth (1000), bounded parser nesting (256), strict UTF-8, no I/O capabilities in v0.1, dedicated 32 MiB interpreter stack in the CLI.
- When changing external behavior, add/update tests and note the security implications in the PR description.

## Public disclosure

Once a fix is released, we may publish a security advisory and credit reporters who wish to be acknowledged.
