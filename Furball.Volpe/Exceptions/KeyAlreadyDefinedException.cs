namespace Furball.Volpe.Exceptions; 

public class KeyAlreadyDefinedException : VolpeException
{
    private string Key { get; set; }
        
    public KeyAlreadyDefinedException(string key, PositionInText positionInText) : base(positionInText)
    {
        Key = key;
    }

    public override string Description => $"the key \"{Key}\" was already defined in the object";
}