using System;
using Volpe.SyntaxAnalysis;

namespace Volpe.Evaluation
{
    public abstract record Function
    {
        public record Builtin(Func<Evaluator.Context, Value[], Value> Delegate) : Function;
        public record Standard(string[] ParameterNames, Expression[] Expressions, Scope ParentScope) : Function;
    }
}