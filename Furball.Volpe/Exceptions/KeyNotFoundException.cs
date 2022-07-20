namespace Furball.Volpe.Exceptions; 

public class KeyNotFoundException : VolpeException
{
    private string Key { get; set; }
        
    public KeyNotFoundException(string key, PositionInText positionInText) : base(positionInText)
    {
        Key = key;
    }

    public override string Description => $"the key \"{Key}\" was not found in the object.";
}