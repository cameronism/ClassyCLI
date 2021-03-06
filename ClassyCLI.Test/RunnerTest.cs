using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;

namespace ClassyCLI.Test
{
    public class RunnerTest
    {
        private static StringBuilder _sb;
        private static JsonConverter[] _converters = new[] { new Newtonsoft.Json.Converters.StringEnumConverter { } };
        private static Task _task;
        private static ValueTask<object> _valueTask; // oh the irony

        private static void Log<T>(T instance, object arguments, [CallerMemberName] string member = "")
        {
            _sb.AppendLine($"`{instance.GetType().Name}.{member}`");
            _sb.AppendLine();

            var argsType = arguments.GetType();
            var args = argsType
                .GetConstructors()
                .Select(ctor => ctor.GetParameters())
                .OrderByDescending(ps => ps.Length)
                .FirstOrDefault();

            foreach (var param in args)
            {
                $"{param.Name} {param.ParameterType.GetTypeName()}".H3();

                var value = argsType.GetProperty(param.Name).GetValue(arguments);

                // skip types that can't / shouldn't be serialized
                if (!(value is Stream || value is TextReader || value is FileInfo || value is DirectoryInfo || value is TextWriter))
                {
                    _sb.AppendLine("```");
                    _sb.AppendLine(JsonConvert.SerializeObject(value, Formatting.Indented, _converters));
                    _sb.AppendLine("```");
                }

                _sb.AppendLine();
            }
            _sb.AppendLine();
        }

        public RunnerTest()
        {
            _sb = new StringBuilder();
            TestExtensions.SetTextWriter(new StringWriter(_sb));
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
            public void OI(string s, IList<DateTime> d) => Log(this, new { s, d });
            public void OJ(IEnumerable<int> ii = null) => Log(this, new { ii });
            public void OK(bool b) => Log(this, new { b });
            public void OL(bool? b) => Log(this, new { b });
            public void OM(CustomType ct) => Log(this, new { ct });
            public void OP(System.Text.RegularExpressions.Regex ct) => Log(this, new { ct });
            public Task ON() { Log(this, new { }); return _task; }
            public ValueTask<object> OO() { Log(this, new { }); return _valueTask; }
            public void OQ(int foo1 = 1, int foo2 = 2) => Log(this, new { foo1, foo2 });
        }

        private class E2 : E1
        {
            public void TwoVoid() => Log(this, new { });
        }

        [TypeConverter(typeof(CustomTypeConverter)), Newtonsoft.Json.JsonObject]
        private class CustomType
        {
            [Newtonsoft.Json.JsonProperty]
            public string Value { get; set; }

            public CustomType(string value)
            {
                Value = value;
            }
        }

