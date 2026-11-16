namespace MsfLang.Core.Lexing;

public enum TokenKind
{
    // Keywords
    Let,
    Var,
    Fn,
    If,
    Else,
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
