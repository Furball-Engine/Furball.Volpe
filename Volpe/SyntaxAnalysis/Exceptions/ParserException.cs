using System;

namespace Volpe.SyntaxAnalysis.Exceptions
{
    public class ParserException : Exception
    {
        public ParserException(PositionInText positionInText, string? message = null) : base(message)
        {
        }
    }
}