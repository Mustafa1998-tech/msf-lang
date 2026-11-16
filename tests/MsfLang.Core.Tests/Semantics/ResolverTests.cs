using MsfLang.Core.Diagnostics;
using MsfLang.Core.Lexing;
using MsfLang.Core.Semantics;
using MsfLang.Core.Source;
using MsfLang.Core.Syntax;
using Xunit;

namespace MsfLang.Core.Tests.Semantics;

public sealed class ResolverTests
{
    [Fact]
    public void S01_ValidForwardUseOfGlobalDeclaredBeforeFunction()
    {
        ResolveOk("let base = 10;\nfn add(n){ return n + base; }\nprint(add(5));");
    }

    [Fact]
    public void S02_LaterGlobalInsideFunction_IsE3001()
    {
        var ex = ResolveErr("fn later(){ return x; }\nlet x = 1;");
        Assert.Equal(DiagnosticCode.E3001, ex.Diagnostic.Code);
    }

    [Fact]
    public void S03_CallBeforeFunctionDeclaration_IsE3001()
    {
        var ex = ResolveErr("print(f());\nfn f(){ return 1; }");
        Assert.Equal(DiagnosticCode.E3001, ex.Diagnostic.Code);
    }

    [Fact]
    public void S04_DirectRecursionResolves()
    {
        ResolveOk("fn fact(n){ if(n<=1){return 1;} return n * fact(n-1); }");
    }

    [Fact]
    public void S06_UndefinedName()
    {
        var ex = ResolveErr("print(total);");
        Assert.Equal(DiagnosticCode.E3001, ex.Diagnostic.Code);
    }

    [Fact]
    public void S07_DuplicateInSameScope()
    {
        var ex = ResolveErr("let x = 1; let x = 2;");
        Assert.Equal(DiagnosticCode.E3002, ex.Diagnostic.Code);
    }

    [Fact]
    public void S08_ShadowingInChildBlockAllowed()
    {
        ResolveOk("let x = 1; { let x = 2; print(x); } print(x);");
    }

    [Fact]
    public void S09_DuplicateParameter()
    {
        var ex = ResolveErr("fn f(a,a){ return a; }");
        Assert.Equal(DiagnosticCode.E3003, ex.Diagnostic.Code);
    }

    [Fact]
    public void S11_RedeclarePrint()
    {
        var ex = ResolveErr("let print = 1;");
        Assert.Equal(DiagnosticCode.E3004, ex.Diagnostic.Code);
    }

    [Fact]
    public void S12_AssignToLet()
    {
        var ex = ResolveErr("let x = 1; x = 2;");
        Assert.Equal(DiagnosticCode.E3005, ex.Diagnostic.Code);
    }

    [Fact]
    public void S13_AssignToFunction()
    {
        var ex = ResolveErr("fn f(){ return 1; } f = 2;");
        Assert.Equal(DiagnosticCode.E3006, ex.Diagnostic.Code);
    }

    [Fact]
    public void S15_NestedFunction()
    {
        var ex = ResolveErr("fn f(){ fn g(){ return 1; } return g(); }");
        Assert.Equal(DiagnosticCode.E3007, ex.Diagnostic.Code);
    }

    [Fact]
    public void S16_WrongArity()
    {
        var ex = ResolveErr("fn f(a,b){ return a; } f(1);");
        Assert.Equal(DiagnosticCode.E3008, ex.Diagnostic.Code);
    }

    [Fact]
    public void S17_CallVariable()
    {
        var ex = ResolveErr("let f = 1; f();");
        Assert.Equal(DiagnosticCode.E3009, ex.Diagnostic.Code);
    }

    [Fact]
    public void S18_FunctionAsValue()
    {
        var ex = ResolveErr("fn f(){ return 1; } let x = f;");
        Assert.Equal(DiagnosticCode.E3010, ex.Diagnostic.Code);
    }

    [Fact]
    public void S20_ReturnOutsideFunction()
    {
        var ex = ResolveErr("return 1;");
        Assert.Equal(DiagnosticCode.E3011, ex.Diagnostic.Code);
    }

    [Fact]
    public void S21_ReturnInNestedBlockInsideFunctionAllowed()
    {
        ResolveOk("fn f(){ { return 1; } }");
    }

    [Fact]
    public void S22_InitializerSeesOuterBinding()
    {
        ResolveOk("let x = 1; { let x = x + 1; print(x); }");
    }

    [Fact]
    public void S23_InitializerWithoutOuter_IsE3001()
    {
        var ex = ResolveErr("{ let x = x; }");
        Assert.Equal(DiagnosticCode.E3001, ex.Diagnostic.Code);
    }

    [Fact]
    public void S30_PrintAcceptsAnyArity()
    {
        ResolveOk("print(); print(1); print(1,2,3);");
    }

    private static SemanticModel ResolveOk(string code)
    {
        var source = new SourceText("test.msf", code);
        var program = Parser.Parse(Lexer.Lex(source));
        return new Resolver().Resolve(program);
    }

    private static SemanticException ResolveErr(string code)
    {
        var source = new SourceText("test.msf", code);
        var program = Parser.Parse(Lexer.Lex(source));
        return Assert.Throws<SemanticException>(() => new Resolver().Resolve(program));
    }
}
