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
