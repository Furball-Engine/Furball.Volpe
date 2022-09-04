namespace Furball.Volpe.Exceptions; 

public class FunctionNotFoundException : VolpeException
{
    private readonly string? _message;
    private string FunctionName { get; }
        
    public FunctionNotFoundException(string functionName, PositionInText positionInText, string? message = null) 
        : base(positionInText) {
        this._message = message;
        FunctionName  = functionName;
    }

    public override string Description => $"the function \"{FunctionName}\" was not found. msg:{this._message}";
}