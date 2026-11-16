using MsfLang.Core.Diagnostics;
using MsfLang.Core.Source;
using MsfLang.Core.Syntax;

namespace MsfLang.Core.Semantics;

public sealed class Resolver
{
    private sealed class Scope
    {
        public Scope? Parent { get; }
        private readonly Dictionary<string, Binding> _bindings = new(StringComparer.Ordinal);

        public Scope(Scope? parent) => Parent = parent;

        public bool TryDefine(Binding binding) => _bindings.TryAdd(binding.Name, binding);

        public bool TryGetLocal(string name, out Binding binding) => _bindings.TryGetValue(name, out binding!);

        public Binding? Resolve(string name)
        {
            for (var scope = this; scope is not null; scope = scope.Parent)
            {
                if (scope._bindings.TryGetValue(name, out var binding))
                {
                    return binding;
                }
            }

            return null;
        }
    }

    private readonly Scope _global = new(null);
    private readonly SemanticModel _model = new();
    private int _nextId = 1;
    private int _functionDepth;
    private FunctionDecl? _currentFunction;

    public Resolver()
    {
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "print",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = null,
            DeclarationSpan = TextSpan.Empty
        });
    }

    public SemanticModel Resolve(ProgramNode program)
    {
        foreach (var decl in program.Declarations)
        {
            switch (decl)
            {
                case FunctionDecl fn:
                    DeclareFunction(fn, _global, isTopLevel: true);
                    ResolveFunctionBody(fn, _global);
                    break;
                case VarDecl v:
                    ResolveExpression(v.Initializer, _global);
                    DeclareVariable(v, _global);
                    break;
                case Statement stmt:
                    ResolveStatement(stmt, _global);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown declaration type {decl.GetType().Name}");
            }
        }

        return _model;
    }

    private void DeclareFunction(FunctionDecl fn, Scope scope, bool isTopLevel)
    {
        if (!isTopLevel)
        {
            throw Semantic(fn.Span, DiagnosticCode.E3007, "function declarations are only allowed at top level");
        }

        if (fn.Name.Lexeme == "print")
        {
            throw Semantic(fn.Name.Span, DiagnosticCode.E3004, "builtin 'print' cannot be redeclared");
        }

        var paramNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var p in fn.Parameters)
        {
            if (p.Lexeme == "print")
            {
                // parameter named print? treat as duplicate-like reserved? spec doesn't list explicitly but print cannot be declared in any scope
                throw Semantic(p.Span, DiagnosticCode.E3004, "builtin 'print' cannot be redeclared");
            }
            if (!paramNames.Add(p.Lexeme))
            {
                throw Semantic(p.Span, DiagnosticCode.E3003, $"duplicate parameter '{p.Lexeme}'");
            }
        }

        var binding = new Binding
        {
            Id = _nextId++,
            Name = fn.Name.Lexeme,
            Kind = BindingKind.Function,
            IsMutable = false,
            Arity = fn.Parameters.Count,
            DeclarationSpan = fn.Name.Span
        };

        if (!scope.TryDefine(binding))
        {
            throw Semantic(fn.Name.Span, DiagnosticCode.E3002, $"duplicate declaration '{fn.Name.Lexeme}' in this scope");
        }

        _model.Set(fn, binding);
    }

    private void DeclareVariable(VarDecl decl, Scope scope)
    {
        if (decl.Name.Lexeme == "print")
        {
            throw Semantic(decl.Name.Span, DiagnosticCode.E3004, "builtin 'print' cannot be redeclared");
        }

        var binding = new Binding
        {
            Id = _nextId++,
            Name = decl.Name.Lexeme,
            Kind = BindingKind.Variable,
            IsMutable = decl.IsMutable,
            Arity = null,
            DeclarationSpan = decl.Name.Span
        };

        if (!scope.TryDefine(binding))
        {
            throw Semantic(decl.Name.Span, DiagnosticCode.E3002, $"duplicate declaration '{decl.Name.Lexeme}' in this scope");
        }

        _model.Set(decl, binding);
    }

    private void ResolveFunctionBody(FunctionDecl fn, Scope global)
    {
        var previousFunction = _currentFunction;
        var previousDepth = _functionDepth;
        _currentFunction = fn;
        _functionDepth++;

        // Function scope contains parameters and body top-level locals (one scope).
        var fnScope = new Scope(global);
        var paramBindings = new List<Binding>(fn.Parameters.Count);
        foreach (var p in fn.Parameters)
        {
            var pb = new Binding
            {
                Id = _nextId++,
                Name = p.Lexeme,
                Kind = BindingKind.Variable,
                IsMutable = true, // parameters mutable
                Arity = null,
                DeclarationSpan = p.Span
            };
            fnScope.TryDefine(pb);
            paramBindings.Add(pb);
        }
        _model.SetParameters(fn, paramBindings);

        ResolveBlockDeclarationsAndStatements(fn.Body, fnScope, isFunctionBody: true);

        _functionDepth = previousDepth;
        _currentFunction = previousFunction;
    }

    private void ResolveBlockDeclarationsAndStatements(BlockStmt block, Scope scope, bool isFunctionBody)
    {
        // Two-pass within block not specified except top-level functions; here declarations in blocks are statements-like in order.
        // But we must handle nested var visibility: initializer resolved before binding declared.
        // For nested function decls in blocks: they parse but must be rejected (E3007). They can appear as declarations.
        foreach (var decl in block.Declarations)
        {
            switch (decl)
            {
                case FunctionDecl nestedFn:
                    // reject nested function regardless of position
                    throw Semantic(nestedFn.Span.Start == nestedFn.Span.Start ? nestedFn.Name.Span : nestedFn.Name.Span, DiagnosticCode.E3007, "function declarations are only allowed at top level");
                case VarDecl v:
                    ResolveExpression(v.Initializer, scope);
                    DeclareVariable(v, scope);
                    break;
                case Statement stmt:
                    ResolveStatement(stmt, scope);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown declaration type {decl.GetType().Name}");
            }
        }

        _ = isFunctionBody;
    }

    private void ResolveStatement(Statement stmt, Scope scope)
    {
        switch (stmt)
        {
            case BlockStmt block:
                ResolveBlockDeclarationsAndStatements(block, new Scope(scope), isFunctionBody: false);
                break;
            case IfStmt ifs:
                ResolveExpression(ifs.Condition, scope);
                ResolveStatement(ifs.ThenBranch, scope);
                if (ifs.ElseBranch is not null)
                {
                    ResolveStatement(ifs.ElseBranch, scope);
                }
                break;
            case ReturnStmt ret:
                if (_functionDepth == 0)
                {
                    throw Semantic(ret.Keyword.Span, DiagnosticCode.E3011, "'return' is only allowed inside a function");
                }
                if (ret.Value is not null)
                {
                    ResolveExpression(ret.Value, scope);
                }
                break;
            case AssignStmt assign:
                ResolveAssignment(assign, scope);
                break;
            case ExprStmt exprStmt:
                ResolveExpression(exprStmt.Expression, scope);
                break;
            default:
                throw new InvalidOperationException($"Unknown statement type {stmt.GetType().Name}");
        }
    }

    private void ResolveAssignment(AssignStmt assign, Scope scope)
    {
        var binding = ResolveName(assign.Name, scope, forCall: false, asValue: false);
        if (binding.Kind == BindingKind.Variable)
        {
            if (!binding.IsMutable)
            {
                throw Semantic(assign.Name.Span, DiagnosticCode.E3005, $"cannot assign to immutable '{binding.Name}'; declare it with 'var'");
            }
        }
        else
        {
            throw Semantic(assign.Name.Span, DiagnosticCode.E3006, $"cannot assign to '{binding.Name}'; functions and builtins are not assignable");
        }

        ResolveExpression(assign.Value, scope);
        _model.Set(assign, binding);
    }

    private void ResolveExpression(Expression expr, Scope scope)
    {
        switch (expr)
        {
            case LiteralExpr:
                break;
            case GroupingExpr g:
                ResolveExpression(g.Expression, scope);
                break;
            case UnaryExpr u:
                ResolveExpression(u.Right, scope);
                break;
            case BinaryExpr b:
                ResolveExpression(b.Left, scope);
                ResolveExpression(b.Right, scope);
                break;
            case NameExpr n:
                var bnd = ResolveName(n.Name, scope, forCall: false, asValue: true);
                _model.Set(n, bnd);
                break;
            case CallExpr call:
                ResolveCall(call, scope);
                break;
            default:
                throw new InvalidOperationException($"Unknown expression type {expr.GetType().Name}");
        }
    }

    private void ResolveCall(CallExpr call, Scope scope)
    {
        var callee = ResolveName(call.Callee, scope, forCall: true, asValue: false);
        if (callee.Kind == BindingKind.Variable)
        {
            throw Semantic(call.Callee.Span, DiagnosticCode.E3009, $"cannot call '{callee.Name}'; it is a variable, not a function");
        }

        if (callee.Kind == BindingKind.Function && callee.Arity is int arity && arity != call.Arguments.Count)
        {
            throw Semantic(call.Callee.Span, DiagnosticCode.E3008, $"function '{callee.Name}' expects {arity} argument(s), got {call.Arguments.Count}");
        }

        foreach (var arg in call.Arguments)
        {
            ResolveExpression(arg, scope);
        }

        _model.Set(call, callee);
    }

    private static Binding ResolveName(Lexing.Token name, Scope scope, bool forCall, bool asValue)
    {
        var binding = scope.Resolve(name.Lexeme);
        if (binding is null)
        {
            throw Semantic(name.Span, DiagnosticCode.E3001, $"undefined name '{name.Lexeme}'");
        }

        if (asValue && binding.Kind is BindingKind.Function or BindingKind.Builtin)
        {
            throw Semantic(name.Span, DiagnosticCode.E3010, $"'{binding.Name}' can only be used in a call");
        }

        _ = forCall;
        return binding;
    }

    private static SemanticException Semantic(TextSpan span, string code, string message)
    {
        return new SemanticException(new Diagnostic(code, DiagnosticPhase.Semantic, message, span));
    }
}
