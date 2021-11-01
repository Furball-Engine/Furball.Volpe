namespace Volpe.Exceptions
{
    public class UnexceptedSymbolException : VolpeException
    {
        public char Symbol { get; }
        
        public UnexceptedSymbolException(char symbol, PositionInText positionInText) 
            : base(positionInText)
        {
            Symbol = symbol;
        }
    }
}