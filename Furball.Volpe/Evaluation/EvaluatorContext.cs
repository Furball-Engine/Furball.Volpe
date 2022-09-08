using System;
using System.Collections.Generic;
using System.Linq;
using Furball.Volpe.Exceptions;
using Furball.Volpe.LexicalAnalysis;
using Furball.Volpe.Memory;
using Furball.Volpe.SyntaxAnalysis;

namespace Furball.Volpe.Evaluation; 

public readonly struct BlockEvaluatorContext
{       
    public Expression[] Expressions { get; }
    public Environment Environment { get; }
    public bool InFunction { get; }
        
    public BlockEvaluatorContext(Expression[] expressions, Environment environment, bool inFunction = false)
    {
        Expressions = expressions;
        Environment = environment;
        InFunction  = inFunction;
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
        Expression  = expression;
        Environment = environment;
        InFunction  = inFunction;
    }

    private Value AssignVariable(string variableName, Value value)
    {
        if (Environment.TryGetVariable(variableName, out var variable))
        {
            switch (variable)
            {
                case HookedVariable hookedVariable:
                    hookedVariable.Setter.Invoke(this, new Value[] { value });
                    break;

                case Variable normalVariable:
                    normalVariable.RawValue = value;
                    break;

                default:
                    throw new Exception("Unreachable code");
            }
        } 
        else
            Environment.SetVariable(new Variable(variableName, value));


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
            ExpressionOperator.LogicalOr  => Operators.LogicalOr(leftValue, rightValue, this),

            ExpressionOperator.Eq                 => Operators.Eq(leftValue, rightValue, this),
            ExpressionOperator.GreaterThan        => Operators.GreaterThan(leftValue, rightValue, this),
            ExpressionOperator.GreaterThanOrEqual => Operators.GreaterThanOrEqual(leftValue, rightValue, this),
            ExpressionOperator.LessThan           => Operators.LessThan(leftValue, rightValue, this),
            ExpressionOperator.LessThanOrEqual    => Operators.LessThanOrEqual(leftValue, rightValue, this),

            ExpressionOperator.ArrayAccess => Operators.ArrayAccess(leftValue, rightValue, this),
            ExpressionOperator.NotEq       => Operators.NotEq(leftValue, rightValue, this),
            ExpressionOperator.Append      => Operators.Append(leftValue, rightValue, this),

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Value EvaluateAssignment(Expression left, Value rightV)
    {
        if (left.Value is ExpressionValue.Variable variable)
            return AssignVariable(variable.Name, rightV);
            
        if (left.Value is ExpressionValue.InfixExpression &&
            new EvaluatorContext(left, Environment).Evaluate(forceInner: false) is Value.ValueReference reference)
        {
            reference.Value.Swap(rightV);
            return reference.InnerOrSelf;
        }

        throw new ExpectedVariableException(Expression.PositionInText);
    }

    private Value EvaluateInfixExpression(ExpressionValue.InfixExpression expr)
    {
        Value rightValue = new EvaluatorContext(expr.Right, Environment).Evaluate();
            
        if (expr.Operator is ExpressionOperator.Assign)
            return EvaluateAssignment(expr.Left, rightValue);

        Value leftValue = new EvaluatorContext(expr.Left, Environment).Evaluate();
            
        if (expr.Operator is ExpressionOperator.OperatorWithAssignment opWithAssignment)
        {
            Value value = ApplyOperator(opWithAssignment.Wrapped, leftValue, rightValue);
            return EvaluateAssignment(expr.Left, value);
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
        Value value;

        if (Environment.TryGetVariable(expr.Name, out var variable))
        {
            switch (variable)
            {
                case HookedVariable hookedVariable:
                    value = hookedVariable.Getter.Invoke(this, Array.Empty<Value>());
                    break;

                case Variable normalVariable:
                    value = normalVariable.RawValue;
                    break;

                default:
                    throw new Exception("Unreachable code");
            }
        } 
        else
            throw new VariableNotFoundException(expr.Name, Expression.PositionInText);

        return value;
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
        
    private Value EvaluateMethodCall(ExpressionValue.MethodCall methodCall)
    {
        Function? function;
        Value     v = new EvaluatorContext(methodCall.Expression, Environment, true).Evaluate();

        Class? cls = v.Class ?? v.BaseClass;
            
        if (cls == null)
            throw new UnknownMethodException(methodCall.Name, "null", Expression.PositionInText);
            
        if (!cls.TryGetMethod(methodCall.Name, out function))
            throw new UnknownMethodException(methodCall.Name, cls.Name, Expression.PositionInText);
            
        int parameterCount = function!.ParameterCount;

        if (methodCall.Parameters.Length < parameterCount - 1)
            throw new ParamaterCountMismatchException(methodCall.Name,
                                                      parameterCount, methodCall.Parameters.Length, Expression.PositionInText);

        List<Value> values = new List<Value>() {v};
            
        foreach (var expression in methodCall.Parameters)
            values.Add(new EvaluatorContext(expression, Environment).Evaluate());

        return function.Invoke(this, values.ToArray());
    }
        
    private Value EvaluateFunctionCall(ExpressionValue.FunctionCall functionCall)
    {
        Function? function;
        Class?    cls = null;

        if (!Environment.TryGetFunctionReference(functionCall.Name, out function))
        {
            if (!Environment.TryGetClass(functionCall.Name, out cls) || !cls!.TryGetConstructor(out function))
                throw new FunctionNotFoundException(functionCall.Name, Expression.PositionInText);
        }

        int parameterCount = function!.ParameterCount;

        if (functionCall.Parameters.Length < parameterCount)
            throw new ParamaterCountMismatchException(functionCall.Name,
                                                      parameterCount, functionCall.Parameters.Length, Expression.PositionInText);

        List<Value> values = new List<Value>();
        foreach (var expression in functionCall.Parameters)
            values.Add(new EvaluatorContext(expression, Environment).Evaluate());

        Value v = function.Invoke(this, values.ToArray());
        v.Class = cls;

        return v;
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
        
    public Value EvaluateObject(string[] keys, Expression[] expressions)
    {
        Dictionary<string, CellSwap<Value>> dict = new Dictionary<string, CellSwap<Value>>();

        for (int i = 0; i < keys.Length; i++)
        {
            if (dict.ContainsKey(keys[i]))
                throw new KeyAlreadyDefinedException(keys[i], Expression.PositionInText);
                
            dict.Add(keys[i],
                     new CellSwap<Value>(new EvaluatorContext(expressions[i], Environment, false).Evaluate()));
        }

        return new Value.Object(dict);
    }

    public Value EvaluateClassDef(ExpressionValue.ClassDefinition expr)
    {
        (string name, Function function)[] fns = new (string, Function)[expr.MethodDefinitions.Length];

        for (int i = 0; i < expr.MethodDefinitions.Length; i++)
        {
            var (name, parameterNames, expressions) = expr.MethodDefinitions[i];
            Function function = new Function.Standard(parameterNames, expressions, Environment);

            fns[i] = (name, function);
        }

        Class? extendsClass = null;
        if (expr.ExtendsClassName != null && !Environment.TryGetClass(expr.ExtendsClassName, out extendsClass))
            throw new ClassNotFoundException(expr.ExtendsClassName, Expression.PositionInText);

        Environment.TrySetClass(expr.ClassName, new Class(expr.ClassName, fns, extendsClass));

        return Value.DefaultVoid;
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
        Value evaluated = this.Expression.Value switch {
            ExpressionValue.InfixExpression expr                  => this.EvaluateInfixExpression(expr),
            ExpressionValue.PrefixExpression expr                 => this.EvaluatePrefixExpression(expr),
            ExpressionValue.Number(var v)                         => new Value.Number(v),
            ExpressionValue.Byte(var v)                           => new Value.Byte(v),
            ExpressionValue.Variable expr                         => this.EvaluateVariable(expr),
            ExpressionValue.FunctionDefinition expr               => this.EvaluateFunctionDefinition(expr),
            ExpressionValue.SubExpression(var expr)               => new EvaluatorContext(expr, this.Environment).Evaluate(),
            ExpressionValue.FunctionReference expr                => this.EvaluateFunctionReference(expr),
            ExpressionValue.FunctionCall expr                     => this.EvaluateFunctionCall(expr),
            ExpressionValue.String expr                           => new Value.String(expr.Value),
            ExpressionValue.IfExpression expr                     => this.EvaluateIfExpression(expr),
            ExpressionValue.WhileExpression expr                  => this.EvaluateWhileExpression(expr),
            ExpressionValue.Return(var expr) when this.InFunction => new Value.ToReturn(new EvaluatorContext(expr, this.Environment).Evaluate()),
            ExpressionValue.Return(_) when !this.InFunction       => throw new ReturnNotAllowedOutsideFunctionsException(this.Expression.PositionInText),
            ExpressionValue.True                                  => Value.DefaultTrue,
            ExpressionValue.False                                 => Value.DefaultFalse,
            ExpressionValue.Array(var initialElements)            => this.EvaluateArray(initialElements),
            ExpressionValue.Lambda lambda                         => this.EvaluateLambda(lambda),
            ExpressionValue.Object(var keys, var expressions)     => this.EvaluateObject(keys, expressions),
            ExpressionValue.ClassDefinition def                   => this.EvaluateClassDef(def),
            ExpressionValue.MethodCall methodCall                 => this.EvaluateMethodCall(methodCall),

            _ => throw new ArgumentException(this.Expression.Value.GetType().ToString())
        };

        return forceInner ? evaluated.InnerOrSelf : evaluated;
    }
}