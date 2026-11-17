using MsfLang.Core.Source;

namespace MsfLang.Core.Diagnostics;

public sealed record Diagnostic(
    string Code,
    DiagnosticPhase Phase,
    string Message,
    TextSpan Span,
    string? Hint = null,
    string? SourcePath = null);
