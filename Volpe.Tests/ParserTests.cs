using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using Volpe.LexicalAnalysis;
using Volpe.SyntaxAnalysis;

namespace Volpe.Tests
{
    public static class ParserExtension
    {
        public static Expression ParseNextExpression(this Parser parser)
        {
            Expression expression;
            parser.TryParseNextExpression(out expression);

            return expression;
        }
    }
    
    public class ParserTests
    {
        [Test]
        public void ParseVariable()
        {
            Parser parser = new Parser(new Lexer("$test").ToImmutableArray());

            Assert.AreEqual(parser.ParseNextExpression().Value, new ExpressionValue.Variable("test"));
        }
        
        [Test]
        public void ParseAssignment()
        {
            Expression[] values = new Parser(new Lexer("$test = 2 * 4").ToImmutableArray())
                .ToArray();

            Assert.AreEqual(values, new Expression[] {
                new Expression {
                    Value = new ExpressionValue.Assignment(
                        "test",
                        new Expression { Value = new ExpressionValue.InfixExpression(
                            new ExpressionOperator.Mul(),
                            new Expression { Value = new ExpressionValue.Number(2) },
                            new Expression { Value = new ExpressionValue.Number(4) })
                        })
                }
            });
        }
        
        [Test]
        public void ParseAdd()
        {
            Expression[] values = new Parser(new Lexer("2 + 2").ToImmutableArray())
                .ToArray();

            Assert.AreEqual(values, new Expression[] {
                new Expression {
                    Value = new ExpressionValue.InfixExpression(
                        new ExpressionOperator.Add(),
                        new Expression { Value = new ExpressionValue.Number(2) },
                        new Expression { Value = new ExpressionValue.Number(2) })
                }
            });
        }
        
        [Test]
        public void ParseAddSub()
        {
            Expression[] values = new Parser(new Lexer("2 + 2 - 4").ToImmutableArray())
                .ToArray();

            Assert.AreEqual(values, new Expression[]
            {
                new Expression
                {
                    Value = new ExpressionValue.InfixExpression(
                        new ExpressionOperator.Sub(),
                        new Expression { 
                            Value = new ExpressionValue.InfixExpression(
                                new ExpressionOperator.Add(), 
                                new Expression
                                {
                                    Value = new ExpressionValue.Number(2)
                                },
                                new Expression
                                {
                                    Value = new ExpressionValue.Number(2)
                                })
                        },
                        new Expression
                        {
                            Value = new ExpressionValue.Number(4)
                        })
                }
            });
        }
        
        
        [Test]
        public void ParseSubExpression()
        {
            Expression[] values = new Parser(new Lexer("(2+2)*(2+2)").ToImmutableArray())
                .ToArray();

            Assert.AreEqual(values, new Expression[]
            {
                new Expression
                {
                    Value = new ExpressionValue.InfixExpression(
                        new ExpressionOperator.Mul(),
                        new Expression
                        {
                            Value = new ExpressionValue.SubExpression(new Expression { 
                                Value = new ExpressionValue.InfixExpression(
                                    new ExpressionOperator.Add(), 
                                    new Expression
                                    {
                                        Value = new ExpressionValue.Number(2)
                                    },
                                    new Expression
                                    {
                                        Value = new ExpressionValue.Number(2)
                                    })
                            })
                        },
                        new Expression
                        {
                            Value = new ExpressionValue.SubExpression(new Expression { 
                                Value = new ExpressionValue.InfixExpression(
                                    new ExpressionOperator.Add(), 
                                    new Expression
                                    {
                                        Value = new ExpressionValue.Number(2)
                                    },
                                    new Expression
                                    {
                                        Value = new ExpressionValue.Number(2)
                                    })
                            })
                        })
                }
            });
        }

        [Test]
        public void ParseFunctionDefinition()
        {
            Expression expr =new Parser(new Lexer("funcdef hi($testVariable, $testVariable2) {}").ToImmutableArray())
                .ParseNextExpression();

            ExpressionValue.FunctionDefinition functionDefinition = (ExpressionValue.FunctionDefinition) expr.Value;
            
            Assert.AreEqual(functionDefinition.Name, "hi");
            CollectionAssert.AreEqual(functionDefinition.ParameterNames, new string[] { "testVariable", "testVariable2"  });
            CollectionAssert.AreEqual(functionDefinition.Expressions, new Expression[] {});
        }
    }
}