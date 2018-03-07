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
        const string _sigil = "-/@=";

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
                else if (outcome == InvocationStatus.BashCompletionScript || outcome == InvocationStatus.PowerShellCompletionScript)
                {
                    return InvokeCreateCompletionScript(match.Next, outcome);
                }
            }

            return InvokeMethod(arg, types);
        }

        private InvocationResult InvokeCreateCompletionScript(Argument next, InvocationStatus outcome)
        {
            var alias = next?.Value;
            if (string.IsNullOrWhiteSpace(alias))
            {
                _stderr.WriteLine("Missing required parameter command name.");
                ExitCode = 1;
                return new InvocationResult { InvocationStatus = InvocationStatus.ArgumentConversionFailed };
            }

            string run;
            var dll = Assembly.GetEntryAssembly().Location;

            // ASSUMPTION assume dotnet core unless entry assembly has extension `exe`
            if (string.Equals(Path.GetExtension(dll), ".exe", StringComparison.OrdinalIgnoreCase))
            {
                run = dll;
            }
            else
            {
                run = "dotnet " + dll;
            }

            if (outcome == InvocationStatus.BashCompletionScript)
            {
                _stdout.WriteLine($"alias {alias}=\"{run}\"");
                _stdout.WriteLine($"_{alias}_bash_complete()");
                _stdout.WriteLine($"{{");
                _stdout.WriteLine($"  local word=${{COMP_WORDS[COMP_CWORD]}}");
                _stdout.WriteLine($"  local {alias}path=${{COMP_WORDS[1]}}");
                _stdout.WriteLine($"  local completions=(\"$({run} --complete --position ${{COMP_POINT}} \"${{COMP_LINE}}\")\")");
                _stdout.WriteLine($"  COMPREPLY=( $(compgen -W \"$completions\" -- \"$word\") )");
                _stdout.WriteLine($"}}");
                _stdout.WriteLine($"complete -f -F _{alias}_bash_complete {alias}");

            }
            else if (outcome == InvocationStatus.PowerShellCompletionScript)
            {
                _stdout.WriteLine($"function {alias} {{ {run} $args }}");
                _stdout.WriteLine($"Register-ArgumentCompleter -Native -CommandName {alias} -ScriptBlock {{");
                _stdout.WriteLine($"  param($commandName, $wordToComplete, $cursorPosition)");
                _stdout.WriteLine($"  {run} --complete --position $cursorPosition \"$wordToComplete\" | ForEach-Object {{");
                _stdout.WriteLine($"    [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)");
                _stdout.WriteLine($"  }}");
                _stdout.WriteLine($"}}");
            }
            else
            {
                throw new NotImplementedException();
            }

            return new InvocationResult { InvocationStatus = outcome };
        }

        public InvocationResult InvokeMethod(Argument arg, IEnumerable<Type> types)
        {
            var head = arg;
            var candidates = Candidate.FromTypes(types);
            var suggestion = GetSuggestions(arg?.Value ?? string.Empty, candidates);

            if (suggestion == null || suggestion.Method == null || suggestion.Next != null || suggestion.Value.Length != arg.Value.Length)
            {
                var helpResult = InvokeHelp(arg, types, _stderr);
                helpResult.InvocationStatus = string.IsNullOrEmpty(arg?.Value) ? InvocationStatus.NoMethodSpecified : InvocationStatus.NoMethodFound;
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

            try
            {
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
                        _stderr.WriteLine("Missing required parameter: " + paramInfo.Name);
                        var helpResult = InvokeHelp(head, types, _stderr);
                        helpResult.InvocationStatus = InvocationStatus.ArgumentMissing;
                        ExitCode = 1;
                        return helpResult;
                    }

                    param.SetValue(paramInfo.DefaultValue, _ignoreCase);
                }
            }
            catch (ConversionException ce)
            {
                _stderr.WriteLine("Fail to parse parameter: " + ce.Parameter.Name);
                var helpResult = InvokeHelp(head, types, _stderr);
                helpResult.InvocationStatus = InvocationStatus.ArgumentConversionFailed;
                helpResult.Exception = ce;
                ExitCode = 1;
                return helpResult;
            }

            object result;
            try
            {
                result = method.Invoke(instance, args);
            }
            catch (Exception e)
            {
                _stderr.WriteLine(e.ToString());
                ExitCode = 2;
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

            if (_sigil.IndexOf(name[0]) == -1) return false;
            name = name.Substring(1);

            if (name.Length == 0) return false;

            param = parameters.SingleOrDefault(p => p.Name.StartsWith(name, _comparison));
            return param != null;
        }

        public InvocationResult InvokeHelp(Argument arg, IEnumerable<Type> types, TextWriter tw)
        {
            string description;
            var candidates = Candidate.FromTypes(types);
            var suggestion = GetSuggestions(arg?.Value ?? string.Empty, candidates);

            if (arg?.Value != null && !string.Equals(arg.Value, suggestion?.Value, _comparison))
            {
                tw.WriteLine("Method not found: " + arg.Value);
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

            var original = param.ParameterType;
            var underlying = Nullable.GetUnderlyingType(original);

            // ignore nullable for now, would be nice to print someday
            var type = underlying ?? original;

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
            return GetTypeName(type);
        }

        private static string GetTypeName(Type t)
        {
            if (t.IsArray)
            {
                return GetTypeName(t.GetElementType()) + "[]";
            }
            if (!t.IsGenericType)
            {
                return t.FullName;
            }

            var name = t.FullName;
            var ix = name.IndexOf('`');
            if (ix != -1) name = name.Substring(0, ix);
            var args = t.GetGenericArguments();

            for (int i = 0; i < args.Length; i++)
            {
                name += (i == 0 ? "<" : ", ") + GetTypeName(args[i]);
            }

            return name + '>';
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
                var value = arg.Value;
                var sigil = '-';
                if (arg.Value.Length >= 1)
                {
                    sigil = value[0];
                    value = value.Substring(1);
                }
                AddCompletions(Matching(parameters.Select(p => p.Name), value).Select(p => sigil + p));
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
            return s?.Length >= 1 && _sigil.IndexOf(s[0]) != -1;
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
                    case "--bash-completion-script": return (InvocationStatus.BashCompletionScript, prev, arg);
                    case "--powershell-completion-script": return (InvocationStatus.PowerShellCompletionScript, prev, arg);
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
                var type = candidate.Type;
                if (type.IsEnum || 
                    typeof(Delegate).IsAssignableFrom(type) || 
                    !(type.IsClass || type.IsValueType) ||
                    type.IsGenericType)
                {
                    continue;
                }

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
                    Suggestion.Append(ref head, ref tail, name, type, i);
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

        private static bool CouldInvoke(MethodInfo mi)
        {
            if (mi.IsAbstract) return false;
            if (mi.IsGenericMethod) return false;
            if (mi.IsSpecialName) return false;

            return true;
        }

        private static IEnumerable<MethodInfo> GetMethods(Type cls)
        {
            return cls.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => CouldInvoke(m) && OriginalDeclaringType(m) != typeof(object));
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