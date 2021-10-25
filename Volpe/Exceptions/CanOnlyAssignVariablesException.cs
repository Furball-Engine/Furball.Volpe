using System;

namespace Volpe.Exceptions
{
    public class CanOnlyAssignVariablesException : VolpeException
    {
        public CanOnlyAssignVariablesException(PositionInText positionInText, string? message = null) 
            : base(positionInText, message)
        {
        }
    }
}