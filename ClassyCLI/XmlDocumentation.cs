using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace ClassyCLI
{
    internal static class XmlDocumentation
    {
        public static Dictionary<string, KeyValuePair<string, string>[]> ParseMembers(Stream stream)
        {
            var dict = new Dictionary<string, KeyValuePair<string, string>[]>(StringComparer.Ordinal);
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.Depth == 2 && reader.IsStartElement())
                    {
                        Console.WriteLine(reader.LocalName);
                    }
                }
            }
            return dict;
        }
    }
}
