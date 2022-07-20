namespace Furball.Volpe.Exceptions; 

public class UnknownMethodException : VolpeException
{
    public string MethodName { get; }
    public string ClassName { get; }
        
    public UnknownMethodException(string methodName, string className, PositionInText positionInText) : base(positionInText)
    {
        MethodName = methodName;
        ClassName  = className;
    }

    public override string Description => $"No method \"{MethodName}\" found for the \"{ClassName}\" class.";
}