namespace Furball.Volpe.Exceptions; 

public class VariableNotFoundException : VolpeException
{
    private string VariableName { get; }
        
    public VariableNotFoundException(string variableName, PositionInText positionInText, string? message = null) 
        : base(positionInText)
    {
        VariableName = variableName;
    }

    public override string Description => $"the variable \"{VariableName}\" was not found";
}