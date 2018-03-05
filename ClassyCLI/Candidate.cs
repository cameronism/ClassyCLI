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

            var prefix = Runner.CommonPrefix(result.Select(item => item.Name));
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
    }
}
