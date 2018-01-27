using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace ClassyCLI.Test
{
    public class RunnerTest
    {
        private static StringBuilder _sb;
        private static JsonConverter[] _converters = new[] { new Newtonsoft.Json.Converters.StringEnumConverter { } };

        private static void Log<T>(T instance, object arguments, [CallerMemberName] string member = "")
        {
            _sb.AppendLine($"# Invoked {instance.GetType().Name} {member}");
            _sb.AppendLine();

            var argsType = arguments.GetType();
            var args = argsType
                .GetConstructors()
                .Select(ctor => ctor.GetParameters())
                .OrderByDescending(ps => ps.Length)
                .FirstOrDefault();

            foreach (var param in args)
            {
                _sb.AppendLine($"## {param.Name} {param.ParameterType.GetTypeName()}");
                _sb.AppendLine();

                var value = argsType.GetProperty(param.Name).GetValue(arguments);

                // skip types that can't / shouldn't be serialized
                if (!(value is Stream || value is TextReader || value is FileInfo || value is DirectoryInfo || value is TextWriter))
                {
                    _sb.AppendLine(JsonConvert.SerializeObject(value, Formatting.Indented, _converters));
                }

                _sb.AppendLine();
            }
            _sb.AppendLine();
        }

        public RunnerTest()
        {
            _sb = new StringBuilder();
        }

        private class E1
        {
            public void O1(string s) => Log(this, new { s });
            public void O2(int i) => Log(this, new { i });
            public void O3(int? i) => Log(this, new { i });
            public void O4(DayOfWeek d) => Log(this, new { d });
            public void O5(DayOfWeek? d) => Log(this, new { d });
            public void O6(DayOfWeek? d = DayOfWeek.Friday) => Log(this, new { d });
            public void O7(DateTime d, DayOfWeek? w = DayOfWeek.Friday) => Log(this, new { d, w });
            public void O8(Stream s) => Log(this, new { s });
            public void O9(Stream s = null) => Log(this, new { s });
            public void OA(TextReader t) => Log(this, new { t });
            public void OB(FileInfo f) => Log(this, new { f });
            public void OC(DirectoryInfo d) => Log(this, new { d });
            public void OD(TextWriter t) => Log(this, new { t });
            public void OE(string a, string b) => Log(this, new { a, b });
            public void OF(params string[] ss) => Log(this, new { ss });
            public void OG(List<object> oo) => Log(this, new { oo });
            public void OH(IEnumerable<int> ii) => Log(this, new { ii });
            public void OI(string s, IList<DateTime> dd) => Log(this, new { s, dd });
            public void OJ(IEnumerable<int> ii = null) => Log(this, new { ii });
            public void OK(bool b) => Log(this, new { b });
            public void OL(bool? b) => Log(this, new { b });
        }

        private class E2 : E1
        {
            public void TwoVoid() => Log(this, new { });
        }

        [Fact]
        public void Test1()
        {
            var types = new[] { typeof(E1) };

            _sb.AppendLine("strings are easy");
            _sb.AppendLine();

            Run("E1 O1 42", types);

            _sb.AppendLine("other types");
            _sb.AppendLine();

            Run("E1 O2 42", types);

            Run("E1 O3 42", types);

            Run("E1 O4 1", types);

            Run("E1 O4 Sunday", types);

            Run("E1 O5 tuesday", types);

            Run(new[] { "E1", "O5", "" }, types);

            Run(new[] { "E1", "O3", "" }, types);

            Run("E1 O6", types);

            Run("E1 O7 2017-10-28 Thursday", types);

            Run("E1 O7 2017-10-28", types);

            Run("E1 O8 -", types);
            Run("E1 O9 -", types);
            Run("E1 O9", types);
            Run("E1 OA -", types);

            var tmp = "./test.tmp.txt";
            // create a little clutter :shrug:
            File.WriteAllText(tmp, "");

            Run(new[] { "E1", "O8", tmp }, types);
            Run(new[] { "E1", "O9", tmp }, types);
            Run(new[] { "E1", "OA", tmp }, types);
            Run(new[] { "E1", "OB", tmp }, types);
            Run("E1 OC .", types);
            Run("E1 OD -", types);

            _sb.AppendLine("Do not allow TextWriter to open existing file (by default)");
            Run(new[] { "E1", "OD", tmp }, types, typeof(IOException));

            // let the named arguments begin
            Run("E1 O1 -s hello", types);
            Run("E1 O7 -d 2017-10-28 Thursday", types);
            Run("E1 O7 -d 2017-10-28 -w Thursday", types);
            Run("E1 O7 -w Thursday -d 2017-10-28", types);
            Run("E1 O7 -w Thursday 2017-10-28", types);

            // let me put param-ish weird characters in my string
            Run("E1 OE -- -a -b", types);
            Run("E1 OE -b bbbbb -- -a", types);
            Run("E1 OE -a aaaaa -- -b", types);

            // params methods should be easy to invoke
            Run("E1 OF a b c", types);
            Run("E1 OG a b c d", types);
            Run("E1 OH 1 2", types);
            Run("E1 OI s 2018-01-01 2019-01-01", types);
            Run("E1 OI -s s 2018-01-01 2019-01-01", types);
            Run("E1 OI s -d 2018-01-01", types);
            Run("E1 OI -s s -d 2018-01-01", types);
            Run("E1 OI -d 2018-01-01 -s s -d 2019-01-01", types);
            Run("E1 OJ", types);
            Run("E1 OJ 1", types);


            // multiple candidate classes
            types = new[] { typeof(E1), typeof(E2) };
            Run("E2 OJ 1", types);
            Run("E2 TwoVoid", types);


            Run("E1 OK true", types);
            Run("E1 OL true", types);
            Run("E1 OL null", types);

            // optional class or method names 
            // - (sometimes) 
            // - or just ignore class names and assume method name is first
            // completion
            // help
            // help for incomplete params
            // runme attribute
            // marker interface


            // multiple classes
            // optional / default params
            // stdin and file names to Stream or StreamReader param
            // named params at cli
            // mixed positional and named params
            // enumerable parameter (it's very greedy)

            Approvals.Approve(_sb);
        }

        private void Run(string args, IEnumerable<Type> classes) => Run(args.Split((string[])null, StringSplitOptions.None), classes);

        private void Run(string[] args, IEnumerable<Type> classes, Type expectException = null)
        {
            _sb.AppendLine($"# Running {string.Join(" ", args)}");
            _sb.AppendLine();

            if (expectException == null)
            {
                // no try/catch for easier debugging by default
                Runner.Run(args, classes);
                return;
            }

            try
            {
                Runner.Run(args, classes);
            }
            catch (Exception e)
            {
                if (expectException != null && expectException.IsAssignableFrom(e.GetType()))
                {

                    _sb.AppendLine($"# Exception {expectException.GetTypeName()}");
                    _sb.AppendLine();
                    _sb.AppendLine();
                    return;
                }

                throw;
            }
        }
    }
}
