using System;
using System.Globalization;
using Volpe.SyntaxAnalysis;

namespace Volpe.Evaluation
{
    public abstract record Value
    {
        public record ToReturn(Value Value) : Value
        {
            public override string Representation => throw new InvalidOperationException();
        }

        public record Number(double Value) : Value
        {
            public override string Representation => Value.ToString(CultureInfo.InvariantCulture);
        }

        public record String(string Value) : Value
        {
            public override string Representation => '"' + Value + '"';
        }
        
        public record Void : Value
        {
            public override string Representation => "void";
        }

        public record FunctionReference(string Name, Function Function) : Value
        {
            public override string Representation => $"<Function \"{Name}\", {Function.GetHashCode()}>";
        }

        public static readonly Void DefaultVoid = new Void();
        
        public abstract string Representation { get; }
    }
}