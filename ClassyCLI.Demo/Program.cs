using System;
using System.Collections.Generic;
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

    public enum MyEnum
    {
        Enums,
        Are,
        Great,
        For,
        Completion,
        But,
        Have,
        No,
        Methods,
    }

    public struct Foo
    {
        public int _value;
        public Foo(int value)
        {
            _value = value;
        }

        public int HopeYouLikeDefaults() => _value;

        public static MyEnum Bar(MyEnum it) => it;
        public static MyEnum? Bar2(MyEnum? it) => it;
        public static void Bar3(params MyEnum[] it) {}
        public static void Bar4(params MyEnum?[] it) {}
        public static void Bar5(List<MyEnum> it) {}
        public static void Bar6(List<MyEnum?> it) {}
        public static void Bar7(Dictionary<MyEnum?, string> it) {}
        public static void Bar8(Dictionary<MyEnum?, string>[] it) {}

        // properties are currently skipped 
        public static int PropertiesSomeday { get => 42; }

        // indexers are currently skipped 
        public int this[int i] { get { return  i; } }
    }

    public delegate void WeSkipThese();

    public interface IHopeWeSkipThis {}

    public abstract class HowAbstract
    {
        public static void ThisWorks() { }
        public abstract void ThisDoesNot();
        public static void ThisAlsoDoesNot<T>(T item) { }
    }

    public class Generic<T>
    {
        public virtual string Nope() => "nope";

        // would have to .MakeGenericType with dummy params
        public static void MaybeSomeday() { }
    }

    public class NotSo : Generic<int>
    {
        public override string Nope() => "yup";
    }
}
