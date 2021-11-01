using System;

namespace Volpe.Exceptions
{
    public class ReturnNotAllowedOutsideFunctionsException : VolpeException
    {
        public ReturnNotAllowedOutsideFunctionsException(PositionInText positionInText) 
            : base(positionInText)
        {
        }
    }
}