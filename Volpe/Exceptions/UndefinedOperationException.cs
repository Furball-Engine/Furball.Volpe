using System;

namespace Volpe.Exceptions
{
    public class UndefinedOperationException : VolpeException
    {
        public Type LOperandType { get; }
        public Type ROperandType { get; }
        public string OperationName { get; }

        public UndefinedOperationException(string operationName, Type lOperandType, Type rOperandType, PositionInText positionInText) 
            : base(positionInText)
        {
            LOperandType = lOperandType;
            ROperandType = rOperandType;
            OperationName = operationName;
        }

        public override string Description =>
            $"no {OperationName} operation is defined for the pair {LOperandType.Name}/{ROperandType.Name}";
    }
}