using System;

namespace Furball.Volpe.Exceptions; 

public class UndefinedPrefixOperationException : VolpeException
{
    public Type LOperandType { get; }
    public string OperationName { get; }

    public UndefinedPrefixOperationException(string operationName, Type lOperandType, PositionInText positionInText) 
        : base(positionInText)
    {
        LOperandType  = lOperandType;
        OperationName = operationName;
    }

    public override string Description =>
        $"no {OperationName} prefix operation is defined for {LOperandType.Name}";
}