using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace ClassyCLI
{
    internal static class XmlDocumentation
    {
        public const string Summary = "summary";

        private static object _lock = new object();
        private static Dictionary<string, Dictionary<string, KeyValuePair<string, string>[]>> _assemblies;


        public static KeyValuePair<string, string>[] GetDocumentation(Type type)
        {
            var members = GetMembers(type.Assembly);
            if (members == null) return null;

            var key = "T:" + type.FullName.Replace('+', '.');
            if (members.TryGetValue(key, out var docs))
            {
                return docs;
            }

            return null;
        }

        private static Dictionary<string, KeyValuePair<string, string>[]> GetMembers(Assembly a)
        {
            Dictionary<string, KeyValuePair<string, string>[]> members = null;
            var location = a.Location;

            lock (_lock)
            {
                if (_assemblies == null)
                {
                    _assemblies = new Dictionary<string, Dictionary<string, KeyValuePair<string, string>[]>>(StringComparer.Ordinal);
                }
                else if (_assemblies.TryGetValue(location, out members))
                {
                    return members;
                }
            }

            var fi = new FileInfo(Path.ChangeExtension(location, "xml"));
            if (fi.Exists)
            {
                using (var stream = fi.OpenRead())
                {
                    members = ParseMembers(stream);
                }
            }

            // whatever happened, cache it
            lock (_lock)
            {
                _assemblies[location] = members;
            }

            return members;
        }

        public static Dictionary<string, KeyValuePair<string, string>[]> ParseMembers(Stream stream)
        {
            var dict = new Dictionary<string, KeyValuePair<string, string>[]>(StringComparer.Ordinal);
            string member = null;
            var values = new List<KeyValuePair<string, string>>();

            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.Depth == 2)
                    {
                        if (reader.IsStartElement())
                        {
                            if (member != null && values.Count > 0)
                            {
                                dict[member] = values.ToArray();
                                values.Clear();
                            }

                            if (string.Equals(reader.LocalName, "member", StringComparison.Ordinal))
                            {
                                member = reader.GetAttribute("name");
                            }
                            else
                            {
                                member = null;
                            }
                        }
                    }
                    else if (reader.Depth == 3)
                    {
                        if (reader.IsStartElement() && member != null)
                        {
                            values.Add(ParseMember(reader));
                        }
                    }
                }
            }

            if (member != null)
            {
                dict[member] = values.ToArray();
            }

            return dict;
        }

        private static KeyValuePair<string, string> ParseMember(XmlReader reader)
        {
            var key = reader.LocalName;
            reader.MoveToContent();

            // you're gonna wanna trim that
            var value = reader.ReadInnerXml();
            return new KeyValuePair<string, string>(key, value);
        }

        public static string GetFirstLine(string description)
        {
            // FIXME
            return description.Trim();
        }
    }
}
