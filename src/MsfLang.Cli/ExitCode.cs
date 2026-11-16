namespace MsfLang.Cli;

internal static class ExitCode
{
    public const int Success = 0;
    public const int CompileError = 1;
    public const int RuntimeError = 2;
    public const int UsageError = 3;
    public const int IoError = 4;
    public const int InternalError = 70;
}
