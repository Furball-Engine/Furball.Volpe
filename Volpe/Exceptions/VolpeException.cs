using System;

namespace Volpe.Exceptions
{
    public class VolpeException : Exception
    {
        public PositionInText PositionInText { get; }
        public virtual Func<string>? AdditionalInfoGenerator => null;
        
        public VolpeException(PositionInText positionInText)
        {
            PositionInText = positionInText;
        }

        public override string Message =>
            $"An error occurred at line {PositionInText.Row}, column {PositionInText.Column}." +
            (AdditionalInfoGenerator != null ? " Additional Info: " + AdditionalInfoGenerator() : String.Empty);
    }
}