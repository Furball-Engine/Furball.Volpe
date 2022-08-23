namespace Furball.Volpe.Exceptions; 

public class ExpectedVariableException : VolpeException
{
    public ExpectedVariableException(PositionInText positionInText, string? message = null) 
        : base(positionInText)
    {
    }

    public override string Description => "a variable was expected";
}