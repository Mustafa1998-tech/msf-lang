using System.Text;
using MsfLang.Core;
using MsfLang.Core.Diagnostics;
using MsfLang.Core.Modules;
using MsfLang.Core.Source;
using Xunit;

namespace MsfLang.Core.Tests.Modules;

public sealed class ModuleTests
{
    [Fact]
    public void Import_QualifiedFunctionCall_Works()
    {
        var (result, stdout) = Run(
            "main.msf",
            "import \"./math.msf\";\nprint(math.square(9));\n",
            ("math.msf", "export fn square(n) { return n * n; }\n"));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("81\n", stdout);
    }

    [Fact]
    public void Import_ExportedVariable_Works()
    {
        var (result, stdout) = Run(
            "main.msf",
            "import \"./config.msf\";\nprint(config.NAME);\nprint(config.LIMIT);\n",
            ("config.msf", "export var NAME = \"msf\";\nexport var LIMIT = 3;\n"));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("msf\n3\n", stdout);
    }

    [Fact]
    public void Import_UnexportedName_IsE4013()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./math.msf\";\nprint(math.secret);\n",
            ("math.msf", "export fn square(n) { return n * n; }\nvar secret = 42;\n"));

        AssertError(result, DiagnosticCode.E4013);
        Assert.Equal(1, result.ExitCode);
    }

    [Fact]
    public void Import_UnqualifiedName_IsE3001()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./math.msf\";\nprint(square(9));\n",
            ("math.msf", "export fn square(n) { return n * n; }\n"));

        AssertError(result, DiagnosticCode.E3001);
    }

    [Fact]
    public void Import_ModuleAsValue_IsE3017()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./math.msf\";\nprint(math);\n",
            ("math.msf", "export fn square(n) { return n * n; }\n"));

        AssertError(result, DiagnosticCode.E3017);
    }

    [Fact]
    public void Import_MissingFile_IsE4010_Exit4()
    {
        var (result, _) = Run("main.msf", "import \"./missing.msf\";\n");

        AssertError(result, DiagnosticCode.E4010);
        Assert.Equal(4, result.ExitCode);
    }

    [Fact]
    public void Import_Cycle_IsE4014()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./a.msf\";\nprint(a.value());\n",
            ("a.msf", "import \"./b.msf\";\nexport fn value() { return b.value(); }\n"),
            ("b.msf", "import \"./a.msf\";\nexport fn value() { return 1; }\n"));

        AssertError(result, DiagnosticCode.E4014);
        Assert.Contains("import cycle detected", result.Diagnostic!.Message);
    }

    [Fact]
    public void Import_DuplicateModuleName_IsE3002()
    {
        var (result, _) = Run(
            "main.msf",
            "var math = 1;\nimport \"./math.msf\";\nprint(math.square(3));\n",
            ("math.msf", "export fn square(n) { return n * n; }\n"));

        AssertError(result, DiagnosticCode.E3002);
    }

    [Fact]
    public void Import_DuplicateImportInSameScope_IsE3002()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./counter.msf\";\nimport \"./counter.msf\";\nprint(counter.next());\n",
            ("counter.msf", "var n = 0;\nexport fn next() { n = n + 1; return n; }\n"));

        AssertError(result, DiagnosticCode.E3002);
    }

    [Fact]
    public void Import_SharedModuleExecutesOnce_AcrossImporters()
    {
        var (result, stdout) = Run(
            "main.msf",
            "import \"./a.msf\";\nimport \"./b.msf\";\nprint(a.start());\nprint(b.start());\n",
            ("a.msf", "import \"./counter.msf\";\nexport fn start() { return counter.next(); }\n"),
            ("b.msf", "import \"./counter.msf\";\nexport fn start() { return counter.next(); }\n"),
            ("counter.msf", "print(\"loaded\");\nvar n = 0;\nexport fn next() { n = n + 1; return n; }\n"));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("loaded\n1\n2\n", stdout);
    }

    [Fact]
    public void Import_TransitiveDependency_Works()
    {
        var (result, stdout) = Run(
            "main.msf",
            "import \"./quad.msf\";\nprint(quad.quad(3));\n",
            ("quad.msf", "import \"./double.msf\";\nexport fn quad(n) { return double.double(double.double(n)); }\n"),
            ("double.msf", "export fn double(n) { return n * 2; }\n"));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("12\n", stdout);
    }

    [Fact]
    public void Import_ModuleLocalHelperFunction_IsCallableInternally()
    {
        var (result, stdout) = Run(
            "main.msf",
            "import \"./service.msf\";\nprint(service.twice(4));\n",
            ("service.msf", "fn inc(n) { return n + 1; }\nexport fn twice(n) { return inc(inc(n)); }\n"));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("6\n", stdout);
    }

    [Fact]
    public void Import_PrivateFunctionFromOutside_IsE4013()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./service.msf\";\nprint(service.inc(4));\n",
            ("service.msf", "fn inc(n) { return n + 1; }\nexport fn twice(n) { return inc(inc(n)); }\n"));

        AssertError(result, DiagnosticCode.E4013);
    }

    [Fact]
    public void Import_AbsolutePath_IsE4012()
    {
        var path = Path.Combine(Path.GetTempPath(), "absolute.msf").Replace("\\", "\\\\");
        var (result, _) = Run("main.msf", $"import \"{path}\";\n");

        AssertError(result, DiagnosticCode.E4012);
        Assert.Equal(4, result.ExitCode);
    }

    [Fact]
    public void Import_NonIdentifierFileName_IsE4012()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./my-module.msf\";\n",
            ("my-module.msf", "export fn value() { return 1; }\n"));

        AssertError(result, DiagnosticCode.E4012);
    }

    [Fact]
    public void Import_InvalidExtension_IsE4012()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./math.txt\";\n",
            ("math.txt", "export fn value() { return 1; }\n"));

        AssertError(result, DiagnosticCode.E4012);
    }

    [Fact]
    public void Import_InsideBlock_IsE3015()
    {
        var (result, _) = Run(
            "main.msf",
            "{ import \"./math.msf\"; }\n",
            ("math.msf", "export fn square(n) { return n * n; }\n"));

        AssertError(result, DiagnosticCode.E3015);
    }

    [Fact]
    public void Export_InsideBlock_IsE3016()
    {
        var (result, _) = Run("main.msf", "{ export var x = 1; }\n");

        AssertError(result, DiagnosticCode.E3016);
    }

    [Fact]
    public void QualifiedCall_WrongArity_IsE3008()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./math.msf\";\nprint(math.square(1, 2));\n",
            ("math.msf", "export fn square(n) { return n * n; }\n"));

        AssertError(result, DiagnosticCode.E3008);
    }

    [Fact]
    public void Module_RuntimeError_ReportsModuleSourcePath()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./bad.msf\";\nprint(bad.go());\n",
            ("bad.msf", "export fn go() { return 1 / 0; }\n"));

        AssertError(result, DiagnosticCode.E4002);
        Assert.Equal(2, result.ExitCode);
        Assert.EndsWith("bad.msf", result.Diagnostic!.SourcePath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Module_SyntaxError_ReportsModuleSourcePath()
    {
        var (result, _) = Run(
            "main.msf",
            "import \"./bad.msf\";\n",
            ("bad.msf", "export fn go( { return 1; }\n"));

        Assert.Equal(1, result.ExitCode);
        Assert.NotNull(result.Diagnostic);
        Assert.EndsWith("bad.msf", result.Diagnostic!.SourcePath, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertError(RunResult result, string code)
    {
        Assert.NotNull(result.Diagnostic);
        Assert.Equal(code, result.Diagnostic!.Code);
    }

    private static (RunResult Result, string Stdout) Run(
        string mainFile,
        string mainText,
        params (string Name, string Text)[] files)
    {
        var dir = Path.Combine(Path.GetTempPath(), "msf-mod-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            foreach (var (name, text) in files)
            {
                File.WriteAllText(Path.Combine(dir, name), text, new UTF8Encoding(false));
            }

            var mainPath = Path.Combine(dir, mainFile);
            File.WriteAllText(mainPath, mainText, new UTF8Encoding(false));
            var source = new SourceText(mainPath, mainText);
            var stdout = new StringWriter { NewLine = "\n" };
            var result = MsfEngine.Run(source, stdout, new ModuleLoadContext(dir));
            return (result, stdout.ToString());
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch (IOException) { }
        }
    }
}
