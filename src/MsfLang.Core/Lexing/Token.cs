using MsfLang.Core.Source;

namespace MsfLang.Core.Lexing;

public sealed record Token(
    TokenKind Kind,
    string Lexeme,
    TextSpan Span,
    object? Literal = null);
