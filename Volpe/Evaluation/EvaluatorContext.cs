

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Volpe.Exceptions;
using Volpe.LexicalAnalysis;
using Volpe.SyntaxAnalysis;

namespace Volpe.Evaluation
{
    public readonly struct BlockEvaluatorContext
    {       
        public Expression[] Expressions { get; }
        public Scope Scope { get; }
        public bool InFunction { get; }
        
        public BlockEvaluatorContext(Expression[] expressions, Scope scope, bool inFunction = false)
        {
            Expressions = expressions;
            Scope = scope;
            InFunction = inFunction;
        }

        public Value Evaluate()
        {
            foreach (Expression expression in Expressions)
            {
                Value v = new EvaluatorContext(expression, Scope, InFunction).Evaluate();

                if (v is Value.ToReturn value)
                    return v;
            }
            
            return Value.DefaultVoid;
        }
    }
    
    public readonly struct EvaluatorContext
    {
        public EvaluatorContext(Expression expression, Scope scope, bool inFunction = false)
        {
            Expression = expression;
            Scope = scope;
            InFunction = inFunction;
        }
        
        public Expression Expression { get; }
        public Scope Scope { get; }
        public bool InFunction { get; }
        
        private Value EvaluateInfixExpression(ExpressionValue.InfixExpression expr)
        {
            Value leftValue = new EvaluatorContext(expr.Left, Scope).Evaluate();
            Value rightValue = new EvaluatorContext(expr.Right, Scope).Evaluate();

            return expr.Operator switch
            {
                ExpressionOperator.Add => Operators.Sum(leftValue, rightValue, this),
                ExpressionOperator.Sub => Operators.Subtract(leftValue, rightValue, this),
                ExpressionOperator.Mul => Operators.Multiply(leftValue, rightValue, this),
                ExpressionOperator.Div => Operators.Divide(leftValue, rightValue, this),
                
                ExpressionOperator.LogicalAnd => Operators.LogicalAnd(leftValue, rightValue, this),
                ExpressionOperator.LogicalOr => Operators.LogicalOr(leftValue, rightValue, this),
                
                ExpressionOperator.Eq => Operators.Eq(leftValue, rightValue, this),
                ExpressionOperator.GreaterThan => Operators.GreaterThan(leftValue, rightValue, this),
                ExpressionOperator.GreaterThanOrEqual => Operators.GreaterThanOrEqual(leftValue, rightValue, this),
                ExpressionOperator.LessThan => Operators.LessThan(leftValue, rightValue, this),
                ExpressionOperator.LessThanOrEqual => Operators.LessThanOrEqual(leftValue, rightValue, this),
                
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private Value EvaluateIfExpression(ExpressionValue.IfExpression expr)
        {
            for (int i = 0; i < expr.Conditions.Length; i++)
            {
                Expression condExpression = expr.Conditions[i];
                
                Value value = new EvaluatorContext(condExpression, Scope).Evaluate();
                if (value is not Value.Boolean(var truthValue))
                    throw new InvalidValueTypeException(typeof(Value.Boolean), value.GetType(), condExpression.PositionInText);

                if (!truthValue)
                    continue;

                Expression[] block = expr.Blocks[i];
                if (new BlockEvaluatorContext(block, Scope, InFunction).Evaluate() is Value.ToReturn v)
                    return v;
            }

            if (expr.ElseBlock != null)
            {
                if (new BlockEvaluatorContext(expr.ElseBlock, Scope, InFunction).Evaluate() is Value.ToReturn v)
                    return v;
            }

            return Value.DefaultVoid;
        }

        private Value EvaluateAssignment(ExpressionValue.Assignment expr)
        {
            Value value = new EvaluatorContext(expr.Expression, Scope).Evaluate();

            Function? setter;
            if (Scope.TryGetSetterFromHookedVariable(expr.VariableName, out setter))
                setter!.Invoke(this, new Value[] { value });
            else
                Scope.SetVariableValue(expr.VariableName, value);

            return value;
        }
        
        private Value EvaluateVariable(ExpressionValue.Variable expr)
        {
            Value? value;
            
            Function? getter;
            if (Scope.TryGetGetterFromHookedVariable(expr.Name, out getter))
                value = getter!.Invoke(this, Array.Empty<Value>());
            else if (!Scope.TryGetVariableValue(expr.Name, out value))
                throw new VariableNotFoundException(expr.Name, Expression.PositionInText);
            
            return value!;
        }

        private Value EvaluateFunctionDefinition(ExpressionValue.FunctionDefinition functionDefinition)
        {
            if (!Scope.TrySetFunction(functionDefinition.Name,
                new Function.Standard(functionDefinition.ParameterNames, functionDefinition.Expressions, Scope)))
            {
                throw new CannotRedefineFunctionsException(Expression.PositionInText);
            }

            return Value.DefaultVoid;
        }
        
        private Value.FunctionReference EvaluateFunctionReference(ExpressionValue.FunctionReference functionReference)
        {
            Function? value;
            if (!Scope.TryGetFunctionReference(functionReference.Name, out value))
                throw new FunctionNotFoundException(functionReference.Name, Expression.PositionInText);
            
            return new Value.FunctionReference(functionReference.Name, value!);
        }

        private Value EvaluateFunctionCall(ExpressionValue.FunctionCall functionCall)
        {
            Function? function;
            if (!Scope.TryGetFunctionReference(functionCall.Name, out function))
                throw new FunctionNotFoundException(functionCall.Name, Expression.PositionInText);

            int parameterCount = function!.ParameterCount;
            
            if (functionCall.Parameters.Length < parameterCount)
                throw new ParamaterCountMismatchException(functionCall.Name, 
                    parameterCount, functionCall.Parameters.Length, Expression.PositionInText);

            List<Value> values = new List<Value>();
            foreach (var expression in functionCall.Parameters)
                values.Add(new EvaluatorContext(expression, Scope).Evaluate());

            return function.Invoke(this, values.ToArray());
        }
        
        private Value EvaluatePrefixExpression(ExpressionValue.PrefixExpression expr)
        {
            Value leftValue = new EvaluatorContext(expr.Left, Scope).Evaluate();

            return expr.Operator switch
            {
                ExpressionOperator.Add => Operators.Positive(leftValue, this),
                ExpressionOperator.Sub => Operators.Negative(leftValue, this),
                
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static Value[] EvaluateAll(string source) => EvaluateAll(source, new Scope());

        public static Value[] EvaluateAll(string source, Scope scope)
        {
            IEnumerable<Expression> expressions = new Parser(new Lexer(source).GetTokenEnumerator().ToImmutableArray())
                .GetExpressionEnumerator();

            return expressions.Select(expression => new EvaluatorContext(expression, scope).Evaluate()).ToArray();
        }
        
        public Value Evaluate()
        {
            return Expression.Value switch
            {
                ExpressionValue.InfixExpression expr => EvaluateInfixExpression(expr),
                ExpressionValue.PrefixExpression expr => EvaluatePrefixExpression(expr),
                ExpressionValue.Number(var v) => new Value.Number(v),
                ExpressionValue.Assignment expr => EvaluateAssignment(expr),
                ExpressionValue.Variable expr => EvaluateVariable(expr),
                ExpressionValue.FunctionDefinition expr => EvaluateFunctionDefinition(expr),
                ExpressionValue.SubExpression(var expr) => new EvaluatorContext(expr, Scope).Evaluate(),
                ExpressionValue.FunctionReference expr => EvaluateFunctionReference(expr),
                ExpressionValue.FunctionCall expr => EvaluateFunctionCall(expr),
                ExpressionValue.String expr => new Value.String(expr.Value),
                ExpressionValue.IfExpression expr => EvaluateIfExpression(expr),
                ExpressionValue.Return(var expr) when InFunction => new Value.ToReturn(new EvaluatorContext(expr, Scope).Evaluate()),
                ExpressionValue.Return(_) when !InFunction => throw new ReturnNotAllowedOutsideFunctionsException(Expression.PositionInText),
                ExpressionValue.True => Value.DefaultTrue,
                ExpressionValue.False => Value.DefaultFalse,

                _ => throw new ArgumentException()
            };
        }

    }
}