using Furball.Volpe.SyntaxAnalysis;

namespace Furball.Volpe.Exceptions; 

public class InvalidPrefixOperatorException : VolpeException
{
    public ExpressionOperator Operator { get; }
        
    public InvalidPrefixOperatorException(ExpressionOperator @operator, PositionInText positionInText) 
        : base(positionInText)
    {
        Operator = @operator;
    }

    public override string Description => $"{Operator.GetType().Name} is not a valid prefix operator";
}