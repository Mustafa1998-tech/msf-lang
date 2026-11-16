using MsfLang.Core.Lexing;
using MsfLang.Core.Source;

namespace MsfLang.Core.Syntax;

public abstract record AstNode(TextSpan Span);

public sealed record ProgramNode(IReadOnlyList<Declaration> Declarations, TextSpan Span) : AstNode(Span);

public abstract record Declaration(TextSpan Span) : AstNode(Span);
public abstract record Statement(TextSpan Span) : Declaration(Span);

public sealed record FunctionDecl(Token Name, IReadOnlyList<Token> Parameters, BlockStmt Body, TextSpan Span) : Declaration(Span);
public sealed record VarDecl(bool IsMutable, Token Name, Expression Initializer, TextSpan Span) : Declaration(Span);

public sealed record BlockStmt(IReadOnlyList<Declaration> Declarations, TextSpan Span) : Statement(Span);
public sealed record IfStmt(Expression Condition, BlockStmt ThenBranch, Statement? ElseBranch, TextSpan Span) : Statement(Span);
public sealed record ReturnStmt(Token Keyword, Expression? Value, TextSpan Span) : Statement(Span);
public sealed record AssignStmt(Token Name, Expression Value, TextSpan Span) : Statement(Span);
public sealed record ExprStmt(Expression Expression, TextSpan Span) : Statement(Span);

public sealed record ListLiteralExpr(IReadOnlyList<Expression> Items, TextSpan Span) : Expression(Span);
public sealed record IndexExpr(Expression Target, Expression Index, TextSpan Span) : Expression(Span);
public sealed record WhileStmt(Expression Condition, BlockStmt Body, TextSpan Span) : Statement(Span);

public sealed record AssignIndexStmt(Expression Target, Expression Index, Expression Value, TextSpan Span) : Statement(Span);

public abstract record Expression(TextSpan Span) : AstNode(Span);

public sealed record LiteralExpr(object? Value, Token Token, TextSpan Span) : Expression(Span);
public sealed record NameExpr(Token Name, TextSpan Span) : Expression(Span);
public sealed record GroupingExpr(Expression Expression, TextSpan Span) : Expression(Span);
public sealed record UnaryExpr(Token Operator, Expression Right, TextSpan Span) : Expression(Span);
public sealed record BinaryExpr(Expression Left, Token Operator, Expression Right, TextSpan Span) : Expression(Span);
public sealed record CallExpr(Token Callee, IReadOnlyList<Expression> Arguments, TextSpan Span) : Expression(Span);
