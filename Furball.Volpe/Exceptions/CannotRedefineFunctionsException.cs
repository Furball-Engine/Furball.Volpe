namespace Furball.Volpe.Exceptions; 

public class CannotRedefineFunctionsException : VolpeException
{
    private readonly string? _message;
    public CannotRedefineFunctionsException(PositionInText positionInText, string? message = null) : base(positionInText) {
        this._message = message;
    }

    public override string Description => $"functions cannot be redefined (you may want to use a lambda?) msg:{this._message}";
}