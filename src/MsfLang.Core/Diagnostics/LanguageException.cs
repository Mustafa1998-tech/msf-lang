namespace MsfLang.Core.Diagnostics;

public class LanguageException : Exception
{
    public LanguageException(Diagnostic diagnostic)
        : base(diagnostic.Message)
    {
        Diagnostic = diagnostic;
    }

    public Diagnostic Diagnostic { get; }
}

public sealed class LexException(Diagnostic diagnostic) : LanguageException(diagnostic);
public sealed class SyntaxException(Diagnostic diagnostic) : LanguageException(diagnostic);
public sealed class SemanticException(Diagnostic diagnostic) : LanguageException(diagnostic);
public sealed class RuntimeException(Diagnostic diagnostic) : LanguageException(diagnostic);

public sealed class InternalErrorException : Exception
{
    public InternalErrorException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
