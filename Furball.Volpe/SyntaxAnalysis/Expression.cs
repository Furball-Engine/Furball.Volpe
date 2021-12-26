using System;

namespace Furball.Volpe.SyntaxAnalysis
{
    public abstract record ExpressionValue
    {
        public record Variable(string Name) : ExpressionValue;
        public record String(string Value) : ExpressionValue;
        public record Number(double Value) : ExpressionValue;
        public record InfixExpression(ExpressionOperator Operator, Expression Left, Expression Right): ExpressionValue;
        public record PrefixExpression(ExpressionOperator Operator, Expression Left) : ExpressionValue;
        public record SubExpression(Expression Expression) : ExpressionValue;
        public record FunctionDefinition
            (string Name, string[] ParameterNames, Expression[] Expressions) : ExpressionValue;
        public record FunctionCall(string Name, Expression[] Parameters) : ExpressionValue;
        public record FunctionReference(string Name) : ExpressionValue;
        public record Return(Expression Expression) : ExpressionValue;
        public record IfExpression(Expression[] Conditions, Expression[][] Blocks, Expression[]? ElseBlock) : ExpressionValue;
        public record WhileExpression(Expression Condition, Expression[] Block) : ExpressionValue;
        public record Void : ExpressionValue;
        public record True : ExpressionValue;
        public record False : ExpressionValue;
        public record Array(Expression[] InitialElements) : ExpressionValue;
    }
    
    public class Expression : IPositionableInText, IEquatable<Expression>
    {
        public ExpressionValue Value { get; set; } = null!;
        public PositionInText PositionInText { get; set; }

        #region IEquatable implementation
        
        public bool Equals(Expression? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Equals(other.Value);
        }

        #endregion
        
        public override string ToString()
        {
            return $"Expression {{ Value = {Value} }}";
        }
    }
}