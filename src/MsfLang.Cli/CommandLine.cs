using System;
using System.IO;
using System.Text;
using MsfLang.Core;
using MsfLang.Core.Diagnostics;
using MsfLang.Core.Source;

namespace MsfLang.Cli;

internal static class CommandLine
{
    public static int Run(string[] args)
    {
        try
        {
            Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            Console.Out.NewLine = "\n";

            if (args.Length == 0)
            {
                PrintUsage(Console.Error);
                return ExitCode.UsageError;
            }

            return args[0] switch
            {
                "run" => RunCommand(args),
                "repl" => ReplCommand(args),
                "--help" or "-h" => Help(),
                "--version" or "-v" => Version(),
                _ => Unknown(args[0])
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"internal error: {ex.Message}");
            return ExitCode.InternalError;
        }
    }

    private static int RunCommand(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("error[E5002]: invalid usage: 'msf run' expects exactly one <path> argument");
            return ExitCode.UsageError;
        }

        var path = args[1];
        string text;
        try
        {
            text = File.ReadAllText(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
        }
        catch (FileNotFoundException)
        {
            Console.Error.WriteLine($"error[E5003]: file not found: {path}");
            return ExitCode.IoError;
        }
        catch (DirectoryNotFoundException)
        {
            Console.Error.WriteLine($"error[E5003]: file not found: {path}");
            return ExitCode.IoError;
        }
        catch (DecoderFallbackException)
        {
            Console.Error.WriteLine($"error[E5005]: file is not valid UTF-8: {path}");
            return ExitCode.IoError;
        }
        catch (Exception)
        {
            Console.Error.WriteLine($"error[E5004]: cannot read file: {path}");
            return ExitCode.IoError;
        }

        var source = new SourceText(path, text);
        var result = MsfEngine.Run(source, Console.Out);
        if (result.Diagnostic is not null)
        {
            Console.Error.Write(DiagnosticFormatter.Format(result.Diagnostic, source));
        }

        return result.ExitCode;
    }

    private static int ReplCommand(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("error[E5002]: invalid usage: 'msf repl' takes no arguments");
            return ExitCode.UsageError;
        }

        Console.Error.WriteLine("error[E5002]: invalid usage: repl not implemented yet");
        return ExitCode.UsageError;
    }

    private static int Help()
    {
        PrintUsage(Console.Out);
        return ExitCode.Success;
    }

    private static int Version()
    {
        Console.Out.Write("msf 0.1.0\n");
        return ExitCode.Success;
    }

    private static int Unknown(string command)
    {
        Console.Error.WriteLine($"error[E5001]: unknown command '{command}'");
        return ExitCode.UsageError;
    }

    private static void PrintUsage(TextWriter writer)
    {
        writer.Write(
            "Usage:\n" +
            "  msf run <path>\n" +
            "  msf repl\n" +
            "  msf --help | -h\n" +
            "  msf --version | -v\n");
    }
}
