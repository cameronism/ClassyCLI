using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ConsoleDump;

namespace ClassyCLI.Demo
{
    class Program
    {
        public static string[] Arguments { get; private set; }

        static void Main(string[] args)
        {
            // File.AppendAllLines("/Users/cameron/demo.log", args);
            // File.AppendAllLines("/Users/cameron/demo.log", new[] { "------------------------------------------" });
            Arguments = args;
            var result = ClassyCLI.Runner.Run();

            if (result != null)
            {
                result.Dump();
            }
        }
    }

    [Description("we put the stuff here")]
    public class Stuff
    {
        [Description("k?")]
        public string[] Gimme() => new [] { "you're", "welcome" };

        [Description("just print the original arguments")]
        public void Args(params string[] _)
        {
            var args = Program.Arguments;

            Console.WriteLine("-------------");
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
                foreach (var b in System.Text.Encoding.UTF8.GetBytes(arg))
                {
                    Console.Write($"{b:x2}");
                }
                Console.WriteLine();
            }
        }

        public void Bash(string alias)
        {
            var dll = GetType().Assembly.Location;
            var lines = new[]
            {
                $"alias {alias}=\"dotnet {dll}\"",
                $"_{alias}_bash_complete()",
                $"{{",
                $"  local word=${{COMP_WORDS[COMP_CWORD]}}",
                $"  local {alias}path=${{COMP_WORDS[1]}}",

                $"  local completions=(\"$(dotnet {dll} --complete --position ${{COMP_POINT}} \"${{COMP_LINE}}\")\")",
                $"  COMPREPLY=( $(compgen -W \"$completions\" -- \"$word\") )",
                $"}}",
                $"complete -f -F _{alias}_bash_complete {alias}",
            };
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }

        public void Powershell(string alias)
        {
            var dll = GetType().Assembly.Location;
            var lines = new[]
            {
                $"# PowerShell parameter completion shim for {alias}",
                $"function {alias} {{ dotnet {dll} $args }}",
                $"Register-ArgumentCompleter -Native -CommandName {alias} -ScriptBlock {{",
                $"  param($commandName, $wordToComplete, $cursorPosition)",
                $"  dotnet {dll} --complete --position $cursorPosition \"$wordToComplete\" | ForEach-Object {{",
                $"    [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)",
                $"  }}",
                $"}}",
            };
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }

        [Description("things go here")]
        public class Things
        {
            [Description("let's take a nap")]
            public Task Sleep(int millis = 100) => Task.Delay(millis);
        }
    }
}
