using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ClassyCLI
{
    public class Runner
    {
        class Parameter
        {
            public string Name { get; private set; }
            public ParameterInfo ParameterInfo { get; private set; }
            public object[] Parameters { get; private set; }
            public int Index { get; private set; }
            public bool HasValue { get; protected set; }

            public virtual void SetValue(object s)
            {
                var value = ConvertValue(s, ParameterInfo.ParameterType, _ignoreCase);
                Parameters[Index] = value;
                HasValue = true;
            }

            public virtual void SetValue(string s)
            {
                var value = ConvertValue(s, ParameterInfo.ParameterType, _ignoreCase);
                Parameters[Index] = value;
                HasValue = true;
            }

            public virtual void SetFinalValue()
            {
            }

            public static (object[] args, Parameter[] info) Create(ParameterInfo[] ps)
            {
                var args = new object[ps.Length];
                var info = new Parameter[ps.Length];

                for (int i = 0; i < ps.Length; i++)
                {
                    var param = ps[i];
                    var p = CreateParameter(param);
                    info[i] = p;

                    p.Name = param.Name;
                    p.Index = i;
                    p.ParameterInfo = param;
                    p.Parameters = args;
                }

                return (args, info);
            }

            static MethodInfo _createList = typeof(ParameterList).GetMethod(nameof(ParameterList.Create));

            private static Parameter CreateParameter(ParameterInfo info)
            {
                var type = info.ParameterType;
                Type ienumerable = null;
                if (type != typeof(string))
                {
                    ienumerable = new[] { type }
                        .Concat(type.GetInterfaces())
                        .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                }

                if (ienumerable == null) return new Parameter();

                var item = ienumerable.GetGenericArguments()[0];
                return new EnumerableParameter(_createList.MakeGenericMethod(item, type));
            }

        }

        class EnumerableParameter : Parameter
        {
            private ParameterList _list;
            private MethodInfo _create;

            public EnumerableParameter(MethodInfo methodInfo)
            {
                _create = methodInfo;
            }

            public override void SetFinalValue() => Parameters[Index] = _list?.Convert();

            public override void SetValue(object s)
            {
                if (s != null) throw new NotSupportedException();
            }

            public override void SetValue(string s)
            {
                if (_list == null)
                {
                    _list = (ParameterList)_create.Invoke(null, null);
                }
                _list.Add(s);
                HasValue = true;
            }
        }

        abstract class ParameterList
        {
            public abstract void Add(string s);
            public abstract object Convert();
            public static ParameterList Create<TItem, TList>() => new ParameterList<TItem, TList>();
        }

        sealed class ParameterList<TItem, TList> : ParameterList
        {
            private List<TItem> _list = new List<TItem>();

            public override void Add(string s)
            {
                _list.Add((TItem)ConvertValue(s, typeof(TItem), _ignoreCase));
            }

            public override object Convert()
            {
                var destination = typeof(TList);
                if (destination.IsAssignableFrom(typeof(List<TItem>))) return _list;

                if (destination == typeof(TItem[]) || destination == typeof(Array)) return _list.ToArray();

                throw new NotImplementedException();
            }
        }

        public static void Run(string[] arguments, IEnumerable<Type> types)
        {
            // red, green, refactor
            // this is going to be stupid simple for a while

            var ignoreCase = true; // TODO make param
            _comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            _ignoreCase = ignoreCase;

            var classArg = arguments[0];
            var methodArg = arguments[1];

            var cls = types
                .Where(m => m.Name.IndexOf(classArg, _comparison) != -1)
                .Single();


            var method = cls.GetMethods()
                .Where(m => m.Name.StartsWith(methodArg, _comparison))
                .Single();

            var instance = method.IsStatic ? null : Activator.CreateInstance(cls);
            var (args, info) = Parameter.Create(method.GetParameters());

            Parameter destination = null;
            int positional = -1;
            var positionalOnly = false;
            var named = false;

            foreach (var a in arguments.Skip(2))
            {
                if (destination == null)
                {
                    if (!positionalOnly && TryGetNamedParam(a, info, out destination))
                    {
                        named = true;
                        continue;
                    }

                    if (a == "--")
                    {
                        positionalOnly = true;
                        continue;
                    }

                    do
                    {
                        positional++;
                        destination = info[positional];
                    } while (destination.HasValue);
                }

                destination.SetValue(a);

                if (named || !(destination is EnumerableParameter))
                {
                    destination = null;
                    named = false;
                }
            }

            foreach (var param in info)
            {
                if (param.HasValue)
                {
                    param.SetFinalValue();
                    continue;
                }

                var paramInfo = param.ParameterInfo;
                if (!paramInfo.HasDefaultValue)
                {
                    // FIXME this is the part where we'll want to be more helpful 
                    throw new Exception("need more");
                }

                param.SetValue(paramInfo.DefaultValue);
            }

            method.Invoke(instance, args);
        }

        static char[] _prefix = new[] { '-', '/', '@', '=' };
        static StringComparison _comparison;
        static bool _ignoreCase;

        // this will currently match a lot of weird things
        static bool TryGetNamedParam(string name, Parameter[] parameters, out Parameter param)
        {
            param = null;

            if (string.IsNullOrEmpty(name)) return false;

            var ix = name.IndexOfAny(_prefix);
            if (ix != 0) return false;

            // remove leading and trainiling prefix characters
            // technically remove them all -- super inefficient
            while (ix != -1)
            {
                name = name.Remove(ix, 1);
                ix = name.IndexOfAny(_prefix);
            }

            if (name.Length == 0) return false;


            param = parameters.SingleOrDefault(p => p.Name.StartsWith(name, _comparison));
            return param != null;
        }

        static object ConvertValue(object value, Type destination, bool ignoreCase)
        {
            if (value == null)
            {
                return null;
            }

            var s = value as string;
            if (destination == typeof(string) && s != null)
            {
                return value;
            }

            var underlying = Nullable.GetUnderlyingType(destination);
            if (underlying != null && s == "")
            {
                // leave param null
                return null;
            }

            var type = underlying ?? destination;
            if (type.IsEnum)
            {
                if (s != null) return Enum.Parse(type, s, ignoreCase: ignoreCase);

                return Enum.ToObject(type, value);
            }

            if (s != null)
            {
                if (type == typeof(Stream))
                {
                    if (s == "-") return Console.OpenStandardInput();

                    // try to treat s as path
                    if (TryGetFileInfo(s, out var fi)) return fi.OpenRead();

                    // FIXME this is the part where we'll want to be more helpful 
                    throw new Exception("can't find that");
                }

                if (type == typeof(TextReader))
                {
                    if (s == "-") return Console.In;

                    // try to treat s as path
                    if (TryGetFileInfo(s, out var fi)) return fi.OpenText();

                    // FIXME this is the part where we'll want to be more helpful 
                    throw new Exception("can't find that");
                }

                if (type == typeof(TextWriter))
                {
                    if (s == "-") return Console.Out;

                    // insist that a new file be created 
                    // not getting into the append / overwrite / confirm fun (yet)
                    var file = File.Open(s, FileMode.CreateNew);
                    return new StreamWriter(file);
                }

                // don't worry about if it exists
                if (type == typeof(FileInfo)) return new FileInfo(s);
                if (type == typeof(DirectoryInfo)) return new DirectoryInfo(s);
            }

            return Convert.ChangeType(value, type);
        }

        static bool TryGetFileInfo(string path, out FileInfo fi)
        {
            fi = new FileInfo(path);
            return fi.Exists;
        }

    }
}
