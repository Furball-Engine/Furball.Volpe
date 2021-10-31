

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
                new Value.Function(functionDefinition.ParameterNames, functionDefinition.Expressions, context.Scope)))
            {
                throw new CannotRedefineFunctionsException(context.RootExpression.PositionInText);
            }

            return Value.DefaultVoid;
        }
        
        private Value.Function EvaluateFunctionReference(ExpressionValue.FunctionReference functionReference, Context context)
        {
            Value.Function? value;
            if (!context.Scope.TryGetFunctionReference(functionReference.Name, out value))
                throw new VariableNotFoundException(context.RootExpression.PositionInText);
                
            return value!;
        }

        private Value EvaluateFunctionCall(ExpressionValue.FunctionCall functionCall, Context context)
        {
            Value.Function? function;
            if (!context.Scope.TryGetFunctionReference(functionCall.Name, out function))
                throw new VariableNotFoundException(context.RootExpression.PositionInText);

            int parameterCount = function!.ParameterNames.Length;
            
            if (parameterCount != functionCall.Parameters.Length)
                throw new ParamaterCountMismatchException(context.RootExpression.PositionInText);

            Scope scope = new Scope(function.ParentScope);

            for (int i = 0; i < parameterCount; i++)
                scope.SetVariableValue(function.ParameterNames[i], Evaluate(functionCall.Parameters[i], context.Scope));

            Value toReturn = Value.DefaultVoid;
            
            foreach (Expression expression in function.Expressions)
            {
                if (Evaluate(expression, scope) is Value.ToReturn value)
                {
                    toReturn = value.Value;
                    break;
                }
            }

            return toReturn;
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
                ExpressionValue.FunctionReference expr => EvaluateFunctionReference(expr, context),
                ExpressionValue.FunctionCall expr => EvaluateFunctionCall(expr, context),
                ExpressionValue.Return(var expr) => new Value.ToReturn(Evaluate(expr, context.Scope))
            };
        }

        public Value Evaluate(Expression expression, Scope scope) =>
            Evaluate(new Context {Scope = scope, RootExpression = expression});

        public void EvaluateAll(IEnumerable<Expression> expressions, Scope scope)
        {
            foreach (var expression in expressions)
            {
                if (Evaluate(expression, scope) is Value.ToReturn)
                    throw new ReturnNotAllowedOutsideFunctionsException(expression.PositionInText);
            }
        }
    }
}