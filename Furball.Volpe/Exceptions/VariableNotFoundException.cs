namespace Furball.Volpe.Exceptions; 

public class VariableNotFoundException : VolpeException
{
    private readonly string? _message;
    private string VariableName { get; }
        
    public VariableNotFoundException(string variableName, PositionInText positionInText, string? message = null) 
        : base(positionInText) {
        this._message = message;
        VariableName  = variableName;
    }

    public override string Description => $"the variable \"{VariableName}\" was not found. msg:{this._message}";
}