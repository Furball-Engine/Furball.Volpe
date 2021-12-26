using System;

namespace Furball.Volpe.Exceptions
{
    public class UndefinedInfixOperationException : VolpeException
    {
        public Type LOperandType { get; }
        public Type ROperandType { get; }
        public string OperationName { get; }

        public UndefinedInfixOperationException(string operationName, Type lOperandType, Type rOperandType, PositionInText positionInText) 
            : base(positionInText)
        {
            LOperandType = lOperandType;
            ROperandType = rOperandType;
            OperationName = operationName;
        }

        public override string Description =>
            $"no \"{OperationName}\" infix operation is defined for the pair {LOperandType.Name}/{ROperandType.Name}";
    }
}