using System.Collections.Immutable;
using System.Linq;
using Volpe.LexicalAnalysis;
using NUnit.Framework;
using Volpe.LexicalAnalysis.Exceptions;

namespace Volpe.Tests
{
    public static class LexerExtension
    {
        public static WithPositionInText<Token>? ConsumeNextToken(this Lexer lexer)
        {
            WithPositionInText<Token> token;
            lexer.TryConsumeNextToken(out token);

            return token;
        }
    }
    
    public class LexerTests
    {
        [Test]
        public void ParseString()
        {
            Lexer lexer = new Lexer("\"I want to kill myself\"");
            
            Assert.AreEqual(
                    lexer.ConsumeNextToken()!.Value.Value, 
                    new Token.String("I want to kill myself"));
        }
        
        [Test]
        public void ParseStringUnicode()
        {
            Lexer lexer = new Lexer("\"こんにちは\"");
            
            Assert.AreEqual(
                lexer.ConsumeNextToken()!.Value.Value, 
                new Token.String("こんにちは"));
        }
        
        [Test]
        public void ParseColumn()
        {
            Lexer lexer = new Lexer(":");
            
            Assert.AreEqual(
                lexer.ConsumeNextToken()!.Value.Value, 
                new Token.Column());
        }
        
        
        [Test]
        public void ParseIntegerNumber()
        {
            Lexer lexer = new Lexer("1234");
            
            Assert.AreEqual(
                lexer.ConsumeNextToken()!.Value.Value, 
                new Token.Number(1234));
        }
        
        [Test]
        public void ParseRationalNumber()
        {
            Lexer lexer = new Lexer("1234.1234");
            
            Assert.AreEqual(
                lexer.ConsumeNextToken()!.Value.Value, 
                new Token.Number(1234.1234));
        }
        
        [Test]
        public void ParseLiteral()
        {
            Lexer lexer = new Lexer("hi");
            
            Assert.AreEqual(
                lexer.ConsumeNextToken()!.Value.Value, 
                new Token.Literal("hi"));
        }
     
        [Test]
        public void ParseMultiple()
        {
            Token[] tokens = new Lexer("helpme 1234 \"help me\" 𝕙𝕖𝕝𝕡𝕞𝕖")
                .Select(token => token!.Value).ToArray();
            
            Assert.AreEqual(
                tokens, 
                new Token[]
                {
                    new Token.Literal("helpme"), 
                    new Token.Number(1234), 
                    new Token.String("help me"),
                    new Token.Literal("𝕙𝕖𝕝𝕡𝕞𝕖")
                });
        }
        
        [Test]
        public void CrashIfDoubleDotEncountered()
        {
            Lexer lexer = new Lexer("1234...23");
            
            LexerUnexceptedSymbolException exception = Assert.Throws<LexerUnexceptedSymbolException>(
                () => lexer.ConsumeNextToken());
                
            Assert.AreEqual(exception.Symbol, '.');
        }
        
        
        [Test]
        public void CheckPositionInText()
        {
            Lexer lexer = new Lexer("hi my name is peppy\nAnd i'm very happy too");

            int[] indices = { 0, 3, 6, 11, 14 };

            foreach (var index in indices)
                Assert.AreEqual(lexer.ConsumeNextToken()!.Value.Position, new PositionInText {Column = 0, Row = index});
            
            Assert.AreEqual(lexer.ConsumeNextToken()!.Value.Position, new PositionInText {Column = 1, Row = 0});
        }
        
        [Test]
        public void ParseAssignmentExampleTokens()
        {
            Token[] tokens = new Lexer("$test = 1").Select(v => v!.Value).ToArray();

            Assert.AreEqual(tokens, new Token[]
            {
                new Token.Dollar(),
                new Token.Literal("test"),
                new Token.Operator(new TokenOperator.Assign()),
                new Token.Number(1),
            });
        }
    }
}