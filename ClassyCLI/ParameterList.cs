using System;
using System.Collections.Generic;
using System.Reflection;

namespace ClassyCLI
{

    internal abstract class ParameterList
    {
        public abstract void Add(string s, bool ignoreCase);
        public abstract object Convert();
        public static ParameterList Create<TItem, TList>() => new ParameterList<TItem, TList>();
        public ParameterInfo ParameterInfo { get; set; }
    }

    internal sealed class ParameterList<TItem, TList> : ParameterList
    {
        private List<TItem> _list = new List<TItem>();

        public override void Add(string s, bool ignoreCase)
        {
            _list.Add((TItem)Parameter.ConvertValue(s, typeof(TItem), ParameterInfo.Name, ignoreCase));
        }

        public override object Convert()
        {
            var destination = typeof(TList);
            if (destination.IsAssignableFrom(typeof(List<TItem>))) return _list;

            if (destination == typeof(TItem[]) || destination == typeof(Array)) return _list.ToArray();

            throw new NotImplementedException();
        }
    }
}