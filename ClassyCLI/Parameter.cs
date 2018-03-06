using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ClassyCLI
{
    internal class Parameter
    {
        public string Name { get; private set; }
        public ParameterInfo ParameterInfo { get; private set; }
        public object[] Parameters { get; private set; }
        public int Index { get; private set; }
        public bool HasValue { get; protected set; }

        public virtual void SetValue(object s, bool ignoreCase)
        {
            var value = ConvertValue(s, ParameterInfo, ignoreCase);
            Parameters[Index] = value;
            HasValue = true;
        }

        public virtual void SetValue(string s, bool ignoreCase)
        {
            var value = ConvertValue(s, ParameterInfo, ignoreCase);
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

        public static object ConvertValue(object value, ParameterInfo parameter, bool ignoreCase)
        {
            return ConvertValue(value, parameter.ParameterType, parameter, ignoreCase);
        }

        public static object ConvertValue(object value, Type destination, ParameterInfo parameter, bool ignoreCase)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
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
                else if (string.Equals(s, "null", comparison))
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
                try
                {
                    if (s != null) return Enum.Parse(type, s, ignoreCase: ignoreCase);

                    return Enum.ToObject(type, value);
                }
                catch (Exception e)
                {
                    throw new ConversionException(parameter, e);
                }
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
                    throw new ConversionException(parameter, null);
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

                // strings are objects -- have fun
                if (type == typeof(object)) return value;

                // finally do the TypeConverter dance
                var converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        return converter.ConvertFromString(s);
                    }
                    catch (Exception e)
                    {
                        throw new ConversionException(parameter, e);
                    }
                }

                var ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string) }, null);

                if (ctor != null)
                {
                    try
                    {
                        return ctor.Invoke(new[] { s });
                    }
                    catch (Exception e)
                    {
                        throw new ConversionException(parameter, e);
                    }
                }

                // this will never work
                throw new ConversionException(parameter, null);
            }

            try
            {
                // only non-strings should be left - which should be from DefaultAttribute
                return Convert.ChangeType(value, type);
            }
            catch (Exception e)
            {
                throw new ConversionException(parameter, e);
            }
        }

        static bool TryGetFileInfo(string path, out FileInfo fi)
        {
            fi = new FileInfo(path);
            return fi.Exists;
        }

        public static bool TryGetEnumerableItem(Type type, out Type item)
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