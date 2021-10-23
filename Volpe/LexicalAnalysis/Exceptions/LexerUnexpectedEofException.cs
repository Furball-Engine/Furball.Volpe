namespace Volpe.LexicalAnalysis.Exceptions
{
    public class LexerUnexpectedEofException : LexerException
    {
        public LexerUnexpectedEofException(PositionInText positionInText, string? message = null) 
            : base(positionInText, message)
        {
        }
    }
}