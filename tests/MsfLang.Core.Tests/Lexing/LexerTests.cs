using MsfLang.Core.Diagnostics;
using MsfLang.Core.Lexing;
using MsfLang.Core.Source;
using Xunit;

namespace MsfLang.Core.Tests.Lexing;

public sealed class LexerTests
{
    [Fact]
    public void L01_EmptyFile_OnlyEof()
    {
        var tokens = Lex("");
        Assert.Single(tokens);
        Assert.Equal(TokenKind.EndOfFile, tokens[0].Kind);
    }

    [Fact]
    public void L02_BomAtStart_Ignored()
    {
        var tokens = Lex("\uFEFFlet x = 1;");
        Assert.Equal(TokenKind.Let, tokens[0].Kind);
        Assert.Equal(0, tokens[0].Span.Start);
    }

    [Fact]
    public void L03_BomNotAtStart_UnexpectedCharacter()
    {
        var ex = Assert.Throws<LexException>(() => Lex("let\uFEFF x = 1;"));
        Assert.Equal(DiagnosticCode.E1001, ex.Diagnostic.Code);
    }

    [Fact]
    public void L04_Whitespace_LfCrlfCr()
    {
        var tokens = Lex("let a = 1;\r\nlet b = 2;\rlet c = 3;\n");
        Assert.Equal(TokenKind.Let, tokens[0].Kind);
        Assert.Equal(TokenKind.Let, tokens[5].Kind);
        Assert.Equal(TokenKind.Let, tokens[10].Kind);
        Assert.Equal(TokenKind.EndOfFile, tokens[^1].Kind);
    }

    [Fact]
    public void L05_LineComments_Ignored()
    {
        var tokens = Lex("// hi\nlet x = 1; // end");
        Assert.Equal(TokenKind.Let, tokens[0].Kind);
    }

    [Fact]
    public void L06_BlockComment_Ignored()
    {
        var tokens = Lex("/* a\nb */ let x = 1;");
        Assert.Equal(TokenKind.Let, tokens[0].Kind);
    }

    [Fact]
    public void L07_NestedBlockCommentEndsAtFirstClose()
    {
        // "/* a /* b */ c" -> after first */ remaining " c" is source
        var tokens = Lex("/* a /* b */ let x = 1;");
        Assert.Equal(TokenKind.Let, tokens[0].Kind);
    }

    [Fact]
    public void L08_UnterminatedBlockComment()
    {
        var ex = Assert.Throws<LexException>(() => Lex("/* nope"));
        Assert.Equal(DiagnosticCode.E1005, ex.Diagnostic.Code);
        Assert.Equal(0, ex.Diagnostic.Span.Start);
    }

    [Fact]
    public void L09_Keywords()
    {
        var kinds = Lex("let var fn if else return true false nil and or not")
            .Where(t => t.Kind != TokenKind.EndOfFile).Select(t => t.Kind).ToArray();
        Assert.Equal(new[]
        {
            TokenKind.Let, TokenKind.Var, TokenKind.Fn, TokenKind.If, TokenKind.Else, TokenKind.Return,
            TokenKind.True, TokenKind.False, TokenKind.Nil, TokenKind.And, TokenKind.Or, TokenKind.Not
        }, kinds);
    }

    [Fact]
    public void L10_KeywordPrefixes_AreIdentifiers()
    {
        var tokens = Lex("lets iffy return1 nil_");
        Assert.All(tokens.Take(4), t => Assert.Equal(TokenKind.Identifier, t.Kind));
    }

    [Fact]
    public void L11_IdentifierWithUnderscoreDigits()
    {
        var t = Lex("_total2")[0];
        Assert.Equal(TokenKind.Identifier, t.Kind);
        Assert.Equal("_total2", t.Lexeme);
    }

    [Fact]
    public void L12_IntegerLiterals()
    {
        var tokens = Lex("42 0 1000");
        Assert.Equal(42d, tokens[0].Literal);
        Assert.Equal(0d, tokens[1].Literal);
        Assert.Equal(1000d, tokens[2].Literal);
    }

    [Fact]
    public void L13_FractionalLiteral()
    {
        var t = Lex("3.14")[0];
        Assert.Equal(TokenKind.Number, t.Kind);
        Assert.Equal(3.14d, (double)t.Literal!, 10);
    }

    [Fact]
    public void L14_DotForms_NotNumbers()
    {
        // ".5" -> unexpected '.' at start
        var ex1 = Assert.Throws<LexException>(() => Lex(".5"));
        Assert.Equal(DiagnosticCode.E1001, ex1.Diagnostic.Code);

        // "5." lexes as number "5" then unexpected '.'? spec says "5." fail lexically/syntactically;
        // lexer will see '5' then '.' not followed by digit so number ends at '5', '.' becomes unexpected char.
        var ex2 = Assert.Throws<LexException>(() => Lex("5."));
        Assert.Equal(DiagnosticCode.E1001, ex2.Diagnostic.Code);
    }

    [Fact]
    public void L16_StringWithArabicAndEmoji()
    {
        var t = Lex("\"مرحبا 😀\"")[0];
        Assert.Equal(TokenKind.StringLiteral, t.Kind);
        Assert.Equal("مرحبا 😀", t.Literal);
    }

