using System;
using System.Collections.Immutable;
using Furball.Volpe.Evaluation;
using Furball.Volpe.LexicalAnalysis;
using Furball.Volpe.SyntaxAnalysis;
using NUnit.Framework;
using Environment = Furball.Volpe.Evaluation.Environment;

namespace Furball.Volpe.Tests; 

public class EvaluatorTests
{
    [Test]
    public void EvaluateAddition()
    {
        Expression expression = new Parser(new Lexer("2 + 2 + 3 + 4")
                                          .GetTokenEnumerator().ToImmutableArray()).ParseNextExpression();

        Value value = new EvaluatorContext(expression, new Environment()).Evaluate();
        Assert.AreEqual(value, new Value.Number(11));
    }

    [Test]
    public void EvaluateVariableAssignment()
    {
        Expression expression = new Parser(new Lexer("$test = 2 + 2 + 3 + 4")
                                          .GetTokenEnumerator().ToImmutableArray()).ParseNextExpression();

        Environment environment = new Environment();

        Value value = new EvaluatorContext(expression, environment).Evaluate();
            
        Assert.AreEqual(value, new Value.Number(11));

        IVariable variable;
        Variable concreteVariable = null;
        
        Assert.IsTrue(environment.TryGetVariable("test", out variable));
        Assert.IsTrue(variable is Variable);
        
        concreteVariable = variable.ToVariable();
        Assert.AreEqual(concreteVariable.RawValue, new Value.Number(11));
    }
        
        
    [Test]
    public void EvaluateFourOperations()
    {
        Expression expression = new Parser(new Lexer("2 + 2 + 3 - 4 + 4 * 8 + 6 / 2")
                                          .GetTokenEnumerator().ToImmutableArray()).ParseNextExpression();

        Value value = new EvaluatorContext(expression, new Environment()).Evaluate();
        Assert.AreEqual(value, new Value.Number(38));
    }
        
    [Test]
    public void EvaluateSubExpression()
    {
        Expression expression = new Parser(new Lexer("(2+2)*(2+2)")
                                          .GetTokenEnumerator().ToImmutableArray()).ParseNextExpression();

        Value value = new EvaluatorContext(expression, new Environment()).Evaluate();
        Assert.AreEqual(value, new Value.Number(16));
    }
        
    [Test]
    public void EvaluateVariable()
    {
        Parser parser = new Parser(new Lexer("$test = 2 + 2 + 3 - 4 + 4 * 8 + 6 / 2; $test")
                                  .GetTokenEnumerator().ToImmutableArray());

        Environment environment = new Environment();
            
        Assert.AreEqual(
            new Value[]
            {
                new EvaluatorContext(parser.ParseNextExpression(), environment).Evaluate(),
                new EvaluatorContext(parser.ParseNextExpression(), environment).Evaluate()
            }, 
            new Value[]
            {
                new Value.Number(38),
                new Value.Number(38)
            });
    }
        
    [Test]
    public void EvaluateFunctionDefinition()
    {
        Parser parser = new Parser(new Lexer(
                                       "funcdef hi($testVariable) {}").GetTokenEnumerator().ToImmutableArray());

        Environment environment = new Environment();

        new EvaluatorContext(parser.ParseNextExpression(), environment).Evaluate();

        Function function;
        environment.TryGetFunctionReference("hi", out function);

        Function.Standard standardFunction = (Function.Standard)function;
            
        CollectionAssert.AreEqual(standardFunction!.ParameterNames, new string[] { "testVariable" });
        CollectionAssert.AreEqual(standardFunction.Expressions, new Expression[] { });
    }
        
    [Test]
    public void EvaluateFunctionCall()
    {
        Parser parser = new Parser(
            new Lexer("funcdef hi($testVariable) { $testVariable2 = $testVariable; ret $testVariable2; } hi(2) + 2")
               .GetTokenEnumerator().ToImmutableArray());

        Environment environment = new Environment();
        new EvaluatorContext(parser.ParseNextExpression(), environment).Evaluate();
            
        Assert.AreEqual(new EvaluatorContext(parser.ParseNextExpression(), environment).Evaluate(), new Value.Number(4));
    }
        
    [Test]
    public void EvaluateConversionStringNumber()
    {
        Parser parser = new Parser(new Lexer("int \"2\";").GetTokenEnumerator().ToImmutableArray());

        Environment environment = new Environment(DefaultBuiltins.GetAll());

        Assert.AreEqual(new EvaluatorContext(parser.ParseNextExpression(), environment).Evaluate(), new Value.Number(2));
    }
        
        
    [Test]
    public void EvaluateConversionNumberString()
    {
        Parser parser = new Parser(new Lexer("string 2;").GetTokenEnumerator().ToImmutableArray());

        Environment environment = new Environment(DefaultBuiltins.GetAll());

        Assert.AreEqual(new EvaluatorContext(parser.ParseNextExpression(), environment).Evaluate(), new Value.String("2"));
    }
        
        
    [Test]
    public void AddTypedVariable()
    {
        TypedVariable<Value.Number> number = new TypedVariable<Value.Number>("abc", new Value.Number(1));

        Parser parser = new Parser(new Lexer("$abc").GetTokenEnumerator().ToImmutableArray());

        Environment environment = new Environment(DefaultBuiltins.GetAll());
        environment.SetVariable(number);
            
        Assert.AreEqual(new EvaluatorContext(parser.ParseNextExpression(), environment).Evaluate(), new Value.Number(1));
    }
        
    [Test]
    public void ThrowErrorIfInvalidTypeForTypedValue()
    {
        TypedVariable<Value.Number> number = new TypedVariable<Value.Number>("abc", new Value.Number(1));

        Parser parser = new Parser(new Lexer("$abc = \"ciao\"").GetTokenEnumerator().ToImmutableArray());

        Environment environment = new Environment(DefaultBuiltins.GetAll());
        environment.SetVariable(number);
            
        Assert.Throws<InvalidOperationException>(
            () => new EvaluatorContext(parser.ParseNextExpression(), environment).Evaluate());
    }
        
    [Test]
    public void ChangeTypedVariable()
    {
        TypedVariable<Value.Number> number = new TypedVariable<Value.Number>("abc", new Value.Number(1));

        Parser parser = new Parser(new Lexer("$abc = 2").GetTokenEnumerator().ToImmutableArray());

        Environment environment = new Environment(DefaultBuiltins.GetAll());
        environment.SetVariable(number);
            
        Assert.AreEqual(new EvaluatorContext(parser.ParseNextExpression(), environment).Evaluate(), new Value.Number(2));
        Assert.AreEqual(number.Value.Value, 2);
    }
}