        private class CustomTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string);
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) => new CustomType((string)value);
        }

        [Fact]
        public void Test1()
        {
            var types = new[] { typeof(E1) };

            "strings are easy".H1();

            Run("E1.O1 42", types);

            "other types".H1();

            Run("E1.O2 42", types);

            Run("E1.O3 42", types);

            Run("E1.O4 1", types);

            Run("E1.O4 Sunday", types);

            Run("E1.O5 tuesday", types);

            Run(new[] { "E1.O5", "" }, types);

            Run(new[] { "E1.O3", "" }, types);

            Run("E1.O6", types);

            Run("E1.O7 2017-10-28 Thursday", types);

            Run("E1.O7 2017-10-28", types);

            Run("E1.O8 -", types);
            Run("E1.O9 -", types);
            Run("E1.O9", types);
            Run("E1.OA -", types);

            var tmp = "./test.tmp.txt";
            // create a little clutter :shrug:
            File.WriteAllText(tmp, "");

            Run(new[] { "E1.O8", tmp }, types);
            Run(new[] { "E1.O9", tmp }, types);
            Run(new[] { "E1.OA", tmp }, types);
            Run(new[] { "E1.OB", tmp }, types);
            Run("E1.OC .", types);
            Run("E1.OD -", types);

            "Do not allow TextWriter to open existing file (by default)".H1();
            Run(new[] { "E1.OD", tmp }, types, typeof(IOException));

            "let the named arguments begin".H1();
            Run("E1.O1 -s hello", types);
            Run("E1.O7 -d 2017-10-28 Thursday", types);
            Run("E1.O7 -d 2017-10-28 -w Thursday", types);
            Run("E1.O7 -w Thursday -d 2017-10-28", types);
            Run("E1.O7 -w Thursday 2017-10-28", types);

            "let me put param-ish weird characters in my string".H1();
            Run("E1.OE -- -a -b", types);
            Run("E1.OE -b bbbbb -- -a", types);
            Run("E1.OE -a aaaaa -- -b", types);

            "params methods should be easy to invoke".H1();
            Run("E1.OF a b c", types);
            Run("E1.OG a b c d", types);
            Run("E1.OH 1 2", types);
            Run("E1.OI s 2018-01-01 2019-01-01", types);
            Run("E1.OI -s s 2018-01-01 2019-01-01", types);
            Run("E1.OI s -d 2018-01-01", types);
            Run("E1.OI -s s -d 2018-01-01", types);
            Run("E1.OI -d 2018-01-01 -s s -d 2019-01-01", types);
            Run("E1.OJ", types);
            Run("E1.OJ 1", types);


            "multiple candidate classes".H1();
            types = new[] { typeof(E1), typeof(E2) };
            Run("E2.OJ 1", types);
            Run("E2.TwoVoid", types);


            Run("E1.OK true", types);
            Run("E1.OL true", types);
            Run("E1.OL null", types);

            Run("E1.OM foo", types);
            Run("E1.OP foo", types);

            "ambiguous argument name".H1();
            Run("E1.OQ -foo 42", types);


            "tasks".H1();
            var tcs = new TaskCompletionSource<object>();
            ThreadPool.QueueUserWorkItem(delegate { Thread.Sleep(1); tcs.SetResult(null); });
            _task = tcs.Task;
            Run("E1.ON", types);
            _sb.AppendLine(tcs.Task.Status.ToString());
            _sb.AppendLine();
            _sb.AppendLine();
            _task = null;


            // Value Task
            tcs = new TaskCompletionSource<object>();
            ThreadPool.QueueUserWorkItem(delegate { Thread.Sleep(1); tcs.SetResult(null); });
            _valueTask = new ValueTask<object>(tcs.Task);
            Run("E1.OO", types);
            _sb.AppendLine(tcs.Task.Status.ToString());
            _sb.AppendLine();
            _sb.AppendLine();
            _valueTask = default(ValueTask<object>);


            // FIXME need to test bad invocations
            // there's a *lot* of ways to get this to blow up right now


            Approvals.Approve(_sb);
        }

        private void Run(string args, IEnumerable<Type> classes) => Run(args.Split((string[])null, StringSplitOptions.None), classes);

        private void Run(string[] args, IEnumerable<Type> classes, Type expectException = null)
        {
            $"Running {string.Join(" ", args)}".H2();
            var errsb = new StringBuilder();
            var invocation = new Invocation(stdout: null, stderr: new StringWriter(errsb), ignoreCase: true);
            InvocationResult result = null;

            try
            {
                if (expectException == null)
                {
                    // no try/catch for easier debugging by default
                    result =  invocation.Invoke(args, classes);
                    return;
                }

                try
                {
                    result =  invocation.Invoke(args, classes);
                }
                catch (Exception e)
                {
                    if (expectException != null && expectException.IsAssignableFrom(e.GetType()))
                    {

                        $"Exception {expectException.GetTypeName()}".H2();
                        _sb.AppendLine();
                        return;
                    }

                    throw;
                }
            }
            finally
            {
                if (errsb.Length != 0)
                {
                    _sb.AppendLine("stderr:");
                    _sb.AppendLine("```");
                    _sb.Append(errsb.ToString());
                    _sb.AppendLine("```");
                    _sb.AppendLine();
                }

            }
        }

        [Fact]
        public void PrefixTest()
        {
            Run();
            Run("foo");
            Run("foo", "foo");
            Run("foo", "bar");
            Run("foo", "foo", "foo");
            Run("foo", "foo", "bar");
            Run("foo.bar");
            Run("foo.bar", "foo.baz");
            Run("foo.bar.who", "foo.bar.why");
            Run("foo.bar.who", "foo.bop.why");
            Run("foo.bar.who", "foo.bar.zoo", "foo.bar.why");
            Run("foo.bar.who", "foo.bar.zoo", "foo.bop.why");

            Run("foo.bar", "foo");
            Run("zoo.foo.bar", "zoo.foo");

            Run("1.2.3.4", "1.2.3.4");
            Run("1.2.3.4", "1.2.3.5");
            Run("1.2.3.4", "1.2.0.4");
            Run("1.2.3.4", "1.0.3.4");
            Run("1.2.3.4", "0.2.3.4");

            Run("1.2.3.4", "1.2.3.4", "1.2.3.4");
            Run("1.2.3.4", "1.2.3.4", "1.2.3.5");
            Run("1.2.3.4", "1.2.4.4", "1.3.3.4");

            Approvals.Approve(_sb);

            void Run(params string[] names)
            {
                var any = false;
                foreach (var name in names)
                {
                    any = true;
                    _sb.AppendLine($"- {name}");
                }
                if (!any)
                {
                    _sb.AppendLine("_no names_");
                }
                _sb.AppendLine();

                var prefix = Candidate.CommonPrefix(names);
                if (prefix == null)
                {
                    _sb.AppendLine("_null_");
                }
                else if (prefix == "")
                {
                    _sb.AppendLine("_empty_");
                }
                else
                {
                    _sb.AppendLine($"`{prefix}`");
                }

                _sb.AppendLine();
                _sb.AppendLine();
            }
        }
    }
}
