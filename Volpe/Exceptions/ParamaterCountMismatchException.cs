using System;

namespace Volpe.Exceptions
{
    public class ParamaterCountMismatchException : VolpeException
    {
        public ParamaterCountMismatchException(PositionInText positionInText, string? message = null) : base(positionInText, message)
        {
        }
    }
}