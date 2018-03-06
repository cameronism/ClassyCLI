using System;
using System.Reflection;

namespace ClassyCLI
{
    class EnumerableParameter : Parameter
    {
        private ParameterList _list;
        private MethodInfo _create;

        public EnumerableParameter(MethodInfo methodInfo)
        {
            _create = methodInfo;
        }

        public override void SetFinalValue() => Parameters[Index] = _list?.Convert();

        public override void SetValue(object s, bool ignoreCase)
        {
            if (s != null) throw new NotSupportedException();
        }

        public override void SetValue(string s, bool ignoreCase)
        {
            if (_list == null)
            {
                _list = (ParameterList)_create.Invoke(null, null);
            }
            _list.Add(s, ignoreCase: ignoreCase);
            HasValue = true;
        }
    }
}