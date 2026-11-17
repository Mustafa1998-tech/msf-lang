using System.Text;
using System.Text.Json;
using MsfLang.Core;
using MsfLang.Core.Modules;
using MsfLang.Core.Source;
using Xunit;

namespace MsfLang.Conformance.Tests;

public sealed class ConformanceTests
{
    public static IEnumerable<object[]> Cases()
    {
        var root = FindRepoRoot();
        var confDir = Path.Combine(root, "tests", "conformance");
        if (!Directory.Exists(confDir))
        {
            yield break;
        }

        foreach (var msf in Directory.EnumerateFiles(confDir, "*.msf", SearchOption.AllDirectories).OrderBy(p => p))
        {
            var expectedPath = Path.ChangeExtension(msf, ".expected.json");
            if (!File.Exists(expectedPath))
            {
                throw new InvalidOperationException($"Missing expected sidecar for {msf}");
            }
            yield return new object[] { msf, expectedPath };
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void ConformanceCase(string msfPath, string expectedPath)
    {
        var expected = JsonSerializer.Deserialize<ExpectedResult>(File.ReadAllText(expectedPath), JsonOptions)
            ?? throw new InvalidOperationException($"Invalid expected JSON: {expectedPath}");

        var text = File.ReadAllText(msfPath, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
        var source = new SourceText(Path.GetRelativePath(FindRepoRoot(), msfPath).Replace('\\', '/'), text);
        var stdout = new StringWriter { NewLine = "\n" };
        var modules = new ModuleLoadContext(Path.GetDirectoryName(msfPath));
        var result = MsfEngine.Run(source, stdout, modules);

        Assert.Equal(expected.ExitCode, result.ExitCode);
        Assert.Equal(Normalize(expected.Stdout), Normalize(stdout.ToString()));

        if (expected.Diagnostic is null)
        {
            Assert.Null(result.Diagnostic);
        }
        else
        {
            Assert.NotNull(result.Diagnostic);
            Assert.Equal(expected.Diagnostic.Code, result.Diagnostic!.Code);
            Assert.Equal(expected.Diagnostic.Line, source.GetPosition(result.Diagnostic.Span.Start).Line);
            Assert.Equal(expected.Diagnostic.Column, source.GetPosition(result.Diagnostic.Span.Start).Column);
            Assert.Equal(expected.Diagnostic.Phase, result.Diagnostic.Phase.ToString().ToLowerInvariant());
        }
    }

    [Fact]
    public void NoOrphanExpectedFiles()
    {
        var root = FindRepoRoot();
        var confDir = Path.Combine(root, "tests", "conformance");
        if (!Directory.Exists(confDir)) return;
        foreach (var json in Directory.EnumerateFiles(confDir, "*.expected.json", SearchOption.AllDirectories))
        {
            var baseName = json.EndsWith(".expected.json", StringComparison.OrdinalIgnoreCase)
                ? json[..^".expected.json".Length]
                : Path.GetFileNameWithoutExtension(json);
            var msf = baseName + ".msf";
            Assert.True(File.Exists(msf), $"Orphan expected file: {json}");
        }
    }

    private static string Normalize(string? s) => (s ?? string.Empty).Replace("\r\n", "\n");

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "MSFLang.slnx")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Could not locate repo root");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class ExpectedResult
    {
        public int ExitCode { get; set; }
        public string? Stdout { get; set; }
        public ExpectedDiagnostic? Diagnostic { get; set; }
    }

    private sealed class ExpectedDiagnostic
    {
        public string Code { get; set; } = "";
        public int Line { get; set; }
        public int Column { get; set; }
        public string Phase { get; set; } = "";
    }
}
