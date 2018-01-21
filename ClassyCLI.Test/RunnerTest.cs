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

        private static string GetFilePath([CallerFilePath] string path = "") => path;

        private static string GetTypeName(Type t)
        {
            if (!t.IsGenericType) return t.FullName;

            var name = t.FullName;
            var ix = name.IndexOf('`');
            name = name.Substring(0, ix);

            return $"{name}<{string.Join(", ", t.GetGenericArguments().Select(a => GetTypeName(a)))}>";
        }

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
                _sb.AppendLine($"## {param.Name} {GetTypeName(param.ParameterType)}");
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
            //Run("E1 OF a b c", types);




            // named params at cli
            // mixed positional and named params
            // multiple classes
            // optional class or method names 
            // - (sometimes) 
            // - or just ignore class names and assume method name is first
            // help
            // help for incomplete params
            // completion
            // runme attribute
            // marker interface

            // optional / default params
            // stdin and file names to Stream or StreamReader param

            Approve(_sb);
        }

        private static void Approve(StringBuilder sb, [CallerFilePath] string path = "", [CallerMemberName] string member = "", string extension = "md")
        {
            // FIXME use approval tests or something here
            var approved = Path.ChangeExtension(path, $".{member}.approved.{extension}");
            var received = Path.ChangeExtension(path, $".{member}.received.{extension}");

            File.WriteAllText(received, sb.ToString());

            Assert.Equal(expected: File.ReadAllText(approved), actual: File.ReadAllText(received), ignoreLineEndingDifferences: true);

            // if previous line didn't throw then cleanup
            File.Delete(received);
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

                    _sb.AppendLine($"# Exception {GetTypeName(expectException)}");
                    _sb.AppendLine();
                    _sb.AppendLine();
                    return;
                }

                throw;
            }
        }
    }
}
