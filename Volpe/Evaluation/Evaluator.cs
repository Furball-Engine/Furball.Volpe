

using System.Collections.Generic;
using System.Linq;
using Volpe.Exceptions;
using Volpe.SyntaxAnalysis;

namespace Volpe.Evaluation
{
    public class Evaluator
    {
        public readonly struct Context
        {
            public Expression RootExpression { get; init; }
            public Scope Scope { get; init; }
        } 
        
        private Value EvaluateInfixExpression(ExpressionValue.InfixExpression expr, Context context)
        {
            Value leftValue = Evaluate(expr.Left, context.Scope);
            Value rightValue = Evaluate(expr.Right, context.Scope);

            return expr.Operator switch
            {
                ExpressionOperator.Add => Operators.Sum(leftValue, rightValue),
                ExpressionOperator.Sub => Operators.Subtract(leftValue, rightValue),
                ExpressionOperator.Mul => Operators.Multiply(leftValue, rightValue),
                ExpressionOperator.Div => Operators.Divide(leftValue, rightValue)
            };
        }

        private Value EvaluateAssignment(ExpressionValue.Assignment expr, Context context)
        {
            Value value = Evaluate(expr.Expression, context.Scope);
            context.Scope.SetVariableValue(expr.VariableName, value);

            return value;
        }
        
        private Value EvaluateVariable(ExpressionValue.Variable expr, Context context)
        {
            Value? value;
            if (!context.Scope.TryGetVariableValue(expr.Name, out value))
                throw new VariableNotFoundException(context.RootExpression.PositionInText);
                
            return value!;
        }

        private Value EvaluateFunctionDefinition(ExpressionValue.FunctionDefinition functionDefinition, Context context)
        {
            if (!context.Scope.TrySetFunction(functionDefinition.Name,
                new Value.Function(functionDefinition.ParameterNames, functionDefinition.Expressions)))
            {
                throw new CannotRedefineFunctionsException(context.RootExpression.PositionInText);
            }

            return new Value.Void();
        }
        
        private Value.Function EvaluateFunctionReference(ExpressionValue.FunctionReference functionReference, Context context)
        {
            Value.Function? value;
            if (!context.Scope.TryGetFunctionReference(functionReference.Name, out value))
                throw new VariableNotFoundException(context.RootExpression.PositionInText);
                
            return value!;
        }

        public Value Evaluate(Context context)
        {
            return context.RootExpression.Value switch
            {
                ExpressionValue.InfixExpression expr => EvaluateInfixExpression(expr, context),
                ExpressionValue.Number(var v) => new Value.Number(v),
                ExpressionValue.Assignment expr => EvaluateAssignment(expr, context),
                ExpressionValue.Variable expr => EvaluateVariable(expr, context),
                ExpressionValue.FunctionDefinition expr => EvaluateFunctionDefinition(expr, context),
                ExpressionValue.SubExpression(var expr) => Evaluate(expr, context.Scope),
                ExpressionValue.FunctionReference expr => EvaluateFunctionReference(expr, context)
            };
        }

        public Value Evaluate(Expression expression, Scope scope) =>
            Evaluate(new Context {Scope = scope, RootExpression = expression});

        public IEnumerable<Value> EvaluateAll(IEnumerable<Expression> expressions, Scope scope) =>
            expressions.Select(expression => Evaluate(expression, scope));
    }
}