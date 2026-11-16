using System.Diagnostics;
using System.Text;
using Xunit;

namespace MsfLang.Cli.Tests;

public sealed class CliSmokeTests
{
    [Fact]
    public void NoArgs_Exits3()
    {
        var (exit, _, stderr) = RunCli();
        Assert.Equal(3, exit);
        Assert.Contains("Usage:", stderr.Replace("\r\n", "\n"));
    }

    [Fact]
    public void Version_Exits0()
    {
        var (exit, stdout, _) = RunCli("--version");
        Assert.Equal(0, exit);
        Assert.Equal("msf 0.1.0\n", Normalize(stdout));
    }

    [Fact]
    public void Help_Exits0()
    {
        var (exit, stdout, _) = RunCli("--help");
        Assert.Equal(0, exit);
        Assert.Contains("msf run <path>", Normalize(stdout));
    }

    [Fact]
    public void UnknownCommand_Exits3()
    {
        var (exit, _, stderr) = RunCli("nope");
        Assert.Equal(3, exit);
        Assert.Contains("error[E5001]", Normalize(stderr));
    }

    [Fact]
    public void Run_NoPath_Exits3()
    {
        var (exit, _, stderr) = RunCli("run");
        Assert.Equal(3, exit);
        Assert.Contains("error[E5002]", Normalize(stderr));
    }

    [Fact]
    public void Run_ExtraArgs_Exits3()
    {
        var (exit, _, stderr) = RunCli("run", "a.msf", "b");
        Assert.Equal(3, exit);
        Assert.Contains("error[E5002]", Normalize(stderr));
    }

    [Fact]
    public void Run_FileNotFound_Exits4()
    {
        var (exit, _, stderr) = RunCli("run", "does-not-exist-12345.msf");
        Assert.Equal(4, exit);
        Assert.Contains("error[E5003]", Normalize(stderr));
    }

    [Fact]
    public void Run_HelloFile_Exits0_PrintsOutput()
    {
        var path = Path.Combine(FindRepoRoot(), "examples", "hello.msf");
        var (exit, stdout, stderr) = RunCli("run", path);
        Assert.Equal(0, exit);
        Assert.Equal("Hello, MSF!\n", Normalize(stdout));
        Assert.Equal("", Normalize(stderr));
    }

    [Fact]
    public void Run_RuntimeError_Exits2()
    {
        var path = WriteTemp("print(10 / 0);\n");
        try
        {
            var (exit, stdout, stderr) = RunCli("run", path);
            Assert.Equal(2, exit);
            Assert.Equal("", Normalize(stdout));
            Assert.Contains("error[E4002]", Normalize(stderr));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Run_CompileError_Exits1()
    {
        var path = WriteTemp("print(x);\n");
        try
        {
            var (exit, _, stderr) = RunCli("run", path);
            Assert.Equal(1, exit);
            Assert.Contains("error[E3001]", Normalize(stderr));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Repl_Piped_ExecutesAndExits0()
    {
        var input = "let x = 5;\nx * 2;\n:quit\n";
        var (exit, stdout, stderr) = RunCliWithInput(input, "repl");
        Assert.Equal(0, exit);
        Assert.Equal("10\n", Normalize(stdout));
        Assert.Equal("", Normalize(stderr));
    }

    [Fact]
    public void Repl_ExtraArgs_Exits3()
    {
        var (exit, _, stderr) = RunCli("repl", "x");
        Assert.Equal(3, exit);
        Assert.Contains("error[E5002]", Normalize(stderr));
    }

    private static string Normalize(string s) => (s ?? "").Replace("\r\n", "\n");

    private static string WriteTemp(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), "msf-cli-test-" + Guid.NewGuid().ToString("N") + ".msf");
        File.WriteAllText(path, content, new UTF8Encoding(false));
        return path;
    }

    private static (int ExitCode, string Stdout, string Stderr) RunCli(params string[] args)
        => RunCliWithInput(null, args);

    private static (int ExitCode, string Stdout, string Stderr) RunCliWithInput(string? stdin, params string[] args)
    {
        var repoRoot = FindRepoRoot();
        var cliDll = Path.Combine(repoRoot, "src", "MsfLang.Cli", "bin", "Release", "net10.0", "msf.dll");
        var psi = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = stdin is not null,
            UseShellExecute = false,
            StandardOutputEncoding = new UTF8Encoding(false),
            StandardErrorEncoding = new UTF8Encoding(false),
        };
        psi.ArgumentList.Add(cliDll);
        foreach (var a in args) psi.ArgumentList.Add(a);
        using var p = Process.Start(psi)!;
        if (stdin is not null)
        {
            p.StandardInput.Write(stdin);
            p.StandardInput.Close();
        }
        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();
        return (p.ExitCode, stdout, stderr);
    }

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
}
