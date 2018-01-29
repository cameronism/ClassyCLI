using System;

namespace ClassyCLI.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-------------");
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
                foreach (var b in System.Text.Encoding.Unicode.GetBytes(arg))
                {
                    Console.Write($"{b:x2}");
                }
                Console.WriteLine();
            }
        }
    }
}
