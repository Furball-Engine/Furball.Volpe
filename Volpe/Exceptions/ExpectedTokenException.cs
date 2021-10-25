namespace Volpe.Exceptions
{
    public class ExpectedTokenException : VolpeException
    {
        public ExpectedTokenException(PositionInText positionInText, string? message = null) 
            : base(positionInText, message)
        {
        }
    }
}