using Volpe.LexicalAnalysis;

namespace Volpe.Exceptions
{
    public class UnexpectedTokenException : VolpeException
    {
        public Token Token { get; }
        
        public UnexpectedTokenException(PositionInText positionInText, Token token, string? message = null) 
            : base(positionInText, message)
        {
            Token = token;
        }
    }
}