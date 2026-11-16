using System;
using System.IO;
using System.Text;
using System.Threading;
using MsfLang.Core;
using MsfLang.Core.Diagnostics;
using MsfLang.Core.Source;
using MsfLang.Core.Runtime;

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

    private sealed class StackResult<T>
    {
        public T? Value { get; set; }
        public Exception? Error { get; set; }
    }

    private static T RunOnDedicatedStack<T>(Func<T> action)
    {
        var box = new StackResult<T>();
        var thread = new Thread(() =>
        {
            try { box.Value = action(); }
            catch (Exception ex) { box.Error = ex; }
        }, 32 * 1024 * 1024);
        thread.Start();
        thread.Join();
        if (box.Error is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(box.Error).Throw();
        }
        return box.Value!;
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
        var result = RunOnDedicatedStack(() => MsfEngine.Run(source, Console.Out));
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

        var interactive = !Console.IsInputRedirected;
        var session = new Core.Repl.ReplSession(Console.Out);
        if (interactive)
        {
            Console.Out.Write("MSF Lang REPL v0.1.0\n");
            Console.Out.Write("Type :help for help, :quit to exit.\n");
        }

        var buffer = new StringBuilder();
        var continuing = false;
        while (true)
        {
            if (interactive)
            {
                Console.Out.Write(continuing ? "...> " : "msf> ");
                Console.Out.Flush();
            }

            var line = Console.In.ReadLine();
            if (line is null)
            {
                if (continuing && buffer.Length > 0)
                {
                    // EOF while incomplete -> evaluate to get incomplete diagnostic
                    var result = RunOnDedicatedStack(() => session.Evaluate(buffer.ToString()));
                    if (!result.Ok && result.Diagnostic is not null)
                    {
                        var src = new SourceText("<repl>", buffer.ToString());
                        Console.Error.Write(DiagnosticFormatter.Format(result.Diagnostic, src));
                    }
                }
                return ExitCode.Success;
            }

            if (!continuing)
            {
                var trimmed = line.Trim();
                if (trimmed == ":quit") return ExitCode.Success;
                if (trimmed == ":help")
                {
                    Console.Out.Write("Commands:\n  :help  Show this help\n  :quit  Exit the REPL\n");
                    continue;
                }
                if (trimmed.StartsWith(':'))
                {
                    Console.Error.WriteLine($"error[E5001]: unknown command '{trimmed}'");
                    continue;
                }
            }

            if (buffer.Length > 0) buffer.Append('\n');
            buffer.Append(line);

            if (continuing && line.Length == 0)
            {
                var pending = buffer.ToString();
                buffer.Clear();
                continuing = false;
                var result = RunOnDedicatedStack(() => session.Evaluate(pending));
                if (!result.Ok && result.Diagnostic is not null)
                {
                    var src = new SourceText("<repl>", pending);
                    Console.Error.Write(DiagnosticFormatter.Format(result.Diagnostic, src));
                }
                continue;
            }

            var eval = RunOnDedicatedStack(() => session.Evaluate(buffer.ToString()));
            if (!eval.Ok && eval.IsIncomplete)
            {
                continuing = true;
                continue;
            }

            continuing = false;
            var sourceText = buffer.ToString();
            buffer.Clear();

            if (!eval.Ok)
            {
                if (eval.Diagnostic is not null)
                {
                    var src = new SourceText("<repl>", sourceText);
                    Console.Error.Write(DiagnosticFormatter.Format(eval.Diagnostic, src));
                }
                continue;
            }

            foreach (var value in eval.Displays)
            {
                Console.Out.Write(Core.Runtime.ValueFormatter.Format(value));
                Console.Out.Write('\n');
            }
        }
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
            "  msf --version | -v\n" +
            "\n" +
            "Commands:\n" +
            "  msf run <path>   Execute a .msf program\n" +
            "  msf repl         Start an interactive or piped REPL\n" +
            "  msf --help       Show this help\n" +
            "  msf --version    Print version\n" +
            "\n" +
            "MSF Lang v0.1 scope: values (number/string/boolean/nil), let/var block scope,\n" +
            "if/else-if/else, functions (top-level only, direct recursion), built-in print,\n" +
            "strict boolean conditions, UTF-8 source, stable diagnostics (E1xxx-E5xxx).\n" +
            "Specification: https://github.com/Mustafa1998-tech/msf-lang/blob/master/docs/en/SPEC-v0.1.md\n");
    }
}
