namespace Volpe.Exceptions
{
    public class InvalidOperatorStringException : VolpeException
    {
        public string OpString { get; }
        
        public InvalidOperatorStringException(string opString, PositionInText positionInText) : base(positionInText)
        {
            OpString = opString;
        }

        public override string Description => $"\"{OpString}\" is not a valid operator";
    }
}