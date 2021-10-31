using Volpe.SyntaxAnalysis;

namespace Volpe.Evaluation
{
    public abstract record Value
    {
        public record ToReturn(Value Value) : Value;
        public record Number(double Value) : Value;
        public record Void : Value;

        public static readonly Void DefaultVoid = new Void();
        
        public record Function(string[] ParameterNames, Expression[] Expressions, Scope ParentScope) : Value;
    }
}