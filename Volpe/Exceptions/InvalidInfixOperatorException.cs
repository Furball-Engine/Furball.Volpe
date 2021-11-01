namespace Volpe.Exceptions
{
    public class InvalidInfixOperatorException : VolpeException
    {
        public InvalidInfixOperatorException(PositionInText positionInText) 
            : base(positionInText)
        {
        }
    }
}