using System;
using Volpe.Evaluation;

namespace Volpe.Exceptions
{
    public class TypeConversionException : VolpeException
    {
        private Value Value { get; }
        private Type Type { get; }
        
        public TypeConversionException(Value value, Type type, PositionInText positionInText) : base(positionInText)
        {
            Value = value;
            Type = type;
        }

        public override string Description =>
            $"Can't convert {Value.GetType().Name} with value '{Value.Representation}' " +
            $"to {Type.Name}";
    }
}