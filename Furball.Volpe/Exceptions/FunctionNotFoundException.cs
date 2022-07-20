namespace Furball.Volpe.Exceptions; 

public class FunctionNotFoundException : VolpeException
{
    private string FunctionName { get; }
        
    public FunctionNotFoundException(string functionName, PositionInText positionInText, string? message = null) 
        : base(positionInText)
    {
        FunctionName = functionName;
    }

    public override string Description => $"the function \"{FunctionName}\" was not found";
}