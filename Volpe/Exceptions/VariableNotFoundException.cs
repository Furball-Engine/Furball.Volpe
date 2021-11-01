using System;

namespace Volpe.Exceptions
{
    public class VariableNotFoundException : VolpeException
    {
        public VariableNotFoundException(PositionInText positionInText, string? message = null) 
            : base(positionInText)
        {
        }
    }
}