using MsfLang.Core.Diagnostics;
using MsfLang.Core.Lexing;
using MsfLang.Core.Runtime;
using MsfLang.Core.Semantics;
using MsfLang.Core.Source;
using MsfLang.Core.Syntax;

namespace MsfLang.Core;

public sealed record RunResult(int ExitCode, Diagnostic? Diagnostic = null);

public static class MsfEngine
{
    public static RunResult Run(SourceText source, TextWriter stdout)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(stdout);

        try
        {
            var tokens = Lexer.Lex(source);
            var program = Parser.Parse(tokens);
            var model = new Resolver().Resolve(program);
            var interpreter = new Interpreter(model, stdout);
            interpreter.Execute(program);
            return new RunResult(0);
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
}
