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

        /// <summary>
        /// (Attempt to) parse line into arguments the same way a shell would
        /// </summary>
        public static Argument Parse(string line)
        {
            Argument arg = null;
            var ix = line.IndexOf(' ');

            if (ix == -1)
            {
                return new Argument
                {
                    Value = line,
                    Offset = 0,
                };
            }

            var first = arg;
            var last = 0;
            while (true)
            {
                var len = ix - last;
                if (len > 0)
                {
                    SetArgument(ref arg, ref first, line.Substring(last, len), last);
                }
                last = ix + 1;

                ix = line.IndexOf(' ', last);
                if (ix == -1)
                {
                    if (last < line.Length)
                    {
                        SetArgument(ref arg, ref first, line.Substring(last), last);
                    }
                    break;
                }
            }

            return first;
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
                    next.Value = next.Value.Substring(0, position - tail.Offset);
                }
            }
            else
            {
                tail.Next = null;
            }
        }
    }
}
