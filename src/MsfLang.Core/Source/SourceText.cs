using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MsfLang.Core.Source;

/// <summary>
/// Immutable source text plus the line map used for 1-based, Unicode-scalar column reporting.
/// A single leading UTF-8 BOM character (U+FEFF) is accepted and removed from <see cref="Text"/>.
/// </summary>
public sealed class SourceText
{
    private readonly int[] _lineStarts;

    public SourceText(string path, string text)
    {
        Path = path;
        if (text.StartsWith('\uFEFF'))
        {
            HasBom = true;
            text = text.Substring(1);
        }

        Text = text;
        _lineStarts = ComputeLineStarts(text);
    }

    public string Path { get; }

    public string Text { get; }

    public bool HasBom { get; }

    public int Length => Text.Length;

    public int LineCount => _lineStarts.Length;

    private static int[] ComputeLineStarts(string text)
    {
        var starts = new List<int> { 0 };
        for (var i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                starts.Add(i + 1);
            }
            else if (c == '\n')
            {
                starts.Add(i + 1);
            }
        }

        return starts.ToArray();
    }

    /// <summary>Returns the 1-based line containing <paramref name="offset"/> and the 1-based
    /// Unicode-scalar column of that offset.</summary>
    public SourcePosition GetPosition(int offset)
    {
        if (offset < 0)
        {
            offset = 0;
        }
        else if (offset > Text.Length)
        {
            offset = Text.Length;
        }

        var lineIndex = FindLine(offset);
        var lineStart = _lineStarts[lineIndex];
        var column = 1;
        var i = lineStart;
        while (i < offset)
        {
            Rune.DecodeFromUtf16(Text.AsSpan(i), out _, out var consumed);
            i += consumed;
            column++;
        }

        return new SourcePosition(lineIndex + 1, column);
    }

    /// <summary>Returns the text of a 1-based line without its trailing line break.</summary>
    public string GetLineText(int line)
    {
        if (line < 1 || line > LineCount)
        {
            return string.Empty;
        }

        var start = _lineStarts[line - 1];
        var end = line < LineCount ? _lineStarts[line] : Text.Length;
        var text = Text.Substring(start, end - start);
        if (text.EndsWith("\r\n", StringComparison.Ordinal))
        {
            return text.Substring(0, text.Length - 2);
        }

        if (text.Length > 0 && (text[^1] == '\n' || text[^1] == '\r'))
        {
            return text.Substring(0, text.Length - 1);
        }

        return text;
    }

    private int FindLine(int offset)
    {
        var lo = 0;
        var hi = _lineStarts.Length - 1;
        while (lo < hi)
        {
            var mid = (lo + hi + 1) / 2;
            if (_lineStarts[mid] <= offset)
            {
                lo = mid;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return lo;
    }
}
