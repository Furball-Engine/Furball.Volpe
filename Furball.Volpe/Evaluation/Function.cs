using System;
using Furball.Volpe.SyntaxAnalysis;

namespace Furball.Volpe.Evaluation
{
    public abstract record Function
    {
        public record Builtin(FunctionInvokeCallback Delegate, int ParameterCount) : Function
        {
            public override Value Invoke(EvaluatorContext context, Value[] actualParameters) =>
                Delegate(context, actualParameters);
        }

        public record Standard(string[] ParameterNames, Expression[] Expressions, Scope ParentScope) : Function
        {
            public override Value Invoke(EvaluatorContext context, Value[] actualParameters)
            {
                Scope scope = new Scope(this.ParentScope);

                for (int i = 0; i < ParameterCount; i++)
                    scope.SetVariableValue(ParameterNames[i], actualParameters[i]);

                Value v = new BlockEvaluatorContext(Expressions, scope, inFunction: true).Evaluate();

                if (v is Value.ToReturn(var returnedValue))
                    return returnedValue;

                return Value.DefaultVoid;
            }

            public override int ParameterCount { get => ParameterNames.Length; init => throw new InvalidOperationException(); }
        }

        public abstract Value Invoke(EvaluatorContext context, Value[] actualParameters);
        
        public abstract int ParameterCount { get; init; }
    }
}