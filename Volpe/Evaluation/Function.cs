using System;
using Volpe.SyntaxAnalysis;

namespace Volpe.Evaluation
{
    public abstract record Function
    {
        public record Builtin(Func<EvaluatorContext, Value[], Value> Delegate, int ParameterCount) : Function
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

                Value toReturn = Value.DefaultVoid;
                
                foreach (Expression expression in Expressions)
                {
                    if (new EvaluatorContext(expression, scope).Evaluate() is Value.ToReturn value)
                    {
                        toReturn = value.Value;
                        break;
                    }
                }

                return toReturn;
            }

            public override int ParameterCount { get => ParameterNames.Length; init => throw new InvalidOperationException(); }
        }

        public abstract Value Invoke(EvaluatorContext context, Value[] actualParameters);
        
        public abstract int ParameterCount { get; init; }
    }
}