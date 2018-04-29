using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ClassyCLI
{
    internal class CompositeParameter : Parameter
    {
        private ParameterInfo _info;
        private ConstructorInfo _ctor;
        private List<PropertyInfo> _props;
        private PropertyInfo _prop;
        private object _instance;

        public CompositeParameter(ParameterInfo info, ConstructorInfo ctor, List<PropertyInfo> props)
        {
            _info = info;
            _props = props;
            _ctor = ctor;
            HasValue = true;
        }

        public bool TrySetName(string name, StringComparison comparison)
        {
            foreach (var prop in _props)
            {
                if (prop.Name.Equals(name, comparison))
                {
                    _prop = prop;
                    return true;
                }
            }

            return false;
        }

        private object GetInstance()
        {
            var instance = _instance;
            if (instance != null)
            {
                return instance;
            }
            instance = _ctor.Invoke(null);
            _instance = instance;
            return instance;
        }

        public override void SetValue(string s, bool ignoreCase)
        {
            var prop = _prop;
            _prop = null;

            if (prop == null)
            {
                throw new UnknownParameterException("All parameters to this method must be named");
            }

            var value = ConvertValue(s, prop.PropertyType, prop.Name, ignoreCase: ignoreCase);
            prop.SetValue(GetInstance(), value);
        }

        public override void SetFinalValue() => Parameters[Index] = GetInstance();
    }
}