    [Fact]
    public void L17_Escapes()
    {
        Assert.Equal("\n", Lex("\"\\n\"")[0].Literal);
        Assert.Equal("\t", Lex("\"\\t\"")[0].Literal);
        Assert.Equal("\r", Lex("\"\\r\"")[0].Literal);
        Assert.Equal("\\", Lex("\"\\\\\"")[0].Literal);
        Assert.Equal("\"", Lex("\"\\\"\"")[0].Literal);
    }

    [Fact]
    public void L18_UnknownEscape()
    {
        var ex = Assert.Throws<LexException>(() => Lex("\"\\q\""));
        Assert.Equal(DiagnosticCode.E1004, ex.Diagnostic.Code);
    }

    [Fact]
    public void L19_RawNewlineInString()
    {
        var ex = Assert.Throws<LexException>(() => Lex("\"a\nb\""));
        Assert.Equal(DiagnosticCode.E1003, ex.Diagnostic.Code);
    }

    [Fact]
    public void L20_UnterminatedStringAtEof()
    {
        var ex = Assert.Throws<LexException>(() => Lex("\"abc"));
        Assert.Equal(DiagnosticCode.E1003, ex.Diagnostic.Code);
    }

    [Fact]
    public void L21_TwoCharacterOperators()
    {
        var kinds = Lex("== != <= >=").Where(t => t.Kind != TokenKind.EndOfFile).Select(t => t.Kind).ToArray();
        Assert.Equal(new[] { TokenKind.EqualEqual, TokenKind.BangEqual, TokenKind.LessEqual, TokenKind.GreaterEqual }, kinds);
    }

    [Fact]
    public void L22_UnexpectedCharacters()
    {
        foreach (var ch in new[] { '!', '?', '&', '|', '[' })
        {
            var ex = Assert.Throws<LexException>(() => Lex(ch.ToString()));
            Assert.Equal(DiagnosticCode.E1001, ex.Diagnostic.Code);
        }
    }

    [Fact]
    public void L23_L24_UnicodeInStrings_AndScalarColumns()
    {
        var srcEm = new SourceText("e.msf", "\"😀\"");
        var tokensEm = Lexer.Lex(srcEm);
        Assert.Equal(TokenKind.StringLiteral, tokensEm[0].Kind);
        Assert.Equal("😀", tokensEm[0].Literal);

        var srcAr = new SourceText("ar.msf", "print(\"مرحبا\");");
        var tokensAr = Lexer.Lex(srcAr);
        var str = tokensAr[2];
        Assert.Equal(TokenKind.StringLiteral, str.Kind);
        Assert.Equal("مرحبا", str.Literal);
        // scalar column of string token start: p,r,i,n,t,(," => 7th scalar is quote at column 7
        Assert.Equal(new SourcePosition(1, 7), srcAr.GetPosition(str.Span.Start));
    }

    [Fact]
    public void L25_TabCountsAsOneColumn_ViaSourcePosition()
    {
        var src = new SourceText("t.msf", "\t1");
        var tokens = Lexer.Lex(src);
        Assert.Equal(new SourcePosition(1, 2), src.GetPosition(tokens[0].Span.Start));
    }

    [Fact]
    public void L26_CrlfCountsAsOneLine()
    {
        var src = new SourceText("crlf.msf", "1\r\n2");
        var tokens = Lexer.Lex(src);
        Assert.Equal(new SourcePosition(2, 1), src.GetPosition(tokens[1].Span.Start));
    }

    [Fact]
    public void L27_EofSpan_ZeroLength()
    {
        var tokens = Lex("ab");
        var eof = tokens[^1];
        Assert.Equal(2, eof.Span.Start);
        Assert.Equal(2, eof.Span.End);
    }

    [Fact]
    public void L28_PrintIsIdentifier()
    {
        var t = Lex("print")[0];
        Assert.Equal(TokenKind.Identifier, t.Kind);
    }

    [Fact]
    public void L29_CommentsProduceNoTokens()
    {
        var tokens = Lex("// a\n/* b */\n");
        Assert.Single(tokens);
        Assert.Equal(TokenKind.EndOfFile, tokens[0].Kind);
    }

    [Fact]
    public void L30_StringContainingCommentMarkers()
    {
        var t = Lex("\"// /* */\"")[0];
        Assert.Equal("// /* */", t.Literal);
    }

    [Fact]
    public void L31_EmptyString()
    {
        var t = Lex("\"\"")[0];
        Assert.Equal(string.Empty, t.Literal);
    }

    [Fact]
    public void L32_StringWithEscapedQuote()
    {
        var t = Lex("\"a\\\"b\"")[0];
        Assert.Equal("a\"b", t.Literal);
    }

    [Fact]
    public void L35_BackslashOutsideString()
    {
        var ex = Assert.Throws<LexException>(() => Lex("\\"));
        Assert.Equal(DiagnosticCode.E1001, ex.Diagnostic.Code);
    }

    private static IReadOnlyList<Token> Lex(string text) => Lexer.Lex(new SourceText("test.msf", text));
}
