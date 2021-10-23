using System;

namespace Volpe.LexicalAnalysis
{
    public abstract record Token
    {
        public record Literal(string value) : Token;
        public record String(string value) : Token;
        public record Column : Token;
        public record Number(double value) : Token;
        public record True : Token;
        public record False : Token;
        public record Eq : Token;
    }
}