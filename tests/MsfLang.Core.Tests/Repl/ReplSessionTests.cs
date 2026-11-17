using System.Text;
using MsfLang.Core.Modules;
using MsfLang.Core.Repl;
using MsfLang.Core.Runtime;
using Xunit;

namespace MsfLang.Core.Tests.Repl;

public sealed class ReplSessionTests
{
    [Fact]
    public void BasicSequence_Works()
    {
        var repl = new ReplSession(new StringWriter());
        var r1 = repl.Evaluate("let x = 5;");
        Assert.True(r1.Ok);
        var r2 = repl.Evaluate("x * 2;");
        Assert.True(r2.Ok);
        Assert.Single(r2.Displays);
        Assert.Equal("10", ValueFormatter.Format(r2.Displays[0]));
        var r3 = repl.Evaluate("fn square(n) { return n * n; }");
        Assert.True(r3.Ok);
        var r4 = repl.Evaluate("square(x);");
        Assert.True(r4.Ok);
        Assert.Single(r4.Displays);
        Assert.Equal("25", ValueFormatter.Format(r4.Displays[0]));
    }

    [Fact]
    public void CompileError_DoesNotChangeState()
    {
        var repl = new ReplSession(new StringWriter());
        Assert.True(repl.Evaluate("let x = 1;").Ok);

        var err = repl.Evaluate("let x = 2;"); // duplicate in same global scope
        Assert.False(err.Ok);
        Assert.False(err.IsIncomplete);
        Assert.NotNull(err.Diagnostic);
        Assert.Equal("E3002", err.Diagnostic!.Code);

        var ok = repl.Evaluate("x;");
        Assert.True(ok.Ok);
        Assert.Single(ok.Displays);
        Assert.Equal("1", ValueFormatter.Format(ok.Displays[0]));
    }

    [Fact]
    public void RuntimeError_RollsBackDeclarationsAndAssignmentsInEntry()
    {
        var stdout = new StringWriter { NewLine = "\n" };
        var repl = new ReplSession(stdout);
        Assert.True(repl.Evaluate("var a = 1;").Ok);

        // Runtime error inside entry: declaration/assignment in same entry must roll back,
        // but output already printed remains.
        var err = repl.Evaluate("var b = 5; a = 2; print(a); 1 / 0;");
        Assert.False(err.Ok);
        Assert.False(err.IsIncomplete);
        Assert.NotNull(err.Diagnostic);
        Assert.Equal("E4002", err.Diagnostic!.Code);
        Assert.Equal("2\n", stdout.ToString().Replace("\r\n", "\n"));

        var a = repl.Evaluate("a;");
        Assert.True(a.Ok);
        Assert.Equal("1", ValueFormatter.Format(a.Displays[0]));

        var b = repl.Evaluate("b;");
        Assert.False(b.Ok);
        Assert.Equal("E3001", b.Diagnostic!.Code);
    }

    [Fact]
    public void Import_Module_UsableAcrossEntries()
    {
        var dir = Path.Combine(Path.GetTempPath(), "msf-repl-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var modulePath = Path.Combine(dir, "math.msf");
        File.WriteAllText(modulePath, "export fn square(n) { return n * n; }\nvar helper = 1;\n", new UTF8Encoding(false));

        try
        {
            var modules = new ModuleLoadContext(dir);
            var stdout = new StringWriter { NewLine = "\n" };
            var repl = new ReplSession(stdout, modules);

            var r1 = repl.Evaluate("import \"./math.msf\";");
            Assert.True(r1.Ok);

            var r2 = repl.Evaluate("math.square(7);");
            Assert.True(r2.Ok);
            Assert.Single(r2.Displays);
            Assert.Equal("49", ValueFormatter.Format(r2.Displays[0]));
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch (IOException) { }
        }
    }

    [Fact]
    public void IncompleteInput_IsDetected()
    {
        var repl = new ReplSession(new StringWriter());

        var block = repl.Evaluate("fn f() {");
        Assert.False(block.Ok);
        Assert.True(block.IsIncomplete);

        var comment = repl.Evaluate("/* hello");
        Assert.False(comment.Ok);
        Assert.True(comment.IsIncomplete);

        // String with raw newline is a lexical error, not a continuable incomplete entry.
        var badString = repl.Evaluate("print(\"hi\n\");");
        Assert.False(badString.Ok);
        Assert.False(badString.IsIncomplete);
        Assert.Equal("E1003", badString.Diagnostic!.Code);
    }
}
