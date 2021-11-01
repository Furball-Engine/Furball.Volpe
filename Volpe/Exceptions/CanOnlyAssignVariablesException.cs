using System;

namespace Volpe.Exceptions
{
    public class CanOnlyAssignVariablesException : VolpeException
    {
        public CanOnlyAssignVariablesException(PositionInText positionInText, string? message = null) 
            : base(positionInText)
        {
        }

        public override string Description => "the left operator of an assign expression can only be a variable";
    }
}