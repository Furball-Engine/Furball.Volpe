using System;

namespace Volpe.Exceptions
{
    public class ExpectedVariableException : VolpeException
    {
        public ExpectedVariableException(PositionInText positionInText, string? message = null) 
            : base(positionInText, message)
        {
        }
    }
}