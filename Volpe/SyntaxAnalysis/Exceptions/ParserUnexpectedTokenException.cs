using Volpe.LexicalAnalysis;

namespace Volpe.SyntaxAnalysis.Exceptions
{
    public class ParserUnexpectedTokenException : ParserException
    {
        public Token Token { get; }
        
        public ParserUnexpectedTokenException(PositionInText positionInText, Token token, string? message = null) 
            : base(positionInText, message)
        {
            Token = token;
        }
    }
}