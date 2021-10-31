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
    }

    public abstract record TokenValue
    {
        public record Assign : TokenValue;
        public record Literal(string Value) : TokenValue;
        public record String(string Value) : TokenValue;
        public record Column : TokenValue;
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
        public record Operator(TokenValueOperator Value) : TokenValue;
        public record Comma : TokenValue;
        public record Hashtag : TokenValue;
    }
    
    public class Token: IPositionableInText
    {
        public PositionInText PositionInText { get; set; }
        public TokenValue Value { get; set; }
    }
}