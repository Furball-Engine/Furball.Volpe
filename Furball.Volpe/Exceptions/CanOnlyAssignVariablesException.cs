namespace Furball.Volpe.Exceptions; 

public class CanOnlyAssignVariablesException : VolpeException
{
    private readonly string? _message;
    public CanOnlyAssignVariablesException(PositionInText positionInText, string? message = null) 
        : base(positionInText) {
        this._message = message;
    }

    public override string Description => $"the left operator of an assign expression can only be a variable. msg:{this._message}";
}