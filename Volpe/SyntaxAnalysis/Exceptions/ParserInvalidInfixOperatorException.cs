namespace Volpe.SyntaxAnalysis.Exceptions
{
    public class ParserInvalidInfixOperatorException : ParserException
    {
        public ParserInvalidInfixOperatorException(PositionInText positionInText, string? message = null) 
            : base(positionInText, message)
        {
        }
    }
}