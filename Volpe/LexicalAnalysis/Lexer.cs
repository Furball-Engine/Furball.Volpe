using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Volpe.LexicalAnalysis.Exceptions;

namespace Volpe.LexicalAnalysis
{
    public class Lexer: IEnumerable<WithPositionInText<Token>>
    {
        private TextConsumer _textConsumer;

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
                    throw new LexerUnexpectedEofException(_textConsumer.PositionInText);

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
                
                if (character is ((>= '¡' or (>= 'a' and <= 'z') or (>= '@' and <= 'Z') or (>= '0' and <= '9')) and not '$'))
                {
                    stringBuilder.Append(character);
                    _textConsumer.TryConsumeNext(out _); 
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
                if (!_textConsumer.TryConsumeNext(out char character))
                    break;

                if (character is ( >= '0' and <= '9'))
                {
                    num = num * 10 + (character - '0');

                    if (isRational)
                        exp--;
                } 
                else if (character == '.')
                {
                    if (isRational)
                        throw new LexerUnexceptedSymbolException('.', _textConsumer.PositionInText);
                        
                    isRational = true;
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

        public bool TryConsumeNextToken(out WithPositionInText<Token> tokenWithPositionInText)
        {
            tokenWithPositionInText = default;
            
            SkipWhiteSpaces();
            PositionInText currentPositionInText = _textConsumer.PositionInText;

            char character;
            if (!_textConsumer.TryPeekNext(out character))
                return false;

            Token token = character switch
            {
                '"' => new Token.String(ConsumeNextString()),
                
                ':' => _textConsumer.TryConsumeNextAndThen((_, _) => new Token.Column()),
                '[' => _textConsumer.TryConsumeNextAndThen((_, _) => new Token.LeftBracket()),
                ']' => _textConsumer.TryConsumeNextAndThen((_, _) => new Token.RightBracket()),
                '$' => _textConsumer.TryConsumeNextAndThen((_, _) => new Token.Dollar()),
                '=' => _textConsumer.TryConsumeNextAndThen((_, _) => new Token.Operator(new TokenOperator.Assign())),
                ';' => _textConsumer.TryConsumeNextAndThen((_, _) => new Token.SemiColon()),

                >= '0' and <= '9' => new Token.Number(ConsumeNextNumber()),

                _ => ConsumeNextLiteral() switch 
                {
                    "true" => new Token.True(),
                    "false" => new Token.False(),
                    
                    {} value => new Token.Literal(value) 
                }
            };

            tokenWithPositionInText = new WithPositionInText<Token>
            {
                Position = currentPositionInText,
                Value = token
            };

            return true;
        }

        public IEnumerator<WithPositionInText<Token>> GetEnumerator()
        {
            WithPositionInText<Token> token;
            while (TryConsumeNextToken(out token))
                yield return token;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        // <variable_name> <value> 
        // <value> : <number> | <"string">
        // :<function_name> <value...>
        // $<variable_name>
        // #()
    }
}
