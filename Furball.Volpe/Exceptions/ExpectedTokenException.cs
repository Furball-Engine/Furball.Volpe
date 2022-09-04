namespace Furball.Volpe.Exceptions; 

public class ExpectedTokenException : VolpeException
{
    private readonly string? _message;
    public ExpectedTokenException(PositionInText positionInText, string? message = null) 
        : base(positionInText) {
        this._message = message;
    }

    public override string Description => $"a token was expected. msg:{this._message}";
}