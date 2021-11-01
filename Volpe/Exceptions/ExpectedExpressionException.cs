namespace Volpe.Exceptions
{
    public class ExpectedExpressionException : VolpeException
    {
        public ExpectedExpressionException(PositionInText positionInText) : base(positionInText)
        {
        }

        public override string Description => "an expression was expected.";
    }
}