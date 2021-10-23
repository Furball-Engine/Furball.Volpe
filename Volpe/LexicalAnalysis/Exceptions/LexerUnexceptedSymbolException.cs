namespace Volpe.LexicalAnalysis.Exceptions
{
    public class LexerUnexceptedSymbolException : LexerException
    {
        public char Symbol { get; }
        
        public LexerUnexceptedSymbolException(char symbol, PositionInText positionInText, string? message = null) 
            : base(positionInText, message)
        {
            Symbol = symbol;
        }
    }
}