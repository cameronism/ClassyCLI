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
        }

        [Fact]
        public void Test1()
        {
            Approvals.Approve(_sb);
        }
    }
}
