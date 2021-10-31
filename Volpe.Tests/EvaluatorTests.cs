using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using Volpe.Evaluation;
using Volpe.LexicalAnalysis;
using Volpe.SyntaxAnalysis;

namespace Volpe.Tests
{
    public class EvaluatorTests
    {
            
        [Test]
        public void EvaluateAddition()
        {
            Expression expression = new Parser(new Lexer("2 + 2 + 3 + 4").ToImmutableArray()).ParseNextExpression();

            Value value = new Evaluator().Evaluate(expression, new Scope());
            Assert.AreEqual(value, new Value.Number(11));
        }
        
           
        [Test]
        public void EvaluateVariableAssignment()
        {
            Expression expression = new Parser(new Lexer("$test = 2 + 2 + 3 + 4").ToImmutableArray()).ParseNextExpression();

            Scope scope = new Scope();
            
            Value value = new Evaluator().Evaluate(expression, scope);
            
            Assert.AreEqual(value, new Value.Number(11));
            Assert.IsTrue(scope.TryGetVariableValue("test", out value));
            Assert.AreEqual(value, new Value.Number(11));
        }
        
        
        [Test]
        public void EvaluateFourOperations()
        {
            Expression expression = new Parser(new Lexer("2 + 2 + 3 - 4 + 4 * 8 + 6 / 2").ToImmutableArray()).ParseNextExpression();

            Value value = new Evaluator().Evaluate(expression, new Scope());
            Assert.AreEqual(value, new Value.Number(38));
        }
        
        [Test]
        public void EvaluateSubExpression()
        {
            Expression expression = new Parser(new Lexer("(2+2)*(2+2)").ToImmutableArray()).ParseNextExpression();

            Value value = new Evaluator().Evaluate(expression, new Scope());
            Assert.AreEqual(value, new Value.Number(16));
        }
        
        [Test]
        public void EvaluateVariable()
        {
            Parser parser = new Parser(new Lexer("$test = 2 + 2 + 3 - 4 + 4 * 8 + 6 / 2; $test").ToImmutableArray());

            Scope scope = new Scope();
            Evaluator evaluator = new Evaluator();
            
            Assert.AreEqual(
                new Value[]
                {
                    evaluator.Evaluate(parser.ParseNextExpression(), scope), 
                    evaluator.Evaluate(parser.ParseNextExpression(), scope)
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
            Parser parser = new Parser(new Lexer("funcdef hi($testVariable) {}").ToImmutableArray());

            Scope scope = new Scope();
            Evaluator evaluator = new Evaluator();

            evaluator.Evaluate(parser.ParseNextExpression(), scope);

            Value.Function function;
            scope.TryGetFunctionReference("hi", out function);

            CollectionAssert.AreEqual(function!.ParameterNames, new string[] { "testVariable" });
            CollectionAssert.AreEqual(function.Expressions, new Expression[] { });
        }
        
        [Test]
        public void EvaluateFunctionCall()
        {
            Parser parser = new Parser(new Lexer("funcdef hi($testVariable) { $testVariable2 = 2; ret $testVariable2 }; hi(2) + 2").ToImmutableArray());

            Scope scope = new Scope();
            Evaluator evaluator = new Evaluator();

            evaluator.Evaluate(parser.ParseNextExpression(), scope);
            Assert.AreEqual(evaluator.Evaluate(parser.ParseNextExpression(), scope), new Value.Number(4));
        }
    }
}