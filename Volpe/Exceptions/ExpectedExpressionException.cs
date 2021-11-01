namespace Volpe.Exceptions
{
    public class ExpectedExpressionException : VolpeException
    {
        public ExpectedExpressionException(PositionInText positionInText, string? message = null) : base(positionInText)
        {
        }
    }
}