using System;
using System.Reflection;

namespace ClassyCLI
{
    [Serializable]
    public class ConversionException : Exception
    {
        public ConversionException(string parameterName, Exception inner) 
            : base($"Failed to convert parameter: {parameterName}", inner)
        {
            ParameterName = parameterName;
        }

        protected ConversionException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string ParameterName { get; }
    }
}