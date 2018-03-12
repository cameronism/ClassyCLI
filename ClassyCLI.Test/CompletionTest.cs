using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ClassyCLI.Test
{
    public class CompletionTest
    {
        private static StringBuilder _sb;

        public CompletionTest()
        {
            _sb = new StringBuilder();
            TestExtensions.SetTextWriter(new StringWriter(_sb));
        }

        private class C1
        {
            public void M1() { }
        }

        private class C2
        {
            public void M1() { }
            public void M2() { }

            // override System.Object methods to make sure they are not considered
            public override bool Equals(object obj) => throw new NotImplementedException();
            public override int GetHashCode() => throw new NotImplementedException();
            public override string ToString() => throw new NotImplementedException();
        }

        private class C3
        {
            public void M1(int foo = 0, int bar = 0) { }
            public void M2(DayOfWeek d) { }
            public void M3(DayOfWeek? d) { }
            public void M4(bool b) { }
            public void M5(bool? b) { }
            public void M6(params DayOfWeek[] d) { }
            public void M7(int foo1, int foo2) { }

            // System.DateTimeKind is another "simple" enum
        }

        private class C20 : C2 { }

        private class C4
        {
            public void M1(int foo = 0, int bar = 0) { }
            public class C5
            {
                public void M1(int foo = 0, int bar = 0) { }
            }
        }

        private class TestInvocation : Invocation
        {
            public TestInvocation(TextWriter stdout, TextWriter stderr, bool ignoreCase)
            : base(stdout, stderr, ignoreCase)
            {
            }

            public List<string> Completions { get; set; } = new List<string>();

            protected override void AddCompletion(string value) => Completions.Add(value);

            protected override int ExitCode
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
        }

        [Fact]
        public void Test1()
        {
            "Unambiguous types: C1 vs C2".H2();
            var types = new[] { typeof(C1), typeof(C2) };

            Complete(types, "c2.");
            Complete(types, "c2");

            "Ambiguous type names: C2 vs C20".H2();
            types = new[] { typeof(C1), typeof(C2), typeof(C20) };
            Complete(types, "c2.");
            Complete(types, "c2");

            "Nested class at same level as methods".H2();
            types = new[] { typeof(C4), typeof(C4.C5) };
            Complete(types, "c4.");

            "Method completions".H2();
            types = new[] { typeof(C1), typeof(C2), typeof(C20) };
            Complete(types, "c2.m");
            Complete(types, "c2.m1");

            types = new[] { typeof(C1), typeof(C2) };
            Complete(types, "C1.");
            Complete(types, "C1.m");
            Complete(types, "C1.M");
            Complete(types, "C1.m1");
            Complete(types, "C2.");
            Complete(types, "C2.m1");
            Complete(types, "C2.x");

            Complete(types, "C");
            Complete(types, "c1");
            Complete(types, "c1.");

            "Should return no completions".H2();
            Complete(types, "xx");
            Complete(types, "xx.");
            Complete(types, "xx.yy");

            "Parameter name completion".H2();
            types = new[] { typeof(C1), typeof(C2), typeof(C3) };
            Complete(types, "C3.M1 -");
            Complete(types, "C3.M1 -f");
            Complete(types, "C3.M1 -foo");
            Complete(types, "C3.M1 -FOO");
            Complete(types, "C3.M1 -b");
            Complete(types, "C3.M1 -bar");
            Complete(types, "C3.M1 -BaR");

            "Track which parameters have already been used".H2();
            Complete(types, "C3.M1 -foo 1 -b");
            Complete(types, "C3.M1 -foo 1 -");
            Complete(types, "C3.M1 -bar 1 -");

            "Don't try to complete numbers".H2();
            Complete(types, "C3.M1 1");
            Complete(types, "C3.M1 1 2");
            Complete(types, "C3.M1 11");
            Complete(types, "C3.M1 11 22");

            "No parameter names after `--`".H2();
            Complete(types, "C3.M1 -- ");
            Complete(types, "C3.M1 -- -");
            Complete(types, "C3.M1 -- -f");
            Complete(types, "C3.M1 1 -- ");
            Complete(types, "C3.M1 1 -- -");
            Complete(types, "C3.M1 1 -- -f");
            Complete(types, "C3.M1 11 -- ");
            Complete(types, "C3.M1 11 -- -");
            Complete(types, "C3.M1 11 -- -f");
            Complete(types, "C3.M1 -bar 1 -- ");
            Complete(types, "C3.M1 -bar 1 -- -");
            Complete(types, "C3.M1 -bar 1 -- -f");
            Complete(types, "C3.M1 -bar 11 -- ");
            Complete(types, "C3.M1 -bar 11 -- -");
            Complete(types, "C3.M1 -bar 11 -- -f");

            "Value completion".H2();
            Complete(types, "C3.M2 -- ");
            Complete(types, "C3.M2 S");
            Complete(types, "C3.M2 s");
            Complete(types, "C3.M3 -- ");
            Complete(types, "C3.M3 S");
            Complete(types, "C3.M3 s");
            Complete(types, "C3.M3 -d ");
            Complete(types, "C3.M3 -d S");
            Complete(types, "C3.M3 -d s");

            Complete(types, "C3.M2 ");
            Complete(types, "C3.M3 ");

            "Mid word completion".H2();
            Complete(types, "C3.M3 -d szz", -3);
            Complete(types, "C3.M3 -d szz", -2);
            Complete(types, "C3.M3 -d szz", -1);

            "Bool completion".H2();
            Complete(types, "C3.M4 -b t");
            Complete(types, "C3.M4 -b ");

            "Null completion".H2();
            Complete(types, "C3.M5 -b n");
            Complete(types, "C3.M5 -b ");

            "Params(-ish) parameters can be used multiple times".H2();
            Complete(types, "C3.M6 -d ");
            Complete(types, "C3.M6 -d Sunday -d ");

            "Handle similarly named parameters".H2();
            Complete(types, "C3.M7 -");
            Complete(types, "C3.M7 -f");
            Complete(types, "C3.M7 -foo");
            Complete(types, "C3.M7 -foo1");

            Approvals.Approve(_sb);
        }

        private void Complete(Type[] types, string line) => Complete(types, line, line.Length);

        private void Complete(Type[] types, string line, int position)
        {
            if (position < 0) position = line.Length + position;

            _sb.AppendLine("```");
            _sb.AppendLine(line);
            for (int i = 0; i < position; i++)
            {
                _sb.Append(' ');
            }
            _sb.AppendLine("^");
            _sb.AppendLine("```");
            _sb.AppendLine();

            var invocation = new TestInvocation(null, null, ignoreCase: true);
            invocation.InvokeCompletion(types, "foo " + line, position + 4);
            var completions = invocation.Completions;
            var any = false;
            foreach (var completion in completions)
            {
                _sb.AppendLine("- " + completion);
                any = true;
            }

            if (!any)
            {
                _sb.AppendLine("_no completions_");
            }

            _sb.AppendLine();
            _sb.AppendLine();
        }

    }
}
