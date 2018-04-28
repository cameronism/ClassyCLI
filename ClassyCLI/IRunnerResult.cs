
using System;
using System.Reflection;

namespace ClassyCLI
{
    public interface IRunnerResult
    {
        InvocationStatus InvocationStatus { get; }
        object Result { get; }
        Exception Exception { get; }
        MethodInfo Method { get; }
    }
}