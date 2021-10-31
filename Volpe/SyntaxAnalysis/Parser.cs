using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Volpe.Exceptions;
using Volpe.LexicalAnalysis;

namespace Volpe.SyntaxAnalysis
{
    public class Parser : IEnumerable<Expression>
    {
        private ArrayConsumer<Token> _tokenConsumer;

        public Parser(ImmutableArray<Token> tokens)
        {
            _tokenConsumer = new ArrayConsumer<Token>(tokens);
        }

        private PositionInText GetLastConsumedTokenPositionOrZero() => _tokenConsumer.LastConsumed?.PositionInText
                                                                  ?? PositionInText.Zero;
        
        private Token ForceGetNextToken()
        {
            Token? token;
            
            if (!_tokenConsumer.TryConsumeNext(out token))
                throw new UnexpectedEofException(GetLastConsumedTokenPositionOrZero());

            return token!;
        }
        
        private T ForceGetNextTokenValueWithType<T>() where T: TokenValue
        {
            Token token = ForceGetNextToken();

            if (token.Value is not T tokenValue)
                throw new UnexpectedTokenException(token!.PositionInText, token);
            
            return tokenValue;
        }

        private T GetAndAssertNextTokenType<T>() where T: TokenValue
        {
            if (ForceGetNextToken().Value is not T value)
                throw new InvalidOperationException();

            return value;
        }
        
        private Expression ForceParseNextExpression(
            ExpressionOperatorPrecedence precedence = ExpressionOperatorPrecedence.Lowest)
        {
            Expression? expression;
            
            if (!TryParseNextExpression(out expression, precedence))
                throw new ExpectedExpressionException(GetLastConsumedTokenPositionOrZero());

            return expression!;
        }

        private ExpressionValue.Variable ParseVariable()
        {
            GetAndAssertNextTokenType<TokenValue.Dollar>();

            return new ExpressionValue.Variable(ForceGetNextTokenValueWithType<TokenValue.Literal>().Value);
        }
        
        private ExpressionValue.FunctionReference ParseFunctionReference()
        {
            GetAndAssertNextTokenType<TokenValue.Hashtag>();

            return new ExpressionValue.FunctionReference(ForceGetNextTokenValueWithType<TokenValue.Literal>().Value);
        }

        private ExpressionValue.Assignment ParseAssign(Expression expression)
        {
            GetAndAssertNextTokenType<TokenValue.Assign>();

            if (expression.Value is not ExpressionValue.Variable { Name: var variableName })
                throw new ExpectedVariableException(expression.PositionInText);
            
            return new ExpressionValue.Assignment(variableName, ForceParseNextExpression());
        }

        private ExpressionValue.PrefixExpression ParsePrefixExpression()
        {
            TokenValue.Operator tokenValueOperator = GetAndAssertNextTokenType<TokenValue.Operator>();

            ExpressionOperator expressionOperator = ExpressionOperator.FromTokenOperator(tokenValueOperator.Value);
            
            if ((expressionOperator.Type & ExpressionOperatorType.Prefix) == 0)
                throw new InvalidInfixOperatorException(GetLastConsumedTokenPositionOrZero());

            Expression leftExpression = ForceParseNextExpression();

            return new ExpressionValue.PrefixExpression(expressionOperator, leftExpression);
        }

        public ExpressionValue.SubExpression ParseSubExpression()
        {
            GetAndAssertNextTokenType<TokenValue.LeftRoundBracket>();
            
            Expression expression = ForceParseNextExpression();
            
            ForceGetNextTokenValueWithType<TokenValue.RightRoundBracket>();

            return new ExpressionValue.SubExpression(expression);
        }

        private ExpressionValue.FunctionDefinition ParseFunctionDefinition()
        {
            string[] ParseVariableNames()
            {
                List<string> variableNames = new List<string>();
                bool needComma = false;
                
                for (;;)
                {
                    Token token = ForceGetNextToken();

                    switch (token.Value)
                    {
                        case TokenValue.Dollar when !needComma:
                            TokenValue.Literal identifier = ForceGetNextTokenValueWithType<TokenValue.Literal>();
                            variableNames.Add(identifier.Value);
                            needComma = true;
                            break;
                    
                        case TokenValue.Comma when needComma:
                            needComma = false;
                            break;
                        
                        case TokenValue.RightRoundBracket:
                            return variableNames.ToArray();

                        default:
                            throw new UnexpectedTokenException(GetLastConsumedTokenPositionOrZero(), token);
                    }
                }
            }
            
            Expression[] ParseExpressionBlock()
            {
                List<Expression> expressions = new List<Expression>();
                
                for (;;)
                {
                    Token token = ForceGetNextToken();

                    switch (token.Value)
                    {
                        case TokenValue.RightCurlyBracket:
                            return expressions.ToArray();
                    
                        default:
                            expressions.Add(ForceParseNextExpression());
                            break;
                    }
                }
            }

            GetAndAssertNextTokenType<TokenValue.FuncDef>();
            
            string functionName = ForceGetNextTokenValueWithType<TokenValue.Literal>().Value;
            
            ForceGetNextTokenValueWithType<TokenValue.LeftRoundBracket>();
            string[] variableNames = ParseVariableNames();

            ForceGetNextTokenValueWithType<TokenValue.LeftCurlyBracket>();

            Expression[] expressions = ParseExpressionBlock();

            return new ExpressionValue.FunctionDefinition(functionName, variableNames, expressions);
        }

