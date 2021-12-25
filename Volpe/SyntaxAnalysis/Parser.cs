using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Volpe.Evaluation;
using Volpe.Exceptions;
using Volpe.LexicalAnalysis;

namespace Volpe.SyntaxAnalysis
{
    public class Parser
    {
        private Consumer<Token> _tokenConsumer;

        public Parser(IEnumerable<Token> tokens)
        {
            _tokenConsumer = new Consumer<Token>(tokens);
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

        private T ForceGetNextTokenValueWithType<T>() where T : TokenValue
        {
            Token token = ForceGetNextToken();

            if (token.Value is not T tokenValue)
                throw new UnexpectedTokenException(token!.PositionInText, token);

            return tokenValue;
        }

        private T GetAndAssertNextTokenType<T>() where T : TokenValue
        {
            TokenValue value = (T)ForceGetNextToken().Value;
            Debug.Assert(value is T);

            return (T)value;
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

        private ExpressionValue.PrefixExpression ParsePrefixExpression()
        {
            TokenValue value = ForceGetNextToken().Value;
            Debug.Assert(value.IsOperator);

            ExpressionOperator expressionOperator = ExpressionOperator.FromTokenValue(value);

            if ((expressionOperator.Type & ExpressionOperatorType.Prefix) == 0)
                throw new InvalidPrefixOperatorException(expressionOperator, GetLastConsumedTokenPositionOrZero());

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

        private Expression[] ParseExpressionBlock()
        {
            List<Expression> expressions = new List<Expression>();
            
            ForceGetNextTokenValueWithType<TokenValue.LeftCurlyBracket>();

            for (;;)
            {
                if (!_tokenConsumer.TryPeekNext(out _))
                    throw new UnexpectedEofException(GetLastConsumedTokenPositionOrZero());

                Expression expression = ForceParseNextExpression();
                    
                if (expression.Value is ExpressionValue.Void)
                {
                    _tokenConsumer.SkipOne();
                    return expressions.ToArray();
                }
                
                expressions.Add(expression);
            }
        }
        
        private ExpressionValue.FunctionDefinition ParseFunctionDefinition()
        {
            string[] ParseFormalParameters()
            {
                ForceGetNextTokenValueWithType<TokenValue.LeftRoundBracket>();
                
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

            GetAndAssertNextTokenType<TokenValue.FuncDef>();

            string functionName = ForceGetNextTokenValueWithType<TokenValue.Literal>().Value;

            string[] parametersName = ParseFormalParameters();
            Expression[] expressions = ParseExpressionBlock();

            return new ExpressionValue.FunctionDefinition(functionName, parametersName, expressions);
        }

        private ExpressionValue.WhileExpression ParseWhileExpression()
        {
            GetAndAssertNextTokenType<TokenValue.While>();
            ForceGetNextTokenValueWithType<TokenValue.LeftRoundBracket>();
            Expression condExpression = ForceParseNextExpression();
            ForceGetNextTokenValueWithType<TokenValue.RightRoundBracket>();

            Expression[] block = ParseExpressionBlock();

            return new ExpressionValue.WhileExpression(condExpression, block);
        }

        private ExpressionValue.IfExpression ParseIfExpression()    
        {
            List<Expression> conditions = new List<Expression>();
            List<Expression[]> blocks = new List<Expression[]>();
            Expression[]? elseBlock = null;

            // Parse first if
            GetAndAssertNextTokenType<TokenValue.If>();
            ForceGetNextTokenValueWithType<TokenValue.LeftRoundBracket>();
            conditions.Add(ForceParseNextExpression());
            ForceGetNextTokenValueWithType<TokenValue.RightRoundBracket>();
            blocks.Add(ParseExpressionBlock());

            for (;;)
            {
                Token? token;
                
                if (!_tokenConsumer.TryPeekNext(out token))
                    break;

                if (token!.Value is TokenValue.Elif)
                {
                    GetAndAssertNextTokenType<TokenValue.Elif>();
                    ForceGetNextTokenValueWithType<TokenValue.LeftRoundBracket>();
                    conditions.Add(ForceParseNextExpression());
                    ForceGetNextTokenValueWithType<TokenValue.RightRoundBracket>();
                    blocks.Add(ParseExpressionBlock());
                }
                else if (token.Value is TokenValue.Else)
                {
                    GetAndAssertNextTokenType<TokenValue.Else>();
                    elseBlock = ParseExpressionBlock();
                    
                    break;
                }
                else
                    break;
            }

            return new ExpressionValue.IfExpression(conditions.ToArray(), blocks.ToArray(), elseBlock);
        }

        private ExpressionValue.Array ParseArray()
        {
            GetAndAssertNextTokenType<TokenValue.LeftBracket>();

            List<Expression> initialElements = new List<Expression>();
            bool needComma = false;
            
            for (;;)
            {
                Token? token;

                if (!_tokenConsumer.TryPeekNext(out token))
                    throw new UnexpectedEofException(GetLastConsumedTokenPositionOrZero());
                
                switch (token!.Value)
                {
                    case TokenValue.RightBracket:
                        _tokenConsumer.SkipOne();
                        return new ExpressionValue.Array(initialElements.ToArray());
                        
                    case { } when !needComma:
                        initialElements.Add(ForceParseNextExpression());
                        needComma = true;
                        break;

                    case TokenValue.Comma when needComma:
                        needComma = false;
                        _tokenConsumer.SkipOne();
                        break;

                    default:
                        throw new UnexpectedTokenException(GetLastConsumedTokenPositionOrZero(), token);
                }
            }
        }

        private ExpressionValue.FunctionCall ParseFunctionCall(out bool canBeSubExpression)
        {
            Expression[] ParseActualParameterListWithoutBrackets()
            {
                List<Expression> actualParameters = new List<Expression>();
                bool needComma = false;

                for (;;)
                {
                    Token? token;

                    if (!_tokenConsumer.TryPeekNext(out token))
                        return actualParameters.ToArray();

                    switch (token!.Value)
                    {
                        case TokenValue.SemiColon 
                            or TokenValue.RightRoundBracket 
                            or TokenValue.RightCurlyBracket 
                            or TokenValue.RightBracket: 
                            return actualParameters.ToArray();
                        
                        case { } when !needComma:
                            actualParameters.Add(ForceParseNextExpression());
                            needComma = true;
                            break;

                        case TokenValue.Comma when needComma:
                            needComma = false;
                            _tokenConsumer.SkipOne();
                            break;

                        default:
                            throw new UnexpectedTokenException(GetLastConsumedTokenPositionOrZero(), token);
                    }
                }
            }

            Expression[] ParseActualParameterListWithBrackets()
            {
                GetAndAssertNextTokenType<TokenValue.LeftRoundBracket>();

                List<Expression> actualParameters = new List<Expression>();
                bool needComma = false;

                for (;;)
                {
                    Token? token;

                    if (!_tokenConsumer.TryPeekNext(out token))
                        throw new UnexpectedEofException(GetLastConsumedTokenPositionOrZero());

                    switch (token!.Value)
                    {
                        case TokenValue.RightRoundBracket:
                            _tokenConsumer.SkipOne();
                            return actualParameters.ToArray();
                        
                        case { } when !needComma:
                            actualParameters.Add(ForceParseNextExpression());
                            needComma = true;
                            break;

                        case TokenValue.Comma when needComma:
                            _tokenConsumer.SkipOne();
                            needComma = false;
                            break;

                        default:
                            throw new UnexpectedTokenException(GetLastConsumedTokenPositionOrZero(), token);
                    }
                }
            }

            canBeSubExpression = false;

            string functionName = GetAndAssertNextTokenType<TokenValue.Literal>().Value;

            Token? token;
            if (!_tokenConsumer.TryPeekNext(out token))
                return new ExpressionValue.FunctionCall(functionName, Array.Empty<Expression>());

            Expression[] actualParameters;

            if (token!.Value is TokenValue.LeftRoundBracket)
            {
                canBeSubExpression = true;
                actualParameters = ParseActualParameterListWithBrackets();
            }
            else
            {
                actualParameters = ParseActualParameterListWithoutBrackets();
            }

            return new ExpressionValue.FunctionCall(functionName, actualParameters);
        }

        // https://en.wikipedia.org/wiki/Operator-precedence_parser for reference
        public bool TryParseNextExpression(
            out Expression? expression,
            ExpressionOperatorPrecedence precedence = ExpressionOperatorPrecedence.Lowest)
        {
            expression = default;

            _tokenConsumer.SkipTill(t => t.Value is TokenValue.SemiColon);

            // Parse the first expression that comes in, if any
            Token? token;
            if (!_tokenConsumer.TryPeekNext(out token))
                return false;

            PositionInText currentPositionInText = token!.PositionInText;
            bool canBeSubExpression = false;

            switch (token.Value)
            {
                case TokenValue.RightCurlyBracket:
                    expression = new Expression { Value = new ExpressionValue.Void() };
                    break;
                case TokenValue.Literal:
                    expression = new Expression {Value = ParseFunctionCall(out canBeSubExpression)};
                    break;
                case TokenValue.FuncDef:
                    expression = new Expression {Value = ParseFunctionDefinition()};
                    break;
                case TokenValue.Return:
                    expression = new Expression {Value = ParseReturnExpression()};
                    break;
                case TokenValue.If:
                    expression = new Expression {Value = ParseIfExpression()};
                    break;
                case TokenValue.While:
                    expression = new Expression {Value = ParseWhileExpression()};
                    break;
                default:
                {
                    canBeSubExpression = true;

                    ExpressionValue value = token.Value switch
                    {
                        TokenValue.Dollar => ParseVariable(),
                        TokenValue.Hashtag => ParseFunctionReference(),
                        
                        {IsOperator: true} => ParsePrefixExpression(),
                        
                        TokenValue.String(var nValue) => 
                            _tokenConsumer.TryConsumeNextAndThen((_, _) => new ExpressionValue.String(nValue)),

                        TokenValue.Number(var nValue) =>
                            _tokenConsumer.TryConsumeNextAndThen((_, _) => new ExpressionValue.Number(nValue)),

                        TokenValue.True => 
                            _tokenConsumer.TryConsumeNextAndThen((_, _) => new ExpressionValue.True()),
                        
                        TokenValue.False => 
                            _tokenConsumer.TryConsumeNextAndThen((_, _) => new ExpressionValue.False()),
                        
                        TokenValue.LeftRoundBracket => ParseSubExpression(),

                        TokenValue.LeftBracket => ParseArray(),
                        
                        _ => throw new UnexpectedTokenException(currentPositionInText, token)
                    };

                    expression = new Expression {Value = value};
                    break;
                }
            }

            // Try to parse an infix expression if there is an operator after it.
            if (canBeSubExpression)
            {
                for (;;)
                {
                    if (!_tokenConsumer.TryPeekNext(out token))
                        break;

                    if (token!.Value.IsOperator)
                    {
                        ExpressionOperator expressionOperator = ExpressionOperator.FromTokenValue(token.Value);

                        if ((expressionOperator.Type & ExpressionOperatorType.Infix) == 0)
                            throw new InvalidInfixOperatorException(expressionOperator, GetLastConsumedTokenPositionOrZero());

                        if (precedence >= expressionOperator.Precedence)
                            break;

                        _tokenConsumer.SkipOne();

                        expression = new Expression
                        {
                            Value = new ExpressionValue.InfixExpression(
                                expressionOperator, expression,
                                ForceParseNextExpression(expressionOperator.Precedence)),
                            PositionInText = currentPositionInText
                        };
                    }
                    else if (token is
                    {
                        Value: TokenValue.Comma or TokenValue.RightCurlyBracket or TokenValue.RightRoundBracket or
                        TokenValue.Dollar or TokenValue.SemiColon or TokenValue.RightBracket
                    })
                        break;
                    else
                        throw new UnexpectedTokenException(GetLastConsumedTokenPositionOrZero(), token!);
                }
            }

            expression.PositionInText = currentPositionInText;

            return true;
        }

        private ExpressionValue.Return ParseReturnExpression()
        {
            GetAndAssertNextTokenType<TokenValue.Return>();

            return new ExpressionValue.Return(ForceParseNextExpression());
        }

        public IEnumerable<Expression> GetExpressionEnumerator()
        {
            Expression? expression;
            while (TryParseNextExpression(out expression))
                yield return expression!;
        }
    }
}