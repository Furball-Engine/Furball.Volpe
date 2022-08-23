namespace Furball.Volpe.Exceptions; 

public class UnexpectedEofException : VolpeException
{
    public UnexpectedEofException(PositionInText positionInText) 
        : base(positionInText)
    {
    }
        
    public override string Description => $"unexpected End-Of-File";
}