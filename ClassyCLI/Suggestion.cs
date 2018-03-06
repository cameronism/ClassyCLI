using System;
using System.Collections.Generic;
using System.Reflection;

namespace ClassyCLI
{
    internal class Suggestion
    {
        public string Value { get; set; }
        public Suggestion Next { get; set; }
        public Type Type { get; set; }
        public MethodInfo Method { get; set; }
        public int CandidateIndex { get; set; }

        public static void Append(ref Suggestion head, ref Suggestion tail, string name, Type type, int index, MethodInfo method = null)
        {
            var suggestion = new Suggestion
            {
                Value = name,
                Type = type,
                Method = method,
                CandidateIndex = index,
            };

            if (head == null)
            {
                head = suggestion;
                tail = suggestion;
            }
            else
            {
                tail.Next = suggestion;
                tail = suggestion;
            }
        }

        internal static IEnumerable<string> ToEnumerable(Suggestion suggestion)
        {
            while (suggestion != null)
            {
                yield return suggestion.Value;
                suggestion = suggestion.Next;
            }
        }
    }
}