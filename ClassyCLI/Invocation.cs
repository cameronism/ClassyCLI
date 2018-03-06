using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ClassyCLI
{
    internal class Invocation
    {
        protected readonly StringComparison _comparison;
        protected readonly bool _ignoreCase;
        protected readonly TextWriter _stdout;
        protected readonly TextWriter _stderr;
        protected readonly char[] _prefix = new[] { '-', '/', '@', '=' };

        public Invocation(TextWriter stdout, TextWriter stderr, bool ignoreCase)
        {
            _stdout = stdout;
            _stderr = stderr;
            _comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            _ignoreCase = ignoreCase;
        }

        protected virtual int ExitCode
        {
            get => Environment.ExitCode;
            set { Environment.ExitCode = value; }
        }

        private void AddCompletions(IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                AddCompletion(value);
            }
        }

        protected virtual void AddCompletion(string value) => _stdout.WriteLine(value);

        public InvocationResult Invoke(IEnumerable<string> arguments, IEnumerable<Type> types)
        {
            var arg = Argument.Parse(arguments);
            var (outcome, previous, match) = GetOutcome(arg);
            if (outcome != InvocationStatus.MethodInvoked)
            {
                arg = arg.Remove(previous, match);
                if (outcome == InvocationStatus.Help)
                {
                    return InvokeHelp(arg, types, _stdout);
                }
                else if (outcome == InvocationStatus.Complete)
                {
                    return InvokeCompletion(arg, types);
                }
            }

            return InvokeMethod(arg, types);
        }

        public InvocationResult InvokeMethod(Argument arg, IEnumerable<Type> types)
        {
            var candidates = Candidate.FromTypes(types);
            var suggestion = GetSuggestions(arg?.Value ?? string.Empty, candidates);

            if (suggestion == null || suggestion.Method == null || suggestion.Next != null || suggestion.Value.Length != arg.Value.Length)
            {
                var status = string.IsNullOrEmpty(arg?.Value) ? InvocationStatus.NoMethodSpecified : InvocationStatus.NoMethodFound;
                if (status == InvocationStatus.NoMethodFound)
                {
                    _stderr.WriteLine("Method not found: " + arg.Value);
                }
                var helpResult = InvokeHelp(arg, types, _stderr);
                helpResult.InvocationStatus = status;
                ExitCode = 1;
                return helpResult;
            }

            var method = suggestion.Method;
            var instance = method.IsStatic ? null : Activator.CreateInstance(suggestion.Type);
            var (args, info) = Parameter.Create(method.GetParameters());

            arg = arg.Next;
            Parameter destination = null;
            int positional = -1;
            var positionalOnly = false;
            var named = false;

            while (arg != null)
            {
                var value = arg.Value;
                arg = arg.Next;

                if (destination == null)
                {
                    if (!positionalOnly && TryGetNamedParam(value, info, out destination))
                    {
                        named = true;
                        continue;
                    }

                    if (value == "--")
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

                destination.SetValue(value, _ignoreCase);

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

                param.SetValue(paramInfo.DefaultValue, _ignoreCase);
            }

            object result;
            try
            {
                result = method.Invoke(instance, args);
            }
            catch (Exception e)
            {
                return new InvocationResult
                {
                    Exception = e,
                    InvocationStatus = InvocationStatus.MethodFaulted,
                    Method = method,
                };
            }

            if (result != null)
            {
                result = HandleReturnValue(result, method.ReturnType);
            }

            return new InvocationResult
            {
                Result = result,
                InvocationStatus = InvocationStatus.MethodInvoked,
                Method = method,
            };
        }

        private static object HandleReturnValue(object result, Type returnType)
        {
            // slowly, painfully, as safely as (reasonably) possible, check for "awaitable" pattern then block on it
            // technically GetResult and ConfigureAwait are not required to be "awaitable" but ValueTask and Task support it so use that
            var configureAwait = returnType.GetMethod("ConfigureAwait", BindingFlags.Instance | BindingFlags.Public, null, new [] { typeof(bool) }, null);

            if (
                configureAwait != null &&
                TryGetMethod(configureAwait.ReturnType, "GetAwaiter", out var getAwaiter) &&
                typeof(ICriticalNotifyCompletion).IsAssignableFrom(getAwaiter.ReturnType) &&
                TryGetMethod(getAwaiter.ReturnType, "GetResult", out var getResult))
            {
                var configured = configureAwait.Invoke(result, new object[] { false });
                var awaiter = getAwaiter.Invoke(configured, null);
                result = getResult.Invoke(awaiter, null);
                returnType = getResult.ReturnType;
            }

            return result;
        }

        // Try to get public parameterless instance method
        private static bool TryGetMethod(Type type, string methodName, out MethodInfo mi)
        {
            mi = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            return mi != null;
        }

        private bool TryGetNamedParam(string name, Parameter[] parameters, out Parameter param)
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

        public InvocationResult InvokeHelp(Argument arg, IEnumerable<Type> types, TextWriter tw)
        {
            string description;
            var candidates = Candidate.FromTypes(types);
            var suggestion = GetSuggestions(arg?.Value ?? string.Empty, candidates);

            if (suggestion == null && arg?.Value != null)
            {
                tw.WriteLine("Method not found: " + arg.Value);
                return new InvocationResult { InvocationStatus = InvocationStatus.Help };
            }

            while (suggestion != null)
            {
                if (suggestion.Method == null)
                {
                    DescribeClass(candidates[suggestion.CandidateIndex], out description);
                    tw.WriteLine("{0,-23}{1}", suggestion.Value, description);
                }
                else
                {
                    var method = suggestion.Method;
                    var xml = XmlDocumentation.GetDocumentation(method);

                    tw.WriteLine("{0,-23}{1}", suggestion.Value, GetMethodDescription(method, xml));
                    foreach (var param in method.GetParameters())
                    {
                        tw.WriteLine("  -{0,-20}{1}", param.Name, DescribeParameter(param, xml));
                    }
                }
                tw.WriteLine();

                suggestion = suggestion.Next;
            }

            return new InvocationResult { InvocationStatus = InvocationStatus.Help };
        }


        private static string DescribeParameter(ParameterInfo param, KeyValuePair<string, string>[] xml)
        {
            var attr = param.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null)
            {
                return attr.Description;
            }

            if (xml != null && TryGetValue(xml, "param:" + param.Name, out var description))
            {
                return description;
            }

            var type = param.ParameterType;
            if (type.IsEnum)
            {
                return string.Join(" | ", Enum.GetNames(type));
            }

            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.DateTime:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.String:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    // _slightly_ better than nothing
                    return typeCode.ToString();
            }

            // _hopefully_ better than nothing
            return type.FullName;
        }

        private static void DescribeClass(Candidate candidate, out string description)
        {
            var type = candidate.Type;
            var attr = type.GetCustomAttribute<DescriptionAttribute>();

            if (attr != null)
            {
                description = attr.Description;
                return;
            }

            var docs = XmlDocumentation.GetDocumentation(type);
            if (docs != null && TryGetValue(docs, XmlDocumentation.Summary, out description))
            {
                description = XmlDocumentation.GetFirstLine(description);
                return;
            }

            description = null;
        }

        private string GetMethodDescription(MethodInfo method, KeyValuePair<string, string>[] docs)
        {
            var attr = method.GetCustomAttribute<DescriptionAttribute>();
            if (attr != null) return attr.Description;

            if (docs != null && TryGetValue(docs, XmlDocumentation.Summary, out var description))
            {
                return XmlDocumentation.GetFirstLine(description);
            }

            return null;
        }

        private static bool TryGetValue(KeyValuePair<string, string>[] docs, string summary, out string description)
        {
            foreach (var kvp in docs)
            {
                if (string.Equals(kvp.Key, summary, StringComparison.Ordinal))
                {
                    description = kvp.Value;
                    return true;
                }
            }

            description = null;
            return false;
        }

        public InvocationResult InvokeCompletion(Argument arg, IEnumerable<Type> types)
        {
            string line = null;
            int? position = null;

            while (arg != null)
            {
                if (string.Equals(arg.Value, "--position", StringComparison.Ordinal))
                {
                    var positionArg = arg.Next; 
                    var positionValue = positionArg?.Value;
                    if (string.IsNullOrEmpty(positionValue) || !int.TryParse(positionValue, out var number))
                    {
                        _stderr.WriteLine("Failed to parse number.  Argument: --position");
                        ExitCode = 1;
                        return new InvocationResult { InvocationStatus = InvocationStatus.ArgumentConversionFailed };
                    }
                    position = number;
                    arg = positionArg.Next;
                    continue;
                }

                line = arg.Value;
                arg = arg.Next;
            }

            if (line == null)
            {
                _stderr.WriteLine("Missing required positional argument.  Argument: line");
                ExitCode = 1;
                return new InvocationResult { InvocationStatus = InvocationStatus.ArgumentConversionFailed };
            }

            return InvokeCompletion(types, line, position ?? line.Length);
        }

        public InvocationResult InvokeCompletion(IEnumerable<Type> types, string line, int position)
        {
            var arg = Argument.Parse(line);

            // ignore everything after `position`
            // doing anything intelligent based on what comes later sounds _very_ challenging
            arg.Trim(position);

            // ignore command name
            arg = arg?.Next;

            var candidates = Candidate.FromTypes(types);
            Suggestion suggestions = null;
            if (!string.IsNullOrWhiteSpace(arg.Value))
            {
                suggestions = GetSuggestions(arg.Value, candidates);
                if (arg.Next == null)
                {
                    while (suggestions != null)
                    {
                        AddCompletion(suggestions.Value);
                        suggestions = suggestions.Next;
                    }
                    return new InvocationResult { InvocationStatus = InvocationStatus.Complete };
                }
            }

            var method = suggestions?.Method;
            if (method == null)
            {
                return new InvocationResult { InvocationStatus = InvocationStatus.Complete };
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
                        if (!Parameter.TryGetEnumerableItem(lastNamedParameter.ParameterType, out var itemType))
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
                // return Matching(parameters.Select(p => p.Name), value).Select(p => '-' + p);
                AddCompletions(Matching(parameters.Select(p => p.Name), value).Select(p => '-' + p));
                return new InvocationResult { InvocationStatus = InvocationStatus.Complete };
            }

            AddCompletions(GetParameterValueCompletions(arg, parameters, lastNamedParameter));
            return new InvocationResult { InvocationStatus = InvocationStatus.Complete };
        }

        private IEnumerable<string> GetParameterValueCompletions(Argument arg, List<ParameterInfo> parameters, ParameterInfo named)
        {
            IEnumerable<string> values = null;
            var param = named ?? parameters?.FirstOrDefault();
            if (param != null)
            {
                values = GetValueCompletions(param.ParameterType, arg);
            }

            return values ?? Enumerable.Empty<string>();
        }

        private IEnumerable<string> GetValueCompletions(Type type, Argument arg)
        {
            if (Parameter.TryGetEnumerableItem(type, out var item))
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

        private bool PossibleParameterName(string s)
        {
            return s?.Length >= 1 && Array.IndexOf(_prefix, s[0]) != -1;
        }

        /// <summary>Figure out if we should actually invoke methods, output help or complete arguments</summary>
        public static (InvocationStatus outcome, Argument previous, Argument match) GetOutcome(Argument arg)
        {
            Argument prev = null;
            while (arg != null)
            {
                switch (arg.Value)
                {
                    case "--": return (InvocationStatus.MethodInvoked, null, null);
                    case "--complete": return (InvocationStatus.Complete, prev, arg);
                    case "--help": return (InvocationStatus.Help, prev, arg);
                }

                prev = arg;
                arg = arg.Next;
            }

            return (InvocationStatus.MethodInvoked, null, null);
        }

        private Suggestion GetSuggestions(string value, Candidate[] candidates)
        {
            Suggestion head = null;
            Suggestion tail = null;
            int lastMatch = -1;

            for (int i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                var name = candidate.Name;
                var typeShorter = name.Length < value.Length;
                string shorter, longer;
                if (typeShorter)
                {
                    shorter = name;
                    longer = value;
                }
                else
                {
                    longer = name;
                    shorter = value;
                }

                if (!longer.StartsWith(shorter, _comparison)) continue;

                lastMatch = i;
                name = name + '.';
                if (!typeShorter)
                {
                    Suggestion.Append(ref head, ref tail, name, candidate.Type, i);
                    continue;
                }

                AddMethodMatches(value, name, ref candidates[i], ref head, ref tail, i);
            }

            // if we only completed one type then go into its methods instead
            if (head != null && head.Next == null && head.Method == null)
            {
                var name = head.Value;
                var type = head.Type;
                head = tail = null;

                AddMethodMatches(null, name, ref candidates[lastMatch], ref head, ref tail, lastMatch);
            }

            return head;
        }

        private void AddMethodMatches(string value, string name, ref Candidate candidate, ref Suggestion head, ref Suggestion tail, int index)
        {
            var methods = candidate.Methods;
            if (methods == null)
            {
                candidate.Methods = methods = GetMethods(candidate.Type).ToArray();
            }

            foreach (var m in methods)
            {
                var completion = name + m.Name;

                if (value == null || completion.StartsWith(value, _comparison))
                {
                    Suggestion.Append(ref head, ref tail, name + m.Name, candidate.Type, index, m);
                }
            }
        }

        private static IEnumerable<MethodInfo> GetMethods(Type cls)
        {
            return cls.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => OriginalDeclaringType(m) != typeof(object));
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

        private IEnumerable<string> Matching(IEnumerable<string> completions, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return completions;
            }

            return completions.Where(c => c.StartsWith(value, _comparison));
        }

        private IEnumerable<T> Matching<T>(IEnumerable<T> completions, string value, Func<T, string> selector)
        {
            if (string.IsNullOrEmpty(value))
            {
                return completions;
            }

            return completions.Where(c => selector(c).StartsWith(value, _comparison));
        }
    }
}