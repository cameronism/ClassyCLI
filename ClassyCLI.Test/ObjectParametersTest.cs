using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using Xunit;

namespace ClassyCLI.Test
{
    public class ObjectParametersTest
    {
        private static StringBuilder _sb;
        private static JsonConverter[] _converters = new[] { new Newtonsoft.Json.Converters.StringEnumConverter { } };

        public ObjectParametersTest()
        {
            _sb = new StringBuilder();
            TestExtensions.SetTextWriter(new StringWriter(_sb));
        }

        private class BC
        {
            public int Foo { get; set; }
            public string Bar { get; set; }
        }

        private class CC
        {
            /// <summary>
            /// This is foo
            /// </summary>
            public int Foo { get; set; }
            [Description("This is bar")]
            public string Bar { get; set; }
        }

        private class DC
        {
            [Required]
            public int? Foo { get; set; }
            [Required]
            public string Bar { get; set; }
        }

        private class A
        {
            public BC B(BC bc) => bc;
            public CC C(CC cc) => cc;
            public DC D(DC dc) => dc;
        }

        [Fact]
        public void Run()
        {
            Invoke("my.exe a.b -foo 1 -bar 2", new[] { typeof(A) });
            Invoke("my.exe a.b -foo foo -bar 2", new[] { typeof(A) });
            Invoke("my.exe a.b --help", new[] { typeof(A) });
            Invoke("my.exe a.c --help", new[] { typeof(A) });
            Invoke("my.exe a.d", new[] { typeof(A) });
            Invoke(new[] { "my.exe", "a.d", "-foo", "42", "-bar", "" }, new[] { typeof(A) });
            Invoke("my.exe a.d -foo 21 -bar bop", new[] { typeof(A) });

            Approvals.Approve(_sb);
        }

        private void Invoke(string args, IEnumerable<Type> classes) => Invoke(args.Split(), classes);

        private void Invoke(string[] args, IEnumerable<Type> classes)
        {
            $"Running {string.Join(" ", args)}".H2();
            var errsb = new StringBuilder();
            var stdout = new StringBuilder();

            var builder = Runner.Configure()
                .WithStderr(new StringWriter(errsb))
                .WithStdout(new StringWriter(stdout))
                .WithTypes(classes);

            var result = builder.Run(args);
            _sb.AppendLine(result.InvocationStatus.ToString());
            _sb.AppendLine();
            _sb.AppendLine(result.Result?.GetType().GetTypeName());
            _sb.AppendLine();
            _sb.AppendLine("```");
            _sb.AppendLine(JsonConvert.SerializeObject(result.Result, Formatting.Indented, _converters));
            _sb.AppendLine("```");
            _sb.AppendLine();

            if (stdout.Length != 0)
            {
                _sb.AppendLine("stdout:");
                _sb.AppendLine("```");
                _sb.Append(stdout.ToString());
                _sb.AppendLine("```");
                _sb.AppendLine();
            }

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
}
