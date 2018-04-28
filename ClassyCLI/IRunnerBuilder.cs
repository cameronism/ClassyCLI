using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ClassyCLI
{
    public interface IRunnerBuilder
    {
        IRunnerBuilder WithInstanceProvider(Func<Type, object> factory);
        IRunnerResult Run(IEnumerable<string> arguments);
        IRunnerBuilder WithType<T>();
        IRunnerBuilder WithType(Type type);
        IRunnerBuilder WithTypes(IEnumerable<Type> types);
        IRunnerBuilder WithAssembly(Assembly a);
        IRunnerBuilder WithAssemblies(IEnumerable<Assembly> aa);
        IRunnerBuilder WithStdout(TextWriter tw);
        IRunnerBuilder WithStderr(TextWriter tw);
    }
}