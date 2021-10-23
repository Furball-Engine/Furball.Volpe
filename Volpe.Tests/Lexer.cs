using System.Linq;
using Volpe.LexicalAnalysis;
using NUnit.Framework;

namespace Volpe.Tests
{
    public class Tests
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
                .Select(token => token!.Value.Value).ToArray();
            
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
    }
}