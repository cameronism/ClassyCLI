using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace ClassyCLI.Test
{
    public static class Approvals
    {
        public static void Approve(StringBuilder sb, [CallerFilePath] string path = "", [CallerMemberName] string member = "", string extension = "md")
        {
            // FIXME use approval tests or something here
            var approved = Path.ChangeExtension(path, $".{member}.approved.{extension}");
            var received = Path.ChangeExtension(path, $".{member}.received.{extension}");

            var actual = sb.ToString();
            File.WriteAllText(received, actual);

            Assert.Equal(expected: File.ReadAllText(approved), actual: actual, ignoreLineEndingDifferences: true);

            // if assertion didn't throw then cleanup
            File.Delete(received);
        }
    }
}
