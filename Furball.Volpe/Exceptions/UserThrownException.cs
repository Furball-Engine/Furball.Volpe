namespace Furball.Volpe.Exceptions; 

public class UserThrownException : VolpeException {
    public UserThrownException(PositionInText positionInText, string message) : base(positionInText) {
        this.Description = message;
    }

    public override string Description { get; }
}