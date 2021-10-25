namespace Volpe.Exceptions
{
    public class InvalidInfixOperatorException : VolpeException
    {
        public InvalidInfixOperatorException(PositionInText positionInText, string? message = null) 
            : base(positionInText, message)
        {
        }
    }
}