        /*private ExpressionValue.FunctionCall ParseFunctionCall()
        {
            Expression[] ParseActualParameterListWithBrackets()
            {
                List<Expression> actualParameters = new List<Expression>();
                
                for (;;)
                {
                    Token token = ForceGetNextToken();

                    switch (token.Value)
                    {
                        case TokenValue.RightRoundBracket:
                            return actualParameters.ToArray();
                            
                        default:
                            actualParameters.Add(ForceParseNextExpression());
                            break;
                    }
                }
            }
            
            Expression[] ParseActualParameterListWithoutBrackets()
            {
                List<Expression> actualParameters = new List<Expression>();
                
                for (;;)
                {
                    Token token = ForceGetNextToken();

                    switch (token.Value)
                    {
                        case TokenValue.SemiColon:
                            return actualParameters.ToArray();
                            
                        default:
                            actualParameters.Add(ForceParseNextExpression());
                            break;
                    }
                }
            }

            GetAndAssertNextTokenType<TokenValue.FuncDef>();
            
            string functionName = ForceGetNextTokenValueWithType<TokenValue.Literal>().Value;
            
            ForceGetNextTokenValueWithType<TokenValue.LeftRoundBracket>();
            string[] variableNames = ParseVariableNames();

            ForceGetNextTokenValueWithType<TokenValue.LeftCurlyBracket>();

            Expression[] expressions = ParseExpressionBlock();

            return new ExpressionValue.FunctionDefinition(functionName, variableNames, expressions);
        }*/
        
        // https://en.wikipedia.org/wiki/Operator-precedence_parser for reference
        public bool TryParseNextExpression(
            out Expression? expression,
            ExpressionOperatorPrecedence precedence = ExpressionOperatorPrecedence.Lowest)
        {
            expression = default;
            
            // Parse the first expression that comes in, if any
            Token? token;
            if (!_tokenConsumer.TryPeekNext(out token))
                return false;

            PositionInText currentPositionInText = token!.PositionInText;

            ExpressionValue value = token.Value switch
            {
                TokenValue.Dollar => ParseVariable(),
                TokenValue.Hashtag => ParseFunctionReference(),
                TokenValue.Operator => ParsePrefixExpression(),
                TokenValue.FuncDef => ParseFunctionDefinition(),

                TokenValue.Number(var nValue) => 
                    _tokenConsumer.TryConsumeNextAndThen((_, _) => new ExpressionValue.Number(nValue)),
                
                TokenValue.LeftRoundBracket => ParseSubExpression(),

                _ => throw new UnexpectedTokenException(currentPositionInText, token)
            };

            expression = new Expression { Value = value };
            
            // Try to parse an infix expression if there is an operator after it.
            for (;;)
            {
                if (!_tokenConsumer.TryPeekNext(out token))
                    break;
                
                if (token is { Value: TokenValue.Operator { Value: var tokenOperator } })
                {
                    ExpressionOperator expressionOperator = ExpressionOperator.FromTokenOperator(tokenOperator);

                    if ((expressionOperator.Type & ExpressionOperatorType.Infix) == 0)
                        throw new InvalidInfixOperatorException(GetLastConsumedTokenPositionOrZero());

                    if (precedence >= expressionOperator.Precedence)
                        break;

                    _tokenConsumer.SkipOne();
                    
                    expression = new Expression
                    {
                        Value = new ExpressionValue.InfixExpression(
                            expressionOperator, expression, ForceParseNextExpression(expressionOperator.Precedence)),
                        PositionInText = currentPositionInText
                    };
                }
                else if (token is {Value: TokenValue.Assign})
                    expression.Value = ParseAssign(expression);
                else if (token is {Value: TokenValue.SemiColon})
                {
                    _tokenConsumer.SkipOne();
                    break;
                }
                else if (token is {Value: TokenValue.Comma or TokenValue.RightRoundBracket or TokenValue.Dollar})
                    break;
                else
                    throw new UnexpectedTokenException(GetLastConsumedTokenPositionOrZero(), token!);
            }

            expression.PositionInText = currentPositionInText;

            return true;
        }

        public IEnumerator<Expression> GetEnumerator()
        {
            Expression? expression;
            while (TryParseNextExpression(out expression))
                yield return expression!;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}