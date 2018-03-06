using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ClassyCLI
{
    [DebuggerDisplay("{Value} {Offset} {Next != null}")]
    internal class Argument
    {
        public string Value { get; private set; }
        public int Offset { get; private set; }
        public Argument Next { get; private set; }

        public static Argument Parse(IEnumerable<string> arguments)
        {
            if (arguments == null) return null;

            Argument first = null;
            Argument last = null;

            foreach (var arg in arguments)
            {
                var current = new Argument
                {
                    Value = arg,
                };

                if (first == null)
                {
                    first = last = current;
                }
                else
                {
                    last.Next = current;
                    current.Offset = last.Offset + last.Value.Length + 1;
                    last = current;
                }
            }

            return first;
        }

        /// <summary>
        /// (Attempt to) parse line into arguments the same way a shell would
        /// </summary>
        public static Argument Parse(string line)
        {
            Argument arg = null;
            var all = new [] { ' ', '"', '\'' };
            var seps = all;
            var ix = line.IndexOfAny(seps);

            if (ix == -1)
            {
                return new Argument
                {
                    Value = line,
                    Offset = 0,
                };
            }

            var prev = ' ';
            var sep = line[ix];
            if (sep != ' ') seps = null;

            var first = arg;
            var last = 0;
            while (true)
            {
                var len = ix - last;
                if (len > 0 || (sep == prev && sep != ' '))
                {
                    SetArgument(ref arg, ref first, line.Substring(last, len), last);
                }
                last = ix + 1;

                ix = seps == null ? line.IndexOf(sep, last) : line.IndexOfAny(seps, last);
                if (ix == -1)
                {
                    if (last < line.Length)
                    {
                        SetArgument(ref arg, ref first, line.Substring(last), last);
                    }
                    break;
                }

                prev = sep;
                sep = line[ix];
                if (seps == null)
                {
                    seps = all;
                }
                else if (sep != ' ') 
                {
                    seps = null;
                }
            }

            return first;
        }

        public Argument Remove(Argument previous, Argument match)
        {
            if (previous == null)
            {
                return match.Next;
            }
            previous.Next = match.Next;
            return this;
        }

        // unconditionally remove first occurence of value
        // this is probably not usable in the general `Run` case since presumably context will matter
        public Argument Remove(string value)
        {
            var arg = this;
            var head = arg;
            Argument prev = null;
            while (arg != null && !string.Equals(arg.Value, value, StringComparison.Ordinal))
            {
                prev = arg;
                arg = arg.Next;
            }

            if (arg != null)
            {
                // match was head
                if (prev == null)
                {
                    return arg.Next;
                }

                prev.Next = arg.Next;
            }

            return head;
        }

        private static void SetArgument(ref Argument arg, ref Argument first, string value, int offset)
        {
            var next = new Argument
            {
                Value = value,
                Offset = offset,
            };

            if (first == null)
            {
                first = next;
            }
            else
            {
                arg.Next = next;
            }

            arg = next;
        }

        public void Trim(int position)
        {
            var it = this;
            var tail = it;
            while (it != null && position >= it.Offset + it.Value.Length)
            {
                tail = it;
                it = it.Next;
            }

            // FIXME needs tests on this
            if (position > tail.Offset + tail.Value.Length)
            {
                var next = tail.Next;
                if (next == null)
                {
                    tail.Next = new Argument
                    {
                        Offset = position,
                        Value = "",
                    };
                }
                else
                {
                    next.Value = next.Value.Substring(0, position - next.Offset);
                }
            }
            else
            {
                tail.Value = tail.Value.Substring(0, position - tail.Offset);
                tail.Next = null;
            }
        }
    }
}
