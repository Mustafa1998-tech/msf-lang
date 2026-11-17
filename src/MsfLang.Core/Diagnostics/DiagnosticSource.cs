using System.Text;
using MsfLang.Core.Source;

namespace MsfLang.Core.Diagnostics;

/// <summary>
/// Resolves the source text a diagnostic belongs to. Diagnostics produced inside an
/// imported module carry the module path, so formatting must not use the entry file.
/// </summary>
public static class DiagnosticSource
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static SourceText Resolve(Diagnostic diagnostic, SourceText fallback)
    {
        if (string.IsNullOrEmpty(diagnostic.SourcePath) ||
            string.Equals(diagnostic.SourcePath, fallback.Path, StringComparison.Ordinal))
        {
            return fallback;
        }

        try
        {
            return new SourceText(diagnostic.SourcePath, File.ReadAllText(diagnostic.SourcePath, StrictUtf8));
        }
        catch (Exception)
        {
            return fallback;
        }
    }
}
