﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ClassyCLI.Test")]
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
                if (TryGetEnumerableItem(info.ParameterType, out var item))
                {
                    return new EnumerableParameter(_createList.MakeGenericMethod(item, info.ParameterType));
                }

                return new Parameter();
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

        private static void SetComparison(bool ignoreCase)
        {
            _comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            _ignoreCase = ignoreCase;
        }

        public static void Run(string[] arguments, IEnumerable<Type> types)
        {
            // red, green, refactor
            // this is going to be stupid simple for a while

            // TODO make param
            SetComparison(ignoreCase: true);

            var classArg = arguments[0];
            var methodArg = arguments[1];

            var cls = GetClasses(types, classArg).Single();

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

        private static IEnumerable<Type> GetClasses(IEnumerable<Type> types, string classArg)
        {
            return types
                .Where(m => m.Name.IndexOf(classArg, _comparison) != -1);
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

            // remove leading and trailing prefix characters
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
            if (underlying != null)
            {
                if (s == "")
                {
                    // leave param null
                    return null;
                }
                else if (string.Equals(s, "null", _comparison))
                {
                    if (underlying == typeof(bool))
                    {
                        return null;
                    }

                    // TODO there are probably a lot more types where should turn the string "null" to `null`
                }
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

        internal static IEnumerable<string> Complete(string line, int position, IEnumerable<Type> types)
        {
            // TODO make param
            SetComparison(ignoreCase: true);

            // class name
            var arg = Argument.Parse(line);

            // ignore everything after `position`
            // doing anything intelligent based on what comes later sounds _very_ challenging
            arg.Trim(position);

            var classes = GetClasses(types, arg.Value).ToList();
            if (classes.Count != 1 || arg.Next == null)
            {
                return classes.Select(GetClassName);
            }

            var cls = classes.Single();

            // method name
            arg = arg.Next;
            var methods = cls.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => OriginalDeclaringType(m) != typeof(object));

            methods = Matching(methods, arg?.Value, m => m.Name);

            if (arg?.Next == null)
            {
                return methods.Select(m => m.Name);
            }

            if (!TryGetSingle(methods, out var method))
            {
                return Enumerable.Empty<string>();
            }

            // parameter names
            arg = arg.Next;
            var parameters = new List<ParameterInfo>(method.GetParameters());
            ParameterInfo lastNamedParameter = null;
            var positionalOnly = false;

            while (arg.Next != null)
            {
                lastNamedParameter = null;

                if (!positionalOnly && arg.Value == "--")
                {
                    positionalOnly = true;
                }
                else if (!positionalOnly && PossibleParameterName(arg.Value))
                {
                    // Span<char> -- sigh
                    var value = arg.Value.Substring(1);

                    var ix = parameters.FindIndex(p => p.Name.Equals(value, _comparison));
                    if (ix != -1)
                    {
                        lastNamedParameter = parameters[ix];

                        // TODO cache MRU? *very* likely we're repeating the same call to TryGetEnumerableItem
                        if (!TryGetEnumerableItem(lastNamedParameter.ParameterType, out var itemType))
                        {
                            // don't consider this param name for completion anymore
                            // don't remove param if it supports multiple values
                            parameters.RemoveAt(ix);
                        }
                    }
                }

                arg = arg.Next;
            }

            if (!positionalOnly && ((arg.Value == "" && lastNamedParameter == null) || PossibleParameterName(arg.Value)))
            {
                // Span<char> -- sigh
                var value = arg.Value.Length >= 1 ? arg.Value.Substring(1) : arg.Value;
                return Matching(parameters.Select(p => p.Name), value).Select(p => '-' + p);
            }

            return GetParameterValueCompletions(arg, parameters, lastNamedParameter);
        }

        private static string GetClassName(Type t)
        {
            return t.Name;
        }

        private static Type OriginalDeclaringType(MethodInfo m)
        {
            if (!m.IsVirtual) return m.DeclaringType;

            // FIXME this should really be recursive
            var parameterTypes = m.GetParameters().Select(p => p.ParameterType).ToArray();
            var b = m.DeclaringType.BaseType;
            var bm = b?.GetMethod(m.Name, parameterTypes);

            return bm != null ? b : m.DeclaringType;
        }

        private static IEnumerable<string> GetParameterValueCompletions(Argument arg, List<ParameterInfo> parameters, ParameterInfo named)
        {
            IEnumerable<string> values = null;
            var param = named ?? parameters?.FirstOrDefault();
            if (param != null)
            {
                values = GetValueCompletions(param.ParameterType, arg);
            }

            return values ?? Enumerable.Empty<string>();
        }

        private static IEnumerable<string> GetValueCompletions(Type type, Argument arg)
        {
            if (TryGetEnumerableItem(type, out var item))
            {
                // do all completions based on item type
                type = item;
            }

            var original = type;
            type = Nullable.GetUnderlyingType(type) ?? type;

            IEnumerable<string> values = null;
            if (type.IsEnum)
            {
                values = Enum.GetNames(type);
            }
            else if (type == typeof(bool))
            {
                if (original == type)
                {
                    values = new[] { "true", "false" };
                }
                else
                {
                    values = new[] { "true", "false", "null" };
                }
            }

            // may want to handle the filtering differently for custom value completers

            if (values != null)
            {
                return Matching(values, arg?.Value);
            }


            return null;
        }

        private static IEnumerable<string> Matching(IEnumerable<string> completions, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return completions;
            }

            return completions.Where(c => c.StartsWith(value, _comparison));
        }

        private static IEnumerable<T> Matching<T>(IEnumerable<T> completions, string value, Func<T, string> selector)
        {
            if (string.IsNullOrEmpty(value))
            {
                return completions;
            }

            return completions.Where(c => selector(c).StartsWith(value, _comparison));
        }


        private static bool TryGetSingle<T>(IEnumerable<T> items, out T item)
        {
            using (var enumerator = items.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    item = default(T);
                    return false;
                }

                item = enumerator.Current;
                return !enumerator.MoveNext();
            }
        }

        private static bool PossibleParameterName(string s)
        {
            return s?.Length >= 1 && Array.IndexOf(_prefix, s[0]) != -1;
        }

        private static bool TryGetEnumerableItem(Type type, out Type item)
        {
            Type ienumerable = null;
            if (type != typeof(string))
            {
                ienumerable = new[] { type }
                    .Concat(type.GetInterfaces())
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            }

            if (ienumerable == null)
            {
                item = null;
                return false;
            }

            item = ienumerable.GetGenericArguments()[0];
            return true;
        }
    }
}
