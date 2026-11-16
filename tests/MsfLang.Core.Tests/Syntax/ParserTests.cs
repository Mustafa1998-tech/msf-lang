using MsfLang.Core.Diagnostics;
using MsfLang.Core.Lexing;
using MsfLang.Core.Source;
using MsfLang.Core.Syntax;
using Xunit;

namespace MsfLang.Core.Tests.Syntax;

public sealed class ParserTests
{
    [Fact]
    public void P01_EmptyProgram()
    {
        var program = Parse("");
        Assert.Empty(program.Declarations);
    }

    [Fact]
    public void P02_VariableDeclarations()
    {
        var program = Parse("let a = 1;\nvar b = a;");
        Assert.Collection(program.Declarations,
            d => Assert.False(((VarDecl)d).IsMutable),
            d => Assert.True(((VarDecl)d).IsMutable));
    }

    [Fact]
    public void P03_AssignmentStatement()
    {
        var stmt = Assert.IsType<AssignStmt>(Assert.Single(Parse("x = 5;").Declarations));
        Assert.Equal("x", stmt.Name.Lexeme);
    }

    [Fact]
    public void P04_ExpressionStatement()
    {
        var stmt = Assert.IsType<ExprStmt>(Assert.Single(Parse("1 + 2;").Declarations));
        Assert.IsType<BinaryExpr>(stmt.Expression);
    }

    [Fact]
    public void P05_FunctionDeclaration_Params()
    {
        var f0 = Assert.IsType<FunctionDecl>(Parse("fn f() { return; }").Declarations[0]);
        Assert.Empty(f0.Parameters);
        var f2 = Assert.IsType<FunctionDecl>(Parse("fn add(a,b){return a;}").Declarations[0]);
        Assert.Equal(2, f2.Parameters.Count);
    }

    [Fact]
    public void P06_CallArguments()
    {
        var expr = Assert.IsType<ExprStmt>(Parse("f(); g(1,2,3);").Declarations[1]).Expression;
        var call = Assert.IsType<CallExpr>(expr);
        Assert.Equal(3, call.Arguments.Count);
    }

    [Fact]
    public void P08_Grouping()
    {
        var expr = Assert.IsType<ExprStmt>(Parse("(1 + 2);").Declarations[0]).Expression;
        Assert.IsType<GroupingExpr>(expr);
    }

    [Fact]
    public void P09_P10_IfElse_AndDanglingElse()
    {
        var code = "if (a) { if (b) { x = 1; } else { x = 2; } }";
        var ifs = Assert.IsType<IfStmt>(Assert.Single(Parse(code).Declarations));
        var inner = Assert.IsType<IfStmt>(Assert.Single(ifs.ThenBranch.Declarations));
        Assert.NotNull(inner.ElseBranch);
        Assert.Null(ifs.ElseBranch);
    }

    [Fact]
    public void P11_StandaloneBlock()
    {
        var block = Assert.IsType<BlockStmt>(Assert.Single(Parse("{ let x = 1; }").Declarations));
        Assert.Single(block.Declarations);
    }

    [Fact]
    public void P12_ReturnWithAndWithoutExpression()
    {
        var prog = Parse("fn f(){ return; } fn g(){ return 1; }");
        Assert.Null(((ReturnStmt)((FunctionDecl)prog.Declarations[0]).Body.Declarations[0]).Value);
        Assert.NotNull(((ReturnStmt)((FunctionDecl)prog.Declarations[1]).Body.Declarations[0]).Value);
    }

    [Fact]
    public void P13_UnaryChains()
    {
        var e1 = Assert.IsType<ExprStmt>(Parse("--1;").Declarations[0]).Expression;
        Assert.IsType<UnaryExpr>(((UnaryExpr)e1).Right);
        var e2 = Assert.IsType<ExprStmt>(Parse("not not true;").Declarations[0]).Expression;
        Assert.IsType<UnaryExpr>(((UnaryExpr)e2).Right);
    }

