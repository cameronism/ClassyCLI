using System;
using System.Collections.Generic;
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

            // System.DateTimeKind is another "simple" enum
        }

        [Fact]
        public void Test1()
        {
            var types = new[] { typeof(C1), typeof(C2) };

            Complete(types, "C1 ");
            Complete(types, "C1 m");
            Complete(types, "C1 M");
            Complete(types, "C1 m1");
            Complete(types, "C2 ");
            Complete(types, "C2 m1");
            Complete(types, "C2 x");

            Complete(types, "C");
            Complete(types, "c1");
            Complete(types, "c1 ");

            // should return no matches
            Complete(types, "xx");
            Complete(types, "xx ");
            Complete(types, "xx yy");

            types = new[] { typeof(C1), typeof(C2), typeof(C3) };
            // arguments
            Complete(types, "C3 M1 -");
            Complete(types, "C3 M1 -f");
            Complete(types, "C3 M1 -foo");
            Complete(types, "C3 M1 -FOO");
            Complete(types, "C3 M1 -b");
            Complete(types, "C3 M1 -bar");
            Complete(types, "C3 M1 -BaR");
            Complete(types, "C3 M1 -foo 1 -b");
            Complete(types, "C3 M1 -foo 1 -");
            Complete(types, "C3 M1 -bar 1 -");
            Complete(types, "C3 M1 1");
            Complete(types, "C3 M1 1 2");
            Complete(types, "C3 M1 11");
            Complete(types, "C3 M1 11 22");
            Complete(types, "C3 M1 -- ");
            Complete(types, "C3 M1 -- -");
            Complete(types, "C3 M1 -- -f");
            Complete(types, "C3 M1 1 -- ");
            Complete(types, "C3 M1 1 -- -");
            Complete(types, "C3 M1 1 -- -f");
            Complete(types, "C3 M1 11 -- ");
            Complete(types, "C3 M1 11 -- -");
            Complete(types, "C3 M1 11 -- -f");
            Complete(types, "C3 M1 -bar 1 -- ");
            Complete(types, "C3 M1 -bar 1 -- -");
            Complete(types, "C3 M1 -bar 1 -- -f");
            Complete(types, "C3 M1 -bar 11 -- ");
            Complete(types, "C3 M1 -bar 11 -- -");
            Complete(types, "C3 M1 -bar 11 -- -f");

            // value completion
            Complete(types, "C3 M2 -- ");
            Complete(types, "C3 M2 S");
            Complete(types, "C3 M2 s");
            Complete(types, "C3 M3 -- ");
            Complete(types, "C3 M3 S");
            Complete(types, "C3 M3 s");
            Complete(types, "C3 M3 -d ");
            Complete(types, "C3 M3 -d S");
            Complete(types, "C3 M3 -d s");

            Complete(types, "C3 M2 ");
            Complete(types, "C3 M3 ");

            Complete(types, "C3 M3 -d szz", -3);
            Complete(types, "C3 M3 -d szz", -2);
            Complete(types, "C3 M3 -d szz", -1);

            Complete(types, "C3 M4 -b t");
            Complete(types, "C3 M4 -b ");
            Complete(types, "C3 M5 -b n");
            Complete(types, "C3 M5 -b ");

            Complete(types, "C3 M6 -d ");
            Complete(types, "C3 M6 -d Sunday -d ");

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

            var completions = Runner.Complete(line, position, types);
            var any = false;
            foreach (var completion in completions)
            {
                _sb.AppendLine(completion);
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
