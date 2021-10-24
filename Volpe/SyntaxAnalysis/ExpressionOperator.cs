using System;
using Volpe.LexicalAnalysis;

namespace Volpe.SyntaxAnalysis
{
    public enum ExpressionOperatorPrecedence
    {
        Lowest = 1,
        Add,
        Mul,
        Assign
    }

    [Flags]
    public enum ExpressionOperatorType : byte
    {
        Prefix = 1 << 0,
        Infix = 1 << 1,
        Postfix = 1 << 2
    }

    public abstract record ExpressionOperator
    {
        public abstract ExpressionOperatorPrecedence Precedence { get; }
        public abstract ExpressionOperatorType Type { get; }

        public static ExpressionOperator FromTokenOperator(TokenValueOperator tokenOperator)
        {
            return tokenOperator switch
            {
                TokenValueOperator.Add => new Add(),
                TokenValueOperator.Assign => new Assign(),
                TokenValueOperator.Div => new Div(),
                TokenValueOperator.Mul => new Mul(),
                TokenValueOperator.Sub => new Sub(),

                _ => throw new ArgumentOutOfRangeException(nameof(tokenOperator))
            };
        }

        public record Assign : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Assign;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
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
    }
}