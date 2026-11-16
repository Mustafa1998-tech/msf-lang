using System.Globalization;
using System.Text;
using MsfLang.Core.Source;

namespace MsfLang.Core.Diagnostics;

/// <summary>
/// Formats diagnostics per SPEC §12.3.
/// </summary>
public static class DiagnosticFormatter
{
    public static string Format(Diagnostic diagnostic, SourceText source)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);
        ArgumentNullException.ThrowIfNull(source);

        var start = source.GetPosition(diagnostic.Span.Start);
        var sb = new StringBuilder();
        sb.Append("error[")
          .Append(diagnostic.Code)
          .Append("]: ")
          .Append(diagnostic.Message)
          .Append('\n');

        sb.Append(" --> ")
          .Append(source.Path)
          .Append(':')
          .Append(start.Line.ToString(CultureInfo.InvariantCulture))
          .Append(':')
          .Append(start.Column.ToString(CultureInfo.InvariantCulture))
          .Append('\n');

        var lineText = source.GetLineText(start.Line);
        var lineNumber = start.Line.ToString(CultureInfo.InvariantCulture);
        var gutter = new string(' ', lineNumber.Length);

        sb.Append(gutter).Append(" |\n");
        sb.Append(lineNumber).Append(" | ").Append(lineText).Append('\n');

        sb.Append(gutter).Append(" | ");
        AppendCaretPrefix(sb, lineText, start.Column);
        var caretCount = GetCaretCount(source, lineText, diagnostic.Span, start.Column);
        sb.Append('^', caretCount).Append('\n');

        if (!string.IsNullOrEmpty(diagnostic.Hint))
        {
            sb.Append(gutter).Append(" = hint: ").Append(diagnostic.Hint).Append('\n');
        }

        return sb.ToString();
    }

    private static void AppendCaretPrefix(StringBuilder sb, string lineText, int column)
    {
        // column is 1-based in Unicode scalar values; preserve tabs for alignment.
        var currentColumn = 1;
        var i = 0;
        while (i < lineText.Length && currentColumn < column)
        {
            var c = lineText[i];
            if (c == '\t')
            {
                sb.Append('\t');
                i++;
                currentColumn++;
                continue;
            }

            System.Text.Rune.DecodeFromUtf16(lineText.AsSpan(i), out _, out var consumed);
            sb.Append(' ', 1);
            i += consumed;
            currentColumn++;
        }
    }

    private static int GetCaretCount(SourceText source, string lineText, TextSpan span, int startColumn)
    {
        if (span.Length <= 0)
        {
            return 1;
        }

        var lineEndOffset = Math.Min(span.End, source.Length);
        var count = 0;
        var i = span.Start;
        while (i < lineEndOffset)
        {
            // stop at end of source line
            if (i >= 0 && i < source.Text.Length)
            {
                var ch = source.Text[i];
                if (ch == '\n' || ch == '\r')
                {
                    break;
                }
            }

            System.Text.Rune.DecodeFromUtf16(source.Text.AsSpan(i), out _, out var consumed);
            i += consumed;
            count++;
            if (i > span.Start && source.GetPosition(i).Line != source.GetPosition(span.Start).Line)
            {
                // multi-line spans: underline only the start line portion in v0.1
                break;
            }
        }

        if (count < 1)
        {
            count = 1;
        }

        // Ensure we don't exceed remaining visible columns on the excerpt line.
        var lineScalarCount = CountScalars(lineText);
        var max = Math.Max(1, lineScalarCount - startColumn + 1);
        return Math.Min(count, max);
    }

    private static int CountScalars(string text)
    {
        var count = 0;
        var i = 0;
        while (i < text.Length)
        {
            System.Text.Rune.DecodeFromUtf16(text.AsSpan(i), out _, out var consumed);
            i += consumed;
            count++;
        }

        return count;
    }
}
