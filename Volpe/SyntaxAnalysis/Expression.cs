using System;
using Volpe.LexicalAnalysis;

namespace Volpe.SyntaxAnalysis
{
    public abstract record ExpressionValue
    {
        public record Variable(string Name) : ExpressionValue;
        public record Number(double Value) : ExpressionValue;
        public record InfixExpression(ExpressionOperator Operator, Expression Left, Expression Right): ExpressionValue;
    }
    
    public class Expression : IPositionableInText, IEquatable<Expression>
    {
        public ExpressionValue Value { get; set; }
        public PositionInText PositionInText { get; set; }

        #region IEquatable implementation
        
        public bool Equals(Expression? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Equals(other.Value);
        }

        public override string ToString()
        {
            return $"Expression {{ Value = {Value} }}";
        }

        #endregion
    }
}