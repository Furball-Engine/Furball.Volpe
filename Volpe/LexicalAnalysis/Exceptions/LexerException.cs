using System;

namespace Volpe.LexicalAnalysis.Exceptions
{
    public class LexerException : Exception
    {
        private PositionInText PositionInText { get; }

        public LexerException(PositionInText positionInText, string? message = null) : base(message)
        {
            PositionInText = positionInText;
        }
    }
}