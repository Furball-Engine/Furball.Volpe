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
    }
}