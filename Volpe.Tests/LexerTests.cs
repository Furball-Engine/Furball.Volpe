using System.Collections.Immutable;
using System.Linq;
using Volpe.LexicalAnalysis;
using NUnit.Framework;
using Volpe.Exceptions;

namespace Volpe.Tests
{
    public static class LexerExtension
    {
        public static Token ConsumeNextToken(this Lexer lexer)
        {
            Token token;
            lexer.TryConsumeNextToken(out token);

            return token!;
        }
    }
    
    public class LexerTests
    {
        [Test]
        public void ParseString()
        {
            Lexer lexer = new Lexer("\"I want to kill myself\"");
            
            Assert.AreEqual(
                    lexer.ConsumeNextToken().Value, 
                    new TokenValue.String("I want to kill myself"));
        }
        
        [Test]
        public void ParseStringUnicode()
        {
            Lexer lexer = new Lexer("\"こんにちは\"");
            
            Assert.AreEqual(
                lexer.ConsumeNextToken().Value, 
                new TokenValue.String("こんにちは"));
        }
        
        [Test]
        public void ParseColumn()
        {
            Lexer lexer = new Lexer(":");
            
            Assert.AreEqual(
                lexer.ConsumeNextToken().Value, 
                new TokenValue.Column());
        }
        
        
        [Test]
        public void ParseIntegerNumber()
        {
            Lexer lexer = new Lexer("1234");
            
            Assert.AreEqual(
                lexer.ConsumeNextToken().Value, 
                new TokenValue.Number(1234));
        }
        
        [Test]
        public void ParseRationalNumber()
        {
            Lexer lexer = new Lexer("1234.1234");
            
            Assert.AreEqual(
                lexer.ConsumeNextToken().Value, 
                new TokenValue.Number(1234.1234));
        }
        
        [Test]
        public void ParseLiteral()
        {
            Lexer lexer = new Lexer("hi");
            
            Assert.AreEqual(
                lexer.ConsumeNextToken().Value, 
                new TokenValue.Literal("hi"));
        }
     
        [Test]
        public void ParseMultiple()
        {
            TokenValue[] tokens = new Lexer("helpme 1234 \"help me\" 𝕙𝕖𝕝𝕡𝕞𝕖").Select(t => t.Value).ToArray();
            
            Assert.AreEqual(
                tokens, 
                new TokenValue[]
                {
                    new TokenValue.Literal("helpme"), 
                    new TokenValue.Number(1234), 
                    new TokenValue.String("help me"),
                    new TokenValue.Literal("𝕙𝕖𝕝𝕡𝕞𝕖")
                });
        }
        
        [Test]
        public void CrashIfDoubleDotEncountered()
        {
            Lexer lexer = new Lexer("1234...23");
            
            UnexceptedSymbolException exception = Assert.Throws<UnexceptedSymbolException>(
                () => lexer.ConsumeNextToken());
                
            Assert.AreEqual(exception.Symbol, '.');
        }
        
        
        [Test]
        public void CheckPositionInText()
        {
            Lexer lexer = new Lexer("hi my name is peppy\nAnd i'm very happy too");

            int[] indices = { 0, 3, 6, 11, 14 };

            foreach (var index in indices)
                Assert.AreEqual(lexer.ConsumeNextToken().PositionInText, new PositionInText {Column = 0, Row = index});
            
            Assert.AreEqual(lexer.ConsumeNextToken().PositionInText, new PositionInText {Column = 1, Row = 0});
        }
        
        [Test]
        public void ParseAssignmentExampleTokens()
        {
            TokenValue[] tokens = new Lexer("$test = 1").Select(t => t.Value).ToArray();

            Assert.AreEqual(tokens, new TokenValue[]
            {
                new TokenValue.Dollar(),
                new TokenValue.Literal("test"),
                new TokenValue.Assign(),
                new TokenValue.Number(1),
            });
        }
        
        [Test]
        public void ParseNumbersNoSpace()
        {
            TokenValue[] tokens = new Lexer("2+1+3+4").Select(t => t.Value).ToArray();

            Assert.AreEqual(tokens, new TokenValue[]
            {
                new TokenValue.Number(2),
                new TokenValue.Operator(new TokenValueOperator.Add()),
                new TokenValue.Number(1),
                new TokenValue.Operator(new TokenValueOperator.Add()),
                new TokenValue.Number(3),
                new TokenValue.Operator(new TokenValueOperator.Add()),
                new TokenValue.Number(4),
            });
        }
    }
}