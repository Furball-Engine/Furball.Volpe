using System;
using Volpe.SyntaxAnalysis;

namespace Volpe.Evaluation
{
    public abstract record Value
    {
        public record ToReturn(Value Value) : Value;
        public record Number(double Value) : Value;
        public record String(string Value) : Value;
        public record Void : Value;

        public record FunctionReference(string Name, Function Function) : Value;
        
        public static readonly Void DefaultVoid = new Void();
    }
}