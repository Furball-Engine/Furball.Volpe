namespace Furball.Volpe.Exceptions; 

public class ExpectedVariableException : VolpeException
{
    private readonly string? _message;
    public ExpectedVariableException(PositionInText positionInText, string? message = null) 
        : base(positionInText) {
        this._message = message;
    }

    public override string Description => $"a variable was expected. msg:{this._message}";
}