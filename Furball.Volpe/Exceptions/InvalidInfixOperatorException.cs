using Furball.Volpe.SyntaxAnalysis;

namespace Furball.Volpe.Exceptions
{
    public class InvalidInfixOperatorException : VolpeException
    {
        public ExpressionOperator Operator { get; }
        
        public InvalidInfixOperatorException(ExpressionOperator @operator, PositionInText positionInText) 
            : base(positionInText)
        {
            Operator = @operator;
        }

        public override string Description => $"{Operator.GetType().Name} is not a valid infix operator";
    }
}