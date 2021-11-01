using System;

namespace Volpe.Exceptions
{
    public class OperatorDomainException : VolpeException
    {
        private string? _message;
        
        public OperatorDomainException(PositionInText positionInText, string? message = null) 
            : base(positionInText)
        {
            _message = message;
        }

        public override Func<string> AdditionalInfoGenerator => () => _message ?? string.Empty;
    }
}