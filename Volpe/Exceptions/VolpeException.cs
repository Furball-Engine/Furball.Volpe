using System;

namespace Volpe.Exceptions
{
    public class VolpeException : Exception
    {
        public PositionInText PositionInText { get; }
        
        public VolpeException(PositionInText positionInText, string? message = null, Exception innerException = null) 
            : base(message, innerException)
        {
            PositionInText = positionInText;
        }
    }
}