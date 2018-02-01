using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Xunit;

namespace ClassyCLI.Test
{
    public class HelpTest
    {
        StringBuilder _sb;
        TextWriter _tw;

        public HelpTest()
        {
            _sb = new StringBuilder();
            _tw = new StringWriter(_sb);
        }

        [Description("This is the foo")]
        public class Foo
        {
            public static void Run(int number, DayOfWeek day) { }
        }

        [Description("The bar stuff")]
        public class Bar
        {
            public static void Run(
                [Description("Really a number")] int number = 0, 
                [Description("A date for something")] DateTime? timestamp = null) { }
        }

        /// <summary>
        /// mmmmmmmm
        /// </summary>
        public class Bop
        {

        }

        [Fact]
        public void Test1()
        {
            var types = new[] { typeof(Foo), typeof(Bar) };
            Run(types, "--help");
            Run(types, "foo --help");
            Run(types, "bar --help");

            Approvals.Approve(_sb);
        }

        private void Run(IEnumerable<Type> types, string line)
        {
            _tw.WriteLine("```");
            _tw.WriteLine(line);
            _tw.WriteLine("```");
            _tw.WriteLine();

            var arg = Argument.Parse(line);
            arg = arg.Remove("--help");
            Runner.Help(types, arg, _tw);

            _tw.WriteLine();
            _tw.WriteLine();
        }
    }
}
