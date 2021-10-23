using System;

namespace Volpe.LexicalAnalysis
{
    public abstract record TokenOperator
    {
        public record Assign : TokenOperator;
        public record Add : TokenOperator;
        public record Sub : TokenOperator;
        public record Mul : TokenOperator;
        public record Div : TokenOperator;
    }
    
    public abstract record Token
    {
        public record Literal(string value) : Token;
        public record String(string value) : Token;
        public record Column : Token;
        public record Number(double value) : Token;
        public record True : Token;
        public record False : Token;
        public record LeftBracket : Token;
        public record RightBracket : Token;
        public record Dollar : Token;
        public record SemiColon : Token;
        public record Operator(TokenOperator @operator) : Token;
    }
}