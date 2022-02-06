namespace Furball.Volpe.Exceptions
{
    public class ClassNotFoundException : VolpeException
    {
        public string ClassName { get; }
        
        public ClassNotFoundException(string className, PositionInText positionInText) : base(positionInText)
        {
            ClassName = className;
        }

        public override string Description => $"the \"{ClassName}\" was not found";
    }
}