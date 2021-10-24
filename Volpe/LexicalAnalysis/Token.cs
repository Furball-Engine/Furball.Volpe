using System;
using Volpe;

namespace Volpe.LexicalAnalysis
{
    public abstract record TokenValueOperator
    {
        public record Assign : TokenValueOperator;
        public record Add : TokenValueOperator;
        public record Sub : TokenValueOperator;
        public record Mul : TokenValueOperator;
        public record Div : TokenValueOperator;
    }

    public abstract record TokenValue
    {
        public record Literal(string Value) : TokenValue;
        public record String(string Value) : TokenValue;
        public record Column : TokenValue;
        public record Number(double Value) : TokenValue;
        public record True : TokenValue;
        public record False : TokenValue;
        public record LeftBracket : TokenValue;
        public record RightBracket : TokenValue;
        public record Dollar : TokenValue;
        public record SemiColon : TokenValue;
        public record Operator(TokenValueOperator Value) : TokenValue;
    }
    
    public class Token: IPositionableInText
    {
        public PositionInText PositionInText { get; set; }
        public TokenValue Value { get; set; }
    }
}