using System;

namespace Furball.Volpe.Exceptions; 

public class InvalidValueTypeException : VolpeException
{
    public Type ExpectedType { get; }
    public Type ReceivedType { get; }
        
    public InvalidValueTypeException(Type expectedType, Type receivedType, PositionInText positionInText) : base(positionInText)
    {
        ExpectedType = expectedType;
        ReceivedType = receivedType;
    }

    public override string Description => $"Expected type {ExpectedType.Name}, received type {ReceivedType.Name}";
}