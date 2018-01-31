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
        }

        [Description("The bar stuff")]
        public class Bar
        {
        }

        [Fact]
        public void Test1()
        {
            var types = new[] { typeof(Foo), typeof(Bar) };
            Run(types, "--help");

            Approvals.Approve(_sb);
        }

        private void Run(IEnumerable<Type> types, string line)
        {
            _tw.WriteLine("```");
            _tw.WriteLine(line);
            _tw.WriteLine("```");
            _tw.WriteLine();

            Runner.Help(types, line, _tw);

            _tw.WriteLine();
            _tw.WriteLine();
        }
    }
}
