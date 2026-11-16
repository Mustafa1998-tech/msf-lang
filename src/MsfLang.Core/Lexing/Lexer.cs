using System.Globalization;
using System.Text;
using MsfLang.Core.Diagnostics;
using MsfLang.Core.Source;

namespace MsfLang.Core.Lexing;

public sealed class Lexer
{
    private static readonly Dictionary<string, TokenKind> Keywords = new(StringComparer.Ordinal)
    {
        ["let"] = TokenKind.Let,
        ["var"] = TokenKind.Var,
        ["fn"] = TokenKind.Fn,
        ["if"] = TokenKind.If,
        ["else"] = TokenKind.Else,
        ["return"] = TokenKind.Return,
        ["true"] = TokenKind.True,
        ["false"] = TokenKind.False,
        ["nil"] = TokenKind.Nil,
        ["and"] = TokenKind.And,
        ["or"] = TokenKind.Or,
        ["not"] = TokenKind.Not,
    };

    private readonly SourceText _source;
    private readonly string _text;
    private readonly List<Token> _tokens = new();
    private int _current;

    public Lexer(SourceText source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _text = source.Text;
    }

    public static IReadOnlyList<Token> Lex(SourceText source) => new Lexer(source).ScanTokens();

    public IReadOnlyList<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            ScanToken();
        }

        _tokens.Add(new Token(TokenKind.EndOfFile, string.Empty, new TextSpan(_current, _current)));
        return _tokens;
    }

    private void ScanToken()
    {
        var start = _current;
        var c = Advance();
        switch (c)
        {
            case ' ':
            case '\t':
            case '\r':
            case '\n':
                return;
            case '(':
                Add(TokenKind.LeftParen, start);
                return;
            case ')':
                Add(TokenKind.RightParen, start);
                return;
            case '{':
                Add(TokenKind.LeftBrace, start);
                return;
            case '}':
                Add(TokenKind.RightBrace, start);
                return;
            case ',':
                Add(TokenKind.Comma, start);
                return;
            case ';':
                Add(TokenKind.Semicolon, start);
                return;
            case '+':
                Add(TokenKind.Plus, start);
                return;
            case '-':
                Add(TokenKind.Minus, start);
                return;
            case '*':
                Add(TokenKind.Star, start);
                return;
            case '%':
                Add(TokenKind.Percent, start);
                return;
            case '=':
                Add(Match('=') ? TokenKind.EqualEqual : TokenKind.Equal, start);
                return;
            case '!':
                if (Match('='))
                {
                    Add(TokenKind.BangEqual, start);
                    return;
                }

                throw Unexpected(start, c);
            case '<':
                Add(Match('=') ? TokenKind.LessEqual : TokenKind.Less, start);
                return;
            case '>':
                Add(Match('=') ? TokenKind.GreaterEqual : TokenKind.Greater, start);
                return;
            case '/':
                if (Match('/'))
                {
                    while (!IsAtEnd() && Peek() != '\n' && Peek() != '\r')
                    {
                        Advance();
                    }

                    return;
                }

                if (Match('*'))
                {
                    ScanBlockComment(start);
                    return;
                }

                Add(TokenKind.Slash, start);
                return;
            case '"':
                ScanString(start);
                return;
            case '\uFEFF':
                // BOM only allowed at offset 0 of the file (handled by SourceText).
                throw Unexpected(start, c);
            default:
                if (IsAsciiDigit(c))
                {
                    ScanNumber(start);
                    return;
                }

                if (IsIdentifierStart(c))
                {
                    ScanIdentifier(start);
                    return;
                }

                throw Unexpected(start, c);
        }
    }

    private void ScanIdentifier(int start)
    {
        while (!IsAtEnd() && IsIdentifierPart(Peek()))
        {
            Advance();
        }

        var text = _text[start.._current];
        var kind = Keywords.TryGetValue(text, out var keyword) ? keyword : TokenKind.Identifier;
        _tokens.Add(new Token(kind, text, new TextSpan(start, _current)));
    }

    private void ScanNumber(int start)
    {
        while (!IsAtEnd() && IsAsciiDigit(Peek()))
        {
            Advance();
        }

        if (!IsAtEnd() && Peek() == '.' && IsAsciiDigit(PeekNext()))
        {
            Advance(); // consume '.'
            while (!IsAtEnd() && IsAsciiDigit(Peek()))
            {
                Advance();
            }
        }

        var text = _text[start.._current];
        if (!double.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) ||
            !double.IsFinite(value))
        {
            throw new LexException(new Diagnostic(
                DiagnosticCode.E1002,
                DiagnosticPhase.Lexical,
                $"numeric literal is not finite: '{text}'",
                new TextSpan(start, _current)));
        }

        _tokens.Add(new Token(TokenKind.Number, text, new TextSpan(start, _current), value));
    }

    private void ScanString(int start)
    {
        var sb = new StringBuilder();
        while (!IsAtEnd())
        {
            var c = Peek();
            if (c == '"')
            {
                Advance(); // closing quote
                var lexeme = _text[start.._current];
                _tokens.Add(new Token(TokenKind.StringLiteral, lexeme, new TextSpan(start, _current), sb.ToString()));
                return;
            }

            if (c == '\n' || c == '\r')
            {
                throw new LexException(new Diagnostic(
                    DiagnosticCode.E1003,
                    DiagnosticPhase.Lexical,
                    "unterminated string literal",
                    new TextSpan(start, start + 1)));
            }

            if (c == '\\')
            {
                var slashPos = _current;
                Advance(); // consume '\'
                if (IsAtEnd())
                {
                    throw new LexException(new Diagnostic(
                        DiagnosticCode.E1003,
                        DiagnosticPhase.Lexical,
                        "unterminated string literal",
                        new TextSpan(start, start + 1)));
                }

                var esc = Advance();
                switch (esc)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case '\\': sb.Append('\\'); break;
                    case '"': sb.Append('"'); break;
                    default:
                        throw new LexException(new Diagnostic(
                            DiagnosticCode.E1004,
                            DiagnosticPhase.Lexical,
                            $"invalid escape sequence '\\{esc}'",
                            new TextSpan(slashPos, _current)));
                }

                continue;
            }

            sb.Append(Advance());
        }

        throw new LexException(new Diagnostic(
            DiagnosticCode.E1003,
            DiagnosticPhase.Lexical,
            "unterminated string literal",
            new TextSpan(start, Math.Min(start + 1, _text.Length))));
    }

    private void ScanBlockComment(int start)
    {
        while (!IsAtEnd())
        {
            if (Peek() == '*' && PeekNext() == '/')
            {
                Advance();
                Advance();
                return;
            }

            Advance();
        }

        throw new LexException(new Diagnostic(
            DiagnosticCode.E1005,
            DiagnosticPhase.Lexical,
            "unterminated block comment",
            new TextSpan(start, start + 2)));
    }

    private bool Match(char expected)
    {
        if (IsAtEnd() || _text[_current] != expected)
        {
            return false;
        }

        _current++;
        return true;
    }

    private char Peek() => IsAtEnd() ? '\0' : _text[_current];
    private char PeekNext() => _current + 1 >= _text.Length ? '\0' : _text[_current + 1];

    private bool IsAtEnd() => _current >= _text.Length;

    private char Advance() => _text[_current++];

    private void Add(TokenKind kind, int start)
    {
        _tokens.Add(new Token(kind, _text[start.._current], new TextSpan(start, _current)));
    }

    private static LexException Unexpected(int pos, char c)
    {
        var display = c == '\uFEFF' ? "\\uFEFF" : c.ToString();
        return new LexException(new Diagnostic(
            DiagnosticCode.E1001,
            DiagnosticPhase.Lexical,
            $"unexpected character '{display}'",
            new TextSpan(pos, pos + 1)));
    }

    private static bool IsAsciiDigit(char c) => c >= '0' && c <= '9';
    private static bool IsIdentifierStart(char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_';
    private static bool IsIdentifierPart(char c) => IsIdentifierStart(c) || IsAsciiDigit(c);
}
