using MsfLang.Core.Diagnostics;
using MsfLang.Core.Lexing;
using MsfLang.Core.Source;

namespace MsfLang.Core.Syntax;

public sealed class Parser
{
    private const int MaxNestingDepth = 256;
    private readonly IReadOnlyList<Token> _tokens;
    private int _current;
    private int _nesting;

    public Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
    }

    public static ProgramNode Parse(IReadOnlyList<Token> tokens) => new Parser(tokens).ParseProgram();

    public ProgramNode ParseProgram()
    {
        var declarations = new List<Declaration>();
        while (!IsAtEnd())
        {
            declarations.Add(ParseDeclaration());
        }

        var start = declarations.Count > 0 ? declarations[0].Span.Start : 0;
        var end = declarations.Count > 0 ? declarations[^1].Span.End : 0;
        return new ProgramNode(declarations, new TextSpan(start, end));
    }

    private Declaration ParseDeclaration()
    {
        if (Match(TokenKind.Import))
        {
            return ParseImportDecl(Previous());
        }

        var isExported = Match(TokenKind.Export);
        if (isExported && !Check(TokenKind.Fn) && !Check(TokenKind.Let) && !Check(TokenKind.Var))
        {
            throw Error(Peek(), DiagnosticCode.E2015, "expected 'fn', 'var', or 'let' after 'export'");
        }

        if (Match(TokenKind.Fn))
        {
            var fnToken = Previous();
            return ParseFunctionDecl(fnToken, isExported);
        }

        if (Match(TokenKind.Let, TokenKind.Var))
        {
            var varToken = Previous();
            return ParseVarDecl(varToken, isExported);
        }

        return ParseStatement();
    }

    private ImportDecl ParseImportDecl(Token keyword)
    {
        if (!Check(TokenKind.StringLiteral))
        {
            throw Error(Peek(), DiagnosticCode.E2014, "expected import path string");
        }

        var path = Advance();
        var semi = Consume(TokenKind.Semicolon, DiagnosticCode.E2002, "expected ';'");
        return new ImportDecl(keyword, path, Span(keyword.Span.Start, semi.Span.End));
    }

    private FunctionDecl ParseFunctionDecl(Token fnToken, bool isExported)
    {
        var name = ConsumeIdentifier("expected identifier");
        Consume(TokenKind.LeftParen, DiagnosticCode.E2007, "expected '('");
        var parameters = new List<Token>();
        if (!Check(TokenKind.RightParen))
        {
            do
            {
                parameters.Add(ConsumeIdentifier("expected identifier"));
            } while (Match(TokenKind.Comma));
        }

        Consume(TokenKind.RightParen, DiagnosticCode.E2003, "expected ')'");
        if (!Check(TokenKind.LeftBrace))
        {
            throw Error(Peek(), DiagnosticCode.E2012, "expected function body");
        }

        var body = ParseBlock();
        return new FunctionDecl(name, parameters, body, isExported, Span(fnToken.Span.Start, body.Span.End));
    }

    private VarDecl ParseVarDecl(Token keyword, bool isExported)
    {
        var name = ConsumeIdentifier("expected identifier");
        Consume(TokenKind.Equal, DiagnosticCode.E2008, "expected '='");
        var initializer = ParseExpression();
        var semi = Consume(TokenKind.Semicolon, DiagnosticCode.E2002, "expected ';'");
        return new VarDecl(keyword.Kind == TokenKind.Var, name, initializer, isExported, Span(keyword.Span.Start, semi.Span.End));
    }

    private Statement ParseStatement()
    {
        if (Match(TokenKind.If))
        {
            return ParseIfStmt(Previous());
        }

        if (Match(TokenKind.While))
        {
            return ParseWhileStmt(Previous());
        }

        if (Match(TokenKind.Break))
        {
            var keyword = Previous();
            var breakSemi = Consume(TokenKind.Semicolon, DiagnosticCode.E2002, "expected ';'");
            return new BreakStmt(Span(keyword.Span.Start, breakSemi.Span.End));
        }

        if (Match(TokenKind.Continue))
        {
            var keyword = Previous();
            var continueSemi = Consume(TokenKind.Semicolon, DiagnosticCode.E2002, "expected ';'");
            return new ContinueStmt(Span(keyword.Span.Start, continueSemi.Span.End));
        }

        if (Match(TokenKind.Return))
        {
            return ParseReturnStmt(Previous());
        }

        if (Check(TokenKind.LeftBrace))
        {
            return ParseBlock();
        }

        if (Check(TokenKind.Identifier) && CheckNext(TokenKind.Equal))
        {
            return ParseAssignStmt();
        }

        if (Check(TokenKind.Identifier) && CheckNext(TokenKind.LeftBracket) && IsIndexAssignmentAhead())
        {
            return ParseAssignIndexStmt();
        }

        // detect invalid assignment target like `x + 1 = 2;`
        var expr = ParseExpression();
        if (Match(TokenKind.Equal))
        {
            throw new SyntaxException(new Diagnostic(
                DiagnosticCode.E2011,
                DiagnosticPhase.Syntax,
                "invalid assignment target",
                Previous().Span));
        }

        var semi = Consume(TokenKind.Semicolon, DiagnosticCode.E2002, "expected ';'");
        return new ExprStmt(expr, Span(expr.Span.Start, semi.Span.End));
    }

    private AssignIndexStmt ParseAssignIndexStmt()
    {
        var target = ParseCallOrPrimary(); // parses identifier and existing postfix ops (should not include list access before this point)
        if (target is not IndexExpr index)
        {
            throw Error(Peek(), DiagnosticCode.E2011, "invalid assignment target");
        }
        if (!Match(TokenKind.Equal))
        {
            throw Error(Peek(), DiagnosticCode.E2008, "expected '='");
        }
        var value = ParseExpression();
        var semi = Consume(TokenKind.Semicolon, DiagnosticCode.E2002, "expected ';'");
        return new AssignIndexStmt(index.Target, index.Index, value, Span(target.Span.Start, semi.Span.End));
    }

    private WhileStmt ParseWhileStmt(Token whileToken)
    {
        Consume(TokenKind.LeftParen, DiagnosticCode.E2007, "expected '('");
        EnterNesting(Peek());
        var condition = ParseExpression();
        ExitNesting();
        Consume(TokenKind.RightParen, DiagnosticCode.E2003, "expected ')'");
        if (!Check(TokenKind.LeftBrace))
        {
            throw Error(Peek(), DiagnosticCode.E2004, "expected '{'");
        }

        var body = ParseBlock();
        return new WhileStmt(condition, body, Span(whileToken.Span.Start, body.Span.End));
    }

    private bool IsIndexAssignmentAhead()
    {
        // look for: Identifier [ expression ] =
        var i = _current; // currently at identifier
        if (i >= _tokens.Count || _tokens[i].Kind != TokenKind.Identifier) return false;
        i++;
        if (i >= _tokens.Count || _tokens[i].Kind != TokenKind.LeftBracket) return false;
        var depth = 0;
        for (; i < _tokens.Count; i++)
        {
            var k = _tokens[i].Kind;
            if (k == TokenKind.LeftBracket) depth++;
            else if (k == TokenKind.RightBracket) { depth--; if (depth == 0) { i++; break; } }
        }
        if (i >= _tokens.Count) return false;
        return _tokens[i].Kind == TokenKind.Equal;
    }

    private IfStmt ParseIfStmt(Token ifToken)
    {
        Consume(TokenKind.LeftParen, DiagnosticCode.E2007, "expected '('");
        EnterNesting(Peek());
        var condition = ParseExpression();
        ExitNesting();
        Consume(TokenKind.RightParen, DiagnosticCode.E2003, "expected ')'");
        if (!Check(TokenKind.LeftBrace))
        {
            throw Error(Peek(), DiagnosticCode.E2004, "expected '{'");
        }

        var thenBranch = ParseBlock();
        Statement? elseBranch = null;
        if (Match(TokenKind.Else))
        {
            if (Match(TokenKind.If))
            {
                EnterNesting(Previous());
                elseBranch = ParseIfStmt(Previous());
                ExitNesting();
            }
            else if (Check(TokenKind.LeftBrace))
            {
                elseBranch = ParseBlock();
            }
            else
            {
                throw Error(Peek(), DiagnosticCode.E2004, "expected '{'");
            }
        }

        var end = elseBranch?.Span.End ?? thenBranch.Span.End;
        return new IfStmt(condition, thenBranch, elseBranch, Span(ifToken.Span.Start, end));
    }

    private ReturnStmt ParseReturnStmt(Token keyword)
    {
        Expression? value = null;
        if (!Check(TokenKind.Semicolon))
        {
            value = ParseExpression();
        }

        var semi = Consume(TokenKind.Semicolon, DiagnosticCode.E2002, "expected ';'");
        return new ReturnStmt(keyword, value, Span(keyword.Span.Start, semi.Span.End));
    }

    private BlockStmt ParseBlock()
    {
        var open = Consume(TokenKind.LeftBrace, DiagnosticCode.E2004, "expected '{'");
        EnterNesting(open);
        var declarations = new List<Declaration>();
        while (!Check(TokenKind.RightBrace) && !IsAtEnd())
        {
            declarations.Add(ParseDeclaration());
        }

        var close = Consume(TokenKind.RightBrace, DiagnosticCode.E2005, "expected '}'");
        ExitNesting();
        return new BlockStmt(declarations, Span(open.Span.Start, close.Span.End));
    }

    private AssignStmt ParseAssignStmt()
    {
        var name = ConsumeIdentifier("expected identifier");
        Consume(TokenKind.Equal, DiagnosticCode.E2008, "expected '='");
        var value = ParseExpression();
        var semi = Consume(TokenKind.Semicolon, DiagnosticCode.E2002, "expected ';'");
        return new AssignStmt(name, value, Span(name.Span.Start, semi.Span.End));
    }

    private Expression ParseExpression() => ParseLogicOr();

    private Expression ParseLogicOr()
    {
        var expr = ParseLogicAnd();
        while (Match(TokenKind.Or))
        {
            var op = Previous();
            var right = ParseLogicAnd();
            expr = new BinaryExpr(expr, op, right, Span(expr.Span.Start, right.Span.End));
        }

        return expr;
    }

    private Expression ParseLogicAnd()
    {
        var expr = ParseEquality();
        while (Match(TokenKind.And))
        {
            var op = Previous();
            var right = ParseEquality();
            expr = new BinaryExpr(expr, op, right, Span(expr.Span.Start, right.Span.End));
        }

        return expr;
    }

    private Expression ParseEquality()
    {
        var expr = ParseComparison();
        while (Match(TokenKind.EqualEqual, TokenKind.BangEqual))
        {
            var op = Previous();
            var right = ParseComparison();
            expr = new BinaryExpr(expr, op, right, Span(expr.Span.Start, right.Span.End));
        }

        return expr;
    }

    private Expression ParseComparison()
    {
        var expr = ParseTerm();
        while (Match(TokenKind.Less, TokenKind.LessEqual, TokenKind.Greater, TokenKind.GreaterEqual))
        {
            var op = Previous();
            var right = ParseTerm();
            expr = new BinaryExpr(expr, op, right, Span(expr.Span.Start, right.Span.End));
        }

        return expr;
    }

    private Expression ParseTerm()
    {
        var expr = ParseFactor();
        while (Match(TokenKind.Plus, TokenKind.Minus))
        {
            var op = Previous();
            var right = ParseFactor();
            expr = new BinaryExpr(expr, op, right, Span(expr.Span.Start, right.Span.End));
        }

        return expr;
    }

    private Expression ParseFactor()
    {
        var expr = ParseUnary();
        while (Match(TokenKind.Star, TokenKind.Slash, TokenKind.Percent))
        {
            var op = Previous();
            var right = ParseUnary();
            expr = new BinaryExpr(expr, op, right, Span(expr.Span.Start, right.Span.End));
        }

        return expr;
    }

    private Expression ParseUnary()
    {
        if (Match(TokenKind.Minus, TokenKind.Not))
        {
            var op = Previous();
            var right = ParseUnary();
            return new UnaryExpr(op, right, Span(op.Span.Start, right.Span.End));
        }

        return ParseCallOrPrimary();
    }

    private Expression ParseCallOrPrimary()
    {
        if (Check(TokenKind.Identifier))
        {
            var name = Advance();
            if (Match(TokenKind.Dot))
            {
                var member = ConsumeIdentifier("expected member name after '.'");
                if (Match(TokenKind.LeftParen))
                {
                    return FinishQualifiedCall(name, member);
                }

                return ParsePostfix(new MemberExpr(name, member, Span(name.Span.Start, member.Span.End)));
            }

            if (Match(TokenKind.LeftParen))
            {
                return FinishCall(name);
            }

            var expr = (Expression)new NameExpr(name, name.Span);
            return ParsePostfix(expr);
        }

        var primary = ParsePrimary();
        return ParsePostfix(primary);
    }

    private Expression ParsePostfix(Expression expr)
    {
        while (Match(TokenKind.LeftBracket))
        {
            var index = ParseExpression();
            var close = Consume(TokenKind.RightBracket, DiagnosticCode.E2003, "expected ']'");
            expr = new IndexExpr(expr, index, Span(expr.Span.Start, close.Span.End));
        }

        return expr;
    }

    private CallExpr FinishCall(Token callee)
    {
        var args = new List<Expression>();
        if (!Check(TokenKind.RightParen))
        {
            do
            {
                args.Add(ParseExpression());
            } while (Match(TokenKind.Comma));
        }

        var close = Consume(TokenKind.RightParen, DiagnosticCode.E2003, "expected ')'");
        return new CallExpr(callee, args, Span(callee.Span.Start, close.Span.End));
    }

    private QualifiedCallExpr FinishQualifiedCall(Token qualifier, Token member)
    {
        var args = new List<Expression>();
        if (!Check(TokenKind.RightParen))
        {
            do
            {
                args.Add(ParseExpression());
            } while (Match(TokenKind.Comma));
        }

        var close = Consume(TokenKind.RightParen, DiagnosticCode.E2003, "expected ')'");
        return new QualifiedCallExpr(qualifier, member, args, Span(qualifier.Span.Start, close.Span.End));
    }

    private Expression ParsePrimary()
    {
        if (Match(TokenKind.Number, TokenKind.StringLiteral))
        {
            var token = Previous();
            return new LiteralExpr(token.Literal, token, token.Span);
        }

        if (Match(TokenKind.True))
        {
            var t = Previous();
            return new LiteralExpr(true, t, t.Span);
        }

        if (Match(TokenKind.False))
        {
            var t = Previous();
            return new LiteralExpr(false, t, t.Span);
        }

        if (Match(TokenKind.Nil))
        {
            var t = Previous();
            return new LiteralExpr(null, t, t.Span);
        }

        if (Match(TokenKind.Map))
        {
            var keyword = Previous();
            var open = Consume(TokenKind.LeftBrace, DiagnosticCode.E2004, "expected '{'");
            EnterNesting(open);
            var items = new List<MapEntry>();
            if (!Check(TokenKind.RightBrace))
            {
                do
                {
                    var key = Consume(TokenKind.StringLiteral, DiagnosticCode.E2006, "expected map key string");
                    Consume(TokenKind.Colon, DiagnosticCode.E2008, "expected ':'");
                    var value = ParseExpression();
                    items.Add(new MapEntry(key, value));
                } while (Match(TokenKind.Comma));
            }

            var close = Consume(TokenKind.RightBrace, DiagnosticCode.E2005, "expected '}'");
            ExitNesting();
            return new MapLiteralExpr(items, Span(keyword.Span.Start, close.Span.End));
        }

        if (Match(TokenKind.LeftBracket))
        {
            var open = Previous();
            EnterNesting(open);
            var items = new List<Expression>();
            if (!Check(TokenKind.RightBracket))
            {
                do
                {
                    items.Add(ParseExpression());
                } while (Match(TokenKind.Comma));
            }

            var close = Consume(TokenKind.RightBracket, DiagnosticCode.E2003, "expected ']'");
            ExitNesting();
            return new ListLiteralExpr(items, Span(open.Span.Start, close.Span.End));
        }

        if (Match(TokenKind.LeftParen))
        {
            var open = Previous();
            EnterNesting(open);
            var expr = ParseExpression();
            var close = Consume(TokenKind.RightParen, DiagnosticCode.E2003, "expected ')'");
            ExitNesting();
            return new GroupingExpr(expr, Span(open.Span.Start, close.Span.End));
        }

        if (Match(TokenKind.Identifier))
        {
            // Should be handled by ParseCallOrPrimary, but keep safe.
            var t = Previous();
            return new NameExpr(t, t.Span);
        }

        throw ExpectedExpression(Peek());
    }

    private Token Consume(TokenKind kind, string code, string message)
    {
        if (Check(kind))
        {
            return Advance();
        }

        throw Error(Peek(), code, message);
    }

    private Token ConsumeIdentifier(string message)
    {
        if (Check(TokenKind.Identifier))
        {
            return Advance();
        }

        if (IsAtEndToken(Peek()))
        {
            throw Error(Peek(), DiagnosticCode.E2010, "unexpected end of input");
        }

        // keywords used where identifier required
        throw Error(Peek(), DiagnosticCode.E2006, "expected identifier");
    }

    private bool Match(params TokenKind[] kinds)
    {
        foreach (var kind in kinds)
        {
            if (Check(kind))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenKind kind) => !IsAtEndToken(Peek()) && Peek().Kind == kind;

    private bool CheckNext(TokenKind kind)
    {
        if (_current + 1 >= _tokens.Count)
        {
            return false;
        }

        return _tokens[_current + 1].Kind == kind;
    }

    private Token Advance()
    {
        if (!IsAtEndToken(Peek()))
        {
            _current++;
        }

        return Previous();
    }

    private bool IsAtEnd() => IsAtEndToken(Peek());
    private Token Peek() => _tokens[_current];
    private Token Previous() => _tokens[_current - 1];

    private static bool IsAtEndToken(Token token) => token.Kind == TokenKind.EndOfFile;

    private void EnterNesting(Token token)
    {
        _nesting++;
        if (_nesting > MaxNestingDepth)
        {
            throw new SyntaxException(new Diagnostic(
                DiagnosticCode.E2013,
                DiagnosticPhase.Syntax,
                "maximum nesting depth (256) exceeded",
                token.Span));
        }
    }

    private void ExitNesting()
    {
        if (_nesting > 0)
        {
            _nesting--;
        }
    }

    private static TextSpan Span(int start, int end) => new(start, end);

    private static SyntaxException ExpectedExpression(Token token)
    {
        if (IsAtEndToken(token))
        {
            return Error(token, DiagnosticCode.E2010, "unexpected end of input");
        }

        if (token.Kind == TokenKind.Semicolon)
        {
            // lone ';' or unexpected semicolon in expression context
            return Error(token, DiagnosticCode.E2009, "unexpected token ';'");
        }

        return Error(token, DiagnosticCode.E2001, "expected expression");
    }

    private static SyntaxException Error(Token token, string code, string baseMessage)
    {
        if (IsAtEndToken(token))
        {
            return new SyntaxException(new Diagnostic(
                DiagnosticCode.E2010,
                DiagnosticPhase.Syntax,
                "unexpected end of input",
                token.Span));
        }

        var message = code switch
        {
            DiagnosticCode.E2001 => $"expected expression, found '{TokenDisplay(token)}'",
            DiagnosticCode.E2002 => $"expected ';', found '{TokenDisplay(token)}'",
            DiagnosticCode.E2003 => $"expected ')', found '{TokenDisplay(token)}'",
            DiagnosticCode.E2004 => $"expected '{{', found '{TokenDisplay(token)}'",
            DiagnosticCode.E2005 => $"expected '}}', found '{TokenDisplay(token)}'",
            DiagnosticCode.E2006 => $"expected identifier, found '{TokenDisplay(token)}'",
            DiagnosticCode.E2007 => $"expected '(', found '{TokenDisplay(token)}'",
            DiagnosticCode.E2008 => $"expected '=', found '{TokenDisplay(token)}'",
            DiagnosticCode.E2009 => $"unexpected token '{TokenDisplay(token)}'",
            DiagnosticCode.E2011 => "invalid assignment target",
            DiagnosticCode.E2012 => "expected function body",
            _ => baseMessage
        };

        return new SyntaxException(new Diagnostic(code, DiagnosticPhase.Syntax, message, token.Span));
    }

    private static string TokenDisplay(Token token)
    {
        return token.Kind switch
        {
            TokenKind.EndOfFile => "end of input",
            TokenKind.StringLiteral => token.Lexeme,
            _ => token.Lexeme.Length > 0 ? token.Lexeme : token.Kind.ToString()
        };
    }
}
