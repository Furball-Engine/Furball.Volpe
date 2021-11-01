namespace Volpe.Exceptions
{
    public class ExpectedTokenException : VolpeException
    {
        public ExpectedTokenException(PositionInText positionInText, string? message = null) 
            : base(positionInText)
        {
        }

        public override string Description => "a token was expected";
    }
}