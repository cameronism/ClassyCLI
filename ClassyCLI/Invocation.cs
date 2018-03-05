using System;
using System.IO;

namespace ClassyCLI
{
    internal class Invocation
    {
        protected readonly StringComparison _comparison;
        protected readonly bool _ignoreCase;
        protected readonly TextWriter _stdout;
        protected readonly TextWriter _stderr;

        protected Invocation(TextWriter stdout, TextWriter stderr, bool ignoreCase)
        {
            _stdout = stdout;
            _stderr = stderr;
            _comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            _ignoreCase = ignoreCase;
        }

        protected virtual int ExitCode
        {
            get => Environment.ExitCode;
            set => Environment.ExitCode = value;
        }
    }
}