﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ClassyCLI.Test
{
    public class ArgumentTest
    {
        StringBuilder _sb = new StringBuilder();

        [Fact]
        public void Parsing()
        {
            Parse("foo");
            Parse("foo bar");
            Parse("foo bar bop");
            Parse("foo  bar   bop");
            Parse("foo  ");
            Parse("foo ");
            Parse("foo  bar  ");
            Parse("foo bar ");
            Parse(" foo bar ");
            Parse(" foo ");
            Parse("  foo  bar  ");
            Parse("  foo  ");
            Parse("\"foo bar bop\"");
            Parse("\"foo bar bop\" baz");
            Parse("\"foo bar bop\" baz a");
            Parse("\"foo bar bop\" \"baz\" a");
            Parse("\"foo bar bop\" \"baz\" \"a\"");
            Parse("\"foo bar bop\" \" baz\" \"a\"");
            Parse("\"foo bar bop\" \"baz \" \"a\"");
            Parse("\"foo bar bop\" \"baz\" \"a b\"");
            Parse("'foo bar bop'");
            Parse("'foo bar bop' baz");
            Parse("'foo bar bop' baz a");
            Parse("'foo bar bop' 'baz' a");
            Parse("'foo bar bop' 'baz' 'a'");
            Parse("'foo bar bop' ' baz' 'a'");
            Parse("'foo bar bop' 'baz ' 'a'");
            Parse("'foo bar bop' 'baz' 'a b'");
            Parse("\"\"");
            Parse(" \"\"");
            Parse(" \"\" ");
            Parse("\"\" \"\"");
            Parse("\"\" \"\" \"\"");
            Parse("''");
            Parse(" ''");
            Parse(" '' ");
            Parse("'' ''");
            Parse("'' '' ''");
            Parse("' ' \" \" ' '");
            Parse("' ' \" \" '\"' \"'\"");
            Parse("  foo  ' bar '  \"  bop  \"");

            // incomplete quoted strings
            Parse("foo 'bar");
            Parse("foo \"bar");
            Parse("' bar");
            Parse("\" bar");
            Parse("' bar ");
            Parse("\" bar ");


            // FIXME
            // back slashes
            // needs to work in powershell, bash and zsh


            Approvals.Approve(_sb);
        }

        private void Parse(string line)
        {
            _sb.AppendLine(line);
            _sb.AppendLine();

            var arg = Argument.Parse(line);
            while (arg != null)
            {
                _sb.AppendLine($"{arg.Offset:d2} {arg.Value.Length:d2} {arg.Value}");
                arg = arg.Next;
            }

            _sb.AppendLine();
            _sb.AppendLine();
        }
    }
}
