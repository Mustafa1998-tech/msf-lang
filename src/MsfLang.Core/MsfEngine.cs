using MsfLang.Core.Diagnostics;
using MsfLang.Core.Lexing;
using MsfLang.Core.Modules;
using MsfLang.Core.Runtime;
using MsfLang.Core.Semantics;
using MsfLang.Core.Source;
using MsfLang.Core.Syntax;

namespace MsfLang.Core;

public sealed record RunResult(int ExitCode, Diagnostic? Diagnostic = null);

public static class MsfEngine
{
    public static RunResult Run(SourceText source, TextWriter stdout, ModuleLoadContext? modules = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(stdout);

        try
        {
            var tokens = Lexer.Lex(source);
            var program = Parser.Parse(tokens);
            var moduleContext = modules ?? new ModuleLoadContext(DefaultBaseDirectory(source.Path));
            var model = new Resolver(moduleContext, source.Path).Resolve(program);
            var interpreter = new Interpreter(model, stdout, source.Path);
            interpreter.Execute(program);
            return new RunResult(0);
        }
        catch (ModuleLoadException ex)
        {
            return new RunResult(ex.ExitCode, ex.Diagnostic);
        }
        catch (LexException ex)
        {
            return new RunResult(1, ex.Diagnostic);
        }
        catch (SyntaxException ex)
        {
            return new RunResult(1, ex.Diagnostic);
        }
        catch (SemanticException ex)
        {
            return new RunResult(1, ex.Diagnostic);
        }
        catch (RuntimeException ex)
        {
            return new RunResult(2, ex.Diagnostic);
        }
        catch (InternalErrorException)
        {
            throw;
        }
    }

    private static string? DefaultBaseDirectory(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            return Path.GetDirectoryName(fullPath);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
