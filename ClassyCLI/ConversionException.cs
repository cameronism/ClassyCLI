using System;
using System.Reflection;

namespace ClassyCLI
{
    [System.Serializable]
    public class ConversionException : System.Exception
    {
        public ConversionException(ParameterInfo parameter, System.Exception inner) 
            : base($"Failed to convert parameter: {parameter.Name}", inner)
        {
            Parameter = parameter;
        }

        protected ConversionException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public ParameterInfo Parameter { get; }
    }
}