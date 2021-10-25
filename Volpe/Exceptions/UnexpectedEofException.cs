using System;

namespace Volpe.Exceptions
{
    public class UnexpectedEofException : VolpeException
    {
        public UnexpectedEofException(PositionInText positionInText, string? message = null) 
            : base(positionInText, message)
        {
        }
    }
}