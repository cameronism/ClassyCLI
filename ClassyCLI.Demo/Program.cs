using System;
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
    public class Stuff
    {
        public string[] Gimme() => new [] { "you're", "welcome" };

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

        public class Things
        {
            public Task Sleep(int millis = 100) => Task.Delay(millis);
        }
    }
}
