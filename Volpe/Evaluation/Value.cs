using Volpe.SyntaxAnalysis;

namespace Volpe.Evaluation
{
    public abstract record Value
    {
        public record Number(double Value) : Value;
        public record Void : Value;
        public record Function(string[] ParameterNames, Expression[] Expressions) : Value;
    }
}