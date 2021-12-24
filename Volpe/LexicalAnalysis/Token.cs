using System;
using Volpe;

namespace Volpe.LexicalAnalysis
{
    public abstract record TokenValueOperator
    {
        public record Add : TokenValueOperator;
        public record Sub : TokenValueOperator;
        public record Mul : TokenValueOperator;
        public record Div : TokenValueOperator;
        public record And : TokenValueOperator;
        public record Or : TokenValueOperator;
        public record Not : TokenValueOperator;

        public static TokenValueOperator FromCharacter(char c) => c switch
        {
            '+' => new Add(),
            '-' => new Sub(),
            '*' => new Mul(),
            '/' => new Div(),
            '&' => new And(),
            '|' => new Or(),
            '!' => new Not(),

            _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
        };
    }

    public abstract record TokenValueBooleanOperator
    {
        public record And : TokenValueBooleanOperator;
        public record Or : TokenValueBooleanOperator;
        public record Eq : TokenValueBooleanOperator;
        public record GreaterThan : TokenValueBooleanOperator;
        public record GreaterThanOrEqual : TokenValueBooleanOperator;
        public record LessThan : TokenValueBooleanOperator;
        public record LessThanOrEqual : TokenValueBooleanOperator;
    }

    public abstract record TokenValue
    {
        public record While : TokenValue;
        public record If : TokenValue;
        public record Elif : TokenValue;
        public record Else : TokenValue;
        public record Assign : TokenValue;
        public record Literal(string Value) : TokenValue;
        public record String(string Value) : TokenValue;
        public record Func : TokenValue;
        public record Number(double Value) : TokenValue;
        public record True : TokenValue;
        public record False : TokenValue;
        public record LeftBracket : TokenValue;
        public record RightBracket : TokenValue;
        public record LeftRoundBracket : TokenValue;
        public record RightRoundBracket : TokenValue;
        public record LeftCurlyBracket : TokenValue;
        public record RightCurlyBracket : TokenValue;
        public record Dollar : TokenValue;
        public record SemiColon : TokenValue;
        public record FuncDef : TokenValue;
        public record ArithmeticalOperator(TokenValueOperator Value) : TokenValue;
        public record BooleanOperator(TokenValueBooleanOperator Value) : TokenValue;
        public record OperatorWithAssignment(TokenValueOperator Value) : TokenValue;
        public record Comma : TokenValue;
        public record Hashtag : TokenValue;
        public record Return : TokenValue;
        
        public bool IsOperator => this is TokenValue.BooleanOperator or TokenValue.ArithmeticalOperator 
            or TokenValue.OperatorWithAssignment;
    }
    
    public class Token: IPositionableInText
    {
        public PositionInText PositionInText { get; set; }
        public TokenValue Value { get; set; } = null!;
        
    }
}