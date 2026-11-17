using System.Text;
using MsfLang.Core.Diagnostics;
using MsfLang.Core.Runtime;
using MsfLang.Core.Semantics;
using MsfLang.Core.Source;
using MsfLang.Core.Syntax;

namespace MsfLang.Core.Modules;

/// <summary>Identity of one loaded module file.</summary>
public sealed class ModuleSymbol
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public Dictionary<string, Binding> Exports { get; } = new(StringComparer.Ordinal);
}

/// <summary>A resolved module: its AST, isolated semantic model, and source.</summary>
public sealed class ModuleUnit
{
    public required ModuleSymbol Symbol { get; init; }
    public required SourceText Source { get; init; }
    public required ProgramNode Program { get; init; }
    public required SemanticModel Model { get; init; }
}

/// <summary>
/// Per-run or per-REPL-session module loading state. The cache guarantees each file is
/// lexed, parsed, and resolved at most once per context, and lets the REPL reuse module
/// bindings across entries.
/// </summary>
public sealed class ModuleLoadContext
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private readonly Func<string, string> _readFile;

    public ModuleLoadContext(string? baseDirectory = null, Func<string, string>? readFile = null)
    {
        BaseDirectory = string.IsNullOrWhiteSpace(baseDirectory)
            ? Environment.CurrentDirectory
            : Path.GetFullPath(baseDirectory);
        _readFile = readFile ?? ReadUtf8;
    }

    public string BaseDirectory { get; }

    internal Dictionary<string, ModuleUnit> Cache { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal int NextModuleId;

    internal int AllocateModuleId() => NextModuleId++;

    internal ModuleUnit? Lookup(string fullPath) => Cache.TryGetValue(fullPath, out var unit) ? unit : null;

    internal void Store(string fullPath, ModuleUnit unit) => Cache[fullPath] = unit;

    internal ModuleSymbol? FindSymbol(int id)
    {
        foreach (var unit in Cache.Values)
        {
            if (unit.Symbol.Id == id)
            {
                return unit.Symbol;
            }
        }

        return null;
    }

    internal string Read(string fullPath) => _readFile(fullPath);

    private static string ReadUtf8(string path) => File.ReadAllText(path, StrictUtf8);
}

/// <summary>
/// Module loading failure with an explicit process exit code. Path/IO problems use exit
/// code 4 (matching CLI I/O errors); cycles and invalid module exports stay semantic (1).
/// </summary>
public sealed class ModuleLoadException : LanguageException
{
    public ModuleLoadException(Diagnostic diagnostic, int exitCode) : base(diagnostic)
    {
        ExitCode = exitCode;
    }

    public int ExitCode { get; }
}

/// <summary>Runtime module environments. Reused across REPL entries so a module executes once.</summary>
public sealed class ModuleRuntimeState
{
    internal Dictionary<int, MsEnvironment> Environments { get; } = new();

    internal MsEnvironment GetOrCreate(int moduleId, MsEnvironment? parent = null)
    {
        if (!Environments.TryGetValue(moduleId, out var env))
        {
            env = new MsEnvironment(parent);
            Environments[moduleId] = env;
        }

        return env;
    }
}
