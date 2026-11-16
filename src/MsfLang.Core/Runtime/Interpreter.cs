using System.Globalization;
using System.Linq;
using MsfLang.Core.Diagnostics;
using MsfLang.Core.Semantics;
using MsfLang.Core.Source;
using MsfLang.Core.Syntax;

namespace MsfLang.Core.Runtime;

public sealed class Interpreter
{
    private sealed class ReturnSignal : Exception
    {
        public ReturnSignal(MsfValue value) => Value = value;
        public MsfValue Value { get; }
    }

    private readonly SemanticModel _model;
    private readonly TextWriter _stdout;
    private int _callDepth;
    private const int MaxCallDepth = 1000;

    public Interpreter(SemanticModel model, TextWriter stdout)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _stdout = stdout ?? throw new ArgumentNullException(nameof(stdout));
    }

    public void Execute(ProgramNode program)
    {
        var globals = new MsEnvironment();
        ExecuteBlockLike(program.Declarations, globals, isGlobal: true, replAutoDisplay: false, displays: null);
    }

    public IReadOnlyList<MsfValue> ExecuteReplEntry(ProgramNode program, MsEnvironment globals)
    {
        ArgumentNullException.ThrowIfNull(globals);
        var displays = new List<MsfValue>();
        ExecuteBlockLike(program.Declarations, globals, isGlobal: true, replAutoDisplay: true, displays: displays);
        return displays;
    }

    private void ExecuteBlockLike(IReadOnlyList<Declaration> declarations, MsEnvironment env, bool isGlobal, bool replAutoDisplay, List<MsfValue>? displays)
    {
        foreach (var decl in declarations)
        {
            ExecuteDeclaration(decl, env, isGlobal, replAutoDisplay, displays);
        }
    }

    private void ExecuteDeclaration(Declaration decl, MsEnvironment env, bool isGlobal, bool replAutoDisplay, List<MsfValue>? displays)
    {
        switch (decl)
        {
            case FunctionDecl fn:
                var fb = _model.Get(fn);
                env.Define(fb.Id, new UserFunction(fn));
                break;
            case VarDecl v:
                var value = Evaluate(v.Initializer, env);
                var vb = _model.Get(v);
                env.Define(vb.Id, value);
                break;
            case Statement stmt:
                ExecuteStatement(stmt, env, isGlobal, replAutoDisplay, displays);
                break;
            default:
                throw new InvalidOperationException($"Unknown declaration {decl.GetType().Name}");
        }
    }

    private void ExecuteStatement(Statement stmt, MsEnvironment env, bool inGlobal, bool replAutoDisplay, List<MsfValue>? displays)
    {
        switch (stmt)
        {
            case BlockStmt block:
                ExecuteBlock(block, new MsEnvironment(env));
                break;
            case IfStmt ifs:
                ExecuteIf(ifs, env);
                break;
            case ReturnStmt ret:
                var val = ret.Value is null ? MsfValue.Nil : Evaluate(ret.Value, env);
                throw new ReturnSignal(val);
            case AssignStmt assign:
                var ab = _model.Get(assign);
                var rhs = Evaluate(assign.Value, env);
                env.Assign(ab.Id, rhs);
                break;
            case ExprStmt exprStmt:
                var result = Evaluate(exprStmt.Expression, env);
                if (replAutoDisplay && inGlobal && displays is not null && result is not MsfNil)
                {
                    displays.Add(result);
                }
                break;
            default:
                throw new InvalidOperationException($"Unknown statement {stmt.GetType().Name}");
        }
    }

    private void ExecuteBlock(BlockStmt block, MsEnvironment env)
    {
        foreach (var decl in block.Declarations)
        {
            ExecuteDeclaration(decl, env, isGlobal: false, replAutoDisplay: false, displays: null);
        }
    }

    private void ExecuteIf(IfStmt ifs, MsEnvironment env)
    {
        var cond = Evaluate(ifs.Condition, env);
        if (cond is not MsfBoolean b)
        {
            throw RuntimeError(DiagnosticCode.E4001, $"condition must be a boolean, got {cond.TypeName}", ifs.Condition.Span);
        }

        if (b.Value)
        {
            ExecuteStatement(ifs.ThenBranch, env, inGlobal: false, replAutoDisplay: false, displays: null);
        }
        else if (ifs.ElseBranch is not null)
        {
            ExecuteStatement(ifs.ElseBranch, env, inGlobal: false, replAutoDisplay: false, displays: null);
        }
    }

    private MsfValue Evaluate(Expression expr, MsEnvironment env)
    {
        return expr switch
        {
            LiteralExpr lit => LiteralToValue(lit),
            GroupingExpr g => Evaluate(g.Expression, env),
            UnaryExpr u => EvaluateUnary(u, env),
            BinaryExpr b => EvaluateBinary(b, env),
            NameExpr n => EvaluateName(n, env),
            CallExpr c => EvaluateCall(c, env),
            _ => throw new InvalidOperationException($"Unknown expression {expr.GetType().Name}")
        };
    }

    private MsfValue EvaluateName(NameExpr name, MsEnvironment env)
    {
        var binding = _model.Get(name);
        if (binding.Kind == BindingKind.Builtin && binding.Name == "print")
        {
            // should not be used as value (resolver prevents), but return internal marker
            return new PrintBuiltin();
        }

        return env.Get(binding.Id);
    }

    private MsfValue EvaluateUnary(UnaryExpr expr, MsEnvironment env)
    {
        var right = Evaluate(expr.Right, env);
        return expr.Operator.Kind switch
        {
            Lexing.TokenKind.Minus => right is MsfNumber n
                ? CheckFinite(-n.Value, expr.Span)
                : throw RuntimeError(DiagnosticCode.E4004, $"invalid operand type for '-': {right.TypeName}", expr.Span),
            Lexing.TokenKind.Not => right is MsfBoolean b
                ? MsfValue.FromBool(!b.Value)
                : throw RuntimeError(DiagnosticCode.E4004, $"invalid operand type for 'not': {right.TypeName}", expr.Span),
            _ => throw new InvalidOperationException("Unknown unary operator")
        };
    }

    private MsfValue EvaluateBinary(BinaryExpr expr, MsEnvironment env)
    {
        if (expr.Operator.Kind == Lexing.TokenKind.And)
        {
            var left = Evaluate(expr.Left, env);
            if (left is not MsfBoolean lb)
            {
                throw RuntimeError(DiagnosticCode.E4004, $"invalid operand types for 'and': {left.TypeName} and <unevaluated>", expr.Span);
            }
            if (!lb.Value) return MsfValue.False;
            var right = Evaluate(expr.Right, env);
            if (right is not MsfBoolean rb)
            {
                throw RuntimeError(DiagnosticCode.E4004, $"invalid operand types for 'and': boolean and {right.TypeName}", expr.Span);
            }
            return MsfValue.FromBool(rb.Value);
        }

        if (expr.Operator.Kind == Lexing.TokenKind.Or)
        {
            var left = Evaluate(expr.Left, env);
            if (left is not MsfBoolean lb)
            {
                throw RuntimeError(DiagnosticCode.E4004, $"invalid operand types for 'or': {left.TypeName} and <unevaluated>", expr.Span);
            }
            if (lb.Value) return MsfValue.True;
            var right = Evaluate(expr.Right, env);
            if (right is not MsfBoolean rb)
            {
                throw RuntimeError(DiagnosticCode.E4004, $"invalid operand types for 'or': boolean and {right.TypeName}", expr.Span);
            }
            return MsfValue.FromBool(rb.Value);
        }

        var l = Evaluate(expr.Left, env);
        var r = Evaluate(expr.Right, env);
        var op = expr.Operator.Lexeme;

        switch (expr.Operator.Kind)
        {
            case Lexing.TokenKind.Plus:
                if (l is MsfNumber ln && r is MsfNumber rn) return CheckFinite(ln.Value + rn.Value, expr.Span);
                if (l is MsfString ls && r is MsfString rs) return new MsfString(ls.Value + rs.Value);
                throw RuntimeError(DiagnosticCode.E4004, $"invalid operand types for '+': {l.TypeName} and {r.TypeName}", expr.Span);
            case Lexing.TokenKind.Minus:
                RequireNumbers(l, r, "-", expr.Span, out var a1, out var b1); return CheckFinite(a1 - b1, expr.Span);
            case Lexing.TokenKind.Star:
                RequireNumbers(l, r, "*", expr.Span, out var a2, out var b2); return CheckFinite(a2 * b2, expr.Span);
            case Lexing.TokenKind.Slash:
                RequireNumbers(l, r, "/", expr.Span, out var a3, out var b3);
                if (b3 == 0d) throw RuntimeError(DiagnosticCode.E4002, "division by zero", expr.Span);
                return CheckFinite(a3 / b3, expr.Span);
            case Lexing.TokenKind.Percent:
                RequireNumbers(l, r, "%", expr.Span, out var a4, out var b4);
                if (b4 == 0d) throw RuntimeError(DiagnosticCode.E4003, "modulo by zero", expr.Span);
                return CheckFinite(a4 - Math.Truncate(a4 / b4) * b4, expr.Span);
            case Lexing.TokenKind.EqualEqual:
                return MsfValue.FromBool(IsEqual(l, r));
            case Lexing.TokenKind.BangEqual:
                return MsfValue.FromBool(!IsEqual(l, r));
            case Lexing.TokenKind.Less:
                RequireNumbers(l, r, "<", expr.Span, out var c1, out var d1); return MsfValue.FromBool(c1 < d1);
            case Lexing.TokenKind.LessEqual:
                RequireNumbers(l, r, "<=", expr.Span, out var c2, out var d2); return MsfValue.FromBool(c2 <= d2);
            case Lexing.TokenKind.Greater:
                RequireNumbers(l, r, ">", expr.Span, out var c3, out var d3); return MsfValue.FromBool(c3 > d3);
            case Lexing.TokenKind.GreaterEqual:
                RequireNumbers(l, r, ">=", expr.Span, out var c4, out var d4); return MsfValue.FromBool(c4 >= d4);
            default:
                throw new InvalidOperationException($"Unknown binary operator {op}");
        }
    }

    private MsfValue EvaluateCall(CallExpr call, MsEnvironment env)
    {
        var binding = _model.Get(call);
        if (binding.Kind == BindingKind.Builtin && binding.Name == "print")
        {
            var args = new MsfValue[call.Arguments.Count];
            for (var i = 0; i < call.Arguments.Count; i++)
            {
                args[i] = Evaluate(call.Arguments[i], env);
            }

            _stdout.Write(string.Join(" ", args.Select(ValueFormatter.Format)));
            _stdout.Write('\n');
            _stdout.Flush();
            return MsfValue.Nil;
        }

        if (binding.Kind != BindingKind.Function)
        {
            throw RuntimeError(DiagnosticCode.E3009, $"cannot call '{binding.Name}'", call.Span);
        }

        if (_callDepth >= MaxCallDepth)
        {
            throw RuntimeError(DiagnosticCode.E4005, $"maximum call depth (1000) exceeded in '{binding.Name}'", call.Span);
        }

        if (env.Get(binding.Id) is not UserFunction fnValue)
        {
            throw new InternalErrorException($"Function '{binding.Name}' not found at runtime.");
        }

        var argValues = new MsfValue[call.Arguments.Count];
        for (var i = 0; i < call.Arguments.Count; i++)
        {
            argValues[i] = Evaluate(call.Arguments[i], env);
        }

        _callDepth++;
        try
        {
            var callEnv = new MsEnvironment(env.Global);
            var decl = fnValue.Declaration;
            var paramBindings = _model.GetParameters(decl);
            for (var i = 0; i < paramBindings.Count; i++)
            {
                callEnv.Define(paramBindings[i].Id, argValues[i]);
            }

            try
            {
                ExecuteBlockLike(decl.Body.Declarations, callEnv, isGlobal: false, replAutoDisplay: false, displays: null);
            }
            catch (ReturnSignal rs)
            {
                return rs.Value;
            }

            return MsfValue.Nil;
        }
        finally
        {
            _callDepth--;
        }
    }

    // Function execution is handled inline in EvaluateCall.

    private static MsfValue LiteralToValue(LiteralExpr lit)
    {
        return lit.Value switch
        {
            null => MsfValue.Nil,
            bool b => MsfValue.FromBool(b),
            double d => new MsfNumber(d),
            string s => new MsfString(s),
            _ => throw new InvalidOperationException("Unknown literal type")
        };
    }

    private static bool IsEqual(MsfValue a, MsfValue b)
    {
        if (a.TypeName != b.TypeName) return false;
        return (a, b) switch
        {
            (MsfNumber x, MsfNumber y) => x.Value == y.Value,
            (MsfString x, MsfString y) => string.Equals(x.Value, y.Value, StringComparison.Ordinal),
            (MsfBoolean x, MsfBoolean y) => x.Value == y.Value,
            (MsfNil, MsfNil) => true,
            _ => false
        };
    }

    private static void RequireNumbers(MsfValue l, MsfValue r, string op, TextSpan span, out double a, out double b)
    {
        if (l is MsfNumber ln && r is MsfNumber rn)
        {
            a = ln.Value; b = rn.Value; return;
        }
        throw RuntimeError(DiagnosticCode.E4004, $"invalid operand types for '{op}': {l.TypeName} and {r.TypeName}", span);
    }

    private static MsfNumber CheckFinite(double value, TextSpan span)
    {
        if (!double.IsFinite(value))
        {
            throw RuntimeError(DiagnosticCode.E4006, "numeric result is not finite", span);
        }
        return new MsfNumber(value);
    }

    private static RuntimeException RuntimeError(string code, string message, TextSpan span)
        => new(new Diagnostic(code, DiagnosticPhase.Runtime, message, span));
}
