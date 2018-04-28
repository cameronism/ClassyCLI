using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ClassyCLI
{
    internal class RunnerBuilder : IRunnerBuilder
    {
        private List<Type> _typeList;
        private IEnumerable<Type> _types;
        private Func<Type, object> _factory;
        private TextWriter _stdout;
        private TextWriter _stderr;

        // following should also be configurable
        private bool _ignoreCase = true;
        private bool _skipInitial = true;

        public IRunnerResult Run(IEnumerable<string> arguments)
        {
            var invocation = new Invocation(
                stdout: _stdout ?? Console.Out,
                stderr: _stderr ?? Console.Error,
                ignoreCase: _ignoreCase);

            if (_skipInitial) arguments = arguments.Skip(1);

            invocation.Factory = _factory;
            return invocation.Invoke(arguments, _typeList ?? _types);
        }

        public IRunnerBuilder WithInstanceProvider(Func<Type, object> factory)
        {
            _factory = factory;
            return this;
        }

        public IRunnerBuilder WithType<T>() => WithType(typeof(T));

        public IRunnerBuilder WithType(Type type)
        {
            GetList().Add(type);
            return this;
        }

        public IRunnerBuilder WithTypes(IEnumerable<Type> types)
        {
            if (_typeList == null && _types == null)
            {
                _types = types;
            }
            else
            {
                GetList().AddRange(types);
            }
            return this;
        }

        public IRunnerBuilder WithAssembly(Assembly a) => WithTypes(a.GetTypes());

        public IRunnerBuilder WithAssemblies(IEnumerable<Assembly> aa)
        {
            foreach (var a in aa)
            {
                WithTypes(a.GetTypes());
            }
            return this;
        }

        private List<Type> GetList()
        {
            var list = _typeList;
            if (list != null)
            {
                return list;
            }

            list = new List<Type>();
            _typeList = list;
            var types = _types;
            if (types != null)
            {
                list.AddRange(types);
                _types = null;
            }

            return list;
        }

        public IRunnerBuilder WithStdout(TextWriter tw)
        {
            _stdout = tw;
            return this;
        }

        public IRunnerBuilder WithStderr(TextWriter tw)
        {
            _stderr = tw;
            return this;
        }
    }
}