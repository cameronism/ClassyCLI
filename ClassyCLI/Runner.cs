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
            public bool HasValue { get; private set; }

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
                    info[i] = new Parameter
                    {
                        Name = param.Name,
                        Index = i,
                        ParameterInfo = param,
                        Parameters = args,
                    };
                }

                return (args, info);
            }
        }

        class EnumerableParameter : Parameter
        {
            private ParameterList _list;

            public override void SetFinalValue() => Parameters[Index] = _list?.Convert();

            public override void SetValue(object s)
            {
                if (s != null) throw new NotSupportedException();
            }

            public override void SetValue(string s)
            {
                if (_list == null) CreateList();
                _list.Add(s);
            }

            private void CreateList()
            {
                throw new NotImplementedException();
            }
        }

        abstract class ParameterList
        {
            public abstract void Add(string s);
            public abstract object Convert();
            public ParameterList Create<TItem, TList>() => new ParameterList<TItem, TList>();
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
                .Single();


            var method = cls.GetMethods()
                .Where(m => m.Name.StartsWith(methodArg, _comparison))
                .Single();

            var instance = method.IsStatic ? null : Activator.CreateInstance(cls);
            var (args, info) = Parameter.Create(method.GetParameters());
            //var margs = method.GetParameters();
            //var parameters = new object[margs.Length];

            Parameter named = null;
            int positional = -1;
            var positionalOnly = false;
            foreach (var a in arguments.Skip(2))
            {
                Parameter destination;
                if (named != null)
                {
                    destination = named;
                    named = null;
                }
                else
                {
                    if (!positionalOnly && TryGetNamedParam(a, info, out named)) continue;

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

                //destination.Value = ConvertValue(a, destination.ParameterInfo.ParameterType, ignoreCase);
                destination.SetValue(a);
            }

            foreach (var param in info)
            {
                if (param.HasValue) continue;

                var paramInfo = param.ParameterInfo;
                if (!paramInfo.HasDefaultValue)
                {
                    // FIXME this is the part where we'll want to be more helpful 
                    throw new Exception("need more");
                }

                //param.Value = ConvertValue(paramInfo.DefaultValue, paramInfo.ParameterType, ignoreCase);
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
            // technically remove them all -- super ineffecient
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

            if (type == typeof(Stream) && s != null)
            {
                if (s == "-") return Console.OpenStandardInput();

                // try to treat s as path
                if (TryGetFileInfo(s, out var fi)) return fi.OpenRead();

                // FIXME this is the part where we'll want to be more helpful 
                throw new Exception("can't find that");
            }

            if (type == typeof(TextReader) && s != null)
            {
                if (s == "-") return Console.In;

                // try to treat s as path
                if (TryGetFileInfo(s, out var fi)) return fi.OpenText();

                // FIXME this is the part where we'll want to be more helpful 
                throw new Exception("can't find that");
            }

            if (type == typeof(TextWriter) && s != null)
            {
                if (s == "-") return Console.Out;

                // insist that a new file be created 
                // not getting into the append / overwrite / confirm fun (yet)
                var file = File.Open(s, FileMode.CreateNew);
                return new StreamWriter(file);
            }

            // don't worry about if it exists
            if (type == typeof(FileInfo) && s != null) return new FileInfo(s);
            if (type == typeof(DirectoryInfo) && s != null) return new DirectoryInfo(s);

            return Convert.ChangeType(value, type);
        }

        static bool TryGetFileInfo(string path, out FileInfo fi)
        {
            fi = new FileInfo(path);
            return fi.Exists;
        }

    }
}
