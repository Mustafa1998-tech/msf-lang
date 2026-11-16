using MsfLang.Core.Diagnostics;
using MsfLang.Core.Lexing;
using MsfLang.Core.Runtime;
using MsfLang.Core.Semantics;
using MsfLang.Core.Source;
using MsfLang.Core.Syntax;

namespace MsfLang.Core.Repl;

public sealed class ReplSession
{
    private readonly TextWriter _stdout;
    private readonly MsEnvironment _globals = new();
    private ResolverSnapshot _resolverSnapshot;
    private SemanticModel _committedModel;

    public ReplSession(TextWriter? stdout = null)
    {
        _stdout = stdout ?? Console.Out;
        Resolver.CreateForRepl(out _resolverSnapshot, out _committedModel);
    }

    public ReplResult Evaluate(string sourceText)
    {
        var source = new SourceText("<repl>", sourceText);
        var envSnapshot = _globals.SnapshotShallow();
        var committedModelBefore = _committedModel.Clone();
        var resolverBefore = new ResolverSnapshot(CloneScope(_resolverSnapshot.Global), _resolverSnapshot.NextId, committedModelBefore);
        try
        {
            var tokens = Lexer.Lex(source);
            var program = Parser.Parse(tokens);
            var (entryModel, afterSnapshot) = ResolveWithSnapshot(resolverBefore, program);

            // Execute with a model that includes this entry's bindings, but keep a clean
            // pre-entry model for rollback if runtime fails.
            var executionModel = committedModelBefore.Clone();
            executionModel.MergeFrom(entryModel);
            var interpreter = new Interpreter(executionModel, _stdout);
            var displays = interpreter.ExecuteReplEntry(program, _globals);

            _resolverSnapshot = new ResolverSnapshot(afterSnapshot.Global, afterSnapshot.NextId, executionModel);
            _committedModel = executionModel;
            return ReplResult.Success(displays);
        }
        catch (LexException ex)
        {
            Restore(envSnapshot, resolverBefore);
            return ReplResult.FromDiagnostic(ex.Diagnostic, isIncomplete: IsIncomplete(ex.Diagnostic, source));
        }
        catch (SyntaxException ex)
        {
            Restore(envSnapshot, resolverBefore);
            return ReplResult.FromDiagnostic(ex.Diagnostic, isIncomplete: IsIncomplete(ex.Diagnostic, source));
        }
        catch (SemanticException ex)
        {
            Restore(envSnapshot, resolverBefore);
            return ReplResult.FromDiagnostic(ex.Diagnostic, isIncomplete: false);
        }
        catch (RuntimeException ex)
        {
            Restore(envSnapshot, resolverBefore);
            return ReplResult.FromDiagnostic(ex.Diagnostic, isIncomplete: false);
        }
    }

    private void Restore(MsEnvironment envSnapshot, ResolverSnapshot resolverSnapshot)
    {
        ReplEnvRestore.Restore(_globals, envSnapshot);
        _resolverSnapshot = new ResolverSnapshot(CloneScope(resolverSnapshot.Global), resolverSnapshot.NextId, resolverSnapshot.Model.Clone());
        _committedModel = _resolverSnapshot.Model;
    }

    private static (SemanticModel Model, ResolverSnapshot Snapshot) ResolveWithSnapshot(ResolverSnapshot before, ProgramNode program)
    {
        var resolver = new Resolver();
        ResolverSeeder.Seed(resolver, before);
        var model = new SemanticModel();
        ResolverSeeder.SetModel(resolver, model);
        resolver.ResolveIntoPublic(program);
        return (model, resolver.CreateSnapshot(model));
    }

    private static bool IsIncomplete(Diagnostic diagnostic, SourceText source)
    {
        if (diagnostic.Code == DiagnosticCode.E2010) return true;
        if (diagnostic.Code == DiagnosticCode.E1003)
        {
            // unterminated string at EOF
            return diagnostic.Span.End >= source.Text.Length || EndsAtEof(source, diagnostic.Span.End);
        }
        if (diagnostic.Code == DiagnosticCode.E1005) return true;
        return false;
    }

    private static bool EndsAtEof(SourceText source, int pos) => pos >= source.Text.Length;

    private static Resolver.Scope CloneScope(Resolver.Scope scope) => scope.CloneShallow();
}

public sealed record ReplResult(bool Ok, bool IsIncomplete, Diagnostic? Diagnostic, IReadOnlyList<MsfValue> Displays)
{
    public static ReplResult Success(IReadOnlyList<MsfValue> displays) => new(true, false, null, displays);
    public static ReplResult FromDiagnostic(Diagnostic diagnostic, bool isIncomplete) => new(false, isIncomplete, diagnostic, Array.Empty<MsfValue>());
}

// Internal helpers to access/seed resolver state without large refactors now
internal static class ResolverSeeder
{
    public static void Seed(Resolver resolver, ResolverSnapshot snapshot)
    {
        typeof(Resolver).GetField("_global", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(resolver, snapshot.Global.CloneShallow());
        typeof(Resolver).GetField("_nextId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(resolver, snapshot.NextId);
    }

    public static void SetModel(Resolver resolver, SemanticModel model)
    {
        typeof(Resolver).GetField("_model", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(resolver, model);
    }
}

internal static class ReplEnvRestore
{
    public static void Restore(MsEnvironment target, MsEnvironment snapshot)
    {
        var valuesField = typeof(MsEnvironment).GetField("_values", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var snapValues = (Dictionary<int, MsfValue>)valuesField.GetValue(snapshot)!;
        valuesField.SetValue(target, new Dictionary<int, MsfValue>(snapValues));
    }
}

internal static class ResolverPublicExt
{
    public static void ResolveIntoPublic(this Resolver resolver, ProgramNode program)
    {
        // call private ResolveInto via reflection
        try
        {
            typeof(Resolver).GetMethod("ResolveInto", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(resolver, new object[] { program, typeof(Resolver).GetField("_global", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(resolver)! });
        }
        catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }
}
