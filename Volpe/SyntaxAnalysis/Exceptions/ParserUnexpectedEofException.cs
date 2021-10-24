namespace Volpe.SyntaxAnalysis.Exceptions
{
    public class ParserUnexpectedEofException : ParserException
    {
        public ParserUnexpectedEofException(PositionInText positionInText, string? message = null) 
            : base(positionInText, message)
        {
        }
    }
}