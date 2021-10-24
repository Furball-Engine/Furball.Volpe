namespace Volpe.SyntaxAnalysis.Exceptions
{
    public class ParserExpectedExpressionException : ParserException
    {
        public ParserExpectedExpressionException(PositionInText positionInText, string? message = null) : base(positionInText, message)
        {
        }
    }
}