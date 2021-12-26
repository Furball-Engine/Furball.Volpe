using System;
using Furball.Volpe.LexicalAnalysis;

namespace Furball.Volpe.SyntaxAnalysis
{
    public enum ExpressionOperatorPrecedence
    {
        Lowest = 1,
        Assign,
        LogicalOr,
        LogicalAnd,
        BitwiseOr,
        BitwiseAnd,
        Equality,
        Relational,
        Add,
        Mul,
        Not,
        ArrayAccess,
    }

    [Flags]
    public enum ExpressionOperatorType : byte
    {
        Prefix = 1 << 0,
        Infix = 1 << 1
    }

    public enum ExpressionOperatorAssociationDirection
    {
        Left,
        Right
    }
    
    public abstract record ExpressionOperator
    {
        public abstract ExpressionOperatorPrecedence Precedence { get; }
        public abstract ExpressionOperatorType Type { get; }

        public virtual ExpressionOperatorAssociationDirection AssociationDirection =>
            ExpressionOperatorAssociationDirection.Left;

        public static ExpressionOperator FromArithmeticalOperatorTokenValue(TokenValueOperator v)
        {
            return v switch
            {
                TokenValueOperator.Add => new Add(),
                TokenValueOperator.Sub => new Sub(),
                TokenValueOperator.Mul => new Mul(),
                TokenValueOperator.Div => new Div(),
                
                _ => throw new ArgumentOutOfRangeException(nameof(v)),
            };
        }

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
                TokenValue.BooleanOperator(TokenValueBooleanOperator.Not) => new Not(),
                TokenValue.BooleanOperator(TokenValueBooleanOperator.NotEq) => new NotEq(),

                TokenValue.ArithmeticalOperator v => FromArithmeticalOperatorTokenValue(v.Value),
                TokenValue.Assign => new Assign(),
                TokenValue.ArrayAccess => new ArrayAccess(),
                TokenValue.OperatorWithAssignment v =>  new OperatorWithAssignment(FromArithmeticalOperatorTokenValue(v.Value)),

                _ => throw new ArgumentOutOfRangeException(nameof(tokenOperator))
            };
        }

        public record ArrayAccess : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.ArrayAccess;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }

        public record Assign : ExpressionOperator
        {
            public override ExpressionOperatorAssociationDirection AssociationDirection =>
                ExpressionOperatorAssociationDirection.Right;
            
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Assign;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Infix;
        }
        
        public record OperatorWithAssignment(ExpressionOperator Wrapped) : ExpressionOperator
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
        
        public record NotEq : ExpressionOperator
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
        
        public record Not : ExpressionOperator
        {
            public override ExpressionOperatorPrecedence Precedence => ExpressionOperatorPrecedence.Not;
            public override ExpressionOperatorType Type => ExpressionOperatorType.Prefix;
        }
    }
}