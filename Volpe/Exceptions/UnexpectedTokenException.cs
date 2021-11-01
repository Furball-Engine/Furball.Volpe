using Volpe.LexicalAnalysis;

namespace Volpe.Exceptions
{
    public class UnexpectedTokenException : VolpeException
    {
        public Token Token { get; }
        
        public UnexpectedTokenException(PositionInText positionInText, Token token) 
            : base(positionInText)
        {
            Token = token;
        }
        
        public override string Description => $"unexpected token {Token.GetType().Name}";
    }
}