    [Fact]
    public void P14_P15_Precedence()
    {
        var addMul = Assert.IsType<BinaryExpr>(Assert.IsType<ExprStmt>(Parse("1 + 2 * 3;").Declarations[0]).Expression);
        Assert.Equal(TokenKind.Plus, addMul.Operator.Kind);
        Assert.Equal(TokenKind.Star, Assert.IsType<BinaryExpr>(addMul.Right).Operator.Kind);

        var cmpAnd = Assert.IsType<BinaryExpr>(Assert.IsType<ExprStmt>(Parse("1 < 2 and 3 == 3;").Declarations[0]).Expression);
        Assert.Equal(TokenKind.And, cmpAnd.Operator.Kind);
    }

    [Fact]
    public void P16_LeftAssociativity()
    {
        var sub = Assert.IsType<BinaryExpr>(Assert.IsType<ExprStmt>(Parse("1 - 2 - 3;").Declarations[0]).Expression);
        Assert.Equal(TokenKind.Minus, sub.Operator.Kind);
        Assert.Equal(TokenKind.Minus, Assert.IsType<BinaryExpr>(sub.Left).Operator.Kind);
    }

    [Fact]
    public void P17_P18_MissingSemicolon_AndEof()
    {
        var ex = AssertThrowsSyntax("let x = 1");
        Assert.Equal(DiagnosticCode.E2010, ex.Diagnostic.Code);
    }

    [Fact]
    public void P19_MissingParen()
    {
        var ex = AssertThrowsSyntax("print(1;");
        Assert.Equal(DiagnosticCode.E2003, ex.Diagnostic.Code);
    }

    [Fact]
    public void P20_MissingBrace()
    {
        var ex = AssertThrowsSyntax("if (true) print(1);");
        Assert.Equal(DiagnosticCode.E2004, ex.Diagnostic.Code);
    }

    [Fact]
    public void P21_MissingClosingBraceAtEof()
    {
        var ex = AssertThrowsSyntax("{ let x = 1;");
        Assert.Equal(DiagnosticCode.E2010, ex.Diagnostic.Code);
    }

    [Fact]
    public void P23_MissingParenAfterIf()
    {
        var ex = AssertThrowsSyntax("if true) { }");
        Assert.Equal(DiagnosticCode.E2007, ex.Diagnostic.Code);
    }

    [Fact]
    public void P24_MissingEqualsInDeclaration()
    {
        var ex = AssertThrowsSyntax("let x 1;");
        Assert.Equal(DiagnosticCode.E2008, ex.Diagnostic.Code);
    }

    [Fact]
    public void P25_LoneSemicolon()
    {
        var ex = AssertThrowsSyntax(";");
        Assert.Equal(DiagnosticCode.E2009, ex.Diagnostic.Code);
    }

    [Fact]
    public void P28_InvalidAssignmentTarget()
    {
        var ex = AssertThrowsSyntax("x + 1 = 2;");
        Assert.Equal(DiagnosticCode.E2011, ex.Diagnostic.Code);
    }

    [Fact]
    public void P32_P33_NestingLimit()
    {
        var ok = "(" + string.Concat(Enumerable.Repeat("(", 255)) + "1" + string.Concat(Enumerable.Repeat(")", 255)) + ");";
        Parse(ok);

        // 257 nested parens/groups: construct 257 '(' before 1
        var deep = string.Concat(Enumerable.Repeat("(", 257)) + "1" + string.Concat(Enumerable.Repeat(")", 257)) + ";";
        var ex = AssertThrowsSyntax(deep);
        Assert.Equal(DiagnosticCode.E2013, ex.Diagnostic.Code);
    }

    [Fact]
    public void P34_Spans()
    {
        var code = "let x = 1;";
        var decl = Assert.IsType<VarDecl>(Assert.Single(Parse(code).Declarations));
        Assert.Equal(0, decl.Span.Start);
        Assert.Equal(code.Length, decl.Span.End);
    }

    private static ProgramNode Parse(string text)
    {
        var source = new SourceText("test.msf", text);
        var tokens = Lexer.Lex(source);
        return Parser.Parse(tokens);
    }

    private static SyntaxException AssertThrowsSyntax(string text)
    {
        var ex = Assert.Throws<SyntaxException>(() => Parse(text));
        return ex;
    }
}
