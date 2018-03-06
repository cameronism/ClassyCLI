using System;
using System.ComponentModel;
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

        [Description("things go here")]
        public class Things
        {
            [Description("let's take a nap")]
            public Task Sleep(int millis = 100) => Task.Delay(millis);
        }
    }
}
