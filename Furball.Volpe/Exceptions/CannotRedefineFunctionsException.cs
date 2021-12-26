using System;

namespace Furball.Volpe.Exceptions
{
    public class CannotRedefineFunctionsException : VolpeException
    {
        public CannotRedefineFunctionsException(PositionInText positionInText, string? message = null) : base(positionInText)
        {
        }

        public override string Description => "functions cannot be redefined (you may want to use a lambda?)";
    }
}