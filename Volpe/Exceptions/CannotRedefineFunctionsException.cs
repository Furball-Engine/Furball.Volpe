using System;

namespace Volpe.Exceptions
{
    public class CannotRedefineFunctionsException : VolpeException
    {
        public CannotRedefineFunctionsException(PositionInText positionInText, string? message = null) : base(positionInText, message)
        {
        }
    }
}