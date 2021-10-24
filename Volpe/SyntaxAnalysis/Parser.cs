using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Volpe.LexicalAnalysis;
using Volpe.SyntaxAnalysis.Exceptions;

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
        
        private Token ForceGetExpression()
        {
            Token? token;
            
            if (!_tokenConsumer.TryConsumeNext(out token))
                throw new ParserUnexpectedEofException(GetLastConsumedTokenPositionOrZero());

            return token!;
        }

        private Token ForceGetNextToken()
        {
            Token? token;
            
            if (!_tokenConsumer.TryConsumeNext(out token))
                throw new ParserUnexpectedEofException(GetLastConsumedTokenPositionOrZero());

            return token!;
        }
        
        private T ForceGetNextTokenValueWithType<T>() where T: TokenValue
        {
            Token token = ForceGetNextToken();

            if (token.Value is not T tokenValue)
                throw new ParserUnexpectedTokenException(token!.PositionInText, token);
            
            return tokenValue;
        }
        
        private Expression ForceParseNextExpression(
            ExpressionOperatorPrecedence precedence = ExpressionOperatorPrecedence.Lowest)
        {
            Expression? expression;
            
            if (!TryParseNextExpression(out expression, precedence))
                throw new ParserExpectedExpressionException(GetLastConsumedTokenPositionOrZero());

            return expression!;
        }

        private ExpressionValue.Variable ParseVariable()
        {
            // If it's not a Dollar sign, the function is being called from the wrong place
            if (ForceGetNextToken().Value is not TokenValue.Dollar)
                throw new InvalidOperationException();

            return new ExpressionValue.Variable(ForceGetNextTokenValueWithType<TokenValue.Literal>().Value);
        }
        
        public bool TryParseNextExpression(
            out Expression? expression,
            ExpressionOperatorPrecedence precedence = ExpressionOperatorPrecedence.Lowest)
        {
            expression = default;
            
            Token? token;
            if (!_tokenConsumer.TryPeekNext(out token))
                return false;

            PositionInText currentPositionInText = token!.PositionInText;

            // Left expression
            expression = new Expression
            {
                Value = token.Value switch
                {
                    TokenValue.Dollar => ParseVariable(),
                    TokenValue.Number(var value) => _tokenConsumer.TryConsumeNextAndThen((_, _) => new ExpressionValue.Number(value))
                }
            };
            
            // Right expression 
            for (;;)
            {
                if (_tokenConsumer.TryPeekNext(out token) && 
                    token is { Value: TokenValue.Operator {Value: var tokenOperator} })
                {
                    ExpressionOperator expressionOperator = ExpressionOperator.FromTokenOperator(tokenOperator);

                    if ((expressionOperator.Type & ExpressionOperatorType.Infix) == 0)
                        throw new ParserInvalidInfixOperatorException(GetLastConsumedTokenPositionOrZero());

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