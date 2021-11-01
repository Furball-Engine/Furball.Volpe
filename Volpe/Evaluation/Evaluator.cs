

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
                ExpressionOperator.Add => Operators.Sum(leftValue, rightValue, context),
                ExpressionOperator.Sub => Operators.Subtract(leftValue, rightValue, context),
                ExpressionOperator.Mul => Operators.Multiply(leftValue, rightValue, context),
                ExpressionOperator.Div => Operators.Divide(leftValue, rightValue, context)
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
                throw new VariableNotFoundException(expr.Name, context.RootExpression.PositionInText);
                
            return value!;
        }

        private Value EvaluateFunctionDefinition(ExpressionValue.FunctionDefinition functionDefinition, Context context)
        {
            if (!context.Scope.TrySetFunction(functionDefinition.Name,
                new Function.Standard(functionDefinition.ParameterNames, functionDefinition.Expressions, context.Scope)))
            {
                throw new CannotRedefineFunctionsException(context.RootExpression.PositionInText);
            }

            return Value.DefaultVoid;
        }
        
        private Value.FunctionReference EvaluateFunctionReference(ExpressionValue.FunctionReference functionReference, Context context)
        {
            Function? value;
            if (!context.Scope.TryGetFunctionReference(functionReference.Name, out value))
                throw new FunctionNotFoundException(functionReference.Name, context.RootExpression.PositionInText);
                
            return new Value.FunctionReference(functionReference.Name, value!);
        }

        private Value EvaluateFunctionCall(ExpressionValue.FunctionCall functionCall, Context context)
        {
            Function? function;
            if (!context.Scope.TryGetFunctionReference(functionCall.Name, out function))
                throw new FunctionNotFoundException(functionCall.Name, context.RootExpression.PositionInText);

            int parameterCount = function!.ParameterCount;
            
            if (parameterCount != functionCall.Parameters.Length)
                throw new ParamaterCountMismatchException(functionCall.Name, 
                    parameterCount, functionCall.Parameters.Length, context.RootExpression.PositionInText);
            
            Value toReturn = Value.DefaultVoid;
            
            switch (function)
            {
                case Function.Standard standardFunction:

                    Scope scope = new Scope(standardFunction.ParentScope);

                    for (int i = 0; i < parameterCount; i++)
                        scope.SetVariableValue(standardFunction.ParameterNames[i], Evaluate(functionCall.Parameters[i], context.Scope));

                    foreach (Expression expression in standardFunction.Expressions)
                    {
                        if (Evaluate(expression, scope) is Value.ToReturn value)
                        {
                            toReturn = value.Value;
                            break;
                        }
                    }
                    break;
                
                case Function.Builtin builtinFunction:
                    toReturn = builtinFunction.Delegate(context,
                        functionCall.Parameters.Select(expr => Evaluate(expr, context.Scope)).ToArray());
                    break;
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
                ExpressionValue.String expr => new Value.String(expr.Value),
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