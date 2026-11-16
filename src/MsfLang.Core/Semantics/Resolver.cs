using MsfLang.Core.Diagnostics;
using MsfLang.Core.Source;
using MsfLang.Core.Syntax;

namespace MsfLang.Core.Semantics;

public sealed class Resolver
{
    internal sealed class Scope
    {
        public Scope? Parent { get; }
        internal readonly Dictionary<string, Binding> Bindings = new(StringComparer.Ordinal);

        public Scope(Scope? parent) => Parent = parent;

        public bool TryDefine(Binding binding) => Bindings.TryAdd(binding.Name, binding);

        public bool TryGetLocal(string name, out Binding binding) => Bindings.TryGetValue(name, out binding!);

        public Binding? Resolve(string name)
        {
            for (var scope = this; scope is not null; scope = scope.Parent)
            {
                if (scope.Bindings.TryGetValue(name, out var binding))
                {
                    return binding;
                }
            }

            return null;
        }

        public Scope CloneShallow()
        {
            var clone = new Scope(Parent);
            foreach (var kv in Bindings)
            {
                clone.Bindings[kv.Key] = kv.Value;
            }
            return clone;
        }
    }

    private Scope _global;
    private SemanticModel _model;
    private int _nextId;
    private int _functionDepth;
    private int _loopDepth;
    private FunctionDecl? _currentFunction;

    public Resolver()
    {
        _global = new Scope(null);
        _model = new SemanticModel();
        _nextId = 1;
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "len",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 1,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "append",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 2,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "get",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 2,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "set",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 3,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "has",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 2,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "remove",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 2,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "abs",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 1,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "sqrt",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 1,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "floor",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 1,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "ceil",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 1,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "round",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 1,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "pow",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 2,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "min",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 2,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding
        {
            Id = _nextId++,
            Name = "max",
            Kind = BindingKind.Builtin,
            IsMutable = false,
            Arity = 2,
            DeclarationSpan = TextSpan.Empty
        });
        _global.TryDefine(new Binding { Id = _nextId++, Name = "upper", Kind = BindingKind.Builtin, IsMutable = false, Arity = 1, DeclarationSpan = TextSpan.Empty });
        _global.TryDefine(new Binding { Id = _nextId++, Name = "lower", Kind = BindingKind.Builtin, IsMutable = false, Arity = 1, DeclarationSpan = TextSpan.Empty });
        _global.TryDefine(new Binding { Id = _nextId++, Name = "trim", Kind = BindingKind.Builtin, IsMutable = false, Arity = 1, DeclarationSpan = TextSpan.Empty });
        _global.TryDefine(new Binding { Id = _nextId++, Name = "contains", Kind = BindingKind.Builtin, IsMutable = false, Arity = 2, DeclarationSpan = TextSpan.Empty });
        _global.TryDefine(new Binding { Id = _nextId++, Name = "startsWith", Kind = BindingKind.Builtin, IsMutable = false, Arity = 2, DeclarationSpan = TextSpan.Empty });
        _global.TryDefine(new Binding { Id = _nextId++, Name = "endsWith", Kind = BindingKind.Builtin, IsMutable = false, Arity = 2, DeclarationSpan = TextSpan.Empty });
        _global.TryDefine(new Binding { Id = _nextId++, Name = "split", Kind = BindingKind.Builtin, IsMutable = false, Arity = 2, DeclarationSpan = TextSpan.Empty });
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
        ResolveInto(program, _global);
        return _model;
    }

    public (SemanticModel Model, ResolverSnapshot SnapshotBefore) ResolveEntryTransactional(ProgramNode program, ResolverSnapshot committed)
    {
        // Restore committed state into a working resolver, resolve, and return new snapshot on success.
        _global = committed.Global.CloneShallow();
        _nextId = committed.NextId;
        _model = committed.Model; // model accumulates mappings across entries; on failure we restore snapshot model ref/copy
        ResolveInto(program, _global);
        var after = new ResolverSnapshot(_global, _nextId, _model);
        return (_model, after);
    }

    public ResolverSnapshot CreateSnapshot(SemanticModel model)
    {
        return new ResolverSnapshot(_global.CloneShallow(), _nextId, model);
    }

    public static Resolver CreateForRepl(out ResolverSnapshot snapshot, out SemanticModel model)
    {
        var r = new Resolver();
        model = r._model;
        snapshot = r.CreateSnapshot(model);
        return r;
    }

    private void ResolveInto(ProgramNode program, Scope global)
    {
        foreach (var decl in program.Declarations)
        {
            switch (decl)
            {
                case FunctionDecl fn:
                    DeclareFunction(fn, global, isTopLevel: true);
                    ResolveFunctionBody(fn, global);
                    break;
                case VarDecl v:
                    ResolveExpression(v.Initializer, global);
                    DeclareVariable(v, global);
                    break;
                case Statement stmt:
                    ResolveStatement(stmt, global);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown declaration type {decl.GetType().Name}");
            }
        }
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
            case BreakStmt breakStmt:
                if (_loopDepth == 0) throw Semantic(breakStmt.Span, DiagnosticCode.E3013, "'break' is only allowed inside a loop");
                break;
            case ContinueStmt continueStmt:
                if (_loopDepth == 0) throw Semantic(continueStmt.Span, DiagnosticCode.E3014, "'continue' is only allowed inside a loop");
                break;
            case WhileStmt whileStmt:
                ResolveExpression(whileStmt.Condition, scope);
                var previousLoop = _loopDepth;
                _loopDepth++;
                ResolveStatement(whileStmt.Body, scope);
                _loopDepth = previousLoop;
                break;
            case AssignIndexStmt assignIndex:
                ResolveExpression(assignIndex.Target, scope);
                ResolveExpression(assignIndex.Index, scope);
                ResolveExpression(assignIndex.Value, scope);
                break;
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
            case MapLiteralExpr map:
                foreach (var item in map.Items)
                {
                    ResolveExpression(item.Value, scope);
                }
                break;
            case ListLiteralExpr list:
                foreach (var item in list.Items)
                {
                    ResolveExpression(item, scope);
                }
                break;
            case IndexExpr index:
                ResolveExpression(index.Target, scope);
                ResolveExpression(index.Index, scope);
                break;
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
