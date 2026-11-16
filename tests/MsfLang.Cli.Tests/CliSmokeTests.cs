using System.Diagnostics;
using Xunit;

namespace MsfLang.Cli.Tests;

public sealed class CliSmokeTests
{
    [Fact]
    public void NoArgs_Exits3()
    {
        var (exit, _, _) = RunCli();
        Assert.Equal(3, exit);
    }

    [Fact]
    public void Version_Exits0()
    {
        var (exit, stdout, _) = RunCli("--version");
        Assert.Equal(0, exit);
        Assert.Equal("msf 0.1.0\n", stdout.Replace("\r\n", "\n"));
    }

    private static (int ExitCode, string Stdout, string Stderr) RunCli(params string[] args)
    {
        var repoRoot = FindRepoRoot();
        var cliDll = Path.Combine(repoRoot, "src", "MsfLang.Cli", "bin", "Release", "net10.0", "msf.dll");
        var psi = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add(cliDll);
        foreach (var a in args) psi.ArgumentList.Add(a);
        using var p = Process.Start(psi)!;
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
