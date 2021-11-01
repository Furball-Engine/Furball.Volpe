using System;

namespace Volpe.Exceptions
{
    public class OperatorDomainException : VolpeException
    {
        private string _details;
        
        public OperatorDomainException(PositionInText positionInText, string details) 
            : base(positionInText)
        {
            _details = details;
        }

        public override string Description => $"operator domain error ({_details})";
    } 
}