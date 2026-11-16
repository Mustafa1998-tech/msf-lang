namespace MsfLang.Core.Source;

/// <summary>
/// A half-open character range <c>[Start, End)</c> into a <see cref="SourceText"/>.
/// Offsets are UTF-16 char indices, matching <see cref="string"/> indexing.
/// </summary>
public readonly record struct TextSpan(int Start, int End)
{
    public int Length => End - Start;

    public static TextSpan Empty => new(0, 0);

    public bool Contains(int offset) => offset >= Start && offset < End;

    public override string ToString() => $"{Start}..{End}";
}
