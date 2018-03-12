using System;
using System.Reflection;

namespace ClassyCLI
{
    [System.Serializable]
    public class UnknownParameterException : System.Exception
    {
        public UnknownParameterException(string name) 
            : base($"Unknown parameter: {name}")
        {
            Name = name;
        }

        protected UnknownParameterException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public string Name { get; }
    }
}