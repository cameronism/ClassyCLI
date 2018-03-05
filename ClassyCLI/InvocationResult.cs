using System;

namespace ClassyCLI
{
    internal class InvocationResult
    {
        public InvocationStatus InvocationStatus { get; set; }
        public object Result { get; set; }
        public Exception Exception { get; set; }
    }
}