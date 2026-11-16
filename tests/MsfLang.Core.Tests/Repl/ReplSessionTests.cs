using MsfLang.Core.Repl;
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
        var r3 = repl.Evaluate("fn square(n) { return n * n; }");
        Assert.True(r3.Ok);
        var r4 = repl.Evaluate("square(x);");
        Assert.True(r4.Ok);
        Assert.Single(r4.Displays);
    }
}
