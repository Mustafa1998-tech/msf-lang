namespace MsfLang.Core.Source;

/// <summary>
/// A 1-based line and column. Columns count Unicode scalar values, not UTF-16 code units.
/// </summary>
public readonly record struct SourcePosition(int Line, int Column);
