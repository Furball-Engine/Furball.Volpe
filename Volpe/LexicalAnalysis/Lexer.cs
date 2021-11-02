﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Volpe.Exceptions;

namespace Volpe.LexicalAnalysis
{
    public class Lexer
    {
        private readonly TextConsumer _textConsumer;

        public Lexer(string source)
        {
            _textConsumer = new TextConsumer(source.ToImmutableArray());
        }

        private string ConsumeNextString()
        {
            char character;
            StringBuilder stringBuilder = new StringBuilder();
            
            if (!_textConsumer.TryConsumeNext(out character) || character != '"')
                throw new InvalidOperationException();

            for(;;)
            {
                if (!_textConsumer.TryConsumeNext(out character))
                    throw new UnexpectedEofException(_textConsumer.PositionInText);

                switch (character)
                {
                    case '"':
                        return stringBuilder.ToString();
                    
                    default:
                        stringBuilder.Append(character);
                        break;
                }
            }
        }

        private string ConsumeNextLiteral()
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            for(;;)
            {
                if (!_textConsumer.TryPeekNext(out char character))
                    break;
                
                if (character is ((>= '¡' or (>= 'a' and <= 'z') or (>= '@' and <= 'Z') or (>= '0' and <= '9')) or '_' and not '$'))
                {
                    stringBuilder.Append(character);
                    _textConsumer.SkipOne();
                } 
                else 
                    break;
            }

            return stringBuilder.ToString();
        }

        private double ConsumeNextNumber()
        {
            double num = 0;
            int exp = 0;
            bool isRational = false;
            
            for(;;)
            {
                if (!_textConsumer.TryPeekNext(out char character))
                    break;

                if (character is ( >= '0' and <= '9'))
                {
                    num = num * 10 + (character - '0');

                    if (isRational)
                        exp--;
                    
                    _textConsumer.TryConsumeNext(out _);
                } 
                else if (character == '.')
                {
                    if (isRational)
                        throw new UnexceptedSymbolException('.', _textConsumer.PositionInText);
                        
                    isRational = true;

                    _textConsumer.TryConsumeNext(out _);
                } 
                else
                    break;
            }

            return Math.Pow(10, exp) * num;
        }

        private void SkipWhiteSpaces()
        {
            while (_textConsumer.TryPeekNext(out char character) && char.IsWhiteSpace(character))
                _textConsumer.TryConsumeNext(out _);
        }

        public bool TryConsumeNextToken(out Token? token)
        {
            token = null;
            
            SkipWhiteSpaces();
            PositionInText currentPositionInText = _textConsumer.PositionInText;

            char character;
            if (!_textConsumer.TryPeekNext(out character))
                return false;

            TokenValue tokenValue = character switch
            {
                '"' => new TokenValue.String(ConsumeNextString()),
                
                ',' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Comma()),
                '#' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Hashtag()),
                '[' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.LeftBracket()),
                ']' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.RightBracket()),
                '(' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.LeftRoundBracket()),
                ')' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.RightRoundBracket()),
                '{' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.LeftCurlyBracket()),
                '}' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.RightCurlyBracket()),
                '$' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Dollar()),
                '=' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Assign()),
                '+' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Operator(new TokenValueOperator.Add())),
                '-' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Operator(new TokenValueOperator.Sub())),
                '/' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Operator(new TokenValueOperator.Div())),
                '*' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.Operator(new TokenValueOperator.Mul())),
                ';' => _textConsumer.TryConsumeNextAndThen((_, _) => new TokenValue.SemiColon()),

                >= '0' and <= '9' => new TokenValue.Number(ConsumeNextNumber()),

                ((>= '¡' or (>= 'a' and <= 'z') or (>= '@' and <= 'Z') or (>= '0' and <= '9')) or '_' and not '$') => ConsumeNextLiteral() switch 
                {
                    "true" => new TokenValue.True(),
                    "false" => new TokenValue.False(),
                    "funcdef" => new TokenValue.FuncDef(),
                    "ret" => new TokenValue.Return(),
                    
                    {} value => new TokenValue.Literal(value) 
                },

                var c => throw new UnexceptedSymbolException(c, currentPositionInText)
            };

            token = new Token {Value = tokenValue, PositionInText = currentPositionInText};

            return true;
        }

        public IEnumerable<Token> GetTokenEnumerator()
        {
            Token? token;
            while (TryConsumeNextToken(out token))
                yield return token!;
        }
    }
}
