using System;

namespace Furball.Volpe.Exceptions; 

public abstract class VolpeException : Exception
{
    public PositionInText PositionInText { get; }
    public virtual Func<string>? AdditionalInfoGenerator => null;
        
    public abstract string Description { get; }
        
    public VolpeException(PositionInText positionInText)
    {
        PositionInText = positionInText;
    }

    public override string Message =>
        $"An error occurred at column {PositionInText.Row}, line {PositionInText.Column}: {Description + (Description.EndsWith(".") ? "" : ".")}" +
        (AdditionalInfoGenerator != null ? " Additional Info: " + AdditionalInfoGenerator() : String.Empty);
}