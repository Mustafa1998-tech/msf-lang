using MsfLang.Core.Diagnostics;
using MsfLang.Core.Source;
using Xunit;

namespace MsfLang.Core.Tests.Diagnostics;

public sealed class DiagnosticFormatterTests
{
    [Fact]
    public void Formats_BasicDiagnostic_WithCodeLocationExcerptCaret()
    {
        var source = new SourceText("examples/div-zero.msf", "print(10 / 0);\n");
        var span = new TextSpan(source.Text.IndexOf('/'), source.Text.IndexOf('/') + 1);
        var diagnostic = new Diagnostic(
            DiagnosticCode.E4002,
            DiagnosticPhase.Runtime,
            "division by zero",
            span,
            "check that the divisor is not zero");

        var formatted = DiagnosticFormatter.Format(diagnostic, source);

        var expected =
            "error[E4002]: division by zero\n" +
            " --> examples/div-zero.msf:1:10\n" +
            "  |\n" +
            "1 | print(10 / 0);\n" +
            "  |          ^\n" +
            "  = hint: check that the divisor is not zero\n";

        Assert.Equal(expected, formatted);
    }

    [Fact]
    public void Caret_PreservesTabsInPrefix()
    {
        var text = "\tlet x = 1;\n";
        var source = new SourceText("tab.msf", text);
        var idx = text.IndexOf('x');
        var diagnostic = new Diagnostic("E1001", DiagnosticPhase.Lexical, "oops", new TextSpan(idx, idx + 1));

        var formatted = DiagnosticFormatter.Format(diagnostic, source);

        Assert.Contains("\n  | \t    ^\n", formatted.Replace("\r\n", "\n"));
    }

    [Fact]
    public void Caret_UsesUnicodeScalarColumns()
    {
        var text = "let س = 1;\n";
        var source = new SourceText("ar.msf", text);
        var idx = text.IndexOf('=', StringComparison.Ordinal);
        var diagnostic = new Diagnostic("E2008", DiagnosticPhase.Syntax, "msg", new TextSpan(idx, idx + 1));

        var formatted = DiagnosticFormatter.Format(diagnostic, source);

        // "let س " is 6 scalars before '=', so caret prefix should be 6 spaces in excerpt area.
        Assert.Contains("\n1 | let س = 1;\n  |       ^\n", formatted);
    }

    [Fact]
    public void GutterWidth_GrowsWithLineNumber()
    {
        var lines = string.Join("\n", Enumerable.Range(1, 10).Select(i => $"print({i});")) + "\n";
        var source = new SourceText("many.msf", lines);
        var idx = lines.IndexOf("print(10)", StringComparison.Ordinal);
        var diagnostic = new Diagnostic("E0000", DiagnosticPhase.Runtime, "x", new TextSpan(idx, idx + 1));

        var formatted = DiagnosticFormatter.Format(diagnostic, source);

        Assert.Contains("\n   |\n10 | print(10);\n   | ^\n", formatted);
    }
}
