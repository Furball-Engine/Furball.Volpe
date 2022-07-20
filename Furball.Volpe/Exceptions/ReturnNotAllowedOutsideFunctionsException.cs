using System;

namespace Furball.Volpe.Exceptions; 

public class ReturnNotAllowedOutsideFunctionsException : VolpeException
{
    public ReturnNotAllowedOutsideFunctionsException(PositionInText positionInText) 
        : base(positionInText)
    {
    }

    public override string Description => "return is not allowed outside of a function";
}