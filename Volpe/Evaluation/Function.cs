using System;
using Volpe.SyntaxAnalysis;

namespace Volpe.Evaluation
{
    public abstract record Function
    {
        public record Builtin(Func<Evaluator.Context, Value[], Value> Delegate, int ParameterCount) : Function;

        public record Standard(string[] ParameterNames, Expression[] Expressions, Scope ParentScope) : Function
        {
            public override int ParameterCount { get => ParameterNames.Length; init => throw new InvalidOperationException(); }
        }

        public abstract int ParameterCount { get; init; }
    }
}