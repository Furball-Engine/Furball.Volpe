using System;
using Volpe.LexicalAnalysis;

namespace Volpe.SyntaxAnalysis
{
    public enum ExpressionOperatorPrecedence
    {
        Lowest = 1,
        LogicalOr,
        LogicalAnd,
        BitwiseOr,
        BitwiseAnd,
        Equality,
        Relational,
        Add,
        Mul,
    }

    [Flags]
    public enum ExpressionOperatorType : byte
    {
        Prefix = 1 << 0,
        Infix = 1 << 1
    }

    public abstract record ExpressionOperator
    {
        public abstract ExpressionOperatorPrecedence Precedence { get; }
        public abstract ExpressionOperatorType Type { get; }

        public static ExpressionOperator FromTokenValue(TokenValue tokenOperator)
        {
            return tokenOperator switch
            {
                TokenValue.BooleanOperator(TokenValueBooleanOperator.And) => new LogicalAnd(),
                TokenValue.BooleanOperator(TokenValueBooleanOperator.Eq) => new Eq(),
                TokenValue.BooleanOperator(TokenValueBooleanOperator.Or) => new LogicalOr(),
                TokenValue.BooleanOperator(TokenValueBooleanOperator.GreaterThan) => new GreaterThan(),
                TokenValue.BooleanOperator(TokenValueBooleanOperator.GreaterThanOrEqual) => new GreaterThanOrEqual(),
                TokenValue.BooleanOperator(TokenValueBooleanOperator.LessThan) => new LessThan(),
                TokenValue.BooleanOperator(TokenValueBooleanOperator.LessThanOrEqual) => new LessThanOrEqual(),
                
                TokenValue.ArithmeticalOperator(TokenValueOperator.Add) => new Add(),
                TokenValue.ArithmeticalOperator(TokenValueOperator.Sub) => new Sub(),
                TokenValue.ArithmeticalOperator(TokenValueOperator.Mul) => new Mul(),
                TokenValue.ArithmeticalOperator(TokenValueOperator.Div) => new Div(),

                _ => throw new ArgumentOutOfRangeException(nameof(tokenOperator))
            };
        }

        public record Add : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Add;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix | ExpressionOperatorType.Prefix;
        }

        public record Sub : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Add;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix | ExpressionOperatorType.Prefix;
        }

        public record Mul : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Mul;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }
        
        public record Div : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Mul;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }
        
        public record LogicalAnd : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.LogicalAnd;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }
        
        public record LogicalOr : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.LogicalOr;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }
        
        public record Eq : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Equality;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }
        
        public record GreaterThan : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Relational;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }
        
        public record GreaterThanOrEqual : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Relational;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }
        
        public record LessThan : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Relational;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }
        
        public record LessThanOrEqual : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Relational;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }
    }
}