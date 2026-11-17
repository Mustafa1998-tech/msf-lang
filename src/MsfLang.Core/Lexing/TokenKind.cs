namespace MsfLang.Core.Lexing;

public enum TokenKind
{
    // Keywords
    Let,
    Var,
    Fn,
    If,
    Else,
    While,
    Break,
    Continue,
    Map,
    Import,
    Export,
    Return,
    True,
    False,
    Nil,
    And,
    Or,
    Not,

    // Identifiers / literals
    Identifier,
    Number,
    StringLiteral,

    // Punctuation / operators
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    LeftBracket,
    RightBracket,
    Colon,
    Dot,
    Comma,
    Semicolon,
    Plus,
    Minus,
    Star,
    Slash,
    Percent,
    Equal,
    EqualEqual,
    BangEqual,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,

    EndOfFile
}
