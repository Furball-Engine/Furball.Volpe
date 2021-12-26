using System.Collections.Immutable;
using Furball.Volpe.Evaluation;
using Furball.Volpe.LexicalAnalysis;
using Furball.Volpe.SyntaxAnalysis;
using NUnit.Framework;

namespace Furball.Volpe.Tests
{
    public class EvaluatorTests
    {
        [Test]
        public void EvaluateAddition()
        {
            Expression expression = new Parser(new Lexer("2 + 2 + 3 + 4")
                .GetTokenEnumerator().ToImmutableArray()).ParseNextExpression();

            Value value = new EvaluatorContext(expression, new Scope()).Evaluate();
            Assert.AreEqual(value, new Value.Number(11));
        }

        [Test]
        public void EvaluateVariableAssignment()
        {
            Expression expression = new Parser(new Lexer("$test = 2 + 2 + 3 + 4")
                .GetTokenEnumerator().ToImmutableArray()).ParseNextExpression();

            Scope scope = new Scope();

            Value value = new EvaluatorContext(expression, scope).Evaluate();
            
            Assert.AreEqual(value, new Value.Number(11));
            Assert.IsTrue(scope.TryGetVariableValue("test", out value));
            Assert.AreEqual(value, new Value.Number(11));
        }
        
        
        [Test]
        public void EvaluateFourOperations()
        {
            Expression expression = new Parser(new Lexer("2 + 2 + 3 - 4 + 4 * 8 + 6 / 2")
                .GetTokenEnumerator().ToImmutableArray()).ParseNextExpression();

            Value value = new EvaluatorContext(expression, new Scope()).Evaluate();
            Assert.AreEqual(value, new Value.Number(38));
        }
        
        [Test]
        public void EvaluateSubExpression()
        {
            Expression expression = new Parser(new Lexer("(2+2)*(2+2)")
                .GetTokenEnumerator().ToImmutableArray()).ParseNextExpression();

            Value value = new EvaluatorContext(expression, new Scope()).Evaluate();
            Assert.AreEqual(value, new Value.Number(16));
        }
        
        [Test]
        public void EvaluateVariable()
        {
            Parser parser = new Parser(new Lexer("$test = 2 + 2 + 3 - 4 + 4 * 8 + 6 / 2; $test")
                .GetTokenEnumerator().ToImmutableArray());

            Scope scope = new Scope();
            
            Assert.AreEqual(
                new Value[]
                {
                    new EvaluatorContext(parser.ParseNextExpression(), scope).Evaluate(),
                    new EvaluatorContext(parser.ParseNextExpression(), scope).Evaluate()
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

            Scope scope = new Scope();

            new EvaluatorContext(parser.ParseNextExpression(), scope).Evaluate();

            Function function;
            scope.TryGetFunctionReference("hi", out function);

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

            Scope scope = new Scope();
            new EvaluatorContext(parser.ParseNextExpression(), scope).Evaluate();
            
            Assert.AreEqual(new EvaluatorContext(parser.ParseNextExpression(), scope).Evaluate(), new Value.Number(4));
        }
        
        [Test]
        public void EvaluateConversionStringNumber()
        {
            Parser parser = new Parser(new Lexer("int \"2\";").GetTokenEnumerator().ToImmutableArray());

            Scope scope = new Scope(DefaultBuiltins.Core);

            Assert.AreEqual(new EvaluatorContext(parser.ParseNextExpression(), scope).Evaluate(), new Value.Number(2));
        }
        
        
        [Test]
        public void EvaluateConversionNumberString()
        {
            Parser parser = new Parser(new Lexer("string 2;").GetTokenEnumerator().ToImmutableArray());

            Scope scope = new Scope(DefaultBuiltins.Core);

            Assert.AreEqual(new EvaluatorContext(parser.ParseNextExpression(), scope).Evaluate(), new Value.String("2"));
        }
    }
}