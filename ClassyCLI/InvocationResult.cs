using System;
using System.Reflection;

namespace ClassyCLI
{
    internal class InvocationResult : IRunnerResult
    {
        public InvocationStatus InvocationStatus { get; set; }
        public object Result { get; set; }
        public Exception Exception { get; set; }
        public MethodInfo Method { get; set; }
    }
}