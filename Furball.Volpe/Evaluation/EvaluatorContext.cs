using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Furball.Volpe.Exceptions;
using Furball.Volpe.LexicalAnalysis;
using Furball.Volpe.Memory;
using Furball.Volpe.SyntaxAnalysis;

namespace Furball.Volpe.Evaluation
{
    public readonly struct BlockEvaluatorContext
    {       
        public Expression[] Expressions { get; }
        public Environment Environment { get; }
        public bool InFunction { get; }
        
        public BlockEvaluatorContext(Expression[] expressions, Environment environment, bool inFunction = false)
        {
            Expressions = expressions;
            Environment = environment;
            InFunction = inFunction;
        }

        public Value Evaluate()
        {
            foreach (Expression expression in Expressions)
            {
                Value v = new EvaluatorContext(expression, Environment, InFunction).Evaluate();

                if (v is Value.ToReturn)
                    return v;
            }
            
            return Value.DefaultVoid;
        }
    }

    public readonly struct EvaluatorContext
    {
        public Expression Expression { get; }
        public Environment Environment { get; }
        public bool InFunction { get; }

        public EvaluatorContext(Expression expression, Environment environment, bool inFunction = false)
        {
            Expression = expression;
            Environment = environment;
            InFunction = inFunction;
        }

        private Value AssignVariable(string variableName, Value value)
        {
            Function? setter;
            if (Environment.TryGetSetterFromHookedVariable(variableName, out setter))
                setter!.Invoke(this, new Value[] {value});
            else
                Environment.SetVariableValue(variableName, value);

            return value;
        }

        private Value ApplyOperator(ExpressionOperator op, Value leftValue, Value rightValue)
        {
            return op switch
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

                ExpressionOperator.ArrayAccess => Operators.ArrayAccess(leftValue, rightValue, this),
                ExpressionOperator.NotEq => Operators.NotEq(leftValue, rightValue, this),
                ExpressionOperator.Append => Operators.Append(leftValue, rightValue, this),

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private Value EvaluateAssignment(Expression left, Expression right)
        {
            Value v = new EvaluatorContext(right, Environment).Evaluate();
            
            if (left.Value is ExpressionValue.Variable variable)
                return AssignVariable(variable.Name, v);
            
            if (left.Value is ExpressionValue.InfixExpression &&
                new EvaluatorContext(left, Environment).Evaluate(forceInner: false) is Value.ValueReference reference)
            {
                reference.Value.Swap(v);
                return reference.InnerOrSelf;
            }

            throw new ExpectedVariableException(Expression.PositionInText);
        }

        private Value EvaluateInfixExpression(ExpressionValue.InfixExpression expr)
        {
            if (expr.Operator is ExpressionOperator.Assign)
                return EvaluateAssignment(expr.Left, expr.Right);

            Value leftValue = new EvaluatorContext(expr.Left, Environment).Evaluate();
            Value rightValue = new EvaluatorContext(expr.Right, Environment).Evaluate();
            
            if (expr.Operator is ExpressionOperator.OperatorWithAssignment opWithAssignment)
            {
                if (expr.Left.Value is not ExpressionValue.Variable(var variableName))
                    throw new ExpectedExpressionException(expr.Left.PositionInText);

                return AssignVariable(variableName, ApplyOperator(opWithAssignment.Wrapped, leftValue, rightValue));
            }

            return ApplyOperator(expr.Operator, leftValue, rightValue);
        }

        private Value EvaluateIfExpression(ExpressionValue.IfExpression expr)
        {
            for (int i = 0; i < expr.Conditions.Length; i++)
            {
                Expression condExpression = expr.Conditions[i];

                Value value = new EvaluatorContext(condExpression, Environment).Evaluate();
                if (value is not Value.Boolean(var truthValue))
                    throw new InvalidValueTypeException(typeof(Value.Boolean), value.GetType(),
                        condExpression.PositionInText);

                if (!truthValue)
                    continue;

                Expression[] block = expr.Blocks[i];
                if (new BlockEvaluatorContext(block, Environment, InFunction).Evaluate() is Value.ToReturn v)
                    return v;

                return Value.DefaultVoid;
            }

            if (expr.ElseBlock != null)
            {
                if (new BlockEvaluatorContext(expr.ElseBlock, Environment, InFunction).Evaluate() is Value.ToReturn v)
                    return v;
            }

            return Value.DefaultVoid;
        }

        private Value EvaluateWhileExpression(ExpressionValue.WhileExpression expr)
        {
            for (;;)
            {
                Value value = new EvaluatorContext(expr.Condition, Environment).Evaluate();
                if (value is not Value.Boolean(var truthValue))
                    throw new InvalidValueTypeException(typeof(Value.Boolean), value.GetType(),
                        expr.Condition.PositionInText);

                if (!truthValue)
                    break;

                if (new BlockEvaluatorContext(expr.Block, Environment, InFunction).Evaluate() is Value.ToReturn v)
                    return v;
            }

            return Value.DefaultVoid;
        }

        private Value EvaluateVariable(ExpressionValue.Variable expr)
        {
            Value? value;

            Function? getter;
            if (Environment.TryGetGetterFromHookedVariable(expr.Name, out getter))
                value = getter!.Invoke(this, Array.Empty<Value>());
            else if (!Environment.TryGetVariableValue(expr.Name, out value))
                throw new VariableNotFoundException(expr.Name, Expression.PositionInText);

            return value!;
        }

        private Value EvaluateLambda(ExpressionValue.Lambda lambda) =>
            new Value.FunctionReference("<lambda>",
                new Function.Standard(lambda.ParameterNames, lambda.Expressions, Environment));
        
        
        private Value EvaluateFunctionDefinition(ExpressionValue.FunctionDefinition functionDefinition)
        {
            if (!Environment.TrySetFunction(functionDefinition.Name,
                    new Function.Standard(functionDefinition.ParameterNames, functionDefinition.Expressions, Environment)))
            {
                throw new CannotRedefineFunctionsException(Expression.PositionInText);
            }

            return Value.DefaultVoid;
        }

        private Value.FunctionReference EvaluateFunctionReference(ExpressionValue.FunctionReference functionReference)
        {
            Function? value;
            if (!Environment.TryGetFunctionReference(functionReference.Name, out value))
                throw new FunctionNotFoundException(functionReference.Name, Expression.PositionInText);

            return new Value.FunctionReference(functionReference.Name, value!);
        }

        private Value EvaluateFunctionCall(ExpressionValue.FunctionCall functionCall)
        {
            Function? function;
            if (!Environment.TryGetFunctionReference(functionCall.Name, out function))
                throw new FunctionNotFoundException(functionCall.Name, Expression.PositionInText);

            int parameterCount = function!.ParameterCount;

            if (functionCall.Parameters.Length < parameterCount)
                throw new ParamaterCountMismatchException(functionCall.Name,
                    parameterCount, functionCall.Parameters.Length, Expression.PositionInText);

            List<Value> values = new List<Value>();
            foreach (var expression in functionCall.Parameters)
                values.Add(new EvaluatorContext(expression, Environment).Evaluate());

            return function.Invoke(this, values.ToArray());
        }

        private Value EvaluatePrefixExpression(ExpressionValue.PrefixExpression expr)
        {
            Value leftValue = new EvaluatorContext(expr.Left, Environment).Evaluate();

            return expr.Operator switch
            {
                ExpressionOperator.Add => Operators.Positive(leftValue, this),
                ExpressionOperator.Sub => Operators.Negative(leftValue, this),
                ExpressionOperator.Not => Operators.Not(leftValue, this),

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static Value[] EvaluateAll(string source) => EvaluateAll(source, new Environment());

        public Value EvaluateArray(Expression[] initialElements)
        {
            Environment environment = Environment;

            return new Value.Array(
                initialElements.Select(element => new CellSwap<Value>(new EvaluatorContext(element, environment, false).Evaluate())).ToList());
        }

        public static Value[] EvaluateAll(string source, Environment environment)
        {
            IEnumerable<Expression> expressions = new Parser(new Lexer(source).GetTokenEnumerator())
                .GetExpressionEnumerator();

            return expressions.Select(expression => new EvaluatorContext(expression, environment).Evaluate()).ToArray();
        }
        
        public static void EvaluateAllNoReturn(string source, Environment environment)
        {
            IEnumerable<Expression> expressions = new Parser(new Lexer(source).GetTokenEnumerator())
                .GetExpressionEnumerator();

            foreach (var expression in expressions)
                new EvaluatorContext(expression, environment).Evaluate();
        }
        
        public Value Evaluate(bool forceInner = true)
        {
            Value evaluated = Expression.Value switch
            {
                ExpressionValue.InfixExpression expr => EvaluateInfixExpression(expr),
                ExpressionValue.PrefixExpression expr => EvaluatePrefixExpression(expr),
                ExpressionValue.Number(var v) => new Value.Number(v),
                ExpressionValue.Variable expr => EvaluateVariable(expr),
                ExpressionValue.FunctionDefinition expr => EvaluateFunctionDefinition(expr),
                ExpressionValue.SubExpression(var expr) => new EvaluatorContext(expr, Environment).Evaluate(),
                ExpressionValue.FunctionReference expr => EvaluateFunctionReference(expr),
                ExpressionValue.FunctionCall expr => EvaluateFunctionCall(expr),
                ExpressionValue.String expr => new Value.String(expr.Value),
                ExpressionValue.IfExpression expr => EvaluateIfExpression(expr),
                ExpressionValue.WhileExpression expr => EvaluateWhileExpression(expr),
                ExpressionValue.Return(var expr) when InFunction => new Value.ToReturn(new EvaluatorContext(expr, Environment).Evaluate()),
                ExpressionValue.Return(_) when !InFunction => throw new ReturnNotAllowedOutsideFunctionsException(Expression.PositionInText),
                ExpressionValue.True => Value.DefaultTrue,
                ExpressionValue.False => Value.DefaultFalse,
                ExpressionValue.Array(var initialElements) => EvaluateArray(initialElements),
                ExpressionValue.Lambda lambda => EvaluateLambda(lambda),

                _ => throw new ArgumentException()
            };

            return forceInner ? evaluated.InnerOrSelf : evaluated;
        }

    }
}