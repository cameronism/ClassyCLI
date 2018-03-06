using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace ClassyCLI
{
    internal struct Candidate
    {
        public Type Type;
        public string Name;
        public MethodInfo[] Methods;

        public static Candidate[] FromTypes(IEnumerable<Type> types)
        {
            int count;
            if (types is ICollection<Type> cc) count = cc.Count;
            else if (types is IReadOnlyCollection<Type> rc) count = rc.Count;
            else
            {
                var list = types.ToList();
                count = list.Count;
                types = list;
            }

            var result = new Candidate[count];
            var i = 0;
            foreach (var type in types)
            {
                result[i++] = new Candidate { Type = type, Name = type.FullName.Replace('+', '.') };
            }

            var prefix = CommonPrefix(result.Select(item => item.Name));
            var len = prefix.Length;
            if (len > 0)
            {
                for (i = 0; i < result.Length; i++)
                {
                    result[i].Name = result[i].Name.Substring(len);
                }
            }

            return result;
        }

        public static string CommonPrefix(IEnumerable<string> names)
        {
            string prefix = null;
            using (var e = names.GetEnumerator())
            {
                if (!e.MoveNext()) return null;
                prefix = e.Current;

                var ix = prefix.LastIndexOf('.', prefix.Length - 2);
                prefix = ix > 0 ? prefix.Substring(0, ix + 1) : "";

                while (e.MoveNext() && prefix.Length > 0)
                {
                    var name = e.Current;
                    if (!name.StartsWith(prefix, StringComparison.Ordinal))
                    {
                        prefix = CommonPrefix(prefix, name);
                    }
                }
            }

            return prefix;
        }

        public static string CommonPrefix(string prefix, string name)
        {
            do
            {
                var ix = prefix.LastIndexOf('.', prefix.Length - 2);
                if (ix <= 0)
                {
                    prefix = "";
                    break;
                }

                prefix = prefix.Substring(0, ix + 1);
            } while (!name.StartsWith(prefix, StringComparison.Ordinal));

            return prefix;
        }
    }
}
