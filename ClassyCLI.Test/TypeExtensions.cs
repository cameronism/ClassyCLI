using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassyCLI.Test
{
    public static class TypeExtensions
    {
        public static string GetTypeName(this Type t)
        {
            if (!t.IsGenericType) return t.FullName;

            var name = t.FullName;
            var ix = name.IndexOf('`');
            name = name.Substring(0, ix);

            return $"{name}<{string.Join(", ", t.GetGenericArguments().Select(a => GetTypeName(a)))}>";
        }
    }
}
