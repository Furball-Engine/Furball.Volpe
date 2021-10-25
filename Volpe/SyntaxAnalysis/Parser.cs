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
            // If it's not a Dollar sign, the function is being called from the wrong place
            if (ForceGetNextToken().Value is not TokenValue.Dollar)
                throw new InvalidOperationException();

            return new ExpressionValue.Variable(ForceGetNextTokenValueWithType<TokenValue.Literal>().Value);
        }

        private ExpressionValue.Assignment ParseAssign(Expression expression)
        {
            if (ForceGetNextToken().Value is not TokenValue.Assign)
                throw new InvalidOperationException();

            if (expression.Value is not ExpressionValue.Variable { Name: var variableName })
                throw new ExpectedVariableException(expression.PositionInText);
            
            return new ExpressionValue.Assignment(variableName, ForceParseNextExpression());
        }

        private ExpressionValue.PrefixExpression ParsePrefixExpression()
        {
            if (ForceGetNextToken().Value is not TokenValue.Operator {Value: var tokenOperator })
                throw new InvalidOperationException();

            ExpressionOperator expressionOperator = ExpressionOperator.FromTokenOperator(tokenOperator);
            
            if ((expressionOperator.Type & ExpressionOperatorType.Prefix) == 0)
                throw new InvalidInfixOperatorException(GetLastConsumedTokenPositionOrZero());

            Expression leftExpression = ForceParseNextExpression();

            return new ExpressionValue.PrefixExpression(expressionOperator, leftExpression);
        }
        
        // https://en.wikipedia.org/wiki/Operator-precedence_parser for reference
        public bool TryParseNextExpression(
            out Expression? expression,
            ExpressionOperatorPrecedence precedence = ExpressionOperatorPrecedence.Lowest)
        {
            expression = default;
            
            Token? token;
            if (!_tokenConsumer.TryPeekNext(out token))
                return false;

            PositionInText currentPositionInText = token!.PositionInText;

            // Parse the first expression that comes in
            expression = new Expression
            {
                Value = token.Value switch
                {
                    TokenValue.Dollar => ParseVariable(),
                    TokenValue.Operator(_) => ParsePrefixExpression(),
                    
                    TokenValue.Number(var value) => 
                        _tokenConsumer.TryConsumeNextAndThen((_, _) => new ExpressionValue.Number(value)),
                    
                }   
            };
            
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

                    _tokenConsumer.TryConsumeNext(out _);
                    
                    expression = new Expression
                    {
                        Value = new ExpressionValue.InfixExpression(
                            expressionOperator, expression, ForceParseNextExpression(expressionOperator.Precedence)),
                        PositionInText = currentPositionInText
                    };
                }
                else if (token is {Value: TokenValue.Assign})
                    expression.Value = ParseAssign(expression);
                else
                    break;
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